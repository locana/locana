using Kazyx.Uwpmm.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Kazyx.Uwpmm.Control
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
            DebugUtil.Log("Enqueue toast: " + content.Text);
            Contents.Add(content);
            if (!Running) { DequeueToast(); }
        }

        async void DequeueToast()
        {
            if (Contents.Count == 0) { return; }

            var content = Contents.ElementAt(0);

            ToastGrid.DataContext = content;
            DebugUtil.Log("Dequeue toast:" + content.Text);
            Running = true;
            AnimationHelper.CreateSlideAnimation(new AnimationRequest()
            {
                Target = ToastGrid,
                Duration = TimeSpan.FromMilliseconds(SLIDE_DURATION),
            }, FadeSide.Top, FadeType.FadeIn).Begin();

            var duration = TimeSpan.FromMilliseconds(3000);
            if (content.Duration != null && content.Duration.Milliseconds > SLIDE_DURATION)
            {
                duration = content.Duration;
            }

            await System.Threading.Tasks.Task.Delay(duration);

            AnimationHelper.CreateSlideAnimation(new AnimationRequest()
            {
                Target = ToastGrid,
                Duration = TimeSpan.FromMilliseconds(SLIDE_DURATION),
                Completed = (e_, sender_) =>
                {
                    Contents.RemoveAt(0);
                    Running = false;
                    DequeueToast();
                }
            }, FadeSide.Top, FadeType.FadeOut).Begin();
        }
    }

    public class ToastContent
    {
        public ToastContent() { }
        public String Text { get; set; }
        public BitmapImage Icon { get; set; }
        public TimeSpan Duration { get; set; }
    }
}
