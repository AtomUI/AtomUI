using Avalonia;

namespace AtomUI.Theme.DesignTokens;

[GlobalDesignToken]
public partial class DesignToken
{
    /// <summary>
    /// 线宽
    /// 描边类组件的默认线宽，如 Button、Input、Select 等输入类控件。
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Map)]
    public partial double LineWidthBold { get; set; } = 1;

    /// <summary>
    /// 边框的宽细
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Map)]
    public partial Thickness BorderThickness { get; set; }

    /// <summary>
    /// XS号圆角
    /// XS号圆角，用于组件中的一些小圆角，如 Segmented 、Arrow 等一些内部圆角的组件样式中。
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Map)]
    public partial CornerRadius BorderRadiusXS { get; set; } = new CornerRadius(2);

    /// <summary>
    /// SM号圆角
    /// SM号圆角，用于组件小尺寸下的圆角，如 Button、Input、Select 等输入类控件在 small size 下的圆角
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Map)]
    public partial CornerRadius BorderRadiusSM { get; set; } = new CornerRadius(4);

    /// <summary>
    /// MD号圆角
    /// MD号圆角，组件默认尺寸下的圆角，与种子 <see cref="BorderRadius"/> 同值；控件读这一项而不是种子，语言可以为某个控件覆盖它。
    /// Medium corner radius: the radius of a control at its default size, equal to the seed BorderRadius. Controls read
    /// this Map token, not the seed, so a language can override it for one control.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Map)]
    public partial CornerRadius BorderRadiusMD { get; set; } = new CornerRadius(6);

    /// <summary>
    /// LG号圆角
    /// LG号圆角，用于组件中的一些大圆角，如 Card、Modal 等一些组件样式。
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Map)]
    public partial CornerRadius BorderRadiusLG { get; set; } = new CornerRadius(8);

    /// <summary>
    /// 外部圆角
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Map)]
    public partial CornerRadius BorderRadiusOuter { get; set; } = new CornerRadius(4);

    // Motion
    /// <summary>
    /// 动效播放速度，快速。用于小型元素动画交互
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Map)]
    public partial TimeSpan MotionDurationFast { get; set; }

    /// <summary>
    /// 动效播放速度，中速。用于中型元素动画交互
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Map)]
    public partial TimeSpan MotionDurationMid { get; set; }

    /// <summary>
    /// 动效播放速度，慢速。用于大型元素如面板动画交互
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Map)]
    public partial TimeSpan MotionDurationSlow { get; set; }

    /// <summary>
    /// 动效播放速度，最慢速。用于大型元素如面板动画交互
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Map)]
    public partial TimeSpan MotionDurationVerySlow { get; set; }
}