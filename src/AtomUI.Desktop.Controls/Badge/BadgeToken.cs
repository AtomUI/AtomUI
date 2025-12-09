using AtomUI.Theme.TokenSystem;
using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Immutable;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
internal class BadgeToken : AbstractControlDesignToken
{
    public const string ID = "Badge";

    public BadgeToken()
        : base(ID)
    {
    }

    /// <summary>
    /// 徽标高度
    /// </summary>
    public double IndicatorHeight { get; set; }

    /// <summary>
    /// 小号徽标高度
    /// </summary>
    public double IndicatorHeightSM { get; set; }

    /// <summary>
    /// 点状徽标尺寸
    /// </summary>
    public double DotSize { get; set; }

    /// <summary>
    /// 徽标文本尺寸
    /// </summary>
    public double TextFontSize { get; set; }

    /// <summary>
    /// 小号徽标文本尺寸
    /// </summary>
    public double TextFontSizeSM { get; set; }

    /// <summary>
    /// 徽标文本粗细
    /// </summary>
    public FontWeight TextFontWeight { get; set; }

    /// <summary>
    /// 状态徽标尺寸
    /// </summary>
    public double StatusSize { get; set; }

    #region 内部使用的 Token

    public double BadgeFontHeight { get; set; }
    public Color BadgeTextColor { get; set; }
    public Color BadgeColor { get; set; }
    public Color BadgeColorHover { get; set; }
    public double BadgeShadowSize { get; set; }
    public Color BadgeShadowColor { get; set; }
    public TimeSpan BadgeProcessingDuration { get; set; }
    public Point BadgeRibbonOffset { get; set; }
    public ImmutableTransform? BadgeRibbonCornerTransform { get; set; }
    public int BadgeRibbonCornerDarkenAmount { get; set; }
    public Thickness BadgeRibbonTextPadding { get; set; }
    public Thickness DotBadgeLabelMargin { get; set; }
    public Thickness CountBadgeTextPadding { get; set; }
    public CornerRadius CountBadgeCornerRadius { get; set; }
    public CornerRadius CountBadgeCornerRadiusSM { get; set; }
    
    #endregion

    public override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        var lineWidth = SharedToken.LineWidth;
        IndicatorHeight   = Math.Round(SharedToken.FontSize * SharedToken.RelativeLineHeight) - 2 * lineWidth;
        IndicatorHeightSM = SharedToken.FontSize;
        DotSize           = SharedToken.FontSizeSM / 2;
        TextFontSize      = SharedToken.FontSizeSM;
        TextFontSizeSM    = SharedToken.FontSizeSM - 2;
        TextFontWeight    = FontWeight.Normal;
        StatusSize        = SharedToken.FontSizeSM / 2;

        // 设置内部 token
        BadgeFontHeight         = SharedToken.FontHeight;
        BadgeShadowSize         = lineWidth;
        BadgeTextColor          = SharedToken.ColorTextLightSolid;
        BadgeColor              = SharedToken.ColorError;
        BadgeColorHover         = SharedToken.ColorErrorHover;
        BadgeShadowColor        = SharedToken.ColorBorderBg;
        BadgeProcessingDuration = TimeSpan.FromMilliseconds(1200);
        BadgeRibbonOffset       = new Point(SharedToken.UniformlyMarginXS, SharedToken.UniformlyMarginXS);

        BadgeRibbonCornerTransform    = new ScaleTransform(1, 0.75).ToImmutable();
        BadgeRibbonCornerDarkenAmount = 15;
        BadgeRibbonTextPadding        = new Thickness(SharedToken.UniformlyPaddingXS, 0);
        DotBadgeLabelMargin           = new Thickness(SharedToken.UniformlyMarginXS, 0, 0, 0);
        CountBadgeTextPadding         = new Thickness(SharedToken.UniformlyPaddingXXS, 0);
        CountBadgeCornerRadius        = new CornerRadius(IndicatorHeight);
        CountBadgeCornerRadiusSM      = new CornerRadius(IndicatorHeightSM);
    }
}