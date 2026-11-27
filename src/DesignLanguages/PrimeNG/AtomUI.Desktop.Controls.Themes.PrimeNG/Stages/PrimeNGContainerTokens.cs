using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;
using AtomUI.Theme;
using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>
/// Line tabs (TabControl, TabStrip) under the PrimeNG presets, PrimeUIX 3.1.1 <c>tabs</c>: the rest tab text is
/// <c>tab.color</c> (text.muted), hover <c>tab.hover.color</c> (text.color); <c>tab.padding</c> 14px 16px (Lara 14px
/// 18px) with no margin between tabs, so the active bar spans the padded tab; the active bar is
/// <c>active.bar.height</c> 1px in Aura (Lara draws a 2px accent and Nora a filled tab: the 2px bar is kept for them).
/// The tab list border is <c>tablist.border.color</c> (content.border.color); Lara has none.
/// </summary>
internal static class PrimeNGTabStage
{
    internal static void ApplyGlobal(in TokenEvaluationContext<PrimeNGToken> context, DesignToken controlGlobal)
    {
        // The ink bar slides over 250ms in every preset (tabs: width / inset 250ms); the tab colors stay instant.
        controlGlobal.MotionDurationSlow = PrimeNGMotion.TabsInkBar;
        // tablist.border.color is content.border.color; Lara has none.
        controlGlobal.ColorBorderSecondary = context.Variant.Style == PrimeNGLanguage.Lara
            ? Colors.Transparent
            : context.Language.GetContentBorder(context.Variant);
    }

    internal static void ApplyLine(in TokenEvaluationContext<PrimeNGToken> context, DesignToken effectiveGlobal, AbstractLineTabToken token)
    {
        var kind = context.Variant.Style;
        // tab.color is text.muted and tab.hover.color text.color; the title weight is 600 in Aura and 700 in Lara
        // and Nora.
        token.ItemColor             = effectiveGlobal.ColorTextSecondary;
        token.ItemHoverColor        = effectiveGlobal.ColorText;
        token.TitleFontWeight       = kind == PrimeNGLanguage.Aura ? FontWeight.SemiBold : FontWeight.Bold;
        // tab.padding 14px 16px (Lara 14px 18px) with no margin between the tabs; the small and large sizes keep
        // that horizontal padding 4px narrower / wider (PrimeUIX has one size), since the items have no gutter.
        var paddingX = kind == PrimeNGLanguage.Lara ? 18 : 16;
        token.HorizontalItemPadding   = new Thickness(paddingX, 14);
        token.HorizontalItemPaddingSM = new Thickness(paddingX - 4, 10);
        token.HorizontalItemPaddingLG = new Thickness(paddingX + 4, 18);
        token.HorizontalItemGutter    = 0;
        // active.bar.height 1px in Aura; Lara's 2px accent is kept for Lara and Nora (Nora fills the tab).
        token.InkBarThickness       = kind == PrimeNGLanguage.Aura ? effectiveGlobal.LineWidth : 2 * effectiveGlobal.LineWidth;
    }
}

/// <summary>
/// PrimeUIX <c>tabs</c> for <see cref="TabControlToken" />; see <see cref="PrimeNGTabStage" />.
/// </summary>
internal static class PrimeNGTabControlTokens
{
    internal static void EvaluateGlobal(in TokenEvaluationContext<PrimeNGToken> context, DesignToken controlGlobal) =>
        PrimeNGTabStage.ApplyGlobal(context, controlGlobal);
    internal static void EvaluateOwn(in TokenEvaluationContext<PrimeNGToken> context, DesignToken effectiveGlobal, TabControlToken token) =>
        PrimeNGTabStage.ApplyLine(context, effectiveGlobal, token);
}

/// <summary>
/// PrimeUIX <c>tabs</c> for <see cref="TabStripToken" />; see <see cref="PrimeNGTabStage" />.
/// </summary>
internal static class PrimeNGTabStripTokens
{
    internal static void EvaluateGlobal(in TokenEvaluationContext<PrimeNGToken> context, DesignToken controlGlobal) =>
        PrimeNGTabStage.ApplyGlobal(context, controlGlobal);
    internal static void EvaluateOwn(in TokenEvaluationContext<PrimeNGToken> context, DesignToken effectiveGlobal, TabStripToken token) =>
        PrimeNGTabStage.ApplyLine(context, effectiveGlobal, token);
}

