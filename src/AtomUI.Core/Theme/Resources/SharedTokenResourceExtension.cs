namespace AtomUI.Theme.Resources;

/// <summary>
/// Markup extension that reads a shared Token as a dynamic resource. The resource key is the
/// <see cref="SharedTokenKind" /> value itself.
/// </summary>
public class SharedTokenResourceExtension : TokenResourceExtension<SharedTokenKind>
{
    public SharedTokenResourceExtension()
    {
    }

    /// <summary>
    /// Creates an extension that reads the shared Token <paramref name="kind" />.
    /// </summary>
    /// <param name="kind">
    /// The shared Token to read.
    /// </param>
    public SharedTokenResourceExtension(SharedTokenKind kind)
        : base(kind)
    {}

}
