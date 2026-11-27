using AtomUI.Theme.DesignTokens;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Own Tokens of the <see cref="QRCode" /> control: the color of the QR code and of the text on its status cover,
/// and the background of that cover.
/// </summary>
[ControlDesignToken]
public sealed partial class QRCodeToken : AbstractControlDesignToken
{
    
    /// <summary>
    /// Color of the QR code modules. The theme also assigns it to the Foreground of the control, which colors the
    /// text on the status cover.
    /// </summary>
    public partial Color QRCodeTextColor { get; set; }

    /// <summary>
    /// Background of the cover that lies over the QR image while the status is not Active.
    /// </summary>
    public partial Color QRCodeMaskBackgroundColor { get; set; }
    
}
