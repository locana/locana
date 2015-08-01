using Kazyx.Uwpmm.Utility;
using System;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Kazyx.Uwpmm.Control
{
    public sealed partial class PhotoPlaybackScreen : UserControl
    {
        public PhotoPlaybackScreen()
        {
            this.InitializeComponent();
        }

        public bool AnimationRunning = false;

        private bool _DetailInfoDisplayed = false;
        public bool DetailInfoDisplayed
        {
            get { return _DetailInfoDisplayed; }
            set
            {
                if (_DetailInfoDisplayed != value)
                {
                    StartSlideAnimation(value);
                }
            }
        }

        void StartSlideAnimation(bool displayed)
        {
            if (AnimationRunning) { return; }
            AnimationRunning = true;
            if (displayed)
            {
                AnimationHelper.CreateSlideAnimation(new AnimationRequest()
                {
                    Target = DetailInfoPanel,
                    Duration = TimeSpan.FromMilliseconds(160),
                    Completed = (sender, obj) =>
                    {
                        _DetailInfoDisplayed = true;
                        AnimationRunning = false;
                    }
                }, FadeSide.Right, FadeType.FadeIn).Begin();
            }
            else
            {
                AnimationHelper.CreateSlideAnimation(new AnimationRequest()
                {
                    Target = DetailInfoPanel,
                    Duration = TimeSpan.FromMilliseconds(200),
                    Completed = (sender, obj) =>
                    {
                        _DetailInfoDisplayed = false;
                        AnimationRunning = false;
                    }
                }, FadeSide.Right, FadeType.FadeOut).Begin();
            }
        }

        public void RotateImage(Rotation r)
        {
            var transform = Image.RenderTransform as CompositeTransform;
            if (transform != null)
            {
                switch (r)
                {
                    case Rotation.Left:
                        RotateSmoothly(Image, -90);
                        break;
                    case Rotation.Right:
                        RotateSmoothly(Image, 90);
                        break;
                }
            }
        }

        bool IsRotating = false;
        public void RotateSmoothly(FrameworkElement target, double angle)
        {
            if (IsRotating) { return; }
            IsRotating = true;
            AnimationHelper.CreateSmoothRotateAnimation(new AnimationRequest()
            {
                Target = target,
                Completed = (obj, sender) =>
                {
                    IsRotating = false;
                }
            }, angle).Begin();
        }

        public static readonly DependencyProperty SourceBitmapProperty = DependencyProperty.Register(
            "SourceBitmap",
            typeof(BitmapImage),
            typeof(PhotoPlaybackScreen),
            new PropertyMetadata(null, new PropertyChangedCallback(PhotoPlaybackScreen.OnSourceBitmapUpdated)));

        private static void OnSourceBitmapUpdated(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as PhotoPlaybackScreen).SourceBitmap = (BitmapImage)e.NewValue;
        }

        const double MAX_SCALE = 5.0;
        const double MIN_SCALE = 0.9;
        const double IMAGE_CLEARANCE = 50;

        BitmapImage _SourceBitmap;
        public BitmapImage SourceBitmap
        {
            get { return _SourceBitmap; }
            set
            {
                _SourceBitmap = value;
            }
        }

        public void SetBitmap()
        {
            Image.Source = _SourceBitmap;
        }

        public void Init()
        {
            var transform = Image.RenderTransform as CompositeTransform;
            if (transform != null)
            {
                transform.ScaleX = 1;
                transform.ScaleY = 1;
                if (transform.Rotation != 0)
                {
                    RotateSmoothly(Image, -transform.Rotation);
                }
                transform.TranslateX = 0;
                transform.TranslateY = 0;
            }
        }

        internal void ReleaseImage()
        {
            _SourceBitmap = null;
        }

        private void Image_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            UIElement element = sender as UIElement;
            var parent = (sender as Image).Parent as ScrollViewer;
            CompositeTransform transform = element.RenderTransform as CompositeTransform;
            if (transform != null && parent != null)
            {
                transform.ScaleX = LimitToRange(transform.ScaleX * e.Delta.Scale, MIN_SCALE, MAX_SCALE);
                transform.ScaleY = LimitToRange(transform.ScaleY * e.Delta.Scale, MIN_SCALE, MAX_SCALE);

                double h_size, v_size;
                if (transform.Rotation % 180 == 0)
                {
                    h_size = element.RenderSize.Width * transform.ScaleX;
                    v_size = element.RenderSize.Height * transform.ScaleY;
                }
                else
                {
                    h_size = element.RenderSize.Height * transform.ScaleY;
                    v_size = element.RenderSize.Width * transform.ScaleX;
                }
                var translateLimitX = (parent.ActualWidth + h_size) / 2 - IMAGE_CLEARANCE;
                var translateLimitY = (parent.ActualHeight + v_size) / 2 - IMAGE_CLEARANCE;
                transform.TranslateX = LimitToRange(transform.TranslateX + e.Delta.Translation.X, -translateLimitX, translateLimitX);
                transform.TranslateY = LimitToRange(transform.TranslateY + e.Delta.Translation.Y, -translateLimitY, translateLimitY);
            }
        }

        double LimitToRange(double value, double min, double max)
        {
            if (value > max) { return max; }
            if (value < min) { return min; }
            return value;
        }

        private void Image_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            Init();
        }
    }

    public enum Rotation
    {
        Right,
        Left,
    }
}
