using System.Collections.Specialized;
using Avalonia;
using Avalonia.Controls;
using Avalonia.LogicalTree;
using Avalonia.Layout;
using Avalonia.VisualTree;

namespace AtomUI.Controls.Commons;

public abstract class AbstractTimeline : ItemsControl
{
    #region 公共属性定义

    public static readonly StyledProperty<TimelineMode> ModeProperty =
        AvaloniaProperty.Register<AbstractTimeline, TimelineMode>(nameof(Mode), TimelineMode.Start);

    public static readonly StyledProperty<Orientation> OrientationProperty =
        StackPanel.OrientationProperty.AddOwner<AbstractTimeline>();

    public static readonly StyledProperty<object?> PendingProperty =
        AvaloniaProperty.Register<AbstractTimeline, object?>(nameof(Pending));

    public static readonly StyledProperty<bool> IsReverseProperty =
        AvaloniaProperty.Register<AbstractTimeline, bool>(nameof(IsReverse), false);

    public static readonly StyledProperty<PathIcon?> PendingIconProperty =
        AvaloniaProperty.Register<AbstractTimeline, PathIcon?>(nameof(PendingIcon));

    public TimelineMode Mode
    {
        get => GetValue(ModeProperty);
        set => SetValue(ModeProperty, value);
    }

    public Orientation Orientation
    {
        get => GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    public object? Pending
    {
        get => GetValue(PendingProperty);
        set => SetValue(PendingProperty, value);
    }

    public bool IsReverse
    {
        get => GetValue(IsReverseProperty);
        set => SetValue(IsReverseProperty, value);
    }

    public PathIcon? PendingIcon
    {
        get => GetValue(PendingIconProperty);
        set => SetValue(PendingIconProperty, value);
    }

    #endregion

    private WeakReference<AbstractTimelineItem>? _pendingItemReference;

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

    protected abstract AbstractTimelineItem CreatePendingItem();

    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey)
    {
        return NeedsContainer<AbstractTimelineItem>(item, out recycleKey);
    }

    protected override void ContainerForItemPreparedOverride(Control container, object? item, int index)
    {
        base.ContainerForItemPreparedOverride(container, item, index);
        CalculateItemsPositionInfo();
    }

    protected override void ContainerIndexChangedOverride(Control container, int oldIndex, int newIndex)
    {
        base.ContainerIndexChangedOverride(container, oldIndex, newIndex);
        CalculateItemsPositionInfo();
    }

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
        // Logical children change before ItemsControl publishes ItemCount and before
        // the presenter removes its old containers. Use the authoritative source.
        for (var index = 0; index < Items.Count; index++)
        {
            if ((Items[index] as AbstractTimelineItem ?? ContainerFromIndex(index)) is not AbstractTimelineItem timelineItem)
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

    #region 内部协作 API

    internal void NotifyItemLayoutChanged()
    {
        CalculateItemsPositionInfo();
    }

    #endregion

}
