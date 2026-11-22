using AtomUI.Controls;
using AtomUI.Desktop.Controls;
using AtomUI.Theme;
using AtomUI.Theme.Algorithms;
using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Themes.PrimeNG.Algorithms.Controls;

/// <summary>
/// The PrimeUIX severity colours of the compact badges and tags (<c>tag</c>, <c>badge</c>): each status palette is
/// recovered from the global status colour and the stop the preset writes it at.
/// </summary>
internal static class PrimeNGSeverity
{
    internal static Color[] Status(in ThemeAlgorithmContext context, DesignToken global, Color statusColor)
    {
        var rules = PrimeNGPresets.Rules(context, global);
        return PrimeNGPalette.FromStop(statusColor, rules.Status.Base.Stop);
    }

    internal static Color[] Error(in ThemeAlgorithmContext context, DesignToken global)
    {
        var rules = PrimeNGPresets.Rules(context, global);
        return PrimeNGPalette.FromStop(global.ColorError, rules.Error.Base.Stop);
    }

    internal static Color Stop(Color[] palette, int stop) => palette[PrimeNGPalette.IndexOf(stop)];

    /// <summary>
    /// The solid severity fill of badges (and Lara / Nora tags): 500 (Nora 600) in light, 400 (Nora 500) in dark.
    /// </summary>
    internal static Color Solid(in ThemeAlgorithmContext context, Color[] palette)
    {
        var nora = PrimeNGPresets.KindOf(context) == PrimeNGPresetKind.Nora;
        var dark = context.Appearance == ThemeAppearance.Dark;
        return Stop(palette, nora ? (dark ? 500 : 600) : (dark ? 400 : 500));
    }

    /// <summary>The text on a solid severity fill: white in light, the palette's 950 in dark.</summary>
    internal static Color OnSolid(in ThemeAlgorithmContext context, Color[] palette) =>
        context.Appearance == ThemeAppearance.Dark ? Stop(palette, 950) : Colors.White;
}

/// <summary>
/// Tag under the PrimeNG presets, PrimeUIX 3.1.1 <c>tag</c>: <c>border.radius</c> border.radius.md,
/// <c>padding</c> 2px 6px (Lara 4px 8px) around the 12px text and no border. The severities are tinted in Aura
/// (light 100 with 700 text, dark the 500 stop at 16% with 300 text) and solid in Lara and Nora (the badge fills);
/// an untinted tag is <c>tag.primary</c> (Aura surface.100 with surface.700 text, Lara and Nora the solid primary).
/// </summary>
[ControlTokenAlgorithm("PrimeNG", "Aura", 1)]
[ControlTokenAlgorithm("PrimeNG", "Aura.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora.Dark", 1)]
internal sealed class PrimeNGTagTokens : IControlTokenAlgorithm<TagToken>
{
    /// <inheritdoc />
    public void EvaluateGlobal(in ThemeAlgorithmContext context, DesignToken controlGlobal)
    {
        controlGlobal.BorderRadiusSM = controlGlobal.BorderRadius;
        var success = PrimeNGSeverity.Status(context, controlGlobal, controlGlobal.ColorSuccess);
        var info    = PrimeNGSeverity.Status(context, controlGlobal, controlGlobal.ColorInfo);
        var warning = PrimeNGSeverity.Status(context, controlGlobal, controlGlobal.ColorWarning);
        var error   = PrimeNGSeverity.Error(context, controlGlobal);
        (controlGlobal.ColorSuccess, controlGlobal.ColorSuccessBg) = Pair(context, success);
        (controlGlobal.ColorInfo, controlGlobal.ColorInfoBg)       = Pair(context, info);
        (controlGlobal.ColorWarning, controlGlobal.ColorWarningBg) = Pair(context, warning);
        (controlGlobal.ColorError, controlGlobal.ColorErrorBg)     = Pair(context, error);
        controlGlobal.ColorSuccessBorder = controlGlobal.ColorSuccessBg;
        controlGlobal.ColorInfoBorder    = controlGlobal.ColorInfoBg;
        controlGlobal.ColorWarningBorder = controlGlobal.ColorWarningBg;
        controlGlobal.ColorErrorBorder   = controlGlobal.ColorErrorBg;
    }

