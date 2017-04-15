using Locana.DataModel;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Locana.Controls
{
    public sealed partial class LocalDirSetting : UserControl
    {
        public LocalDirSetting()
        {
            InitializeComponent();
        }

        public AppSettingData<string> SettingData
        {
            set
            {
                DataContext = value;
            }
        }

        public Button Button
        {
            get { return RightButton; }
        }
    }
}
