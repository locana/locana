using Locana.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Locana.Controls
{
    public sealed partial class Toast : UserControl
    {
        public Toast()
        {
            this.InitializeComponent();
        }

        private List<ToastContent> Contents = new List<ToastContent>();
        private bool Running = false;
        const int SLIDE_DURATION = 120;

        public void PushToast(ToastContent content)
        {
            DebugUtil.Log(() => "Enqueue toast: " + content.Text);
            Contents.Add(content);
            if (!Running) { DequeueToast(); }
        }

        async void DequeueToast()
        {
            var content = Contents.FirstOrDefault();
            if (content == null)
            {
                return;
            }

            ToastGrid.DataContext = content;
            DebugUtil.Log(() => "Dequeue toast:" + content.Text);
            Running = true;
            AnimationHelper.CreateSlideAnimation(new SlideAnimationRequest()
            {
                Target = ToastGrid,
                Duration = TimeSpan.FromMilliseconds(SLIDE_DURATION),
                RequestFadeSide = FadeSide.Top,
                RequestFadeType = FadeType.FadeIn
            }).Begin();

            var duration = TimeSpan.FromMilliseconds(3000);
            if (content.Duration != null && content.Duration.Milliseconds > SLIDE_DURATION)
            {
                duration = content.Duration;
            }

            await System.Threading.Tasks.Task.Delay(duration);

            AnimationHelper.CreateSlideAnimation(new SlideAnimationRequest()
            {
                Target = ToastGrid,
                Duration = TimeSpan.FromMilliseconds(SLIDE_DURATION),
                Completed = (e_, sender_) =>
                {
                    Contents.RemoveAt(0);
                    Running = false;
                    DequeueToast();
                },
                RequestFadeSide = FadeSide.Top,
                RequestFadeType = FadeType.FadeOut
            }).Begin();
        }

        private void ToastGrid_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            DebugUtil.Log(() => "Toast tapped");
            Contents.FirstOrDefault()?.OnTapped?.Invoke();
        }
    }

    public class ToastContent
    {
        public ToastContent() { }
        public string Text { get; set; }
        public BitmapImage Icon { get; set; }
        public double MaxIconHeight { get; set; } = double.PositiveInfinity;
        public TimeSpan Duration { get; set; }
        public Action OnTapped { get; set; }
    }
}
