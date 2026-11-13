using AtomUI.Media;
using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

// vertical: part   (水平时，垂直方向命名为 part)
// horizontal: full (水平时，水平方向命名为 full)
[ControlDesignToken]
public sealed partial class SliderToken : AbstractControlDesignToken
{
    
    public SliderToken()

    {
    }

    /// <summary>
    /// 滑动条控件的高度
    /// Slider track size (antd controlSize)
    /// </summary>
    [ReservedToken("antd Slider controlSize. The Slider cross size is pinned to ControlHeight by the derived padding and " +
                   "the rail inset follows the thumb size, so this Token has no design-meaningful effect yet.")]
    public partial double SliderTrackSize { get; set; }

    /// <summary>
    /// 轨道高度
    /// </summary>
    public partial double RailSize { get; set; }

    /// <summary>
    /// Mark 的大小
    /// </summary>
    public partial double MarkSize { get; set; }

    /// <summary>
    /// Thumb 推荐的大小，方便风格使用，是最大需要的大小
    /// </summary>
    public partial double ThumbSize { get; set; }

    /// <summary>
    /// 滑块尺寸
    /// </summary>
    public partial double ThumbCircleSize { get; set; }

    /// <summary>
    /// 滑块尺寸（悬浮态）
    /// </summary>
    public partial double ThumbCircleSizeHover { get; set; }

    /// <summary>
    /// 滑块边框宽度
    /// </summary>
    public partial Thickness ThumbCircleBorderThickness { get; set; }

    /// <summary>
    /// 滑块边框宽度（悬浮态）
    /// </summary>
    public partial Thickness ThumbCircleBorderThicknessHover { get; set; }

    /// <summary>
    /// 轨道背景色
    /// </summary>
    public partial Color RailBg { get; set; }

    /// <summary>
    /// 轨道背景色（悬浮态）
    /// </summary>
    public partial Color RailHoverBg { get; set; }

    /// <summary>
    /// 轨道已覆盖部分背景色
    /// </summary>
    public partial Color TrackBg { get; set; }

    /// <summary>
    /// 轨道已覆盖部分背景色（悬浮态）
    /// </summary>
    public partial Color TrackHoverBg { get; set; }

    /// <summary>
    /// Mark 的背景颜色
    /// </summary>
    public partial Color MarkBorderColor { get; set; }

    /// <summary>
    /// Mark 的背景 hover 效果
    /// </summary>
    public partial Color MarkBorderColorHover { get; set; }

    /// <summary>
    /// Mark 的背景激活时候效果
    /// </summary>
    public partial Color MarkBorderColorActive { get; set; }

    /// <summary>
    /// 滑块颜色
    /// </summary>
    public partial Color ThumbCircleBorderColor { get; set; }

    /// <summary>
    /// 滑块颜色 hover
    /// </summary>
    public partial Color ThumbCircleBorderHoverColor { get; set; }

    /// <summary>
    /// 滑块激活态颜色
    /// </summary>
    public partial Color ThumbCircleBorderActiveColor { get; set; }

    /// <summary>
    /// 滑块禁用颜色
    /// </summary>
    public partial Color ThumbCircleBorderColorDisabled { get; set; }

    /// <summary>
    /// 滑块的 outline 环的颜色
    /// </summary>
    public partial Color ThumbOutlineColor { get; set; }

    /// <summary>
    /// 滑块的 outline 环的厚度
    /// </summary>
    public partial Thickness ThumbOutlineThickness { get; set; }

    /// <summary>
    /// 轨道禁用态背景色
    /// </summary>
    public partial Color TrackBgDisabled { get; set; }

    public partial Thickness SliderPaddingHorizontal { get; set; }
    public partial Thickness SliderPaddingVertical { get; set; }
    /// <summary>
    /// 带刻度时的底部外边距
    /// Bottom margin of a slider with marks
    /// </summary>
    [ReservedToken("antd Slider marginPartWithMark (controlHeightLG - controlSize). SliderTrack measures the mark labels " +
                   "and adds their size to the track height, so no fixed mark margin is applied yet.")]
    public partial Thickness MarginPartWithMark { get; set; }

