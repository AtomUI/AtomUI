using System.Collections.Specialized;
using System.Diagnostics;
using System.Reactive.Disposables;
using AtomUI.Data;
using Avalonia;
using Avalonia.Controls;

namespace AtomUI.Desktop.Controls;

internal class CascaderViewLevelList : ItemsControl
{
    private readonly Dictionary<CascaderViewItem, CompositeDisposable> _itemsBindingDisposables = new();
    
    internal CascaderView? OwnerView { get; set; }
    
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
            var disposables = new CompositeDisposable(2);
            
            if (item != null && item is not Visual && item is ICascaderViewItemData cascaderViewItemData)
            {
                CascaderViewItem.ApplyNodeData(cascaderViewItem, cascaderViewItemData, disposables);
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
}