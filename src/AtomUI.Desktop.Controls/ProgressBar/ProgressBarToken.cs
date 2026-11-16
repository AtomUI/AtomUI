using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
public sealed partial class ProgressBarToken : AbstractLineProgressToken
{
    /// <summary>
    /// 条状进度条圆角
    /// Corner radius of the rounded (capsule) line progress bar
    /// </summary>
    public partial CornerRadius LineBorderRadius { get; set; }

    /// <summary>
    /// 内部百分比模式下进度条的内间距
    /// Padding of the bar around an inner percentage label
    /// </summary>
    public partial double LineProgressPadding { get; set; }

    /// <summary>
    /// Text colour of an inner percentage label where it reads better than <c>ColorTextLightSolid</c> on the bar
    /// (the label takes the more readable of the two); defaults to <c>ColorTextLabel</c>.
    /// </summary>
    public partial Color InnerLabelColor { get; set; }

    /// <summary>
    /// Opacity of an inner percentage label while the value is 0, where the label sits on the empty track instead of
    /// the bar. Defaults to 1 (shown); 0 hides it, as PrimeNG does.
    /// </summary>
    public partial double InnerLabelEmptyOpacity { get; set; }

    /// <summary>
    /// Active 状态轨道闪光动画的时长（antd progressActiveMotionDuration，2.4s）
    /// Duration of the Active status shimmer over the track (antd progressActiveMotionDuration, 2.4s)
    /// </summary>
    [ReservedToken("antd Progress progressActiveMotionDuration. The Active status shimmer animation over the track " +
                   "is not implemented yet; Active renders like Normal.")]
    public partial TimeSpan ProgressActiveMotionDuration { get; set; }

    protected override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        LineBorderRadius = new CornerRadius(100); // magic for capsule shape, should be a very large number
        LineProgressPadding = EffectiveGlobalToken.UniformlyPaddingXXS / 2;
        InnerLabelColor = EffectiveGlobalToken.ColorTextLabel;
        InnerLabelEmptyOpacity = 1;
        ProgressActiveMotionDuration = TimeSpan.FromSeconds(2.4); // antd progressActiveMotionDuration: '2.4s'
    }
}
