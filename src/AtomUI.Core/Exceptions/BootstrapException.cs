namespace AtomUI.Exceptions;

/// <summary>
/// The exception that reports a failure while AtomUI is being started.
/// </summary>
public class BootstrapException : InvalidOperationException
{
    /// <summary>
    /// Creates the exception with an error text and no inner exception.
    /// </summary>
    /// <param name="message">
    /// The text that describes the failure, or <see langword="null" /> to use the default message of
    /// <see cref="InvalidOperationException" />.
    /// </param>
    public BootstrapException(string? message)
        : base(message)
    {
    }

    /// <summary>
    /// Creates the exception with an error text and the exception that caused it.
    /// </summary>
    /// <param name="message">
    /// The text that describes the failure, or <see langword="null" /> to use the default message of
    /// <see cref="InvalidOperationException" />.
    /// </param>
    /// <param name="innerException">
    /// The exception that caused this one, or <see langword="null" /> when there is none.
    /// </param>
    public BootstrapException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }
}