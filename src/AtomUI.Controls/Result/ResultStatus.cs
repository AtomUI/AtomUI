namespace AtomUI.Controls;

/// <summary>
/// The outcome a result reports. It selects the status icon or the illustration shown above the header.
/// </summary>
public enum ResultStatus
{
    /// <summary>
    /// General information, neither a success nor a failure.
    /// </summary>
    Info,
    /// <summary>
    /// The operation succeeded.
    /// </summary>
    Success,
    /// <summary>
    /// The operation failed.
    /// </summary>
    Error,
    /// <summary>
    /// The result needs attention, without being reported as an error.
    /// </summary>
    Warning,
    /// <summary>
    /// The requested resource was not found (HTTP 404).
    /// </summary>
    ErrorCode404,
    /// <summary>
    /// Access to the requested resource is forbidden (HTTP 403).
    /// </summary>
    ErrorCode403,
    /// <summary>
    /// The server failed to handle the request (HTTP 500).
    /// </summary>
    ErrorCode500,
    /// <summary>
    /// The operation is not finished yet.
    /// </summary>
    Waiting
}