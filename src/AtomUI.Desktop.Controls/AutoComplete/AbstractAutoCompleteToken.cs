using AtomUI.Theme.DesignTokens;
using Avalonia;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
public abstract partial class AbstractAutoCompleteToken : AbstractControlDesignToken
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

    /// <summary>
    /// 在不跟随 Anchor 宽度时候的 Popup 最大宽度
    /// </summary>
    public partial double MaxPopupWidth { get; set; }

protected override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        OptionHeight        = EffectiveGlobalToken.ControlHeight;
        PopupContentPadding = new Thickness(EffectiveGlobalToken.UniformlyPaddingXXS / 2);
        MinPopupWidth       = 120;
        MaxPopupWidth       = 200;
    }

}
