using AtomUI.Theme.DesignTokens;
using Avalonia;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Own design tokens of the <see cref="RangeDatePicker" />. The tokens it shares with the single date picker are in
/// <see cref="AbstractDatePickerToken" />.
/// </summary>
[ControlDesignToken]
public sealed partial class RangeDatePickerToken : AbstractDatePickerToken
{
    /// <summary>
    /// Margin around the arrow between the start and end date fields.
    /// </summary>
    public partial Thickness RangePickerArrowMargin { get; set; }
    /// <summary>
    /// Thickness of the underline marking the active date field.
    /// </summary>
    public partial double RangePickerIndicatorThickness { get; set; }
    /// <summary>
    /// Spacing between the range picker's calendar panels.
    /// </summary>
    public partial double RangePanelSpacing { get; set; }
}
