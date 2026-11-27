namespace AtomUI.Controls;

/// <summary>
/// Classifies the outcome of a remote request that loads data for a control. A load result carries it so that the
/// handlers of the load can tell the kinds of failure apart.
/// </summary>
public enum RpcStatusCode
{
    /// <summary>
    /// The request completed without an error.
    /// </summary>
    Success = 0,
    /// <summary>
    /// The request failed for a reason that no other value describes. A <c>CascaderItemLoadResult</c> whose status is
    /// not set carries this value.
    /// </summary>
    Unknown,
    
    /// <summary>
    /// The request was rejected, for example because an argument failed validation.
    /// </summary>
    InvalidRequest,
    /// <summary>
    /// Authentication failed: the request carried no valid credentials.
    /// </summary>
    Unauthorized,
    /// <summary>
    /// The caller is authenticated but lacks the permission that the request needs.
    /// </summary>
    Forbidden,
    /// <summary>
    /// The requested resource or endpoint does not exist.
    /// </summary>
    NotFound,
    
    /// <summary>
    /// The server could not be reached, for example because the connection was refused or the DNS lookup failed.
    /// </summary>
    NetworkFailure,
    /// <summary>
    /// The request did not complete within the time limit.
    /// </summary>
    Timeout,
    
    /// <summary>
    /// The server failed with an internal error.
    /// </summary>
    ServerInternal,
    /// <summary>
    /// The service is unavailable, for example because a circuit breaker is open or it is under maintenance.
    /// </summary>
    ServiceUnavailable,
    
    /// <summary>
    /// The request was canceled before it completed.
    /// </summary>
    Cancelled
}