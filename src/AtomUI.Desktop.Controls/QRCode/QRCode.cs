using AtomUI.Controls.Commons;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// A QR code image. The class adds nothing to its base class; it exists so that the desktop library can attach
/// its own theme, whose template draws the image, the center icon and the status cover.
/// </summary>
public partial class QRCode : AbstractQRCode
{
    public QRCode()
    {
    }
}
