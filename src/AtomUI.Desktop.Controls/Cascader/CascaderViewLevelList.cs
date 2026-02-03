using System.Collections.Specialized;
using System.Diagnostics;
using System.Reactive.Disposables;
using AtomUI.Data;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;

namespace AtomUI.Desktop.Controls;

internal class CascaderViewLevelList : SelectingItemsControl
{
    #region 公共属性定义

    public static readonly DirectProperty<CascaderViewLevelList, int> LevelProperty =
        AvaloniaProperty.RegisterDirect<CascaderViewLevelList, int>(nameof(Level), 
            o => o.Level);
    
    private int _level;
    public int Level
    {
        get => _level;
        internal set => SetAndRaise(LevelProperty, ref _level, value);
    }

    #endregion
    
    private readonly Dictionary<CascaderViewItem, CompositeDisposable> _itemsBindingDisposables = new();
    
    internal CascaderView? OwnerView { get; set; }
    internal CascaderViewItem? ParentCascaderViewItem { get; set; }
    
    public CascaderViewLevelList()
    {
        LogicalChildren.CollectionChanged += HandleLogicalChildrenChanged;
    }
    
    private void HandleLogicalChildrenChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Remove)
        {
            if (e.OldItems != null)
            {
                foreach (var item in e.OldItems)
                {
                    if (item is CascaderViewItem cascaderViewItem)
                    {
                        if (_itemsBindingDisposables.TryGetValue(cascaderViewItem, out var disposable))
                        {
                            disposable.Dispose();
                        }
                        _itemsBindingDisposables.Remove(cascaderViewItem);
                    }
                }
            }
        }
    }
    
    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
    {
        return new CascaderViewItem();
    }

    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey)
    {
        return NeedsContainer<CascaderViewItem>(item, out recycleKey);
    }
    
    protected override void ContainerForItemPreparedOverride(Control container, object? item, int index)
    {
        base.ContainerForItemPreparedOverride(container, item, index);
        Debug.Assert(OwnerView != null);
        if (container is CascaderViewItem cascaderViewItem)
        {
            var disposables = new CompositeDisposable(8);
            
            if (item != null && item is not Visual && item is ICascaderViewOption option)
            {
                cascaderViewItem.SetCurrentValue(CascaderViewItem.HeaderProperty, option);
                cascaderViewItem.SetCurrentValue(CascaderViewItem.IconProperty, option.Icon);
                cascaderViewItem.SetCurrentValue(CascaderViewItem.IsCheckedProperty, option.IsChecked);
                cascaderViewItem.SetCurrentValue(CascaderViewItem.IsEnabledProperty, option.IsEnabled);
                cascaderViewItem.SetCurrentValue(CascaderViewItem.IsExpandedProperty, option.IsExpanded);
                cascaderViewItem.SetCurrentValue(CascaderViewItem.IsCheckBoxEnabledProperty, option.IsCheckBoxEnabled);
                cascaderViewItem.ItemKey   = option.ItemKey;
            
                if (!cascaderViewItem.IsSet(CascaderViewItem.IsLeafProperty))
                {
                    cascaderViewItem.IsLeaf = option.IsLeaf;
                }
            }
            
            if (ItemTemplate != null)
            {
                disposables.Add(BindUtils.RelayBind(this, ItemTemplateProperty, cascaderViewItem, CascaderViewItem.HeaderTemplateProperty));
            }
            
            disposables.Add(BindUtils.RelayBind(OwnerView, CascaderView.LoadingIconProperty, cascaderViewItem, CascaderViewItem.LoadingIconProperty));
            disposables.Add(BindUtils.RelayBind(OwnerView, CascaderView.ExpandIconProperty, cascaderViewItem, CascaderViewItem.ExpandIconProperty));
            disposables.Add(BindUtils.RelayBind(OwnerView, CascaderView.IsMotionEnabledProperty, cascaderViewItem, CascaderViewItem.IsMotionEnabledProperty));
            disposables.Add(BindUtils.RelayBind(OwnerView, CascaderView.EffectiveToggleTypeProperty, cascaderViewItem, CascaderViewItem.ToggleTypeProperty));
            disposables.Add(BindUtils.RelayBind(OwnerView, CascaderView.HasItemAsyncDataLoaderProperty, cascaderViewItem,
                CascaderViewItem.HasItemAsyncDataLoaderProperty));
            disposables.Add(BindUtils.RelayBind(OwnerView, CascaderView.IsMaxSelectReachedProperty, cascaderViewItem, CascaderViewItem.IsMaxSelectReachedProperty));
            
            if (_itemsBindingDisposables.TryGetValue(cascaderViewItem, out var oldDisposables))
            {
                oldDisposables.Dispose();
                _itemsBindingDisposables.Remove(cascaderViewItem);
            }
            _itemsBindingDisposables.Add(cascaderViewItem, disposables);
        }
    }
    
    internal bool UpdateSelectionFromPointerEvent(Control source, PointerEventArgs e)
    {
        var container = GetContainerFromEventSource(e.Source);
        if (container is CascaderViewItem cascaderViewItem && cascaderViewItem.IsLeaf)
        {
            return UpdateSelectionFromEventSource(
                source,
                true,
                false,
                true,
                e.GetCurrentPoint(source).Properties.IsRightButtonPressed);
        }
        return false;
    }
}