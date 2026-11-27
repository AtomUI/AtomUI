namespace AtomUI.Desktop.Controls;

/// <summary>
/// The exception thrown when a <see cref="CompactSpaceFiller" /> is not the last item of a
/// <see cref="CompactSpace" />, or when there is more than one of them.
/// </summary>
public class InvalidSpaceFillerUsageException : SystemException
{
    /// <summary>
    /// Creates the exception with the message that describes the invalid usage.
    /// </summary>
    /// <param name="message">
    /// The text that describes how the filler is used incorrectly.
    /// </param>
    public InvalidSpaceFillerUsageException(string message) : base(message)
    {
    }
}