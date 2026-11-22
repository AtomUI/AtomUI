using AtomUI.Desktop.Controls;
using AtomUI.Theme;
using AtomUI.Theme.Algorithms;
using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Themes.PrimeNG.Algorithms.Controls;

/// <summary>
/// The PrimeUIX <c>form.field</c> semantics for every owner of the shared input frame (InputControlFrame /
/// AddOnDecoratedBox). The frame reads Ant-shaped globals, so each owner's scope gets the PrimeUIX values:
/// <list type="bullet">
/// <item>hover border <c>form.field.hover.border.color</c> (Aura surface.400 / dark surface.500, Lara the primary colour,
/// Nora unchanged): the frame's hover kind is <c>ColorPrimaryHover</c>, and an invalid field keeps the same hover
/// colour (the PrimeNG hover selector outranks <c>.p-invalid</c>);</item>
/// <item>disabled text <c>form.field.disabled.color</c> (text.muted: PrimeNG inputs set <c>opacity: 1</c>, so the
/// global component-opacity stand-in does not apply here);</item>
/// <item>suffix, clear and reveal icons <c>form.field.icon.color</c> (Aura surface.400, Lara surface.500, Nora
/// surface.900 / white; dark Aura and Lara surface.400);</item>
/// <item>one radius at every size (<c>.p-inputtext-sm/-lg</c> change only font and padding);</item>
/// <item>Nora's 2px primary focus ring (<c>form.field.focus.ring</c> 2px at offset -1px), drawn as the 1px primary
/// border plus a 1px outer spread; an invalid field focuses with the same ring;</item>
/// <item>input-group add-ons <c>inputgroup.addon.background</c> (Aura and Nora the field background, Lara surface.50 /
/// surface.800).</item>
/// </list>
/// </summary>
internal static class PrimeNGInputStage
{
    internal static void Apply(in ThemeAlgorithmContext context, DesignToken controlGlobal)
    {
        var preset  = PrimeNGPresets.Resolve(context);
        var rules   = PrimeNGPresets.Rules(context, controlGlobal);
        var surface = rules.Surface.AsSpan();
        var dark    = context.Appearance == ThemeAppearance.Dark;

        Color hoverBorder;
        Color icon;
        Color addOnBackground;
        if (ReferenceEquals(preset, PrimeNGLaraPreset.Table))
        {
            hoverBorder     = controlGlobal.ColorPrimary;
            icon            = new PrimeNGShade(dark ? 400 : 500).Resolve(surface);
            addOnBackground = new PrimeNGShade(dark ? 800 : 50).Resolve(surface);
        }
        else if (ReferenceEquals(preset, PrimeNGNoraPreset.Table))
        {
            hoverBorder     = controlGlobal.ColorBorder;
            icon            = dark ? Colors.White : new PrimeNGShade(900).Resolve(surface);
            addOnBackground = controlGlobal.ColorBgContainer;
            var ring = new BoxShadows(new BoxShadow { Spread = controlGlobal.LineWidth, Color = controlGlobal.ColorPrimary });
            controlGlobal.InputActiveShadow      = ring;
            controlGlobal.InputErrorActiveShadow = ring;
        }
        else
        {
            hoverBorder     = new PrimeNGShade(dark ? 500 : 400).Resolve(surface);
            icon            = new PrimeNGShade(400).Resolve(surface);
            addOnBackground = controlGlobal.ColorBgContainer;
        }

        controlGlobal.ColorPrimaryHover     = hoverBorder;
        controlGlobal.ColorErrorBorderHover = hoverBorder;
        controlGlobal.ColorTextDisabled     = controlGlobal.ColorTextSecondary;
        controlGlobal.ColorTextQuaternary   = icon;
        controlGlobal.ColorIcon             = icon;
        controlGlobal.ColorIconHover        = icon;
        controlGlobal.ColorFillAlter        = addOnBackground;
        controlGlobal.BorderRadiusSM        = controlGlobal.BorderRadius;
        controlGlobal.BorderRadiusLG        = controlGlobal.BorderRadius;
    }

