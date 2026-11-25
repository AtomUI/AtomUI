using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
public sealed partial class CompactSpaceAddOnToken : AbstractControlDesignToken
{
    /// <summary>
    /// Add On 背景色
    /// </summary>
    public partial Color AddonBg { get; set; }

    /// <summary>Padding inside the add-on at the default size.</summary>
    public partial Thickness AddOnPadding { get; set; }

    /// <summary>Padding inside a small add-on.</summary>
    public partial Thickness AddOnPaddingSM { get; set; }

    /// <summary>Padding inside a large add-on.</summary>
    public partial Thickness AddOnPaddingLG { get; set; }
}
