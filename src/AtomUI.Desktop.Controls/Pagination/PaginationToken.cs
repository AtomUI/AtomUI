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
    
    
    
    
    
    
    /// <summary>
    /// 页码激活态文本颜色
    /// Text color of the active Pagination item
    /// </summary>
    public partial Color ItemActiveColor { get; set; }

    /// <summary>
    /// 页码激活态边框颜色
    /// Border color of the active Pagination item
    /// </summary>
    public partial Color ItemActiveBorderColor { get; set; }

    /// <summary>
    /// 页码激活态悬停文本颜色
    /// Text color of the hovered active Pagination item
    /// </summary>
    public partial Color ItemActiveHoverColor { get; set; }

    /// <summary>
    /// 页码激活态悬停边框颜色
    /// Border color of the hovered active Pagination item
    /// </summary>
    public partial Color ItemActiveHoverBorderColor { get; set; }

    /// <summary>
    /// 页码激活态字重
    /// Font weight of the active Pagination item
    /// </summary>
    public partial FontWeight ItemActiveFontWeight { get; set; }
    
    /// <summary>Inline padding between the page number and the edges of its pagination item.</summary>
    public partial Thickness PaginationItemPaddingInline { get; set; }
    
}
