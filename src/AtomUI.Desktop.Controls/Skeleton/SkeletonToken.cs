using AtomUI.Controls;
using AtomUI.Controls.DesignTokens;
using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

internal static class SkeletonShimmerResources
{
    public static SkeletonShimmerColorTokenResourceKeys ShimmerColorTokenResourceKeys { get; } =
        new(SkeletonTokenKind.GradientFromColor, SkeletonTokenKind.GradientToColor);
}

/// <summary>
/// Design tokens of the desktop skeleton placeholders on top of the shared skeleton tokens: the margins between
/// the avatar, the title and the paragraph of a composite skeleton, the radius of rounded paragraph lines, and
/// the size and icon of an image placeholder.
/// </summary>
[ControlDesignToken]
public sealed partial class SkeletonToken : AbstractSkeletonToken
{
    /// <summary>
    /// Top margin of the paragraph placeholder. The theme applies it only below a title that is shown.
    /// </summary>
    public partial Thickness ParagraphMarginTop { get; set; }
    
    /// <summary>
    /// Margin to the right of the avatar placeholder.
    /// </summary>
    public partial Thickness AvatarMarginRight { get; set; }
    
    /// <summary>
    /// Corner radius of a skeleton paragraph line when rounded ends are enabled.
    /// </summary>
    public partial CornerRadius ParagraphLineRoundCornerRadius { get; set; }
    
    // Background brushes of the shimmer animation, which shifts the background by 400% of the width to sweep a
    // highlight across the block.
    /// <summary>
    /// Reserved background brush for the start of the skeleton shimmer animation.
    /// </summary>
    [ReservedToken("antd 400% background-position shimmer keyframes. The Desktop theme draws the shimmer from " +
                   "GradientFromColor / GradientToColor (ShimmerFromColor / ShimmerToColor) and does not read these brushes.")]
    public partial IBrush? LoadingBackgroundStart { get; set; }

    /// <summary>
    /// Reserved background brush for the midpoint of the skeleton shimmer animation.
    /// </summary>
    [ReservedToken("antd 400% background-position shimmer keyframes. The Desktop theme draws the shimmer from " +
                   "GradientFromColor / GradientToColor (ShimmerFromColor / ShimmerToColor) and does not read these brushes.")]
    public partial IBrush? LoadingBackgroundMiddle { get; set; }

    /// <summary>
    /// Reserved background brush for the end of the skeleton shimmer animation.
    /// </summary>
    [ReservedToken("antd 400% background-position shimmer keyframes. The Desktop theme draws the shimmer from " +
                   "GradientFromColor / GradientToColor (ShimmerFromColor / ShimmerToColor) and does not read these brushes.")]
    public partial IBrush? LoadingBackgroundEnd { get; set; }
    
    /// <summary>
    /// Fill color of the icon in the middle of an image placeholder. AntDesign uses a fixed #bfbfbf.
    /// </summary>
    public partial Color ImageColor { get; set; }

    /// <summary>
    /// Width and height of the icon inside an image placeholder.
    /// </summary>
    public partial double ImageSize { get; set; }
    /// <summary>
    /// Default width and height of the skeleton image container.
    /// </summary>
    public partial double ImageContainerSize {  get; set; }
    /// <summary>
    /// Maximum width and height of the skeleton image container.
    /// </summary>
    public partial double ImageContainerMaxSize { get; set; }
}
