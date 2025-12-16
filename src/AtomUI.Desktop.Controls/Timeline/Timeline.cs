using System.Collections.Specialized;
using System.Reactive.Disposables;
using AtomUI.Controls;
using AtomUI.Data;
using AtomUI.Icons.AntDesign;
using AtomUI.Theme;
using AtomUI.Utils;
using Avalonia;
using Avalonia.Controls;
using Avalonia.LogicalTree;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

public enum TimeLineMode
{
    Left,
    Right,
    Alternate
}

public class Timeline : ItemsControl,
                        IControlSharedTokenResourcesHost
{
    #region 公共属性定义

    public static readonly StyledProperty<TimeLineMode> ModeProperty =
        AvaloniaProperty.Register<Timeline, TimeLineMode>(nameof(Mode), TimeLineMode.Left);

    public static readonly StyledProperty<object?> PendingProperty =
        AvaloniaProperty.Register<Timeline, object?>(nameof(Pending));

    public static readonly StyledProperty<bool> IsReverseProperty =
        AvaloniaProperty.Register<Timeline, bool>(nameof(IsReverse), false);

    public static readonly StyledProperty<PathIcon?> PendingIconProperty =
        AvaloniaProperty.Register<Timeline, PathIcon?>(nameof(PendingIcon));

    public TimeLineMode Mode
    {
        get => GetValue(ModeProperty);
        set => SetValue(ModeProperty, value);
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

    #region 内部属性定义

    Control IControlSharedTokenResourcesHost.HostControl => this;
    string IControlSharedTokenResourcesHost.TokenId => TimelineToken.ID;

    internal WeakReference<TimelineItem>? PendingItemReference => _pendingItemReference;
    private WeakReference<TimelineItem>? _pendingItemReference;

    #endregion
    
    private readonly Dictionary<TimelineItem, CompositeDisposable> _itemsBindingDisposables = new();

    static Timeline()
    {
        AffectsMeasure<Timeline>(ModeProperty);
        AffectsArrange<Timeline>(IsReverseProperty);
    }
    
    public Timeline()
    {
        this.RegisterResources();
        LogicalChildren.CollectionChanged += HandleItemsChanged;
    }

    private void HandleItemsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        var isLabelLayout = false;
        foreach (var item in LogicalChildren)
        {
            if (item is TimelineItem timelineItem)
            {
                if (timelineItem.Label is not null)
                {
                    isLabelLayout = true;
                }
            }
        }

        foreach (var item in LogicalChildren)
        {
            if (item is TimelineItem timelineItem)
            {
                timelineItem.IsLabelLayout = isLabelLayout;
            }
        }
        
        if (e.OldItems != null)
        {
            if (e.Action == NotifyCollectionChangedAction.Remove && e.OldItems.Count > 0)
            {
                foreach (var item in e.OldItems)
                {
                    if (item is TimelineItem timelineItem)
                    {
                        if (_itemsBindingDisposables.TryGetValue(timelineItem, out var disposable))
                        {
                            disposable.Dispose();
                            _itemsBindingDisposables.Remove(timelineItem);
                        }
                    }
                }
            }
        }
    }

    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
    {
        return new TimelineItem()
        {
            IsPending = false
        };
    }

    protected override void OnAttachedToLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        base.OnAttachedToLogicalTree(e);
        SetupPendingItem();
    }

    private void SetupPendingItem()
    {
        if (Pending != null)
        {
            if (_pendingItemReference != null)
            {
                if (_pendingItemReference.TryGetTarget(out var item))
                {
                    Items.Remove(item);
                }
            }

            var pathIcon = PendingIcon ?? new LoadingOutlined();
            if (pathIcon is Icon icon)
            {
                icon.LoadingAnimation = IconAnimation.Spin;
            }
      
            var pendingTimelineItem = new TimelineItem()
            {
                Content       = Pending,
                IndicatorIcon = pathIcon,
                IsPending     = true
            };
            _pendingItemReference = new WeakReference<TimelineItem>(pendingTimelineItem);
            Items.Add(pendingTimelineItem);
        }
    }

    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey)
    {
        return NeedsContainer<TimelineItem>(item, out recycleKey);
    }

    protected override void PrepareContainerForItemOverride(Control element, object? item, int index)
    {
        base.PrepareContainerForItemOverride(element, item, index);
        if (element is TimelineItem timelineItem)
        {
            var disposables = new CompositeDisposable(2);
            disposables.Add(BindUtils.RelayBind(this, ModeProperty, timelineItem, TimelineItem.ModeProperty));
            disposables.Add(BindUtils.RelayBind(this, IsReverseProperty, timelineItem, TimelineItem.IsReverseProperty));
            if (_itemsBindingDisposables.TryGetValue(timelineItem, out var oldDisposables))
            {
                oldDisposables.Dispose();
                _itemsBindingDisposables.Remove(timelineItem);
            }
            _itemsBindingDisposables.Add(timelineItem, disposables);
        }
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
        for (int i = 0, logicalIndex = 0; i < ItemCount; i++)
        {
            if (ContainerFromIndex(i) is TimelineItem timelineItem && timelineItem.IsVisible)
            {
                var idx = IsReverse ? ItemCount - 1 - logicalIndex : logicalIndex;
                CalculateItemPositionInfo(timelineItem, idx);
                logicalIndex++;
            }
        }
    }
    
    internal void CalculateItemPositionInfo(TimelineItem timelineItem, int index)
    {
        timelineItem.IsOdd         = index % 2 != 0;
        timelineItem.IsFirst       = index == 0;
        timelineItem.IsLast        = index == GetVisibleItemsCount() - 1;
        timelineItem.NextIsPending = false;
        if (PendingItemReference != null && PendingItemReference.TryGetTarget(out var pendingItem))
        {
            if (timelineItem == pendingItem)
            {
                if (!IsReverse)
                {
                    var previousItemIndex = index - 1;
                    while (previousItemIndex >= 0)
                    {
                        if (ContainerFromIndex(previousItemIndex) is TimelineItem previousItem && previousItem.IsVisible)
                        {
                            previousItem.NextIsPending = true;
                            break;
                        }

                        previousItemIndex--;
                    }
                }
                else
                {
                    var previousItemIndex = index + 1;
                    while (previousItemIndex < ItemCount)
                    {
                        if (ContainerFromIndex(previousItemIndex) is TimelineItem previousItem && previousItem.IsVisible)
                        {
                            previousItem.NextIsPending = true;
                            break;
                        }

                        previousItemIndex++;
                    }
                }
            }
        }
    }

    private int GetVisibleItemsCount()
    {
        var count = 0;
        for (var i = 0; i < ItemCount; i++)
        {
            if (ContainerFromIndex(i) is TimelineItem previousItem && previousItem.IsVisible)
            {
                ++count;
            }
        }
        return count;
    }
}