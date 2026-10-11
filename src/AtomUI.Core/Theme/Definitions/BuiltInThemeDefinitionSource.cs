using Avalonia.Platform;

namespace AtomUI.Theme.Definitions;

// Only this sealed Core-owned source may bypass runtime XML validation. The build
// worker uses the authoritative Reader/XSD; binding still uses the actual registry.
internal sealed class BuiltInThemeDefinitionSource : IThemeDefinitionSource
{
    private readonly Uri _uri;
    private readonly Lazy<ThemeDocumentReadResult> _readResult;

    internal BuiltInThemeDefinitionSource(string revision)
    {
        // Avalonia registers an avares UriParser during platform setup. Preserve the
        // original resolver's normalization timing both before and after that setup.
        _uri = new Uri("avares://AtomUI.Core/Assets/Themes/DaybreakBlue.theme.xml");
        SourceIdentity = _uri.ToString();
        SourceRevision = revision;
        _readResult = new Lazy<ThemeDocumentReadResult>(() => BuiltInThemeDocument.CreateReadResult(SourceIdentity));
    }

    public string SourceIdentity { get; }
    public string SourceRevision { get; }
    internal ThemeDocumentReadResult ReadResult => _readResult.Value;
    internal string ContentDigest => BuiltInThemeDocument.ContentDigest;

    // Keep the original resource contract available to source and AssetLoader callers.
    public Stream OpenRead() => AssetLoader.Open(_uri);
}
