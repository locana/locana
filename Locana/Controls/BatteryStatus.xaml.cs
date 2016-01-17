using Kazyx.RemoteApi.Camera;
using Locana.Utility;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Locana.Controls
{
    public sealed partial class BatteryStatus : UserControl
    {
        public BatteryStatus()
        {
            this.InitializeComponent();
        }
        const double MAX_AMOUNT_WIDTH = 410.0 / 600.0;
        const double AMOUNT_OFFSET = 65.0 / 600.0;
        const double BACKGROUND_OFFSET = 62.0 / 600.0;

        public List<BatteryInfo> BatteryInfo
        {
            set
            {
                SetValue(BatteryInfoProperty, value);
                UpdateBatteryLevelDisplay(value);
            }
            get { return (List<BatteryInfo>)GetValue(BatteryInfoProperty); }
        }

        public object RemoteApi { get; private set; }

        public static readonly DependencyProperty BatteryInfoProperty = DependencyProperty.Register(
            nameof(BatteryInfo),
            typeof(List<BatteryInfo>),
            typeof(BatteryStatus),
            new PropertyMetadata(null, new PropertyChangedCallback(BatteryStatus.BatteryInfoUpdated)));

        private static void BatteryInfoUpdated(DependencyObject d, DependencyPropertyChangedEventArgs e) { }

        private void UpdateBatteryLevelDisplay(List<BatteryInfo> info)
        {
            if (info == null || FindFirstActiveBattery(info) == null)
            {
                LayoutRoot.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                return;
            }
            else
            {
                LayoutRoot.Visibility = Windows.UI.Xaml.Visibility.Visible;
            }

            var batt = FindFirstActiveBattery(info);

            double w = LayoutRoot.ActualWidth;
            double offset = w * AMOUNT_OFFSET;
            double bg_offset = w * BACKGROUND_OFFSET;

            Amount.Margin = new Thickness(offset);

            if (batt.LevelDenominator > 0)
            {
                Amount.Visibility = Windows.UI.Xaml.Visibility.Visible;
                double level = (double)batt.LevelNumerator / (double)batt.LevelDenominator;
                Amount.Width = level * MAX_AMOUNT_WIDTH * w;
            }
            else
            {
                Amount.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            }

            Camera.Visibility = Visibility.Collapsed;
            NearEnd.Visibility = Visibility.Collapsed;
            Charging.Visibility = Visibility.Collapsed;

            switch (batt.AdditionalStatus)
            {
                case Kazyx.RemoteApi.Camera.BatteryStatus.NearEnd:
                    Amount.Fill = ResourceManager.SystemControlForegroundAccentBrush;
                    NearEnd.Visibility = Visibility.Visible;
                    break;
                case Kazyx.RemoteApi.Camera.BatteryStatus.Charging:
                    Amount.Fill = ResourceManager.ForegroundBrush;
                    Charging.Visibility = Visibility.Visible;
                    break;
                default:
                    Camera.Visibility = Visibility.Visible;
                    Amount.Fill = ResourceManager.ForegroundBrush;
                    break;
            }
        }

        BatteryInfo FindFirstActiveBattery(List<BatteryInfo> batteries)
        {
            foreach (var b in batteries)
            {
                switch (b.Status)
                {
                    case Kazyx.RemoteApi.Camera.BatteryStatus.Active:
                        return b;
                }
            }
            return null;
        }

        private void LayoutRoot_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateBatteryLevelDisplay((List<BatteryInfo>)GetValue(BatteryInfoProperty));
        }
    }
}
