using AtomUI.Theme.DesignTokens;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
public sealed partial class SearchEditToken : AbstractControlDesignToken
{
    /// <summary>
    /// 小号输入框字体大小，默认等于 <see cref="DesignToken.FontSize"/>（与 antd 一致）
    /// Font size of the small input, the middle font size by default (as in antd)
    /// </summary>
    public partial double InputFontSizeSM { get; set; }

    protected override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        InputFontSizeSM = EffectiveGlobalToken.FontSize;
    }
}
