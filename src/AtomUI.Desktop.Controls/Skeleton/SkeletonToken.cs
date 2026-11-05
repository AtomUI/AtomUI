using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken(ControlTokenEvaluationVersion.DependencyDefaultsV2, Revision = 1)]
internal sealed partial class SkeletonToken : AbstractControlDesignToken, IControlTokenDefaults
{
    
    /// <summary>
    /// 渐变色起点颜色
    /// Start color of gradient
    /// </summary>
    public Color GradientFromColor { get; set; }
    
    /// <summary>
    /// 渐变色终点颜色
    /// End color of gradient
    /// </summary>
    public Color GradientToColor { get; set; }
    
    /// <summary>
    /// 标题骨架屏高度
    /// Height of title skeleton
    /// </summary>
    public double TitleHeight { get; set; }
    
    /// <summary>
    /// 骨架屏圆角
    /// Border radius of skeleton
    /// </summary>
    public CornerRadius BlockRadius { get; set; }
    
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
    /// 段落骨架屏单行高度
    /// Line height of paragraph skeleton
    /// </summary>
    public double ParagraphLineHeight { get; set; }
    
    /// <summary>
    /// 段落骨架屏单行胶囊圆角值
    /// </summary>
    public CornerRadius ParagraphLineRoundCornerRadius { get; set; }
    
    /// <summary>
    /// 默认加载的时长
    /// </summary>
    public TimeSpan LoadingMotionDuration { get; set; }
    
    // 流光动画背景定义，背景按 400% 宽度平移形成扫光效果。
    public IBrush? LoadingBackgroundStart { get; set; }
    public IBrush? LoadingBackgroundMiddle { get; set; }
    public IBrush? LoadingBackgroundEnd { get; set; }
    
    public double ImageSize { get; set; }
    public double ImageContainerSize {  get; set; }
    public double ImageContainerMaxSize { get; set; }

    [ControlTokenDefault(nameof(GradientFromColor))]
    private static Color DefaultForGradientFromColor(in ControlTokenDefaultContext context) =>
        context.Global.ColorFillContent;

    [ControlTokenDefault(nameof(GradientToColor))]
    private static Color DefaultForGradientToColor(in ControlTokenDefaultContext context) =>
        context.Global.ColorFill;

    [ControlTokenDefault(nameof(TitleHeight))]
    private static double DefaultForTitleHeight(in ControlTokenDefaultContext context) =>
        context.Global.ControlHeight / 2;

    [ControlTokenDefault(nameof(BlockRadius))]
    private static CornerRadius DefaultForBlockRadius(in ControlTokenDefaultContext context) =>
        context.Global.BorderRadiusSM;

    [ControlTokenDefault(nameof(ParagraphMarginTop))]
    private static Thickness DefaultForParagraphMarginTop(in ControlTokenDefaultContext context) =>
        new Thickness(0, context.Global.UniformlyMarginLG + context.Global.UniformlyMarginXXS, 0, 0);

    [ControlTokenDefault(nameof(AvatarMarginRight))]
    private static Thickness DefaultForAvatarMarginRight(in ControlTokenDefaultContext context) =>
        new Thickness(0, 0, context.Global.UniformlyMargin, 0);

    [ControlTokenDefault(nameof(ParagraphLineHeight))]
    private static double DefaultForParagraphLineHeight(in ControlTokenDefaultContext context) =>
        context.Global.ControlHeight / 2;

    [ControlTokenDefault(nameof(ParagraphLineRoundCornerRadius), nameof(ParagraphLineHeight))]
    private static CornerRadius DefaultForParagraphLineRoundCornerRadius(in ControlTokenDefaultContext context, double paragraphLineHeight) =>
        new CornerRadius(paragraphLineHeight / 2);

    [ControlTokenDefault(nameof(LoadingMotionDuration))]
    private static TimeSpan DefaultForLoadingMotionDuration(in ControlTokenDefaultContext context) =>
        TimeSpan.FromSeconds(1.4);

    [ControlTokenDefault(nameof(LoadingBackgroundStart), nameof(GradientFromColor), nameof(GradientToColor))]
    private static IBrush? DefaultForLoadingBackgroundStart(in ControlTokenDefaultContext context, Color gradientFromColor, Color gradientToColor) =>
        CreateLoadingBackground(-3.0, 1.0, gradientFromColor, gradientToColor);

    [ControlTokenDefault(nameof(LoadingBackgroundMiddle), nameof(GradientFromColor), nameof(GradientToColor))]
    private static IBrush? DefaultForLoadingBackgroundMiddle(in ControlTokenDefaultContext context, Color gradientFromColor, Color gradientToColor) =>
        CreateLoadingBackground(-1.5, 2.5, gradientFromColor, gradientToColor);

    [ControlTokenDefault(nameof(LoadingBackgroundEnd), nameof(GradientFromColor), nameof(GradientToColor))]
    private static IBrush? DefaultForLoadingBackgroundEnd(in ControlTokenDefaultContext context, Color gradientFromColor, Color gradientToColor) =>
        CreateLoadingBackground(0.0, 4.0, gradientFromColor, gradientToColor);

    [ControlTokenDefault(nameof(ImageSize))]
    private static double DefaultForImageSize(in ControlTokenDefaultContext context) =>
        context.Global.ControlHeight * 1.5;

    [ControlTokenDefault(nameof(ImageContainerSize), nameof(ImageSize))]
    private static double DefaultForImageContainerSize(in ControlTokenDefaultContext context, double imageSize) =>
        imageSize * 2;

    [ControlTokenDefault(nameof(ImageContainerMaxSize), nameof(ImageSize))]
    private static double DefaultForImageContainerMaxSize(in ControlTokenDefaultContext context, double imageSize) =>
        imageSize * 4;

    private static LinearGradientBrush CreateLoadingBackground(double startX, double endX, Color fromColor, Color toColor)
    {
        return new LinearGradientBrush
        {
            StartPoint = new RelativePoint(startX, 0.5, RelativeUnit.Relative),
            EndPoint = new RelativePoint(endX, 0.5, RelativeUnit.Relative),
            GradientStops =
            {
                new GradientStop { Offset = 0.25, Color = fromColor },
                new GradientStop { Offset = 0.37, Color = toColor },
                new GradientStop { Offset = 0.63, Color = fromColor }
            }
        };
    }
}
