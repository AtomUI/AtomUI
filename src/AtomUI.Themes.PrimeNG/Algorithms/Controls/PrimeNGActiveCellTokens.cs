using AtomUI.Desktop.Controls;
using AtomUI.Theme.Algorithms;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Themes.PrimeNG.Algorithms.Controls;

// The content presentation of Calendar (fullscreen) and DateViewer draws today's and the selected cell in the primary
// colour on ControlItemBgActive (Ant Design). Under a solid highlight (Nora, or noir in any preset) that text would
// vanish on the primary, so it takes the highlight text ControlItemColorActive (PrimeUIX highlight.color); under the
// pale highlight of Aura / Lara the Ant Design primary text stays. The compact cells (pickers) already follow PrimeUIX
// datepicker: selected date primary.color with primary.contrast.color, range middle highlight.background with
// highlight.color.

/// <summary>
/// Calendar under the PrimeNG presets: the fullscreen selected cell's text under a solid highlight.
/// </summary>
[ControlTokenAlgorithm("PrimeNG", "Aura", 1)]
[ControlTokenAlgorithm("PrimeNG", "Aura.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora.Dark", 1)]
internal sealed class PrimeNGCalendarTokens : IControlTokenAlgorithm<CalendarToken>
{
    /// <inheritdoc />
    public void EvaluateGlobal(in ThemeAlgorithmContext context, DesignToken controlGlobal)
    {
    }

    /// <inheritdoc />
    public void EvaluateOwn(in ThemeAlgorithmContext context, DesignToken effectiveGlobal, CalendarToken token)
    {
        if (PrimeNGPresets.HasSolidHighlight(effectiveGlobal))
        {
            token.ItemActiveColor = effectiveGlobal.ControlItemColorActive;
        }
    }
}

/// <summary>
/// LunarCalendar under the PrimeNG presets: its selected cells always draw their text in the contrast colour
/// <c>ColorTextLightSolid</c> (the lunar cell content), on the primary colour for a date and on <c>ItemActiveBg</c>
/// (Ant Design: <c>ControlItemBgActive</c>) for a month in the fullscreen year view. Under the pale highlight of Aura /
/// Lara that month is white on primary 50. PrimeUIX <c>datepicker</c> draws a selected date or month as
/// <c>date.selected.background</c> / <c>month.selected.background</c> = <c>primary.color</c> with
/// <c>primary.contrast.color</c>, so the LunarCalendar's selected background is the primary in every preset (under a
/// solid highlight, Nora or noir, that is the value it already had) and <c>ItemActiveColor</c>, the text of a selected
/// cell without lunar content, the contrast colour.
/// </summary>
[ControlTokenAlgorithm("PrimeNG", "Aura", 1)]
[ControlTokenAlgorithm("PrimeNG", "Aura.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora.Dark", 1)]
internal sealed class PrimeNGLunarCalendarTokens : IControlTokenAlgorithm<LunarCalendarToken>
{
    /// <inheritdoc />
    public void EvaluateGlobal(in ThemeAlgorithmContext context, DesignToken controlGlobal)
    {
    }

    /// <inheritdoc />
    public void EvaluateOwn(in ThemeAlgorithmContext context, DesignToken effectiveGlobal, LunarCalendarToken token)
    {
        token.ItemActiveBg    = effectiveGlobal.ColorPrimary;
        token.ItemActiveColor = effectiveGlobal.ColorTextLightSolid;
    }
}

/// <summary>
/// DateViewer under the PrimeNG presets: today's and the selected content cell's text under a solid highlight.
/// </summary>
[ControlTokenAlgorithm("PrimeNG", "Aura", 1)]
[ControlTokenAlgorithm("PrimeNG", "Aura.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora.Dark", 1)]
internal sealed class PrimeNGDateViewerTokens : IControlTokenAlgorithm<DateViewerToken>
{
    /// <inheritdoc />
    public void EvaluateGlobal(in ThemeAlgorithmContext context, DesignToken controlGlobal)
    {
    }

