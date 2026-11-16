using AtomUI.Desktop.Controls;
using AtomUI.Theme.Algorithms;
using AtomUI.Theme.DesignTokens;

namespace AtomUI.Themes.PrimeNG.Algorithms.Controls;

// The date and time cells (TimeViewCell, the DateViewer cells of the pickers, Calendar and DateViewer) draw ColorText or
// the primary colour on ControlItemBgActive and have no selected-text token. Under a solid highlight (Nora, or noir in
// any preset) their owners take the pale primary back (ColorPrimaryBg / ColorPrimaryBgHover, the Ant Design formula),
// so the selected cell, the range middle and today's cell stay readable. Under Aura / Lara's pale highlight they write
// nothing.
internal static class PrimeNGPaleHighlight
{
    /// <summary>
    /// Restores the pale selected background on a control's own global tokens when the highlight is solid.
    /// </summary>
    internal static void Apply(DesignToken controlGlobal)
    {
        if (PrimeNGPresets.HasSolidHighlight(controlGlobal))
        {
            controlGlobal.ControlItemBgActive      = controlGlobal.ColorPrimaryBg;
            controlGlobal.ControlItemBgActiveHover = controlGlobal.ColorPrimaryBgHover;
        }
    }
}

/// <summary>
/// DatePicker under the PrimeNG presets: the pale selected background under a solid highlight (see
/// <see cref="PrimeNGPaleHighlight"/>).
/// </summary>
[ControlTokenAlgorithm("PrimeNG", "Aura", 1)]
[ControlTokenAlgorithm("PrimeNG", "Aura.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora.Dark", 1)]
internal sealed class PrimeNGDatePickerPaleHighlightTokens : IControlTokenAlgorithm<DatePickerToken>
{
    /// <inheritdoc />
    public void EvaluateGlobal(in ThemeAlgorithmContext context, DesignToken controlGlobal) =>
        PrimeNGPaleHighlight.Apply(controlGlobal);

    /// <inheritdoc />
    public void EvaluateOwn(in ThemeAlgorithmContext context, DesignToken effectiveGlobal, DatePickerToken token)
    {
    }
}

/// <summary>
/// RangeDatePicker under the PrimeNG presets: the pale selected background under a solid highlight (see
/// <see cref="PrimeNGPaleHighlight"/>).
/// </summary>
[ControlTokenAlgorithm("PrimeNG", "Aura", 1)]
[ControlTokenAlgorithm("PrimeNG", "Aura.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora.Dark", 1)]
internal sealed class PrimeNGRangeDatePickerPaleHighlightTokens : IControlTokenAlgorithm<RangeDatePickerToken>
{
    /// <inheritdoc />
    public void EvaluateGlobal(in ThemeAlgorithmContext context, DesignToken controlGlobal) =>
        PrimeNGPaleHighlight.Apply(controlGlobal);

    /// <inheritdoc />
    public void EvaluateOwn(in ThemeAlgorithmContext context, DesignToken effectiveGlobal, RangeDatePickerToken token)
    {
    }
}

/// <summary>
/// TimePicker under the PrimeNG presets: the pale selected background under a solid highlight (see
/// <see cref="PrimeNGPaleHighlight"/>).
/// </summary>
[ControlTokenAlgorithm("PrimeNG", "Aura", 1)]
[ControlTokenAlgorithm("PrimeNG", "Aura.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora.Dark", 1)]
internal sealed class PrimeNGTimePickerPaleHighlightTokens : IControlTokenAlgorithm<TimePickerToken>
{
    /// <inheritdoc />
    public void EvaluateGlobal(in ThemeAlgorithmContext context, DesignToken controlGlobal) =>
        PrimeNGPaleHighlight.Apply(controlGlobal);

    /// <inheritdoc />
    public void EvaluateOwn(in ThemeAlgorithmContext context, DesignToken effectiveGlobal, TimePickerToken token)
    {
    }
}

