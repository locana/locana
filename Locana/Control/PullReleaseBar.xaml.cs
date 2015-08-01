using Kazyx.Uwpmm.Utility;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Kazyx.Uwpmm.Control
{
    public sealed partial class PullReleaseBar : UserControl
    {
        public PullReleaseBar()
        {
            this.InitializeComponent();
            InitialCursorMargin = new Thickness(0, 0, 0, 0);
            InitialLabelMargin = new Thickness(0, 0, 0, 0);
            CurrentValue = 0.0;
            if (Unit == null)
            {
                Unit = "";
            }
        }

        public static readonly DependencyProperty MaxProperty = DependencyProperty.Register(
            "Max",
            typeof(int),
            typeof(PullReleaseBar),
            new PropertyMetadata(new PropertyChangedCallback(PullReleaseBar.OnMaxValueChanged)));

        public static readonly DependencyProperty MinProperty = DependencyProperty.Register(
            "Min",
            typeof(int),
            typeof(PullReleaseBar),
            new PropertyMetadata(new PropertyChangedCallback(PullReleaseBar.OnMinValueChanged)));

        public delegate void OnReleaseHandler(object sender, OnReleaseArgs e);
        public event OnReleaseHandler OnRelease;

        public int Max { get; set; }
        public int Min { get; set; }
        public string Unit { get; set; }

        private static void OnMaxValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // DebugUtil.Log("max updated: " + (int)e.NewValue);
            (d as PullReleaseBar).Max = (int)e.NewValue;
        }

        private static void OnMinValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // DebugUtil.Log("min updated: " + (int)e.NewValue);
            (d as PullReleaseBar).Min = (int)e.NewValue;
        }

        // public Action<int> OnRelease { get; set; }

        private double CurrentValue;
        private Thickness InitialCursorMargin;
        private Thickness InitialLabelMargin;

        private void LayoutRoot_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.NewSize.Width != 0)
            {
                InitialCursorMargin = new Thickness(LayoutRoot.ActualWidth / 2 - Cursor.ActualWidth / 2, 0, 0, 0);
                InitialLabelMargin = new Thickness(LayoutRoot.ActualWidth / 2 - CurrentValueText.Width / 2, 0, 0, 0);
                //DebugUtil.Log("initial X: " + (LayoutRoot.ActualWidth / 2 - Cursor.ActualWidth / 2));
                //DebugUtil.Log("initial label X: " + (LayoutRoot.ActualWidth / 2 - CurrentValueText.ActualWidth / 2));
                Cursor.Margin = InitialCursorMargin;
                CurrentValueText.Margin = InitialLabelMargin;
                DynamicBar.Y2 = DynamicBar.Y1 = LayoutRoot.ActualHeight / 2;
                DynamicBar.X2 = DynamicBar.X1 = LayoutRoot.ActualWidth / 2;
                //DebugUtil.Log("Max: " + Max + " Min: " + Min);
            }
        }

        private void TouchArea_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            var accm = e.Cumulative;
            //DebugUtil.Log("accm: " + accm.Translation.X);
            var vel = e.Velocities;
            var margin = 10;
            var transX = Math.Max(-(LayoutRoot.ActualWidth / 2 - margin), accm.Translation.X);
            //DebugUtil.Log("pull " + LayoutRoot.ActualWidth + " " + accm.Translation.X +" "+transX);
            transX = Math.Min(LayoutRoot.ActualWidth / 2 - margin, transX);

            Cursor.Margin = new Thickness(InitialCursorMargin.Left + transX, 0, 0, 0); ;
            CurrentValueText.Margin = new Thickness(InitialLabelMargin.Left + transX, 0, 0, 0);
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
        }

        private void TouchArea_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            Cursor.Margin = InitialCursorMargin;
            CurrentValueText.Margin = InitialLabelMargin;
            DynamicBar.X2 = DynamicBar.X1;
            CurrentValueText.Text = "";

            if (OnRelease != null)
            {
                OnRelease(this, new OnReleaseArgs() { Value = (int)CurrentValue });
            }
        }
    }

    public class OnReleaseArgs : EventArgs
    {
        public int Value { get; set; }
    }
}