    /// <inheritdoc />
    public void EvaluateOwn(in ThemeAlgorithmContext context, DesignToken effectiveGlobal, DateViewerToken token)
    {
        PrimeNGDatePanel.Apply(context, effectiveGlobal, token);
        if (PrimeNGPresets.HasSolidHighlight(effectiveGlobal))
        {
            token.CellActiveColor = effectiveGlobal.ControlItemColorActive;
        }
    }
}

/// <summary>
/// RangeDateViewer under the PrimeNG presets: today's and the selected content cell's text under a solid highlight.
/// </summary>
[ControlTokenAlgorithm("PrimeNG", "Aura", 1)]
[ControlTokenAlgorithm("PrimeNG", "Aura.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora.Dark", 1)]
internal sealed class PrimeNGRangeDateViewerTokens : IControlTokenAlgorithm<RangeDateViewerToken>
{
    /// <inheritdoc />
    public void EvaluateGlobal(in ThemeAlgorithmContext context, DesignToken controlGlobal)
    {
    }

    /// <inheritdoc />
    public void EvaluateOwn(in ThemeAlgorithmContext context, DesignToken effectiveGlobal, RangeDateViewerToken token)
    {
        PrimeNGDatePanel.Apply(context, effectiveGlobal, token);
        if (PrimeNGPresets.HasSolidHighlight(effectiveGlobal))
        {
            token.CellActiveColor = effectiveGlobal.ControlItemColorActive;
        }
    }
}

/// <summary>
/// The date panel of the pickers and the compact date viewers, PrimeUIX 3.1.1 <c>datepicker</c>: day cells of
/// <c>date.width</c> 1.75rem (Lara 2.25rem) with <c>date.padding</c> 0.25rem (Lara 0.375rem) around them, so seven
/// columns make the panel width; <c>day.view.margin</c> 0.5rem (Lara 0.625rem) under the header;
/// <c>date.hover.background</c> (content.hover.background), <c>panel.background</c> (content.background),
/// <c>date.border.radius</c> 50% and the borderless <c>today</c> chip (surface 200 / 900 text, dark 700 / white).
/// </summary>
internal static class PrimeNGDatePanel
{
    internal static void Apply(in ThemeAlgorithmContext context, DesignToken effectiveGlobal, AbstractDateViewerToken token)
    {
        var lara    = PrimeNGPresets.KindOf(context) == PrimeNGPresetKind.Lara;
        var cell    = lara ? 36 : 28;
        var padding = lara ? 6 : 4;
        token.CellWidth       = cell;
        token.CellHeight      = cell;
        token.CellRowHeight   = cell + 2 * padding;
        token.PanelWidth      = 7 * (cell + 2 * padding);
        token.DateBodyPadding = new Thickness(0, lara ? 10 : 8, 0, 0);
        token.CellHoverBg     = effectiveGlobal.ControlItemBgHover;
        token.PanelBg         = effectiveGlobal.ColorBgElevated;

        var dark = context.Appearance == ThemeAppearance.Dark;
        token.CellCornerRadius     = new CornerRadius(cell / 2.0);
        token.CellTodayBg          = PrimeNGPresets.Surface(context, dark ? 700 : 200);
        token.CellTodayColor       = dark ? Colors.White : PrimeNGPresets.Surface(context, 900);
        token.CellTodayBorderColor = Colors.Transparent;

        // select.month / select.year font.weight 500 in every preset; week.day.font.weight 500 (Lara 700) in a
        // weekday cell of week.day.padding 0.25rem (Lara 0.375rem) around the 1rem table line (measured 32px on Aura).
        token.HeaderTitleFontWeight = FontWeight.Medium;
        token.WeekHeaderFontWeight  = lara ? FontWeight.Bold : FontWeight.Medium;
        token.WeekHeaderHeight      = 24 + 2 * padding;
    }
}
