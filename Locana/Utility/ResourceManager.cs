using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace Kazyx.Uwpmm.Utility
{
    public static class ResourceManager
    {
        public static Brush AccentColorBrush
        {
            get { return (Brush)Application.Current.Resources["AccentColorBrush"]; }
        }

        public static Brush ForegroundBrush
        {
            get { return (Brush)Application.Current.Resources["ApplicationForegroundThemeBrush"]; }
        }
    }
}
