using AtomUI.Media;
using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken(ControlTokenEvaluationVersion.DependencyDefaultsV2, Revision = 1)]
internal sealed partial class ToggleSwitchToken : AbstractControlDesignToken, IControlTokenDefaults
{
    
/// <summary>
    /// 开关高度
    /// </summary>
    public double TrackHeight { get; set; }

    /// <summary>
    /// 小号开关高度
    /// </summary>
    public double TrackHeightSM { get; set; }

    /// <summary>
    /// 开关最小宽度
    /// </summary>
    public double TrackMinWidth { get; set; }

    /// <summary>
    /// 小号开关最小宽度
    /// </summary>
    public double TrackMinWidthSM { get; set; }

    /// <summary>
    /// 开关内边距
    /// </summary>
    public double TrackPadding { get; set; }

    /// <summary>
    /// 开关把手背景色
    /// </summary>
    public Color HandleBg { get; set; }

    /// <summary>
    /// 开关把手阴影
    /// </summary>
    public BoxShadow HandleShadow { get; set; }

    /// <summary>
    /// 开关把手大小
    /// </summary>
    public Size HandleSize { get; set; }

    /// <summary>
    /// 小号开关把手大小
    /// </summary>
    public Size HandleSizeSM { get; set; }

    /// <summary>
    /// 内容区域最小边距
    /// </summary>
    public double InnerMinMargin { get; set; }

    /// <summary>
    /// 内容区域最大边距
    /// </summary>
    public double InnerMaxMargin { get; set; }

    /// <summary>
    /// 小号开关内容区域最小边距
    /// </summary>
    public double InnerMinMarginSM { get; set; }

    /// <summary>
    /// 小号开关内容区域最大边距
    /// </summary>
    public double InnerMaxMarginSM { get; set; }

    /// <summary>
    /// 正常状态的图标大小
    /// </summary>
    public double ContentIconSize { get; set; }

    /// <summary>
    /// 小号状态的图标大小
    /// </summary>
    public double ContentIconSizeSM { get; set; }

    /// <summary>
    /// 开关的颜色
    /// </summary>
    public Color SwitchColor { get; set; }
    
    /// <summary>
    /// 开源禁用状态的透明度
    /// </summary>
    public double SwitchDisabledOpacity { get; set; }
    
    /// <summary>
    /// 开关空白区域的字体大小，正常尺寸
    /// </summary>
    public double ExtraInfoFontSize { get; set; }
    
    /// <summary>
    /// 开关空白区域的字体大小，小尺寸
    /// </summary>
    public double ExtraInfoFontSizeSM { get; set; }
    
    /// <summary>
    /// 加载动画的周期时长
    /// </summary>
    public TimeSpan LoadingAnimationDuration { get; set; }
    
    /// <summary>
    /// 在关状态下的加载指示器的颜色
    /// </summary>
    public Color OffStateLoadIndicatorColor { get; set; }

    [ControlTokenDefault(nameof(TrackHeight))]
    private static double DefaultForTrackHeight(in ControlTokenDefaultContext context) =>
        context.Global.FontSize * context.Global.RelativeLineHeight;

    [ControlTokenDefault(nameof(TrackHeightSM))]
    private static double DefaultForTrackHeightSM(in ControlTokenDefaultContext context) =>
        context.Global.ControlHeight / 2;

    [ControlTokenDefault(nameof(TrackPadding))]
    private static double DefaultForTrackPadding(in ControlTokenDefaultContext context) =>
        2;

    [ControlTokenDefault(nameof(TrackMinWidth), nameof(HandleSize), nameof(TrackPadding))]
    private static double DefaultForTrackMinWidth(in ControlTokenDefaultContext context, Size handleSize, double trackPadding) =>
        handleSize.Width * 2 + trackPadding * 4;

    [ControlTokenDefault(nameof(TrackMinWidthSM), nameof(HandleSizeSM), nameof(TrackPadding))]
    private static double DefaultForTrackMinWidthSM(in ControlTokenDefaultContext context, Size handleSizeSM, double trackPadding) =>
        handleSizeSM.Width * 2 + trackPadding * 2;

    [ControlTokenDefault(nameof(HandleBg))]
    private static Color DefaultForHandleBg(in ControlTokenDefaultContext context) =>
        context.Global.ColorWhite;

