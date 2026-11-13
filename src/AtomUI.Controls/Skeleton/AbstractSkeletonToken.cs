using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Controls.DesignTokens;

[ControlDesignToken]
public abstract partial class AbstractSkeletonToken : AbstractControlDesignToken
{
    /// <summary>
    /// 渐变色起点颜色
    /// Start color of gradient
    /// </summary>
    public partial Color GradientFromColor { get; set; }

    /// <summary>
    /// 渐变色终点颜色
    /// End color of gradient
    /// </summary>
    public partial Color GradientToColor { get; set; }

    /// <summary>
    /// 默认加载的时长
    /// </summary>
    public partial TimeSpan LoadingMotionDuration { get; set; }

    /// <summary>
    /// 骨架屏圆角
    /// Border radius of skeleton
    /// </summary>
    public partial CornerRadius BlockRadius { get; set; }

    /// <summary>
    /// 标题骨架屏高度
    /// Height of title skeleton
    /// </summary>
    public partial double TitleHeight { get; set; }

    /// <summary>
    /// 段落骨架屏单行高度
    /// Line height of paragraph skeleton
    /// </summary>
    public partial double ParagraphLineHeight { get; set; }

    protected override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        // GradientFromColor, GradientToColor and ParagraphLineHeight feed values derived in the terminal, which
        // assigns their defaults behind an IsExplicit guard of its own Token kind.
        TitleHeight           = EffectiveGlobalToken.ControlHeight / 2;
        BlockRadius           = EffectiveGlobalToken.BorderRadiusSM;
        LoadingMotionDuration = TimeSpan.FromSeconds(1.4);
    }
}
