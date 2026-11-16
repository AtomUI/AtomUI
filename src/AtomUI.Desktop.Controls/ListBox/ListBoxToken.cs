using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
public sealed partial class ListBoxToken : AbstractControlDesignToken
{
    
    public ListBoxToken()

    {
    }
    
    /// <summary>
    /// List 内边距
    /// </summary>
    public partial Thickness ContentPadding { get; set; }

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
    /// 列表项选中标记的外间距
    /// </summary>
    public partial Thickness SelectedIndicatorMargin { get; set; }
    
    /// <summary>
    /// 过滤高亮颜色
    /// </summary>
    public partial Color FilterHighlightColor { get; set; }

    /// <summary>
    /// 列表项小号内间距
    /// </summary>
    public partial Thickness ItemPaddingSM { get; set; }

    /// <summary>
    /// 列表项内间距
    /// </summary>
    public partial Thickness ItemPadding { get; set; }

    /// <summary>
    /// 列表项大号内间距
    /// </summary>
    public partial Thickness ItemPaddingLG { get; set; }

    /// <summary>
    /// 列表项外边距
    /// </summary>
    public partial Thickness ItemMargin { get; set; }
    

    protected override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        var colorTextDisabled  = EffectiveGlobalToken.ColorTextDisabled;
        var colorTextSecondary = EffectiveGlobalToken.ColorTextSecondary;
        var colorBgTextHover   = EffectiveGlobalToken.ColorBgTextHover;

        ItemColor         = colorTextSecondary;
        ItemHoverColor    = colorTextSecondary;
        ItemSelectedColor = EffectiveGlobalToken.ControlItemColorActive;

        ItemBgColor         = EffectiveGlobalToken.ColorTransparent;
        ItemHoverBgColor    = colorBgTextHover;
        ItemSelectedBgColor = EffectiveGlobalToken.ControlItemBgActive;

        ItemDisabledColor = colorTextDisabled;

        ItemPaddingLG = new Thickness(EffectiveGlobalToken.UniformlyPadding, 0);
        ItemPaddingSM = new Thickness(EffectiveGlobalToken.UniformlyPaddingXS, 0);
        ItemPadding   = new Thickness(EffectiveGlobalToken.UniformlyPaddingSM, 0);

        ContentPadding   = new Thickness(0);
        ItemMargin       = new Thickness(0);

        FilterHighlightColor = EffectiveGlobalToken.ColorError;

        SelectedIndicatorMargin = new Thickness(EffectiveGlobalToken.UniformlyMarginXXS, 0, 0, 0);
    }
    
}
