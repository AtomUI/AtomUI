using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
public sealed partial class CardToken : AbstractControlDesignToken
{
    
    /// <summary>
    /// 卡片头部背景色
    /// Background color of card header
    /// </summary>
    public partial Color HeaderBg { get; set; }
    
    /// <summary>
    /// 卡片头部文字大小，大号尺寸
    /// Font size of card header
    /// </summary>
    public partial double HeaderFontSizeLG { get; set; }
    
    /// <summary>
    /// 卡片头部文字大小，默认尺寸
    /// Font size of card header
    /// </summary>
    public partial double HeaderFontSize { get; set; }
    
    /// <summary>
    /// 小号卡片头部文字大小
    /// Font size of small card header
    /// </summary>
    public partial double HeaderFontSizeSM { get; set; }
    
    /// <summary>
    /// 卡片头部高度，大号尺寸
    /// Height of card header
    /// </summary>
    public partial double HeaderHeightLG { get; set; }
    
    /// <summary>
    /// 卡片头部高度，默认尺寸
    /// Height of card header
    /// </summary>
    public partial double HeaderHeight { get; set; }
    
    /// <summary>
    /// 小号卡片头部高度
    /// Height of small card header
    /// </summary>
    public partial double HeaderHeightSM { get; set; }
    
    /// <summary>
    /// 卡片内边距，大号
    /// Padding of card body
    /// </summary>
    public partial Thickness BodyPaddingLG { get; set; }
    
    /// <summary>
    /// 卡片内边距
    /// Padding of card body
    /// </summary>
    public partial Thickness BodyPadding { get; set; }
    
    /// <summary>
    /// 小号卡片内边距
    /// Padding of small card body
    /// </summary>
    public partial Thickness BodyPaddingSM { get; set; }
    
    /// <summary>
    /// 卡片头部内边距,大号
    /// Padding of card head
    /// </summary>
    public partial Thickness HeaderPaddingLG { get; set; }
    
    /// <summary>
    /// 卡片头部内边距
    /// Padding of card head
    /// </summary>
    public partial Thickness HeaderPadding { get; set; }
    
    /// <summary>
    /// 小号卡片头部内边距
    /// Padding of small card head
    /// </summary>
    public partial Thickness HeaderPaddingSM { get; set; }
    
    /// <summary>
    /// 操作区背景色
    /// Background color of card actions
    /// </summary>
    public partial Color ActionsBg { get; set; }
    
    /// <summary>
    /// 操作区每一项的间距
    ///  Margin of each item in card actions
    /// </summary>
    [ReservedToken("antd Card actionsLiMargin (the vertical margin of each action). The AtomUI action row is a " +
                   "ControlHeightLG-high row whose actions stretch over the whole cell, so there is no item margin to set.")]
    public partial double ActionsSpacing { get; set; }
    
    /// <summary>
    /// 内置标签页组件下间距
    /// Margin bottom of tabs component
    /// </summary>
    [ReservedToken("antd Card tabsMarginBottom (pulls the tabs inside the card head onto the head border). CardTabsContent " +
                   "renders its tab bar in the body below the header, so there is no head to overlap.")]
    public partial Thickness TabsMarginBottom { get; set; }
    
    /// <summary>
    /// 额外区文字颜色
    /// Text color of extra area
    /// </summary>
    public partial Color ExtraColor { get; set; }
    
    /// <summary>
    /// 卡片阴影
    /// Shadow of card
    /// </summary>
    public partial BoxShadows CardShadows { get; set; }

    /// <summary>
    /// 卡片静止阴影（AtomUI 扩展，antd 无对应 Token）：非悬停的有边框卡片的阴影，供主题预设表达常驻阴影；
    /// 默认是与 <see cref="CardShadows"/> 同层数的透明阴影，悬停过渡不会增减阴影层
    /// Resting shadow of an outlined card (an AtomUI extension for presets; antd has none). Defaults to transparent
    /// layers matching <see cref="CardShadows"/>, so the hover transition keeps its layer count.
    /// </summary>
    public partial BoxShadows RestingShadows { get; set; }
    
    /// <summary>
    /// 卡片头部内边距
    /// Padding of card header
    /// </summary>
    [ReservedToken("antd Card cardHeadPadding (the title padding of a head that also holds the tabs). CardTabsContent " +
                   "renders its tab bar in the body, so the header keeps its normal height and padding.")]
    public partial Thickness CardHeadPadding { get; set; }
    
    /// <summary>
    /// 卡片基础内边距
    /// Padding of base card
    /// </summary>
    [ReservedToken("antd Card cardPaddingBase (the padding of a grid item). CardGridItem follows the card size with " +
                   "BodyPaddingLG / BodyPadding / BodyPaddingSM instead.")]
    public partial Thickness CardPaddingBase { get; set; }
    
    /// <summary>
    /// 卡片操作区图标大小
    /// Size of card actions icon
    /// </summary>
    public partial double CardActionsIconSize { get; set; }
    
    /// <summary>
    /// 网格项的常驻装饰阴影。网格分隔线由网格容器绘制。
    /// Resting decoration of a grid item. The grid container paints the separators.
    /// </summary>
    public partial BoxShadows CardGridItemShadows { get; set; }

    /// <summary>
    /// 网格项的悬停阴影。
    /// Hover shadow of a grid item.
    /// </summary>
    public partial BoxShadows CardGridHoverShadows { get; set; }
    
}
