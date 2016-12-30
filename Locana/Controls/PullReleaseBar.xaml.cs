using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Locana.Controls
{
    public sealed partial class PullReleaseBar : UserControl
    {
        public PullReleaseBar()
        {
            this.InitializeComponent();
            InitialCursorMargin = new Thickness(0, 0, 0, 0);
            CurrentValue = 0.0;
            ToolTipFlyout.Hide();
        }
        public int Min
        {
            get { return (int)GetValue(MinProperty); }
            set { SetValue(MinProperty, value); }
        }

        public static readonly DependencyProperty MinProperty = DependencyProperty.Register(
            nameof(Min),
            typeof(int),
            typeof(PullReleaseBar),
            new PropertyMetadata(0, new PropertyChangedCallback(OnMinChanged)));

        private static void OnMinChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as PullReleaseBar).Min = (int)e.NewValue;
        }

        public int Max
        {
            get { return (int)GetValue(MaxProperty); }
            set { SetValue(MaxProperty, value); }
        }

        public static readonly DependencyProperty MaxProperty = DependencyProperty.Register(
            nameof(Max),
            typeof(int),
            typeof(PullReleaseBar),
            new PropertyMetadata(0, new PropertyChangedCallback(OnMaxChanged)));

        private static void OnMaxChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as PullReleaseBar).Max = (int)e.NewValue;
        }

        public string Unit
        {
            get { return (string)GetValue(UnitProperty); }
            set { SetValue(UnitProperty, value); }
        }

        public static readonly DependencyProperty UnitProperty = DependencyProperty.Register(
            nameof(Unit),
            typeof(string),
            typeof(PullReleaseBar),
            new PropertyMetadata("", new PropertyChangedCallback(OnUnitChanged)));

        private static void OnUnitChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as PullReleaseBar).Unit = (string)e.NewValue;
        }

        private static void OnMaxValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // DebugUtil.Log(() => "max updated: " + (int)e.NewValue);
            (d as PullReleaseBar).Max = (int)e.NewValue;
        }

        private static void OnMinValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // DebugUtil.Log(() => "min updated: " + (int)e.NewValue);
            (d as PullReleaseBar).Min = (int)e.NewValue;
        }

        public delegate void OnReleaseHandler(object sender, OnReleaseArgs e);
        public event OnReleaseHandler OnRelease;

        private double CurrentValue;
        private Thickness InitialCursorMargin;

        private void LayoutRoot_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.NewSize.Width != 0)
            {
                InitialCursorMargin = new Thickness(LayoutRoot.ActualWidth / 2 - Cursor.ActualWidth / 2, 0, 0, 0);
                //DebugUtil.Log(() => "initial X: " + (LayoutRoot.ActualWidth / 2 - Cursor.ActualWidth / 2));
                //DebugUtil.Log(() => "initial label X: " + (LayoutRoot.ActualWidth / 2 - CurrentValueText.ActualWidth / 2));
                Cursor.Margin = InitialCursorMargin;
                DynamicBar.Y2 = DynamicBar.Y1 = LayoutRoot.ActualHeight / 2;
                DynamicBar.X2 = DynamicBar.X1 = LayoutRoot.ActualWidth / 2;
                //DebugUtil.Log(() => "Max: " + Max + " Min: " + Min);
            }
        }

        double margin = 10;

        private void TouchArea_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            var accm = e.Cumulative;
            //DebugUtil.Log(() => "accm: " + accm.Translation.X);
            var vel = e.Velocities;
            var transX = Math.Max(-(LayoutRoot.ActualWidth / 2 - margin), accm.Translation.X);
            //DebugUtil.Log(() => "pull " + LayoutRoot.ActualWidth + " " + accm.Translation.X +" "+transX);
            transX = Math.Min(LayoutRoot.ActualWidth / 2 - margin, transX);

            ShiftCursor(transX);
        }

        private void ShiftCursor(double transX)
        {
            Cursor.Margin = new Thickness(InitialCursorMargin.Left + transX, 0, 0, 0); ;
            DynamicBar.X2 = LayoutRoot.ActualWidth / 2 + transX;

            var length = Math.Abs(DynamicBar.X2 - DynamicBar.X1);
            DynamicBar.Opacity = length / (LayoutRoot.ActualWidth / 2);

            var value = (DynamicBar.X2 - DynamicBar.X1) / (LayoutRoot.ActualWidth / 2);
            if (value > 0)
            {
                CurrentValue = Math.Min(Math.Truncate((Max + 1) * value), Max);
            }
            else
            {
                CurrentValue = Math.Max(Math.Truncate((Min - 1) * Math.Abs(value)), Min);
            }
            CurrentValueText.Text = CurrentValue.ToString() + " " + Unit;
            ShowToolTip(transX);
        }

        public bool TickSlider(int amount)
        {
            if (CurrentValue + amount > Max || CurrentValue + amount < Min) { return false; }
            var new_value = CurrentValue + amount;
            var transX = ((LayoutRoot.ActualWidth - margin * 2) / (Max - Min)) * new_value;

            ShiftCursor(transX);
            return true;
        }

        public void ReleaseSlider()
        {
            _ReleaseSlider();
        }

        private void TouchArea_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            _ReleaseSlider();
        }

        private void _ReleaseSlider()
        {
            Cursor.Margin = InitialCursorMargin;
            DynamicBar.X2 = DynamicBar.X1;
            CurrentValueText.Text = "";

            OnRelease?.Invoke(this, new OnReleaseArgs() { Value = (int)CurrentValue });

            CurrentValue = 0;
            ToolTipFlyout.Hide();
        }

        private void ShowToolTip(double transX)
        {
            var xOffset = transX + LayoutRoot.ActualWidth / 2;
            double yOffset = 0;

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

    }

    public class OnReleaseArgs : EventArgs
    {
        public int Value { get; set; }
    }
}
