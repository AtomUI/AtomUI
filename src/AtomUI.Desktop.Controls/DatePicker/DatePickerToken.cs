using AtomUI.Theme.DesignTokens;
using Avalonia;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
public sealed partial class DatePickerToken : AbstractDatePickerToken
{

    /// <summary>Thickness of the border around the calendar header in the date picker's popup.</summary>
    public partial Thickness HeaderBorderThickness { get; set; }
}
