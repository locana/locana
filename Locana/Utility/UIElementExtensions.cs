using Windows.UI.Xaml.Controls;

namespace Locana.Utility
{
    public static class UIElementExtensions
    {
        public static void SetChildrenControlHitTest(this Panel parent, bool enable)
        {
            foreach (var child in parent.Children)
            {
                if (child is Panel)
                {
                    var panel = child as Panel;
                    panel.SetChildrenControlHitTest(enable);
                }
                else if (child is Control)
                {
                    child.IsHitTestVisible = enable;
                }
            }
        }

        public static void SetChildrenControlTabStop(this Panel parent, bool enable)
        {
            foreach (var child in parent.Children)
            {
                if (child is Panel)
                {
                    var panel = child as Panel;
                    panel.SetChildrenControlTabStop(enable);
                }
                else if (child is Control)
                {
                    var control = child as Control;
                    control.IsTabStop = enable;
                }
            }
        }
    }
}
