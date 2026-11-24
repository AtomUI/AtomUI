using AtomUI.DesignLanguages.Shadcn.TokenEvaluation;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Languages;
using AtomUI.Theme.Schema;

namespace AtomUI.DesignLanguages.Shadcn;

/// <summary>
/// The shadcn/ui language (ui.shadcn.com/create): eight styles (Nova by default), the default density and the compact
/// density (an AtomUI extension: one step smaller), the desktop idiom. The theme primary (<c>ColorPrimary</c>) and the
/// base colour (the language seed <see cref="ShadcnToken.BaseColor"/>) are the seeds an end user may adjust; every
/// shadcn colour is a stop of the Tailwind v4.3.5 palette.
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

    public override string Id => "Shadcn";

    public override DesignLanguageAxes Axes => s_axes;

    private static readonly IReadOnlyDictionary<string, string> s_defaultThemeSeeds =
        new System.Collections.ObjectModel.ReadOnlyDictionary<string, string>(new Dictionary<string, string>
        {
            [nameof(DesignToken.ColorPrimary)] = "#171717",
            [nameof(ShadcnToken.BaseColor)] = "#737373"
        });

    public override IReadOnlyDictionary<string, string> DefaultThemeSeeds => s_defaultThemeSeeds;

    /// <summary>
    /// The neutral theme on the neutral base colour (<c>ColorPrimary</c> neutral-900, <c>BaseColor</c> neutral-500)
    /// and no wave effect, in every style and appearance.
    /// </summary>
    public override void DefineSeeds(in DesignVariant variant, DesignToken seed, ShadcnToken languageSeed)
    {
        LanguageTokenRules.DefineSeeds(seed, languageSeed);
    }

    /// <summary>The base-colour variables of the appearance, the resolved theme and the style's tables.</summary>
    public override void DeriveLanguage(in DesignVariant variant, DesignToken seed, ShadcnToken language)
    {
        LanguageTokenRules.Apply(in variant, seed, language);
    }

    /// <summary>The shadcn map over the copied antd calculators, then the alias formulas with the shadcn variables.</summary>
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