    // Aura: tinted (the status text on the light fill). Lara and Nora: the status colour is the solid fill, which the
    // Filled tags paint with the contrast text (FilledStatusSolid); the tint stays for the Outlined variant.
    private static (Color Text, Color Background) Pair(in ThemeAlgorithmContext context, Color[] palette)
    {
        if (PrimeNGPresets.KindOf(context) != PrimeNGPresetKind.Aura)
        {
            var tint = context.Appearance == ThemeAppearance.Dark
                ? PrimeNGPalette.WithOpacity(PrimeNGSeverity.Stop(palette, 500), 0.16)
                : PrimeNGSeverity.Stop(palette, 100);
            return (PrimeNGSeverity.Solid(context, palette), tint);
        }
        return context.Appearance == ThemeAppearance.Dark
            ? (PrimeNGSeverity.Stop(palette, 300), PrimeNGPalette.WithOpacity(PrimeNGSeverity.Stop(palette, 500), 0.16))
            : (PrimeNGSeverity.Stop(palette, 700), PrimeNGSeverity.Stop(palette, 100));
    }

    /// <inheritdoc />
    public void EvaluateOwn(in ThemeAlgorithmContext context, DesignToken effectiveGlobal, TagToken token)
    {
        var kind = PrimeNGPresets.KindOf(context);
        var dark = context.Appearance == ThemeAppearance.Dark;
        var padY = kind == PrimeNGPresetKind.Lara ? 4 : 2;
        token.TagPadding    = new Thickness(kind == PrimeNGPresetKind.Lara ? 8 : 6, 0);
        token.TagLineHeight = 12 * 1.5 + 2 * padY - 2 * effectiveGlobal.LineWidth;
        // tag root.font.weight 700 in every preset.
        token.TagFontWeight = FontWeight.Bold;
        token.FilledStatusSolid = kind != PrimeNGPresetKind.Aura;
        token.TagIconSize   = 10;
        if (kind == PrimeNGPresetKind.Aura)
        {
            token.DefaultBg = dark
                ? PrimeNGPalette.WithOpacity(PrimeNGPresets.Surface(context, 500), 0.16)
                : PrimeNGPresets.Surface(context, 100);
            token.DefaultColor = PrimeNGPresets.Surface(context, dark ? 300 : 700);
        }
        else
        {
            token.DefaultBg    = effectiveGlobal.ColorPrimary;
            token.DefaultColor = effectiveGlobal.ColorTextLightSolid;
        }
    }
}

/// <summary>
/// CountBadge under the PrimeNG presets, PrimeUIX 3.1.1 <c>badge</c> (the danger severity, AtomUI's count colour):
/// solid danger fill with its contrast text, <c>border.radius</c> 6 (a rounded rectangle), <c>height</c> 20 (sm 18),
/// <c>font.size</c> 10 (sm 8), <c>padding</c> 0 6px (Lara 0 8px) and the <c>overlaybadge</c> 2px outline in the
/// content background.
/// </summary>
[ControlTokenAlgorithm("PrimeNG", "Aura", 1)]
[ControlTokenAlgorithm("PrimeNG", "Aura.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora.Dark", 1)]
internal sealed class PrimeNGCountBadgeTokens : IControlTokenAlgorithm<CountBadgeToken>
{
    /// <inheritdoc />
    public void EvaluateGlobal(in ThemeAlgorithmContext context, DesignToken controlGlobal)
    {
    }

