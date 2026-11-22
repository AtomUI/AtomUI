using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Media;
using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
public sealed partial class ToggleSwitchToken : AbstractControlDesignToken
{
    
    public ToggleSwitchToken()

    {
    }

    /// <summary>
    /// 开关高度
    /// </summary>
    public partial double TrackHeight { get; set; }

    /// <summary>
    /// 小号开关高度
    /// </summary>
    public partial double TrackHeightSM { get; set; }

    /// <summary>
    /// 开关最小宽度
    /// </summary>
    public partial double TrackMinWidth { get; set; }

    /// <summary>
    /// 小号开关最小宽度
    /// </summary>
    public partial double TrackMinWidthSM { get; set; }

    /// <summary>
    /// 开关内边距
    /// </summary>
    public partial double TrackPadding { get; set; }

    /// <summary>
    /// 开关把手背景色
    /// </summary>
    public partial Color HandleBg { get; set; }

    /// <summary>
    /// The handle background of a checked switch, drawn on the primary track. Defaults to <see cref="HandleBg"/>; a
    /// theme whose primary is near white sets a dark handle so it stays visible.
    /// </summary>
    public partial Color HandleCheckedBg { get; set; }

    /// <summary>
    /// The handle background of an unchecked switch under the pointer. Defaults to <see cref="HandleBg"/>.
    /// </summary>
    public partial Color HandleHoverBg { get; set; }

    /// <summary>
    /// The handle background of a checked switch under the pointer. Defaults to <see cref="HandleCheckedBg"/>.
    /// </summary>
    public partial Color HandleCheckedHoverBg { get; set; }

    /// <summary>
    /// The track background of an unchecked switch. Defaults to <c>ColorTextQuaternary</c> (Ant Design).
    /// </summary>
    public partial Color TrackBg { get; set; }

    /// <summary>
    /// The track background of an unchecked switch under the pointer. Defaults to <c>ColorTextTertiary</c> (Ant Design).
    /// </summary>
    public partial Color TrackHoverBg { get; set; }

    /// <summary>
    /// The track background of a checked switch under the pointer; the checked track at rest is
    /// <see cref="SwitchColor"/>. Defaults to <c>ColorPrimaryHover</c> (Ant Design).
    /// </summary>
    public partial Color TrackCheckedHoverBg { get; set; }

    /// <summary>
    /// The outline of an unchecked switch's track, drawn inside the track when <see cref="TrackBorderThickness"/> is
    /// not 0. Defaults to transparent.
    /// </summary>
    public partial Color TrackBorderColor { get; set; }

    /// <summary>
    /// The outline of a checked switch's track, drawn inside the track when <see cref="TrackBorderThickness"/> is not
    /// 0. Defaults to transparent.
    /// </summary>
    public partial Color TrackCheckedBorderColor { get; set; }

    /// <summary>
    /// The width of the track outline. Defaults to 0: no outline (Ant Design).
    /// </summary>
    public partial double TrackBorderThickness { get; set; }

    /// <summary>
    /// 开关把手阴影
    /// </summary>
    public partial BoxShadow HandleShadow { get; set; }

    /// <summary>
    /// 开关把手大小
    /// </summary>
    public partial Size HandleSize { get; set; }

    /// <summary>
    /// 小号开关把手大小
    /// </summary>
    public partial Size HandleSizeSM { get; set; }

    /// <summary>
    /// Whether pressing the switch stretches the handle inward and shifts the visible content. Defaults to true.
    /// </summary>
    public partial bool EnableHandlePressStretch { get; set; }

    /// <summary>
    /// 内容区域最小边距
    /// </summary>
    public partial double InnerMinMargin { get; set; }

    /// <summary>
    /// 内容区域最大边距
    /// </summary>
    public partial double InnerMaxMargin { get; set; }

    /// <summary>
    /// 小号开关内容区域最小边距
    /// </summary>
    public partial double InnerMinMarginSM { get; set; }

    /// <summary>
    /// 小号开关内容区域最大边距
    /// </summary>
    public partial double InnerMaxMarginSM { get; set; }

    /// <summary>
    /// 正常状态的图标大小
    /// </summary>
    public partial double ContentIconSize { get; set; }

    /// <summary>
    /// 小号状态的图标大小
    /// </summary>
    public partial double ContentIconSizeSM { get; set; }

    /// <summary>
    /// 开关的颜色
    /// </summary>
    public partial Color SwitchColor { get; set; }

