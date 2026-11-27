using AtomUI.Controls;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// The outcome of one <see cref="ICompleteOptionsAsyncLoader.LoadAsync" /> call: the options to show for the
/// typed text, or the reason why none were loaded.
/// </summary>
public class CompleteOptionsLoadResult
{
    /// <summary>
    /// Whether the load succeeded, that is whether <see cref="StatusCode" /> is <see cref="RpcStatusCode.Success" />.
    /// </summary>
    public bool IsSuccess => StatusCode == RpcStatusCode.Success;
    /// <summary>
    /// Outcome of the load as the loader reports it. The default is <see cref="RpcStatusCode.Success" />; another
    /// value states why no options were loaded.
    /// </summary>
    /// <remarks>
    /// The control passes the value on to handlers of <see cref="AbstractAutoComplete.OptionsLoaded" /> but does
    /// not act on it: it takes <see cref="Data" /> over as soon as the load returned a result. For a load that
    /// timed out, was canceled or threw, the control creates the result itself with the status
    /// <see cref="RpcStatusCode.Timeout" />, <see cref="RpcStatusCode.Cancelled" /> or
    /// <see cref="RpcStatusCode.Unknown" />.
    /// </remarks>
    public RpcStatusCode StatusCode { get; init; } = RpcStatusCode.Success;
    /// <summary>
    /// Message that describes a failed load to the user, or <see langword="null" /> when there is none. For a load
    /// that threw, was canceled or timed out, the control sets it to the message of the error, if there is one.
    /// </summary>
    public string? UserFriendlyMessage { get; init; }
    /// <summary>
    /// The options loaded for the typed text, or <see langword="null" /> when the load produced none.
    /// </summary>
    public IReadOnlyList<IAutoCompleteOption>? Data { get; init; }
}