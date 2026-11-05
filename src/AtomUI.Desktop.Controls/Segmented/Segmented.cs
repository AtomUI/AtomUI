using AtomUI.Theme.Resources;
using Avalonia;
using AtomUI.Controls.Commons;
using AtomUI.Generated.AtomUIDesktopControls;
using Avalonia.Controls;

namespace AtomUI.Desktop.Controls;

public partial class Segmented : AbstractSegmented,
    IThemeInheritanceSource
{
    public Segmented()
    {
    }

    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
    {
        var segmentedItem = new SegmentedItem();
        segmentedItem.Classes.Add(SegmentedSemanticParts.ItemClass);
        return segmentedItem;
    }

    protected override void PrepareContainerForItemOverride(Control container, object? item, int index)
    {
        base.PrepareContainerForItemOverride(container, item, index);
        if (container is SegmentedItem segmentedItem)
        {
            segmentedItem.Classes.Add(SegmentedSemanticParts.ItemClass);
        }
    }
    #region 实现 IThemeInheritanceSource 接口

    // ControlTheme keeps native inheritance when no explicit owner typography is configured.
    AvaloniaObject? IThemeInheritanceSource.ThemeInheritanceParent => InheritanceParent;

    #endregion

}
