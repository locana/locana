using Kazyx.RemoteApi;
using Kazyx.Uwpmm.Utility;
using System;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Locana.Controls
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
                bool refresh = this._ModeInfo == null || IsUpdated(this._ModeInfo?.ShootModeCapability, value.ShootModeCapability);
                if (refresh)
                {
                    UpdateCandidates(value);
                    RequestingMode = this._ModeInfo?.ShootModeCapability?.Current;
                }
                else
                {
                    ShiftButtons(value, value.ShootModeCapability.Current);
                    UpdateProgressVisibility(this.RequestingMode, value.ShootModeCapability?.Current);
                    RequestingMode = this._ModeInfo?.ShootModeCapability?.Current;
                }

                this._ModeInfo = value;
            }
        }

        private string _RequestingMode = "";
        public string RequestingMode
        {
            get { return _RequestingMode; }
            private set
            {
                _RequestingMode = value;
                UpdateProgressVisibility(value, this._ModeInfo?.ShootModeCapability?.Current ?? "");
            }
        }

        void UpdateProgressVisibility(string requesting, string current)
        {
            if (requesting == null || current == null) { return; }

            if (requesting == current)
            {
                ProgressRing.IsActive = false;
                SetAllButtonEnabled(true);
            }
            else
            {
                ProgressRing.IsActive = true;
                SetAllButtonEnabled(false);
            }
        }

        private void SetAllButtonEnabled(bool enable)
        {
            foreach (EllipseButton b in this.Buttons.Children)
            {
                if (b != null) { b.Enabled = enable; }
            }
        }

        EllipseButton FindButton(string mode)
        {
            int i = 0;
            foreach (var b in this.Buttons.Children)
            {
                if (b as EllipseButton == null) { continue; }

                if (this._ModeInfo.ShootModeCapability.Candidates[i] == mode)
                {
                    return b as EllipseButton;
                }
                i++;
            }
            return null;
        }

        const double BUTTON_SIZE = 80;

        /// <summary>
        /// Clear all buttons and create new buttons.
        /// </summary>
        /// <param name="info"></param>
        void UpdateCandidates(ShootModeInfo info)
        {
            if (info == null) { return; }

            var targetShootMode = info.ShootModeCapability.Current;

            var selectedIndex = FindCandidateIndex(info.ShootModeCapability, targetShootMode);
            int currentIndex = 0;

            Buttons.Children.Clear();

            foreach (var i in info.ShootModeCapability.Candidates)
            {
                BitmapImage icon = null;
                if (info.Icons != null && info.Icons.ContainsKey(i))
                {
                    icon = info.Icons[i];
                }

                var scale = i == targetShootMode ? 1.0 : 0.6;
                double newX = (currentIndex - selectedIndex) * 50;
                if (newX < 0) { newX -= 30; }
                else if (newX > 0) { newX += 30; }

                var button = CreateBaseButton(icon, scale);
                Buttons.Children.Add(button);
                (button.RenderTransform as CompositeTransform).TranslateX = newX;


                currentIndex++;
            }
        }

        private void ShiftButtons(ShootModeInfo info, string targetShootMode)
        {
            if (info == null) { return; }

            var selectedIndex = FindCandidateIndex(info?.ShootModeCapability, targetShootMode);
            int currentIndex = 0;

            foreach (var i in info.ShootModeCapability.Candidates)
            {
                BitmapImage icon = null;
                if (info.Icons != null && info.Icons.ContainsKey(i))
                {
                    icon = info.Icons[i];
                }

                var scale = i == targetShootMode ? 1.0 : 0.6;
                double newX = (currentIndex - selectedIndex) * 50;
                if (newX < 0) { newX -= 30; }
                else if (newX > 0) { newX += 30; }

                AnimationHelper.CreateMoveAndResizeAnimation(new MoveAndResizeAnimation()
                {
                    Target = Buttons.Children[currentIndex] as EllipseButton,
                    Duration = TimeSpan.FromMilliseconds(300),
                    newX = newX,
                    newScaleX = scale,
                    newScaleY = scale,
                }).Begin();

                currentIndex++;
            }
        }

        int FindCandidateIndex<T>(Capability<T> capability, T target)
        {
            if (capability == null) { return 0; }

            int i = 0;
            foreach (var candidate in capability.Candidates)
            {
                if (target.Equals(candidate)) { return i; }
                i++;
            }
            return 0;
        }

        bool IsUpdated(Capability<string> last, Capability<string> current)
        {
            if (last == null || current == null || last.Candidates == null || current.Candidates == null)
            {
                return true;
            }

            if (last.Candidates.Count != current.Candidates.Count) { return true; }

            for (int i = 0; i < last.Candidates.Count; i++)
            {
                if (last.Candidates[i] != current.Candidates[i]) { return true; }
            }

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
            foreach (var e in Buttons.Children)
            {
                if (e.Equals(sender))
                {
                    if (this._ModeInfo == null || this._ModeInfo.ShootModeCapability == null || this._ModeInfo.ShootModeCapability.Candidates.Count <= i) { return; }

                    if (this._ModeInfo.ShootModeCapability.Candidates[i] == this._ModeInfo.ShootModeCapability.Current)
                    {
                        // when the center button is pressed:
                        this._ModeInfo.ButtonPressed?.Invoke();
                    }
                    else
                    {
                        // When one of other buttons is selected
                        var requestMode = this._ModeInfo.ShootModeCapability.Candidates[i];
                        this.ShiftButtons(this._ModeInfo, requestMode);
                        this._ModeInfo.ModeSelected?.Invoke(requestMode);
                        RequestingMode = requestMode;
                    }
                }
                i++;
            }
        }

        private void Grid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ShiftButtons(this._ModeInfo, this._ModeInfo?.ShootModeCapability?.Current);
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
