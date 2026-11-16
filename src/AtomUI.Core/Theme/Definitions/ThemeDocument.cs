using AtomUI.Theme.Configuration;
using AtomUI.Theme.Algorithms;

namespace AtomUI.Theme.Definitions;

internal readonly record struct ThemeSourceLocation(
    string Source,
    int Line,
    int Column,
    string Path);

internal readonly record struct ThemeControlDocumentIdentity(string Catalog, string Id)
{
    public override string ToString() => $"{Catalog}:{Id}";
}

internal sealed record ThemeAlgorithmDocument(
    ThemeAlgorithm Algorithm,
    ThemeSourceLocation Location);

internal sealed record ThemeTokenDocument(
    string Name,
    string Value,
    ThemeSourceLocation Location);

internal sealed class ControlThemeDocument
{
    internal ControlThemeDocument(
        ThemeControlDocumentIdentity identity,
        ControlAlgorithmMode algorithmMode,
        IEnumerable<ThemeAlgorithmDocument> algorithms,
        IEnumerable<ThemeTokenDocument> tokens,
        ThemeSourceLocation location)
    {
        Identity      = identity;
        AlgorithmMode = algorithmMode;
        Algorithms    = Array.AsReadOnly(algorithms.ToArray());
        Tokens        = Array.AsReadOnly(tokens.ToArray());
        Location      = location;
    }

    public ThemeControlDocumentIdentity Identity { get; }
    public ControlAlgorithmMode AlgorithmMode { get; }
    public IReadOnlyList<ThemeAlgorithmDocument> Algorithms { get; }
    public IReadOnlyList<ThemeTokenDocument> Tokens { get; }
    public ThemeSourceLocation Location { get; }
}

internal sealed class ThemeDocument
{
    internal ThemeDocument(
        string id,
        string name,
        ThemeAppearance appearance,
        bool isDefault,
        IEnumerable<ThemeAlgorithmDocument> algorithms,
        IEnumerable<ThemeTokenDocument> tokens,
        IEnumerable<ControlThemeDocument> controls,
        ThemeSourceLocation location,
        string? designLanguage = null,
        ThemeDensity density = ThemeDensity.Normal,
        ThemeBrandColorSupport brandColor = ThemeBrandColorSupport.Fixed,
        ThemeNeutralColorSupport neutralColor = ThemeNeutralColorSupport.Fixed)
    {
        Id         = id;
        Name       = name;
        Appearance = appearance;
        IsDefault  = isDefault;
        Algorithms = Array.AsReadOnly(algorithms.ToArray());
        Tokens     = Array.AsReadOnly(tokens.ToArray());
        Controls   = Array.AsReadOnly(controls.ToArray());
        Location   = location;
        DesignLanguage = designLanguage;
        Density        = density;
        BrandColor     = brandColor;
        NeutralColor   = neutralColor;
    }

    public string Id { get; }
    public string? DesignLanguage { get; }
    public ThemeDensity Density { get; }
    public ThemeBrandColorSupport BrandColor { get; }
    public ThemeNeutralColorSupport NeutralColor { get; }
    public string Name { get; }
    public ThemeAppearance Appearance { get; }
    public bool IsDefault { get; }
    public IReadOnlyList<ThemeAlgorithmDocument> Algorithms { get; }
    public IReadOnlyList<ThemeTokenDocument> Tokens { get; }
    public IReadOnlyList<ControlThemeDocument> Controls { get; }
    public ThemeSourceLocation Location { get; }
}

internal sealed class ThemeDocumentReadResult
{
    internal ThemeDocumentReadResult(
        ThemeDocument? document,
        IEnumerable<ThemeDefinitionDiagnostic> diagnostics)
    {
        Document    = document;
        Diagnostics = Array.AsReadOnly(diagnostics.ToArray());
    }

    public ThemeDocument? Document { get; }
    public IReadOnlyList<ThemeDefinitionDiagnostic> Diagnostics { get; }

    public bool Success =>
        Document is not null &&
        Diagnostics.All(static diagnostic =>
            diagnostic.Severity != ThemeDefinitionDiagnosticSeverity.Error);
}
