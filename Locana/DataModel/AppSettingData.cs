using Kazyx.Uwpmm.Utility;
using System;
using Windows.UI.Xaml;

namespace Kazyx.Uwpmm.DataModel
{
    public class AppSettingData<T> : ObservableBase
    {
        public AppSettingData(string title, string guide, Func<T> StateChecker, Action<T> StateChanger, string[] candidates = null)
        {
            if (StateChecker == null || StateChanger == null)
            {
                throw new ArgumentNullException("StateChecker must not be null");
            }
            Title = title;
            Guide = guide;
            Candidates = candidates;
            this.StateChecker = StateChecker;
            this.StateChanger = StateChanger;
        }

        private string _Title = null;
        public string Title
        {
            set
            {
                _Title = value;
                NotifyChangedOnUI("Title");
            }
            get { return _Title; }
        }

        private string _Guide = null;
        public string Guide
        {
            set
            {
                _Guide = value;
                NotifyChangedOnUI("Guide");
                NotifyChangedOnUI("GuideVisibility");
            }
            get { return _Guide; }
        }

        private string[] _Candidates = null;
        public string[] Candidates
        {
            set
            {
                _Candidates = value;
                NotifyChangedOnUI("Candidates");
            }
            get { return _Candidates; }
        }

        public Visibility GuideVisibility
        {
            get { return Guide == null ? Visibility.Collapsed : Visibility.Visible; }
        }

        private readonly Func<T> StateChecker;
        private readonly Action<T> StateChanger;

        public T CurrentSetting
        {
            get { return StateChecker(); }
            set
            {
                StateChanger.Invoke(value);
                NotifyChangedOnUI("CurrentSetting");
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
                NotifyChangedOnUI("IsActive");
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
                    DebugUtil.Log("visibility changed: " + _SettingVisibility);
                    NotifyChangedOnUI("SettingVisibility");
                }
            }
        }
    }
}
