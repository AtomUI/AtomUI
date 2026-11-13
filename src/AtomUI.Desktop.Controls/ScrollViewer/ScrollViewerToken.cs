using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Media;
using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
public sealed partial class ScrollViewerToken : AbstractControlDesignToken
{
    
    /// <summary>
    /// 极简模式下，滚动条滑块的粗细
    /// </summary>
    public partial double LiteModeThumbThickness { get; set; }
    
    /// <summary>
    /// 正常模式下，滚动条滑块的粗细
    /// </summary>
    public partial double NormalModeThumbThickness { get; set; }
    
    /// <summary>
    /// 滚动条滑块的圆角大小
    /// </summary>
    public partial CornerRadius ThumbCornerRadius { get; set; }

    /// <summary>
    /// 指示器滚动条滑块的圆角大小。
    /// </summary>
    public partial CornerRadius IndicatorThumbCornerRadius { get; set; }

    /// <summary>
    /// 滚动条滑块背景颜色
    /// </summary>
    public partial Color ThumbBg { get; set; }

    /// <summary>
    /// 滚动条滑块鼠标 hover 背景颜色
    /// </summary>
    public partial Color ThumbHoverBg { get; set; }
    
    /// <summary>
    /// 滚动条滑块鼠标按下的背景颜色
    /// </summary>
    public partial Color ThumbActiveBg { get; set; }
    
    #region 内部 Token 定义
    /// <summary>
    /// 水平滚动条内间距
    /// </summary>
    public partial Thickness ScrollBarContentHPadding { get; set; }
    
    /// <summary>
    /// 垂直滚动条内间距
    /// </summary>
    public partial Thickness ScrollBarContentVPadding { get; set; }
    #endregion
    
    public ScrollViewerToken()

    {
    }

    protected override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);

        if (!IsExplicit(ScrollViewerTokenKind.LiteModeThumbThickness))
        {
            LiteModeThumbThickness   = EffectiveGlobalToken.LineWidthBold;
        }

        if (!IsExplicit(ScrollViewerTokenKind.NormalModeThumbThickness))
        {
            NormalModeThumbThickness = EffectiveGlobalToken.SizeXS;
        }

        if (isDarkMode)
        {
            if (!IsExplicit(ScrollViewerTokenKind.ThumbBg))
            {
                ThumbBg       = EffectiveGlobalToken.ColorBorder;
            }

            if (!IsExplicit(ScrollViewerTokenKind.ThumbHoverBg))
            {
                ThumbHoverBg  = ThumbBg.Lighten();
            }

            ThumbActiveBg = ThumbHoverBg.Lighten();
        }
        else
        {
            if (!IsExplicit(ScrollViewerTokenKind.ThumbBg))
            {
                ThumbBg       = EffectiveGlobalToken.ColorBorder;
            }

            if (!IsExplicit(ScrollViewerTokenKind.ThumbHoverBg))
            {
                ThumbHoverBg  = ThumbBg.Darken();
            }

            ThumbActiveBg = ThumbHoverBg.Darken();
        }
        ThumbCornerRadius        = new CornerRadius(NormalModeThumbThickness / 2);
        IndicatorThumbCornerRadius = new CornerRadius(LiteModeThumbThickness / 2);
        ScrollBarContentHPadding = new Thickness(EffectiveGlobalToken.UniformlyPaddingXXS, 0d);
        ScrollBarContentVPadding = new Thickness(0d, EffectiveGlobalToken.UniformlyPaddingXXS);
    }
    
}
