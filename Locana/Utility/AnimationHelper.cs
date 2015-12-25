using System;
using System.Collections.Generic;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;

namespace Locana.Utility
{
    public class AnimationHelper
    {

        public static Storyboard CreateSlideAnimation(SlideAnimationRequest request)
        {
            var sb = new Storyboard();
            var slide = new DoubleAnimationUsingKeyFrames();
            var transform = new TranslateTransform();
            var duration = request.Duration.Milliseconds;

            var KeyframeTimes = _KeyframeTimes(request.RequestFadeType, duration);
            List<double> KeyframeDistance = new List<double>();


            Storyboard.SetTarget(slide, transform);
            request.Target.RenderTransform = transform;

            if (request.WithFade)
            {
                var fade = CreateFadeKeyframes(request.RequestFadeType, KeyframeTimes);
                Storyboard.SetTargetProperty(fade, "Opacity");
                Storyboard.SetTarget(fade, request.Target);
                sb.Children.Add(fade);
            }

            double _distance = 0;
            double requested_distance = request.Distance;

            switch (request.RequestFadeType)
            {
                case FadeType.FadeIn:

                    switch (request.RequestFadeSide)
                    {
                        case FadeSide.Top:
                            _distance = requested_distance == 0 ? -request.Target.ActualHeight : requested_distance;
                            Storyboard.SetTargetProperty(slide, "Y");
                            KeyframeDistance = new List<double>() { _distance, _distance * 0.5, 0 };
                            break;
                        case FadeSide.Bottom:
                            _distance = requested_distance == 0 ? request.Target.ActualHeight : requested_distance;
                            Storyboard.SetTargetProperty(slide, "Y");
                            KeyframeDistance = new List<double>() { _distance, _distance * 0.5, 0 };
                            break;
                        case FadeSide.Left:
                            _distance = requested_distance == 0 ? -request.Target.ActualWidth : requested_distance;
                            Storyboard.SetTargetProperty(slide, "X");
                            KeyframeDistance = new List<double>() { _distance, _distance * 0.5, 0 };
                            break;
                        case FadeSide.Right:
                            _distance = requested_distance == 0 ? request.Target.ActualWidth : requested_distance;
                            Storyboard.SetTargetProperty(slide, "X");
                            KeyframeDistance = new List<double>() { _distance, _distance * 0.5, 0 };
                            break;
                    }
                    break;
                case FadeType.FadeOut:
                    switch (request.RequestFadeSide)
                    {
                        case FadeSide.Top:
                            _distance = requested_distance == 0 ? -request.Target.ActualHeight : requested_distance;
                            Storyboard.SetTargetProperty(slide, "Y");
                            KeyframeDistance = new List<double>() { 0, _distance * 0.2, _distance };
                            break;
                        case FadeSide.Bottom:
                            _distance = requested_distance == 0 ? request.Target.ActualHeight : requested_distance;
                            Storyboard.SetTargetProperty(slide, "Y");
                            KeyframeDistance = new List<double>() { 0, _distance * 0.2, _distance };
                            break;
                        case FadeSide.Left:
                            _distance = requested_distance == 0 ? -request.Target.ActualWidth : requested_distance;
                            Storyboard.SetTargetProperty(slide, "X");
                            KeyframeDistance = new List<double>() { 0, _distance * 0.2, _distance };
                            break;
                        case FadeSide.Right:
                            _distance = requested_distance == 0 ? request.Target.ActualWidth : requested_distance;
                            Storyboard.SetTargetProperty(slide, "X");
                            KeyframeDistance = new List<double>() { 0, _distance * 0.2, _distance };
                            break;
                    }
                    break;
            }

            for (int i = 0; i < KeyframeTimes.Count; i++)
            {
                slide.KeyFrames.Add(new EasingDoubleKeyFrame() { KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(KeyframeTimes[i])), Value = KeyframeDistance[i] });
            }

            sb.Children.Add(slide);
            if (request.Completed != null) { sb.Completed += request.Completed; }
            return sb;
        }

