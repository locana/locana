using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace Locana.Utility
{
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return (bool)value ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return (Visibility)value == Visibility.Visible ? true : false;
        }
    }

    public class BoolToVisibilityReverseConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return (bool)value ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return (Visibility)value == Visibility.Visible ? false : true;
        }
    }

    public static class BoolToVisibilityExtension
    {
        public static Visibility AsVisibility(this bool val)
        {
            if (val)
            {
                return Visibility.Visible;
            }
            else
            {
                return Visibility.Collapsed;
            }
        }

        public static Visibility AsReverseVisibility(this bool val)
        {
            if (val)
            {
                return Visibility.Collapsed;
            }
            else
            {
                return Visibility.Visible;
            }
        }
    }
}
