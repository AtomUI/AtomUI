using System.Collections.Specialized;
using Avalonia;
using Avalonia.Controls;
using Avalonia.LogicalTree;
using Avalonia.Layout;
using Avalonia.VisualTree;

namespace AtomUI.Controls.Commons;

/// <summary>
/// Base class of controls that list items along a vertical or horizontal axis, each item drawn with a node and
/// a rail toward the next node. A derived class provides the pending item through
/// <see cref="CreatePendingItem" />.
/// </summary>
public abstract class AbstractTimeline : ItemsControl
{
    #region 公共属性定义

    /// <summary>
    /// Identifies the <see cref="Mode" /> property.
    /// </summary>
    public static readonly StyledProperty<TimelineMode> ModeProperty =
        AvaloniaProperty.Register<AbstractTimeline, TimelineMode>(nameof(Mode), TimelineMode.Start);

    /// <summary>
    /// Identifies the <see cref="Orientation" /> property.
    /// </summary>
    public static readonly StyledProperty<Orientation> OrientationProperty =
        StackPanel.OrientationProperty.AddOwner<AbstractTimeline>();

    /// <summary>
    /// Identifies the <see cref="Pending" /> property.
    /// </summary>
    public static readonly StyledProperty<object?> PendingProperty =
        AvaloniaProperty.Register<AbstractTimeline, object?>(nameof(Pending));

    /// <summary>
    /// Identifies the <see cref="IsReverse" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsReverseProperty =
        AvaloniaProperty.Register<AbstractTimeline, bool>(nameof(IsReverse), false);

    /// <summary>
    /// Identifies the <see cref="PendingIcon" /> property.
    /// </summary>
    public static readonly StyledProperty<PathIcon?> PendingIconProperty =
        AvaloniaProperty.Register<AbstractTimeline, PathIcon?>(nameof(PendingIcon));

    /// <summary>
    /// Sets on which side of its node each item places its content. The value is bound to every item container.
    /// The default is <see cref="TimelineMode.Start" />.
    /// </summary>
    public TimelineMode Mode
    {
        get => GetValue(ModeProperty);
        set => SetValue(ModeProperty, value);
    }

    /// <summary>
    /// Direction in which the items are stacked. The value is bound to every item container. The default is
    /// <see cref="Avalonia.Layout.Orientation.Vertical" />.
    /// </summary>
    public Orientation Orientation
    {
        get => GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    /// <summary>
    /// Content of the pending item. Any value other than <see langword="null" /> makes the control append such an
    /// item, created by <see cref="CreatePendingItem" />, to <c>Items</c>; <see langword="null" /> removes it.
    /// </summary>
    public object? Pending
    {
        get => GetValue(PendingProperty);
        set => SetValue(PendingProperty, value);
    }

    /// <summary>
    /// Displays the items in reverse order, the last item first. The first, last and odd positions of the items
    /// follow that order. The value is bound to every item container. The default is <see langword="false" />.
    /// </summary>
    public bool IsReverse
    {
        get => GetValue(IsReverseProperty);
        set => SetValue(IsReverseProperty, value);
    }

    /// <summary>
    /// Icon for the node of the pending item. <see cref="CreatePendingItem" /> reads it when the item is built.
    /// The default is <see langword="null" />, which leaves the choice to the derived control.
    /// </summary>
    public PathIcon? PendingIcon
    {
        get => GetValue(PendingIconProperty);
        set => SetValue(PendingIconProperty, value);
    }

    #endregion

    #region 内部属性定义

    private WeakReference<AbstractTimelineItem>? _pendingItemReference;

    #endregion

    static AbstractTimeline()
    {
        OrientationProperty.OverrideDefaultValue<AbstractTimeline>(Orientation.Vertical);
        AffectsMeasure<AbstractTimeline>(ModeProperty, OrientationProperty);
        AffectsArrange<AbstractTimeline>(IsReverseProperty);
    }
    
    public AbstractTimeline()
    {
        LogicalChildren.CollectionChanged += HandleItemsChanged;
    }

    private void HandleItemsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        CalculateItemsPositionInfo();
    }

