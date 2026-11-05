using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken(ControlTokenEvaluationVersion.DependencyDefaultsV2, Revision = 1)]
internal sealed partial class UploadToken : AbstractControlDesignToken, IControlTokenDefaults
{
    /// <summary>
    /// 操作按扭颜色
    /// Action button color
    /// </summary>
    public Color ActionsColor { get; set; }

    /// <summary>
    /// 卡片类型文件列表项的尺寸（对 picture-card 和 picture-circle 生效）
    ///
    /// </summary>
    public double PictureCardSize { get; set; }

    /// <summary>
    /// 文本列表的外间距
    /// </summary>
    public Thickness TextListItemMargin { get; set; }

    /// <summary>
    /// 文本列表文件名的内间距
    /// </summary>
    public Thickness TextListNamePadding { get; set; }

    /// <summary>
    /// 文本列表项的内间距（垂直方向）
    /// </summary>
    public Thickness TextListItemPadding { get; set; }

    /// <summary>
    /// 文本列表上传进度线的高度
    /// </summary>
    public double TextListProgressLineHeight { get; set; }

    /// <summary>
    /// 文本列表上传进度的内间距
    /// </summary>
    public Thickness TextListProgressPadding { get; set; }

    /// <summary>
    /// 文件类型图标大小
    /// </summary>
    public double UploadThumbnailSize { get; set; }

    /// <summary>
    /// 拖动上传区域图标大小
    /// </summary>
    public double DragIconSize { get; set; }

    /// <summary>
    /// 拖动上传区域图标外间距
    /// </summary>
    public Thickness DragIconMargin { get; set; }

    /// <summary>
    /// 拖动上传区域主标题的外间距
    /// </summary>
    public Thickness DragHeaderMargin { get; set; }

    /// <summary>
    /// 图片列表的外间距
    /// </summary>
    public Thickness PictureListItemMargin { get; set; }

    /// <summary>
    /// 图片列表图片预览大小
    /// </summary>
    public double PictureListPreviewerSize { get; set; }

    [ControlTokenDefault(nameof(ActionsColor))]
    private static Color DefaultForActionsColor(in ControlTokenDefaultContext context) =>
        context.Global.ColorIcon;

    [ControlTokenDefault(nameof(PictureCardSize))]
    private static double DefaultForPictureCardSize(in ControlTokenDefaultContext context) =>
        context.Global.ControlHeightLG * 2.55;

    [ControlTokenDefault(nameof(TextListItemMargin))]
    private static Thickness DefaultForTextListItemMargin(in ControlTokenDefaultContext context) =>
        new Thickness(0, context.Global.UniformlyMarginXS, 0, 0);

    [ControlTokenDefault(nameof(TextListNamePadding))]
    private static Thickness DefaultForTextListNamePadding(in ControlTokenDefaultContext context) =>
        new Thickness(context.Global.UniformlyPaddingXS, 0);

    [ControlTokenDefault(nameof(TextListItemPadding))]
    private static Thickness DefaultForTextListItemPadding(in ControlTokenDefaultContext context) =>
        new Thickness(0, context.Global.UniformlyPaddingXXS, 0, context.Global.UniformlyPaddingXXS);

    [ControlTokenDefault(nameof(TextListProgressLineHeight))]
    private static double DefaultForTextListProgressLineHeight(in ControlTokenDefaultContext context) =>
        2;

    [ControlTokenDefault(nameof(TextListProgressPadding))]
    private static Thickness DefaultForTextListProgressPadding(in ControlTokenDefaultContext context) =>
        new Thickness(context.Global.FontSize + context.Global.UniformlyPaddingXS, 0, 0, 0);

    [ControlTokenDefault(nameof(UploadThumbnailSize))]
    private static double DefaultForUploadThumbnailSize(in ControlTokenDefaultContext context) =>
        context.Global.FontSizeHeading2;

    [ControlTokenDefault(nameof(DragIconSize))]
    private static double DefaultForDragIconSize(in ControlTokenDefaultContext context) =>
        context.Global.FontSizeHeading3 * 2;

    [ControlTokenDefault(nameof(DragIconMargin))]
    private static Thickness DefaultForDragIconMargin(in ControlTokenDefaultContext context) =>
        new Thickness(0, 0, 0, context.Global.UniformlyMargin);

    [ControlTokenDefault(nameof(DragHeaderMargin))]
    private static Thickness DefaultForDragHeaderMargin(in ControlTokenDefaultContext context) =>
        new Thickness(0, 0, 0, context.Global.UniformlyMarginXXS);

    [ControlTokenDefault(nameof(PictureListItemMargin))]
    private static Thickness DefaultForPictureListItemMargin(in ControlTokenDefaultContext context) =>
        new Thickness(0, context.Global.UniformlyMarginXS, 0, 0);

    [ControlTokenDefault(nameof(PictureListPreviewerSize))]
    private static double DefaultForPictureListPreviewerSize(in ControlTokenDefaultContext context) =>
        context.Global.SizeXXL;

}
