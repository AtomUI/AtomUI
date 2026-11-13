using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
public sealed partial class PaginationToken : AbstractPaginationToken
{

    /// <summary>
    /// 页码选项背景色
    /// Background color of Pagination item
    /// </summary>
    public partial Color ItemBg { get; set; }
    
        
    /// <summary>
    /// 页码激活态背景色
    /// Background color of active Pagination item
    /// </summary>
    public partial Color ItemActiveBg { get; set; }
    
    
    /// <summary>
    /// 页码链接背景色
    /// Background color of Pagination item link
    /// </summary>
    [ReservedToken("antd Pagination itemLinkBg: the background of the bordered pagination's previous / next buttons. " +
                   "AtomUI Pagination has no bordered variant, so no element draws it.")]
    public partial Color ItemLinkBg { get; set; }
    
    /// <summary>
    /// 页码激活态禁用状态背景色
    /// Background color of disabled active Pagination item
    /// </summary>
    public partial Color ItemActiveBgDisabled { get; set; }
    
    
    
    /// <summary>
    /// 输入框轮廓偏移量
    /// Outline offset of input
    /// </summary>
    [ReservedToken("antd Pagination inputOutlineOffset (internal): the x offset of the simple pager input's focus " +
                   "box-shadow. The simple pager input here is a LineEdit drawn with the Input family's focus shadow.")]
    public partial Thickness InputOutlineOffset { get; set; }
    
    
    
    
    
    
    public PaginationToken()

    {
    }
    
    public partial Thickness PaginationItemPaddingInline { get; set; }

    protected override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        PaginationItemPaddingInline = new Thickness(EffectiveGlobalToken.UniformlyMarginXXS * 1.5, 0);
        ItemBg                    = EffectiveGlobalToken.ColorBgContainer;
        ItemActiveBg              = EffectiveGlobalToken.ColorBgContainer;
        ItemLinkBg                = EffectiveGlobalToken.ColorBgContainer;
        ItemActiveBgDisabled      = EffectiveGlobalToken.ControlItemBgActiveDisabled;

        InputOutlineOffset                  = new Thickness(0);
        
    }
    
}
