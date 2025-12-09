using AtomUI.Media;
using AtomUI.Theme.TokenSystem;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
internal class ToggleSwitchToken : AbstractControlDesignToken
{
    public const string ID = "ToggleSwitch";

    public ToggleSwitchToken()
        : base(ID)
    {
    }

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
    public double IconSize { get; set; }

    /// <summary>
    /// 小号状态的图标大小
    /// </summary>
    public double IconSizeSM { get; set; }

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

    public override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        var fontSize      = SharedToken.FontSize;
        var lineHeight    = SharedToken.RelativeLineHeight;
        var controlHeight = SharedToken.ControlHeight;

        var    height       = fontSize * lineHeight;
        var    heightSM     = controlHeight / 2;
        double padding      = 2; // Fixed value
        var    handleSize   = height - padding * 2;
        var    handleSizeSM = heightSM - padding * 2;

        TrackHeight     = height;
        TrackHeightSM   = heightSM;
        TrackMinWidth   = handleSize * 2 + padding * 4;
        TrackMinWidthSM = handleSizeSM * 2 + padding * 2;
        TrackPadding    = padding; // Fixed value
        HandleBg        = SharedToken.ColorWhite;
        HandleSize      = new Size(handleSize, handleSize);
        HandleSizeSM    = new Size(handleSizeSM, handleSizeSM);

        InnerMinMargin   = handleSize / 2;
        InnerMaxMargin   = handleSize + padding * 3;
        InnerMinMarginSM = handleSizeSM / 2 - padding;
        InnerMaxMarginSM = handleSizeSM + padding * 3;
        
        SwitchColor            = SharedToken.ColorPrimary;
        SwitchDisabledOpacity  = SharedToken.OpacityLoading;

        ExtraInfoFontSize   = SharedToken.FontSizeSM;
        ExtraInfoFontSizeSM = ExtraInfoFontSize - 1;

        HandleShadow = new BoxShadow
        {
            OffsetX = 0,
            OffsetY = 2,
            Blur    = 4,
            Color   = Color.FromArgb((int)(255 * 0.2), 0, 35, 11)
        };

        IconSize                   = TrackHeightSM;
        IconSizeSM                 = TrackHeightSM - SharedToken.UniformlyPaddingXXS;
        LoadingAnimationDuration   = TimeSpan.FromMilliseconds(1200); // 毫秒
        OffStateLoadIndicatorColor = ColorUtils.FromRgbF(0.4,0.0, 0.0, 0.0);
    }
}