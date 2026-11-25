using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
public sealed partial class OptionButtonToken : AbstractControlDesignToken
{
    private const string GroupDrawnFrame =
        "OptionButtonGroup draws its separators and the selection border behind the options, so an opaque option " +
        "background would cover them; options stay transparent until the group draws the frame above its options.";

    /// <summary>
    /// 单选框按钮背景色（antd Radio buttonBg）
    /// </summary>
    [ReservedToken("antd Radio buttonBg. " + GroupDrawnFrame)]
    public partial Color ButtonBackground { get; set; }

    /// <summary>
    /// 单选框按钮选中背景色（antd Radio buttonCheckedBg）
    /// </summary>
    [ReservedToken("antd Radio buttonCheckedBg. " + GroupDrawnFrame)]
    public partial Color ButtonCheckedBackground { get; set; }

    /// <summary>Label foreground of an option button at rest.</summary>
    public partial Color ButtonColor { get; set; }

    /// <summary>
    /// 单选框按钮水平内间距（antd Radio buttonPaddingInline），作为默认与大号 <see cref="ContentPadding"/> 的左右分量；
    /// 只使用 Left / Right。
    /// </summary>
    [ReservedToken("A derivation input: the design language derives other tokens of this control from it, so overriding it moves them.")]
    public partial Thickness ButtonPadding { get; set; }

    /// <summary>Fill of a checked disabled option button. Pair with <see cref="ButtonCheckedColorDisabled"/>.</summary>
    public partial Color ButtonCheckedBgDisabled { get; set; }

    /// <summary>Label foreground on <see cref="ButtonCheckedBgDisabled"/>.</summary>
    public partial Color ButtonCheckedColorDisabled { get; set; }

    /// <summary>
    /// Fill of the checked option in the outline style (transparent: only the border and text show the selection).
    /// </summary>
    public partial Color ButtonOutlineCheckedBackground { get; set; }

    /// <summary>
    /// Text colour of the checked option in the outline style.
    /// </summary>
    public partial Color ButtonOutlineCheckedColor { get; set; }

    /// <summary>
    /// Text colour of the checked option in the outline style while hovered.
    /// </summary>
    public partial Color ButtonOutlineCheckedHoverColor { get; set; }

    /// <summary>
    /// Text colour of the checked option in the outline style while pressed.
    /// </summary>
    public partial Color ButtonOutlineCheckedActiveColor { get; set; }

    /// <summary>
    /// Label foreground across the checked solid button states, paired with
    /// <see cref="ButtonSolidCheckedBackground"/> .
    /// </summary>
    public partial Color ButtonSolidCheckedColor { get; set; }

    /// <summary>Resting fill of the checked solid variant, paired with <see cref="ButtonSolidCheckedColor"/>.</summary>
    public partial Color ButtonSolidCheckedBackground { get; set; }

    /// <summary>Fill of a checked solid button under the pointer.</summary>
    public partial Color ButtonSolidCheckedHoverBackground { get; set; }

    /// <summary>Fill of a checked solid button while pressed.</summary>
    public partial Color ButtonSolidCheckedActiveBackground { get; set; }

    /// <summary>Label font size for the regular button size.</summary>
    public partial double ContentFontSize { get; set; }

    /// <summary>Label font size for the large button size.</summary>
    public partial double ContentFontSizeLG { get; set; }

    /// <summary>Label font size for the small button size.</summary>
    public partial double ContentFontSizeSM { get; set; }

    /// <summary>Label line height for the regular button size.</summary>
    [ReservedToken("A derivation input: the design language derives other tokens of this control from it, so overriding it moves them.")]
    public partial double ContentLineHeight { get; set; }

    /// <summary>Label line height for the large button size.</summary>
    [ReservedToken("A derivation input: the design language derives other tokens of this control from it, so overriding it moves them.")]
    public partial double ContentLineHeightLG { get; set; }

    /// <summary>Label line height for the small button size.</summary>
    [ReservedToken("A derivation input: the design language derives other tokens of this control from it, so overriding it moves them.")]
    public partial double ContentLineHeightSM { get; set; }

    /// <summary>Padding around the label in a regular option button.</summary>
    public partial Thickness ContentPadding { get; set; }

    /// <summary>Padding around the label in a large option button.</summary>
    public partial Thickness ContentPaddingLG { get; set; }

    /// <summary>Padding around the label in a small option button.</summary>
    public partial Thickness ContentPaddingSM { get; set; }
    
}
