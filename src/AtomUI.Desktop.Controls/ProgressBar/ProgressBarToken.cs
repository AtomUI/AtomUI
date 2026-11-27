using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Own Tokens of the <see cref="ProgressBar" /> control: the stroke thickness of each size preset, the corner
/// radius of the bar, the padding around an inner label, the color and opacity of that label, and the reserved
/// duration of the Active status animation.
/// </summary>
[ControlDesignToken]
public sealed partial class ProgressBarToken : AbstractLineProgressToken
{
    /// <summary>
    /// Line stroke thickness of the large preset.
    /// </summary>
    public partial double LineStrokeThicknessLG { get; set; }

    /// <summary>
    /// Line stroke thickness of the middle preset.
    /// </summary>
    public partial double LineStrokeThickness { get; set; }

    /// <summary>
    /// Line stroke thickness of the small preset.
    /// </summary>
    public partial double LineStrokeThicknessSM { get; set; }

    /// <summary>
    /// Corner radius of the rounded (capsule) line progress bar. Applied to the rail, the track and the success bar
    /// only while <c>StrokeLineCap</c> is <c>Round</c>.
    /// </summary>
    public partial CornerRadius LineBorderRadius { get; set; }

    /// <summary>
    /// Padding of the bar around an inner percentage label.
    /// </summary>
    public partial double LineProgressPadding { get; set; }

    /// <summary>
    /// Text color of an inner percentage label where it reads better than <c>ColorTextLightSolid</c> on the bar
    /// (the label takes the more readable of the two); defaults to <c>ColorTextLabel</c>.
    /// </summary>
    public partial Color InnerLabelColor { get; set; }

    /// <summary>
    /// Opacity of an inner percentage label while the value is 0, where the label sits on the empty track instead of
    /// the bar. Defaults to 1 (shown); 0 hides it, as PrimeNG does.
    /// </summary>
    public partial double InnerLabelEmptyOpacity { get; set; }

    /// <summary>
    /// Duration of the shimmer that the Active status moves over the track. Reserved: the animation is not
    /// implemented, so the value has no effect and Active renders like Normal.
    /// </summary>
    [ReservedToken("antd Progress progressActiveMotionDuration. The Active status shimmer animation over the track " +
                   "is not implemented yet; Active renders like Normal.")]
    public partial TimeSpan ProgressActiveMotionDuration { get; set; }
}
