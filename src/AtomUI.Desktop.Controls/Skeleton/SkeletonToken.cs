using AtomUI.Controls.DesignTokens;
using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
internal sealed class SkeletonToken : AbstractSkeletonToken
{
    /// <summary>
    /// 段落骨架屏上间距
    /// Margin top of paragraph skeleton
    /// </summary>
    public Thickness ParagraphMarginTop { get; set; }
    
    /// <summary>
    /// 头像骨架屏右间距
    /// </summary>
    public Thickness AvatarMarginRight { get; set; }
    
    /// <summary>
    /// 段落骨架屏单行胶囊圆角值
    /// </summary>
    public CornerRadius ParagraphLineRoundCornerRadius { get; set; }
    
    // 流光动画背景定义，背景按 400% 宽度平移形成扫光效果。
    public IBrush? LoadingBackgroundStart { get; set; }
    public IBrush? LoadingBackgroundMiddle { get; set; }
    public IBrush? LoadingBackgroundEnd { get; set; }
    
    public double ImageSize { get; set; }
    public double ImageContainerSize {  get; set; }
    public double ImageContainerMaxSize { get; set; }

    public SkeletonToken()

    {
    }

    public override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        ParagraphMarginTop    = new Thickness(0, EffectiveGlobalToken.UniformlyMarginLG + EffectiveGlobalToken.UniformlyMarginXXS, 0, 0);
        AvatarMarginRight = new Thickness(0, 0, EffectiveGlobalToken.UniformlyMargin, 0);
        ParagraphLineRoundCornerRadius = new CornerRadius(ParagraphLineHeight / 2);
        LoadingBackgroundStart  = CreateLoadingBackground(-3.0, 1.0);
        LoadingBackgroundMiddle = CreateLoadingBackground(-1.5, 2.5);
        LoadingBackgroundEnd    = CreateLoadingBackground(0.0, 4.0);

        var imageSizeBase = EffectiveGlobalToken.ControlHeight * 1.5;
        ImageSize             = imageSizeBase;
        ImageContainerSize    = imageSizeBase * 2;
        ImageContainerMaxSize = imageSizeBase * 4;
    }


    private LinearGradientBrush CreateLoadingBackground(double startX, double endX)
    {
        return new LinearGradientBrush
        {
            StartPoint = new RelativePoint(startX, 0.5, RelativeUnit.Relative),
            EndPoint   = new RelativePoint(endX, 0.5, RelativeUnit.Relative),
            GradientStops =
            {
                new GradientStop { Offset = 0.25, Color = GradientFromColor },
                new GradientStop { Offset = 0.37, Color = GradientToColor },
                new GradientStop { Offset = 0.63, Color = GradientFromColor }
            }
        };
    }
}
