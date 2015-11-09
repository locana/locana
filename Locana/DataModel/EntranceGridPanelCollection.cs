using Kazyx.Uwpmm.CameraControl;
using Kazyx.Uwpmm.Utility;
using Locana.Pages;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Locana.DataModel
{
    public class EntrancePanelGroupCollection : List<EntrancePanelGroup>, INotifyPropertyChanged, INotifyCollectionChanged
    {
        public event NotifyCollectionChangedEventHandler CollectionChanged;
        public event PropertyChangedEventHandler PropertyChanged;

        new public void Add(EntrancePanelGroup group)
        {
            base.Add(group);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, group, Count - 1));
        }

        private void OnPropertyChanged(string name)
        {
            PropertyChanged.Raise(this, new PropertyChangedEventArgs(name));
        }

        private void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged("Count");
            OnPropertyChanged("Item[]");
            try
            {
                CollectionChanged.Raise(this, e);
            }
            catch (NotSupportedException)
            {
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
        }
    }

    public class EntrancePanelGroup : List<EntrancePanel>, INotifyPropertyChanged, INotifyCollectionChanged
    {
        public event NotifyCollectionChangedEventHandler CollectionChanged;
        public event PropertyChangedEventHandler PropertyChanged;

        public EntrancePanelGroup(string name)
        {
            GroupKey = name;
        }

        public string GroupKey { private set; get; }

        new public void Add(EntrancePanel panel)
        {
            base.Add(panel);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, panel, Count - 1));
        }

        private void OnPropertyChanged(string name)
        {
            PropertyChanged.Raise(this, new PropertyChangedEventArgs(name));
        }

        private void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged("Count");
            OnPropertyChanged("Item[]");
            try
            {
                CollectionChanged.Raise(this, e);
            }
            catch (NotSupportedException)
            {
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
        }
    }

    public class EntrancePanel
    {
        public EntrancePanel(string name, Action onClick)
        {
            PanelTitle = name;
            OnClick = onClick;
        }

        public string PanelTitle { private set; get; }

        public Action OnClick { private set; get; }
    }

    public class DevicePanel : EntrancePanel
    {
        public DevicePanel(TargetDevice device)
            : base(device.FriendlyName, () =>
             {
                 var frame = Window.Current.Content as Frame;
                 frame.Navigate(typeof(MainPage), device);
             })
        {

        }
    }

}
