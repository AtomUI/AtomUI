using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;
using AtomUI.Theme;
using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>
/// The PrimeUIX severity colors of the compact badges and tags (<c>tag</c>, <c>badge</c>): each status palette is
/// recovered from the global status color and the stop the preset writes it at.
/// </summary>
internal static class PrimeNGSeverity
{
    internal static PrimeNGSeverityScale Error(in TokenEvaluationContext<PrimeNGToken> context, DesignToken global) =>
        context.Language.GetStatusScale(context.Variant, global.ColorError, error: true);

    /// <summary>
    /// The solid severity fill of badges (and Lara / Nora tags): 500 (Nora 600) in light, 400 (Nora 500) in dark.
    /// </summary>
    internal static Color Solid(in TokenEvaluationContext<PrimeNGToken> context, PrimeNGSeverityScale palette)
    {
        var nora = context.Variant.Style == PrimeNGLanguage.Nora;
        var dark = context.Variant.Appearance == ThemeAppearance.Dark;
        return palette[nora ? (dark ? 500 : 600) : (dark ? 400 : 500)];
    }

    /// <summary>
    /// The text on a solid severity fill: white in light, the palette's 950 in dark.
    /// </summary>
    internal static Color OnSolid(in TokenEvaluationContext<PrimeNGToken> context, PrimeNGSeverityScale palette) =>
        context.Variant.Appearance == ThemeAppearance.Dark ? palette[950] : Colors.White;
}

/// <summary>
/// Tag under the PrimeNG presets, PrimeUIX 3.1.1 <c>tag</c>: the <c>md</c> step of <c>border.radius</c>,
/// <c>padding</c> 2px 6px (Lara 4px 8px) around the 12px text and no border. The severities are tinted in Aura
/// (light 100 with 700 text, dark the 500 stop at 16% with 300 text) and solid in Lara and Nora (the badge fills);
/// an untinted tag is <c>tag.primary</c> (Aura surface.100 with surface.700 text, Lara and Nora the solid primary).
/// </summary>
internal static class PrimeNGTagTokens
{
    /// <summary>
    /// Writes the status surfaces and returns the status main colors (the tag's own status color tokens).
    /// </summary>
    internal static PrimeNGStatusColors EvaluateGlobal(in TokenEvaluationContext<PrimeNGToken> context, DesignToken controlGlobal)
    {
        // tag.border.radius is the md step of border.radius. The Tag theme reads the corner radius from
        // BorderRadiusSM, so the scope's SM step is moved to the md value.
        controlGlobal.BorderRadiusSM = controlGlobal.BorderRadius;
        var success = context.Language.GetStatusScale(context.Variant, controlGlobal.ColorSuccess);
        var info    = context.Language.GetStatusScale(context.Variant, controlGlobal.ColorInfo);
        var warning = context.Language.GetStatusScale(context.Variant, controlGlobal.ColorWarning);
        var error   = PrimeNGSeverity.Error(context, controlGlobal);
        Color successColor, infoColor, warningColor, errorColor;
        (successColor, controlGlobal.ColorSuccessBg) = Pair(context, success);
        (infoColor, controlGlobal.ColorInfoBg)       = Pair(context, info);
        (warningColor, controlGlobal.ColorWarningBg) = Pair(context, warning);
        (errorColor, controlGlobal.ColorErrorBg)     = Pair(context, error);
        // tag has no border: the status border color is the fill.
        controlGlobal.ColorSuccessBorder = controlGlobal.ColorSuccessBg;
        controlGlobal.ColorInfoBorder    = controlGlobal.ColorInfoBg;
        controlGlobal.ColorWarningBorder = controlGlobal.ColorWarningBg;
        controlGlobal.ColorErrorBorder   = controlGlobal.ColorErrorBg;
        return new PrimeNGStatusColors(infoColor, infoColor, successColor, warningColor, errorColor);
    }

