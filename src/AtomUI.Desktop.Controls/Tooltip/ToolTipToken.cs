using AtomUI.Theme.TokenSystem;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
internal class ToolTipToken : AbstractControlDesignToken
{
    public const string ID = "ToolTip";

    public ToolTipToken()
        : base(ID)
    {
    }

    /// <summary>
    /// tooltip 的最大宽度，超过了就换行
    /// </summary>
    public double ToolTipMaxWidth { get; set; }

    /// <summary>
    /// ToolTip 默认的前景色
    /// </summary>
    public Color ToolTipColor { get; set; }

    /// <summary>
    /// ToolTip 默认的背景色
    /// </summary>
    public Color ToolTipBackground { get; set; }

    /// <summary>
    /// ToolTip 默认的圆角
    /// </summary>
    public CornerRadius BorderRadiusOuter { get; set; }

    /// <summary>
    /// ToolTip 默认的内间距
    /// </summary>
    public Thickness ToolTipPadding { get; set; }

    /// <summary>
    /// 内置阴影
    /// </summary>
    public BoxShadows ToolTipShadows { get; set; }

    /// <summary>
    /// 动画时长
    /// </summary>
    public TimeSpan ToolTipMotionDuration { get; set; }

    /// <summary>
    /// ToolTip 箭头三角形大小
    /// </summary>
    public double ToolTipArrowSize { get; set; }

    public override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);

        ToolTipMaxWidth   = 250;
        ToolTipColor      = SharedToken.ColorTextLightSolid;
        ToolTipBackground = SharedToken.ColorBgSpotlight;
        BorderRadiusOuter = new CornerRadius(Math.Max(BorderRadiusOuter.TopLeft, 4),
            Math.Max(BorderRadiusOuter.TopRight, 4),
            Math.Max(BorderRadiusOuter.BottomLeft, 4),
            Math.Max(BorderRadiusOuter.BottomRight, 4));
        ToolTipPadding        = new Thickness(SharedToken.UniformlyPaddingSM, SharedToken.UniformlyPaddingSM / 2 + 2);
        ToolTipShadows        = SharedToken.BoxShadowsSecondary;
        ToolTipMotionDuration = SharedToken.MotionDurationMid;
    }
}