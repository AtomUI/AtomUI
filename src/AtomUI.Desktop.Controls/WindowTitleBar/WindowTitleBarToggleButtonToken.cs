using AtomUI.Theme.DesignTokens;
using Avalonia;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Design token of the title bar toggle button. It adds the corner radius of the toggle button to the colors and
/// caption button metrics of <see cref="AbstractTitleBarActionToken" />.
/// </summary>
[ControlDesignToken]
public sealed partial class WindowTitleBarToggleButtonToken : AbstractTitleBarActionToken
{
    /// <summary>
    /// Corner radius drawn on the title-bar toggle button surface.
    /// </summary>
    public partial CornerRadius CaptionButtonCornerRadius { get; set; }
}
