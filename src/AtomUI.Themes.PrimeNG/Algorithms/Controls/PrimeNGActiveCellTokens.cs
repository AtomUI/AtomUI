using AtomUI.Desktop.Controls;
using AtomUI.Theme.Algorithms;
using AtomUI.Theme.DesignTokens;

namespace AtomUI.Themes.PrimeNG.Algorithms.Controls;

// The content presentation of Calendar (fullscreen) and DateViewer (LunarCalendar draws its selected cells with the
// contrast colour already) draws today's and the selected cell in the primary
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
        if (PrimeNGPresets.HasSolidHighlight(effectiveGlobal))
        {
            token.CellActiveColor = effectiveGlobal.ControlItemColorActive;
        }
    }
}
