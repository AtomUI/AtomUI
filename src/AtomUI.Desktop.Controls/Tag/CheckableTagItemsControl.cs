using AtomUI.Controls.Commons;
using AtomUI.Generated.AtomUIDesktopControls;
using Avalonia.Controls;

namespace AtomUI.Desktop.Controls;

internal class CheckableTagItemsControl : AbstractCheckableTagItemsControl
{
    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
    {
        var checkableTag = new CheckableTag();
        checkableTag.Classes.Add(CheckableTagGroupSemanticParts.ItemClass);
        return checkableTag;
    }

    protected override void PrepareContainerForItemOverride(Control container, object? item, int index)
    {
        base.PrepareContainerForItemOverride(container, item, index);
        if (container is CheckableTag checkableTag)
        {
            checkableTag.Classes.Add(CheckableTagGroupSemanticParts.ItemClass);
        }
    }
}
