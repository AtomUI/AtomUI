using AtomUI.Controls;

namespace AtomUI.Desktop.Controls;

public class TreeSelectTreeView : TreeView
{
    protected override Type StyleKeyOverride => typeof(TreeView);

    protected override void NotifyTreeItemClicked(TreeViewItem item)
    {
        if (ToggleType == ItemToggleType.CheckBox)
        {
            if (item.IsChecked == true)
            {
                item.SetCurrentValue(TreeViewItem.IsCheckedProperty, false);
            }
            else
            {
                item.SetCurrentValue(TreeViewItem.IsCheckedProperty, true);
            }
        }
    }
}