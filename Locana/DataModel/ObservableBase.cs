using Kazyx.Uwpmm.Utility;
using System;
using System.ComponentModel;
using Windows.UI.Core;

namespace Kazyx.Uwpmm.DataModel
{
    public abstract class ObservableBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected async void NotifyChangedOnUI(string name, CoreDispatcherPriority priority = CoreDispatcherPriority.Normal)
        {
            var dispatcher = SystemUtil.GetCurrentDispatcher();
            if (dispatcher == null) { return; }

            await dispatcher.RunAsync(priority, () =>
            {
                NotifyChanged(name);
            });
        }

        protected void NotifyChanged(string name)
        {
            PropertyChanged.Raise(this, new PropertyChangedEventArgs(name));
        }
    }
}
