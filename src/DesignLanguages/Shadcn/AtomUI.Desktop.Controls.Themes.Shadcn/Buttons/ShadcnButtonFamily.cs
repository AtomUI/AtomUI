using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Languages;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>
/// The shadcn button recipe shared by Button, DropdownButton, SubmitButton and ResetButton (and the outline surfaces
/// FloatButton reuses): the focus ring as the focus border, the secondary surface, the destructive tints and the
/// outline variant's surfaces of each style.
/// </summary>
internal static class ShadcnButtonFamily
{
    private static bool UsesThemeLinkColour(DesignToken global) => global.ColorLink == global.ColorInfo;

    /// <summary>The global tokens the button family sees.</summary>
    public static void DeriveGlobal(in DerivationContext<ShadcnToken> context, DesignToken global)
    {
        var language = context.Language;
        global.BorderRadiusMD   = new CornerRadius(language.ButtonRadius);
        global.BorderRadiusLG   = new CornerRadius(language.ButtonRadiusLG);
        global.BorderRadiusSM   = new CornerRadius(language.ButtonRadiusSM);
        global.ColorFocusBorder = global.ColorControlOutline;
        global.LineWidthFocus   = global.ControlOutlineWidth;

        var dark = context.Variant.IsDark;
        // Official colour themes use zinc for the secondary surface; the neutral theme follows the base colour.
        var secondary = language.HasColorTheme ? ShadcnRecipes.TailwindStop("zinc", dark ? 800 : 100) : language.Secondary;
        global.ControlItemColorActive = language.HasColorTheme
            ? ShadcnRecipes.TailwindStop("zinc", dark ? 50 : 900)
            : language.SecondaryForeground;
        global.ColorFillTertiary  = secondary;
        global.ColorFillSecondary = ShadcnRecipes.MixOklch(secondary, global.ColorText, 0.05);

        global.ColorErrorBg      = ShadcnRecipes.MultiplyAlpha(global.ColorError, dark ? 0.2 : 0.1);
        global.ColorErrorBgHover = ShadcnRecipes.MultiplyAlpha(global.ColorError, dark ? 0.3 : 0.2);

        // Links take the theme primary in every state unless the application chose a link colour. Without a ColorLink
        // seed the copied antd map derives the link family from ColorInfo, so the two computed tokens are equal; an
        // explicit ColorLink makes them differ and keeps its own link family. The resting colour is the button's
        // LinkColor (DeriveToken).
        if (UsesThemeLinkColour(global))
        {
            global.ColorLinkHover  = global.ColorPrimary;
            global.ColorLinkActive = global.ColorPrimary;
        }
    }

    /// <summary>
    /// The button behaviour switches and surfaces of the style; written after the copied formulas, which keep reading
    /// their own defaults.
    /// </summary>
    public static void DeriveToken(in DerivationContext<ShadcnToken> context, DesignToken global, AbstractButtonGeometryToken token)
    {
        var style = context.Variant.Style;
        // Recipe colours, disabled at half opacity with their own colours, a 1px press, links underlined on hover;
        // Vega keeps its shadow on the outlined variant only, Sera also underlines links at rest.
        token.RecipeVariants           = true;
        token.OutlineOnlyDefaultShadow = style == ShadcnStyles.Vega;
        token.DisabledByOpacity        = true;
        token.DisabledOpacity          = 0.5;
        token.PressMoves               = true;
        token.UnderlineLinkOnHover     = true;
        token.UnderlineLinkAtRest      = style == ShadcnStyles.Sera;
        token.DefaultShadow = token.OutlineOnlyDefaultShadow ? ShadcnRecipes.ShadowXS : default;
        token.PrimaryShadow = default;
        token.DangerShadow  = default;
        if (UsesThemeLinkColour(global))
        {
            token.LinkColor = global.ColorPrimary;
        }

        var outline = ResolveOutline(in context, global);
        if (outline.Background is { } background)
        {
            token.DefaultBg = background;
        }
        token.DefaultHoverBg          = outline.HoverBackground;
        token.DefaultBorderColor      = outline.Border;
        token.DefaultHoverBorderColor = outline.Border;
        token.DefaultHoverColor       = global.ColorText;

        var dark = context.Variant.IsDark;
        token.TextHoverBg        = dark ? ShadcnRecipes.MultiplyAlpha(global.ColorFillContent, 0.5) : global.ColorFillContent;
        token.TextTextHoverColor = global.ColorText;
        token.LinkHoverBg        = Colors.Transparent;
        // Keep destructive text readable on a tinted hover surface in either appearance.
        token.DangerColor = dark
            ? ShadcnRecipes.CompositeOver(global.ColorError, 0.80, Colors.White)
            : ShadcnRecipes.CompositeOver(global.ColorError, 0.78, Colors.Black);
        if (token is AbstractStandardButtonToken standard)
        {
            standard.FontWeight = context.Language.ButtonWeight;
        }
    }

    /// <summary>
    /// The outline variant's resting background (<see langword="null"/>: the formula's), hover background and border
    /// of the style: <c>bg-background</c> / <c>dark:bg-input/30</c> and their hover shares.
    /// </summary>
    public static (Color? Background, Color HoverBackground, Color Border) ResolveOutline(
        in DerivationContext<ShadcnToken> context, DesignToken global)
    {
        var style = context.Variant.Style;
        var dark  = context.Variant.IsDark;
        var input = context.Language.Input;
        var darkInputOutline = dark && (style == ShadcnStyles.Nova || style == ShadcnStyles.Vega || style == ShadcnStyles.Lyra);
        var inputHover = darkInputOutline || style == ShadcnStyles.Maia || style == ShadcnStyles.Mira;

        Color? background = null;
        if (style == ShadcnStyles.Maia || darkInputOutline || (dark && style == ShadcnStyles.Mira))
        {
            background = ShadcnRecipes.MultiplyAlpha(input, 0.3);
        }
        else if (dark || style == ShadcnStyles.Mira || style == ShadcnStyles.Sera)
        {
            background = Colors.Transparent;
        }

        var hoverBackground = inputHover
            ? ShadcnRecipes.MultiplyAlpha(input, 0.5)
            : dark ? ShadcnRecipes.MultiplyAlpha(input, 0.3) : global.ColorFillContent;
        return (background, hoverBackground, darkInputOutline ? input : global.ColorBorder);
    }
}
