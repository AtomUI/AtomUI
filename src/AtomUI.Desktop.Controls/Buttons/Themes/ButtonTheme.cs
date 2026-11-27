using AtomUI.Controls.Converters;
using Avalonia.Styling;

namespace AtomUI.Desktop.Controls.Themes;

/// <summary>
/// Code-behind of the theme of the standard button. The template and the styles are declared in the AXAML part;
/// other button themes, such as the reset and submit button themes, include it to reuse them.
/// </summary>
public class ButtonTheme : ControlTheme
{
    /// <summary>
    /// Shared instance of <see cref="ButtonIconVisibleConverter" />, which returns whether a button shows its icon.
    /// The built-in button templates no longer reference it.
    /// </summary>
    public static readonly ButtonIconVisibleConverter IconVisibleConverter = new();
    /// <summary>
    /// Dash pattern of the border frame of a dashed button, as alternating dash and gap lengths. The button theme
    /// and the dropdown button theme share it.
    /// </summary>
    public static readonly IReadOnlyList<double> DashedStyle = [4, 2];
}