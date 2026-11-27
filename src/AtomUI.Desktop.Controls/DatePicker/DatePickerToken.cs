using AtomUI.Theme.DesignTokens;
using Avalonia;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Own design tokens of the single <see cref="DatePicker" />. The tokens it shares with the range picker are in
/// <see cref="AbstractDatePickerToken" />.
/// </summary>
[ControlDesignToken]
public sealed partial class DatePickerToken : AbstractDatePickerToken
{

    /// <summary>
    /// Thickness of the border around the calendar header in the date picker's popup.
    /// </summary>
    public partial Thickness HeaderBorderThickness { get; set; }
}