    protected override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        // Thumb line width is always width-er 1px
        var increaseThumbWidth   = 1d;
        var controlSize          = EffectiveGlobalToken.ControlHeightLG / 4;
        var controlSizeHover     = EffectiveGlobalToken.ControlHeightSM / 2;
        var handleLineWidth      = EffectiveGlobalToken.LineWidth + increaseThumbWidth;
        var handleLineWidthHover = EffectiveGlobalToken.LineWidth + increaseThumbWidth * 1.5;

        SliderTrackSize                 = controlSizeHover;

        RailSize                        = 4;
        MarkSize                        = 8;
        ThumbCircleSize                 = controlSize;

        if (!IsExplicit(SliderTokenKind.ThumbCircleSizeHover))
        {
            ThumbCircleSizeHover            = controlSizeHover;
        }

        ThumbCircleBorderThickness      = new Thickness(handleLineWidth);

        if (!IsExplicit(SliderTokenKind.ThumbCircleBorderThicknessHover))
        {
            ThumbCircleBorderThicknessHover = new Thickness(handleLineWidthHover);
        }

        RailBg       = EffectiveGlobalToken.ColorFillTertiary;
        RailHoverBg  = EffectiveGlobalToken.ColorFillSecondary;
        TrackBg      = EffectiveGlobalToken.ColorPrimaryBorder;
        TrackHoverBg = EffectiveGlobalToken.ColorPrimaryBorderHover;

        MarkBorderColor       = EffectiveGlobalToken.ColorBorderSecondary;
        MarkBorderColorHover  = EffectiveGlobalToken.ColorFillContentHover;
        MarkBorderColorActive = EffectiveGlobalToken.ColorPrimaryBorder;

        ThumbCircleBorderColor       = EffectiveGlobalToken.ColorPrimaryBorder;
        ThumbCircleBorderHoverColor  = EffectiveGlobalToken.ColorPrimaryBorderHover;

        if (!IsExplicit(SliderTokenKind.ThumbCircleBorderActiveColor))
        {
            ThumbCircleBorderActiveColor = EffectiveGlobalToken.ColorPrimary;
        }

        ThumbCircleBorderColorDisabled =
            ColorUtils.OnBackground(EffectiveGlobalToken.ColorTextDisabled, EffectiveGlobalToken.ColorBgContainer);
        TrackBgDisabled = EffectiveGlobalToken.ColorBgContainerDisabled;

        // The track reserves the hovered handle circle across the rail; the padding brings the cross size to
        // ControlHeight. SliderTrackSize (antd controlSize) is reserved and does not take part in the layout.
        var trackCrossSize = ThumbCircleSizeHover;
        SliderPaddingHorizontal =
            new Thickness(trackCrossSize / 2, (EffectiveGlobalToken.ControlHeight - trackCrossSize) / 2);
        SliderPaddingVertical = new Thickness((EffectiveGlobalToken.ControlHeight - trackCrossSize) / 2,
            trackCrossSize / 2);
        MarginPartWithMark = new Thickness(0, 0, 0, EffectiveGlobalToken.ControlHeightLG - controlSize);

        ThumbOutlineColor = ColorUtils.FromRgbF(0.2, ThumbCircleBorderActiveColor.GetRedF(),
            ThumbCircleBorderActiveColor.GetGreenF(),
            ThumbCircleBorderActiveColor.GetBlueF());

        if (!IsExplicit(SliderTokenKind.ThumbOutlineThickness))
        {
            ThumbOutlineThickness = new Thickness(EffectiveGlobalToken.WaveAnimationRange);
        }

        ThumbSize = ThumbCircleSizeHover + ThumbCircleBorderThicknessHover.Left * 2 + ThumbOutlineThickness.Left * 2;
    }
    
}
