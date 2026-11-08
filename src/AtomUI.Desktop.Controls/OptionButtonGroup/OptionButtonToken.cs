using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Theme.Algorithms;
using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
internal sealed class OptionButtonToken : AbstractControlDesignToken
{
    private const string GroupDrawnFrame =
        "OptionButtonGroup draws its separators and the selection border behind the options, so an opaque option " +
        "background would cover them; options stay transparent until the group draws the frame above its options.";

    /// <summary>
    /// 单选框按钮背景色（antd Radio buttonBg）
    /// </summary>
    [ReservedToken("antd Radio buttonBg. " + GroupDrawnFrame)]
    public Color ButtonBackground { get; set; }

    /// <summary>
    /// 单选框按钮选中背景色（antd Radio buttonCheckedBg）
    /// </summary>
    [ReservedToken("antd Radio buttonCheckedBg. " + GroupDrawnFrame)]
    public Color ButtonCheckedBackground { get; set; }

    /// <summary>
    /// 单选框按钮文本颜色
    /// </summary>
    public Color ButtonColor { get; set; }

    /// <summary>
    /// 单选框按钮水平内间距（antd Radio buttonPaddingInline），作为默认与大号 <see cref="ContentPadding"/> 的左右分量；
    /// 只使用 Left / Right。
    /// </summary>
    public Thickness ButtonPadding { get; set; }

    /// <summary>
    /// 单选框按钮选中并禁用时的背景色
    /// </summary>
    public Color ButtonCheckedBgDisabled { get; set; }

    /// <summary>
    /// 单选框按钮选中并禁用时的文本颜色
    /// </summary>
    public Color ButtonCheckedColorDisabled { get; set; }

    /// <summary>
    /// 单选框实色按钮选中时的文本颜色
    /// </summary>
    public Color ButtonSolidCheckedColor { get; set; }

    /// <summary>
    /// 单选框实色按钮选中时的背景色
    /// </summary>
    public Color ButtonSolidCheckedBackground { get; set; }

    /// <summary>
    /// 单选框实色按钮选中时的悬浮态背景色
    /// </summary>
    public Color ButtonSolidCheckedHoverBackground { get; set; }

    /// <summary>
    /// 单选框实色按钮选中时的激活态背景色
    /// </summary>
    public Color ButtonSolidCheckedActiveBackground { get; set; }

    /// <summary>
    /// 按钮内容字体大小
    /// </summary>
    public double ContentFontSize { get; set; }

    /// <summary>
    /// 大号按钮内容字体大小
    /// </summary>
    public double ContentFontSizeLG { get; set; }

    /// <summary>
    /// 小号按钮内容字体大小
    /// </summary>
    public double ContentFontSizeSM { get; set; }

    /// <summary>
    /// 按钮内容字体行高
    /// </summary>
    public double ContentLineHeight { get; set; }

    /// <summary>
    /// 大号按钮内容字体行高
    /// </summary>
    public double ContentLineHeightLG { get; set; }

    /// <summary>
    /// 小号按钮内容字体行高
    /// </summary>
    public double ContentLineHeightSM { get; set; }

    /// <summary>
    /// 按钮内间距
    /// </summary>
    public Thickness ContentPadding { get; set; }

    /// <summary>
    /// 大号按钮内间距
    /// </summary>
    public Thickness ContentPaddingLG { get; set; }

    /// <summary>
    /// 小号按钮内间距
    /// </summary>
    public Thickness ContentPaddingSM { get; set; }

    public OptionButtonToken()

    {
    }

    public override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        ButtonSolidCheckedColor            = EffectiveGlobalToken.ColorTextLightSolid;
        ButtonSolidCheckedBackground       = EffectiveGlobalToken.ColorPrimary;
        ButtonSolidCheckedHoverBackground  = EffectiveGlobalToken.ColorPrimaryHover;
        ButtonSolidCheckedActiveBackground = EffectiveGlobalToken.ColorPrimaryActive;
        ButtonBackground                   = EffectiveGlobalToken.ColorBgContainer;
        ButtonCheckedBackground            = EffectiveGlobalToken.ColorBgContainer;
        ButtonColor                        = EffectiveGlobalToken.ColorText;
        ButtonCheckedBgDisabled            = EffectiveGlobalToken.ControlItemBgActiveDisabled;
        ButtonCheckedColorDisabled         = EffectiveGlobalToken.ColorTextDisabled;

        var fontSize   = EffectiveGlobalToken.FontSize;
        var fontSizeLG = EffectiveGlobalToken.FontSizeLG;

        if (!IsExplicit(OptionButtonTokenKind.ContentFontSize))
        {
            ContentFontSize = fontSize;
        }

        if (!IsExplicit(OptionButtonTokenKind.ContentFontSizeSM))
        {
            ContentFontSizeSM = fontSize;
        }

        if (!IsExplicit(OptionButtonTokenKind.ContentFontSizeLG))
        {
            ContentFontSizeLG = fontSizeLG;
        }

        // Line heights are ratios of the matching font size (antd lineHeight), not pixel heights.
        if (!IsExplicit(OptionButtonTokenKind.ContentLineHeight))
        {
            ContentLineHeight = CalculatorUtils.CalculateLineHeight(ContentFontSize);
        }

        if (!IsExplicit(OptionButtonTokenKind.ContentLineHeightSM))
        {
            ContentLineHeightSM = CalculatorUtils.CalculateLineHeight(ContentFontSizeSM);
        }

        if (!IsExplicit(OptionButtonTokenKind.ContentLineHeightLG))
        {
            ContentLineHeightLG = CalculatorUtils.CalculateLineHeight(ContentFontSizeLG);
        }

        var controlHeight   = EffectiveGlobalToken.ControlHeight;
        var controlHeightSM = EffectiveGlobalToken.ControlHeightSM;
        var controlHeightLG = EffectiveGlobalToken.ControlHeightLG;
        var lineWidth       = EffectiveGlobalToken.LineWidth;

        if (!IsExplicit(OptionButtonTokenKind.ButtonPadding))
        {
            ButtonPadding = new Thickness(EffectiveGlobalToken.PaddingContentHorizontal - lineWidth, 0);
        }

        // Kept as direct reads in the assignments: the Own Token reference analysis follows derivations, not locals.
        ContentPadding = new Thickness(
            ButtonPadding.Left,
            Math.Max((controlHeight - ContentFontSize * ContentLineHeight) / 2 - lineWidth, 0),
            ButtonPadding.Right,
            Math.Max((controlHeight - ContentFontSize * ContentLineHeight) / 2 - lineWidth, 0));
        ContentPaddingLG = new Thickness(
            ButtonPadding.Left,
            Math.Max((controlHeightLG - ContentFontSizeLG * ContentLineHeightLG) / 2 - lineWidth, 0),
            ButtonPadding.Right,
            Math.Max((controlHeightLG - ContentFontSizeLG * ContentLineHeightLG) / 2 - lineWidth, 0));
        ContentPaddingSM = new Thickness(8 - lineWidth,
            Math.Max((controlHeightSM - ContentFontSizeSM * ContentLineHeightSM) / 2 - lineWidth, 0));
    }
    
}
