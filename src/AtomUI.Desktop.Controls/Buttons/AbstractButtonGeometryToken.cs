using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Design tokens of a button that are independent of its color variant: the content padding, the icon size
/// and the label metrics of each size, and the switches that select how disabled, pressed and link buttons
/// are painted.
/// </summary>
[ControlDesignToken]
public abstract partial class AbstractButtonGeometryToken : AbstractButtonVariantToken
{
    /// <summary>
    /// Outline color in the disabled state, used when disabled styling replaces the normal paint.
    /// </summary>
    public partial Color BorderColorDisabled { get; set; }
    /// <summary>
    /// Space inside a regular button between its frame and its label or icon.
    /// </summary>
    public partial Thickness ContentPadding { get; set; }
    /// <summary>
    /// Content padding for a large button.
    /// </summary>
    public partial Thickness ContentPaddingLG { get; set; }
    /// <summary>
    /// Content padding for a small button.
    /// </summary>
    public partial Thickness ContentPaddingSM { get; set; }
    /// <summary>
    /// Content padding for a circular button.
    /// </summary>
    public partial Thickness CirclePadding { get; set; }
    /// <summary>
    /// Icon size in a regular button with no text label.
    /// </summary>
    public partial double OnlyIconSize { get; set; }
    /// <summary>
    /// Icon size in a large button with no text label.
    /// </summary>
    public partial double OnlyIconSizeLG { get; set; }
    /// <summary>
    /// Icon size in a small button with no text label.
    /// </summary>
    public partial double OnlyIconSizeSM { get; set; }
    /// <summary>
    /// Space between the leading icon and the button label.
    /// </summary>
    public partial Thickness IconMargin { get; set; }
    /// <summary>
    /// Space between the label and the trailing icon.
    /// </summary>
    public partial Thickness IconEndMargin { get; set; }
    /// <summary>
    /// Font size of the label in a regular button.
    /// </summary>
    public partial double ContentFontSize { get; set; }
    /// <summary>
    /// Font size of the label in a large button.
    /// </summary>
    public partial double ContentFontSizeLG { get; set; }
    /// <summary>
    /// Font size of the label in a small button.
    /// </summary>
    public partial double ContentFontSizeSM { get; set; }
    /// <summary>
    /// Line height of the label in a regular button.
    /// </summary>
    [ReservedToken("A derivation input: the design language derives other tokens of this control from it, so overriding it moves them.")]
    public partial double ContentLineHeight { get; set; }
    /// <summary>
    /// Line height of the label in a large button.
    /// </summary>
    [ReservedToken("A derivation input: the design language derives other tokens of this control from it, so overriding it moves them.")]
    public partial double ContentLineHeightLG { get; set; }
    /// <summary>
    /// Line height of the label in a small button.
    /// </summary>
    [ReservedToken("A derivation input: the design language derives other tokens of this control from it, so overriding it moves them.")]
    public partial double ContentLineHeightSM { get; set; }
    /// <summary>
    /// Inner padding of a regular button with no text label.
    /// </summary>
    public partial Thickness IconOnyPadding { get; set; }
    /// <summary>
    /// Inner padding of a large button with no text label.
    /// </summary>
    public partial Thickness IconOnyPaddingLG { get; set; }
    /// <summary>
    /// Inner padding of a small button with no text label.
    /// </summary>
    public partial Thickness IconOnyPaddingSM { get; set; }

    /// <summary>
    /// Whether the button uses the recipe color model: a soft danger fill, filled text from
    /// <c>ControlItemColorActive</c>, no status shadows and the default colors on an open flyout.
    /// </summary>
    public partial bool RecipeVariants { get; set; }

    /// <summary>
    /// Whether only the outlined variant keeps the default shadow.
    /// </summary>
    public partial bool OutlineOnlyDefaultShadow { get; set; }

    /// <summary>
    /// Whether a disabled button keeps its colors at <see cref="DisabledOpacity" /> instead of the gray disabled
    /// paint.
    /// </summary>
    public partial bool DisabledByOpacity { get; set; }

    /// <summary>
    /// The opacity of a disabled button when <see cref="DisabledByOpacity" /> is set.
    /// </summary>
    public partial double DisabledOpacity { get; set; }

    /// <summary>
    /// Whether pressing moves the button down 1px instead of switching to the pressed colors.
    /// </summary>
    public partial bool PressMoves { get; set; }

    /// <summary>
    /// Whether a link button underlines its label on hover.
    /// </summary>
    public partial bool UnderlineLinkOnHover { get; set; }

    /// <summary>
    /// Whether a link button underlines its label at rest.
    /// </summary>
    public partial bool UnderlineLinkAtRest { get; set; }
}
