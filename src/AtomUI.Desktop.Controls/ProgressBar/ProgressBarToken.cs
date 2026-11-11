using AtomUI.Theme.DesignTokens;
using Avalonia;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
internal sealed class ProgressBarToken : AbstractLineProgressToken
{
    /// <summary>
    /// 条状进度条圆角
    /// Corner radius of the rounded (capsule) line progress bar
    /// </summary>
    public CornerRadius LineBorderRadius { get; set; }

    /// <summary>
    /// 内部百分比模式下进度条的内间距
    /// Padding of the bar around an inner percentage label
    /// </summary>
    public double LineProgressPadding { get; set; }

    /// <summary>
    /// Active 状态轨道闪光动画的时长（antd progressActiveMotionDuration，2.4s）
    /// Duration of the Active status shimmer over the track (antd progressActiveMotionDuration, 2.4s)
    /// </summary>
    [ReservedToken("antd Progress progressActiveMotionDuration. The Active status shimmer animation over the track " +
                   "is not implemented yet; Active renders like Normal.")]
    public TimeSpan ProgressActiveMotionDuration { get; set; }

    public override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        LineBorderRadius = new CornerRadius(100); // magic for capsule shape, should be a very large number
        LineProgressPadding = EffectiveGlobalToken.UniformlyPaddingXXS / 2;
        ProgressActiveMotionDuration = TimeSpan.FromSeconds(2.4); // antd progressActiveMotionDuration: '2.4s'
    }
}
