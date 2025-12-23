using AtomUI.Media;
using AtomUI.Theme.TokenSystem;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

// vertical: part   (水平时，垂直方向命名为 part)
// horizontal: full (水平时，水平方向命名为 full)
[ControlDesignToken]
internal class SliderToken : AbstractControlDesignToken
{
    public const string ID = "Slider";

    public SliderToken()
        : base(ID)
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
        var controlSize          = SharedToken.ControlHeightLG / 4;
        var controlSizeHover     = SharedToken.ControlHeightSM / 2;
        var handleLineWidth      = SharedToken.LineWidth + increaseThumbWidth;
        var handleLineWidthHover = SharedToken.LineWidth + increaseThumbWidth * 1.5;

        SliderTrackSize                 = controlSizeHover;
        RailSize                        = 4;
        MarkSize                        = 8;
        ThumbCircleSize                 = controlSize;
        ThumbCircleSizeHover            = controlSizeHover;
        ThumbCircleBorderThickness      = new Thickness(handleLineWidth);
        ThumbCircleBorderThicknessHover = new Thickness(handleLineWidthHover);

        RailBg       = SharedToken.ColorFillTertiary;
        RailHoverBg  = SharedToken.ColorFillSecondary;
        TrackBg      = SharedToken.ColorPrimaryBorder;
        TrackHoverBg = SharedToken.ColorPrimaryBorderHover;

        MarkBorderColor       = SharedToken.ColorBorderSecondary;
        MarkBorderColorHover  = SharedToken.ColorFillContentHover;
        MarkBorderColorActive = SharedToken.ColorPrimaryBorder;

        ThumbCircleBorderColor       = SharedToken.ColorPrimaryBorder;
        ThumbCircleBorderHoverColor  = SharedToken.ColorPrimaryBorderHover;
        ThumbCircleBorderActiveColor = SharedToken.ColorPrimary;
        ThumbCircleBorderColorDisabled =
            ColorUtils.OnBackground(SharedToken.ColorTextDisabled, SharedToken.ColorBgContainer);
        TrackBgDisabled = SharedToken.ColorBgContainerDisabled;

        SliderPaddingHorizontal =
            new Thickness(SliderTrackSize / 2, (SharedToken.ControlHeight - SliderTrackSize) / 2);
        SliderPaddingVertical = new Thickness((SharedToken.ControlHeight - SliderTrackSize) / 2,
            SliderTrackSize / 2);
        MarginPartWithMark = new Thickness(0, 0, 0, SharedToken.ControlHeightLG - SliderTrackSize);

        ThumbOutlineColor = ColorUtils.FromRgbF(0.2, ThumbCircleBorderActiveColor.GetRedF(),
            ThumbCircleBorderActiveColor.GetGreenF(),
            ThumbCircleBorderActiveColor.GetBlueF());
        ThumbOutlineThickness = new Thickness(SharedToken.WaveAnimationRange);
        ThumbSize = ThumbCircleSizeHover + ThumbCircleBorderThicknessHover.Left * 2 + ThumbOutlineThickness.Left * 2;
    }
}