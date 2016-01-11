using System;
using Windows.UI.Xaml;

namespace Locana
{
    /// <summary>
    /// Data to represent an item in the nav menu.
    /// </summary>
    public class NavMenuItem
    {
        public string Label { get; set; }

        public Type DestPage { get; set; }
        public object Arguments { get; set; }

        public string IconResId { set { Resource = Application.Current.Resources[value] as DataTemplate; } }
        public DataTemplate Resource { get; set; }
    }
}