    [ControlTokenDefault(nameof(HandleSize), nameof(TrackHeight), nameof(TrackPadding))]
    private static Size DefaultForHandleSize(in ControlTokenDefaultContext context, double trackHeight, double trackPadding) =>
        new Size(trackHeight - trackPadding * 2, trackHeight - trackPadding * 2);

    [ControlTokenDefault(nameof(HandleSizeSM), nameof(TrackHeightSM), nameof(TrackPadding))]
    private static Size DefaultForHandleSizeSM(in ControlTokenDefaultContext context, double trackHeightSM, double trackPadding) =>
        new Size(trackHeightSM - trackPadding * 2, trackHeightSM - trackPadding * 2);

    [ControlTokenDefault(nameof(InnerMinMargin), nameof(HandleSize))]
    private static double DefaultForInnerMinMargin(in ControlTokenDefaultContext context, Size handleSize) =>
        handleSize.Width / 2;

    [ControlTokenDefault(nameof(InnerMaxMargin), nameof(HandleSize), nameof(TrackPadding))]
    private static double DefaultForInnerMaxMargin(in ControlTokenDefaultContext context, Size handleSize, double trackPadding) =>
        handleSize.Width + trackPadding * 3;

    [ControlTokenDefault(nameof(InnerMinMarginSM), nameof(HandleSizeSM), nameof(TrackPadding))]
    private static double DefaultForInnerMinMarginSM(in ControlTokenDefaultContext context, Size handleSizeSM, double trackPadding) =>
        handleSizeSM.Width / 2 - trackPadding;

    [ControlTokenDefault(nameof(InnerMaxMarginSM), nameof(HandleSizeSM), nameof(TrackPadding))]
    private static double DefaultForInnerMaxMarginSM(in ControlTokenDefaultContext context, Size handleSizeSM, double trackPadding) =>
        handleSizeSM.Width + trackPadding * 3;

    [ControlTokenDefault(nameof(SwitchColor))]
    private static Color DefaultForSwitchColor(in ControlTokenDefaultContext context) =>
        context.Global.ColorPrimary;

    [ControlTokenDefault(nameof(SwitchDisabledOpacity))]
    private static double DefaultForSwitchDisabledOpacity(in ControlTokenDefaultContext context) =>
        context.Global.OpacityLoading;

    [ControlTokenDefault(nameof(ExtraInfoFontSize))]
    private static double DefaultForExtraInfoFontSize(in ControlTokenDefaultContext context) =>
        context.Global.FontSizeSM;

    [ControlTokenDefault(nameof(ExtraInfoFontSizeSM), nameof(ExtraInfoFontSize))]
    private static double DefaultForExtraInfoFontSizeSM(in ControlTokenDefaultContext context, double extraInfoFontSize) =>
        extraInfoFontSize - 1;

    [ControlTokenDefault(nameof(HandleShadow))]
    private static BoxShadow DefaultForHandleShadow(in ControlTokenDefaultContext context) =>
        new BoxShadow { OffsetX = 0, OffsetY = 2, Blur = 4, Color = Color.FromArgb((int)(255 * 0.2), 0, 35, 11) };

    [ControlTokenDefault(nameof(ContentIconSize), nameof(TrackHeightSM))]
    private static double DefaultForContentIconSize(in ControlTokenDefaultContext context, double trackHeightSM) =>
        trackHeightSM;

    [ControlTokenDefault(nameof(ContentIconSizeSM), nameof(TrackHeightSM))]
    private static double DefaultForContentIconSizeSM(in ControlTokenDefaultContext context, double trackHeightSM) =>
        trackHeightSM - context.Global.UniformlyPaddingXXS;

    [ControlTokenDefault(nameof(LoadingAnimationDuration))]
    private static TimeSpan DefaultForLoadingAnimationDuration(in ControlTokenDefaultContext context) =>
        TimeSpan.FromMilliseconds(1200);

    [ControlTokenDefault(nameof(OffStateLoadIndicatorColor))]
    private static Color DefaultForOffStateLoadIndicatorColor(in ControlTokenDefaultContext context) =>
        ColorUtils.FromRgbF(0.4, 0.0, 0.0, 0.0);

}