    /// <inheritdoc />
    public void EvaluateOwn(in ThemeAlgorithmContext context, DesignToken effectiveGlobal, CountBadgeToken token)
    {
        var error = PrimeNGSeverity.Error(context, effectiveGlobal);
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
        token.TextPadding       = new Thickness(PrimeNGPresets.KindOf(context) == PrimeNGPresetKind.Lara ? 8 : 6, 0);
        token.ShadowSize        = 2 * effectiveGlobal.LineWidth;
        token.ShadowColor       = effectiveGlobal.ColorBgElevated;
    }
}

/// <summary>
/// DotBadge under the PrimeNG presets, PrimeUIX 3.1.1 <c>badge.dot.size</c> 8 in the danger fill, with the
/// <c>overlaybadge</c> 2px outline.
/// </summary>
[ControlTokenAlgorithm("PrimeNG", "Aura", 1)]
[ControlTokenAlgorithm("PrimeNG", "Aura.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora.Dark", 1)]
internal sealed class PrimeNGDotBadgeTokens : IControlTokenAlgorithm<DotBadgeToken>
{
    /// <inheritdoc />
    public void EvaluateGlobal(in ThemeAlgorithmContext context, DesignToken controlGlobal)
    {
    }

    /// <inheritdoc />
    public void EvaluateOwn(in ThemeAlgorithmContext context, DesignToken effectiveGlobal, DotBadgeToken token)
    {
        token.DotColor    = PrimeNGSeverity.Solid(context, PrimeNGSeverity.Error(context, effectiveGlobal));
        token.DotSize     = 8;
        token.ShadowSize  = 2 * effectiveGlobal.LineWidth;
        token.ShadowColor = effectiveGlobal.ColorBgElevated;
    }
}

/// <summary>
/// Avatar under the PrimeNG presets, PrimeUIX 3.1.1 <c>avatar</c>: <c>background</c> surface.200 / 700 with
/// text.color, one <c>border.radius</c> at every size, <c>lg.font.size</c> 20 and the group border in the content
/// background.
/// </summary>
[ControlTokenAlgorithm("PrimeNG", "Aura", 1)]
[ControlTokenAlgorithm("PrimeNG", "Aura.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora.Dark", 1)]
internal sealed class PrimeNGAvatarTokens : IControlTokenAlgorithm<AvatarToken>
{
    /// <inheritdoc />
    public void EvaluateGlobal(in ThemeAlgorithmContext context, DesignToken controlGlobal)
    {
        controlGlobal.BorderRadiusSM = controlGlobal.BorderRadius;
        controlGlobal.BorderRadiusLG = controlGlobal.BorderRadius;
    }

    /// <inheritdoc />
    public void EvaluateOwn(in ThemeAlgorithmContext context, DesignToken effectiveGlobal, AvatarToken token)
    {
        token.AvatarBg         = PrimeNGPresets.Surface(context, context.Appearance == ThemeAppearance.Dark ? 700 : 200);
        token.AvatarColor      = effectiveGlobal.ColorText;
        token.TextFontSizeLG   = 20;
        token.GroupBorderColor = effectiveGlobal.ColorBgElevated;
    }
}

/// <summary>AvatarGroup under the PrimeNG presets: PrimeUIX <c>avatar.group.offset</c> -10.</summary>
[ControlTokenAlgorithm("PrimeNG", "Aura", 1)]
[ControlTokenAlgorithm("PrimeNG", "Aura.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora.Dark", 1)]
internal sealed class PrimeNGAvatarGroupTokens : IControlTokenAlgorithm<AvatarGroupToken>
{
    /// <inheritdoc />
    public void EvaluateGlobal(in ThemeAlgorithmContext context, DesignToken controlGlobal)
    {
    }

    /// <inheritdoc />
    public void EvaluateOwn(in ThemeAlgorithmContext context, DesignToken effectiveGlobal, AvatarGroupToken token)
    {
        token.GroupOverlapping = 10;
    }
}

