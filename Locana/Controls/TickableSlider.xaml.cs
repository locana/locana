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
        }

        private void TooltipTimer_Tick(object sender, object e)
        {
            // MyToolTipFrame.Visibility = Visibility.Collapsed;
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
            // MyToolTipFrame.Visibility = Visibility.Collapsed;
            ToolTipFlyout.Hide();
        }

        private void ReflectNewValue(Slider slider, int selected)
        {
            slider.Value = selected;
            ValueFixed?.Invoke(this, new TickableSliderValueChangedArgs() { NewValue = selected });
            Debug.WriteLine("Save new value: " + selected);
        }

        public object Header { get; set; }
        public DataTemplate HeaderTemplate { get; set; }
        public double IntermediateValue { get; set; } = 0;
        public bool IsDirectionReversed { get; set; } = false;
        public Orientation Orientation { get; set; } = Orientation.Horizontal;
        public SliderSnapsTo SnapsTo { get; set; } = SliderSnapsTo.StepValues;
        public double StepFrequency { get; set; } = 1;

        public string ToolTipText { get; set; } = "hoge";

        public IValueConverter ThumbToolTipValueConverter { get; set; } // not to pass through to original Slider.

        public double Maximum { get; set; }
        public double Minimum { get; set; }
        public TickPlacement TickPlacement { get; set; }
        public double TickFrequency { get; set; }

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
