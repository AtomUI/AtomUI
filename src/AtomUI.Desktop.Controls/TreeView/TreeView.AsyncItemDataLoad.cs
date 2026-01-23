using System.Diagnostics;
using Avalonia;
using Avalonia.Threading;
using DynamicData;

namespace AtomUI.Desktop.Controls;

public partial class TreeView
{
    #region 内部属性定义
    internal static readonly DirectProperty<TreeView, bool> HasTreeItemDataLoaderProperty =
        AvaloniaProperty.RegisterDirect<TreeView, bool>(nameof(HasTreeItemDataLoader),
            o => o.HasTreeItemDataLoader,
            (o, v) => o.HasTreeItemDataLoader = v);
    
    private bool _hasTreeItemDataLoader;

    internal bool HasTreeItemDataLoader
    {
        get => _hasTreeItemDataLoader;
        set => SetAndRaise(HasTreeItemDataLoaderProperty, ref _hasTreeItemDataLoader, value);
    }
    #endregion

    private void HandleNodeLoadRequest(TreeViewItem item)
    {
        if (DataLoader == null)
        {
            return;
        }
        if (DataLoader != null && ItemsSource == null)
        {
            throw new InvalidOperationException("ITreeNodeDataLoader is set, but the tree nodes are not initially set via ItemsSource.");
        }
        var data = TreeItemFromContainer(item);
        if (data is ITreeViewItemData treeItemData)
        {
            var cts = new CancellationTokenSource(); // TODO 做一个超时结束
            item.IsLoading = true;
            Dispatcher.UIThread.InvokeAsync(async () =>
            {
                Debug.Assert(DataLoader != null);
                var result = await DataLoader.LoadAsync(treeItemData, cts.Token);
                item.IsLoading   = false;
                item.AsyncLoaded = true; // TODO 是不是应该多给几次机会？
                TreeItemLoaded?.Invoke(this, new TreeViewItemLoadedEventArgs(item, result));
                if (result.IsSuccess)
                {
                    if (result.Data?.Count > 0)
                    {
                        foreach (var child in result.Data)
                        {
                            child.UpdateParentNode(treeItemData);
                        }
                        treeItemData.Children.AddRange(result.Data);
                        item.IsExpanded = true;
                    }
                }
            });
        }
    }
}