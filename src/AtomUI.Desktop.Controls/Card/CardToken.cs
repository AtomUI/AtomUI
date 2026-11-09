using AtomUI.Media;
using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
internal sealed class CardToken : AbstractControlDesignToken
{
    
    /// <summary>
    /// 卡片头部背景色
    /// Background color of card header
    /// </summary>
    public Color HeaderBg { get; set; }
    
    /// <summary>
    /// 卡片头部文字大小，大号尺寸
    /// Font size of card header
    /// </summary>
    public double HeaderFontSizeLG { get; set; }
    
    /// <summary>
    /// 卡片头部文字大小，默认尺寸
    /// Font size of card header
    /// </summary>
    public double HeaderFontSize { get; set; }
    
    /// <summary>
    /// 小号卡片头部文字大小
    /// Font size of small card header
    /// </summary>
    public double HeaderFontSizeSM { get; set; }
    
    /// <summary>
    /// 卡片头部高度，大号尺寸
    /// Height of card header
    /// </summary>
    public double HeaderHeightLG { get; set; }
    
    /// <summary>
    /// 卡片头部高度，默认尺寸
    /// Height of card header
    /// </summary>
    public double HeaderHeight { get; set; }
    
    /// <summary>
    /// 小号卡片头部高度
    /// Height of small card header
    /// </summary>
    public double HeaderHeightSM { get; set; }
    
    /// <summary>
    /// 卡片内边距，大号
    /// Padding of card body
    /// </summary>
    public Thickness BodyPaddingLG { get; set; }
    
    /// <summary>
    /// 卡片内边距
    /// Padding of card body
    /// </summary>
    public Thickness BodyPadding { get; set; }
    
    /// <summary>
    /// 小号卡片内边距
    /// Padding of small card body
    /// </summary>
    public Thickness BodyPaddingSM { get; set; }
    
    /// <summary>
    /// 卡片头部内边距,大号
    /// Padding of card head
    /// </summary>
    public Thickness HeaderPaddingLG { get; set; }
    
    /// <summary>
    /// 卡片头部内边距
    /// Padding of card head
    /// </summary>
    public Thickness HeaderPadding { get; set; }
    
    /// <summary>
    /// 小号卡片头部内边距
    /// Padding of small card head
    /// </summary>
    public Thickness HeaderPaddingSM { get; set; }
    
    /// <summary>
    /// 操作区背景色
    /// Background color of card actions
    /// </summary>
    public Color ActionsBg { get; set; }
    
    /// <summary>
    /// 操作区每一项的间距
    ///  Margin of each item in card actions
    /// </summary>
    [ReservedToken("antd Card actionsLiMargin (the vertical margin of each action). The AtomUI action row is a " +
                   "ControlHeightLG-high row whose actions stretch over the whole cell, so there is no item margin to set.")]
    public double ActionsSpacing { get; set; }
    
    /// <summary>
    /// 内置标签页组件下间距
    /// Margin bottom of tabs component
    /// </summary>
    [ReservedToken("antd Card tabsMarginBottom (pulls the tabs inside the card head onto the head border). CardTabsContent " +
                   "renders its tab bar in the body below the header, so there is no head to overlap.")]
    public Thickness TabsMarginBottom { get; set; }
    
    /// <summary>
    /// 额外区文字颜色
    /// Text color of extra area
    /// </summary>
    public Color ExtraColor { get; set; }
    
    /// <summary>
    /// 卡片阴影
    /// Shadow of card
    /// </summary>
    public BoxShadows CardShadows { get; set; }
    
    /// <summary>
    /// 卡片头部内边距
    /// Padding of card header
    /// </summary>
    [ReservedToken("antd Card cardHeadPadding (the title padding of a head that also holds the tabs). CardTabsContent " +
                   "renders its tab bar in the body, so the header keeps its normal height and padding.")]
    public Thickness CardHeadPadding { get; set; }
    
    /// <summary>
    /// 卡片基础内边距
    /// Padding of base card
    /// </summary>
    [ReservedToken("antd Card cardPaddingBase (the padding of a grid item). CardGridItem follows the card size with " +
                   "BodyPaddingLG / BodyPadding / BodyPaddingSM instead.")]
    public Thickness CardPaddingBase { get; set; }
    