        public static Storyboard CreateMoveAndResizeAnimation(MoveAndResizeAnimation request)
        {
            var transform = request.Target.RenderTransform as CompositeTransform;

            if (transform == null)
            {
                transform = new CompositeTransform()
                {
                    TranslateX = 0,
                    TranslateY = 0,
                    ScaleX = 1,
                    ScaleY = 1,
                };
                request.Target.RenderTransform = transform;
            }

            var duration = new Duration(TimeSpan.FromMilliseconds(200));
            if (request.Duration != null && request.Duration.Milliseconds != 0)
            {
                duration = request.Duration;
            }

            var sb = new Storyboard() { Duration = duration };

            var animation_x = _Animation(duration, transform.TranslateX, request.newX - transform.TranslateX);
            var animation_scale_x = _Animation(duration, transform.ScaleX, request.newScaleX - transform.ScaleX);
            var animation_scale_y = _Animation(duration, transform.ScaleY, request.newScaleY - transform.ScaleY);
            Storyboard.SetTargetProperty(animation_x, "TranslateX");
            Storyboard.SetTargetProperty(animation_scale_x, "ScaleX");
            Storyboard.SetTargetProperty(animation_scale_y, "ScaleY");
            Storyboard.SetTarget(animation_x, transform);
            Storyboard.SetTarget(animation_scale_x, transform);
            Storyboard.SetTarget(animation_scale_y, transform);

            sb.Children.Add(animation_x);
            sb.Children.Add(animation_scale_x);
            sb.Children.Add(animation_scale_y);

            if (request.Completed != null)
            {
                sb.Completed += request.Completed;
            }

            return sb;
        }

        static List<double> _KeyframeTimes(FadeType type, double duration)
        {
            switch (type)
            {
                case FadeType.FadeIn:
                    return new List<double>() { 0, duration / 6, duration };
                case FadeType.FadeOut:
                    return new List<double>() { 0, duration / 6, duration };
            }
            return new List<double>() { 0, duration / 6, duration };
        }

        public static Storyboard CreateFadeAnimation(FadeAnimationRequest request)
        {
            var sb = new Storyboard();
            var duration = request.Duration.Milliseconds;
            var KeyframeTimes = _KeyframeTimes(request.RequestFadeType, duration);

            List<double> KeyframeDistance = new List<double>();

            var fade = CreateFadeKeyframes(request.RequestFadeType, KeyframeTimes);
            Storyboard.SetTargetProperty(fade, "Opacity");
            Storyboard.SetTarget(fade, request.Target);
            sb.Children.Add(fade);

            if (request.Completed != null) { sb.Completed += request.Completed; }
            return sb;
        }

        static DoubleAnimationUsingKeyFrames CreateFadeKeyframes(FadeType type, List<double> KeyframeTimes)
        {
            var fade = new DoubleAnimationUsingKeyFrames();
            var KeyframeOpacity = new List<double>();

            switch (type)
            {
                case FadeType.FadeIn:
                    KeyframeOpacity = new List<double>() { 0, 0.6, 1.0 };
                    break;
                case FadeType.FadeOut:
                    KeyframeOpacity = new List<double>() { 1.0, 0.6, 0 };
                    break;
            }

            for (int i = 0; i < KeyframeTimes.Count; i++)
            {
                fade.KeyFrames.Add(new EasingDoubleKeyFrame() { KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(KeyframeTimes[i])), Value = KeyframeOpacity[i] });
            }

