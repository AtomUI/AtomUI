using AtomUI.Controls;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// The outcome of one <see cref="AtomUI.Desktop.Controls.DataLoad.IMentionOptionsAsyncLoader.LoadAsync" />
/// call: the candidates to show, or a failure status.
/// </summary>
/// <remarks>
/// The control raises <see cref="Mentions.OptionsLoaded" /> with every result, but replaces its candidates
/// only when <see cref="IsSuccess" /> is <see langword="true" />.
/// </remarks>
public record MentionOptionsLoadResult
{
    /// <summary>
    /// Whether the load succeeded, that is whether <see cref="StatusCode" /> is
    /// <see cref="RpcStatusCode.Success" />.
    /// </summary>
    public bool IsSuccess => StatusCode == RpcStatusCode.Success;
    /// <summary>
    /// The status of the load. It defaults to <see cref="RpcStatusCode.Success" />, so a loader sets it only to
    /// report a failure. A result that the control builds for a failed request carries
    /// <see cref="RpcStatusCode.Timeout" />, <see cref="RpcStatusCode.Cancelled" /> or
    /// <see cref="RpcStatusCode.Unknown" />.
    /// </summary>
    public RpcStatusCode StatusCode { get; init; } = RpcStatusCode.Success;
    /// <summary>
    /// A message for the user that describes a failed load; <see langword="null" /> when there is none. For a
    /// request that failed without a result, the control sets it to the error message.
    /// </summary>
    public string? UserFriendlyMessage { get; init; }
    /// <summary>
    /// The loaded candidates. The control takes them over as its <see cref="Mentions.OptionsSource" /> when
    /// <see cref="IsSuccess" /> is <see langword="true" />; <see langword="null" /> then leaves the popup without
    /// candidates.
    /// </summary>
    public IReadOnlyList<IMentionOption>? Data { get; init; }
}