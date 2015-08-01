using System.Diagnostics;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

// ユーザー コントロールのアイテム テンプレートについては、http://go.microsoft.com/fwlink/?LinkId=234236 を参照してください

namespace Kazyx.Uwpmm.Control
{
    public sealed partial class ZoomBar : UserControl
    {
        public int CurrentBoxIndex { get; set; }
        public static readonly DependencyProperty CurrentBoxProperty = DependencyProperty.Register(
        "CurrentBoxIndex",
        typeof(int),
        typeof(ZoomBar),
        new PropertyMetadata(0, new PropertyChangedCallback(ZoomBar.OnCurrentBoxUpdated)));

        private static void OnCurrentBoxUpdated(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as ZoomBar).CurrentBoxIndex = (int)e.NewValue;
            (d as ZoomBar).UpdateCursor();
            (d as ZoomBar).UpdateBackground();
        }

        public int TotalBoxNum { get; set; }
        public static readonly DependencyProperty TotalBoxNumProperty = DependencyProperty.Register(
            "TotalBoxNum",
            typeof(int),
            typeof(ZoomBar),
            new PropertyMetadata(0, new PropertyChangedCallback(ZoomBar.OnTotalBoxNumUpdated)));

        private static void OnTotalBoxNumUpdated(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as ZoomBar).TotalBoxNum = (int)e.NewValue;
            (d as ZoomBar).UpdateCursor();
            (d as ZoomBar).UpdateBackground();
        }

        public int PositionInCurrentBox { get; set; }
        public static readonly DependencyProperty PositionInCurrentBoxProperty = DependencyProperty.Register(
            "PositionInCurrentBox",
            typeof(int),
            typeof(ZoomBar),
            new PropertyMetadata(0, new PropertyChangedCallback(ZoomBar.OnPositionInCurrentBoxUpdated)));

        private static void OnPositionInCurrentBoxUpdated(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as ZoomBar).PositionInCurrentBox = (int)e.NewValue;
            (d as ZoomBar).UpdateCursor();
        }

        private const double padding = 0.5;
        private const double topPadding = 0.5;

        public ZoomBar()
        {
            InitializeComponent();
        }

        private void UpdateCursor()
        {
            var leftPadding = this.Background.StrokeThickness + padding;
            var w = (this.Background.ActualWidth - (leftPadding * 2)) / this.TotalBoxNum;
            var offset = leftPadding + w * this.CurrentBoxIndex;
            Cursor.Margin = new Thickness(offset + ((double)PositionInCurrentBox / 100) * (w - Cursor.ActualWidth), this.Background.StrokeThickness + topPadding, 0, 0);
        }

        private void UpdateBackground()
        {
            this.SeparatorLines.Children.Clear();
            for (int i = 1; i < this.TotalBoxNum; i++)
            {
                SeparatorLines.Children.Add(new Line()
                {
                    Stroke = (Brush)Application.Current.Resources["ApplicationForegroundThemeBrush"],
                    StrokeThickness = 1,
                    X1 = (this.Background.ActualWidth / this.TotalBoxNum) * i,
                    X2 = (this.Background.ActualWidth / this.TotalBoxNum) * i,
                    Y1 = 0,
                    Y2 = this.Background.ActualHeight,
                });
            }
        }
    }
}
