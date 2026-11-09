using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
internal sealed class PaginationToken : AbstractControlDesignToken
{

    /// <summary>
    /// 页码选项背景色
    /// Background color of Pagination item
    /// </summary>
    public Color ItemBg { get; set; }
    
    /// <summary>
    /// 页码尺寸
    /// Size of Pagination item
    /// </summary>
    public double ItemSize { get; set; }
        
    /// <summary>
    /// 页码激活态背景色
    /// Background color of active Pagination item
    /// </summary>
    public Color ItemActiveBg { get; set; }
    
    /// <summary>
    /// 小号页码尺寸
    /// Size of small Pagination item
    /// </summary>
    public double ItemSizeSM { get; set; }
    
    /// <summary>
    /// 页码链接背景色
    /// Background color of Pagination item link
    /// </summary>
    [ReservedToken("antd Pagination itemLinkBg: the background of the bordered pagination's previous / next buttons. " +
                   "AtomUI Pagination has no bordered variant, so no element draws it.")]
    public Color ItemLinkBg { get; set; }
    
    /// <summary>
    /// 页码激活态禁用状态背景色
    /// Background color of disabled active Pagination item
    /// </summary>
    public Color ItemActiveBgDisabled { get; set; }
    
    /// <summary>
    /// 页码激活态禁用状态文字颜色
    /// Text color of disabled active Pagination item
    /// </summary>
    public Color ItemActiveColorDisabled { get; set; }
    
    /// <summary>
    /// 输入框背景色
    /// Background color of input
    /// </summary>
    public Color ItemInputBg { get; set; }
    
    /// <summary>
    /// 输入框轮廓偏移量
    /// Outline offset of input
    /// </summary>
    [ReservedToken("antd Pagination inputOutlineOffset (internal): the x offset of the simple pager input's focus " +
                   "box-shadow. The simple pager input here is a LineEdit drawn with the Input family's focus shadow.")]
    public Thickness InputOutlineOffset { get; set; }
    
    /// <summary>
    /// 分页布局间距
    /// Horizontal layout spacing
    /// </summary>
    public double PaginationLayoutSpacing { get; set; }
    
    /// <summary>
    /// 分页布局迷你间距
    /// Horizontal layout mini spacing
    /// </summary>
    public double PaginationLayoutMiniSpacing { get; set; }
    
    /// <summary>
    /// 快速跳转输入框宽度
    /// Width of quick jumper input
    /// </summary>
    public double PaginationQuickJumperInputWidth { get; set; }
    
    /// <summary>
    /// 迷你快速跳转输入框宽度
    /// Width of mini quick jumper input
    /// </summary>
    public double PaginationMiniQuickJumperInputWidth { get; set; }
    
    /// <summary>
    /// 页码横向内边距
    /// Horizontal padding of Pagination item
    /// </summary>
    public Thickness PaginationItemPaddingInline { get; set; }
    
    public PaginationToken()

    {
    }
    
    public override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        ItemBg                    = EffectiveGlobalToken.ColorBgContainer;
        ItemSize                  = EffectiveGlobalToken.ControlHeight;
        ItemSizeSM                = EffectiveGlobalToken.ControlHeightSM;
        ItemActiveBg              = EffectiveGlobalToken.ColorBgContainer;
        ItemLinkBg                = EffectiveGlobalToken.ColorBgContainer;
        ItemActiveColorDisabled   = EffectiveGlobalToken.ColorTextDisabled;
        ItemActiveBgDisabled      = EffectiveGlobalToken.ControlItemBgActiveDisabled;
        ItemInputBg               = EffectiveGlobalToken.ColorBgContainer;

        InputOutlineOffset                  = new Thickness(0);
        PaginationLayoutSpacing             = EffectiveGlobalToken.UniformlyMarginXS;
        PaginationLayoutMiniSpacing         = EffectiveGlobalToken.UniformlyMarginXXS / 2;
        
        PaginationQuickJumperInputWidth     = EffectiveGlobalToken.ControlHeightLG * 1.25;
        PaginationMiniQuickJumperInputWidth = EffectiveGlobalToken.ControlHeightLG * 1.1;
        PaginationItemPaddingInline         = new Thickness(EffectiveGlobalToken.UniformlyMarginXXS * 1.5, 0);
    }
    
}
