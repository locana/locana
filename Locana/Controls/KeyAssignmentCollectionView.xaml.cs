using System;
using System.Collections.ObjectModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

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
        public virtual bool IsEnabled { set; get; } = true;
    }

    public class TitleTextForegroundBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var resources = Application.Current.Resources;
            return (bool)value ? resources["TextBoxForegroundHeaderThemeBrush"] as Brush : resources["TextBoxDisabledForegroundThemeBrush"];
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class DescriptionTextForegroundBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var resources = Application.Current.Resources;
            return (bool)value ? resources["ApplicationSecondaryForegroundThemeBrush"] as Brush : resources["TextBoxDisabledForegroundThemeBrush"];
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
