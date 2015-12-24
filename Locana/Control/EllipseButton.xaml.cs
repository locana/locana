using Kazyx.Uwpmm.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Locana.Control
{
    public sealed partial class EllipseButton : UserControl
    {
        public EllipseButton()
        {
            this.InitializeComponent();
        }

        public BitmapImage Icon { set { this._Icon.Source = value; } }
        public Action<object> Tapped { get; set; }

        private void LayoutRoot_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (Tapped != null) { Tapped(this); }
        }

        private void LayoutRoot_ManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            DebugUtil.Log("Started");
        }

        private void LayoutRoot_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
        }

        private void LayoutRoot_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            _Ellipse.Fill = ResourceManager.SystemControlForegroundAccentBrush;
            DebugUtil.Log("entered");
        }

        private void LayoutRoot_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            _Ellipse.Fill = ResourceManager.ForegroundBrush;

        }
    }
}