/// <summary>
/// Skeleton under the PrimeNG presets, PrimeUIX 3.1.1 <c>skeleton</c>: <c>background</c> surface.200 (Nora
/// surface.300; dark white at 6%, Nora 10%) with a lighter shimmer (<c>animation.background</c> white at 40%, dark 4%,
/// over it), border.radius.md corners and a 1.2s sweep.
/// </summary>
[ControlTokenAlgorithm("PrimeNG", "Aura", 1)]
[ControlTokenAlgorithm("PrimeNG", "Aura.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora.Dark", 1)]
internal sealed class PrimeNGSkeletonTokens : IControlTokenAlgorithm<SkeletonToken>
{
    /// <inheritdoc />
    public void EvaluateGlobal(in ThemeAlgorithmContext context, DesignToken controlGlobal)
    {
        controlGlobal.BorderRadiusSM = controlGlobal.BorderRadius;
    }

    /// <inheritdoc />
    public void EvaluateOwn(in ThemeAlgorithmContext context, DesignToken effectiveGlobal, SkeletonToken token)
    {
        var nora = PrimeNGPresets.KindOf(context) == PrimeNGPresetKind.Nora;
        Color from;
        Color to;
        if (context.Appearance == ThemeAppearance.Dark)
        {
            var alpha = nora ? 0.10 : 0.06;
            from = PrimeNGPalette.WithOpacity(Colors.White, alpha);
            to   = PrimeNGPalette.WithOpacity(Colors.White, alpha + 0.04 * (1 - alpha));
        }
        else
        {
            from = PrimeNGPresets.Surface(context, nora ? 300 : 200);
            to   = PrimeNGPalette.Mix(Colors.White, from, 40);
        }
        token.GradientFromColor     = from;
        token.GradientToColor       = to;
        token.BlockRadius           = effectiveGlobal.BorderRadius;
        token.LoadingMotionDuration = TimeSpan.FromSeconds(1.2);
    }
}

/// <summary>
/// Separator under the PrimeNG presets, PrimeUIX 3.1.1 <c>divider</c>: <c>border.color</c> content.border.color,
/// <c>horizontal.margin</c> 14px 0 (Lara 18px 0), <c>vertical.margin</c> 0 14px (Lara 0 18px), normal content weight.
/// </summary>
[ControlTokenAlgorithm("PrimeNG", "Aura", 1)]
[ControlTokenAlgorithm("PrimeNG", "Aura.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora.Dark", 1)]
internal sealed class PrimeNGSeparatorTokens : IControlTokenAlgorithm<SeparatorToken>
{
    /// <inheritdoc />
    public void EvaluateGlobal(in ThemeAlgorithmContext context, DesignToken controlGlobal)
    {
        controlGlobal.ColorSplit = PrimeNGPresets.ContentBorder(context);
    }

    /// <inheritdoc />
    public void EvaluateOwn(in ThemeAlgorithmContext context, DesignToken effectiveGlobal, SeparatorToken token)
    {
        var margin = PrimeNGPresets.KindOf(context) == PrimeNGPresetKind.Lara ? 18 : 14;
        token.HorizontalMarginBlock = new Thickness(0, margin);
        token.VerticalMarginInline  = margin;
        token.TitleFontWeight       = FontWeight.Normal;
    }
}

/// <summary>
/// Timeline under the PrimeNG presets, PrimeUIX 3.1.1 <c>timeline</c>: <c>event.connector.color</c>
/// (content.border.color) at <c>event.connector.size</c> 2.
/// </summary>
[ControlTokenAlgorithm("PrimeNG", "Aura", 1)]
[ControlTokenAlgorithm("PrimeNG", "Aura.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora.Dark", 1)]
internal sealed class PrimeNGTimelineTokens : IControlTokenAlgorithm<TimelineToken>
{
    /// <inheritdoc />
    public void EvaluateGlobal(in ThemeAlgorithmContext context, DesignToken controlGlobal)
    {
    }

