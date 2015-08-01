using Kazyx.RemoteApi.Camera;
using Kazyx.Uwpmm.Utility;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Kazyx.Uwpmm.Control
{
    public sealed partial class ProgramShiftSlider : UserControl
    {
        public ProgramShiftSlider()
        {
            this.InitializeComponent();
        }

        public event EventHandler<ProgramShiftChangedEventArgs> SliderOperated;

        public ProgramShiftRange Parameter
        {
            set
            {
                SetValue(ParameterProperty, value);
                UpdateDisplay(value);
            }
            get { return (ProgramShiftRange)GetValue(ParameterProperty); }
        }

        public ImageSource IconSource { set { SettingImage.Source = value; } }

        private void UpdateDisplay(ProgramShiftRange value)
        {
            Bar.Max = value.Max;
            Bar.Min = value.Min;
            MaxLabel.Text = value.Max.ToString();
            MinLabel.Text = value.Min.ToString();
        }

        public static readonly DependencyProperty ParameterProperty = DependencyProperty.Register(
            "Parameter",
            typeof(ProgramShiftRange),
            typeof(ProgramShiftSlider),
            new PropertyMetadata(null, new PropertyChangedCallback(ProgramShiftSlider.ParameterUpdated)));

        private static void ParameterUpdated(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != null)
            {
                DebugUtil.Log("Parameter updated: " + (e.NewValue as EvCapability).CurrentIndex);
                (d as ProgramShiftSlider).UpdateDisplay(e.NewValue as ProgramShiftRange);
            }
        }

        private void Bar_OnRelease(object sender, OnReleaseArgs e)
        {
            //DebugUtil.Log("Slider released: " + selected);
            //if (Parameter == null || e.Value < Parameter.Min || e.Value > Parameter.Max) { return; }
            if (SliderOperated != null) { SliderOperated(this, new ProgramShiftChangedEventArgs() { OperatedStep = e.Value }); }
        }
    }

    public class ProgramShiftChangedEventArgs
    {
        public int OperatedStep { get; set; }
    }
}
