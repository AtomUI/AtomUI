namespace AtomUI.Controls;

/// <summary>
/// Provides the content of a file that is queued for upload as a stream, so that a transport does not
/// need to know where the content is stored.
/// </summary>
public interface IUploadFileSource
{
    /// <summary>
    /// Opens the content of the file for reading.
    /// </summary>
    /// <param name="cancellationToken">
    /// Cancels the wait for the stream to be opened.
    /// </param>
    /// <returns>
    /// A task whose result is a stream to read the content from.
    /// </returns>
    ValueTask<Stream> OpenReadAsync(CancellationToken cancellationToken = default);
}
