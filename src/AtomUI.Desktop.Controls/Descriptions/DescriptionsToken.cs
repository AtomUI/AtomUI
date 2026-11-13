using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
public sealed partial class DescriptionsToken : AbstractControlDesignToken
{
    
    /// <summary>
    /// 标签背景色
    /// Background color of label
    /// </summary>
    public partial Color LabelBg { get; set; }

    /// <summary>
    /// 标签文字颜色
    /// Text color of label
    /// </summary>
    public partial Color LabelColor { get; set; }

    /// <summary>
    /// 标题文字颜色
    /// Text color of title
    /// </summary>
    public partial Color TitleColor { get; set; }

    /// <summary>
    /// 标题下间距
    /// Bottom margin of Header
    /// </summary>
    public partial Thickness HeaderMargin { get; set; }

    /// <summary>
    /// 子项下间距，大号
    /// padding of item
    /// </summary>
    public partial Thickness ItemPaddingLG { get; set; }
    
    /// <summary>
    /// 子项下间距，默认
    /// padding of item
    /// </summary>
    public partial Thickness ItemPadding { get; set; }
    
    /// <summary>
    /// 子项下间距，小号
    /// padding of item
    /// </summary>
    public partial Thickness ItemPaddingSM { get; set; }

    /// <summary>
    /// 冒号间距
    /// margin of colon
    /// </summary>
    public partial Thickness ColonMargin { get; set; }

    /// <summary>
    /// 内容区域文字颜色
    /// Text color of content
    /// </summary>
    public partial Color ContentColor { get; set; }

    /// <summary>
    /// 额外区域文字颜色
    /// Text color of extra area
    /// </summary>
    public partial Color ExtraColor { get; set; }

    public DescriptionsToken()

    {
    }

    protected override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        LabelBg      = EffectiveGlobalToken.ColorFillAlter;
        LabelColor   = EffectiveGlobalToken.ColorTextTertiary;
        TitleColor   = EffectiveGlobalToken.ColorText;
        HeaderMargin = new Thickness(0, 0, 0, EffectiveGlobalToken.FontSizeSM * EffectiveGlobalToken.RelativeLineHeightSM);
        ItemPaddingLG  = new Thickness(EffectiveGlobalToken.UniformlyPaddingLG, EffectiveGlobalToken.UniformlyPadding);
        ItemPadding  = new Thickness(EffectiveGlobalToken.UniformlyPaddingLG, EffectiveGlobalToken.UniformlyPaddingSM);
        ItemPaddingSM  = new Thickness(EffectiveGlobalToken.UniformlyPadding, EffectiveGlobalToken.UniformlyPaddingXS);
        ColonMargin  = new Thickness(EffectiveGlobalToken.UniformlyMarginXXS / 2, 0, EffectiveGlobalToken.UniformlyMarginXS, 0);
        ContentColor = EffectiveGlobalToken.ColorText;
        ExtraColor   = EffectiveGlobalToken.ColorText;
    }
    
}
