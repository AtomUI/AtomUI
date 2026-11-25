using AtomUI.Theme.DesignTokens;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
public sealed partial class LineEditToken : AbstractControlDesignToken
{
    /// <summary>Font size of text entered in a regular line editor.</summary>
    public partial double InputFontSize { get; set; }

    /// <summary>Font size of text entered in a large line editor.</summary>
    public partial double InputFontSizeLG { get; set; }

    /// <summary>Font size of text entered in a small line editor.</summary>
    public partial double InputFontSizeSM { get; set; }

    /// <summary>Whether the input frame keeps a transparent border at rest and on hover.</summary>
    public partial bool TransparentOutlineBorder { get; set; }

    /// <summary>Whether the input frame draws only its bottom border.</summary>
    public partial bool BottomOutlineBorder { get; set; }

}