    /// <inheritdoc />
    public void EvaluateOwn(in ThemeAlgorithmContext context, DesignToken effectiveGlobal, TimelineToken token)
    {
        token.IndicatorTailColor = PrimeNGPresets.ContentBorder(context);
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
    internal static void Apply(in ThemeAlgorithmContext context, DesignToken effectiveGlobal, AbstractListViewToken token)
    {
        var controls = PrimeNGPresets.Resolve(context).Controls;
        var padding  = controls.SelectOptionPadding(effectiveGlobal);
        token.ItemPadding        = new Thickness(padding.Left, 0);
        token.ContentPadding     = controls.SelectPopupContentPadding(effectiveGlobal);
        token.ItemColor          = effectiveGlobal.ColorText;
        token.ItemHoverColor     = effectiveGlobal.ColorText;
        token.ItemHoverBgColor   = effectiveGlobal.ControlItemBgHover;
    }
}

/// <summary>PrimeUIX <c>listbox</c> for <see cref="ListViewToken"/>; see <see cref="PrimeNGListViewStage"/>.</summary>
[ControlTokenAlgorithm("PrimeNG", "Aura", 1)]
[ControlTokenAlgorithm("PrimeNG", "Aura.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora.Dark", 1)]
internal sealed class PrimeNGListViewTokens : IControlTokenAlgorithm<ListViewToken>
{
    /// <inheritdoc />
    public void EvaluateGlobal(in ThemeAlgorithmContext context, DesignToken controlGlobal)
    {
    }

    /// <inheritdoc />
    public void EvaluateOwn(in ThemeAlgorithmContext context, DesignToken effectiveGlobal, ListViewToken token)
    {
        PrimeNGListViewStage.Apply(context, effectiveGlobal, token);
        token.GroupHeaderPadding = PrimeNGPresets.Resolve(context).Controls.SelectOptionPadding(effectiveGlobal);
        token.GroupHeaderColor   = PrimeNGPresets.KindOf(context) == PrimeNGPresetKind.Aura
            ? effectiveGlobal.ColorTextSecondary
            : effectiveGlobal.ColorText;
        // The check mark of a selected item takes the selected text colour (PrimeUIX highlight.color): the primary check
        // vanishes on a solid primary highlight (noir, Nora), and on the pale highlight it is the primary 700 text.
        token.ItemSelectedIndicatorColor = token.ItemSelectedColor;
    }
}

/// <summary>PrimeUIX <c>listbox</c> for <see cref="TransferListViewToken"/>; see <see cref="PrimeNGListViewStage"/>.</summary>
[ControlTokenAlgorithm("PrimeNG", "Aura", 1)]
[ControlTokenAlgorithm("PrimeNG", "Aura.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora.Dark", 1)]
internal sealed class PrimeNGTransferListViewTokens : IControlTokenAlgorithm<TransferListViewToken>
{
    /// <inheritdoc />
    public void EvaluateGlobal(in ThemeAlgorithmContext context, DesignToken controlGlobal)
    {
    }

    /// <inheritdoc />
    public void EvaluateOwn(in ThemeAlgorithmContext context, DesignToken effectiveGlobal, TransferListViewToken token)
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
    internal static void Apply(in ThemeAlgorithmContext context, DesignToken effectiveGlobal, AbstractTreeViewToken token)
    {
        token.NodeHoverBg  = effectiveGlobal.ControlItemBgHover;
        token.HeaderHeight = PrimeNGPresets.KindOf(context) == PrimeNGPresetKind.Lara ? 36 : 32;
    }
}

/// <summary>PrimeUIX <c>tree</c> for <see cref="TreeViewToken"/>; see <see cref="PrimeNGTreeStage"/>.</summary>
[ControlTokenAlgorithm("PrimeNG", "Aura", 1)]
[ControlTokenAlgorithm("PrimeNG", "Aura.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora.Dark", 1)]
internal sealed class PrimeNGTreeViewTokens : IControlTokenAlgorithm<TreeViewToken>
{
    /// <inheritdoc />
    public void EvaluateGlobal(in ThemeAlgorithmContext context, DesignToken controlGlobal)
    {
        PrimeNGMotion.Restore(context, controlGlobal, slow: PrimeNGMotion.Collapsible);
    }