    /// <summary>
    /// 卡片操作区图标大小
    /// Size of card actions icon
    /// </summary>
    public double CardActionsIconSize { get; set; }
    
    /// <summary>
    /// 表格内容的默认阴影
    /// </summary>
    public BoxShadows CardGridItemShadows { get; set; }
    
    public CardToken()

    {
    }

    public override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        HeaderBg         = Colors.Transparent;
        HeaderFontSizeLG = EffectiveGlobalToken.FontSizeLG;
        HeaderFontSize   = EffectiveGlobalToken.FontSize;
        HeaderFontSizeSM = EffectiveGlobalToken.FontSize;

        HeaderHeightLG   = EffectiveGlobalToken.FontHeightLG + EffectiveGlobalToken.UniformlyPadding * 2;
        HeaderHeight     = EffectiveGlobalToken.FontHeight + EffectiveGlobalToken.UniformlyPaddingXS * 2;
        HeaderHeightSM   = EffectiveGlobalToken.FontHeightSM;
        ActionsBg        = EffectiveGlobalToken.ColorBgContainer;
        ActionsSpacing   = EffectiveGlobalToken.SpacingSM;
        TabsMarginBottom = new Thickness(0, 0, 0, -EffectiveGlobalToken.UniformlyPadding - EffectiveGlobalToken.LineWidth);
        ExtraColor       = EffectiveGlobalToken.ColorText;
        
        BodyPaddingLG = EffectiveGlobalToken.PaddingLG;
        BodyPadding   = EffectiveGlobalToken.PaddingSM;
        BodyPaddingSM = EffectiveGlobalToken.PaddingXS;
        
        HeaderPaddingLG  = new Thickness(EffectiveGlobalToken.UniformlyPaddingLG, 0);
        HeaderPadding    = new Thickness(EffectiveGlobalToken.UniformlyPaddingSM, 0);
        HeaderPaddingSM  = new Thickness(EffectiveGlobalToken.UniformlyPaddingXS, 0);

        CardShadows         = new BoxShadows(new BoxShadow()
        {
            OffsetX = 0,
            OffsetY = 1,
            Blur    = 2,
            Spread  = -1,
            Color   = ColorUtils.FromRgbF(0.16, 0, 0, 0)
        }, [
            new BoxShadow
            {
                OffsetX = 0,
                OffsetY = 3,
                Blur    = 6,
                Spread  = 0,
                Color   = ColorUtils.FromRgbF(0.12, 0, 0, 0)
            },
            new BoxShadow
            {
                OffsetX = 0,
                OffsetY = 5,
                Blur    = 12,
                Spread  = 4,
                Color   = ColorUtils.FromRgbF(0.09, 0, 0, 0)
            }
        ]);
        var lineWidth = EffectiveGlobalToken.LineWidth;
        CardGridItemShadows = new BoxShadows(new BoxShadow()
        {
            OffsetX = lineWidth,
            OffsetY = 0,
            Blur    = 0,
            Spread  = 0,
            Color   = EffectiveGlobalToken.ColorBorderSecondary
        }, [
            new BoxShadow
            {
                OffsetX = 0,
                OffsetY = lineWidth,
                Blur    = 0,
                Spread  = 0,
                Color   = EffectiveGlobalToken.ColorBorderSecondary
            },
            new BoxShadow
            {
                OffsetX = lineWidth,
                OffsetY = lineWidth,
                Blur    = 0,
                Spread  = 0,
                Color   = EffectiveGlobalToken.ColorBorderSecondary
            },
            new BoxShadow
            {
                IsInset = true,
                OffsetX = lineWidth,
                OffsetY = 0,
                Blur    = 0,
                Spread  = 0,
                Color   = EffectiveGlobalToken.ColorBorderSecondary
            },
            new BoxShadow
            {
                IsInset = true,
                OffsetX = 0,
                OffsetY = lineWidth,
                Blur    = 0,
                Spread  = 0,
                Color   = EffectiveGlobalToken.ColorBorderSecondary
            },
        ]);
        CardHeadPadding     = EffectiveGlobalToken.Padding;
        CardPaddingBase     = EffectiveGlobalToken.PaddingLG;
        CardActionsIconSize = EffectiveGlobalToken.FontSize;
    }
    
}
