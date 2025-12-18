using System.Collections.Specialized;
using System.Reactive.Disposables;
using AtomUI.Controls;
using Avalonia;
using Avalonia.Controls;

namespace AtomUI.Desktop.Controls;

internal class UploadList : ItemsControl, IMotionAwareControl
{
    #region 公共属性定义

    public static readonly StyledProperty<UploadListType> ListTypeProperty =
        Upload.ListTypeProperty.AddOwner<UploadList>();
    
    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<UploadList>();
    
    public UploadListType ListType
    {
        get => GetValue(ListTypeProperty);
        set => SetValue(ListTypeProperty, value);
    }
    
    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }
    
    #endregion

    #region 内部属性定义

    Control IMotionAwareControl.PropertyBindTarget => this;

    #endregion
    
    private readonly Dictionary<AbstractUploadListItem, CompositeDisposable> _itemsBindingDisposables = new();
    
    public UploadList()
    {
        LogicalChildren.CollectionChanged += HandleCollectionChanged;
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == ListTypeProperty)
        {
            RefreshContainers();
        }
    }

    private void HandleCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems != null)
        {
            if (e.Action == NotifyCollectionChangedAction.Remove && e.OldItems.Count > 0)
            {
                foreach (var item in e.OldItems)
                {
                    if (item is AbstractUploadListItem listItem)
                    {
                        if (_itemsBindingDisposables.TryGetValue(listItem, out var disposable))
                        {
                            disposable.Dispose();
                            _itemsBindingDisposables.Remove(listItem);
                        }
                    }
                }
            }
        }
    }
    
    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
    {
        return new SegmentedItem();
    }

    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey)
    {
        return NeedsContainer<AbstractUploadListItem>(item, out recycleKey);
    }
    
    protected override void PrepareContainerForItemOverride(Control container, object? item, int index)
    {
        base.PrepareContainerForItemOverride(container, item, index);
        // if (container is SegmentedItem segmentedItem)
        // {
        //     var disposables = new CompositeDisposable(2);
        //     
        //     if (item != null && item is not Visual)
        //     {
        //         if (!segmentedItem.IsSet(SegmentedItem.ContentProperty))
        //         {
        //             segmentedItem.SetCurrentValue(SegmentedItem.ContentProperty, item);
        //         }
        //     }
        //     
        //     if (ItemTemplate != null)
        //     {
        //         disposables.Add(BindUtils.RelayBind(this, ItemTemplateProperty, segmentedItem, SegmentedItem.ContentTemplateProperty));
        //     }
        //     
        //     disposables.Add(BindUtils.RelayBind(this, SizeTypeProperty, segmentedItem, SegmentedItem.SizeTypeProperty));
        //     disposables.Add(BindUtils.RelayBind(this, IsMotionEnabledProperty, segmentedItem, SegmentedItem.IsMotionEnabledProperty));
        //     
        //     PrepareSegmentedItem(segmentedItem, item, index, disposables);
        //     
        //     if (_itemsBindingDisposables.TryGetValue(segmentedItem, out var oldDisposables))
        //     {
        //         oldDisposables.Dispose();
        //         _itemsBindingDisposables.Remove(segmentedItem);
        //     }
        //     _itemsBindingDisposables.Add(segmentedItem, disposables);
        // }
        // else
        // {
        //     throw new ArgumentOutOfRangeException(nameof(container), "The container type is incorrect, it must be type SegmentedItem.");
        // }
    }
}