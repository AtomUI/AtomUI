using AtomUI.Theme.TokenSystem;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
internal class PaginationToken : AbstractControlDesignToken
{
    public const string ID = "Pagination";
    
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
        : base(ID)
    {
    }
    
    public override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        ItemBg                    = SharedToken.ColorBgContainer;
        ItemSize                  = SharedToken.ControlHeight;
        ItemSizeSM                = SharedToken.ControlHeightSM;
        ItemActiveBg              = SharedToken.ColorBgContainer;
        ItemLinkBg                = SharedToken.ColorBgContainer;
        ItemActiveColorDisabled   = SharedToken.ColorTextDisabled;
        ItemActiveBgDisabled      = SharedToken.ControlItemBgActiveDisabled;
        ItemInputBg               = SharedToken.ColorBgContainer;

        InputOutlineOffset                  = new Thickness(0);
        PaginationLayoutSpacing             = SharedToken.UniformlyMarginXS;
        PaginationLayoutMiniSpacing         = SharedToken.UniformlyMarginXXS / 2;
        
        PaginationQuickJumperInputWidth     = SharedToken.ControlHeightLG * 1.25;
        PaginationMiniQuickJumperInputWidth = SharedToken.ControlHeightLG * 1.1;
        PaginationItemPaddingInline         = new Thickness(SharedToken.UniformlyMarginXXS * 1.5);
    }
}