    /// <summary>
    /// PrimeUIX <c>form.field.hover.border.color</c>, also the unchecked checkbox / radio hover border: Aura surface
    /// 400 (dark 500), Lara the primary colour, Nora the resting border colour.
    /// </summary>
    internal static Color HoverBorder(in ThemeAlgorithmContext context, DesignToken global)
    {
        var preset = PrimeNGPresets.Resolve(context);
        if (ReferenceEquals(preset, PrimeNGLaraPreset.Table))
        {
            return global.ColorPrimary;
        }
        if (ReferenceEquals(preset, PrimeNGNoraPreset.Table))
        {
            return global.ColorBorder;
        }
        var surface = PrimeNGPresets.Rules(context, global).Surface.AsSpan();
        return new PrimeNGShade(context.Appearance == ThemeAppearance.Dark ? 500 : 400).Resolve(surface);
    }
}

/// <summary>The PrimeUIX form field for <see cref="LineEditToken"/>; see <see cref="PrimeNGInputStage"/>.</summary>
[ControlTokenAlgorithm("PrimeNG", "Aura", 1)]
[ControlTokenAlgorithm("PrimeNG", "Aura.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora.Dark", 1)]
internal sealed class PrimeNGLineEditTokens : IControlTokenAlgorithm<LineEditToken>
{
    /// <inheritdoc />
    public void EvaluateGlobal(in ThemeAlgorithmContext context, DesignToken controlGlobal)
    {
        PrimeNGInputStage.Apply(context, controlGlobal);
    }

    /// <inheritdoc />
    public void EvaluateOwn(in ThemeAlgorithmContext context, DesignToken effectiveGlobal, LineEditToken token)
    {
        // PrimeUIX form.field.sm.font.size 0.75rem.
        token.InputFontSizeSM = effectiveGlobal.FontSizeSM;
    }
}

/// <summary>
/// The PrimeUIX form field for <see cref="SearchEditToken"/> (an input group of an input text and a button); see
/// <see cref="PrimeNGInputStage"/>.
/// </summary>
[ControlTokenAlgorithm("PrimeNG", "Aura", 1)]
[ControlTokenAlgorithm("PrimeNG", "Aura.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora.Dark", 1)]
internal sealed class PrimeNGSearchEditTokens : IControlTokenAlgorithm<SearchEditToken>
{
    /// <inheritdoc />
    public void EvaluateGlobal(in ThemeAlgorithmContext context, DesignToken controlGlobal)
    {
        PrimeNGInputStage.Apply(context, controlGlobal);
    }

    /// <inheritdoc />
    public void EvaluateOwn(in ThemeAlgorithmContext context, DesignToken effectiveGlobal, SearchEditToken token)
    {
        // PrimeUIX form.field.sm.font.size 0.75rem.
        token.InputFontSizeSM = effectiveGlobal.FontSizeSM;
    }
}

/// <summary>The PrimeUIX form field for <see cref="TextAreaToken"/>; see <see cref="PrimeNGInputStage"/>.</summary>
[ControlTokenAlgorithm("PrimeNG", "Aura", 1)]
[ControlTokenAlgorithm("PrimeNG", "Aura.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora.Dark", 1)]
internal sealed class PrimeNGTextAreaTokens : IControlTokenAlgorithm<TextAreaToken>
{
    /// <inheritdoc />
    public void EvaluateGlobal(in ThemeAlgorithmContext context, DesignToken controlGlobal)
    {
        PrimeNGInputStage.Apply(context, controlGlobal);
    }

    /// <inheritdoc />
    public void EvaluateOwn(in ThemeAlgorithmContext context, DesignToken effectiveGlobal, TextAreaToken token)
    {
    }
}

