namespace AtomUI.Theme.Definitions;

/// <summary>
/// One theme definition document offered to the theme catalog by a resolver.
/// </summary>
public interface IThemeDefinitionSource
{
    /// <summary>
    /// The identity of the document, such as its path or URI.
    /// </summary>
    /// <remarks>
    /// It must not be blank and must be unique among the sources of one resolver. The catalog reports it as the
    /// location of every diagnostic of the document.
    /// </remarks>
    string SourceIdentity { get; }
    /// <summary>
    /// The revision of the document as its source reports it, such as a version or a size and write time.
    /// </summary>
    /// <remarks>
    /// It must not be blank. The catalog never uses it to skip reading: every load reads the content again. A changed
    /// value still counts as a catalog change on reload.
    /// </remarks>
    string SourceRevision { get; }

    /// <summary>
    /// Opens a stream over the theme definition document.
    /// </summary>
    /// <returns>
    /// A readable stream. The catalog reads it to the end, to a limit of 4 MiB, and disposes it.
    /// </returns>
    /// <remarks>
    /// The stream only has to support reading. A catalog reload calls this method on a worker thread.
    /// </remarks>
    Stream OpenRead();
}
