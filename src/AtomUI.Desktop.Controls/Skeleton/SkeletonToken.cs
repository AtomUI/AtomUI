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

[ControlDesignToken]
public sealed partial class SkeletonToken : AbstractSkeletonToken
{
    /// <summary>
    /// 段落骨架屏上间距
    /// Margin top of paragraph skeleton
    /// </summary>
    public partial Thickness ParagraphMarginTop { get; set; }
    
    /// <summary>Margin to the right of the avatar placeholder.</summary>
    public partial Thickness AvatarMarginRight { get; set; }
    
    /// <summary>Corner radius of a skeleton paragraph line when rounded ends are enabled.</summary>
    public partial CornerRadius ParagraphLineRoundCornerRadius { get; set; }
    
    // 流光动画背景定义，背景按 400% 宽度平移形成扫光效果。
    /// <summary>Reserved background brush for the start of the skeleton shimmer animation.</summary>
    [ReservedToken("antd 400% background-position shimmer keyframes. The Desktop theme draws the shimmer from " +
                   "GradientFromColor / GradientToColor (ShimmerFromColor / ShimmerToColor) and does not read these brushes.")]
    public partial IBrush? LoadingBackgroundStart { get; set; }

    /// <summary>Reserved background brush for the midpoint of the skeleton shimmer animation.</summary>
    [ReservedToken("antd 400% background-position shimmer keyframes. The Desktop theme draws the shimmer from " +
                   "GradientFromColor / GradientToColor (ShimmerFromColor / ShimmerToColor) and does not read these brushes.")]
    public partial IBrush? LoadingBackgroundMiddle { get; set; }

    /// <summary>Reserved background brush for the end of the skeleton shimmer animation.</summary>
    [ReservedToken("antd 400% background-position shimmer keyframes. The Desktop theme draws the shimmer from " +
                   "GradientFromColor / GradientToColor (ShimmerFromColor / ShimmerToColor) and does not read these brushes.")]
    public partial IBrush? LoadingBackgroundEnd { get; set; }
    
    /// <summary>
    /// 图片占位图标颜色（antd 固定 #bfbfbf）
    /// Fill color of the image placeholder icon (antd fixes #bfbfbf)
    /// </summary>
    public partial Color ImageColor { get; set; }

    /// <summary>Width and height of the icon inside an image placeholder.</summary>
    public partial double ImageSize { get; set; }
    /// <summary>Default width and height of the skeleton image container.</summary>
    public partial double ImageContainerSize {  get; set; }
    /// <summary>Maximum width and height of the skeleton image container.</summary>
    public partial double ImageContainerMaxSize { get; set; }
}
