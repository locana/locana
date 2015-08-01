using Kazyx.Uwpmm.DataModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

namespace Kazyx.Uwpmm.Control
{
    public class GridViewItemSelectivityBinder : StyleSelector
    {
        protected override Style SelectStyleCore(object item, DependencyObject container)
        {
            var selectorItem = container as GridViewItem;

            selectorItem.SetBinding(GridViewItem.IsEnabledProperty, new Binding
            {
                Source = item as Thumbnail,
                Path = new PropertyPath("IsSelectable"),
                Mode = BindingMode.OneWay
            });

            return base.SelectStyleCore(item, container);
        }
    }
}
