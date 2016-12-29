using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Locana.Controls
{
    public sealed partial class TickableSlider : UserControl
    {
        public TickableSlider()
        {
            this.InitializeComponent();
            Slider.AddHandler(PointerReleasedEvent, new PointerEventHandler(Slider_PointerReleased), true);
            ToolTipTimer.Tick += TooltipTimer_Tick;
            ToolTipTimer.Interval = TimeSpan.FromSeconds(1);
            ToolTipFlyout.Hide();
        }

        private void TooltipTimer_Tick(object sender, object e)
        {
            ToolTipFlyout.Hide();
            ToolTipTimer.Stop();
        }

        public delegate void TickableSliderValueChangedEventHandler(object sender, TickableSliderValueChangedArgs e);
        public event TickableSliderValueChangedEventHandler ValueFixed;
        public event TickableSliderValueChangedEventHandler ValueChanged;


        private void Slider_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            var selected = (int)Math.Round((sender as Slider).Value);
            ReflectNewValue(sender as Slider, selected);
            Debug.WriteLine("Released");
            ToolTipFlyout.Hide();
        }

        private void ReflectNewValue(Slider slider, int selected)
        {
            slider.Value = selected;
            ValueFixed?.Invoke(this, new TickableSliderValueChangedArgs() { NewValue = selected });
            Debug.WriteLine("Save new value: " + selected);
        }
        
        public object Header
        {
            get { return (object)GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }

        public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register(
            nameof(Header),
            typeof(object),
            typeof(TickableSlider),
            new PropertyMetadata(null, new PropertyChangedCallback(OnHeaderChanged)));

        private static void OnHeaderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as TickableSlider).Header = e.NewValue;
        }

        public DataTemplate HeaderTemplate
        {
            get { return (DataTemplate)GetValue(HeaderTemplateProperty); }
            set { SetValue(HeaderTemplateProperty, value); }
        }

        public static readonly DependencyProperty HeaderTemplateProperty = DependencyProperty.Register(
            nameof(HeaderTemplate),
            typeof(DataTemplate),
            typeof(TickableSlider),
            new PropertyMetadata(null, new PropertyChangedCallback(OnHeaderTemplateChanged)));

        private static void OnHeaderTemplateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as TickableSlider).HeaderTemplate = (DataTemplate)e.NewValue;
        }

        public double IntermediateValue
        {
            get { return (double)GetValue(IntermediateValueProperty); }
            set { SetValue(IntermediateValueProperty, value); }
        }

        public static readonly DependencyProperty IntermediateValueProperty = DependencyProperty.Register(
            nameof(IntermediateValue),
            typeof(double),
            typeof(TickableSlider),
            new PropertyMetadata(0, new PropertyChangedCallback(OnIntermediateValueChanged)));

        private static void OnIntermediateValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as TickableSlider).IntermediateValue = (double)e.NewValue;
        }

        public bool IsDirectionReversed
        {
            get { return (bool)GetValue(IsDirectionReversedProperty); }
            set { SetValue(IsDirectionReversedProperty, value); }
        }

        public static readonly DependencyProperty IsDirectionReversedProperty = DependencyProperty.Register(
            nameof(IsDirectionReversed),
            typeof(bool),
            typeof(TickableSlider),
            new PropertyMetadata(false, new PropertyChangedCallback(OnIsDirectionReversedChanged)));

        private static void OnIsDirectionReversedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as TickableSlider).IsDirectionReversed = (bool)e.NewValue;
        }

        public Orientation Orientation
        {
            get { return (Orientation)GetValue(OrientationProperty); }
            set { SetValue(OrientationProperty, value); }
        }

        public static readonly DependencyProperty OrientationProperty = DependencyProperty.Register(
            nameof(Orientation),
            typeof(Orientation),
            typeof(TickableSlider),
            new PropertyMetadata(Orientation.Horizontal, new PropertyChangedCallback(OnOrientationChanged)));

        private static void OnOrientationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as TickableSlider).Orientation = (Orientation)e.NewValue;
        }

        public SliderSnapsTo SnapsTo
        {
            get { return (SliderSnapsTo)GetValue(SnapsToProperty); }
            set { SetValue(SnapsToProperty, value); }
        }

        public static readonly DependencyProperty SnapsToProperty = DependencyProperty.Register(
            nameof(SnapsTo),
            typeof(SliderSnapsTo),
            typeof(TickableSlider),
            new PropertyMetadata(SliderSnapsTo.Ticks, new PropertyChangedCallback(OnSnapsToChanged)));

        private static void OnSnapsToChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as TickableSlider).SnapsTo = (SliderSnapsTo)e.NewValue;
        }

        public double StepFrequency
        {
            get { return (double)GetValue(StepFrequencyProperty); }
            set { SetValue(StepFrequencyProperty, value); }
        }

        public static readonly DependencyProperty StepFrequencyProperty = DependencyProperty.Register(
            nameof(StepFrequency),
            typeof(double),
            typeof(TickableSlider),
            new PropertyMetadata(0, new PropertyChangedCallback(OnStepFrequencyChanged)));

        private static void OnStepFrequencyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as TickableSlider).StepFrequency = (double)e.NewValue;
        }


        public string ToolTipText
        {
            get { return (string)GetValue(ToolTipTextProperty); }
            set { SetValue(ToolTipTextProperty, value); }
        }

        public static readonly DependencyProperty ToolTipTextProperty = DependencyProperty.Register(
            nameof(ToolTipText),
            typeof(string),
            typeof(TickableSlider),
            new PropertyMetadata("tooltip", new PropertyChangedCallback(OnToolTipTextChanged)));

        private static void OnToolTipTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as TickableSlider).ToolTipText = (string)e.NewValue;
        }


        public IValueConverter ThumbToolTipValueConverter
        {
            get { return (IValueConverter)GetValue(ThumbToolTipValueConverterProperty); }
            set { SetValue(ThumbToolTipValueConverterProperty, value); }
        }

        public static readonly DependencyProperty ThumbToolTipValueConverterProperty = DependencyProperty.Register(
            nameof(ThumbToolTipValueConverter),
            typeof(IValueConverter),
            typeof(TickableSlider),
            new PropertyMetadata(null, new PropertyChangedCallback(OnThumbToolTipValueConverterChanged)));

        private static void OnThumbToolTipValueConverterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as TickableSlider).ThumbToolTipValueConverter = (IValueConverter)e.NewValue;
        }

        public double Maximum
        {
            get { return (double)GetValue(MaximumProperty); }
            set { SetValue(MaximumProperty, value); }
        }

        public static readonly DependencyProperty MaximumProperty = DependencyProperty.Register(
            nameof(Maximum),
            typeof(double),
            typeof(TickableSlider),
            new PropertyMetadata(0, new PropertyChangedCallback(OnMaximumChanged)));

        private static void OnMaximumChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as TickableSlider).Maximum = (double)e.NewValue;
        }

        public double Minimum
        {
            get { return (double)GetValue(MinimumProperty); }
            set { SetValue(MinimumProperty, value); }
        }

        public static readonly DependencyProperty MinimumProperty = DependencyProperty.Register(
            nameof(Minimum),
            typeof(double),
            typeof(TickableSlider),
            new PropertyMetadata(0, new PropertyChangedCallback(OnMinimumChanged)));

        private static void OnMinimumChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as TickableSlider).Minimum = (double)e.NewValue;
        }



        public TickPlacement TickPlacement
        {
            get { return (TickPlacement)GetValue(TickPlacementProperty); }
            set { SetValue(TickPlacementProperty, value); }
        }

        public static readonly DependencyProperty TickPlacementProperty = DependencyProperty.Register(
            nameof(TickPlacement),
            typeof(TickPlacement),
            typeof(TickableSlider),
            new PropertyMetadata(TickPlacement.TopLeft, new PropertyChangedCallback(OnTickPlacementChanged)));

        private static void OnTickPlacementChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as TickableSlider).TickPlacement = (TickPlacement)e.NewValue;
        }

        public double TickFrequency
        {
            get { return (double)GetValue(TickFrequencyProperty); }
            set { SetValue(TickFrequencyProperty, value); }
        }

        public static readonly DependencyProperty TickFrequencyProperty = DependencyProperty.Register(
            nameof(TickFrequency),
            typeof(double),
            typeof(TickableSlider),
            new PropertyMetadata(0, new PropertyChangedCallback(OnTickFrequencyChanged)));

        private static void OnTickFrequencyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as TickableSlider).TickFrequency = (double)e.NewValue;
        }

        public double Value
        {
            get { return (double)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            nameof(Value),
            typeof(double),
            typeof(TickableSlider),
            new PropertyMetadata(0, new PropertyChangedCallback(OnValueChanged)));

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as TickableSlider).Value = (double)e.NewValue;
        }

        DispatcherTimer ToolTipTimer = new DispatcherTimer();

        public new event ManipulationStartedEventHandler ManipulationStarted;
        private void Slider_ManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            ShowToolTip();
            ManipulationStarted?.Invoke(this, e);
        }

        private void Slider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            ShowToolTip();
            ValueChanged?.Invoke(this, new TickableSliderValueChangedArgs() { NewValue = (int)Math.Round(e.NewValue) });
        }

        public new event ManipulationCompletedEventHandler ManipulationCompleted;
        private void Slider_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            ManipulationCompleted?.Invoke(this, e);
        }

        public new event ManipulationDeltaEventHandler ManipulationDelta;
        private void Slider_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            ManipulationDelta?.Invoke(this, e);
        }

        public new event ManipulationInertiaStartingEventHandler ManipulationInertiaStarting;
        private void Slider_ManipulationInertiaStarting(object sender, ManipulationInertiaStartingRoutedEventArgs e)
        {
            ShowToolTip();
            ManipulationInertiaStarting?.Invoke(this, e);
        }

        public new event ManipulationStartingEventHandler ManipulationStarting;
        private void Slider_ManipulationStarting(object sender, ManipulationStartingRoutedEventArgs e)
        {
            ShowToolTip();
            ManipulationStarting?.Invoke(this, e);
        }

        /// <summary>
        /// Tick slider by given amount.
        /// </summary>
        /// <param name="amount">Positive value moves slider right.</param>
        /// <returns>true if succeed</returns>
        public bool TickSlider(int amount)
        {
            if ((amount < 0 && Slider.Value == Slider.Minimum) || (amount > 0 && Slider.Value == Slider.Maximum))
            {
                Debug.WriteLine("Do nothing.");
                return false;
            }

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

            Debug.WriteLine("Value: " + Slider.Value);
            ValueChanged?.Invoke(this, new TickableSliderValueChangedArgs() { NewValue = (int)Math.Round(Slider.Value) });

            ShowToolTip();
            CloseToolTipByTimer();

            return true;
        }

        private void ShowToolTip()
        {
            var text = (string)ThumbToolTipValueConverter?.Convert(Slider.Value, typeof(string), null, null);
            MyToolTip.Text = text ?? ((int)Math.Round(Slider.Value)).ToString();

            var xOffset = (Slider.ActualWidth / (Slider.Maximum - Slider.Minimum) * (Slider.Value - Slider.Minimum)) + 10;
            double yOffset = -5;

            ToolTipFlyout.FlyoutPresenterStyle = BuildToolTipStyle(xOffset, yOffset);
            ToolTipFlyout.ShowAt(FlyoutAnchor);
        }

        private static Style BuildToolTipStyle(double xOffset, double yOffset)
        {
            var flyoutStyle = new Style()
            {
                TargetType = typeof(FlyoutPresenter),
            };
            flyoutStyle.Setters.Add(new Setter(FlyoutPresenter.MarginProperty, new Thickness(xOffset, yOffset, 0, 0)));
            flyoutStyle.Setters.Add(new Setter(FlyoutPresenter.MinWidthProperty, 0));
            flyoutStyle.Setters.Add(new Setter(FlyoutPresenter.MaxWidthProperty, Double.NaN));
            flyoutStyle.Setters.Add(new Setter(FlyoutPresenter.MinHeightProperty, 0));
            flyoutStyle.Setters.Add(new Setter(FlyoutPresenter.MaxHeightProperty, Double.NaN));
            flyoutStyle.Setters.Add(new Setter(FlyoutPresenter.PaddingProperty, new Thickness(0)));
            return flyoutStyle;
        }

        private async void CloseToolTipByTimer()
        {
            if (ToolTipTimer.IsEnabled) { ToolTipTimer.Stop(); }
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                ToolTipTimer.Start();
            });
        }

        public void FixNewValue()
        {
            ReflectNewValue(Slider, (int)Math.Round(Slider.Value));
        }
    }

    public class TickableSliderValueChangedArgs : RoutedEventArgs
    {
        public int NewValue { get; set; }
    }
}