    // Aura: tinted (the status text on the light fill). Lara and Nora: the status color is the solid fill, which the
    // Filled tags paint with the contrast text (FilledStatusSolid); the tint stays for the Outlined variant.
    private static (Color Text, Color Background) Pair(in TokenEvaluationContext<PrimeNGToken> context, PrimeNGSeverityScale palette)
    {
        if (context.Variant.Style != PrimeNGLanguage.Aura)
        {
            var tint = context.Variant.Appearance == ThemeAppearance.Dark
                ? palette.AtOpacity(500, 0.16)
                : palette[100];
            return (PrimeNGSeverity.Solid(context, palette), tint);
        }
        return context.Variant.Appearance == ThemeAppearance.Dark
            ? (palette[300], palette.AtOpacity(500, 0.16))
            : (palette[700], palette[100]);
    }
    internal static void EvaluateOwn(in TokenEvaluationContext<PrimeNGToken> context, DesignToken effectiveGlobal, TagToken token)
    {
        var kind = context.Variant.Style;
        // tag.padding 2px 6px (Lara 4px 8px) around the 12px text on a 1.5 line.
        var padY = kind == PrimeNGLanguage.Lara ? 4 : 2;
        token.TagPadding    = new Thickness(kind == PrimeNGLanguage.Lara ? 8 : 6, 0);
        token.TagLineHeight = 12 * 1.5 + 2 * padY - 2 * effectiveGlobal.LineWidth;
        // tag root.font.weight 700 in every preset.
        token.TagFontWeight = FontWeight.Bold;
        // Lara and Nora fill a status tag with the solid badge fill; Aura keeps the tint of Pair.
        token.FilledStatusSolid = kind != PrimeNGLanguage.Aura;
        token.TagIconSize   = 10;
        // tag.primary: Aura surface.100 with surface.700 text; Lara and Nora the solid primary with its contrast
        // color.
        if (kind == PrimeNGLanguage.Aura)
        {
            var neutral = context.Language.GetNeutralTagColors(context.Variant);
            token.DefaultBg = neutral.Background;
            token.DefaultColor = neutral.Foreground;
        }
        else
        {
            token.DefaultBg    = effectiveGlobal.ColorPrimary;
            token.DefaultColor = effectiveGlobal.ColorTextLightSolid;
        }
    }
}

/// <summary>
/// CountBadge under the PrimeNG presets, PrimeUIX 3.1.1 <c>badge</c> (the danger severity, AtomUI's count color):
/// solid danger fill with its contrast text, <c>border.radius</c> 6 (a rounded rectangle), <c>height</c> 20 (sm 18),
/// <c>font.size</c> 10 (sm 8), <c>padding</c> 0 6px (Lara 0 8px) and the <c>overlaybadge</c> 2px outline in the
/// content background.
/// </summary>
internal static class PrimeNGCountBadgeTokens
{
    internal static void EvaluateGlobal(in TokenEvaluationContext<PrimeNGToken> context, DesignToken controlGlobal)
    {
    }
    internal static void EvaluateOwn(in TokenEvaluationContext<PrimeNGToken> context, DesignToken effectiveGlobal, CountBadgeToken token)
    {
        var error = PrimeNGSeverity.Error(context, effectiveGlobal);
        // badge: the danger fill with its contrast text, border.radius 6 (a rounded rectangle), height 20 (sm 18),
        // font.size 10 (sm 8), padding 0 6px (Lara 0 8px); overlaybadge: a 2px outline in the content background.
        token.IndicatorColor    = PrimeNGSeverity.Solid(context, error);
        token.TextColor         = PrimeNGSeverity.OnSolid(context, error);
        token.CornerRadius      = new CornerRadius(6);
        token.CornerRadiusSM    = new CornerRadius(6);
        token.IndicatorHeight   = 20;
        token.IndicatorHeightSM = 18;
        token.TextFontSize      = 10;
        token.TextFontSizeSM    = 8;
        // badge root.font.weight 700 in every preset.
        token.TextFontWeight    = FontWeight.Bold;
        token.TextPadding       = new Thickness(context.Variant.Style == PrimeNGLanguage.Lara ? 8 : 6, 0);
        token.ShadowSize        = 2 * effectiveGlobal.LineWidth;
        token.ShadowColor       = effectiveGlobal.ColorBgElevated;
    }
}

