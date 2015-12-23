using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.Resources;
using Windows.UI.Core;

namespace Kazyx.Uwpmm.Utility
{
    public class SystemUtil
    {
        public static string GetStringResource(string key)
        {
            return ResourceLoader.GetForCurrentView().GetString(key);
        }

        public static CoreDispatcher GetCurrentDispatcher()
        {
            return CoreApplication.MainView?.CoreWindow?.Dispatcher;
        }
    }
}
