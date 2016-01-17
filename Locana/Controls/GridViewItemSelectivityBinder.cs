using Locana.DataModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

namespace Locana.Controls
{
    public class GridViewItemSelectivityBinder : StyleSelector
    {
        protected override Style SelectStyleCore(object item, DependencyObject container)
        {
            var selectorItem = container as GridViewItem;

            selectorItem.SetBinding(UIElement.IsHitTestVisibleProperty, new Binding
            {
                Source = item as Thumbnail,
                Path = new PropertyPath("IsSelectable"),
                Mode = BindingMode.OneWay
            });

            selectorItem.HorizontalContentAlignment = HorizontalAlignment.Stretch;
            selectorItem.VerticalContentAlignment = VerticalAlignment.Stretch;

            return base.SelectStyleCore(item, container);
        }
    }
}
