using System.Diagnostics;
using AtomUI.Controls;
using AtomUI.Controls.AsyncLoad;
using Avalonia;

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

    #region 公开属性定义
    /// <summary>
    /// Identifies the <see cref="AsyncLoadTimeout" /> property.
    /// </summary>
    public static readonly StyledProperty<TimeSpan> AsyncLoadTimeoutProperty =
        AvaloniaProperty.Register<TreeView, TimeSpan>(nameof(AsyncLoadTimeout),
            TimeSpan.FromSeconds(30));

    /// <summary>
    /// Maximum time a <see cref="DataLoader" /> call for the children of a node may take. Defaults to 30 seconds.
    /// The value is read each time a node load starts. A load that takes longer ends with a
    /// <see cref="TreeItemLoaded" /> result whose status is <see cref="RpcStatusCode.Timeout" />, and no children
    /// are added.
    /// </summary>
    public TimeSpan AsyncLoadTimeout
    {
        get => GetValue(AsyncLoadTimeoutProperty);
        set => SetValue(AsyncLoadTimeoutProperty, value);
    }
    #endregion

    private readonly AsyncExpandLoadCoordinator<ITreeItemNode, TreeItemLoadResult> _asyncLoadCoordinator
        = new(ReferenceEqualityComparer<ITreeItemNode>.Instance);

    private void HandleNodeLoadRequest(TreeViewItem viewItem)
    {
        if (DataLoader == null)
        {
            return;
        }
        if (ItemsSource == null)
        {
            throw new InvalidOperationException("ITreeNodeDataLoader is set, but the tree nodes are not initially set via ItemsSource.");
        }
        var data = TreeItemFromContainer(viewItem);
        if (data is ITreeItemNode treeItemData)
        {
            _ = LoadNodeDataAsync(viewItem, treeItemData);
        }
    }

    private async Task LoadNodeDataAsync(TreeViewItem viewItem, ITreeItemNode treeItemData)
    {
        _asyncLoadCoordinator.Timeout = AsyncLoadTimeout;
        viewItem.IsLoading = true;

        var outcome = await _asyncLoadCoordinator.LoadOrJoinAsync(
            treeItemData,
            (ctx, token) =>
            {
                Debug.Assert(DataLoader != null);
                return DataLoader!.LoadAsync(ctx, token);
            });

        await Dispatcher.InvokeAsync(() =>
        {
            viewItem.IsLoading = false;

            if (outcome.IsSuccess && outcome.Result != null)
            {
                var result = outcome.Result;
                viewItem.AsyncLoaded = true;
                TreeItemLoaded?.Invoke(this, new TreeViewItemLoadedEventArgs(viewItem, result));

                if (result.IsSuccess && result.Data?.Count > 0)
                {
                    if (_treeDataController.TryAppendChildren(treeItemData, result.Data))
                    {
                        viewItem.IsExpanded = true;
                    }
                }
                return;
            }

            var statusCode = outcome.Status switch
            {
                AsyncLoadStatus.TimedOut  => RpcStatusCode.Timeout,
                AsyncLoadStatus.Cancelled => RpcStatusCode.Cancelled,
                _                         => RpcStatusCode.Unknown
            };

            TreeItemLoaded?.Invoke(this, new TreeViewItemLoadedEventArgs(viewItem, new TreeItemLoadResult()
            {
                IsSuccess           = false,
                StatusCode          = statusCode,
                UserFriendlyMessage = outcome.Error?.Message
            }));
        });
    }
}
