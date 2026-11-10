using AtomUI.Theme.DesignTokens;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
internal abstract class AbstractStatisticToken : AbstractStatisticValueToken
{

    /// <summary>
    /// 标题字体大小
    /// Title font size
    /// </summary>
    public double TitleFontSize { get; set; }

    /// <summary>
    /// 内容字体大小
    /// Content font size
    /// </summary>

public override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        TitleFontSize = EffectiveGlobalToken.FontSize;
    }

}
