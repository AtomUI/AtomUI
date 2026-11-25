using AtomUI.Theme.DesignTokens;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
public sealed partial class ImagePreviewerToken : AbstractImagePreviewerToken
{
    /// <summary>Reserved size for the image preview navigation buttons.</summary>
    [ReservedToken("antd Image imagePreviewSwitchSize is declared but its styles do not consume it; switch buttons size from operation size and padding.")]
    public partial double ImagePreviewSwitchSize { get; set; }
}
