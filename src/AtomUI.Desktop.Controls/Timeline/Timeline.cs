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

/// <summary>
/// Displays a sequence of items along a vertical or horizontal line, each item in a
/// <see cref="TimelineItem" /> marked by an indicator node.
/// </summary>
public partial class Timeline : AbstractTimeline
{
    public Timeline()
    {
    }
    
    /// <summary>
    /// Creates the container for a data item that is not itself an <see cref="AbstractTimelineItem" />: a new
    /// <see cref="TimelineItem" />. The item, its index and the recycle key do not affect the result.
    /// </summary>
    /// <returns>
    /// A new <see cref="TimelineItem" /> with the <c>semantic-item</c> class, so the generated part styles apply.
    /// </returns>
    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
    {
        var timelineItem = new TimelineItem
        {
            IsPending = false
        };
        timelineItem.Classes.Add(TimelineSemanticParts.ItemClass);
        return timelineItem;
    }

    /// <summary>
    /// Prepares a container for an item. It runs the base preparation and then adds the <c>semantic-item</c> class
    /// when the container is a <see cref="TimelineItem" />. This covers items that were supplied as
    /// <see cref="TimelineItem" /> instances and therefore did not come from
    /// <see cref="CreateContainerForItemOverride" />.
    /// </summary>
    protected override void PrepareContainerForItemOverride(Control container, object? item, int index)
    {
        base.PrepareContainerForItemOverride(container, item, index);
        if (container is TimelineItem timelineItem)
        {
            timelineItem.Classes.Add(TimelineSemanticParts.ItemClass);
        }
    }

    /// <summary>
    /// Creates the pending item. The base class calls it when the timeline attaches and whenever
    /// <see cref="AbstractTimeline.Pending" /> or <see cref="AbstractTimeline.PendingIcon" /> changes, as long as
    /// <see cref="AbstractTimeline.Pending" /> is not <see langword="null" />, and appends the result to the items.
    /// </summary>
    /// <returns>
    /// A <see cref="TimelineItem" /> whose content is <see cref="AbstractTimeline.Pending" /> and whose indicator shows
    /// <see cref="AbstractTimeline.PendingIcon" />, or a <see cref="LoadingOutlined" /> when that is
    /// <see langword="null" />. The icon is set to spin when it is an <see cref="Icon" />.
    /// </returns>
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
