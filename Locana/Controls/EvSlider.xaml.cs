using Kazyx.RemoteApi.Camera;
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
    public sealed partial class EvSlider : UserControl
    {
        public EvSlider()
        {
            this.InitializeComponent();
            Slider.AddHandler(PointerReleasedEvent, new PointerEventHandler(Slider_PointerReleased), true);
        }


        public event EventHandler<EvChangedEventArgs> SliderOperated;
        private void Slider_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            var selected = (int)Math.Round((sender as Slider).Value);
            (sender as Slider).Value = selected;
            //DebugUtil.Log("Slider released: " + selected);
            if (Parameter == null || selected < Parameter.Candidate.MinIndex || selected > Parameter.Candidate.MaxIndex) { return; }
            if (SliderOperated != null) { SliderOperated(this, new EvChangedEventArgs() { Selected = selected }); }
        }

        public EvCapability Parameter
        {
            set
            {
                SetValue(ParameterProperty, value);
                UpdateDisplay(value);
            }
            get { return (EvCapability)GetValue(ParameterProperty); }
        }

        public static readonly DependencyProperty ParameterProperty = DependencyProperty.Register(
            "Parameter",
            typeof(EvCapability),
            typeof(EvSlider),
            new PropertyMetadata(null, new PropertyChangedCallback(EvSlider.ParameterUpdated)));

        private static void ParameterUpdated(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != null)
            {
                //DebugUtil.Log("Parameter updated: " + (e.NewValue as EvCapability).CurrentIndex);
                (d as EvSlider).UpdateDisplay(e.NewValue as EvCapability);
            }
        }

        public ImageSource IconSource { set { SettingImage.Source = value; } }

        void UpdateDisplay(EvCapability parameter)
        {
            if (parameter == null || parameter.Candidate == null) { return; }

            Slider.Minimum = parameter.Candidate.MinIndex;
            Slider.Maximum = parameter.Candidate.MaxIndex;
            if (parameter.CurrentIndex < parameter.Candidate.MinIndex || parameter.CurrentIndex > parameter.Candidate.MaxIndex) { return; }
            Slider.Value = parameter.CurrentIndex;

            var max = EvConverter.GetEv(parameter.Candidate.MaxIndex, parameter.Candidate.IndexStep);
            MaxLabel.Text = Math.Round(max, 1, MidpointRounding.AwayFromZero).ToString("0.0");

            var min = EvConverter.GetEv(parameter.Candidate.MinIndex, parameter.Candidate.IndexStep);
            MinLabel.Text = Math.Round(min, 1, MidpointRounding.AwayFromZero).ToString("0.0");

            var center = EvConverter.GetEv(0, parameter.Candidate.IndexStep);
            CenterLabel.Text = Math.Round(center, 1, MidpointRounding.AwayFromZero).ToString("0.0");

            var labels = new Dictionary<int, string>();
            double unit = parameter.Candidate.IndexStep == EvStepDefinition.EV_1_3 ? 1.0 / 3.0 : 0.5;
            for (int i = parameter.Candidate.MinIndex; i <= parameter.Candidate.MaxIndex; i++)
            {
                var value = EvConverter.GetEv(i, parameter.Candidate.IndexStep);
                var strValue = Math.Round(value, 1, MidpointRounding.AwayFromZero).ToString("0.0");

                if (value < 0) { strValue = "EV " + strValue; }
                else if (value == 0.0f) { strValue = "EV " + strValue; }
                else { strValue = "EV +" + strValue; }
                labels.Add(i, strValue);
            }

            Slider.ThumbToolTipValueConverter = new EvValueConverter() { Labels = labels };
        }
    }

    public class EvValueConverter : IValueConverter
    {
        public Dictionary<int, string> Labels { get; set; }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var selected = (int)Math.Round((double)value);

            if (Labels == null || !Labels.ContainsKey(selected)) { return value.ToString(); }

            return Labels[selected];
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class EvChangedEventArgs
    {
        public int Selected { get; set; }
    }
}
