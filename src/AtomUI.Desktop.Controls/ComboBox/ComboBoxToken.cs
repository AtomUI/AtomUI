using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
public sealed partial class ComboBoxToken : AbstractControlDesignToken
{

    public ComboBoxToken()

    {
    }

    /// <summary>
    /// 菜单内容边距
    /// </summary>
    public partial Thickness PopupContentPadding { get; set; }

    /// <summary>
    /// 列表项文字颜色
    /// </summary>
    public partial Color ItemColor { get; set; }

    /// <summary>
    /// 列表项文字悬浮颜色
    /// </summary>
    public partial Color ItemHoverColor { get; set; }

    /// <summary>
    /// 列表项文字选中颜色
    /// </summary>
    public partial Color ItemSelectedColor { get; set; }

    /// <summary>
    /// 列表项文字禁用颜色
    /// </summary>
    public partial Color ItemDisabledColor { get; set; }

    /// <summary>
    /// 列表项背景色
    /// </summary>
    public partial Color ItemBgColor { get; set; }

    /// <summary>
    /// 列表项悬浮态背景色
    /// </summary>
    public partial Color ItemHoverBgColor { get; set; }

    /// <summary>
    /// 列表项选中背景色
    /// </summary>
    public partial Color ItemSelectedBgColor { get; set; }

    /// <summary>
    /// 列表项内间距
    /// </summary>
    public partial Thickness ItemPadding { get; set; }

    /// <summary>
    /// 列表项外边距
    /// </summary>
    public partial Thickness ItemMargin { get; set; }

    /// <summary>
    /// 下拉手柄悬浮颜色。
    /// </summary>
    public partial Color HandleHoverColor { get; set; }

    protected override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        PopupContentPadding  = new Thickness(EffectiveGlobalToken.UniformlyPaddingXXS, EffectiveGlobalToken.BorderRadiusLG.TopLeft / 2);
        
        var colorTextDisabled  = EffectiveGlobalToken.ColorTextDisabled;
        var colorTextSecondary = EffectiveGlobalToken.ColorTextSecondary;
        var colorBgContainer   = EffectiveGlobalToken.ColorBgElevated;
        var colorBgTextHover   = EffectiveGlobalToken.ColorBgTextHover;

        ItemColor         = colorTextSecondary;
        ItemHoverColor    = colorTextSecondary;
        ItemSelectedColor = EffectiveGlobalToken.ColorText;

        ItemBgColor         = colorBgContainer;
        ItemHoverBgColor    = colorBgTextHover;
        ItemSelectedBgColor = EffectiveGlobalToken.ControlItemBgActive;
        ItemDisabledColor = colorTextDisabled;
        HandleHoverColor = EffectiveGlobalToken.ColorPrimary;

        ItemPadding = new Thickness(EffectiveGlobalToken.UniformlyPaddingSM, 0);
        ItemMargin  = new Thickness(0, 0.5);
    }
    
}