/// <summary>The PrimeUIX form field for <see cref="AutoCompleteToken"/>; see <see cref="PrimeNGInputStage"/>.</summary>
[ControlTokenAlgorithm("PrimeNG", "Aura", 1)]
[ControlTokenAlgorithm("PrimeNG", "Aura.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora.Dark", 1)]
internal sealed class PrimeNGAutoCompleteTokens : IControlTokenAlgorithm<AutoCompleteToken>
{
    /// <inheritdoc />
    public void EvaluateGlobal(in ThemeAlgorithmContext context, DesignToken controlGlobal)
    {
        PrimeNGInputStage.Apply(context, controlGlobal);
    }

    /// <inheritdoc />
    public void EvaluateOwn(in ThemeAlgorithmContext context, DesignToken effectiveGlobal, AutoCompleteToken token)
    {
        // PrimeUIX list option: option.color is the text colour, the selected option highlight.color on the highlight.
        token.OptionColor         = effectiveGlobal.ColorText;
        token.OptionSelectedColor = effectiveGlobal.ControlItemColorActive;
        PrimeNGListOptions.Apply(context, effectiveGlobal, token);
    }
}

/// <summary>The PrimeUIX form field and list options for <see cref="AutoCompleteSearchEditToken"/>; see <see cref="PrimeNGInputStage"/>.</summary>
[ControlTokenAlgorithm("PrimeNG", "Aura", 1)]
[ControlTokenAlgorithm("PrimeNG", "Aura.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora.Dark", 1)]
internal sealed class PrimeNGAutoCompleteSearchEditTokens : IControlTokenAlgorithm<AutoCompleteSearchEditToken>
{
    /// <inheritdoc />
    public void EvaluateGlobal(in ThemeAlgorithmContext context, DesignToken controlGlobal)
    {
        PrimeNGInputStage.Apply(context, controlGlobal);
    }

    /// <inheritdoc />
    public void EvaluateOwn(in ThemeAlgorithmContext context, DesignToken effectiveGlobal, AutoCompleteSearchEditToken token)
    {
        token.OptionColor         = effectiveGlobal.ColorText;
        token.OptionSelectedColor = effectiveGlobal.ControlItemColorActive;
        PrimeNGListOptions.Apply(context, effectiveGlobal, token);
    }
}

/// <summary>The PrimeUIX form field and list options for <see cref="AutoCompleteTextAreaToken"/>; see <see cref="PrimeNGInputStage"/>.</summary>
[ControlTokenAlgorithm("PrimeNG", "Aura", 1)]
[ControlTokenAlgorithm("PrimeNG", "Aura.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora.Dark", 1)]
internal sealed class PrimeNGAutoCompleteTextAreaTokens : IControlTokenAlgorithm<AutoCompleteTextAreaToken>
{
    /// <inheritdoc />
    public void EvaluateGlobal(in ThemeAlgorithmContext context, DesignToken controlGlobal)
    {
        PrimeNGInputStage.Apply(context, controlGlobal);
    }

    /// <inheritdoc />
    public void EvaluateOwn(in ThemeAlgorithmContext context, DesignToken effectiveGlobal, AutoCompleteTextAreaToken token)
    {
        token.OptionColor         = effectiveGlobal.ColorText;
        token.OptionSelectedColor = effectiveGlobal.ControlItemColorActive;
        PrimeNGListOptions.Apply(context, effectiveGlobal, token);
    }
}

/// <summary>The PrimeUIX form field for <see cref="MentionsToken"/>; see <see cref="PrimeNGInputStage"/>.</summary>
[ControlTokenAlgorithm("PrimeNG", "Aura", 1)]
[ControlTokenAlgorithm("PrimeNG", "Aura.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora.Dark", 1)]
internal sealed class PrimeNGMentionsTokens : IControlTokenAlgorithm<MentionsToken>
{
    /// <inheritdoc />
    public void EvaluateGlobal(in ThemeAlgorithmContext context, DesignToken controlGlobal)
    {
        PrimeNGInputStage.Apply(context, controlGlobal);
    }