    /// <summary>
    /// 未选中时轨道内文字 / 图标颜色
    /// Colour of the inner text and icon on the unchecked track
    /// </summary>
    public partial Color UncheckedContentColor { get; set; }
    
    /// <summary>
    /// 开源禁用状态的透明度
    /// </summary>
    public partial double SwitchDisabledOpacity { get; set; }
    
    /// <summary>
    /// 开关空白区域的字体大小，正常尺寸
    /// </summary>
    public partial double ExtraInfoFontSize { get; set; }
    
    /// <summary>
    /// 开关空白区域的字体大小，小尺寸
    /// </summary>
    public partial double ExtraInfoFontSizeSM { get; set; }
    
    /// <summary>
    /// 加载动画的周期时长
    /// </summary>
    public partial TimeSpan LoadingAnimationDuration { get; set; }
    
    /// <summary>
    /// 在关状态下的加载指示器的颜色
    /// </summary>
    public partial Color OffStateLoadIndicatorColor { get; set; }

    protected override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        var fontSize      = EffectiveGlobalToken.FontSize;
        var lineHeight    = EffectiveGlobalToken.RelativeLineHeight;
        var controlHeight = EffectiveGlobalToken.ControlHeight;

        var    height       = fontSize * lineHeight;
        var    heightSM     = controlHeight / 2;
        double padding      = 2; // Fixed value
        var    handleSize   = height - padding * 2;
        var    handleSizeSM = heightSM - padding * 2;

        TrackHeight     = height;

        if (!IsExplicit(ToggleSwitchTokenKind.TrackHeightSM))
        {
            TrackHeightSM   = heightSM;
        }

        TrackMinWidth   = handleSize * 2 + padding * 4;
        TrackMinWidthSM = handleSizeSM * 2 + padding * 2;
        TrackPadding    = padding; // Fixed value
        if (!IsExplicit(ToggleSwitchTokenKind.HandleBg))
        {
            HandleBg = EffectiveGlobalToken.ColorWhite;
        }
        // The checked and hover handles follow the handle background, an explicit one included.
        if (!IsExplicit(ToggleSwitchTokenKind.HandleCheckedBg))
        {
            HandleCheckedBg = HandleBg;
        }
        if (!IsExplicit(ToggleSwitchTokenKind.HandleHoverBg))
        {
            HandleHoverBg = HandleBg;
        }
        if (!IsExplicit(ToggleSwitchTokenKind.HandleCheckedHoverBg))
        {
            HandleCheckedHoverBg = HandleCheckedBg;
        }

        TrackBg                 = EffectiveGlobalToken.ColorTextQuaternary;
        TrackHoverBg            = EffectiveGlobalToken.ColorTextTertiary;
        TrackCheckedHoverBg     = EffectiveGlobalToken.ColorPrimaryHover;
        TrackBorderColor        = Colors.Transparent;
        TrackCheckedBorderColor = Colors.Transparent;
        TrackBorderThickness    = 0;
        HandleSize      = new Size(handleSize, handleSize);
        HandleSizeSM    = new Size(handleSizeSM, handleSizeSM);
        EnableHandlePressStretch = true;

        InnerMinMargin   = handleSize / 2;
        InnerMaxMargin   = handleSize + padding * 3;
        InnerMinMarginSM = handleSizeSM / 2 - padding;
        InnerMaxMarginSM = handleSizeSM + padding * 3;
        
        SwitchColor            = EffectiveGlobalToken.ColorPrimary;
        UncheckedContentColor  = EffectiveGlobalToken.ColorTextLightSolid;
        SwitchDisabledOpacity  = EffectiveGlobalToken.OpacityLoading;

        if (!IsExplicit(ToggleSwitchTokenKind.ExtraInfoFontSize))
        {
            ExtraInfoFontSize   = EffectiveGlobalToken.FontSizeSM;
        }

        ExtraInfoFontSizeSM = ExtraInfoFontSize - 1;

        HandleShadow = new BoxShadow
        {
            OffsetX = 0,
            OffsetY = 2,
            Blur    = 4,
            Color   = Color.FromArgb((int)(255 * 0.2), 0, 35, 11)
        };

        ContentIconSize            = TrackHeightSM;
        ContentIconSizeSM          = TrackHeightSM - EffectiveGlobalToken.UniformlyPaddingXXS;
        LoadingAnimationDuration   = TimeSpan.FromMilliseconds(1200); // 毫秒
        OffStateLoadIndicatorColor = ColorUtils.FromRgbF(0.4,0.0, 0.0, 0.0);
    }
    
}
