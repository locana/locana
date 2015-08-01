using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Kazyx.Uwpmm.Control
{
    public sealed partial class SettingSection : UserControl
    {
        string Title
        {
            get
            {
                return TitleTextBlock.Text;
            }
            set
            {
                TitleTextBlock.Text = value;
            }
        }

        public SettingSection(string SectionTitle)
        {
            InitializeComponent();
            TitleTextBlock.Text = SectionTitle;
        }

        public void Add(UIElement child)
        {
            SettingItems.Children.Add(child);
        }
    }
}
