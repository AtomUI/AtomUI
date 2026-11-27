namespace AtomUI.Controls;

/// <summary>
/// Describes one file of an upload by its name, the source of its content and optional metadata. Applications pass
/// instances to the <c>EnqueueFilesAsync</c> method of an <c>Upload</c> control; its task events report the instance
/// of the task.
/// </summary>
public sealed class UploadFileInfo
{
    /// <summary>
    /// Gets the file name. It is never null, empty or white space. The admission check matches it against the name
    /// patterns of the accepted file types, and while <c>IsImageFilePredicate</c> is not set its extension decides
    /// whether the file counts as an image.
    /// </summary>
    public string Name { get; }
    /// <summary>
    /// Gets the location the file comes from, or <see langword="null" /> when it has none.
    /// The content to send is read from <see cref="Source" />, not from this location.
    /// </summary>
    public Uri? Path { get; }
    /// <summary>
    /// Gets the size of the file in bytes, or <see langword="null" /> when it is unknown.
    /// </summary>
    public long? Size { get; }
    /// <summary>
    /// Gets the MIME type of the content, or <see langword="null" /> when it is unknown. The admission check
    /// matches it against the MIME types of the accepted file types. Instances that an <c>Upload</c> control creates
    /// itself have no content type, so only instances created by the application can carry one.
    /// </summary>
    public string? ContentType { get; }
    /// <summary>
    /// Gets the creation time of the file, or <see langword="null" /> when it is unknown.
    /// </summary>
    public DateTimeOffset? DateCreated { get; }
    /// <summary>
    /// Gets the last modification time of the file, or <see langword="null" /> when it is unknown.
    /// </summary>
    public DateTimeOffset? DateModified { get; }
    /// <summary>
    /// Gets the source that provides the content to send. The upload transport reads it instead of <see cref="Path" />.
    /// </summary>
    public IUploadFileSource Source { get; }

    /// <summary>
    /// Creates the description of one file to upload from its name, its content source and optional metadata.
    /// </summary>
    /// <exception cref="ArgumentException">
    /// <paramref name="name" /> is null, empty or white space.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="source" /> is <see langword="null" />.
    /// </exception>
    public UploadFileInfo(
        string name,
        IUploadFileSource source,
        Uri? path = null,
        long? size = null,
        string? contentType = null,
        DateTimeOffset? dateCreated = null,
        DateTimeOffset? dateModified = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(source);

        Name         = name;
        Source       = source;
        Path         = path;
        Size         = size;
        ContentType  = contentType;
        DateCreated  = dateCreated;
        DateModified = dateModified;
    }
}
