using System.Collections.Frozen;
using AtomUI.Theme.DesignTokens;

namespace AtomUI.Theme.Languages;

/// <summary>
/// A design language: the one place that decides every value. It gives the seed defaults for a variant, derives its
/// own vocabulary (language tokens) and every Map and Alias token; per-control values come from its
/// control token rules (<see cref="IControlTokenRule{TLanguageToken,TToken}"/>) in the value packages. Implement
/// <see cref="DesignLanguage{TLanguageToken}"/>.
/// </summary>
public abstract class DesignLanguage
{
    private protected DesignLanguage()
    {
    }

    /// <summary>Stable identity, e.g. "Shadcn".</summary>
    public abstract string Id { get; }

    /// <summary>The styles, densities and idioms this language offers, its default style and adjustable seeds.</summary>
    public abstract DesignLanguageAxes Axes { get; }

    /// <summary>Declared adjustable seed text for the default theme's catalog entry, available before evaluation.</summary>
    public virtual IReadOnlyDictionary<string, string> DefaultThemeSeeds => FrozenDictionary<string, string>.Empty;

    /// <summary>The language token type; control token rules are found along its base types.</summary>
    public abstract Type LanguageTokenType { get; }

    internal abstract LanguageDesignToken CreateLanguageToken();

    internal abstract void DefineSeedsCore(in DesignVariant variant, DesignToken seed, LanguageDesignToken languageSeed);

    internal abstract void DeriveLanguageCore(in DesignVariant variant, DesignToken seed, LanguageDesignToken language);

    internal abstract void DeriveGlobalCore(in DesignVariant variant, LanguageDesignToken language, DesignToken token);

    public override string ToString() => Id;
}

/// <inheritdoc />
/// <typeparam name="TLanguageToken">The language's own token class.</typeparam>
public abstract class DesignLanguage<TLanguageToken> : DesignLanguage
    where TLanguageToken : LanguageDesignToken, new()
{
    public sealed override Type LanguageTokenType => typeof(TLanguageToken);

    /// <summary>The language's default seeds for a variant: the global seeds and the language's own seeds.</summary>
    public abstract void DefineSeeds(in DesignVariant variant, DesignToken seed, TLanguageToken languageSeed);

    /// <summary>Writes every derived language token.</summary>
    public abstract void DeriveLanguage(in DesignVariant variant, DesignToken seed, TLanguageToken language);

    /// <summary>Writes every Map and Alias token. Explicit values are frozen before it runs.</summary>
    public abstract void DeriveGlobal(in TokenEvaluationContext<TLanguageToken> context, DesignToken token);

    internal sealed override LanguageDesignToken CreateLanguageToken() => new TLanguageToken();

    internal sealed override void DefineSeedsCore(in DesignVariant variant, DesignToken seed, LanguageDesignToken languageSeed) =>
        DefineSeeds(in variant, seed, (TLanguageToken)languageSeed);

    internal sealed override void DeriveLanguageCore(in DesignVariant variant, DesignToken seed, LanguageDesignToken language) =>
        DeriveLanguage(in variant, seed, (TLanguageToken)language);

    internal sealed override void DeriveGlobalCore(in DesignVariant variant, LanguageDesignToken language, DesignToken token) =>
        DeriveGlobal(new TokenEvaluationContext<TLanguageToken>(variant, (TLanguageToken)language), token);
}
