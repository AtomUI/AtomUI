using System.Collections.Frozen;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Languages;

namespace AtomUI.Theme.Schema;

/// <summary>
/// Registers one design language with the schema of its language token class. The generated
/// <c>GeneratedDesignLanguageRegistration</c> of a language package creates one per <c>[DesignLanguage]</c> class.
/// </summary>
public sealed class DesignLanguageDescriptor
{
    private readonly FrozenDictionary<string, TokenDescriptor> _languageTokensByName;

    /// <param name="language">The language; its methods must be pure functions of their inputs.</param>
    /// <param name="languageTokens">One descriptor per property of the language token class, ordered by slot.</param>
    public DesignLanguageDescriptor(DesignLanguage language, IReadOnlyList<TokenDescriptor> languageTokens)
    {
        ArgumentNullException.ThrowIfNull(language);
        ArgumentNullException.ThrowIfNull(languageTokens);
        SchemaIdentifier.Validate(language.Id, nameof(language));
        ArgumentNullException.ThrowIfNull(language.Axes);
        if (!language.Axes.Styles.Contains(language.Axes.DefaultStyle))
        {
            throw new ArgumentException(
                $"Design language '{language.Id}' declares a default style '{language.Axes.DefaultStyle}' it does not offer.",
                nameof(language));
        }

        for (var slot = 0; slot < languageTokens.Count; slot++)
        {
            if (languageTokens[slot].Slot != slot)
            {
                throw new ArgumentException("Language token descriptors must be ordered by slot.", nameof(languageTokens));
            }
        }

        Language              = language;
        LanguageTokens        = languageTokens.ToArray();
        _languageTokensByName = LanguageTokens.ToFrozenDictionary(static token => token.Name, StringComparer.Ordinal);
        // The language assembly's module version id changes whenever its code changes (deterministic builds), so the
        // revision needs no hand-maintained number.
        Revision = language.GetType().Assembly.ManifestModule.ModuleVersionId;
    }

    public DesignLanguage Language { get; }

    public string Id => Language.Id;

    public DesignLanguageAxes Axes => Language.Axes;

    public Type LanguageTokenType => Language.LanguageTokenType;

    /// <summary>The language tokens, ordered by slot.</summary>
    public IReadOnlyList<TokenDescriptor> LanguageTokens { get; }

    /// <summary>The identity of the language's code; part of every compiled-theme fingerprint and cache key.</summary>
    public Guid Revision { get; }

    /// <summary>Identifies this registration in compilation caches.</summary>
    internal object CompilationIdentity { get; } = new();

    public bool TryGetLanguageToken(string name, [System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out TokenDescriptor? descriptor) =>
        _languageTokensByName.TryGetValue(name, out descriptor);

    /// <summary>The language's default variant: its default style, light, the default density and the given idiom.</summary>
    public DesignVariant DefaultVariant(DesignIdiom idiom) =>
        new(Axes.DefaultStyle, ThemeAppearance.Light,
            Axes.Densities.Contains(DesignDensity.Default) ? DesignDensity.Default : Axes.Densities[0], idiom);

    internal LanguageDesignToken CreateLanguageToken() => Language.CreateLanguageToken();

    public override string ToString() => Id;
}
