using AtomUI.Theme.DesignTokens;
using Avalonia;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
public sealed partial class HyperLinkButtonToken : AbstractControlDesignToken
{
    /// <summary>Content padding for the default button size.</summary>
    public partial Thickness ContentPadding { get; set; }
    /// <summary>Content padding for large buttons.</summary>
    public partial Thickness ContentPaddingLG { get; set; }
    /// <summary>Content padding for small buttons.</summary>
    public partial Thickness ContentPaddingSM { get; set; }
    /// <summary>Width and height of the icon in an icon-only button of the default size.</summary>
    public partial double OnlyIconSize { get; set; }
    /// <summary>Width and height of the icon in a large icon-only button.</summary>
    public partial double OnlyIconSizeLG { get; set; }
    /// <summary>Width and height of the icon in a small icon-only button.</summary>
    public partial double OnlyIconSizeSM { get; set; }
    /// <summary>Margin around the icon when the button also displays content.</summary>
    public partial Thickness IconMargin { get; set; }
    /// <summary>Font size of the button text at the default size.</summary>
    public partial double ContentFontSize { get; set; }
    /// <summary>Font size of the text in a large button.</summary>
    public partial double ContentFontSizeLG { get; set; }
    /// <summary>Font size of the text in a small button.</summary>
    public partial double ContentFontSizeSM { get; set; }
    /// <summary>Text line height used to derive geometry for the default button size.</summary>
    [ReservedToken("A derivation input: the design language derives other tokens of this control from it, so overriding it moves them.")]
    public partial double ContentLineHeight { get; set; }
    /// <summary>Text line height used to derive geometry for large buttons.</summary>
    [ReservedToken("A derivation input: the design language derives other tokens of this control from it, so overriding it moves them.")]
    public partial double ContentLineHeightLG { get; set; }
    /// <summary>Text line height used to derive geometry for small buttons.</summary>
    [ReservedToken("A derivation input: the design language derives other tokens of this control from it, so overriding it moves them.")]
    public partial double ContentLineHeightSM { get; set; }
    /// <summary>Padding for icon-only buttons of the default size.</summary>
    public partial Thickness IconOnyPadding { get; set; }
    /// <summary>Padding for large icon-only buttons.</summary>
    public partial Thickness IconOnyPaddingLG { get; set; }
    /// <summary>Padding for small icon-only buttons.</summary>
    public partial Thickness IconOnyPaddingSM { get; set; }
}