/// <summary>
/// DotBadge under the PrimeNG presets, PrimeUIX 3.1.1 <c>badge.dot.size</c> 8 in the danger fill, with the
/// <c>overlaybadge</c> 2px outline.
/// </summary>
internal static class PrimeNGDotBadgeTokens
{
    internal static void EvaluateGlobal(in TokenEvaluationContext<PrimeNGToken> context, DesignToken controlGlobal)
    {
    }
    internal static void EvaluateOwn(in TokenEvaluationContext<PrimeNGToken> context, DesignToken effectiveGlobal, DotBadgeToken token)
    {
        token.DotColor    = PrimeNGSeverity.Solid(context, PrimeNGSeverity.Error(context, effectiveGlobal));
        token.DotSize     = 8;
        token.ShadowSize  = 2 * effectiveGlobal.LineWidth;
        token.ShadowColor = effectiveGlobal.ColorBgElevated;
    }
}

/// <summary>
/// AvatarGroup under the PrimeNG presets: PrimeUIX <c>avatar.group.offset</c> -10.
/// </summary>
internal static class PrimeNGAvatarGroupTokens
{
    internal static void EvaluateGlobal(in TokenEvaluationContext<PrimeNGToken> context, DesignToken controlGlobal)
    {
    }
    internal static void EvaluateOwn(in TokenEvaluationContext<PrimeNGToken> context, DesignToken effectiveGlobal, AvatarGroupToken token)
    {
        token.GroupOverlapping = 10;
    }
}

/// <summary>
/// Skeleton under the PrimeNG presets, PrimeUIX 3.1.1 <c>skeleton</c>: <c>background</c> surface.200 (Nora
/// surface.300; dark white at 6%, Nora 10%) with a lighter shimmer (<c>animation.background</c> white at 40%, dark 4%,
/// over it), corners at the md step of border.radius and a 1.2s sweep.
/// </summary>
internal static class PrimeNGSkeletonTokens
{
    internal static void EvaluateGlobal(in TokenEvaluationContext<PrimeNGToken> context, DesignToken controlGlobal)
    {
        // The Skeleton formulas and theme read BorderRadiusSM; the md step is written there so every corner follows it.
        controlGlobal.BorderRadiusSM = controlGlobal.BorderRadius;
    }
    internal static void EvaluateOwn(in TokenEvaluationContext<PrimeNGToken> context, DesignToken effectiveGlobal, SkeletonToken token)
    {
        var colors = context.Language.GetSkeletonColors(context.Variant);
        // skeleton.background with animation.background (the shimmer) over it, the md step of border.radius and
        // the 1.2s sweep.
        token.GradientFromColor     = colors.Background;
        token.GradientToColor       = colors.Shimmer;
        token.BlockRadius           = effectiveGlobal.BorderRadius;
        token.LoadingMotionDuration = TimeSpan.FromSeconds(1.2);
    }
}

/// <summary>
/// Separator under the PrimeNG presets, PrimeUIX 3.1.1 <c>divider</c>: <c>border.color</c> content.border.color,
/// <c>horizontal.margin</c> 14px 0 (Lara 18px 0), <c>vertical.margin</c> 0 14px (Lara 0 18px), normal content weight.
/// </summary>
internal static class PrimeNGSeparatorTokens
{
    internal static void EvaluateGlobal(in TokenEvaluationContext<PrimeNGToken> context, DesignToken controlGlobal)
    {
        controlGlobal.ColorSplit = context.Language.GetContentBorder(context.Variant);
    }
    internal static void EvaluateOwn(in TokenEvaluationContext<PrimeNGToken> context, DesignToken effectiveGlobal, SeparatorToken token)
    {
        var margin = context.Variant.Style == PrimeNGLanguage.Lara ? 18 : 14;
        // divider.horizontal.margin 14px 0 (Lara 18px 0), divider.vertical.margin 0 14px (Lara 0 18px); the title
        // keeps the normal content weight.
        token.HorizontalMarginBlock = new Thickness(0, margin);
        token.VerticalMarginInline  = margin;
        token.TitleFontWeight       = FontWeight.Normal;
    }
}

/// <summary>
/// Timeline under the PrimeNG presets, PrimeUIX 3.1.1 <c>timeline</c>: <c>event.connector.color</c>
/// (content.border.color) at <c>event.connector.size</c> 2.
/// </summary>
internal static class PrimeNGTimelineTokens
{
    internal static void EvaluateGlobal(in TokenEvaluationContext<PrimeNGToken> context, DesignToken controlGlobal)
    {
    }
    internal static void EvaluateOwn(in TokenEvaluationContext<PrimeNGToken> context, DesignToken effectiveGlobal, TimelineToken token)
    {
        token.IndicatorTailColor = context.Language.GetContentBorder(context.Variant);
        token.IndicatorTailWidth = 2;
    }
}

