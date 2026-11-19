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
    
    /// <summary>
    /// 头像骨架屏右间距
    /// </summary>
    public partial Thickness AvatarMarginRight { get; set; }
    
    /// <summary>
    /// 段落骨架屏单行胶囊圆角值
    /// </summary>
    public partial CornerRadius ParagraphLineRoundCornerRadius { get; set; }
    
    // 流光动画背景定义，背景按 400% 宽度平移形成扫光效果。
    [ReservedToken("antd 400% background-position shimmer keyframes. The Desktop theme draws the shimmer from " +
                   "GradientFromColor / GradientToColor (ShimmerFromColor / ShimmerToColor) and does not read these brushes.")]
    public partial IBrush? LoadingBackgroundStart { get; set; }

    [ReservedToken("antd 400% background-position shimmer keyframes. The Desktop theme draws the shimmer from " +
                   "GradientFromColor / GradientToColor (ShimmerFromColor / ShimmerToColor) and does not read these brushes.")]
    public partial IBrush? LoadingBackgroundMiddle { get; set; }

    [ReservedToken("antd 400% background-position shimmer keyframes. The Desktop theme draws the shimmer from " +
                   "GradientFromColor / GradientToColor (ShimmerFromColor / ShimmerToColor) and does not read these brushes.")]
    public partial IBrush? LoadingBackgroundEnd { get; set; }
    
    /// <summary>
    /// 图片占位图标颜色（antd 固定 #bfbfbf）
    /// Fill color of the image placeholder icon (antd fixes #bfbfbf)
    /// </summary>
    public partial Color ImageColor { get; set; }

    public partial double ImageSize { get; set; }
    public partial double ImageContainerSize {  get; set; }
    public partial double ImageContainerMaxSize { get; set; }

    public SkeletonToken()

    {
    }

    protected override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        // The gradient colors and the paragraph line height feed the derived values below, so their defaults yield to
        // explicit overrides; AbstractSkeletonToken leaves these three to the terminal for that reason.
        if (!IsExplicit(SkeletonTokenKind.GradientFromColor))
        {
            GradientFromColor = EffectiveGlobalToken.ColorFillContent;
        }
        if (!IsExplicit(SkeletonTokenKind.GradientToColor))
        {
            GradientToColor = EffectiveGlobalToken.ColorFill;
        }
        if (!IsExplicit(SkeletonTokenKind.ParagraphLineHeight))
        {
            ParagraphLineHeight = EffectiveGlobalToken.ControlHeight / 2;
        }

        ParagraphMarginTop    = new Thickness(0, EffectiveGlobalToken.UniformlyMarginLG + EffectiveGlobalToken.UniformlyMarginXXS, 0, 0);
        AvatarMarginRight = new Thickness(0, 0, EffectiveGlobalToken.UniformlyMargin, 0);
        ParagraphLineRoundCornerRadius = new CornerRadius(ParagraphLineHeight / 2);
        LoadingBackgroundStart  = CreateLoadingBackground(-3.0, 1.0, GradientFromColor, GradientToColor);
        LoadingBackgroundMiddle = CreateLoadingBackground(-1.5, 2.5, GradientFromColor, GradientToColor);
        LoadingBackgroundEnd    = CreateLoadingBackground(0.0, 4.0, GradientFromColor, GradientToColor);
        ImageColor              = Color.FromRgb(0xbf, 0xbf, 0xbf);

        var imageSizeBase = EffectiveGlobalToken.ControlHeight * 1.5;
        ImageSize             = imageSizeBase;
        ImageContainerSize    = imageSizeBase * 2;
        ImageContainerMaxSize = imageSizeBase * 4;
    }


    private static LinearGradientBrush CreateLoadingBackground(double startX, double endX, Color fromColor, Color toColor)
    {
        return new LinearGradientBrush
        {
            StartPoint = new RelativePoint(startX, 0.5, RelativeUnit.Relative),
            EndPoint   = new RelativePoint(endX, 0.5, RelativeUnit.Relative),
            GradientStops =
            {
                new GradientStop { Offset = 0.25, Color = fromColor },
                new GradientStop { Offset = 0.37, Color = toColor },
                new GradientStop { Offset = 0.63, Color = fromColor }
            }
        };
    }
}