    /// <inheritdoc />
    public void EvaluateOwn(in ThemeAlgorithmContext context, DesignToken effectiveGlobal, MentionsToken token)
    {
        // PrimeUIX list option: option.color is the text colour, the selected option highlight.color on the highlight.
        token.OptionColor         = effectiveGlobal.ColorText;
        token.OptionSelectedColor = effectiveGlobal.ControlItemColorActive;
    }
}

/// <summary>The PrimeUIX form field for <see cref="CascaderToken"/>; see <see cref="PrimeNGInputStage"/>.</summary>
[ControlTokenAlgorithm("PrimeNG", "Aura", 1)]
[ControlTokenAlgorithm("PrimeNG", "Aura.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora.Dark", 1)]
internal sealed class PrimeNGCascaderTokens : IControlTokenAlgorithm<CascaderToken>
{
    /// <inheritdoc />
    public void EvaluateGlobal(in ThemeAlgorithmContext context, DesignToken controlGlobal)
    {
        PrimeNGInputStage.Apply(context, controlGlobal);
    }

    /// <inheritdoc />
    public void EvaluateOwn(in ThemeAlgorithmContext context, DesignToken effectiveGlobal, CascaderToken token)
    {
        // PrimeUIX cascadeselect: option.padding as the select options, normal selected weight, list.padding for the
        // columns, list.option.focus.background on hover and the chip.background for multiple values.
        // PrimeUIX form.field.sm.font.size 0.75rem.
        token.InputFontSizeSM = effectiveGlobal.FontSizeSM;
        var controls = PrimeNGPresets.Resolve(context).Controls;
        token.OptionPadding            = controls.SelectOptionPadding(effectiveGlobal);
        token.OptionSelectedFontWeight = FontWeight.Normal;
        token.MenuPadding              = controls.SelectPopupContentPadding(effectiveGlobal);
        // list.option.border.radius and list.gap as the Select's options: Aura 4 with a 2px gap, Lara and Nora square.
        var aura = PrimeNGPresets.KindOf(context) == PrimeNGPresetKind.Aura;
        token.OptionBorderRadius       = new CornerRadius(aura ? PrimeNGListOptions.AuraOptionRadius : 0);
        token.OptionMargin             = aura ? PrimeNGListOptions.AuraOptionMargin : default;
        if (aura)
        {
            token.MenuPadding = PrimeNGListOptions.InsetForMargin(token.MenuPadding);
        }
        token.OptionHoverBg            = effectiveGlobal.ControlItemBgHover;
        token.MultipleItemBg           = effectiveGlobal.ControlItemBgHover;
    }
}

/// <summary>The PrimeUIX form field for <see cref="TreeSelectToken"/>; see <see cref="PrimeNGInputStage"/>.</summary>
[ControlTokenAlgorithm("PrimeNG", "Aura", 1)]
[ControlTokenAlgorithm("PrimeNG", "Aura.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora.Dark", 1)]
internal sealed class PrimeNGTreeSelectTokens : IControlTokenAlgorithm<TreeSelectToken>
{
    /// <inheritdoc />
    public void EvaluateGlobal(in ThemeAlgorithmContext context, DesignToken controlGlobal)
    {
        PrimeNGInputStage.Apply(context, controlGlobal);
    }

