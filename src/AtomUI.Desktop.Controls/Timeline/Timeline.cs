using AtomUI.Controls;
using AtomUI.Controls.Commons;
using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Generated.AtomUIDesktopControls;
using AtomUI.Icons.AntDesign;
using AtomUI.Theme.Resources;
using Avalonia.Controls;

namespace AtomUI.Desktop.Controls;

internal static class TimelineIndicatorResources
{
    public static TimelineIndicatorTokenResourceKeys IndicatorTokenResourceKeys { get; } = new(
        ControlTokenResourceKey.Global(TimelineTokens.Identity, SharedTokenKind.RelativeLineHeight),
        ControlTokenResourceKey.Global(TimelineTokens.Identity, SharedTokenKind.ColorPrimary));
}

public partial class Timeline : AbstractTimeline
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
}
