using Locana.Utility;

namespace Locana.DataModel
{
    public class ContentDialogSource
    {
        public string PrimaryButtonTextRes { set; get; }
        public string PrimaryButtonText
        {
            get { return SystemUtil.GetStringResource(PrimaryButtonTextRes); }
        }

        public string SecondaryButtonTextRes { set; get; }
        public string SecondaryButtonText
        {
            get { return SystemUtil.GetStringResource(SecondaryButtonTextRes); }
        }

        public string DialogMessageTextRes { set; get; }
        public string DialogMessageText
        {
            get { return SystemUtil.GetStringResource(DialogMessageTextRes); }
        }

        public string DialogTitleTextRes { set; get; }
        public string DialogTitleText
        {
            get { return SystemUtil.GetStringResource(DialogTitleTextRes); }
        }
    }
}