    /// <summary>
    /// Calls the base method and then builds the pending item when <see cref="Pending" /> is not
    /// <see langword="null" />.
    /// </summary>
    /// <remarks>
    /// Overrides must call the base method, or no pending item is created on attach.
    /// </remarks>
    protected override void OnAttachedToLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        base.OnAttachedToLogicalTree(e);
        SetupPendingItem();
    }

    private void SetupPendingItem()
    {
        RemovePendingItem();
        if (Pending != null)
        {
            var pendingTimelineItem = CreatePendingItem();
            _pendingItemReference = new WeakReference<AbstractTimelineItem>(pendingTimelineItem);
            Items.Add(pendingTimelineItem);
        }
    }

    private void RemovePendingItem()
    {
        if (_pendingItemReference?.TryGetTarget(out var item) == true)
        {
            Items.Remove(item);
        }

        _pendingItemReference = null;
    }

    /// <summary>
    /// Creates the pending item. The base class calls it whenever it builds that item: when the control is
    /// attached to the logical tree and whenever <see cref="Pending" /> or <see cref="PendingIcon" /> changes. It
    /// adds the returned item to <c>Items</c> itself.
    /// </summary>
    /// <returns>
    /// The item for the current <see cref="Pending" /> and <see cref="PendingIcon" /> values.
    /// </returns>
    protected abstract AbstractTimelineItem CreatePendingItem();

    /// <summary>
    /// Returns <see langword="false" /> when <paramref name="item" /> is itself an
    /// <see cref="AbstractTimelineItem" />, which then serves as its own container, and
    /// <see langword="true" /> when it needs one. <paramref name="recycleKey" /> is
    /// <see langword="null" /> in the first case and the item itself in the second.
    /// </summary>
    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey)
    {
        return NeedsContainer<AbstractTimelineItem>(item, out recycleKey);
    }

    /// <summary>
    /// Calls the base method and then binds <see cref="Orientation" />, <see cref="Mode" /> and
    /// <see cref="IsReverse" /> of this control to the container, when it is an <see cref="AbstractTimelineItem" />.
    /// </summary>
    /// <remarks>
    /// Overrides must call the base method, or the item does not follow the layout settings of its timeline.
    /// </remarks>
    protected override void PrepareContainerForItemOverride(Control element, object? item, int index)
    {
        base.PrepareContainerForItemOverride(element, item, index);
        if (element is AbstractTimelineItem timelineItem)
        {
            timelineItem[!AbstractTimelineItem.OrientationProperty] = this[!OrientationProperty];
            timelineItem[!AbstractTimelineItem.ModeProperty]      = this[!ModeProperty];
            timelineItem[!AbstractTimelineItem.IsReverseProperty] = this[!IsReverseProperty];
        }
    }

    /// <summary>
    /// Calls the base method and then recalculates the order flags of all item containers: odd, first, last, label
    /// layout and whether the next item is pending.
    /// </summary>
    /// <remarks>
    /// Overrides must call the base method, or items keep the order flags of their previous position.
    /// </remarks>
    protected override void ContainerForItemPreparedOverride(Control container, object? item, int index)
    {
        base.ContainerForItemPreparedOverride(container, item, index);
        CalculateItemsPositionInfo();
    }

    /// <summary>
    /// Calls the base method and then recalculates the order flags of all item containers.
    /// </summary>
    /// <remarks>
    /// Overrides must call the base method, or the order flags do not follow the new position of the container.
    /// </remarks>
    protected override void ContainerIndexChangedOverride(Control container, int oldIndex, int newIndex)
    {
        base.ContainerIndexChangedOverride(container, oldIndex, newIndex);
        CalculateItemsPositionInfo();
    }

    /// <summary>
    /// Calls the base method, then rebuilds the pending item when <see cref="Pending" /> or
    /// <see cref="PendingIcon" /> changed, and recalculates the order flags of the items when
    /// <see cref="IsReverse" /> changed while the control is attached to the visual tree.
    /// </summary>
    /// <remarks>
    /// Overrides must call the base method, or the pending item and the order flags no longer follow these
    /// properties.
    /// </remarks>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == PendingProperty || change.Property == PendingIconProperty)
        {
            SetupPendingItem();
        }

        if (this.IsAttachedToVisualTree())
        {
            if (change.Property == IsReverseProperty)
            {
                CalculateItemsPositionInfo();
            }
        }
    }

    private void CalculateItemsPositionInfo()
    {
        var visibleItems = new List<AbstractTimelineItem>();
        for (var index = 0; index < ItemCount; index++)
        {
            if (ContainerFromIndex(index) is not AbstractTimelineItem timelineItem)
            {
                continue;
            }

            timelineItem.IsOdd         = false;
            timelineItem.IsFirst       = false;
            timelineItem.IsLast        = false;
            timelineItem.IsLabelLayout = false;
            timelineItem.NextIsPending = false;

            if (timelineItem.IsVisible)
            {
                visibleItems.Add(timelineItem);
            }
        }

        if (IsReverse)
        {
            visibleItems.Reverse();
        }

        var isLabelLayout = false;
        foreach (var timelineItem in visibleItems)
        {
            if (timelineItem.Label is not null)
            {
                isLabelLayout = true;
                break;
            }
        }

        for (var index = 0; index < visibleItems.Count; index++)
        {
            var timelineItem = visibleItems[index];
            timelineItem.IsOdd         = index % 2 != 0;
            timelineItem.IsFirst       = index == 0;
            timelineItem.IsLast        = index == visibleItems.Count - 1;
            timelineItem.IsLabelLayout = isLabelLayout;
        }

        for (var index = 0; index < visibleItems.Count - 1; index++)
        {
            visibleItems[index].NextIsPending = visibleItems[index + 1].IsPending;
        }
    }

    internal void NotifyItemLayoutChanged()
    {
        CalculateItemsPositionInfo();
    }

}
