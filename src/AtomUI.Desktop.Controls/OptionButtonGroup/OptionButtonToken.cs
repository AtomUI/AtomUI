using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Design tokens of the desktop option button: the label and fill colors per state and style, the label font sizes
/// and line heights per size level, and the paddings.
/// </summary>
[ControlDesignToken]
public sealed partial class OptionButtonToken : AbstractControlDesignToken
{
    private const string GroupDrawnFrame =
        "OptionButtonGroup draws its separators and the selection border behind the options, so an opaque option " +
        "background would cover them; options stay transparent until the group draws the frame above its options.";

    /// <summary>
    /// Background of an option button, mirroring antd Radio buttonBg. Reserved: nothing consumes it yet, because an
    /// opaque option background would cover the separators and the selection border that the group draws behind the
    /// options.
    /// </summary>
    [ReservedToken("antd Radio buttonBg. " + GroupDrawnFrame)]
    public partial Color ButtonBackground { get; set; }

    /// <summary>
    /// Background of a checked option button, mirroring antd Radio buttonCheckedBg. Reserved: nothing consumes it
    /// yet, because an opaque option background would cover the separators and the selection border that the group
    /// draws behind the options.
    /// </summary>
    [ReservedToken("antd Radio buttonCheckedBg. " + GroupDrawnFrame)]
    public partial Color ButtonCheckedBackground { get; set; }

    /// <summary>
    /// Label foreground of an unchecked option button in the outline style at rest.
    /// </summary>
    public partial Color ButtonColor { get; set; }

    /// <summary>
    /// Horizontal padding of an option button, mirroring antd Radio buttonPaddingInline. A derivation input: only
    /// Left and Right are read, as the left and right components of <see cref="ContentPadding" /> and
    /// <see cref="ContentPaddingLG" />; overriding it moves them.
    /// </summary>
    [ReservedToken("A derivation input: the design language derives other tokens of this control from it, so overriding it moves them.")]
    public partial Thickness ButtonPadding { get; set; }

    /// <summary>
    /// Fill of a checked disabled option button. Pair with <see cref="ButtonCheckedColorDisabled" />.
    /// </summary>
    public partial Color ButtonCheckedBgDisabled { get; set; }

    /// <summary>
    /// Label foreground on <see cref="ButtonCheckedBgDisabled" />.
    /// </summary>
    public partial Color ButtonCheckedColorDisabled { get; set; }

    /// <summary>
    /// Fill of the checked option in the outline style. AntDesign and shadcn set it transparent, so only the border
    /// and text show the selection; PrimeNG Nora fills the checked cell with <c>ControlItemBgActive</c>.
    /// </summary>
    public partial Color ButtonOutlineCheckedBackground { get; set; }

    /// <summary>
    /// Text color of the checked option in the outline style.
    /// </summary>
    public partial Color ButtonOutlineCheckedColor { get; set; }

    /// <summary>
    /// Text color of the checked option in the outline style while hovered.
    /// </summary>
    public partial Color ButtonOutlineCheckedHoverColor { get; set; }

    /// <summary>
    /// Text color of the checked option in the outline style while pressed.
    /// </summary>
    public partial Color ButtonOutlineCheckedActiveColor { get; set; }

    /// <summary>
    /// Label foreground across the checked solid button states, paired with
    /// <see cref="ButtonSolidCheckedBackground" />.
    /// </summary>
    public partial Color ButtonSolidCheckedColor { get; set; }

    /// <summary>
    /// Resting fill of the checked solid variant, paired with <see cref="ButtonSolidCheckedColor" />.
    /// </summary>
    public partial Color ButtonSolidCheckedBackground { get; set; }

    /// <summary>
    /// Fill of a checked solid button under the pointer.
    /// </summary>
    public partial Color ButtonSolidCheckedHoverBackground { get; set; }

    /// <summary>
    /// Fill of a checked solid button while pressed.
    /// </summary>
    public partial Color ButtonSolidCheckedActiveBackground { get; set; }

    /// <summary>
    /// Label font size for the regular button size.
    /// </summary>
    public partial double ContentFontSize { get; set; }

    /// <summary>
    /// Label font size for the large button size.
    /// </summary>
    public partial double ContentFontSizeLG { get; set; }

    /// <summary>
    /// Label font size for the small button size.
    /// </summary>
    public partial double ContentFontSizeSM { get; set; }

    /// <summary>
    /// Label line height for the regular button size. A derivation input: the theme does not read it directly; the
    /// design language derives other tokens of this control from it, so overriding it moves them.
    /// </summary>
    [ReservedToken("A derivation input: the design language derives other tokens of this control from it, so overriding it moves them.")]
    public partial double ContentLineHeight { get; set; }

    /// <summary>
    /// Label line height for the large button size. A derivation input: the theme does not read it directly; the
    /// design language derives other tokens of this control from it, so overriding it moves them.
    /// </summary>
    [ReservedToken("A derivation input: the design language derives other tokens of this control from it, so overriding it moves them.")]
    public partial double ContentLineHeightLG { get; set; }

    /// <summary>
    /// Label line height for the small button size. A derivation input: the theme does not read it directly; the
    /// design language derives other tokens of this control from it, so overriding it moves them.
    /// </summary>
    [ReservedToken("A derivation input: the design language derives other tokens of this control from it, so overriding it moves them.")]
    public partial double ContentLineHeightSM { get; set; }

    /// <summary>
    /// Padding around the label in a regular option button.
    /// </summary>
    public partial Thickness ContentPadding { get; set; }

    /// <summary>
    /// Padding around the label in a large option button.
    /// </summary>
    public partial Thickness ContentPaddingLG { get; set; }

    /// <summary>
    /// Padding around the label in a small option button.
    /// </summary>
    public partial Thickness ContentPaddingSM { get; set; }
    
}
