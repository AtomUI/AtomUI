using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Theme.Algorithms;
using AtomUI.Theme.DesignTokens;
using AtomUI.Utils;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
internal sealed class OptionButtonToken : AbstractControlDesignToken
{
    
    /// <summary>
    /// 单选框按钮背景色
    /// </summary>
    public Color ButtonBackground { get; set; }

    /// <summary>
    /// 单选框按钮选中背景色
    /// </summary>
    public Color ButtonCheckedBackground { get; set; }

    /// <summary>
    /// 单选框按钮文本颜色
    /// </summary>
    public Color ButtonColor { get; set; }

    /// <summary>
    /// 单选框按钮内间距
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
    public double ContentFontSize { get; set; } = -1;

    /// <summary>
    /// 大号按钮内容字体大小
    /// </summary>
    public double ContentFontSizeLG { get; set; } = -1;

    /// <summary>
    /// 小号按钮内容字体大小
    /// </summary>
    public double ContentFontSizeSM { get; set; } = -1;

    /// <summary>
    /// 按钮内容字体行高
    /// </summary>
    public double ContentLineHeight { get; set; } = -1;

    /// <summary>
    /// 大号按钮内容字体行高
    /// </summary>
    public double ContentLineHeightLG { get; set; } = -1;

    /// <summary>
    /// 小号按钮内容字体行高
    /// </summary>
    public double ContentLineHeightSM { get; set; } = -1;

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
        ButtonPadding                      = new Thickness(EffectiveGlobalToken.UniformlyPadding, 0);

        var fontSize   = EffectiveGlobalToken.FontSize;
        var fontSizeLG = EffectiveGlobalToken.FontSizeLG;

        if (!IsExplicit(OptionButtonTokenKind.ContentFontSize))
        {
            ContentFontSize   = !MathUtils.AreClose(ContentFontSize, -1) ? ContentFontSize : fontSize;
        }

        if (!IsExplicit(OptionButtonTokenKind.ContentFontSizeSM))
        {
            ContentFontSizeSM = !MathUtils.AreClose(ContentFontSizeSM, -1) ? ContentFontSizeSM : fontSize;
        }

        if (!IsExplicit(OptionButtonTokenKind.ContentFontSizeLG))
        {
            ContentFontSizeLG = !MathUtils.AreClose(ContentFontSizeLG, -1) ? ContentFontSizeLG : fontSizeLG;
        }

        if (!IsExplicit(OptionButtonTokenKind.ContentLineHeight))
        {
            ContentLineHeight = !MathUtils.AreClose(ContentLineHeight, -1)
                ? ContentLineHeight
                : CalculatorUtils.CalculateLineHeight(ContentFontSize);
        }

        if (!IsExplicit(OptionButtonTokenKind.ContentLineHeightSM))
        {
            ContentLineHeightSM = !MathUtils.AreClose(ContentLineHeightSM, -1)
                ? ContentLineHeightSM
                : CalculatorUtils.CalculateLineHeight(ContentFontSizeSM);
        }

        ContentLineHeightLG = !MathUtils.AreClose(ContentLineHeightLG, -1)
            ? ContentLineHeightLG
            : CalculatorUtils.CalculateLineHeight(ContentFontSizeLG);

        var controlHeight   = EffectiveGlobalToken.ControlHeight;
        var controlHeightSM = EffectiveGlobalToken.ControlHeightSM;
        var controlHeightLG = EffectiveGlobalToken.ControlHeightLG;
        var lineWidth       = EffectiveGlobalToken.LineWidth;

        ContentPadding = new Thickness(EffectiveGlobalToken.PaddingContentHorizontal - lineWidth,
            Math.Max((controlHeight - ContentFontSize * ContentLineHeight) / 2 - lineWidth, 0));
        ContentPaddingLG = new Thickness(EffectiveGlobalToken.PaddingContentHorizontal - lineWidth,
            Math.Max((controlHeightSM - ContentFontSizeSM * ContentLineHeightSM) / 2 - lineWidth, 0));
        ContentPaddingSM = new Thickness(8 - EffectiveGlobalToken.LineWidth,
            Math.Max((controlHeightLG - controlHeightLG * controlHeightLG) / 2 - lineWidth, 0));
    }
    
}
