using AtomUI.Theme.DesignTokens;
using Avalonia;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
public sealed partial class TextBoxToken : AbstractControlDesignToken
{

    /// <summary>Padding inside the text box at the default size.</summary>
    public partial Thickness ContentPadding { get; set; }

    /// <summary>Padding inside a small text box.</summary>
    public partial Thickness ContentPaddingSM { get; set; }

    /// <summary>Padding inside a large text box.</summary>
    public partial Thickness ContentPaddingLG { get; set; }

}
