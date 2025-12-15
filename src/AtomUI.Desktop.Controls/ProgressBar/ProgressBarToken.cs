using AtomUI.Theme.TokenSystem;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
internal class ProgressBarToken : AbstractControlDesignToken
{
    public const string ID = "ProgressBar";

    /// <summary>
    /// 进度条默认颜色
    /// </summary>
    public Color DefaultColor { get; set; }

    /// <summary>
    /// 进度条剩余部分颜色
    /// </summary>
    public Color RemainingColor { get; set; }

    /// <summary>
    /// 圆形进度条文字颜色
    /// </summary>
    public Color CircleTextColor { get; set; }

    /// <summary>
    /// 条状进度条圆角
    /// </summary>
    public CornerRadius LineBorderRadius { get; set; }

    /// <summary>
    /// 圆形进度条文本最小大小
    /// </summary>
    public double CircleMinimumTextFontSize { get; set; }

    /// <summary>
    /// 圆形进度条图标最小大小
    /// </summary>
    public double CircleMinimumIconSize { get; set; }

    // 内部属性
    public double ProgressStepMinWidth { get; set; }
    public Thickness ProgressStepMarginInlineEnd { get; set; } // Step 之间的间距
    public double ProgressActiveMotionDuration { get; set; }
    public double LineExtraInfoMargin { get; set; } // 额外信息和 Indicator 之间的间距
    public double LineProgressPadding { get; set; } // 线行进度条的内间距
    public double LineInfoIconSize { get; set; }
    public double LineInfoIconSizeSM { get; set; }

    public ProgressBarToken()
        : base(ID)
    {
    }

    public override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);

        CircleTextColor  = SharedToken.ColorText;
        DefaultColor     = SharedToken.ColorInfo;
        RemainingColor   = SharedToken.ColorFillSecondary;
        LineBorderRadius = new CornerRadius(100); // magic for capsule shape, should be a very large number
        // 这两个要通过计算
        CircleMinimumIconSize     = SharedToken.SizeXS;
        CircleMinimumTextFontSize = SharedToken.FontSizeSM - 2;
        LineInfoIconSize          = SharedToken.IconSize;
        LineInfoIconSizeSM        = SharedToken.IconSizeSM;
        LineExtraInfoMargin       = SharedToken.ControlPaddingHorizontalSM;
        LineProgressPadding       = SharedToken.UniformlyPaddingXXS / 2;
    }
}