/// <summary>
/// RangeTimePicker under the PrimeNG presets: the pale selected background under a solid highlight (see
/// <see cref="PrimeNGPaleHighlight"/>).
/// </summary>
[ControlTokenAlgorithm("PrimeNG", "Aura", 1)]
[ControlTokenAlgorithm("PrimeNG", "Aura.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora.Dark", 1)]
internal sealed class PrimeNGRangeTimePickerPaleHighlightTokens : IControlTokenAlgorithm<RangeTimePickerToken>
{
    /// <inheritdoc />
    public void EvaluateGlobal(in ThemeAlgorithmContext context, DesignToken controlGlobal) =>
        PrimeNGPaleHighlight.Apply(controlGlobal);

    /// <inheritdoc />
    public void EvaluateOwn(in ThemeAlgorithmContext context, DesignToken effectiveGlobal, RangeTimePickerToken token)
    {
    }
}

/// <summary>
/// Calendar under the PrimeNG presets: the pale selected background under a solid highlight (see
/// <see cref="PrimeNGPaleHighlight"/>).
/// </summary>
[ControlTokenAlgorithm("PrimeNG", "Aura", 1)]
[ControlTokenAlgorithm("PrimeNG", "Aura.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora.Dark", 1)]
internal sealed class PrimeNGCalendarPaleHighlightTokens : IControlTokenAlgorithm<CalendarToken>
{
    /// <inheritdoc />
    public void EvaluateGlobal(in ThemeAlgorithmContext context, DesignToken controlGlobal) =>
        PrimeNGPaleHighlight.Apply(controlGlobal);

    /// <inheritdoc />
    public void EvaluateOwn(in ThemeAlgorithmContext context, DesignToken effectiveGlobal, CalendarToken token)
    {
    }
}

/// <summary>
/// LunarCalendar under the PrimeNG presets: the pale selected background under a solid highlight (see
/// <see cref="PrimeNGPaleHighlight"/>).
/// </summary>
[ControlTokenAlgorithm("PrimeNG", "Aura", 1)]
[ControlTokenAlgorithm("PrimeNG", "Aura.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora.Dark", 1)]
internal sealed class PrimeNGLunarCalendarPaleHighlightTokens : IControlTokenAlgorithm<LunarCalendarToken>
{
    /// <inheritdoc />
    public void EvaluateGlobal(in ThemeAlgorithmContext context, DesignToken controlGlobal) =>
        PrimeNGPaleHighlight.Apply(controlGlobal);

    /// <inheritdoc />
    public void EvaluateOwn(in ThemeAlgorithmContext context, DesignToken effectiveGlobal, LunarCalendarToken token)
    {
    }
}

/// <summary>
/// DateViewer under the PrimeNG presets: the pale selected background under a solid highlight (see
/// <see cref="PrimeNGPaleHighlight"/>).
/// </summary>
[ControlTokenAlgorithm("PrimeNG", "Aura", 1)]
[ControlTokenAlgorithm("PrimeNG", "Aura.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora.Dark", 1)]
internal sealed class PrimeNGDateViewerPaleHighlightTokens : IControlTokenAlgorithm<DateViewerToken>
{
    /// <inheritdoc />
    public void EvaluateGlobal(in ThemeAlgorithmContext context, DesignToken controlGlobal) =>
        PrimeNGPaleHighlight.Apply(controlGlobal);

    /// <inheritdoc />
    public void EvaluateOwn(in ThemeAlgorithmContext context, DesignToken effectiveGlobal, DateViewerToken token)
    {
    }
}

/// <summary>
/// RangeDateViewer under the PrimeNG presets: the pale selected background under a solid highlight (see
/// <see cref="PrimeNGPaleHighlight"/>).
/// </summary>
[ControlTokenAlgorithm("PrimeNG", "Aura", 1)]
[ControlTokenAlgorithm("PrimeNG", "Aura.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora.Dark", 1)]
internal sealed class PrimeNGRangeDateViewerPaleHighlightTokens : IControlTokenAlgorithm<RangeDateViewerToken>
{
    /// <inheritdoc />
    public void EvaluateGlobal(in ThemeAlgorithmContext context, DesignToken controlGlobal) =>
        PrimeNGPaleHighlight.Apply(controlGlobal);

    /// <inheritdoc />
    public void EvaluateOwn(in ThemeAlgorithmContext context, DesignToken effectiveGlobal, RangeDateViewerToken token)
    {
    }
}
