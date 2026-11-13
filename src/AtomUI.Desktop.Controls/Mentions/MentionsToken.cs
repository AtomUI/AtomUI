using AtomUI.Theme.DesignTokens;
using Avalonia;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
public sealed partial class MentionsToken : AbstractControlDesignToken
{
    
    /// <summary>
    /// 菜单内容边距
    /// </summary>
    public partial Thickness PopupContentPadding { get; set; }
    
    /// <summary>
    /// 选项高度
    /// Height of option
    /// </summary>
    public partial double OptionHeight { get; set; }
    
    /// <summary>
    /// 候选列表弹窗最小宽度
    /// </summary>
    public partial double MinPopupWidth { get; set; }
    
    public MentionsToken()

    {
    }

    protected override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        OptionHeight        = EffectiveGlobalToken.ControlHeight;
        PopupContentPadding = new Thickness(EffectiveGlobalToken.UniformlyPaddingXXS / 2);
        MinPopupWidth       = 120;
    }
    
}
