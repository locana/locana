using Kazyx.RemoteApi.Camera;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Kazyx.Uwpmm.Control
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
        }
    }

    public class EvChangedEventArgs
    {
        public int Selected { get; set; }
    }
}
