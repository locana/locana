using Kazyx.Uwpmm.Utility;
using System;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Kazyx.Uwpmm.Control
{
    public sealed partial class FramingGridsSurface : UserControl
    {
        private SolidColorBrush _Stroke = new SolidColorBrush() { Color = Color.FromArgb(200, 200, 200, 200) };
        public SolidColorBrush Stroke
        {
            get { return _Stroke; }
            set
            {
                if (!value.Equals(_Stroke))
                {
                    _Stroke = value;
                    // DebugUtil.Log("Stroke updated: " + _Stroke.Color.R + " " + _Stroke.Color.G + " " + _Stroke.Color.B);
                    this.DrawGridLines(_Type);
                }
                else
                {
                    // DebugUtil.Log("skip stroke value updating");
                }
            }
        }

        private double _StrokeThickness = 1;
        public double StrokeThickness
        {
            get { return _StrokeThickness; }
            set
            {
                if (value != _StrokeThickness)
                {
                    _StrokeThickness = value;
                    this.DrawGridLines(_Type);
                }
            }
        }

        private FramingGridTypes _Type = FramingGridTypes.Off;
        public FramingGridTypes Type
        {
            get { return _Type; }
            set
            {
                if (value != _Type)
                {
                    _Type = value;
                    this.DrawGridLines(value);
                }
            }
        }

        private const double GoldenRatio = 0.382;

        public static readonly DependencyProperty GridTypeProperty = DependencyProperty.Register(
            "Type",
            typeof(FramingGridTypes),
            typeof(FramingGridsSurface),
            new PropertyMetadata("", new PropertyChangedCallback(FramingGridsSurface.OnGridTypeChanged)));

        public static void OnGridTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // DebugUtil.Log("[FramingGridsSurface]Type changed: " + (string)e.NewValue);
            (d as FramingGridsSurface).Type = (FramingGridTypes)e.NewValue;
        }

        public static readonly DependencyProperty StrokeProperty = DependencyProperty.Register(
            "Stroke",
            typeof(SolidColorBrush),
            typeof(FramingGridsSurface),
            new PropertyMetadata(null, new PropertyChangedCallback(FramingGridsSurface.OnStrokeChanged)));

        public static void OnStrokeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // DebugUtil.Log("[FramingGridsSurface]Stroke changed: " + (e.NewValue as SolidColorBrush).Color.G.ToString());
            (d as FramingGridsSurface).Stroke = (SolidColorBrush)e.NewValue;
        }

        public static readonly DependencyProperty StrokeThicknessProperty = DependencyProperty.Register(
            "StrokeThickness",
            typeof(double),
            typeof(FramingGridsSurface),
            new PropertyMetadata(0, new PropertyChangedCallback(FramingGridsSurface.OnStrokeThicknessChanged)));

        private static void OnStrokeThicknessChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // DebugUtil.Log("[FramingGridsSurface]Stroke thickness changed: " + e.NewValue);
            (d as FramingGridsSurface).StrokeThickness = (double)e.NewValue;
        }

        private FibonacciLineOrigins _FibonacciOrigin = FibonacciLineOrigins.UpperLeft;
        public FibonacciLineOrigins FibonacciOrigin
        {
            get { return _FibonacciOrigin; }
            set
            {
                if (value != _FibonacciOrigin)
                {
                    this._FibonacciOrigin = value;
                    this.DrawGridLines(_Type);
                }
            }
        }

        public static readonly DependencyProperty FibonacciOriginProperty = DependencyProperty.Register(
            "FibonacciOrigin",
            typeof(FibonacciLineOrigins),
            typeof(FramingGridsSurface),
            new PropertyMetadata("", new PropertyChangedCallback(FramingGridsSurface.OnFibonacciOriginChanged)));

        private static void OnFibonacciOriginChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // DebugUtil.Log("fibonacci origin changed: " + e.NewValue);
            (d as FramingGridsSurface).FibonacciOrigin = (FibonacciLineOrigins)e.NewValue;
        }


        public FramingGridsSurface()
        {
            this.InitializeComponent();
        }

        private void Clear()
        {
            Lines.Children.Clear();
        }

        private void DrawGridLines(FramingGridTypes t)
        {
            double w = LayoutRoot.ActualWidth;
            double h = LayoutRoot.ActualHeight;

            this.Clear();

            switch (t)
            {
                case FramingGridTypes.RuleOfThirds:
                    DrawLine(w / 3, w / 3, 0, h);
                    DrawLine(2 * w / 3, 2 * w / 3, 0, h);
                    DrawLine(0, w, h / 3, h / 3);
                    DrawLine(0, w, 2 * h / 3, 2 * h / 3);
                    break;
                case FramingGridTypes.Crosshairs:
                    DrawLine(w / 2, w / 2, 0, h);
                    DrawLine(0, w, h / 2, h / 2);
                    break;
                case FramingGridTypes.Diagonal:
                    DrawLine(0, w, 0, h);
                    DrawLine(0, w, h, 0);
                    break;
                case FramingGridTypes.Fibonacci:
                    if (w > h)
                    {
                        switch (FibonacciOrigin)
                        {
                            case FibonacciLineOrigins.UpperLeft:
                                DrawFibonacciSpiral(new Point(0, 0), w, h);
                                break;
                            case FibonacciLineOrigins.UpperRight:
                                DrawFibonacciSpiral(new Point(w, 0), w, h);
                                break;
                            case FibonacciLineOrigins.BottomLeft:
                                DrawFibonacciSpiral(new Point(0, h), w, h);
                                break;
                            case FibonacciLineOrigins.BottomRight:
                                DrawFibonacciSpiral(new Point(w, h), w, h);
                                break;
                        }
                    }
                    break;
                case FramingGridTypes.GoldenRatio:
                    DrawLine(w * GoldenRatio, w * GoldenRatio, 0, h);
                    DrawLine(w * (1 - GoldenRatio), w * (1 - GoldenRatio), 0, h);
                    DrawLine(0, w, h * GoldenRatio, h * GoldenRatio);
                    DrawLine(0, w, h * (1 - GoldenRatio), h * (1 - GoldenRatio));
                    break;
                case FramingGridTypes.Square:
                    if (w > h)
                    {
                        // only vertical lines.
                        DrawLine((w - h) / 2, (w - h) / 2, 0, h);
                        DrawLine(w - ((w - h) / 2), w - ((w - h) / 2), 0, h);
                    }
                    else if (h > w)
                    {
                        // horizontal lines
                        DrawLine((h - w) / 2, (h - w) / 2, 0, w);
                        DrawLine(h - ((h - w) / 2), h - ((h - w) / 2), 0, w);
                    }
                    break;
                case FramingGridTypes.Off:
                default:
                    break;
            }
        }

        private void DrawFibonacciSpiral(Point StartPoint, double w, double h)
        {
            // DebugUtil.Log("draw fibonaci: " + w + " " + h);

            PathFigure figure = new PathFigure();
            figure.StartPoint = StartPoint;

            var FullWidth = w;
            var FullHeight = h;

            var HorizontallyReversed = false;
            var VerticallyReversed = false;

            if (StartPoint.X == w)
            {
                HorizontallyReversed = true;
            }
            else if (StartPoint.X != 0)
            {
                DebugUtil.Log("Error: start point must be at corner");
                return;
            }

            if (StartPoint.Y == h)
            {
                VerticallyReversed = true;
            }
            else if (StartPoint.Y != 0)
            {
                DebugUtil.Log("Error: start point must be at corner");
                return;
            }

            // first control point
            var x1 = 0.0;
            var y1 = 0.0;

            // second contorl point
            var x2 = 0.0;
            var y2 = h;

            // end of line
            var x3 = w * (1 - GoldenRatio);
            var y3 = h;

            for (int i = 0; i < 10; i++)
            {
                // DebugUtil.Log("Bezier: " + x1 + " " + y1 + " / " + x2 + " " + y2 + " / " + x3 + " " + y3);
                var seg = new BezierSegment();
                var tempX1 = x1;
                var tempY1 = y1;
                var tempX2 = x2;
                var tempY2 = y2;
                var tempX3 = x3;
                var tempY3 = y3;

                if (HorizontallyReversed)
                {
                    tempX1 = FullWidth - tempX1;
                    tempX2 = FullWidth - tempX2;
                    tempX3 = FullWidth - tempX3;
                }

                if (VerticallyReversed)
                {
                    tempY1 = FullHeight - tempY1;
                    tempY2 = FullHeight - tempY2;
                    tempY3 = FullHeight - tempY3;
                }

                seg.Point1 = new Point(tempX1, tempY1);
                seg.Point2 = new Point(tempX2, tempY2);
                seg.Point3 = new Point(tempX3, tempY3);

                figure.Segments.Add(seg);

                x1 = x3;
                y1 = y3;

                switch (i % 4)
                {
                    case 0: // lower right
                        w = w * GoldenRatio;
                        x2 = x1 + w;
                        y2 = y1;
                        x3 = x1 + w;
                        y3 = y1 - h * (1 - GoldenRatio);
                        break;
                    case 1:
                        h = h * GoldenRatio;
                        x2 = x1;
                        y2 = y1 - h;
                        x3 = x1 - w * (1 - GoldenRatio);
                        y3 = y1 - h;
                        break;
                    case 2:
                        w = w * GoldenRatio;
                        x2 = x1 - w;
                        y2 = y1;
                        x3 = x1 - w;
                        y3 = y1 + h * (1 - GoldenRatio);
                        break;
                    case 3:
                        h = h * GoldenRatio;
                        x2 = x1;
                        y2 = y1 + h;
                        x3 = x1 + w * (1 - GoldenRatio);
                        y3 = y1 + h;
                        break;
                }

            }

            PathFigureCollection pthFigureCollection = new PathFigureCollection();
            pthFigureCollection.Add(figure);

            PathGeometry pthGeometry = new PathGeometry();
            pthGeometry.Figures = pthFigureCollection;

            Windows.UI.Xaml.Shapes.Path Fibonacci = new Windows.UI.Xaml.Shapes.Path();
            Fibonacci.Stroke = Stroke = this.Stroke;
            Fibonacci.StrokeThickness = this.StrokeThickness;
            Fibonacci.Data = pthGeometry;
            Lines.Children.Add(Fibonacci);
        }

        private void DrawLine(double x1, double x2, double y1, double y2)
        {
            // DebugUtil.Log("draw line: " + x1 + " " + x2 + " " + y1 + " " + y2);

            double minX = StrokeThickness / 2;
            double maxX = LayoutRoot.ActualWidth - minX;
            double minY = StrokeThickness / 2;
            double maxY = LayoutRoot.ActualHeight - minY;

            x1 = RoundToRange(x1, minX, maxX);
            x2 = RoundToRange(x2, minX, maxX);
            y1 = RoundToRange(y1, minY, maxY);
            y2 = RoundToRange(y2, minY, maxY);

            var line = new Line()
            {
                Stroke = this.Stroke,
                StrokeThickness = this.StrokeThickness,
                X1 = x1,
                X2 = x2,
                Y1 = y1,
                Y2 = y2,

            };
            Lines.Children.Add(line);
        }

        private void DrawArcSegment(Point start, Point end, SweepDirection dir)
        {
            // DebugUtil.Log("draw arc: " + start.X + " " + start.Y + " " + end.X + " " + end.Y + " " + dir);

            PathFigure pthFigure = new PathFigure();
            pthFigure.StartPoint = start;

            ArcSegment arcSeg = new ArcSegment();
            arcSeg.Point = end;
            arcSeg.Size = new Size(Math.Abs(start.X - end.X), Math.Abs(start.Y - end.Y));
            arcSeg.IsLargeArc = false;
            arcSeg.SweepDirection = dir;
            arcSeg.RotationAngle = 90;

            PathSegmentCollection myPathSegmentCollection = new PathSegmentCollection();
            myPathSegmentCollection.Add(arcSeg);

            pthFigure.Segments = myPathSegmentCollection;

            PathFigureCollection pthFigureCollection = new PathFigureCollection();
            pthFigureCollection.Add(pthFigure);

            PathGeometry pthGeometry = new PathGeometry();
            pthGeometry.Figures = pthFigureCollection;

            Windows.UI.Xaml.Shapes.Path arcPath = new Windows.UI.Xaml.Shapes.Path();
            arcPath.Stroke = this.Stroke;
            arcPath.StrokeThickness = this.StrokeThickness;
            arcPath.Data = pthGeometry;
            Lines.Children.Add(arcPath);
        }

        private static double RoundToRange(double value, double min, double max)
        {
            if (value < min) { return min; }
            if (value > max) { return max; }
            return value;
        }

        private void Lines_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.Clear();
            DrawGridLines(_Type);
        }
    }

    public enum FramingGridTypes
    {
        Off,
        RuleOfThirds,
        Diagonal,
        GoldenRatio,
        Crosshairs,
        Square,
        Fibonacci,
    }

    public enum FramingGridColors
    {
        White,
        Black,
        Red,
        Blue,
        Green,
    }

    public enum FibonacciLineOrigins
    {
        UpperLeft,
        UpperRight,
        BottomLeft,
        BottomRight,
    }
}
