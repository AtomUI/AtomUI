using AtomUI.Controls;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// The outcome of one <see cref="ITreeItemNodeLoader.LoadAsync" /> call: the children of a node, or a failure.
/// </summary>
public record TreeItemLoadResult
{
    /// <summary>
    /// Whether the children were loaded. It is <see langword="true" /> by default, so a loader that returns only
    /// <see cref="Data" /> reports a success.
    /// </summary>
    public bool IsSuccess { get; init; } = true;
    /// <summary>
    /// A status code that describes the outcome of the load. The tree view adds the children based on
    /// <see cref="IsSuccess" /> and <see cref="Data" />, not on this code.
    /// </summary>
    public RpcStatusCode StatusCode { get; init; } = RpcStatusCode.Unknown;
    /// <summary>
    /// A message about a failed load that is suitable for display to the user. <see langword="null" /> when the
    /// loader gives no message.
    /// </summary>
    public string? UserFriendlyMessage { get; init; }
    /// <summary>
    /// The children to show under the node. The tree view adds them and expands the node only when
    /// <see cref="IsSuccess" /> is <see langword="true" /> and the list is not empty.
    /// </summary>
    public IReadOnlyList<ITreeItemNode>? Data { get; init; }
}