/// <summary>
/// List views under the PrimeNG presets, PrimeUIX 3.1.1 <c>listbox</c>: option text <c>option.color</c>
/// (text.color), hover <c>option.focus.background</c>, <c>option.padding</c> 4px 10px (Lara 8px 14px),
/// <c>list.padding</c>, and the option group label (<c>option.group.color</c>: text.muted in Aura, text.color in Lara
/// and Nora).
/// </summary>
internal static class PrimeNGListViewStage
{
    internal static void Apply(in TokenEvaluationContext<PrimeNGToken> context, DesignToken effectiveGlobal, AbstractListViewToken token)
    {
        var recipes = context.Language.GetVisualRecipes(effectiveGlobal, context.Variant);
        var padding  = recipes.ListOptionPadding;
        // listbox: option.padding 4px 10px (Lara 8px 14px) as the item padding, list.padding as the content padding,
        // option.color for the text (unchanged on hover) and option.focus.background for the hover.
        token.ItemPadding        = new Thickness(padding.Left, 0);
        token.ContentPadding     = context.Language.GetSelectOverlayRecipe(effectiveGlobal, context.Variant).Padding;
        token.ItemColor          = effectiveGlobal.ColorText;
        token.ItemHoverColor     = effectiveGlobal.ColorText;
        token.ItemHoverBgColor   = effectiveGlobal.ControlItemBgHover;
    }
}

/// <summary>
/// PrimeUIX <c>listbox</c> for <see cref="ListViewToken" />; see <see cref="PrimeNGListViewStage" />.
/// </summary>
internal static class PrimeNGListViewTokens
{
    internal static void EvaluateGlobal(in TokenEvaluationContext<PrimeNGToken> context, DesignToken controlGlobal)
    {
    }
    internal static void EvaluateOwn(in TokenEvaluationContext<PrimeNGToken> context, DesignToken effectiveGlobal, ListViewToken token)
    {
        PrimeNGListViewStage.Apply(context, effectiveGlobal, token);
        // The option group label: option.group.color, text.muted in Aura and text.color in Lara and Nora, on the
        // option padding.
        token.GroupHeaderPadding = context.Language.GetVisualRecipes(effectiveGlobal, context.Variant).ListOptionPadding;
        token.GroupHeaderColor   = context.Variant.Style == PrimeNGLanguage.Aura
            ? effectiveGlobal.ColorTextSecondary
            : effectiveGlobal.ColorText;
        // The check mark of a selected item takes the selected text color (PrimeUIX highlight.color): the primary check
        // vanishes on a solid primary highlight (noir, Nora), and on the pale highlight it is the primary 700 text.
        token.ItemSelectedIndicatorColor = token.ItemSelectedColor;
    }
}

/// <summary>
/// PrimeUIX <c>listbox</c> for <see cref="TransferListViewToken" />; see <see cref="PrimeNGListViewStage" />.
/// </summary>
internal static class PrimeNGTransferListViewTokens
{
    internal static void EvaluateGlobal(in TokenEvaluationContext<PrimeNGToken> context, DesignToken controlGlobal)
    {
    }
    internal static void EvaluateOwn(in TokenEvaluationContext<PrimeNGToken> context, DesignToken effectiveGlobal, TransferListViewToken token)
    {
        PrimeNGListViewStage.Apply(context, effectiveGlobal, token);
        // A 2px gap between rows (Aura's list.gap; AtomUI uses it in every preset), so adjacent selected rows read as
        // separate items instead of one solid block.
        token.ItemMargin = new Thickness(0, 1);
    }
}

/// <summary>
/// Trees under the PrimeNG presets, PrimeUIX 3.1.1 <c>tree</c>: <c>node.hover.background</c>
/// (list.option.focus.background) and a node row of <c>node.padding</c> 4px 8px (Lara 6px 8px) around the 24px
/// toggle button (32, Lara 36).
/// </summary>
internal static class PrimeNGTreeStage
{
    internal static void Apply(in TokenEvaluationContext<PrimeNGToken> context, DesignToken effectiveGlobal, AbstractTreeViewToken token)
    {
        token.NodeHoverBg  = effectiveGlobal.ControlItemBgHover;
        token.HeaderHeight = context.Variant.Style == PrimeNGLanguage.Lara ? 36 : 32;
    }
}

