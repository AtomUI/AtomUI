using System.Collections.Frozen;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Languages;

namespace AtomUI.Theme.Schema;

/// <summary>
/// Registers one design language with the schema of its language token class. The generated
/// <c>DesignLanguageRegistration</c> of a language package creates one per <c>[DesignLanguage]</c> class.
/// </summary>
public sealed class DesignLanguageDescriptor
{
    private readonly FrozenDictionary<string, TokenDescriptor> _languageTokensByName;

    /// <summary>
    /// Registers <paramref name="language" /> with the schema of its language Token class.
    /// </summary>
    /// <param name="language">
    /// The language; its methods must be pure functions of their inputs.
    /// </param>
    /// <param name="languageTokens">
    /// One descriptor per property of the language Token class; the descriptor at index <c>i</c> must have
    /// slot <c>i</c>.
    /// </param>
    /// <exception cref="ArgumentException">
    /// The id of <paramref name="language" /> is blank or not a valid theme schema identifier, its default style
    /// is not among the styles it offers, or the slots of <paramref name="languageTokens" /> do not match their
    /// indexes.
    /// </exception>
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

    /// <summary>
    /// The design language this descriptor registers; the descriptor takes <see cref="Id" />, <see cref="Axes" />
    /// and <see cref="LanguageTokenType" /> from it. Its methods derive the language Token values and must be pure
    /// functions of their inputs.
    /// </summary>
    public DesignLanguage Language { get; }

    /// <summary>
    /// The id of <see cref="Language" />. Theme configuration and diagnostics use it to name the language.
    /// </summary>
    public string Id => Language.Id;

    /// <summary>
    /// The axes the language offers, such as its styles, densities and idioms, and its default style.
    /// </summary>
    public DesignLanguageAxes Axes => Language.Axes;

    /// <summary>
    /// The type of the language's Token class. Together with a control identity it is the key that finds the
    /// control Token rule of this language for that control.
    /// </summary>
    public Type LanguageTokenType => Language.LanguageTokenType;

    /// <summary>
    /// The language tokens, ordered by slot.
    /// </summary>
    public IReadOnlyList<TokenDescriptor> LanguageTokens { get; }

    /// <summary>
    /// The identity of the language's code; part of every compiled-theme fingerprint and cache key.
    /// </summary>
    public Guid Revision { get; }

    /// <summary>
    /// Identifies this registration in compilation caches.
    /// </summary>
    internal object CompilationIdentity { get; } = new();

    /// <summary>
    /// Finds the language Token descriptor with the given name.
    /// </summary>
    /// <param name="name">
    /// The name of a Token of the language Token class; compared ordinally.
    /// </param>
    /// <param name="descriptor">
    /// The descriptor of that Token, or <see langword="null" /> when none has that name.
    /// </param>
    /// <returns>
    /// <see langword="true" /> when the language Token class declares a Token with that name.
    /// </returns>
    public bool TryGetLanguageToken(string name, [System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out TokenDescriptor? descriptor) =>
        _languageTokensByName.TryGetValue(name, out descriptor);

    /// <summary>
    /// The language's default variant: its default style, light, the default density and the given idiom.
    /// </summary>
    public DesignVariant DefaultVariant(DesignIdiom idiom) =>
        new(Axes.DefaultStyle, ThemeAppearance.Light,
            Axes.Densities.Contains(DesignDensity.Default) ? DesignDensity.Default : Axes.Densities[0], idiom);

    internal LanguageDesignToken CreateLanguageToken() => Language.CreateLanguageToken();

    /// <summary>
    /// Returns the language id.
    /// </summary>
    public override string ToString() => Id;
}