    /// <inheritdoc />
    public void EvaluateOwn(in ThemeAlgorithmContext context, DesignToken effectiveGlobal, TreeViewToken token) =>
        PrimeNGTreeStage.Apply(context, effectiveGlobal, token);
}

/// <summary>PrimeUIX <c>tree</c> for <see cref="TransferTreeViewToken"/>; see <see cref="PrimeNGTreeStage"/>.</summary>
[ControlTokenAlgorithm("PrimeNG", "Aura", 1)]
[ControlTokenAlgorithm("PrimeNG", "Aura.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora.Dark", 1)]
internal sealed class PrimeNGTransferTreeViewTokens : IControlTokenAlgorithm<TransferTreeViewToken>
{
    /// <inheritdoc />
    public void EvaluateGlobal(in ThemeAlgorithmContext context, DesignToken controlGlobal)
    {
    }

    /// <inheritdoc />
    public void EvaluateOwn(in ThemeAlgorithmContext context, DesignToken effectiveGlobal, TransferTreeViewToken token) =>
        PrimeNGTreeStage.Apply(context, effectiveGlobal, token);
}

/// <summary>
/// Carousel indicators under the PrimeNG presets, PrimeUIX 3.1.1 <c>carousel.indicator</c>: 28 x 8 bars (Lara
/// 14 x 14), the active one the same size, 8 apart.
/// </summary>
[ControlTokenAlgorithm("PrimeNG", "Aura", 1)]
[ControlTokenAlgorithm("PrimeNG", "Aura.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora.Dark", 1)]
internal sealed class PrimeNGCarouselTokens : IControlTokenAlgorithm<CarouselToken>
{
    /// <inheritdoc />
    public void EvaluateGlobal(in ThemeAlgorithmContext context, DesignToken controlGlobal)
    {
        PrimeNGMotion.Restore(context, controlGlobal, slow: PrimeNGMotion.Carousel);
    }

