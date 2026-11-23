using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;
using AtomUI.Theme;
using AtomUI.Theme.DesignTokens;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>
/// Slider under the PrimeNG presets, PrimeUIX 3.1.1 <c>slider</c>: the range (AtomUI track) is
/// <c>range.background</c> (the primary colour), the rail (AtomUI rail) <c>track.background</c> (Aura and Lara
/// surface.200 / 700, Nora surface.300 / 600) at <c>track.size</c> 3, neither changing on hover; the handle is
/// <c>handle.width</c> 20 (Lara and Nora 16) around a <c>handle.content</c> disc 4 smaller, drawn as a 2px ring in
/// <c>handle.background</c> (Aura surface.200 / 700, Lara and Nora the primary colour) that neither grows nor gets an
/// outline on hover.
/// </summary>
internal static class PrimeNGSliderTokens
{
    internal static void EvaluateGlobal(in DerivationContext<PrimeNGToken> context, DesignToken controlGlobal)
    {
    }
    internal static void EvaluateOwn(in DerivationContext<PrimeNGToken> context, DesignToken effectiveGlobal, SliderToken token)
    {
        var kind = context.Variant.Style;
        var dark = context.Variant.Appearance == ThemeAppearance.Dark;
        var rail = context.Language.GetSurfaceColor(kind == PrimeNGLanguage.Nora ? (dark ? 600 : 300) : (dark ? 700 : 200));
        token.RailBg       = rail;
        token.RailHoverBg  = rail;
        token.RailSize     = 3;
        token.TrackBg      = effectiveGlobal.ColorPrimary;
        token.TrackHoverBg = effectiveGlobal.ColorPrimary;

        var ring = kind == PrimeNGLanguage.Aura
            ? context.Language.GetSurfaceColor(dark ? 700 : 200)
            : effectiveGlobal.ColorPrimary;
        var border  = 2 * effectiveGlobal.LineWidth;
        var content = kind == PrimeNGLanguage.Aura ? 16 : 12;
        token.ThumbCircleSize                 = content;
        token.ThumbCircleSizeHover            = content;
        token.ThumbCircleBorderThickness      = new Thickness(border);
        token.ThumbCircleBorderThicknessHover = new Thickness(border);
        token.ThumbCircleBorderColor          = ring;
        token.ThumbCircleBorderHoverColor     = ring;
        token.ThumbCircleBorderActiveColor    = ring;
        // handle.focus.ring: Aura 1px primary, Lara a 3.2px primary.200 glow, Nora 2px primary (the global focus ring);
        // nothing on hover. AtomUI draws the ring against the handle (PrimeUIX offsets Aura's and Nora's by 2px).
        token.ThumbOutlineThickness           = new Thickness(effectiveGlobal.LineWidthFocus);
        token.ThumbOutlineColor               = effectiveGlobal.ColorFocusBorder;
        token.ThumbHoverOutlineThickness      = new Thickness(0);
        // handle.content.background: Aura and Lara content.background (white), Nora the primary colour.
        token.ThumbBg = kind == PrimeNGLanguage.Nora ? effectiveGlobal.ColorPrimary : effectiveGlobal.ColorBgContainer;
        // The thumb's bounds hold the focus ring as well (as the Ant formula does): a ring drawn past them is cut flat
        // whenever only the thumb's rectangle repaints, which squares the handle while it is dragged.
        token.ThumbSize = content + 2 * border + 2 * token.ThumbOutlineThickness.Left;
        token.SliderPaddingHorizontal = new Thickness(content / 2, (effectiveGlobal.ControlHeight - content) / 2);
        token.SliderPaddingVertical   = new Thickness((effectiveGlobal.ControlHeight - content) / 2, content / 2);
    }
}

/// <summary>
/// Rate under the PrimeNG presets, PrimeUIX 3.1.1 <c>rating</c>: <c>icon.size</c> 1rem (Lara 1.125rem) with the
/// other sizes in the same proportion, no hover enlargement, the on star <c>icon.active.color</c> (Aura and Lara the
/// primary colour, Nora text.color) and the off star <c>icon.color</c> as text.muted (PrimeNG draws the off star as
/// an outline; AtomUI fills it, so Lara's primary and Nora's text-coloured off stars would look selected).
/// </summary>
internal static class PrimeNGRateTokens
{
    internal static void EvaluateGlobal(in DerivationContext<PrimeNGToken> context, DesignToken controlGlobal)
    {
    }
    internal static void EvaluateOwn(in DerivationContext<PrimeNGToken> context, DesignToken effectiveGlobal, RateToken token)
    {
        var kind = context.Variant.Style;
        var size = kind == PrimeNGLanguage.Lara ? 18 : 16;
        token.StarSize       = size;
        token.StarSizeSM     = size * 0.8;
        token.StarSizeLG     = size * 1.2;
        token.StarHoverScale = 1;
        token.StarColor      = kind == PrimeNGLanguage.Nora ? effectiveGlobal.ColorText : effectiveGlobal.ColorPrimary;
        token.StarBg         = effectiveGlobal.ColorTextSecondary;
    }
}

/// <summary>
/// OtpLineEdit under the PrimeNG presets, PrimeUIX 3.1.1 <c>inputotp</c>: <c>input.width</c> 2.25rem (Lara 2.5rem),
/// sm 1.75rem (Lara 2rem), lg 2.625rem (Lara 3rem), <c>root.gap</c> 0.5rem.
/// </summary>
internal static class PrimeNGOtpLineEditTokens
{
    internal static void EvaluateGlobal(in DerivationContext<PrimeNGToken> context, DesignToken controlGlobal)
    {
        PrimeNGInputStage.Apply(context, controlGlobal);
    }
    internal static void EvaluateOwn(in DerivationContext<PrimeNGToken> context, DesignToken effectiveGlobal, OtpLineEditToken token)
    {
        var lara = context.Variant.Style == PrimeNGLanguage.Lara;
        token.CellWidth   = lara ? 40 : 36;
        token.CellWidthSM = lara ? 32 : 28;
        token.CellWidthLG = lara ? 48 : 42;
        token.CellGap     = 8;
    }
}

/// <summary>
/// ListBox under the PrimeNG presets, PrimeUIX 3.1.1 <c>listbox</c>: <c>option.padding</c> 4px 10px (Lara 8px 14px),
/// <c>list.padding</c>, option text <c>option.color</c> (text.color) and <c>option.focus.background</c> on hover.
/// </summary>
internal static class PrimeNGListBoxTokens
{
    internal static void EvaluateGlobal(in DerivationContext<PrimeNGToken> context, DesignToken controlGlobal)
    {
    }
    internal static void EvaluateOwn(in DerivationContext<PrimeNGToken> context, DesignToken effectiveGlobal, ListBoxToken token)
    {
        var recipes = context.Language.GetVisualRecipes(effectiveGlobal, context.Variant);
        var padding  = recipes.ListOptionPadding;
        var item     = context.Language.GetListItemRecipe(effectiveGlobal);
        token.ItemPadding      = new Thickness(padding.Left, 0);
        token.ContentPadding   = context.Language.GetSelectOverlayRecipe(effectiveGlobal, context.Variant).Padding;
        token.ItemColor        = effectiveGlobal.ColorText;
        token.ItemHoverColor   = item.HoverForeground;
        token.ItemHoverBgColor = item.HoverBackground;
        // The check mark of a selected item takes the selected text colour (PrimeUIX highlight.color): the primary check
        // vanishes on a solid primary highlight (noir, Nora), and on the pale highlight it is the primary 700 text.
        token.ItemSelectedIndicatorColor = token.ItemSelectedColor;
    }
}