/// <summary>
/// PrimeUIX <c>tree</c> for <see cref="TreeViewToken" />; see <see cref="PrimeNGTreeStage" />.
/// </summary>
internal static class PrimeNGTreeViewTokens
{
    internal static void EvaluateGlobal(in TokenEvaluationContext<PrimeNGToken> context, DesignToken controlGlobal)
    {
        controlGlobal.MotionDurationSlow = PrimeNGMotion.Collapsible;
    }
    internal static void EvaluateOwn(in TokenEvaluationContext<PrimeNGToken> context, DesignToken effectiveGlobal, TreeViewToken token) =>
        PrimeNGTreeStage.Apply(context, effectiveGlobal, token);
}

/// <summary>
/// PrimeUIX <c>tree</c> for <see cref="TransferTreeViewToken" />; see <see cref="PrimeNGTreeStage" />.
/// </summary>
internal static class PrimeNGTransferTreeViewTokens
{
    internal static void EvaluateGlobal(in TokenEvaluationContext<PrimeNGToken> context, DesignToken controlGlobal)
    {
    }
    internal static void EvaluateOwn(in TokenEvaluationContext<PrimeNGToken> context, DesignToken effectiveGlobal, TransferTreeViewToken token) =>
        PrimeNGTreeStage.Apply(context, effectiveGlobal, token);
}

/// <summary>
/// Carousel indicators under the PrimeNG presets, PrimeUIX 3.1.1 <c>carousel.indicator</c>: 28 x 8 bars (Lara
/// 14 x 14), the active one the same size, 8 apart.
/// </summary>
internal static class PrimeNGCarouselTokens
{
    internal static void EvaluateGlobal(in TokenEvaluationContext<PrimeNGToken> context, DesignToken controlGlobal)
    {
        controlGlobal.MotionDurationSlow = PrimeNGMotion.Carousel;
    }
    internal static void EvaluateOwn(in TokenEvaluationContext<PrimeNGToken> context, DesignToken effectiveGlobal, CarouselToken token)
    {
        var lara = context.Variant.Style == PrimeNGLanguage.Lara;
        // carousel.indicator: 28 x 8 bars (Lara 14 x 14), the active one the same size, 8 apart.
        token.IndicatorWidth       = lara ? 14 : 28;
        token.IndicatorActiveWidth = token.IndicatorWidth;
        token.IndicatorHeight      = lara ? 14 : 8;
        token.IndicatorGap         = 8;
    }
}

/// <summary>
/// Descriptions under the PrimeNG presets (no PrimeUIX counterpart; derived from <c>datatable</c>): labels in
/// text.muted, lines in content.border.color and bordered cells padded as <c>body.cell.padding</c> 8px 14px (Lara
/// 10px 14px).
/// </summary>
internal static class PrimeNGDescriptionsTokens
{
    internal static void EvaluateGlobal(in TokenEvaluationContext<PrimeNGToken> context, DesignToken controlGlobal)
    {
        var border = context.Language.GetContentBorder(context.Variant);
        controlGlobal.ColorSplit           = border;
        controlGlobal.ColorBorderSecondary = border;
    }
    internal static void EvaluateOwn(in TokenEvaluationContext<PrimeNGToken> context, DesignToken effectiveGlobal, DescriptionsToken token)
    {
        // datatable: labels in text.muted, and bordered cells padded as body.cell.padding 8px 14px (Lara 10px 14px).
        token.LabelColor  = effectiveGlobal.ColorTextSecondary;
        token.ItemPadding = new Thickness(14, context.Variant.Style == PrimeNGLanguage.Lara ? 10 : 8);
    }
}

/// <summary>
/// QRCode under the PrimeNG presets (no PrimeUIX counterpart): the frame takes content.border.color and the
/// md step of border.radius.
/// </summary>
internal static class PrimeNGQRCodeTokens
{
    internal static void EvaluateGlobal(in TokenEvaluationContext<PrimeNGToken> context, DesignToken controlGlobal)
    {
        controlGlobal.ColorSplit     = context.Language.GetContentBorder(context.Variant);
        controlGlobal.BorderRadiusLG = controlGlobal.BorderRadius;
    }
    internal static void EvaluateOwn(in TokenEvaluationContext<PrimeNGToken> context, DesignToken effectiveGlobal, QRCodeToken token)
    {
    }
}