    /// <inheritdoc />
    public void EvaluateOwn(in ThemeAlgorithmContext context, DesignToken effectiveGlobal, TreeSelectToken token)
    {
        // PrimeUIX treeselect: tree.padding = list.padding, tree.node.hover.background = list.option.focus.background,
        // chip.background for multiple values.
        // PrimeUIX form.field.sm.font.size 0.75rem.
        token.InputFontSizeSM = effectiveGlobal.FontSizeSM;
        token.PopupContentPadding = PrimeNGPresets.Resolve(context).Controls.SelectPopupContentPadding(effectiveGlobal);
        if (PrimeNGPresets.KindOf(context) == PrimeNGPresetKind.Aura)
        {
            // list.padding 4 all round: the rows already leave their 2px tree.gap below them.
            token.PopupContentPadding = new Thickness(4, 4, 4, 2);
        }
        token.NodeHoverBg         = effectiveGlobal.ControlItemBgHover;
        token.MultipleItemBg      = effectiveGlobal.ControlItemBgHover;
    }
}

/// <summary>The PrimeUIX form field for <see cref="DatePickerToken"/>; see <see cref="PrimeNGInputStage"/>.</summary>
[ControlTokenAlgorithm("PrimeNG", "Aura", 1)]
[ControlTokenAlgorithm("PrimeNG", "Aura.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora.Dark", 1)]
internal sealed class PrimeNGDatePickerTokens : IControlTokenAlgorithm<DatePickerToken>
{
    /// <inheritdoc />
    public void EvaluateGlobal(in ThemeAlgorithmContext context, DesignToken controlGlobal)
    {
        PrimeNGInputStage.Apply(context, controlGlobal);
    }

    /// <inheritdoc />
    public void EvaluateOwn(in ThemeAlgorithmContext context, DesignToken effectiveGlobal, DatePickerToken token)
    {
        // PrimeUIX overlays draw no pointer arrow.
        token.PopupArrowVisible = false;
        // PrimeUIX datepicker panel.padding 0.625rem (Lara 0.875rem) and buttonbar.padding 0.5rem (Lara 0.625rem) 0 0 0.
        var lara = PrimeNGPresets.KindOf(context) == PrimeNGPresetKind.Lara;
        token.PanelContentPadding = new Thickness(lara ? 14 : 10);
        token.ButtonsPanelMargin  = new Thickness(0, lara ? 10 : 8, 0, 0);
    }
}

/// <summary>The PrimeUIX form field for <see cref="RangeDatePickerToken"/>; see <see cref="PrimeNGInputStage"/>.</summary>
[ControlTokenAlgorithm("PrimeNG", "Aura", 1)]
[ControlTokenAlgorithm("PrimeNG", "Aura.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora.Dark", 1)]
internal sealed class PrimeNGRangeDatePickerTokens : IControlTokenAlgorithm<RangeDatePickerToken>
{
    /// <inheritdoc />
    public void EvaluateGlobal(in ThemeAlgorithmContext context, DesignToken controlGlobal)
    {
        PrimeNGInputStage.Apply(context, controlGlobal);
    }

    /// <inheritdoc />
    public void EvaluateOwn(in ThemeAlgorithmContext context, DesignToken effectiveGlobal, RangeDatePickerToken token)
    {
        // PrimeUIX overlays draw no pointer arrow.
        token.PopupArrowVisible = false;
        // PrimeUIX datepicker panel.padding 0.625rem (Lara 0.875rem) and buttonbar.padding 0.5rem (Lara 0.625rem) 0 0 0.
        var lara = PrimeNGPresets.KindOf(context) == PrimeNGPresetKind.Lara;
        token.PanelContentPadding = new Thickness(lara ? 14 : 10);
        token.ButtonsPanelMargin  = new Thickness(0, lara ? 10 : 8, 0, 0);
        // group.gap 0.625rem (Lara 0.875rem) on each side of the 1px line between the months.
        token.RangePanelSpacing = 2 * (lara ? 14 : 10) + effectiveGlobal.LineWidth;
    }
}

