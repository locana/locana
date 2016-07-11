using Locana.Utility;
using System;
using Windows.UI.Xaml;

namespace Locana.DataModel
{
    public class AppSettingData<T> : ObservableBase
    {
        private string _Title = null;
        public string Title
        {
            set
            {
                _Title = value;
                NotifyChangedOnUI(nameof(Title));
            }
            get { return _Title; }
        }

        private string _Guide = null;
        public string Guide
        {
            set
            {
                _Guide = value;
                NotifyChangedOnUI(nameof(Guide));
                NotifyChangedOnUI(nameof(GuideVisibility));
            }
            get { return _Guide; }
        }

        private string[] _Candidates = null;
        public string[] Candidates
        {
            set
            {
                _Candidates = value;
                NotifyChangedOnUI(nameof(Candidates));
            }
            get { return _Candidates; }
        }

        public Visibility GuideVisibility
        {
            get { return Guide == null ? Visibility.Collapsed : Visibility.Visible; }
        }

        public Func<T> StateProvider;
        public Action<T> StateObserver;

        public T CurrentSetting
        {
            get
            {
                if (StateProvider != null) { return StateProvider.Invoke(); }
                else { return default(T); }
            }
            set
            {
                DebugUtil.Log(() => string.Format("{0} is changing from {1} to {2}", Title, CurrentSetting, value));
                StateObserver?.Invoke(value);
                NotifyChangedOnUI(nameof(CurrentSetting));
            }
        }

        private bool _IsActive = true;
        public bool IsActive
        {
            get
            {
                return _IsActive;
            }
            set
            {
                _IsActive = value;
                NotifyChangedOnUI(nameof(IsActive));
            }
        }

        private Visibility _SettingVisibility = Visibility.Visible;
        public Visibility SettingVisibility
        {
            get { return _SettingVisibility; }
            set
            {
                if (value != _SettingVisibility)
                {
                    this._SettingVisibility = value;
                    DebugUtil.Log(() => "visibility changed: " + _SettingVisibility);
                    NotifyChangedOnUI(nameof(SettingVisibility));
                }
            }
        }
    }
}
