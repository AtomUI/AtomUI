using AtomUI.Theme.DesignTokens;
using Avalonia;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
public sealed partial class WindowTitleBarToggleButtonToken : AbstractTitleBarActionToken
{
    /// <summary>Corner radius drawn on the title-bar toggle button surface.</summary>
    public partial CornerRadius CaptionButtonCornerRadius { get; set; }
}
