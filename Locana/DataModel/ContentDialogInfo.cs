using Locana.Utility;

namespace Locana.DataModel
{
    public class ContentDialogInfo : ObservableBase
    {
        public string Title
        {
            get
            {
                if (_TitleId == null) { return ""; }
                else { return SystemUtil.GetStringResource(_TitleId); }
            }
        }

        private string _TitleId = null;
        public string TitleId
        {
            set
            {
                _TitleId = value;
                NotifyChangedOnUI(nameof(Title));
            }
        }

        public string Description
        {
            get
            {
                if (_DescriptionId == null) { return ""; }
                else { return SystemUtil.GetStringResource(_DescriptionId); }
            }
        }

        private string _DescriptionId = null;
        public string DescriptionId
        {
            set
            {
                _DescriptionId = value;
                NotifyChangedOnUI(nameof(Description));
            }
        }

        public string PrimaryText
        {
            get
            {
                if (_PrimaryTextId == null) { return ""; }
                else { return SystemUtil.GetStringResource(_PrimaryTextId); }
            }
        }

        private string _PrimaryTextId = null;
        public string PrimaryTextId
        {
            set
            {
                _PrimaryTextId = value;
                NotifyChangedOnUI(nameof(PrimaryText));
            }
        }

        public string SecondaryText
        {
            get
            {
                if (_SecondaryTextId == null) { return ""; }
                else { return SystemUtil.GetStringResource(_SecondaryTextId); }
            }
        }

        private string _SecondaryTextId = null;
        public string SecondaryTextId
        {
            set
            {
                _SecondaryTextId = value;
                NotifyChangedOnUI(nameof(SecondaryText));
            }
        }
    }
}
