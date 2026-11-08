using AtomUI.Media;
using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

// vertical: part   (水平时，垂直方向命名为 part)
// horizontal: full (水平时，水平方向命名为 full)
[ControlDesignToken]
internal sealed class SliderToken : AbstractControlDesignToken
{
    
    public SliderToken()

    {
    }

    /// <summary>
    /// 滑动条控件的高度
    /// </summary>
    public double SliderTrackSize { get; set; }

    /// <summary>
    /// 轨道高度
    /// </summary>
    public double RailSize { get; set; }

    /// <summary>
    /// Mark 的大小
    /// </summary>
    public double MarkSize { get; set; }

    /// <summary>
    /// Thumb 推荐的大小，方便风格使用，是最大需要的大小
    /// </summary>
    public double ThumbSize { get; set; }

    /// <summary>
    /// 滑块尺寸
    /// </summary>
    public double ThumbCircleSize { get; set; }

    /// <summary>
    /// 滑块尺寸（悬浮态）
    /// </summary>
    public double ThumbCircleSizeHover { get; set; }

    /// <summary>
    /// 滑块边框宽度
    /// </summary>
    public Thickness ThumbCircleBorderThickness { get; set; }

    /// <summary>
    /// 滑块边框宽度（悬浮态）
    /// </summary>
    public Thickness ThumbCircleBorderThicknessHover { get; set; }

    /// <summary>
    /// 轨道背景色
    /// </summary>
    public Color RailBg { get; set; }

    /// <summary>
    /// 轨道背景色（悬浮态）
    /// </summary>
    public Color RailHoverBg { get; set; }

    /// <summary>
    /// 轨道已覆盖部分背景色
    /// </summary>
    public Color TrackBg { get; set; }

    /// <summary>
    /// 轨道已覆盖部分背景色（悬浮态）
    /// </summary>
    public Color TrackHoverBg { get; set; }

    /// <summary>
    /// Mark 的背景颜色
    /// </summary>
    public Color MarkBorderColor { get; set; }

    /// <summary>
    /// Mark 的背景 hover 效果
    /// </summary>
    public Color MarkBorderColorHover { get; set; }

    /// <summary>
    /// Mark 的背景激活时候效果
    /// </summary>
    public Color MarkBorderColorActive { get; set; }

    /// <summary>
    /// 滑块颜色
    /// </summary>
    public Color ThumbCircleBorderColor { get; set; }

    /// <summary>
    /// 滑块颜色 hover
    /// </summary>
    public Color ThumbCircleBorderHoverColor { get; set; }

    /// <summary>
    /// 滑块激活态颜色
    /// </summary>
    public Color ThumbCircleBorderActiveColor { get; set; }

    /// <summary>
    /// 滑块禁用颜色
    /// </summary>
    public Color ThumbCircleBorderColorDisabled { get; set; }

    /// <summary>
    /// 滑块的 outline 环的颜色
    /// </summary>
    public Color ThumbOutlineColor { get; set; }

    /// <summary>
    /// 滑块的 outline 环的厚度
    /// </summary>
    public Thickness ThumbOutlineThickness { get; set; }

    /// <summary>
    /// 轨道禁用态背景色
    /// </summary>
    public Color TrackBgDisabled { get; set; }

    public Thickness SliderPaddingHorizontal { get; set; }
    public Thickness SliderPaddingVertical { get; set; }
    public Thickness MarginPartWithMark { get; set; }

    public override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        // Thumb line width is always width-er 1px
        var increaseThumbWidth   = 1d;
        var controlSize          = EffectiveGlobalToken.ControlHeightLG / 4;
        var controlSizeHover     = EffectiveGlobalToken.ControlHeightSM / 2;
        var handleLineWidth      = EffectiveGlobalToken.LineWidth + increaseThumbWidth;
        var handleLineWidthHover = EffectiveGlobalToken.LineWidth + increaseThumbWidth * 1.5;

        if (!IsExplicit(SliderTokenKind.SliderTrackSize))
        {
            SliderTrackSize                 = controlSizeHover;
        }

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

        SliderPaddingHorizontal =
            new Thickness(SliderTrackSize / 2, (EffectiveGlobalToken.ControlHeight - SliderTrackSize) / 2);
        SliderPaddingVertical = new Thickness((EffectiveGlobalToken.ControlHeight - SliderTrackSize) / 2,
            SliderTrackSize / 2);
        MarginPartWithMark = new Thickness(0, 0, 0, EffectiveGlobalToken.ControlHeightLG - SliderTrackSize);

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
