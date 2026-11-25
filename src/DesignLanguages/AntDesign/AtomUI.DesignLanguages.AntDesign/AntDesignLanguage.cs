using AtomUI.DesignLanguages.AntDesign.TokenEvaluation;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Languages;
using AtomUI.Theme.Schema;

namespace AtomUI.DesignLanguages.AntDesign;

/// <summary>
/// The Ant Design language (https://ant.design/docs/react/customize-theme): one style, the default and compact
/// densities, the desktop and mobile idioms; the primary colour is the seed an end user may adjust. The dark and
/// compact algorithms of antd are this language's rules for the dark appearance and the compact density.
/// </summary>
[DesignLanguage]
public sealed class AntDesignLanguage : DesignLanguage<AntDesignToken>
{
    /// <summary>Ant Design's single style.</summary>
    public static DesignStyle DefaultStyle { get; } = new("Default");

    private static readonly DesignLanguageAxes s_axes = new(
        [DefaultStyle],
        DefaultStyle,
        [DesignDensity.Default, DesignDensity.Compact],
        [DesignIdiom.Desktop, DesignIdiom.Mobile],
        AdjustableSeeds());

    public override string Id => "AntDesign";

    public override DesignLanguageAxes Axes => s_axes;

    private static readonly IReadOnlyDictionary<string, string> s_defaultThemeSeeds =
        new System.Collections.ObjectModel.ReadOnlyDictionary<string, string>(
            new Dictionary<string, string> { [nameof(DesignToken.ColorPrimary)] = "#1677FF" });

    public override IReadOnlyDictionary<string, string> DefaultThemeSeeds => s_defaultThemeSeeds;

    /// <summary>
    /// The global token's declared defaults are antd's seed tokens (colorPrimary #1677FF, borderRadius 6, fontSize 14,
    /// controlHeight 32, …), so the variant changes no seed.
    /// </summary>
    public override void DefineSeeds(in DesignVariant variant, DesignToken seed, AntDesignToken languageSeed)
    {
    }

    public override void DeriveLanguage(in DesignVariant variant, DesignToken seed, AntDesignToken language)
    {
    }

    /// <summary>
    /// antd's derivation: the default map algorithm, then the compact one for the compact density and the dark one for
    /// the dark appearance (each applied to the previous result, as antd composes them), then the alias tokens.
    /// </summary>
    public override void DeriveGlobal(in TokenEvaluationContext<AntDesignToken> context, DesignToken token)
    {
        MapTokenRules.Apply(context.Variant, token);
        AliasTokenRules.Apply(token);
        // The role tokens controls read instead of the seeds: the medium radius and the focused input border.
        token.BorderRadiusMD         = token.BorderRadius;
        token.InputActiveBorderColor = token.ColorPrimary;
    }

    private static IReadOnlyList<TokenDescriptor> AdjustableSeeds() => [GlobalTokenSchema.Get(nameof(DesignToken.ColorPrimary))];
}
