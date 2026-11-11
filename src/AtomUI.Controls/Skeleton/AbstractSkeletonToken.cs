using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Controls.DesignTokens;

[ControlDesignToken]
internal abstract class AbstractSkeletonToken : AbstractControlDesignToken
{
    /// <summary>
    /// 渐变色起点颜色
    /// Start color of gradient
    /// </summary>
    public Color GradientFromColor { get; set; }

    /// <summary>
    /// 渐变色终点颜色
    /// End color of gradient
    /// </summary>
    public Color GradientToColor { get; set; }

    /// <summary>
    /// 默认加载的时长
    /// </summary>
    public TimeSpan LoadingMotionDuration { get; set; }

    /// <summary>
    /// 骨架屏圆角
    /// Border radius of skeleton
    /// </summary>
    public CornerRadius BlockRadius { get; set; }

    /// <summary>
    /// 标题骨架屏高度
    /// Height of title skeleton
    /// </summary>
    public double TitleHeight { get; set; }

    /// <summary>
    /// 段落骨架屏单行高度
    /// Line height of paragraph skeleton
    /// </summary>
    public double ParagraphLineHeight { get; set; }

    public override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        GradientFromColor     = EffectiveGlobalToken.ColorFillContent;
        GradientToColor       = EffectiveGlobalToken.ColorFill;
        TitleHeight           = EffectiveGlobalToken.ControlHeight / 2;
        BlockRadius           = EffectiveGlobalToken.BorderRadiusSM;
        ParagraphLineHeight   = EffectiveGlobalToken.ControlHeight / 2;
        LoadingMotionDuration = TimeSpan.FromSeconds(1.4);
    }
}
