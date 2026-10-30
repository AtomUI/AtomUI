using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
internal sealed class ListBoxToken : AbstractControlDesignToken
{
    
    public ListBoxToken()

    {
    }
    
    /// <summary>
    /// List 内边距
    /// </summary>
    public Thickness ContentPadding { get; set; }

    /// <summary>
    /// 列表项文字颜色
    /// </summary>
    public Color ItemColor { get; set; }

    /// <summary>
    /// 列表项文字悬浮颜色
    /// </summary>
    public Color ItemHoverColor { get; set; }

    /// <summary>
    /// 列表项文字选中颜色
    /// </summary>
    public Color ItemSelectedColor { get; set; }

    /// <summary>
    /// 列表项文字禁用颜色
    /// </summary>
    public Color ItemDisabledColor { get; set; }

    /// <summary>
    /// 列表项背景色
    /// </summary>
    public Color ItemBgColor { get; set; }

    /// <summary>
    /// 列表项悬浮态背景色
    /// </summary>
    public Color ItemHoverBgColor { get; set; }

    /// <summary>
    /// 列表项选中背景色
    /// </summary>
    public Color ItemSelectedBgColor { get; set; }
    
    /// <summary>
    /// 列表项选中标记的外间距
    /// </summary>
    public Thickness SelectedIndicatorMargin { get; set; }
    
    /// <summary>
    /// 过滤高亮颜色
    /// </summary>
    public Color FilterHighlightColor { get; set; }

    /// <summary>
    /// 列表项小号内间距
    /// </summary>
    public Thickness ItemPaddingSM { get; set; }

    /// <summary>
    /// 列表项内间距
    /// </summary>
    public Thickness ItemPadding { get; set; }

    /// <summary>
    /// 列表项大号内间距
    /// </summary>
    public Thickness ItemPaddingLG { get; set; }

    /// <summary>
    /// 列表项外边距
    /// </summary>
    public Thickness ItemMargin { get; set; }
    

    public override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        var colorTextDisabled  = EffectiveGlobalToken.ColorTextDisabled;
        var colorTextSecondary = EffectiveGlobalToken.ColorTextSecondary;
        var colorBgTextHover   = EffectiveGlobalToken.ColorBgTextHover;

        ItemColor         = colorTextSecondary;
        ItemHoverColor    = colorTextSecondary;
        ItemSelectedColor = EffectiveGlobalToken.ColorText;

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
