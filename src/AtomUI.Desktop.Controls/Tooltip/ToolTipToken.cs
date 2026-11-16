using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
public sealed partial class ToolTipToken : AbstractControlDesignToken
{
    
    public ToolTipToken()

    {
    }

    /// <summary>
    /// tooltip 的最大宽度，超过了就换行
    /// </summary>
    public partial double ToolTipMaxWidth { get; set; }

    /// <summary>
    /// ToolTip 默认的前景色
    /// </summary>
    public partial Color ToolTipColor { get; set; }

    /// <summary>
    /// ToolTip 默认的背景色
    /// </summary>
    public partial Color ToolTipBackground { get; set; }

    /// <summary>
    /// ToolTip 默认的圆角
    /// </summary>
    public partial CornerRadius ToolTipCornerRadius { get; set; }

    /// <summary>
    /// ToolTip 默认的内间距
    /// </summary>
    public partial Thickness ContentPadding { get; set; }
    
    /// <summary>
    /// 弹出与收起的缩放动效时长（antd Tooltip 使用 zoom-big-fast，即 motionDurationFast）
    /// </summary>
    public partial TimeSpan MotionDuration { get; set; }

    /// <summary>
    /// The shadow of the tooltip popup, independent of the menu popup's. Defaults to what the tooltip popup drew
    /// before (the popup token's <c>BoxShadowsSecondary</c>); a theme with a shadowless tooltip (shadcn) sets it
    /// to empty.
    /// </summary>
    public partial BoxShadows ToolTipShadows { get; set; }

    protected override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);

        ToolTipMaxWidth   = 250;
        ToolTipColor      = EffectiveGlobalToken.ColorTextLightSolid;
        ToolTipBackground = EffectiveGlobalToken.ColorBgSpotlight;
        ToolTipCornerRadius = new CornerRadius(4);
        ContentPadding = new Thickness(EffectiveGlobalToken.UniformlyPaddingSM, EffectiveGlobalToken.UniformlyPaddingSM / 2 + 2);
        MotionDuration = EffectiveGlobalToken.MotionDurationFast;
        ToolTipShadows = EffectiveGlobalToken.BoxShadowsSecondary;
    }
    
}
