using Locana.DataModel;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Locana.Controls
{
    public sealed partial class ToggleSetting : UserControl
    {
        public ToggleSetting()
        {
            InitializeComponent();
        }

        public AppSettingData<bool> SettingData
        {
            set
            {
                DataContext = value;
            }
        }
    }
}