            return fade;
        }

        private static DoubleAnimationUsingKeyFrames _Animation(Duration duration, double origin, double diff)
        {
            var animation = new DoubleAnimationUsingKeyFrames();
            animation.KeyFrames.Add(new EasingDoubleKeyFrame() { KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(0)), Value = origin });
            animation.KeyFrames.Add(new EasingDoubleKeyFrame() { KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(duration.TimeSpan.Milliseconds / 3)), Value = origin + diff * 0.7 });
            animation.KeyFrames.Add(new EasingDoubleKeyFrame() { KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(duration.TimeSpan.Milliseconds)), Value = origin + diff });
            return animation;
        }

        public static Storyboard CreateSmoothRotateAnimation(AnimationRequest request, double angle)
        {
            var transform = request.Target.RenderTransform as CompositeTransform;

            if (transform == null)
            {
                transform = new CompositeTransform()
                {
                    Rotation = 0,
                };
                request.Target.RenderTransform = transform;
            }

            transform.CenterX = request.Target.RenderSize.Width / 2;
            transform.CenterY = request.Target.RenderSize.Height / 2;

            var duration = new Duration(TimeSpan.FromMilliseconds(200));
            if (request.Duration != null && request.Duration.Milliseconds != 0)
            {
                duration = request.Duration;
            }

            var sb = new Storyboard() { Duration = duration };

            var animation = _Animation(duration, transform.Rotation, angle);
            Storyboard.SetTargetProperty(animation, "Rotation");
            Storyboard.SetTarget(animation, transform);

            sb.Children.Add(animation);

            if (request.Completed != null)
            {
                sb.Completed += request.Completed;
            }

            return sb;
        }

        /// <summary>
        /// Create a storyboard with an animation to change angle and scale
        /// </summary>
        /// <param name="request">A object that includes target UIElement</param>
        /// <param name="angle">relative angle to rotate. e.g.: -90</param>
        /// <param name="scale">absolute scale. e.g.: 1.5</param>
        /// <returns></returns>
        public static Storyboard CreateSmoothRotateScaleAnimation(AnimationRequest request, double angle, double scale)
        {
            var transform = request.Target.RenderTransform as CompositeTransform;

            if (transform == null)
            {
                transform = new CompositeTransform()
                {
                    ScaleX = 1.0,
                    ScaleY = 1.0,
                    Rotation = 0,
                };
                request.Target.RenderTransform = transform;
            }

            transform.CenterX = request.Target.RenderSize.Width / 2;
            transform.CenterY = request.Target.RenderSize.Height / 2;

            var duration = new Duration(TimeSpan.FromMilliseconds(200));
            if (request.Duration != null && request.Duration.Milliseconds != 0)
            {
                duration = request.Duration;
            }

            var sb = new Storyboard() { Duration = duration };

            var animation = _Animation(duration, transform.Rotation, angle);
            Storyboard.SetTargetProperty(animation, "Rotation");
            Storyboard.SetTarget(animation, transform);
            sb.Children.Add(animation);

            var scaleXAnimation = _Animation(duration, transform.ScaleX, scale - transform.ScaleX);
            Storyboard.SetTargetProperty(scaleXAnimation, "ScaleX");
            Storyboard.SetTarget(scaleXAnimation, transform);
            sb.Children.Add(scaleXAnimation);

            var scaleYAnimation = _Animation(duration, transform.ScaleY, scale - transform.ScaleY);
            Storyboard.SetTargetProperty(scaleYAnimation, "ScaleY");
            Storyboard.SetTarget(scaleYAnimation, transform);
            sb.Children.Add(scaleYAnimation);

            if (request.Completed != null)
            {
                sb.Completed += request.Completed;
            }
            return sb;
        }

        public static Storyboard CreateRotateAnimation(AnimationRequest request, double from, double to)
        {
            var duration = new Duration(TimeSpan.FromMilliseconds(200));
            if (request.Duration != null && request.Duration.Milliseconds != 0)
            {
                duration = request.Duration;
            }

            var sb = new Storyboard() { Duration = duration };
            var da = new DoubleAnimation() { Duration = duration };

            sb.Children.Add(da);

            var rt = new RotateTransform();

            Storyboard.SetTarget(da, rt);
            Storyboard.SetTargetProperty(da, "Angle");
            da.From = from;
            da.To = to;

            request.Target.RenderTransform = rt;
            request.Target.RenderTransformOrigin = new Point(0.5, 0.5);

            return sb;
        }

    }

    public class AnimationRequest
    {
        public FrameworkElement Target { get; set; }
        public TimeSpan Duration { get; set; }
        public EventHandler<object> Completed { get; set; }
    }

    public class FadeAnimationRequest : AnimationRequest
    {
        public FadeType RequestFadeType { get; set; }
    }

    public class MoveAndResizeAnimation : AnimationRequest
    {
        public double newX { get; set; }
        public double newY { get; set; }
        public double newScaleX { get; set; }
        public double newScaleY { get; set; }
    }

    public class SlideAnimationRequest : AnimationRequest
    {
        private double _Distance = 0;
        /// <summary>
        /// [Optional] If this is not set, distance of slide animation is determined by the target's size.
        /// </summary>
        public double Distance
        {
            get { return _Distance; }
            set { _Distance = value; }
        }

        private bool _WithFade = true;
        public bool WithFade
        {
            get { return _WithFade; }
            set { _WithFade = value; }
        }

        public FadeSide RequestFadeSide { get; set; }
        public FadeType RequestFadeType { get; set; }
    }

    public enum FadeSide
    {
        Left,
        Top,
        Right,
        Bottom,
    }

    public enum FadeType
    {
        FadeIn,
        FadeOut,
    }


}
