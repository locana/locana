using System;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

// ユーザー コントロールのアイテム テンプレートについては、http://go.microsoft.com/fwlink/?LinkId=234236 を参照してください

namespace Kazyx.Uwpmm.Control
{
    public sealed partial class Histogram : UserControl
    {
        public Histogram()
        {
            this.InitializeComponent();
        }

        public enum ColorType
        {
            Red,
            Green,
            Blue,
            White,
        }

        private int MaxFrequency = 0;
        private double ScaleFactor;
        private double HorizontalResolution;
        private double MaxHistogramLevel;

        public void Init(ColorType type, int maxLevel)
        {
            InitColorBar(type);
            InitBars(maxLevel);
        }

        private void InitBars(int maxFrequency)
        {
            MaxFrequency = maxFrequency;
            ScaleFactor = BarsGrid.ActualHeight / (double)maxFrequency * 6;
            HorizontalResolution = BarsGrid.ActualWidth / X_SKIP_ORDER;
            MaxHistogramLevel = BarsGrid.ActualHeight - HISTOGRAM_PADDING_TOP;
            // DebugUtil.Log("Freq: " + MaxFrequency + " maxLevel: " + MaxHistogramLevel);
        }

        private void InitColorBar(ColorType type)
        {
            var colorBarBrush = new SolidColorBrush();

            switch (type)
            {
                case ColorType.Red:
                    colorBarBrush.Color = Color.FromArgb(255, 255, 0, 0);
                    break;
                case ColorType.Green:
                    colorBarBrush.Color = Color.FromArgb(255, 0, 255, 0);
                    break;
                case ColorType.Blue:
                    colorBarBrush.Color = Color.FromArgb(255, 0, 0, 255);
                    break;
                case ColorType.White:
                    colorBarBrush.Color = Color.FromArgb(255, 160, 160, 160);
                    break;
                default:
                    colorBarBrush.Color = Color.FromArgb(255, 0, 0, 0);
                    break;
            }
            ColorBar.Fill = colorBarBrush;
        }

        private const int X_SKIP_ORDER = 2;
        private const int HISTOGRAM_PADDING_TOP = 2;

        public void SetHistogramValue(int[] values)
        {
            if (values == null)
            {
                return;
            }

            var rate = (int)(values.Length / BarsGrid.ActualWidth * X_SKIP_ORDER);

            var points = new PointCollection();

            // Left corner
            points.Add(new Point(0.0, BarsGrid.ActualHeight));

            for (int i = 0; i < BarsGrid.ActualWidth / X_SKIP_ORDER; i++)
            {
                var index = rate * i;
                if (index > values.Length - 1)
                {
                    index = values.Length - 1;
                }
                var barHeight = ScaleFactor * values[index];
                points.Add(new Point(i * X_SKIP_ORDER, BarsGrid.ActualHeight - Math.Min(BarsGrid.ActualHeight, barHeight)));
            }

            // Right corner
            points.Add(new Point(BarsGrid.ActualWidth, BarsGrid.ActualHeight));

            HistogramPolygon.Points = points;
        }

        public void SetHistogramValue(int[] valuesR, int[] valuesG, int[] valuesB)
        {
            if (valuesR == null || valuesG == null || valuesB == null)
            {
                return;
            }

            if (!(valuesR.Length == valuesG.Length && valuesG.Length == valuesB.Length) || valuesR.Length <= 0)
            {
                return;
            }

            var rate = (int)(valuesR.Length / BarsGrid.ActualWidth * X_SKIP_ORDER);

            var pointsR = new PointCollection();
            var pointsG = new PointCollection();
            var pointsB = new PointCollection();

            for (int i = 0; i < HorizontalResolution; i++)
            {
                var index = rate * i + X_SKIP_ORDER;
                if (index < 0) { return; }
                if (index > valuesR.Length - 1)
                {
                    index = valuesR.Length - 1;
                }

                var x = i * X_SKIP_ORDER;

                pointsR.Add(new Point(x, BarsGrid.ActualHeight - Math.Min(MaxHistogramLevel, ScaleFactor * valuesR[index])));
                pointsG.Add(new Point(x, BarsGrid.ActualHeight - Math.Min(MaxHistogramLevel, ScaleFactor * valuesG[index])));
                pointsB.Add(new Point(x, BarsGrid.ActualHeight - Math.Min(MaxHistogramLevel, ScaleFactor * valuesB[index])));
            }

            HistogramPolylineR.Points = pointsR;
            HistogramPolylineG.Points = pointsG;
            HistogramPolylineB.Points = pointsB;

        }

        private void Rectangle_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            InitBars(MaxFrequency);
        }
    }
}
