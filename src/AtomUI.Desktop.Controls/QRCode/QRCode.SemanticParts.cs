using AtomUI.Theme;
using Avalonia.Controls;

namespace AtomUI.Desktop.Controls;

// The cover that lies over the QR image while the status is not Active. It holds the loading spinner or the text
// of the expired and scanned states. QRCodeCoverStyle applies to the cover.
[SemanticPart(
    "cover",
    SelectorClass = "semantic-cover",
    ContractType = typeof(Border),
    Since = "6.2.0")]
public partial class QRCode
{
}
