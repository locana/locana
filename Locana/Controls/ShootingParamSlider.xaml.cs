using Kazyx.RemoteApi;
using Locana.Utility;
using System;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Locana.Controls
{
    public sealed partial class ShootingParamSlider : UserControl
    {
        private SliderValueConverter ToolTipConverter = null;
        DispatcherTimer ToolTipTimer = new DispatcherTimer();

        public ShootingParamSlider()
        {
            InitializeComponent();
            Slider.AddHandler(PointerReleasedEvent, new PointerEventHandler(Slider_PointerReleased), true);

            ToolTipTimer.Tick += CloserTooltipTimer_Tick;
            ToolTipTimer.Interval = TimeSpan.FromSeconds(3);
        }

        private void CloserTooltipTimer_Tick(object sender, object e)
        {
            MyToolTipFrame.Visibility = Visibility.Collapsed;
            ToolTipTimer.Stop();
        }

        public event EventHandler<ShootingParameterChangedEventArgs> SliderOperated;
        private void Slider_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            var selected = (int)Math.Round((sender as Slider).Value);
            ReflectNewValue(sender as Slider, selected);
            MyToolTipFrame.Visibility = Visibility.Collapsed;
        }

        private void ReflectNewValue(Slider slider, int selected)
        {
            slider.Value = selected;
            DebugUtil.Log(() => "Slider released: " + selected);
            if (Parameter == null || selected < 0 || selected >= Parameter.Candidates.Count) { return; }
            SliderOperated?.Invoke(this, new ShootingParameterChangedEventArgs() { Selected = Parameter.Candidates[selected] });
        }

        public Capability<string> Parameter
        {
            set
            {
                SetValue(ParameterProperty, value);
                UpdateDisplay(value);
            }
            get { return (Capability<string>)GetValue(ParameterProperty); }
        }

        public static readonly DependencyProperty ParameterProperty = DependencyProperty.Register(
            nameof(Parameter),
            typeof(Capability<string>),
            typeof(ShootingParamSlider),
            new PropertyMetadata(null, new PropertyChangedCallback(ShootingParamSlider.ParameterUpdated)));

        private static void ParameterUpdated(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != null)
            {
                DebugUtil.Log(() => "Parameter updated: " + (e.NewValue as Capability<string>).Current);
                (d as ShootingParamSlider).UpdateDisplay<string>(e.NewValue as Capability<string>);
            }
        }

        public ImageSource IconSource { set { SettingImage.Source = value; } }
        public DataTemplate IconDataTemplate { set { SettingImageContent.ContentTemplate = value; } }

        void UpdateDisplay<T>(Capability<T> parameter)
        {
            if (parameter == null || parameter.Candidates == null || parameter.Candidates.Count == 0) { return; }

            Slider.Minimum = 0;
            Slider.Maximum = parameter.Candidates.Count - 1;
            for (int i = 0; i < parameter.Candidates.Count; i++)
            {
                if (parameter.Current.Equals(parameter.Candidates[i]))
                {
                    Slider.Value = i;
                }
            }
            MinLabel.Text = parameter.Candidates[0].ToString();
            MaxLabel.Text = parameter.Candidates[parameter.Candidates.Count - 1].ToString();

            var labels = new List<string>();
            foreach (var value in parameter.Candidates)
            {
                labels.Add(value.ToString());
            }
            ToolTipConverter = new SliderValueConverter() { Labels = labels };
        }

        /// <summary>
        /// Tick slider by given amount.
        /// </summary>
        /// <param name="amount">Positive value moves slider right.</param>
        /// <returns>true if succeed</returns>
        public bool TickSlider(int amount)
        {
            if ((amount < 0 && Slider.Value == Slider.Minimum) || (amount > 0 && Slider.Value == Slider.Maximum)) { return false; }

            if (Slider.Value + amount > Slider.Maximum)
            {
                Slider.Value = Slider.Maximum;
            }
            else if (Slider.Value + amount < Slider.Minimum)
            {
                Slider.Value = Slider.Minimum;
            }
            else
            {
                Slider.Value = (int)Math.Round(Slider.Value) + amount;
            }

            ShowToolTip(true);

            return true;
        }

        private void ShowToolTip(bool CloseByTimer = false)
        {
            if (Slider == null || ToolTipConverter == null) { return; }
            var text = (string)ToolTipConverter.Convert(Slider.Value, typeof(string), null, null);

            MyToolTip.Text = text;
            var xOffset = (Slider.ActualWidth / Slider.Maximum * Slider.Value) + 10;
            double yOffset = 0;
            MyToolTipFrame.Margin = new Thickness(xOffset, yOffset, 0, 0);
            MyToolTipFrame.Visibility = Visibility.Visible;

            if (CloseByTimer)
            {
                if (ToolTipTimer.IsEnabled) { ToolTipTimer.Stop(); }
                ToolTipTimer.Start();
            }
        }

        public void FixShootingParam()
        {
            ReflectNewValue(Slider, (int)Math.Round(Slider.Value));
        }

        private void Slider_ManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            ShowToolTip();
        }

        private void Slider_ValueChanged(object sender, Windows.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        {
            ShowToolTip();
        }
    }

    public class SliderValueConverter : IValueConverter
    {
        public List<string> Labels { get; set; }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var selected = (int)Math.Round((double)value);

            if (Labels == null || selected >= Labels.Count) { return value.ToString(); }

            return Labels[selected];
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class ShootingParameterChangedEventArgs
    {
        public string Selected { get; set; }
    }


}
