using AtomUI.Theme.DesignTokens;
using Avalonia;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
public sealed partial class WindowTitleBarButtonToken : AbstractTitleBarActionToken
{
    /// <summary>Corner radius of the title bar caption button.</summary>
    public partial CornerRadius CaptionButtonCornerRadius { get; set; }
}
