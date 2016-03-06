using Kazyx.ImageStream;
using Kazyx.RemoteApi.Camera;
using Locana.CameraControl;
using Locana.DataModel;
using Locana.Utility;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Naotaco.Histogram.Win2d;
using System;
using System.ComponentModel;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Locana.Controls
{
    public sealed partial class ImageStreamView : UserControl
    {
        public ImageStreamView()
        {
            this.InitializeComponent();
            InitializeTimer();
            FocusMarkDrawer.OnTouchFocusOperated += (obj, args) => { Context?.Target?.Api?.Camera?.SetAFPositionAsync(args.X, args.Y).IgnoreExceptions(); };
        }


        private bool IsDecoding = false;

        private ReaderWriterLockSlim rwLock = new ReaderWriterLockSlim();

        private CanvasBitmap LiveviewImageBitmap;

        private JpegPacket PendingPakcet;

        private BitmapSize OriginalLvSize;
        private double LvOffsetV, LvOffsetH;

        private const double DEFAULT_DPI = 96.0;
        public DispatcherTimer FpsTimer { get; private set; } = new DispatcherTimer();

        private const int FPS_INTERVAL = 5000;
        private int LiveviewFrameCount = 0;

        private void InitializeTimer()
        {
            FpsTimer.Interval = TimeSpan.FromMilliseconds(FPS_INTERVAL);
            FpsTimer.Tick += (sender, arg) =>
            {
                var fps = (double)LiveviewFrameCount * 1000 / FPS_INTERVAL;
                DebugUtil.Log(() => string.Format("[LV CanvasBitmap] {0} fps", fps));
                LiveviewFrameCount = 0;
            };
        }

        public object FramingGuideDataContext
        {
            set { FramingGuideSurface.DataContext = value; }
        }

        public async Task<bool> SetupFocusFrame(bool RequestFocusFrameEnabled)
        {
            if (Context?.Target == null)
            {
                DebugUtil.Log("No target to set up focus frame is available.");
                FocusMarkDrawer.ClearFrames();
                return false;
            }

            if (Context.Target.Api.Capability.IsAvailable("setLiveviewFrameInfo"))
            {
                await Context.Target.Api.Camera?.SetLiveviewFrameInfoAsync(new FrameInfoSetting() { TransferFrameInfo = RequestFocusFrameEnabled });
            }

            if (RequestFocusFrameEnabled && !Context.Target.Api.Capability.IsSupported("setLiveviewFrameInfo") && Context.Target.Api.Capability.IsAvailable("setTouchAFPosition"))
            {
                // For devices which does not support to transfer focus frame info, draw focus frame itself.
                FocusMarkDrawer.SelfDrawTouchAFFrame = true;
            }
            else { FocusMarkDrawer.SelfDrawTouchAFFrame = false; }

            FocusMarkDrawer.ClearFrames();
            return true;
        }

        private LiveviewContext _Context;
        public LiveviewContext Context
        {
            set
            {
                if (_Context != null)
                {
                    _Context.PropertyChanged -= Context_PropertyChanged;
                }
                _Context = value;
                _Context.PropertyChanged += Context_PropertyChanged;
            }
            get { return _Context; }
        }

        private void Context_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(LiveviewContext.JpegPacket):
                    OnJpegPacket((sender as LiveviewContext).JpegPacket);
                    break;
                case nameof(LiveviewContext.FocusPacket):
                    OnFocusFramePacket((sender as LiveviewContext).FocusPacket);
                    break;
            }
        }

        private async void OnJpegPacket(JpegPacket packet)
        {
            if (IsDecoding)
            {
                PendingPakcet = packet;
                return;
            }

            IsDecoding = true;
            await DecodeLiveviewFrame(packet);
            IsDecoding = false;

            if (Context.HistogramCreator != null && ApplicationSettings.GetInstance().IsHistogramDisplayed && !Context.HistogramCreator.IsRunning)
            {
                rwLock.EnterReadLock();
                try
                {
                    Context.HistogramCreator.CreateHistogram(LiveviewImageBitmap);
                }
                finally
                {
                    rwLock.ExitReadLock();
                }
            }
        }

        private void OnFocusFramePacket(FocusFramePacket packet)
        {
            var task = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                FocusMarkDrawer.SetFocusFrames(packet.FocusFrames);
            });
        }

        private double dpi;
        bool decodedOnce = false;

        private async Task DecodeLiveviewFrame(JpegPacket packet, bool retry = false)
        {
            Action trailingTask = null;

            if (LiveviewImageBitmap == null || sizeChanged)
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                {
                    var writeable = await LiveviewUtil.AsWriteableBitmap(packet.ImageData, Dispatcher);
                    OriginalLvSize = new BitmapSize { Width = (uint)writeable.PixelWidth, Height = (uint)writeable.PixelHeight };

                    var magnification = CalcLiveviewMagnification();
                    DebugUtil.Log(() => { return "Decode: mag: " + magnification + " offsetV: " + LvOffsetV; });
                    dpi = DEFAULT_DPI / magnification;

                    RefreshOverlayControlParams(magnification);

                    trailingTask = () =>
                    {
                        if (Context?.Target?.Status != null)
                        {
                            RotateLiveviewImage(Context.Target.Status.LiveviewOrientationAsDouble, (sender, arg) =>
                            {
                                RefreshOverlayControlParams(magnification);
                            });
                        }

                        sizeChanged = false;
                    };
                });
            }
            else
            {
                rwLock.EnterWriteLock();

                try
                {
                    var toDelete = LiveviewImageBitmap;
                    trailingTask = () =>
                    {
                        // Dispose after it is drawn
                        toDelete?.Dispose();
                    };
                }
                finally
                {
                    rwLock.ExitWriteLock();
                }
            }

            using (var stream = new InMemoryRandomAccessStream())
            {
                await stream.WriteAsync(packet.ImageData.AsBuffer());
                stream.Seek(0);

                var bmp = await CanvasBitmap.LoadAsync(LiveviewImageCanvas, stream, (float)dpi);
                var size = bmp.SizeInPixels;

                rwLock.EnterWriteLock();
                try
                {
                    LiveviewImageBitmap = bmp;
                }
                finally
                {
                    rwLock.ExitWriteLock();
                }

                if (!OriginalLvSize.Equals(size))
                {
                    DisposeLiveviewImageBitmap();
                    if (!retry)
                    {
                        await DecodeLiveviewFrame(packet, true);
                    }
                    return;
                }
            }

            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                LiveviewImageCanvas.Invalidate();
                trailingTask?.Invoke();
            });

            if (PendingPakcet != null)
            {
                var task = DecodeLiveviewFrame(PendingPakcet);
                PendingPakcet = null;
            }
        }

        public void RotateLiveviewImage(double angle, EventHandler<object> Completed = null)
        {
            var scale = CalcRotatedLiveviewImageScale(angle);
            if (scale == double.NaN) { return; } // LiveviewGrid is already disappeared.

            angle = ToRelativeLiveviewAngle(angle);

            AnimationHelper.CreateSmoothRotateScaleAnimation(new AnimationRequest()
            {
                Target = LiveviewGrid,
                Completed = Completed,
            }, angle, scale).Begin();
        }

        private double CalcLiveviewMagnification()
        {
            var mag_h = LiveviewImageCanvas.ActualWidth / OriginalLvSize.Width;
            var mag_v = LiveviewImageCanvas.ActualHeight / OriginalLvSize.Height;
            return Math.Min(mag_h, mag_v);
        }

        private void CanvasControl_Draw(CanvasControl sender, CanvasDrawEventArgs args)
        {
            if (!decodedOnce) { return; }

            rwLock.EnterReadLock();
            try
            {
                if (LiveviewImageBitmap == null) { return; }
                args.DrawingSession.DrawImage(LiveviewImageBitmap, (float)LvOffsetH, (float)LvOffsetV);
            }
            finally
            {
                rwLock.ExitReadLock();
            }
            LiveviewFrameCount++;
        }

        private void LiveviewImageCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            DisposeLiveviewImageBitmap();
        }

        private void DisposeLiveviewImageBitmap()
        {
            rwLock.EnterWriteLock();
            try
            {
                LiveviewImageBitmap?.Dispose();
                LiveviewImageBitmap = null;
            }
            finally
            {
                rwLock.ExitWriteLock();
            }
        }

        private bool sizeChanged = false;

        private void Grid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            sizeChanged = true;
            DisposeLiveviewImageBitmap();
        }

        private void RefreshOverlayControlParams(double magnification)
        {
            double imageHeight, imageWidth;

            rwLock.EnterReadLock();
            try
            {
                if (LiveviewImageBitmap == null)
                {
                    // Maybe changing window size
                    return;
                }

                imageHeight = LiveviewImageBitmap.SizeInPixels.Height * magnification;
                imageWidth = LiveviewImageBitmap.SizeInPixels.Width * magnification;
            }
            finally
            {
                rwLock.ExitReadLock();
            }

            LvOffsetV = (LiveviewImageCanvas.ActualHeight - imageHeight) / 2;
            LvOffsetH = (LiveviewImageCanvas.ActualWidth - imageWidth) / 2;

            decodedOnce = true;

            FocusMarkDrawer.Height = imageHeight;
            FocusMarkDrawer.Width = imageWidth;
            FramingGuideSurface.Height = imageHeight;
            FramingGuideSurface.Width = imageWidth;

            FocusMarkDrawer.Margin = new Thickness(LvOffsetH, LvOffsetV, 0, 0);
            FramingGuideSurface.Margin = new Thickness(LvOffsetH, LvOffsetV, 0, 0);
        }

        private double CalcRotatedLiveviewImageScale(double angle)
        {
            double scale_h = 1.0;
            double scale_v = 1.0;
            if (LiveviewImageCanvas == null || LiveviewGrid == null) { return 1.0; }

            var screen_w = LiveviewGrid.ActualWidth;
            var screen_h = LiveviewGrid.ActualHeight;

            if (angle % 180 == 0)
            {
                scale_h = screen_w / LiveviewImageCanvas.RenderSize.Width;
                scale_v = screen_h / LiveviewImageCanvas.RenderSize.Height;
            }
            else
            {
                scale_h = screen_w / LiveviewImageCanvas.RenderSize.Height;
                scale_v = screen_h / LiveviewImageCanvas.RenderSize.Width;
            }

            if (LiveviewGrid.ActualHeight > LiveviewGrid.ActualWidth)
            {
                // portrait
                return Math.Max(scale_h, scale_v);
            }
            else
            {
                return Math.Min(scale_h, scale_v);
            }
        }

        private double ToRelativeLiveviewAngle(double angle)
        {
            var t = LiveviewGrid?.RenderTransform as CompositeTransform;
            if (t != null)
            {
                angle = angle - t.Rotation;

                if (angle > 180)
                {
                    angle = angle - ((int)(angle / 360) + 1) * 360;
                }
                else if (angle < -180)
                {
                    angle = angle + ((int)(-angle / 360) + 1) * 360;
                }
            }
            return angle;
        }

        public void SetFocusedMark(bool focused)
        {
            FocusMarkDrawer.Focused = focused;
        }
    }

    public class LiveviewContext : ObservableBase
    {
        public LiveviewContext(TargetDevice target, HistogramCreator histogram = null)
        {
            Target = target;
            HistogramCreator = histogram;
        }

        public TargetDevice Target
        {
            get; private set;
        }

        public HistogramCreator HistogramCreator { get; private set; }

        private JpegPacket _JpegPacket;
        public JpegPacket JpegPacket
        {
            get { return _JpegPacket; }
            set
            {
                _JpegPacket = value;
                NotifyChanged(nameof(JpegPacket));
            }
        }

        private FocusFramePacket _FocusPacket;
        public FocusFramePacket FocusPacket
        {
            get { return _FocusPacket; }
            set
            {
                _FocusPacket = value;
                NotifyChanged(nameof(FocusPacket));
            }
        }
    }
}
