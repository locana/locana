using Kazyx.ImageStream.FocusInfo;
using Kazyx.Uwpmm.Utility;
using System;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Kazyx.Uwpmm.Control
{
    public sealed partial class FocusFrameSurface : UserControl
    {
        public FocusFrameSurface()
        {
            this.InitializeComponent();
            SelfDrawTouchAFFrame = false;
            Focused = false;
        }

        public void ClearFrames()
        {
            this.LayoutRoot.Children.Clear();
        }

        Brush FocusedBrush = ResourceManager.AccentColorBrush;
        Brush NormalBrush = (Brush)Application.Current.Resources["ApplicationPageBackgroundThemeBrush"];
        Brush MainBrush = (Brush)Application.Current.Resources["ApplicationForegroundThemeBrush"];
        Brush SubBrush = (Brush)Application.Current.Resources["ApplicationPageBackgroundThemeBrush"];
        private static readonly double FaceFrameStrokeThickness = 1.5;
        private static readonly double FrameStrokeThickness = 2.5;
        private static readonly double FrameOpacity = 0.7;

        public delegate void TouchFocusHandler(object sender, TouchFocusEventArgs e);
        public event TouchFocusHandler OnTouchFocusOperated;

        public bool SelfDrawTouchAFFrame { get; set; }
        public bool Focused
        {
            set
            {
                if (SelfDrawTouchAFFrame && SelfDrawnFrame != null)
                {
                    if (value) { SelfDrawnFrame.Stroke = FocusedBrush; }
                    else { this.ClearFrames(); }
                }
            }
        }

        Rectangle SelfDrawnFrame = null;

        public class TouchFocusEventArgs : EventArgs
        {
            public double X { get; set; }
            public double Y { get; set; }
            public TouchFocusEventArgs(double x, double y)
            {
                this.X = x;
                this.Y = y;
            }
        }

        Rectangle AddFrame(double X1, double X2, double Y1, double Y2, Brush StrokeBrush, double StrokeThickness, bool DottedBorder = false)
        {
            // DebugUtil.Log("[FocusFrames] " + X1 + " " + X2 + " " + Y1 + " " + Y2);
            var r = new Rectangle()
            {
                VerticalAlignment = Windows.UI.Xaml.VerticalAlignment.Top,
                HorizontalAlignment = Windows.UI.Xaml.HorizontalAlignment.Left,
                Width = X2 - X1,
                Height = Y2 - Y1,
                Margin = new Thickness(X1, Y1, 0, 0),
                Stroke = StrokeBrush,
                Opacity = FrameOpacity,
                StrokeThickness = StrokeThickness,
            };

            if (DottedBorder)
            {
                var d = new DoubleCollection();
                d.Add(4);
                r.StrokeDashArray = d;
            }

            this.LayoutRoot.Children.Add(r);
            return r;
        }

        public void SetFocusFrames(List<FocusFrameInfo> frames)
        {
            this.ClearFrames();

            foreach (FocusFrameInfo info in frames)
            {
                double x1 = (double)info.TopLeft_X / 10000 * LayoutRoot.ActualWidth;
                double x2 = (double)info.BottomRight_X / 10000 * LayoutRoot.ActualWidth;
                double y1 = (double)info.TopLeft_Y / 10000 * LayoutRoot.ActualHeight;
                double y2 = (double)info.BottomRight_Y / 10000 * LayoutRoot.ActualHeight;

                switch (info.Category)
                {
                    case Category.ContrastAF:
                    case Category.Tracking:
                    case Category.PhaseDetectionAF:

                        switch (info.Status)
                        {
                            case Status.Focused:
                                this.AddFrame(x1, x2, y1, y2, FocusedBrush, FrameStrokeThickness, info.AdditionalStatus == AdditionalStatus.LargeFrame);
                                break;
                            case Status.Normal:
                                this.AddFrame(x1, x2, y1, y2, NormalBrush, FrameStrokeThickness, info.AdditionalStatus == AdditionalStatus.LargeFrame);
                                break;
                        }
                        break;
                    case Category.Face:
                        switch (info.Status)
                        {
                            case Status.Focused:
                                this.AddFrame(x1, x2, y1, y2, FocusedBrush, FaceFrameStrokeThickness);
                                break;
                            case Status.Main:
                                this.AddFrame(x1, x2, y1, y2, MainBrush, FaceFrameStrokeThickness);
                                break;
                            case Status.Sub:
                                this.AddFrame(x1, x2, y1, y2, SubBrush, FaceFrameStrokeThickness);
                                break;
                        }
                        break;
                }
            }
        }

        private void LayoutRoot_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            if (OnTouchFocusOperated != null)
            {
                OnTouchFocusOperated(this, new TouchFocusEventArgs(
                    e.GetPosition(this).X / this.ActualWidth * 100.0,
                    e.GetPosition(this).Y / this.ActualHeight * 100.0
                    ));
            }

            if (SelfDrawTouchAFFrame)
            {
                this.ClearFrames();
                this.SelfDrawnFrame = this.AddFrame(e.GetPosition(this).X - 15, e.GetPosition(this).X + 15, e.GetPosition(this).Y - 15, e.GetPosition(this).Y + 15, NormalBrush, FrameStrokeThickness);
            }
        }
    }
}
