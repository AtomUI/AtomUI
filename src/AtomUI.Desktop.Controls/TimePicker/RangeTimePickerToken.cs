using AtomUI.Theme.DesignTokens;
using Avalonia;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Own tokens of <see cref="RangeTimePicker" />: the tokens shared by all time pickers, plus the
/// arrow between the two times, the indicator of the field being edited and the gap between the
/// popup and the input anchor.
/// </summary>
[ControlDesignToken]
public sealed partial class RangeTimePickerToken : AbstractTimePickerToken
{
    /// <summary>
    /// Space around the arrow between the two range values.
    /// </summary>
    public partial Thickness RangePickerArrowMargin { get; set; }
    /// <summary>
    /// Thickness of the indicator that marks the active range field.
    /// </summary>
    public partial double RangePickerIndicatorThickness { get; set; }
    /// <summary>
    /// Gap between the time-range popup and its input anchor.
    /// </summary>
    public partial double RangeMarginToAnchor { get; set; }
}
