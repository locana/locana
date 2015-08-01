using System;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;

namespace Kazyx.Uwpmm.Utility
{
    public class AnimationHelper
    {
        public static Storyboard CreateSlideAnimation(AnimationRequest request, FadeSide edge, FadeType fadetype)
        {
            double distance = 0;
            var sb = new Storyboard();
            var slide = new DoubleAnimationUsingKeyFrames();
            var fade = new DoubleAnimationUsingKeyFrames();
            var transform = new TranslateTransform();
            var duration = request.Duration.Milliseconds;

            var KeyframeTimes = new List<double>() { 0, duration / 6, duration }; // 3 key frames.
            List<double> KeyframeDistance = new List<double>();
            List<double> KeyframeOpacity = new List<double>();

            Storyboard.SetTarget(slide, transform);
            Storyboard.SetTargetProperty(fade, "Opacity");
            Storyboard.SetTarget(fade, request.Target);
            request.Target.RenderTransform = transform;

            switch (fadetype)
            {
                case FadeType.FadeIn:
                    KeyframeOpacity = new List<double>() { 0, 0.8, 1.0 };

                    switch (edge)
                    {
                        case FadeSide.Top:
                            distance = -request.Target.ActualHeight;
                            Storyboard.SetTargetProperty(slide, "Y");
                            KeyframeDistance = new List<double>() { distance, distance * 0.5, 0 };
                            break;
                        case FadeSide.Bottom:
                            distance = request.Target.ActualHeight;
                            Storyboard.SetTargetProperty(slide, "Y");
                            KeyframeDistance = new List<double>() { distance, distance * 0.5, 0 };
                            break;
                        case FadeSide.Left:
                            distance = -request.Target.ActualWidth;
                            Storyboard.SetTargetProperty(slide, "X");
                            KeyframeDistance = new List<double>() { distance, distance * 0.5, 0 };
                            break;
                        case FadeSide.Right:
                            distance = request.Target.ActualWidth;
                            Storyboard.SetTargetProperty(slide, "X");
                            KeyframeDistance = new List<double>() { distance, distance * 0.5, 0 };
                            break;
                    }
                    break;
                case FadeType.FadeOut:
                    KeyframeOpacity = new List<double>() { 1.0, 0.6, 0 };
                    switch (edge)
                    {
                        case FadeSide.Top:
                            distance = -request.Target.ActualHeight;
                            Storyboard.SetTargetProperty(slide, "Y");
                            KeyframeDistance = new List<double>() { 0, distance * 0.2, distance };
                            break;
                        case FadeSide.Bottom:
                            distance = request.Target.ActualHeight;
                            Storyboard.SetTargetProperty(slide, "Y");
                            KeyframeDistance = new List<double>() { 0, distance * 0.2, distance };
                            break;
                        case FadeSide.Left:
                            distance = -request.Target.ActualWidth;
                            Storyboard.SetTargetProperty(slide, "X");
                            KeyframeDistance = new List<double>() { 0, distance * 0.2, distance };
                            break;
                        case FadeSide.Right:
                            distance = request.Target.ActualWidth;
                            Storyboard.SetTargetProperty(slide, "X");
                            KeyframeDistance = new List<double>() { 0, distance * 0.2, distance };
                            break;
                    }
                    break;
            }

            for (int i = 0; i < KeyframeTimes.Count; i++)
            {
                slide.KeyFrames.Add(new EasingDoubleKeyFrame() { KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(KeyframeTimes[i])), Value = KeyframeDistance[i] });
                fade.KeyFrames.Add(new EasingDoubleKeyFrame() { KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(KeyframeTimes[i])), Value = KeyframeOpacity[i] });
            }

            sb.Children.Add(slide);
            sb.Children.Add(fade);
            if (request.Completed != null) { sb.Completed += request.Completed; }
            return sb;
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
    }

    public class AnimationRequest
    {
        public FrameworkElement Target { get; set; }
        public TimeSpan Duration { get; set; }
        public EventHandler<object> Completed { get; set; }
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