/// <summary>The PrimeUIX form field for <see cref="TimePickerToken"/>; see <see cref="PrimeNGInputStage"/>.</summary>
[ControlTokenAlgorithm("PrimeNG", "Aura", 1)]
[ControlTokenAlgorithm("PrimeNG", "Aura.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora.Dark", 1)]
internal sealed class PrimeNGTimePickerTokens : IControlTokenAlgorithm<TimePickerToken>
{
    /// <inheritdoc />
    public void EvaluateGlobal(in ThemeAlgorithmContext context, DesignToken controlGlobal)
    {
        PrimeNGInputStage.Apply(context, controlGlobal);
    }

    /// <inheritdoc />
    public void EvaluateOwn(in ThemeAlgorithmContext context, DesignToken effectiveGlobal, TimePickerToken token)
    {
        // PrimeUIX overlays draw no pointer arrow.
        token.PopupArrowVisible = false;
    }
}

/// <summary>The PrimeUIX form field for <see cref="RangeTimePickerToken"/>; see <see cref="PrimeNGInputStage"/>.</summary>
[ControlTokenAlgorithm("PrimeNG", "Aura", 1)]
[ControlTokenAlgorithm("PrimeNG", "Aura.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora.Dark", 1)]
internal sealed class PrimeNGRangeTimePickerTokens : IControlTokenAlgorithm<RangeTimePickerToken>
{
    /// <inheritdoc />
    public void EvaluateGlobal(in ThemeAlgorithmContext context, DesignToken controlGlobal)
    {
        PrimeNGInputStage.Apply(context, controlGlobal);
    }

    /// <inheritdoc />
    public void EvaluateOwn(in ThemeAlgorithmContext context, DesignToken effectiveGlobal, RangeTimePickerToken token)
    {
        // PrimeUIX overlays draw no pointer arrow.
        token.PopupArrowVisible = false;
    }
}

/// <summary>
/// The PrimeUIX list options of the candidate popups (AutoComplete), as the Select's: <c>list.padding</c> around the
/// rows, the option height of <c>list.option.padding</c> and, in Aura, <c>list.option.border.radius</c> 4 with a 2px <c>list.gap</c> (a 1px margin above and below each
/// row); Lara and Nora rows are square without a gap.
/// </summary>
internal static class PrimeNGListOptions
{
    internal const double AuraOptionRadius = 4;
    internal static readonly Thickness AuraOptionMargin = new(0, 1);

    internal static void Apply(in ThemeAlgorithmContext context, DesignToken effectiveGlobal, AbstractAutoCompleteToken token)
    {
        var controls = PrimeNGPresets.Resolve(context).Controls;
        token.PopupContentPadding = controls.SelectPopupContentPadding(effectiveGlobal);
        // The row the Select renders: list.option.padding around the text, at least its FontHeight line (Lara's 38
        // exceeds the 1.5 line). Aura's Select row also holds the 1px background inset on each side, which is the
        // margin outside the candidate row here.
        var padding = controls.SelectOptionPadding(effectiveGlobal);
        var line    = Math.Max(effectiveGlobal.FontSize * 1.5, effectiveGlobal.FontHeight);
        token.OptionHeight = PrimeNGPresets.KindOf(context) == PrimeNGPresetKind.Aura
            ? Math.Max(effectiveGlobal.FontSize * 1.5 + padding.Top + padding.Bottom + 2, line + padding.Top + padding.Bottom) - 2
            : line + padding.Top + padding.Bottom;
        if (PrimeNGPresets.KindOf(context) == PrimeNGPresetKind.Aura)
        {
            token.OptionBorderRadius  = new CornerRadius(AuraOptionRadius);
            token.OptionMargin        = AuraOptionMargin;
            token.PopupContentPadding = InsetForMargin(token.PopupContentPadding);
        }
    }

    /// <summary>
    /// The list padding less the rows' 1px vertical margin, so the first and last rows keep <c>list.padding</c> from
    /// the panel edge.
    /// </summary>
    internal static Thickness InsetForMargin(Thickness padding) =>
        new(padding.Left, padding.Top - AuraOptionMargin.Top, padding.Right, padding.Bottom - AuraOptionMargin.Bottom);
}