    /// <inheritdoc />
    public void EvaluateOwn(in ThemeAlgorithmContext context, DesignToken effectiveGlobal, CarouselToken token)
    {
        var lara = PrimeNGPresets.KindOf(context) == PrimeNGPresetKind.Lara;
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
[ControlTokenAlgorithm("PrimeNG", "Aura", 1)]
[ControlTokenAlgorithm("PrimeNG", "Aura.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora.Dark", 1)]
internal sealed class PrimeNGDescriptionsTokens : IControlTokenAlgorithm<DescriptionsToken>
{
    /// <inheritdoc />
    public void EvaluateGlobal(in ThemeAlgorithmContext context, DesignToken controlGlobal)
    {
        var border = PrimeNGPresets.ContentBorder(context);
        controlGlobal.ColorSplit           = border;
        controlGlobal.ColorBorderSecondary = border;
    }

    /// <inheritdoc />
    public void EvaluateOwn(in ThemeAlgorithmContext context, DesignToken effectiveGlobal, DescriptionsToken token)
    {
        token.LabelColor  = effectiveGlobal.ColorTextSecondary;
        token.ItemPadding = new Thickness(14, PrimeNGPresets.KindOf(context) == PrimeNGPresetKind.Lara ? 10 : 8);
    }
}

/// <summary>
/// QRCode under the PrimeNG presets (no PrimeUIX counterpart): the frame takes content.border.color and
/// border.radius.md.
/// </summary>
[ControlTokenAlgorithm("PrimeNG", "Aura", 1)]
[ControlTokenAlgorithm("PrimeNG", "Aura.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora.Dark", 1)]
internal sealed class PrimeNGQRCodeTokens : IControlTokenAlgorithm<QRCodeToken>
{
    /// <inheritdoc />
    public void EvaluateGlobal(in ThemeAlgorithmContext context, DesignToken controlGlobal)
    {
        controlGlobal.ColorSplit     = PrimeNGPresets.ContentBorder(context);
        controlGlobal.BorderRadiusLG = controlGlobal.BorderRadius;
    }

    /// <inheritdoc />
    public void EvaluateOwn(in ThemeAlgorithmContext context, DesignToken effectiveGlobal, QRCodeToken token)
    {
    }
}

/// <summary>
/// Pagination under the PrimeNG presets, PrimeUIX 3.1.1 <c>paginator.nav.button</c>: 2.25rem buttons, round in Aura
/// and Lara (<c>border.radius</c> 50%) and border.radius.md in Nora; text.muted text that hovers to
/// <c>hover.color</c> (surface.600, Nora surface.700; dark surface.300) on <c>hover.background</c>
/// (content.hover.background); the selected page is the solid highlight (<c>selected.background</c> /
/// <c>selected.color</c>) without a border and at the normal weight, unchanged on hover.
/// </summary>
[ControlTokenAlgorithm("PrimeNG", "Aura", 1)]
[ControlTokenAlgorithm("PrimeNG", "Aura.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora.Dark", 1)]
internal sealed class PrimeNGPaginationTokens : IControlTokenAlgorithm<PaginationToken>
{
    /// <inheritdoc />
    public void EvaluateGlobal(in ThemeAlgorithmContext context, DesignToken controlGlobal)
    {
        // PrimeUIX paginator.root.gap 0.25rem: between the page buttons and between the paginator parts.
        controlGlobal.UniformlyMarginXS = 4;
    }

    /// <inheritdoc />
    public void EvaluateOwn(in ThemeAlgorithmContext context, DesignToken effectiveGlobal, PaginationToken token)
    {
        PrimeNGPaginationItems.Apply(context, effectiveGlobal, token);
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
/// border.radius.md in Nora, text.muted text hovering to surface.600 (Nora surface.700; dark surface.300) on the
/// content hover background.
/// </summary>
internal static class PrimeNGPaginationItems
{
    internal static void Apply(in ThemeAlgorithmContext context, DesignToken effectiveGlobal, AbstractPaginationToken token)
    {
        var kind = PrimeNGPresets.KindOf(context);
        var dark = context.Appearance == ThemeAppearance.Dark;
        token.ItemSize         = 36;
        token.ItemColor        = effectiveGlobal.ColorTextSecondary;
        token.ItemHoverColor   = PrimeNGPresets.Surface(context, dark ? 300 : kind == PrimeNGPresetKind.Nora ? 700 : 600);
        token.ItemHoverBg      = effectiveGlobal.ControlItemBgHover;
        token.ItemBorderRadius = kind == PrimeNGPresetKind.Nora ? effectiveGlobal.BorderRadius : new CornerRadius(token.ItemSize / 2);
    }
}

/// <summary>SimplePagination under the PrimeNG presets: the shared <see cref="PrimeNGPaginationItems"/>.</summary>
[ControlTokenAlgorithm("PrimeNG", "Aura", 1)]
[ControlTokenAlgorithm("PrimeNG", "Aura.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora.Dark", 1)]
internal sealed class PrimeNGSimplePaginationTokens : IControlTokenAlgorithm<SimplePaginationToken>
{
    /// <inheritdoc />
    public void EvaluateGlobal(in ThemeAlgorithmContext context, DesignToken controlGlobal)
    {
        controlGlobal.UniformlyMarginXS = 4;
    }

    /// <inheritdoc />
    public void EvaluateOwn(in ThemeAlgorithmContext context, DesignToken effectiveGlobal, SimplePaginationToken token)
    {
        PrimeNGPaginationItems.Apply(context, effectiveGlobal, token);
    }
}
