using Kazyx.RemoteApi;
using Kazyx.Uwpmm.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Locana.Control
{
    public sealed partial class MultiModeShutterButton : UserControl
    {
        public MultiModeShutterButton()
        {
            this.InitializeComponent();
        }

        private ShootModeInfo _ModeInfo;
        public ShootModeInfo ModeInfo
        {
            set
            {
                bool refresh = this._ModeInfo == null || IsUpdated(this._ModeInfo.ShootModeCapability, value.ShootModeCapability);
                UpdateCandidates(value, refresh);
                this._ModeInfo = value;
            }
        }

        const double BUTTON_SIZE = 80;

        /// <summary>
        /// Clear all buttons and create new buttons.
        /// </summary>
        /// <param name="info"></param>
        async void UpdateCandidates(ShootModeInfo info, bool refresh)
        {
            if (info == null) { return; }

            var selectedIndex = SettingValueConverter.GetSelectedIndex(info.ShootModeCapability);
            var xCenter = this.ActualWidth / 2;
            var yCenter = this.ActualHeight / 2;
            var bigSize = this.ActualWidth * 0.8;
            var smallSize = bigSize / 2;

            int currentIndex = 0;

            if (refresh)
            {
                LayoutRoot.Children.Clear();
            }


            foreach (var i in info.ShootModeCapability.Candidates)
            {
                BitmapImage icon = null;
                if (info.Icons != null && info.Icons.ContainsKey(i))
                {
                    icon = info.Icons[i];
                }

                var scale = i == info.ShootModeCapability.Current ? 1.0 : 0.6;
                EllipseButton button;
                double newX = (currentIndex - selectedIndex) * 50;
                if (newX < 0) { newX -= 30; }
                else if (newX > 0) { newX += 30; }

                if (refresh)
                {
                    button = CreateBaseButton(icon, scale);
                    LayoutRoot.Children.Add(button);
                    (button.RenderTransform as CompositeTransform).TranslateX = newX;
                }
                else
                {

                    await Dispatcher.RunAsync(CoreDispatcherPriority.High, () =>
                    {
                        button = LayoutRoot.Children[currentIndex] as EllipseButton;
                        AnimationHelper.CreateMoveAndResizeAnimation(new MoveAndResizeAnimation()
                        {
                            Target = button,
                            Duration = TimeSpan.FromMilliseconds(500),
                            newX = newX,
                            newScaleX = scale,
                            newScaleY = scale,
                        }).Begin();
                    });
                }

                currentIndex++;
            }
        }

        bool IsUpdated(Capability<string> last, Capability<string> current)
        {
            if (last == null || current == null || last.Candidates == null || current.Candidates == null)
            {
                return true;
            }

            if (last.Candidates.Count != current.Candidates.Count) { return true; }

            return false;
        }

        EllipseButton CreateBaseButton(BitmapImage icon, double scale)
        {
            var button = new EllipseButton()
            {
                Width = BUTTON_SIZE,
                Height = BUTTON_SIZE,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Bottom,
                Margin = new Thickness(0),
                Icon = icon,
                RenderTransform = new CompositeTransform()
                {
                    TranslateX = 0,
                    TranslateY = 0,
                    ScaleX = scale,
                    ScaleY = scale,
                    CenterX = BUTTON_SIZE / 2,
                    CenterY = BUTTON_SIZE,
                },
                Tapped = ElementTapped,
            };

            return button;
        }

        private void ElementTapped(object s)
        {
            var sender = s as EllipseButton;
            int i = 0;
            foreach (var e in LayoutRoot.Children)
            {
                if (e.Equals(sender))
                {
                    if (this._ModeInfo == null || this._ModeInfo.ShootModeCapability == null || this._ModeInfo.ShootModeCapability.Candidates.Count <= i) { return; }

                    if (this._ModeInfo.ShootModeCapability.Candidates[i] == this._ModeInfo.ShootModeCapability.Current)
                    {
                        if (this._ModeInfo.ButtonPressed != null) { this._ModeInfo.ButtonPressed(); }
                    }
                    else
                    {
                        // When one of other buttons is selected
                        if (this._ModeInfo.ModeSelected != null)
                        {
                            this._ModeInfo.ModeSelected(this._ModeInfo.ShootModeCapability.Candidates[i]);
                        }
                    }
                }
                i++;
            }
        }

        private void Grid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateCandidates(_ModeInfo, false);
        }
    }

    public class ShootModeInfo
    {
        public Capability<string> ShootModeCapability { get; set; }
        public Dictionary<string, BitmapImage> Icons { get; set; }
        public Action<string> ModeSelected { get; set; }
        public Action ButtonPressed { get; set; }
    }
}
