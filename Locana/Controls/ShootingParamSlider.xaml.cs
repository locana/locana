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


        public ShootingParamSlider()
        {
            InitializeComponent();

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
        
        public void TickSlider(int amount)
        {
            //
        }

        public void FixShootingParam()
        {
            //
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