/// <summary>
/// Collapse under the PrimeNG presets, PrimeUIX 3.1.1 <c>accordion</c>: header background
/// <c>header.background</c> (Aura and Nora the content background, Lara surface.50 / surface.800), content background
/// <c>content.background</c> (dark: surface.900), a header padding of 6px 16px (Lara 4px 16px; PrimeUIX
/// <c>header.padding</c> 16, Nora 14px 16px, see the Expander), <c>content.padding</c> 0 16 16 16 in Aura (no gap
/// under the header) and 16 in Lara and Nora, borders in <c>content.border.color</c> and corners at the <c>md</c>
/// step of <c>border.radius</c>.
/// </summary>
internal static class PrimeNGCollapseTokens
{
    internal static void EvaluateGlobal(in TokenEvaluationContext<PrimeNGToken> context, DesignToken controlGlobal)
    {
        controlGlobal.MotionDurationSlow = PrimeNGMotion.Collapsible;
        controlGlobal.ColorBorder = context.Language.GetContentBorder(context.Variant);
    }
    internal static void EvaluateOwn(in TokenEvaluationContext<PrimeNGToken> context, DesignToken effectiveGlobal, CollapseToken token)
    {
        var kind = context.Variant.Style;
        var dark = context.Variant.Appearance == ThemeAppearance.Dark;
        // accordion.header.background: Lara surface.50 / surface.800, Aura and Nora the content background; the
        // content is content.background (dark surface.900).
        token.HeaderBg = kind == PrimeNGLanguage.Lara
            ? context.Language.GetSurfaceColor(dark ? 800 : 50)
            : effectiveGlobal.ColorBgElevated;
        token.ContentBg     = effectiveGlobal.ColorBgElevated;
        // PrimeUIX accordion header.padding is 16 (Nora 14px 16px), which makes a panel header about 50px, far taller
        // than the Expander's (panel toggleable header 6px 16px, Lara 4px 16px). AtomUI gives both the Expander's padding
        // so the two collapsible headers line up (a deliberate deviation).
        token.HeaderPadding = new Thickness(16, kind == PrimeNGLanguage.Lara ? 4 : 6);
        // accordion.content.padding 16 (Aura 0 16 16 16: no gap under the header) and the md step of border.radius.
        token.ContentPadding = kind == PrimeNGLanguage.Aura ? new Thickness(16, 0, 16, 16) : new Thickness(16);
        token.CollapsePanelBorderRadius = effectiveGlobal.BorderRadius;
        // header.color text.muted (Nora text.color), header.active.color text.color.
        token.HeaderColor       = kind == PrimeNGLanguage.Nora ? effectiveGlobal.ColorText : effectiveGlobal.ColorTextSecondary;
        // accordion header.font.weight 600 (Lara and Nora 700).
        token.HeaderFontWeight  = kind == PrimeNGLanguage.Aura ? FontWeight.SemiBold : FontWeight.Bold;
        token.HeaderActiveColor = effectiveGlobal.ColorText;
    }
}

/// <summary>
/// Expander under the PrimeNG presets, PrimeUIX 3.1.1 <c>panel</c> (toggleable): header background
/// <c>header.background</c> (Lara surface.50 / surface.800, Aura and Nora the content background),
/// <c>toggleable.header.padding</c> 6px 16px (Lara 4px 16px), <c>content.padding</c> 0 16 16 16 (Lara and Nora 16),
/// border <c>root.border.color</c> (content.border.color) and corners at the <c>md</c> step of <c>border.radius</c>.
/// </summary>
internal static class PrimeNGExpanderTokens
{
    internal static void EvaluateGlobal(in TokenEvaluationContext<PrimeNGToken> context, DesignToken controlGlobal)
    {
        controlGlobal.MotionDurationSlow = PrimeNGMotion.Collapsible;
        controlGlobal.ColorBorder = context.Language.GetContentBorder(context.Variant);
    }
    internal static void EvaluateOwn(in TokenEvaluationContext<PrimeNGToken> context, DesignToken effectiveGlobal, ExpanderToken token)
    {
        var kind = context.Variant.Style;
        var lara = kind == PrimeNGLanguage.Lara;
        var dark = context.Variant.Appearance == ThemeAppearance.Dark;
        // panel.toggleable: header.background (Lara surface.50 / surface.800, Aura and Nora the content background),
        // toggleable.header.padding 6px 16px (Lara 4px 16px), content.padding 0 16 16 16 (Aura) or 16, and the md
        // step of border.radius.
        token.HeaderBg       = lara ? context.Language.GetSurfaceColor(dark ? 800 : 50) : effectiveGlobal.ColorBgElevated;
        token.ContentBg      = effectiveGlobal.ColorBgElevated;
        token.HeaderPadding  = new Thickness(16, lara ? 4 : 6);
        token.ContentPadding = kind == PrimeNGLanguage.Aura ? new Thickness(16, 0, 16, 16) : new Thickness(16);
        token.ExpanderBorderRadius = effectiveGlobal.BorderRadius;
        // panel.header.color text.color.
        token.HeaderColor = effectiveGlobal.ColorText;
        // accordion header.font.weight 600 (Lara and Nora 700), the weight the Collapse takes as well.
        token.HeaderFontWeight = kind == PrimeNGLanguage.Aura ? FontWeight.SemiBold : FontWeight.Bold;
    }
}

