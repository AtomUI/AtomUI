using AtomUI.DesignLanguages.Shadcn.TokenEvaluation;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Languages;
using AtomUI.Theme.Schema;

namespace AtomUI.DesignLanguages.Shadcn;

/// <summary>
/// The shadcn/ui language (ui.shadcn.com/create): eight styles (Nova by default), the default density and the compact
/// density (an AtomUI extension: one step smaller), the desktop idiom. The theme primary (<c>ColorPrimary</c>) and the
/// base color (the language seed <see cref="ShadcnToken.BaseColor" />) are the seeds an end user may adjust; every
/// shadcn color is a stop of the Tailwind v4.3.5 palette.
/// </summary>
[DesignLanguage]
public sealed class ShadcnLanguage : DesignLanguage<ShadcnToken>
{
    private static readonly DesignLanguageAxes s_axes = new(
        ShadcnStyles.All,
        ShadcnStyles.Nova,
        [DesignDensity.Default, DesignDensity.Compact],
        [DesignIdiom.Desktop],
        AdjustableSeeds());

    /// <summary>
    /// The language's stable id, <c>Shadcn</c>. It is the same for every style, density and appearance.
    /// </summary>
    public override string Id => "Shadcn";

    /// <summary>
    /// The motion scheme for Shadcn transitions and component animations.
    /// </summary>
    public override AtomUI.Animations.MotionScheme MotionScheme => Motions.Default;

    private static class Motions
    {
        internal static readonly ShadcnMotionScheme Default = new();
    }

    /// <summary>
    /// Returns the default interaction effect for Shadcn controls.
    /// </summary>
    public override AtomUI.Animations.InteractionEffect GetInteractionEffect(AtomUI.Theme.Schema.ControlTokenIdentity? owner,
        in DesignVariant variant) =>
        AtomUI.Animations.InteractionEffect.None;

    /// <summary>
    /// The axes of the language and their defaults: the styles with the default style, the densities, the desktop
    /// idiom, and the seeds an end user may adjust: <c>ColorPrimary</c> and
    /// <see cref="ShadcnToken.BaseColor" />.
    /// </summary>
    public override DesignLanguageAxes Axes => s_axes;

    private static readonly IReadOnlyDictionary<string, string> s_defaultThemeSeeds =
        new System.Collections.ObjectModel.ReadOnlyDictionary<string, string>(new Dictionary<string, string>
        {
            [nameof(DesignToken.ColorPrimary)] = "#171717",
            [nameof(ShadcnToken.BaseColor)] = "#737373"
        });

    /// <summary>
    /// The seeds of the language's default theme, keyed by token name: the neutral theme primary
    /// (<c>ColorPrimary</c>) and the neutral base color (<see cref="ShadcnToken.BaseColor" />). The theme
    /// catalog takes the accent color of that theme from the <c>ColorPrimary</c> entry.
    /// </summary>
    public override IReadOnlyDictionary<string, string> DefaultThemeSeeds => s_defaultThemeSeeds;

    /// <summary>
    /// The neutral theme on the neutral base color (<c>ColorPrimary</c> neutral-900, <c>BaseColor</c> neutral-500)
    /// and no wave effect, in every style and appearance.
    /// </summary>
    public override void DefineSeeds(in DesignVariant variant, DesignToken seed, ShadcnToken languageSeed)
    {
        LanguageTokenRules.DefineSeeds(seed, languageSeed);
    }

    /// <summary>
    /// The base-color variables of the appearance, the resolved theme and the style's tables.
    /// </summary>
    public override void DeriveLanguage(in DesignVariant variant, DesignToken seed, ShadcnToken language)
    {
        LanguageTokenRules.Apply(in variant, seed, language);
    }

    /// <summary>
    /// The shadcn map over the copied antd calculators, then the alias formulas with the shadcn variables.
    /// </summary>
    public override void DeriveGlobal(in TokenEvaluationContext<ShadcnToken> context, DesignToken token)
    {
        GlobalTokenRules.Apply(context.Variant, context.Language, token);
    }

    private static IReadOnlyList<TokenDescriptor> AdjustableSeeds() =>
    [
        GlobalTokenSchema.Get(nameof(DesignToken.ColorPrimary)),
        Generated.AtomUIDesignLanguagesShadcn.GeneratedLanguageTokenSchema.Tokens_AtomUI_DesignLanguages_Shadcn_ShadcnToken()
                 .Single(static token => token.Name == nameof(ShadcnToken.BaseColor))
    ];
}
