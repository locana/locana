using System.Collections.ObjectModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Locana.Controls
{
    public sealed partial class KeyAssignmentCollectionView : UserControl
    {
        public KeyAssignmentCollectionView()
        {
            this.InitializeComponent();
        }

        private void KeyAssignmentsGrid_Loaded(object sender, RoutedEventArgs e)
        {
            GridSources.Source = KeyAssignmentCollection;
        }

        private void KeyAssignmentsGrid_Unloaded(object sender, RoutedEventArgs e)
        {
            GridSources.Source = null;
        }

        public readonly ObservableCollection<KeyAssignmentData> KeyAssignmentCollection = new ObservableCollection<KeyAssignmentData>();
    }

    public class KeyAssignmentData
    {
        public string AssignedKey { set; get; }
        public string Description { set; get; }
    }
}