/// <summary>
/// GroupBox under the PrimeNG presets, PrimeUIX 3.1.1 <c>fieldset</c>: border <c>border.color</c>
/// (content.border.color), background <c>background</c> (content.background), <c>legend.padding</c> 6px 10px (Lara
/// 8px 12px), <c>legend.gap</c> 8 and <c>root.padding</c> 0 16 16 16 (Lara and Nora 10 16 16 16).
/// </summary>
internal static class PrimeNGGroupBoxTokens
{
    internal static void EvaluateGlobal(in TokenEvaluationContext<PrimeNGToken> context, DesignToken controlGlobal)
    {
        controlGlobal.ColorBorder      = context.Language.GetContentBorder(context.Variant);
        controlGlobal.ColorBgContainer = controlGlobal.ColorBgElevated;
    }
    internal static void EvaluateOwn(in TokenEvaluationContext<PrimeNGToken> context, DesignToken effectiveGlobal, GroupBoxToken token)
    {
        var kind = context.Variant.Style;
        // fieldset.legend.padding 6px 10px (Lara 8px 12px), legend.gap 8, and root.padding 0 16 16 16 (Lara and
        // Nora 10 16 16 16).
        token.HeaderContentPadding = kind == PrimeNGLanguage.Lara ? new Thickness(12, 8) : new Thickness(10, 6);
        token.HeaderIconMargin     = new Thickness(0, 0, 8, 0);
        // fieldset legend.font.weight 600 (Lara and Nora 700).
        token.HeaderFontWeight     = kind == PrimeNGLanguage.Aura ? FontWeight.SemiBold : FontWeight.Bold;
        token.ContentPadding       = new Thickness(16, kind == PrimeNGLanguage.Aura ? 0 : 10, 16, 16);
    }
}

/// <summary>
/// Breadcrumb under the PrimeNG presets, PrimeUIX 3.1.1 <c>breadcrumb</c>: every item is <c>item.color</c>
/// (text.muted, the last one too) and hovers to <c>item.hover.color</c> without a background; the separator is
/// <c>separator.color</c> (Aura and Lara surface.400 / surface.500, Nora surface.600 / surface.400) with
/// <c>root.gap</c> 8 on each side, and links have no padding.
/// </summary>
internal static class PrimeNGBreadcrumbTokens
{
    internal static void EvaluateGlobal(in TokenEvaluationContext<PrimeNGToken> context, DesignToken controlGlobal)
    {
    }
    internal static void EvaluateOwn(in TokenEvaluationContext<PrimeNGToken> context, DesignToken effectiveGlobal, BreadcrumbToken token)
    {
        var nora = context.Variant.Style == PrimeNGLanguage.Nora;
        var dark = context.Variant.Appearance == ThemeAppearance.Dark;
        // breadcrumb: every item in item.color (text.muted, the last one too), no hover background, and links without
        // padding; separator.color (Aura and Lara surface.400 / surface.500, Nora surface.600 / surface.400) with
        // root.gap 8 on each side.
        token.SeparatorColor = context.Language.GetSurfaceColor(nora ? (dark ? 400 : 600) : (dark ? 500 : 400));
        token.ItemColor                    = effectiveGlobal.ColorTextSecondary;
        token.LinkColor                    = effectiveGlobal.ColorTextSecondary;
        token.LastItemColor                = effectiveGlobal.ColorTextSecondary;
        token.LinkHoverColor               = effectiveGlobal.ColorText;
        token.LinkHoverBgColor             = Colors.Transparent;
        token.SeparatorMargin              = new Thickness(8, 0);
        token.BreadcrumbItemContentPadding = new Thickness(0);
    }
}

/// <summary>
/// Splitter under the PrimeNG presets, PrimeUIX 3.1.1 <c>splitter</c>: a 1px gutter in <c>gutter.background</c>
/// (content.border.color) that does not change on hover or drag, a 24 handle, and a handle grip in Lara's
/// <c>handle.background</c> (surface.400 / surface.600). Aura and Nora's handle is transparent, which leaves nothing to
/// see or aim at on the 1px gutter: AtomUI draws Lara's grip in every preset (a usability deviation), and under Nora,
/// whose gutter is already surface 400 / 500, two stops further toward the text (600 / dark 300).
/// </summary>
internal static class PrimeNGSplitterTokens
{
    internal static void EvaluateGlobal(in TokenEvaluationContext<PrimeNGToken> context, DesignToken controlGlobal)
    {
    }
    internal static void EvaluateOwn(in TokenEvaluationContext<PrimeNGToken> context, DesignToken effectiveGlobal, SplitterToken token)
    {
        var line = context.Language.GetContentBorder(context.Variant);
        // splitter.gutter.background (content.border.color), 1px and the same on hover and drag; a 24 handle.
        token.HandleLineColor      = line;
        token.HandleLineHoverColor = line;
        token.HandleLineDragColor  = line;
        token.HandleLineThickness  = effectiveGlobal.LineWidth;
        token.SplitBarHandleSize   = 24;
        // Nora's gutter is already surface 400 / 500 (its content.border.color), so the grip goes two stops further
        // toward the text (600 / dark 300) to stay distinct from it.
        var dark = context.Variant.Appearance == ThemeAppearance.Dark;
        var grip = context.Variant.Style == PrimeNGLanguage.Nora
            ? context.Language.GetSurfaceColor(dark ? 300 : 600)
            : context.Language.GetSurfaceColor(dark ? 600 : 400);
        token.HandleGripColor      = grip;
        token.HandleGripHoverColor = grip;
        token.HandleGripDragColor  = grip;
    }
}
