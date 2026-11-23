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

internal sealed record ThemeTokenDocument(
    string Name,
    string Value,
    ThemeSourceLocation Location,
    bool Locked = false);

internal sealed class ControlThemeDocument
{
    internal ControlThemeDocument(
        ThemeControlDocumentIdentity identity,
        ThemeAppearance? appearance,
        string? density,
        IEnumerable<ThemeTokenDocument> tokens,
        ThemeSourceLocation location)
    {
        Identity   = identity;
        Appearance = appearance;
        Density    = density;
        Tokens     = Array.AsReadOnly(tokens.ToArray());
        Location   = location;
    }

    public ThemeControlDocumentIdentity Identity { get; }

    /// <summary>The appearance the control is fixed to; only allowed in the base section.</summary>
    public ThemeAppearance? Appearance { get; }

    /// <summary>The density the control is fixed to; only allowed in the base section.</summary>
    public string? Density { get; }

    public IReadOnlyList<ThemeTokenDocument> Tokens { get; }
    public ThemeSourceLocation Location { get; }
}

/// <summary>
/// The values of one section of a theme document: the base section or a <c>&lt;When&gt;</c> condition section.
/// </summary>
internal sealed class ThemeSectionDocument
{
    internal ThemeSectionDocument(
        IEnumerable<ThemeTokenDocument> tokens,
        IEnumerable<ThemeTokenDocument> languageTokens,
        IEnumerable<ControlThemeDocument> controls)
    {
        Tokens         = Array.AsReadOnly(tokens.ToArray());
        LanguageTokens = Array.AsReadOnly(languageTokens.ToArray());
        Controls       = Array.AsReadOnly(controls.ToArray());
    }

    public IReadOnlyList<ThemeTokenDocument> Tokens { get; }
    public IReadOnlyList<ThemeTokenDocument> LanguageTokens { get; }
    public IReadOnlyList<ControlThemeDocument> Controls { get; }

    public bool IsEmpty => Tokens.Count == 0 && LanguageTokens.Count == 0 && Controls.Count == 0;
}

/// <summary>A <c>&lt;When&gt;</c> section: the condition attributes it was written with and its values.</summary>
internal sealed class ThemeConditionDocument
{
    internal ThemeConditionDocument(
        string? style,
        ThemeAppearance? appearance,
        string? density,
        string? idiom,
        ThemeSectionDocument values,
        ThemeSourceLocation location)
    {
        Style      = style;
        Appearance = appearance;
        Density    = density;
        Idiom      = idiom;
        Values     = values;
        Location   = location;
    }

    public string? Style { get; }
    public ThemeAppearance? Appearance { get; }
    public string? Density { get; }
    public string? Idiom { get; }
    public ThemeSectionDocument Values { get; }
    public ThemeSourceLocation Location { get; }

    public bool HasCondition => Style is not null || Appearance is not null || Density is not null || Idiom is not null;
}

/// <summary>A theme definition XML document (format v2, spec 7.2), before binding to the registry.</summary>
internal sealed class ThemeDocument
{
    internal ThemeDocument(
        string id,
        string name,
        string language,
        string? style,
        ThemeAppearance? appearance,
        string? density,
        ThemeSectionDocument values,
        IEnumerable<ThemeConditionDocument> conditions,
        ThemeSourceLocation location)
    {
        Id         = id;
        Name       = name;
        Language   = language;
        Style      = style;
        Appearance = appearance;
        Density    = density;
        Values     = values;
        Conditions = Array.AsReadOnly(conditions.ToArray());
        Location   = location;
    }

    public string Id { get; }
    public string Name { get; }

    /// <summary>The design language the theme belongs to.</summary>
    public string Language { get; }

    /// <summary>The fixed style, or <see langword="null"/> when the theme leaves the style open.</summary>
    public string? Style { get; }

    /// <summary>The fixed appearance, or <see langword="null"/> when the theme leaves it open.</summary>
    public ThemeAppearance? Appearance { get; }

    /// <summary>The fixed density, or <see langword="null"/> when the theme leaves it open.</summary>
    public string? Density { get; }

    /// <summary>The base section.</summary>
    public ThemeSectionDocument Values { get; }

    /// <summary>The condition sections, in document order.</summary>
    public IReadOnlyList<ThemeConditionDocument> Conditions { get; }

    public ThemeSourceLocation Location { get; }

    // Convenience views of the base section.
    public IReadOnlyList<ThemeTokenDocument> Tokens => Values.Tokens;
    public IReadOnlyList<ThemeTokenDocument> LanguageTokens => Values.LanguageTokens;
    public IReadOnlyList<ControlThemeDocument> Controls => Values.Controls;
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
