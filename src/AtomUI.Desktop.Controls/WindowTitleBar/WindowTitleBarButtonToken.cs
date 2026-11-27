using AtomUI.Theme.DesignTokens;
using Avalonia;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Design token of a title bar button. It adds the corner radius of the button to the colors and caption button
/// metrics of <see cref="AbstractTitleBarActionToken" />.
/// </summary>
[ControlDesignToken]
public sealed partial class WindowTitleBarButtonToken : AbstractTitleBarActionToken
{
    /// <summary>
    /// Corner radius of the title bar caption button.
    /// </summary>
    public partial CornerRadius CaptionButtonCornerRadius { get; set; }
}