/// <summary>
/// Pagination under the PrimeNG presets, PrimeUIX 3.1.1 <c>paginator.nav.button</c>: 2.25rem buttons, round in Aura
/// and Lara (<c>border.radius</c> 50%) and the md step of border.radius in Nora; text.muted text that
/// hovers to <c>hover.color</c> (surface.600, Nora surface.700; dark surface.300) on <c>hover.background</c>
/// (content.hover.background); the selected page is the solid highlight (<c>selected.background</c> /
/// <c>selected.color</c>) without a border and at the normal weight, unchanged on hover.
/// </summary>
internal static class PrimeNGPaginationTokens
{
    internal static void EvaluateGlobal(in TokenEvaluationContext<PrimeNGToken> context, DesignToken controlGlobal)
    {
        // PrimeUIX paginator.root.gap 0.25rem: between the page buttons and between the paginator parts.
        controlGlobal.UniformlyMarginXS = 4;
    }
    internal static void EvaluateOwn(in TokenEvaluationContext<PrimeNGToken> context, DesignToken effectiveGlobal, PaginationToken token)
    {
        PrimeNGPaginationItems.Apply(context, effectiveGlobal, token);
        // paginator.nav.button.selected.background / selected.color: the solid highlight, without a border, at the
        // normal weight and with the same colors on hover.
        token.ItemActiveBg               = effectiveGlobal.ControlItemBgActive;
        token.ItemActiveColor            = effectiveGlobal.ControlItemColorActive;
        token.ItemActiveBorderColor      = Colors.Transparent;
        token.ItemActiveHoverColor       = effectiveGlobal.ControlItemColorActive;
        token.ItemActiveHoverBorderColor = Colors.Transparent;
        token.ItemActiveFontWeight       = FontWeight.Normal;
    }
}

/// <summary>
/// The <c>paginator.nav.button</c> items shared by Pagination and SimplePagination: 2.25rem, round in Aura and Lara and
/// the md step of border.radius in Nora, text.muted text hovering to surface.600 (Nora surface.700; dark
/// surface.300) on the content hover background.
/// </summary>
internal static class PrimeNGPaginationItems
{
    internal static void Apply(in TokenEvaluationContext<PrimeNGToken> context, DesignToken effectiveGlobal, AbstractPaginationToken token)
    {
        var kind = context.Variant.Style;
        var dark = context.Variant.Appearance == ThemeAppearance.Dark;
        // paginator.nav.button: 2.25rem (36), round in Aura and Lara and the md step of border.radius in Nora;
        // text.muted text that hovers to hover.color (surface.600, Nora surface.700; dark surface.300) on
        // content.hover.background.
        token.ItemSize         = 36;
        token.ItemColor        = effectiveGlobal.ColorTextSecondary;
        token.ItemHoverColor   = context.Language.GetSurfaceColor(dark ? 300 : kind == PrimeNGLanguage.Nora ? 700 : 600);
        token.ItemHoverBg      = effectiveGlobal.ControlItemBgHover;
        token.ItemBorderRadius = kind == PrimeNGLanguage.Nora ? effectiveGlobal.BorderRadius : new CornerRadius(token.ItemSize / 2);
    }
}

/// <summary>
/// SimplePagination under the PrimeNG presets: the shared <see cref="PrimeNGPaginationItems" />.
/// </summary>
internal static class PrimeNGSimplePaginationTokens
{
    internal static void EvaluateGlobal(in TokenEvaluationContext<PrimeNGToken> context, DesignToken controlGlobal)
    {
        // PrimeUIX paginator.root.gap 0.25rem: between the page buttons and between the paginator parts.
        controlGlobal.UniformlyMarginXS = 4;
    }
    internal static void EvaluateOwn(in TokenEvaluationContext<PrimeNGToken> context, DesignToken effectiveGlobal, SimplePaginationToken token)
    {
        PrimeNGPaginationItems.Apply(context, effectiveGlobal, token);
    }
}
