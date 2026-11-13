using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Theme.DesignTokens;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
public sealed partial class LineEditToken : AbstractControlDesignToken
{
    /// <summary>
    /// 字体大小
    /// </summary>
    public partial double InputFontSize { get; set; }

    /// <summary>
    /// 大号字体大小
    /// </summary>
    public partial double InputFontSizeLG { get; set; }

    /// <summary>
    /// 小号字体大小，默认等于 <see cref="InputFontSize"/>（与 antd 一致）
    /// </summary>
    public partial double InputFontSizeSM { get; set; }
    
    protected override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        // antd Input: mergedFontSizeSM = inputFontSizeSM || mergedFontSize, mergedFontSizeLG = inputFontSizeLG || fontSizeLG.
        if (!IsExplicit(LineEditTokenKind.InputFontSize))
        {
            InputFontSize = EffectiveGlobalToken.FontSize;
        }
        InputFontSizeLG = EffectiveGlobalToken.FontSizeLG;
        InputFontSizeSM = InputFontSize;
    }

}
