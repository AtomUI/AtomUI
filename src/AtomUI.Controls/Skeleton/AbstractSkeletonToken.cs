using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Controls.DesignTokens;

/// <summary>
/// Skeleton tokens shared by the design languages: the colors and timing of the shimmer, the corner radius of
/// the placeholder blocks and the heights of the title and paragraph bars.
/// </summary>
[ControlDesignToken]
public abstract partial class AbstractSkeletonToken : AbstractControlDesignToken
{
    /// <summary>
    /// Base color of the skeleton: the fill of every placeholder block and the color of the band outside its
    /// highlight.
    /// </summary>
    public partial Color GradientFromColor { get; set; }

    /// <summary>
    /// Highlight color of the band that sweeps the placeholder blocks.
    /// </summary>
    public partial Color GradientToColor { get; set; }

    /// <summary>
    /// Duration of one cycle of the animated skeleton highlight.
    /// </summary>
    public partial TimeSpan LoadingMotionDuration { get; set; }

    /// <summary>
    /// Corner radius of the skeleton placeholder shapes.
    /// </summary>
    public partial CornerRadius BlockRadius { get; set; }

    /// <summary>
    /// Height of the title placeholder bar.
    /// </summary>
    public partial double TitleHeight { get; set; }

    /// <summary>
    /// Height of each paragraph placeholder bar.
    /// </summary>
    public partial double ParagraphLineHeight { get; set; }
}
