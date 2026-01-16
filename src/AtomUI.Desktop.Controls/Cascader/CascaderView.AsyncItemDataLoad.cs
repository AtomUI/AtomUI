using System.Diagnostics;
using Avalonia;
using Avalonia.Threading;
using DynamicData;

namespace AtomUI.Desktop.Controls;

public partial class CascaderView
{
     #region 内部属性定义
    internal static readonly DirectProperty<CascaderView, bool> HasItemAsyncDataLoaderProperty =
        AvaloniaProperty.RegisterDirect<CascaderView, bool>(nameof(HasItemAsyncDataLoader),
            o => o.HasItemAsyncDataLoader,
            (o, v) => o.HasItemAsyncDataLoader = v);
    
    private bool _hasItemAsyncDataLoader;

    internal bool HasItemAsyncDataLoader
    {
        get => _hasItemAsyncDataLoader;
        set => SetAndRaise(HasItemAsyncDataLoaderProperty, ref _hasItemAsyncDataLoader, value);
    }
    #endregion

    private void HandleNodeLoadRequest(CascaderViewItem item)
    {
        if (_itemDataLoader == null)
        {
            return;
        }
        if (_itemDataLoader != null && ItemsSource == null)
        {
            throw new InvalidOperationException("ITreeNodeDataLoader is set, but the tree nodes are not initially set via ItemsSource.");
        }
        var data = TreeItemFromContainer(item);
        if (data is ICascaderViewItemData cascaderViewItemData)
        {
            var cts = new CancellationTokenSource(); // TODO 做一个超时结束
            item.IsLoading = true;
            Dispatcher.UIThread.InvokeAsync(async () =>
            {
                Debug.Assert(_itemDataLoader != null);
                var result = await _itemDataLoader.LoadAsync(cascaderViewItemData, cts.Token);
                item.IsLoading   = false;
                item.AsyncLoaded = true;
                ItemAsyncLoaded?.Invoke(this, new CascaderViewItemLoadedEventArgs(item, result));
                if (result.IsSuccess)
                {
                    if (result.Data?.Count > 0)
                    {
                        foreach (var child in result.Data)
                        {
                            child.UpdateParentNode(cascaderViewItemData);
                        }
                        cascaderViewItemData.Children.AddRange(result.Data);
                        item.IsExpanded = true;
                    }
                }
            });
        }
    }
}