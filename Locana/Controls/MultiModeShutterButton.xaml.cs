using Kazyx.RemoteApi;
using Locana.Utility;
using System;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
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

        public BitmapImage CurrentModeButtonImage
        {
            set
            {
                SetValue(CurrentModeButtonImageProperty, value);
            }
            get
            {
                return (BitmapImage)GetValue(CurrentModeButtonImageProperty);
            }
        }

        public static readonly DependencyProperty CurrentModeButtonImageProperty = DependencyProperty.Register(
            nameof(CurrentModeButtonImage),
            typeof(BitmapImage),
            typeof(MultiModeShutterButton),
            new PropertyMetadata(null, new PropertyChangedCallback(MultiModeShutterButton.CurrentModeButtonImageUpdated)));

        private static void CurrentModeButtonImageUpdated(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue == null) { return; }
            var image = e.NewValue as BitmapImage;
            (d as MultiModeShutterButton).UpdateCurrentModeButtonImage(image);
        }

        public DataTemplate CurrentModeButtonTemplate { get; set; }

        public static readonly DependencyProperty CurrentModeButtonTemplateProperty = DependencyProperty.Register(
            nameof(CurrentModeButtonTemplate),
            typeof(DataTemplate),
            typeof(MultiModeShutterButton),
            new PropertyMetadata(null, new PropertyChangedCallback(MultiModeShutterButton.CurrentModeButtonTemplateUpdated)));

        private static void CurrentModeButtonTemplateUpdated(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue == null) { return; }

            var icon = e.NewValue as DataTemplate;
            (d as MultiModeShutterButton).UpdateCurrentModeButtonImage(icon);
        }

        public bool ShutterButtonEnabled
        {
            set { SetValue(ShutterButtonEnabledProperty, value); }
            get { return (bool)GetValue(ShutterButtonEnabledProperty); }
        }

        public static readonly DependencyProperty ShutterButtonEnabledProperty = DependencyProperty.Register(
            nameof(ShutterButtonEnabled),
            typeof(bool),
            typeof(MultiModeShutterButton),
            new PropertyMetadata(false, new PropertyChangedCallback(MultiModeShutterButton.ShutterButtonEnabledUpdated)));

        private static void ShutterButtonEnabledUpdated(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue == null) { return; }
            (d as MultiModeShutterButton).SetCurrentModeButtonEnable((bool)e.NewValue);
        }

        public bool ModeButtonsEnabled
        {
            set { SetValue(ModeButtonsEnabledProperty, value); }
            get { return (bool)GetValue(ModeButtonsEnabledProperty); }
        }

        public static readonly DependencyProperty ModeButtonsEnabledProperty = DependencyProperty.Register(
            nameof(ModeButtonsEnabled),
            typeof(bool),
            typeof(MultiModeShutterButton),
            new PropertyMetadata(false, new PropertyChangedCallback(MultiModeShutterButton.ModeButtonsEnabledUpdated)));

        private static void ModeButtonsEnabledUpdated(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue == null) { return; }
            (d as MultiModeShutterButton).SetModeButtonsEnable((bool)e.NewValue);
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
            }
            else
            {
                ProgressRing.IsActive = true;
            }
        }

        private void SetModeButtonsEnable(bool enable)
        {

            if (this._ModeInfo?.ShootModeCapability == null) { return; }
            var SelectedModeButton = FindButton(this._ModeInfo.ShootModeCapability.Current);

            foreach (EllipseButton b in this.Buttons.Children)
            {
                // set enable other than selected button
                if (b != null && !b.Equals(SelectedModeButton)) { b.Enabled = enable; }
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

                var scale = i == targetShootMode ? 1.0 : 0.6;
                double newX = (currentIndex - selectedIndex) * 50;
                if (newX < 0) { newX -= 30; }
                else if (newX > 0) { newX += 30; }


                EllipseButton button = null;

                if (selectedIndex == currentIndex)
                {
                    if (this.CurrentModeButtonTemplate != null)
                    {
                        button = CreateBaseButton(this.CurrentModeButtonTemplate, scale);
                    }
                    else {
                        button = CreateBaseButton(this.CurrentModeButtonImage, scale);
                    }
                }
                else
                {
                    if (info.IconTemplates != null && info.IconTemplates.ContainsKey(i))
                    {
                        button = CreateBaseButton(info.IconTemplates[i], scale);
                    }
                    if (info.IconTemplates == null && info.Icons != null && info.Icons.ContainsKey(i))
                    {
                        button = CreateBaseButton(info.Icons[i], scale);
                    }
                }

                if (button != null)
                {
                    Buttons.Children.Add(button);
                    (button.RenderTransform as CompositeTransform).TranslateX = newX;
                }

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
                if (info.IconTemplates != null && info.IconTemplates.ContainsKey(i))
                {
                    (Buttons.Children[currentIndex] as EllipseButton).IconTemplate = info.IconTemplates[i];
                }
                if (info.IconTemplates == null && info.Icons != null && info.Icons.ContainsKey(i))
                {
                    (Buttons.Children[currentIndex] as EllipseButton).Icon = info.Icons[i];
                }

                var scale = i == targetShootMode ? 1.0 : 0.6;
                double newX = (currentIndex - selectedIndex) * 50;
                if (newX < 0) { newX -= 30; }
                else if (newX > 0) { newX += 30; }

                var story = AnimationHelper.CreateMoveAndResizeAnimation(new MoveAndResizeAnimation()
                {
                    Target = Buttons.Children[currentIndex] as EllipseButton,
                    Duration = TimeSpan.FromMilliseconds(300),
                    newX = newX,
                    newScaleX = scale,
                    newScaleY = scale,
                });
                if (currentIndex == 0)
                {
                    story.Completed += Story_Completed;
                }
                story.Begin();

                currentIndex++;
            }
        }

        private void Story_Completed(object sender, object e)
        {
            // Refresh button state when animation is completed.
            (sender as Storyboard).Completed -= Story_Completed;
            SetModeButtonsEnable(ModeButtonsEnabled);
        }

        void UpdateCurrentModeButtonImage<T>(T newIcon)
        {
            if (this._ModeInfo?.ShootModeCapability == null) { return; }
            var button = FindButton(this._ModeInfo.ShootModeCapability.Current);
            if (button != null)
            {
                if (typeof(T) == typeof(BitmapImage))
                {
                    button.Icon = newIcon as BitmapImage;
                }
                else if (typeof(T) == typeof(DataTemplate))
                {
                    button.IconTemplate = newIcon as DataTemplate;
                }
            }
        }

        void SetCurrentModeButtonEnable(bool enable)
        {
            if (this._ModeInfo?.ShootModeCapability == null) { return; }
            var button = FindButton(this._ModeInfo.ShootModeCapability.Current);
            if (button != null)
            {
                button.Enabled = enable;
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

        EllipseButton CreateBaseButton<T>(T icon, double scale)
        {
            var button = new EllipseButton()
            {
                Width = BUTTON_SIZE,
                Height = BUTTON_SIZE,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Bottom,
                Margin = new Thickness(0),
                // Icon = icon,
                RenderTransform = new CompositeTransform()
                {
                    TranslateX = 0,
                    TranslateY = 0,
                    ScaleX = scale,
                    ScaleY = scale,
                    CenterX = BUTTON_SIZE / 2,
                    CenterY = BUTTON_SIZE,
                },
            };
            button.Clicked += ElementTapped;

            if (typeof(T) == typeof(BitmapImage))
            {
                button.Icon = icon as BitmapImage;
            }
            else if (typeof(T) == typeof(DataTemplate))
            {
                button.IconTemplate = icon as DataTemplate;
            }

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
        public Dictionary<string, DataTemplate> IconTemplates { get; set; }
        public Action<string> ModeSelected { get; set; }
        public Action ButtonPressed { get; set; }
    }
}
