using Locana.Utility;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Locana.Controls
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
                    if (AlwaysShowDetailInfo)
                    {
                        DebugUtil.Log(() => "AlwaysShowDetailInfo is true. Ignore setting DetailInfoDisplayed.");
                    }
                    else
                    {
                        _DetailInfoDisplayed = value;
                        DetailInfoDisplayStatusUpdated?.Invoke();
                        StartSlideAnimation(value);
                    }
                }
            }
        }

        private bool _AlwaysShowDetailInfo = false;
        public bool AlwaysShowDetailInfo
        {
            get { return _AlwaysShowDetailInfo; }
            set
            {
                DetailInfoDisplayed = value;
                if (_AlwaysShowDetailInfo != value)
                {
                    _AlwaysShowDetailInfo = value;
                    DetailInfoDisplayStatusUpdated?.Invoke();
                    UpdateLayout(value);
                }
            }
        }

        public Action DetailInfoDisplayStatusUpdated;

        void UpdateLayout(bool always_show)
        {
            if (always_show)
            {
                // for wide view
                Grid.SetColumnSpan(Image, 1);
            }
            else
            {
                Grid.SetColumnSpan(Image, 2);
            }

            InitImageTransform();
        }

        void StartSlideAnimation(bool displayed)
        {
            if (AnimationRunning) { return; }
            AnimationRunning = true;
            if (displayed)
            {
                AnimationHelper.CreateSlideAnimation(new SlideAnimationRequest()
                {
                    Target = DetailInfoPanel,
                    Duration = TimeSpan.FromMilliseconds(160),
                    Completed = (sender, obj) =>
                    {
                        _DetailInfoDisplayed = true;
                        AnimationRunning = false;
                    },
                    RequestFadeSide = FadeSide.Right,
                    RequestFadeType = FadeType.FadeIn
                }).Begin();
            }
            else
            {
                AnimationHelper.CreateSlideAnimation(new SlideAnimationRequest()
                {
                    Target = DetailInfoPanel,
                    Duration = TimeSpan.FromMilliseconds(200),
                    Completed = (sender, obj) =>
                    {
                        _DetailInfoDisplayed = false;
                        AnimationRunning = false;
                    },
                    RequestFadeSide = FadeSide.Right,
                    RequestFadeType = FadeType.FadeOut
                }).Begin();
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
            nameof(SourceBitmap),
            typeof(BitmapImage),
            typeof(PhotoPlaybackScreen),
            new PropertyMetadata(null, new PropertyChangedCallback(PhotoPlaybackScreen.OnSourceBitmapUpdated)));

        private static void OnSourceBitmapUpdated(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as PhotoPlaybackScreen).SourceBitmap = (BitmapImage)e.NewValue;
        }

        const double MAX_SCALE = 5.0;
        const double MIN_SCALE = 0.5;
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

        public void InitImageTransform()
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
            TransformImage(e.Delta.Scale, e.Delta.Translation.X, e.Delta.Translation.Y);
        }

        double LimitToRange(double value, double min, double max)
        {
            if (value > max) { return max; }
            if (value < min) { return min; }
            return value;
        }

        private void Image_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            InitImageTransform();
        }

        private void Image_PointerWheelChanged(object sender, PointerRoutedEventArgs e)
        {
            var image = sender as Image;

            var delta = e.GetCurrentPoint(image).Properties.MouseWheelDelta;
            var scale = 1.0 + (delta < 0 ? 0.1 : -0.1);

            TransformImage(scale, 0, 0);
        }

        private void TransformImage(double scale, double translationX, double translationY)
        {
            var parent = Image.Parent as Grid;
            var transform = Image.RenderTransform as CompositeTransform;

            transform.ScaleX = LimitToRange(transform.ScaleX * scale, MIN_SCALE, MAX_SCALE);
            transform.ScaleY = LimitToRange(transform.ScaleY * scale, MIN_SCALE, MAX_SCALE);

            double h_size, v_size;
            if (transform.Rotation % 180 == 0)
            {
                h_size = Image.RenderSize.Width * transform.ScaleX;
                v_size = Image.RenderSize.Height * transform.ScaleY;
            }
            else
            {
                h_size = Image.RenderSize.Height * transform.ScaleY;
                v_size = Image.RenderSize.Width * transform.ScaleX;
            }
            var translateLimitX = (parent.ActualWidth + h_size) / 2 - IMAGE_CLEARANCE;
            var translateLimitY = (parent.ActualHeight + v_size) / 2 - IMAGE_CLEARANCE;
            transform.TranslateX = LimitToRange(transform.TranslateX + translationX, -translateLimitX, translateLimitX);
            transform.TranslateY = LimitToRange(transform.TranslateY + translationY, -translateLimitY, translateLimitY);
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            StartSlideAnimation(DetailInfoDisplayed);
            UpdateLayout(AlwaysShowDetailInfo);
        }
    }

    public enum Rotation
    {
        Right,
        Left,
    }
}
