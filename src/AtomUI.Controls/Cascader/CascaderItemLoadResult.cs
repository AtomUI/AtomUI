using AtomUI.Controls;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// The outcome of one <c>ICascaderItemDataLoader.LoadAsync</c> call: the children to add to the option being
/// expanded, or a failure to report. A cascader view raises its <c>ItemAsyncLoaded</c> event with it when the
/// load ends.
/// </summary>
public record CascaderItemLoadResult
{
    /// <summary>
    /// Whether the load succeeded. The default is <see langword="true" />. The view appends <see cref="Data" /> to
    /// the loaded option only while this is <see langword="true" />; otherwise it only raises the
    /// <c>ItemAsyncLoaded</c> event.
    /// </summary>
    public bool IsSuccess { get; init; } = true;
    /// <summary>
    /// The status of the load, for the handlers of the <c>ItemAsyncLoaded</c> event. For a load that times out, is
    /// canceled or throws, the view reports <see cref="RpcStatusCode.Timeout" />,
    /// <see cref="RpcStatusCode.Cancelled" /> or <see cref="RpcStatusCode.Unknown" /> respectively. The view does
    /// not read this value.
    /// </summary>
    public RpcStatusCode StatusCode { get; init; } = RpcStatusCode.Unknown;
    /// <summary>
    /// A message about a failed load that is fit to show to the user. The view sets it to the message of the error
    /// when a load times out, is canceled or throws. The view does not show it itself.
    /// </summary>
    public string? UserFriendlyMessage { get; init; }
    /// <summary>
    /// The children of the loaded option, or <see langword="null" /> when the load produced none. When
    /// <see cref="IsSuccess" /> is <see langword="true" /> and the list is not empty, the view makes the loaded
    /// option their parent and appends them to its children.
    /// </summary>
    public IReadOnlyList<ICascaderOption>? Data { get; init; }
}