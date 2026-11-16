using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
public sealed partial class CascaderToken : AbstractSelectInputToken
{
    
    /// <summary>
    /// 节点标题高度
    /// </summary>
    public partial double HeaderHeight { get; set; }
    
    /// <summary>
    /// 选择器宽度
    /// Width of Cascader
    /// </summary>
    public partial double ControlWidth { get; set; }
    
    /// <summary>
    /// 选项宽度
    /// Width of item
    /// </summary>
    public partial double ControlItemWidth { get; set; }
    
    /// <summary>
    /// 下拉菜单高度
    /// Height of dropdown
    /// </summary>
    public partial double DropdownHeight { get; set; }
    
    /// <summary>
    /// 选项选中时背景色
    /// Background color of selected item
    /// </summary>
    public partial Color OptionSelectedBg { get; set; }
    
    /// <summary>
    /// 选项 hover 时背景色
    /// Background color of hover item
    /// </summary>
    public partial Color OptionHoverBg { get; set; }
    
    /// <summary>
    /// 选项选中时文本颜色
    /// Text color when option is selected
    /// </summary>
    public partial Color OptionSelectedColor { get; set; }
    
    /// <summary>
    /// 选项选中时字重
    /// Font weight of selected item
    /// </summary>
    public partial FontWeight OptionSelectedFontWeight { get; set; }
    
    /// <summary>
    /// 选项内间距
    /// Padding of menu item
    /// </summary>
    public partial Thickness OptionPadding { get; set; }
    
    /// <summary>
    /// 选项菜单（单列）内间距
    /// Padding of menu item (single column)
    /// </summary>
    public partial Thickness MenuPadding { get; set; }
    
    /// <summary>
    /// 过滤高亮颜色
    /// </summary>
    public partial Color FilterHighlightColor { get; set; }
    
    /// <summary>
    /// Item 头的元素间距
    /// </summary>
    public partial double ItemHeaderSpacing { get; set; }

    public CascaderToken()

    {
    }

    protected override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        var itemPaddingVertical = Math.Round((EffectiveGlobalToken.ControlHeight - EffectiveGlobalToken.FontHeight) / 2);
        ControlWidth             = 184;
        ControlItemWidth         = 111;
        DropdownHeight           = 180;
        OptionHoverBg            = EffectiveGlobalToken.ControlItemBgHover;
        OptionSelectedBg         = EffectiveGlobalToken.ControlItemBgActive;
        OptionSelectedFontWeight = EffectiveGlobalToken.FontWeightStrong;
        OptionPadding            = new Thickness(EffectiveGlobalToken.UniformlyPaddingSM, itemPaddingVertical);
        MenuPadding              = EffectiveGlobalToken.PaddingXXS;
        OptionSelectedColor      = EffectiveGlobalToken.ControlItemColorActive;
        HeaderHeight             = EffectiveGlobalToken.ControlHeightSM;
        FilterHighlightColor     = EffectiveGlobalToken.ColorError;
        ItemHeaderSpacing        = EffectiveGlobalToken.SpacingXXS;
    }
    
}
