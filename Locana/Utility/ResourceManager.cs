using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace Locana.Utility
{
    public static class ResourceManager
    {
        public static Brush SystemControlForegroundAccentBrush
        {
            get { return (Brush)Application.Current.Resources["SystemControlForegroundAccentBrush"]; }
        }

        public static Brush BackGroundBrush
        {
            get { return (Brush)Application.Current.Resources["ApplicationPageBackgroundThemeBrush"]; }
        }

        public static Brush ForegroundBrush
        {
            get { return (Brush)Application.Current.Resources["ApplicationForegroundThemeBrush"]; }
        }
    }
}
