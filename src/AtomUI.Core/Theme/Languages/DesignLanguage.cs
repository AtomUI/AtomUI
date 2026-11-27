using System.Collections.Frozen;
using AtomUI.Animations;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Schema;

namespace AtomUI.Theme.Languages;

/// <summary>
/// A design language: the one place that decides every value. It gives the seed defaults for a variant, derives its
/// own vocabulary (language tokens) and every Map and Alias token; per-control values come from its
/// control token rules (<see cref="IControlTokenRule{TLanguageToken,TToken}" />) in the value packages. Implement
/// <see cref="DesignLanguage{TLanguageToken}" />.
/// </summary>
public abstract class DesignLanguage
{
    private protected DesignLanguage()
    {
    }

    /// <summary>
    /// Stable identity, e.g. "Shadcn".
    /// </summary>
    public abstract string Id { get; }

    /// <summary>
    /// The styles, densities and idioms this language offers, its default style and adjustable seeds.
    /// </summary>
    public abstract DesignLanguageAxes Axes { get; }

    /// <summary>
    /// Declared adjustable seed text for the default theme's catalog entry, available before evaluation.
    /// </summary>
    public virtual IReadOnlyDictionary<string, string> DefaultThemeSeeds => FrozenDictionary<string, string>.Empty;

    /// <summary>
    /// The language token type; control token rules are found along its base types.
    /// </summary>
    public abstract Type LanguageTokenType { get; }

    /// <summary>
    /// The default feedback for the variant. Null selects the control's default.
    /// </summary>
    public virtual InteractionEffect? GetInteractionEffect(ControlTokenIdentity? owner, in DesignVariant variant) => null;

    /// <summary>
    /// Optional finite animations for this language's component slots.
    /// </summary>
    public virtual MotionScheme? MotionScheme => null;

    internal abstract LanguageDesignToken CreateLanguageToken();

    internal abstract void DefineSeedsCore(in DesignVariant variant, DesignToken seed, LanguageDesignToken languageSeed);

    internal abstract void DeriveLanguageCore(in DesignVariant variant, DesignToken seed, LanguageDesignToken language);

    internal abstract void DeriveGlobalCore(in DesignVariant variant, LanguageDesignToken language, DesignToken token);

    /// <summary>
    /// Returns the language identity.
    /// </summary>
    public override string ToString() => Id;
}

/// <inheritdoc />
/// <typeparam name="TLanguageToken">
/// The language's own token class.
/// </typeparam>
public abstract class DesignLanguage<TLanguageToken> : DesignLanguage
    where TLanguageToken : LanguageDesignToken, new()
{
    /// <inheritdoc />
    public sealed override Type LanguageTokenType => typeof(TLanguageToken);

    /// <summary>
    /// The language's default seeds for a variant: the global seeds and the language's own seeds.
    /// </summary>
    /// <param name="variant">
    /// The style, appearance, density and idiom being evaluated.
    /// </param>
    /// <param name="seed">
    /// A newly created global token; write the global seed defaults to it.
    /// </param>
    /// <param name="languageSeed">
    /// A newly created language token; write the language's own seed defaults to it.
    /// </param>
    /// <remarks>
    /// The theme compiler calls it first for every variant it evaluates. The explicit seed values of the theme
    /// configuration are applied over the defaults written here, before <see cref="DeriveLanguage" /> runs.
    /// </remarks>
    public abstract void DefineSeeds(in DesignVariant variant, DesignToken seed, TLanguageToken languageSeed);

    /// <summary>
    /// Writes every derived language token. Explicit values of those tokens are frozen before it runs.
    /// </summary>
    /// <param name="variant">
    /// The style, appearance, density and idiom being evaluated.
    /// </param>
    /// <param name="seed">
    /// The global seeds: the defaults of <see cref="DefineSeeds" /> with the explicit seed values of the theme
    /// configuration applied over them.
    /// </param>
    /// <param name="language">
    /// The language token that received the language's own seeds; write the derived language tokens to it.
    /// </param>
    /// <remarks>
    /// The theme compiler calls it after <see cref="DefineSeeds" /> and before <see cref="DeriveGlobal" />.
    /// </remarks>
    public abstract void DeriveLanguage(in DesignVariant variant, DesignToken seed, TLanguageToken language);

    /// <summary>
    /// Writes every Map and Alias token. Explicit values are frozen before it runs.
    /// </summary>
    public abstract void DeriveGlobal(in TokenEvaluationContext<TLanguageToken> context, DesignToken token);

    internal sealed override LanguageDesignToken CreateLanguageToken() => new TLanguageToken();

    internal sealed override void DefineSeedsCore(in DesignVariant variant, DesignToken seed, LanguageDesignToken languageSeed) =>
        DefineSeeds(in variant, seed, (TLanguageToken)languageSeed);

    internal sealed override void DeriveLanguageCore(in DesignVariant variant, DesignToken seed, LanguageDesignToken language) =>
        DeriveLanguage(in variant, seed, (TLanguageToken)language);

    internal sealed override void DeriveGlobalCore(in DesignVariant variant, LanguageDesignToken language, DesignToken token) =>
        DeriveGlobal(new TokenEvaluationContext<TLanguageToken>(variant, (TLanguageToken)language), token);
}
