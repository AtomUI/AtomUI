using AtomUI.Controls;
using AtomUI.Controls.Commons;
using AtomUI.Generated.AtomUIDesktopControls;
using AtomUI.Icons.AntDesign;
using AtomUI.Theme.Resources;
using Avalonia;
using Avalonia.Controls;

namespace AtomUI.Desktop.Controls;

public partial class Timeline : AbstractTimeline, IThemeInheritanceSource
{
    public Timeline()
    {
    }
    
    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
    {
        var timelineItem = new TimelineItem
        {
            IsPending = false
        };
        timelineItem.Classes.Add(TimelineSemanticParts.ItemClass);
        return timelineItem;
    }

    protected override void PrepareContainerForItemOverride(Control container, object? item, int index)
    {
        base.PrepareContainerForItemOverride(container, item, index);
        if (container is TimelineItem timelineItem)
        {
            timelineItem.Classes.Add(TimelineSemanticParts.ItemClass);
        }
    }

    protected override AbstractTimelineItem CreatePendingItem()
    {
        var pathIcon = PendingIcon ?? new LoadingOutlined();
        if (pathIcon is Icon icon)
        {
            icon.LoadingAnimation = IconAnimation.Spin;
        }
        var item = new TimelineItem
        {
            Content       = Pending,
            IndicatorIcon = pathIcon,
            IsPending     = true
        };
        item.Classes.Add(TimelineSemanticParts.ItemClass);
        return item;
    }

    #region 实现 IThemeInheritanceSource 接口

    // ControlTheme cannot read the protected inheritance parent. Its Setters own
    // the final typography values; this bridge only exposes their actual source.
    AvaloniaObject? IThemeInheritanceSource.ThemeInheritanceParent => InheritanceParent;

    #endregion
}
