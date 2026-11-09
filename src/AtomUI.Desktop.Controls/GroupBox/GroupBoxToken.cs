using AtomUI.Theme.DesignTokens;
using Avalonia;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
internal sealed class GroupBoxToken : AbstractControlDesignToken
{
    
    public GroupBoxToken()

    {
    }

    /// <summary>
    /// 内容区域内间距
    /// </summary>
    public Thickness ContentPadding { get; set; }

    /// <summary>
    /// Header 容器外间距
    /// </summary>
    public Thickness HeaderContainerMargin { get; set; }

    /// <summary>
    /// 头部图标的外间距
    /// </summary>
    public Thickness HeaderIconMargin { get; set; }

    /// <summary>
    /// Header 内容区域内间距
    /// </summary>
    public Thickness HeaderContentPadding { get; set; }

    public override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        ContentPadding           = EffectiveGlobalToken.PaddingXS;
        HeaderContainerMargin = new Thickness(EffectiveGlobalToken.UniformlyMargin, EffectiveGlobalToken.UniformlyMarginXS,
            EffectiveGlobalToken.UniformlyMargin, 0);
        HeaderContentPadding = new Thickness(EffectiveGlobalToken.UniformlyPaddingXXS, 0);
        HeaderIconMargin     = new Thickness(0, 0, EffectiveGlobalToken.UniformlyMarginXXS, 0);
    }
    
}
