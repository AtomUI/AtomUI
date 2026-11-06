using System.Diagnostics;
using AtomUI.Controls;
using AtomUI.Controls.AsyncLoad;
using Avalonia;
using Avalonia.Threading;

namespace AtomUI.Desktop.Controls;

public partial class CascaderView
{
    #region 公开属性定义
    public static readonly StyledProperty<TimeSpan> AsyncLoadTimeoutProperty =
        AvaloniaProperty.Register<CascaderView, TimeSpan>(nameof(AsyncLoadTimeout),
            TimeSpan.FromSeconds(30));

    public TimeSpan AsyncLoadTimeout
    {
        get => GetValue(AsyncLoadTimeoutProperty);
        set => SetValue(AsyncLoadTimeoutProperty, value);
    }
    #endregion
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

    private readonly AsyncExpandLoadCoordinator<ICascaderOption, CascaderItemLoadResult> _asyncLoadCoordinator
        = new(ReferenceEqualityComparer<ICascaderOption>.Instance);

    private async Task LoadItemDataAsync(CascaderViewItem item)
    {
        if (DataLoader == null)
        {
            return;
        }

        var option = item.AttachedOption;
        if (option == null)
        {
            return;
        }

        _asyncLoadCoordinator.Timeout = AsyncLoadTimeout;
        item.IsLoading = true;

        var outcome = await _asyncLoadCoordinator.LoadOrJoinAsync(
            option,
            (ctx, token) =>
            {
                Debug.Assert(DataLoader != null);
                return DataLoader!.LoadAsync(ctx, token);
            });

        await Dispatcher.InvokeAsync(() =>
        {
            item.IsLoading = false;

            if (outcome.IsSuccess && outcome.Result != null)
            {
                var result = outcome.Result;
                item.AsyncLoaded = true;
                ItemAsyncLoaded?.Invoke(this, new CascaderViewItemLoadedEventArgs(item, result));

                if (result.IsSuccess && result.Data?.Count > 0)
                {
                    foreach (var child in result.Data)
                    {
                        child.UpdateParentNode(option);
                    }
                    foreach (var child in result.Data)
                    {
                        ((IList<ICascaderOption>)option.Children).Add(child);
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

            ItemAsyncLoaded?.Invoke(this, new CascaderViewItemLoadedEventArgs(item, new CascaderItemLoadResult()
            {
                IsSuccess           = false,
                StatusCode          = statusCode,
                UserFriendlyMessage = outcome.Error?.Message
            }));
        });
    }
}
