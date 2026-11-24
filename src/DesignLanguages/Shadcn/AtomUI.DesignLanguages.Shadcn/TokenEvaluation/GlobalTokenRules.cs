using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Languages;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.DesignLanguages.Shadcn.TokenEvaluation;

/// <summary>
/// The shadcn Map and Alias tokens. The Map first runs the copied antd calculators (default, or dark over default)
/// so every Map token has a value, with the background and text bases taken from the shadcn variables, then writes
/// the shadcn variables and the style geometry over them; the compact density applies the copied antd compact
/// calculator to that result (an AtomUI extension: one step smaller). The Alias runs the copied antd alias formulas
/// with the shadcn variables merged in.
/// </summary>
internal static class GlobalTokenRules
{
    public static void Apply(in DesignVariant variant, ShadcnToken language, DesignToken token)
    {
        ApplyMap(in variant, language, token);
        AliasTokenRules.Apply(token, language);
        // The role tokens controls read instead of the seeds: the medium radius and the focused input border.
        token.BorderRadiusMD         = token.BorderRadius;
        token.InputActiveBorderColor = token.ColorPrimary;
    }

    private static void ApplyMap(in DesignVariant variant, ShadcnToken language, DesignToken token)
    {
        var bases = new ShadcnSeedBases(language.Background, language.Foreground);
        if (variant.IsDark)
        {
            new ShadcnDarkCalculator().Evaluate(in bases, false, token);
        }
        else
        {
            new ShadcnDefaultCalculator().Evaluate(in bases, false, token);
        }

        WriteColors(token, language);
        WriteSizes(token, language);

        // The dialog mask strength of the style.
        token.ColorBgMask = Color.FromArgb(ShadcnRecipes.AlphaByte(language.MaskAlpha), 0, 0, 0);

        if (variant.Density == DesignDensity.Compact)
        {
            new ShadcnCompactCalculator().Evaluate(in bases, true, token);
        }
    }

    private static void WriteColors(DesignToken token, ShadcnToken language)
    {
        // Every base-colour variable lands on its AtomUI token: the Map part here, the Alias part in
        // AliasTokenRules.
        token.ColorBgLayout    = language.Background;
        token.ColorBgContainer = language.Background;
        token.ColorBgElevated  = language.Popover;
        token.ColorBgSpotlight = language.Foreground;

        token.ColorText          = language.Foreground;
        token.ColorTextSecondary = language.MutedForeground;
        var mutedComposite = ShadcnRecipes.CompositeOver(language.MutedForeground, 0.5, language.Background);
        token.ColorTextTertiary   = mutedComposite;
        token.ColorTextQuaternary = mutedComposite;

        token.ColorFill           = language.Accent;
        token.ColorFillSecondary  = language.Secondary;
        token.ColorFillTertiary   = language.Muted;
        token.ColorFillQuaternary = language.Muted;

        token.ColorBorder          = language.Border;
        token.ColorBorderSecondary = language.Border;

        token.ColorError = language.Destructive;

        var primary = language.ThemePrimary;
        var background = language.Background;
        token.ColorPrimary = primary;
        // The brand family: hover:bg-primary/80 with the press being a translate, not a colour, so the hover and
        // active pair are the same 80% composite; the pale surfaces and borders are AtomUI proposals (shadcn has no
        // such variables).
        token.ColorPrimaryHover       = ShadcnRecipes.CompositeOver(primary, 0.8, background);
        token.ColorPrimaryActive      = token.ColorPrimaryHover;
        token.ColorPrimaryBg          = ShadcnRecipes.CompositeOver(primary, 0.1, background);
        token.ColorPrimaryBgHover     = ShadcnRecipes.CompositeOver(primary, 0.2, background);
        token.ColorPrimaryBorder      = ShadcnRecipes.CompositeOver(primary, 0.3, background);
        token.ColorPrimaryBorderHover = ShadcnRecipes.CompositeOver(primary, 0.5, background);
        token.ColorPrimaryText        = primary;
        token.ColorPrimaryTextHover   = token.ColorPrimaryHover;
        token.ColorPrimaryTextActive  = token.ColorPrimaryActive;
        // The solid surfaces take the theme primary and its 80% hover composite.
        token.ColorBgSolid       = primary;
        token.ColorBgSolidHover  = token.ColorPrimaryHover;
        token.ColorBgSolidActive = token.ColorBgSolidHover;
    }

    private static void WriteSizes(DesignToken token, ShadcnToken language)
    {
        token.ControlHeight   = language.StyleControlHeight;
        token.ControlHeightSM = language.StyleControlHeightSM;
        token.ControlHeightLG = language.StyleControlHeightLG;
        token.ControlHeightXS = language.StyleControlHeightXS;
        // The full radius ladder: the per-style button radius at every size, mirroring the official
        // one-radius-per-style classes.
        token.BorderRadius   = new CornerRadius(language.ButtonRadius);
        token.BorderRadiusSM = new CornerRadius(language.ButtonRadiusSM);
        token.BorderRadiusLG = new CornerRadius(language.ButtonRadiusLG);
        token.BorderRadiusXS = new CornerRadius(language.ButtonRadiusSM);
        token.FontSize       = language.StyleFontSize;
        token.FontSizeSM     = language.StyleFontSizeSM;
    }
}
