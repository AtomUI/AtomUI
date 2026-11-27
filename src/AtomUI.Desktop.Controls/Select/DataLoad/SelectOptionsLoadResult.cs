using AtomUI.Controls;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// The outcome of an options load of a <see cref="Select" />, as reported by the
/// <see cref="Select.OptionsLoaded" /> event: the result that
/// <see cref="ISelectOptionsAsyncLoader.LoadAsync" /> returned, or a failure that the select recorded itself.
/// </summary>
public class SelectOptionsLoadResult
{
    /// <summary>
    /// Whether the load succeeded, that is whether <see cref="StatusCode" /> is <see cref="RpcStatusCode.Success" />.
    /// </summary>
    public bool IsSuccess => StatusCode == RpcStatusCode.Success;
    /// <summary>
    /// The status of the load. A failure that the select recorded itself carries
    /// <see cref="RpcStatusCode.Timeout" />, <see cref="RpcStatusCode.Cancelled" /> or
    /// <see cref="RpcStatusCode.Unknown" />.
    /// </summary>
    public RpcStatusCode StatusCode { get; init; } = RpcStatusCode.Success;
    /// <summary>
    /// A message about a failed load that is meant for the user. For a failure that the select recorded itself,
    /// it is the message of the error.
    /// </summary>
    public string? UserFriendlyMessage { get; init; }
    /// <summary>
    /// The loaded options. Once a load finishes with a result, the select takes them as its
    /// <see cref="Select.OptionsSource" /> and opens the dropdown only when the list has at least one item.
    /// </summary>
    public IReadOnlyList<ISelectOption>? Data { get; init; }
}