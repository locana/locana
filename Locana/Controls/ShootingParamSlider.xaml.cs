using Kazyx.RemoteApi;
using Locana.Utility;
using System;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Locana.Controls
{
    public sealed partial class ShootingParamSlider : UserControl
    {
        public ShootingParamSlider()
        {
            InitializeComponent();
            Slider.ValueFixed += Slider_ValueFixed;
        }

        private void Slider_ValueFixed(object sender, TickableSliderValueChangedArgs e)
        {
            var selected = e.NewValue;
            if (Parameter == null || selected < 0 || selected >= Parameter.Candidates.Count) { return; }
            SliderOperated?.Invoke(this, new ShootingParameterChangedEventArgs() { Selected = Parameter.Candidates[selected] });
        }
        public event EventHandler<ShootingParameterChangedEventArgs> SliderOperated;

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

        public string TooltipPrefix
        {
            get { return (string)GetValue(TooltipPrefixProperty); }
            set { SetValue(TooltipPrefixProperty, value); }
        }

        public static readonly DependencyProperty TooltipPrefixProperty = DependencyProperty.Register(
            nameof(TooltipPrefix),
            typeof(string),
            typeof(ShootingParamSlider),
            new PropertyMetadata("", new PropertyChangedCallback(OnTooltipPrefixChanged)));

        private static void OnTooltipPrefixChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as ShootingParamSlider).TooltipPrefix = (string)e.NewValue;
        }

        public string TooltipPostfix
        {
            get { return (string)GetValue(TooltipPostfixProperty); }
            set { SetValue(TooltipPostfixProperty, value); }
        }

        public static readonly DependencyProperty TooltipPostfixProperty = DependencyProperty.Register(
            nameof(TooltipPostfix),
            typeof(string),
            typeof(ShootingParamSlider),
            new PropertyMetadata("", new PropertyChangedCallback(OnTooltipPostfixChanged)));

        private static void OnTooltipPostfixChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as ShootingParamSlider).TooltipPostfix = (string)e.NewValue;
        }

        void UpdateDisplay<T>(Capability<T> parameter)
        {
            if (parameter == null || parameter.Candidates == null || parameter.Candidates.Count == 0) { return; }

            Slider.Minimum = 0;
            Slider.Maximum = parameter.Candidates.Count - 1;
            DebugUtil.Log(() => { return "Min: " + Slider.Minimum + " Max: " + Slider.Maximum; });
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
            Slider.ThumbToolTipValueConverter = new SliderValueConverter() { Labels = labels, Prefix = this.TooltipPrefix, Postfix = this.TooltipPostfix };
        }

        public void TickSlider(int amount)
        {
            Slider.TickSlider(amount);
        }

        public void FixShootingParam()
        {
            Slider.FixNewValue();
        }
    }

    public class SliderValueConverter : IValueConverter
    {
        public List<string> Labels { get; set; }
        public string Prefix { get; set; }
        public string Postfix { get; set; }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var selected = (int)Math.Round((double)value);

            if (Labels == null || selected >= Labels.Count) { return value.ToString(); }
            
            return Prefix + Labels[selected].TrimEnd('"') + Postfix;
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
