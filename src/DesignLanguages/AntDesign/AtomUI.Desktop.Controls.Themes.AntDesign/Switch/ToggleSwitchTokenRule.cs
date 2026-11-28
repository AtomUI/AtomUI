using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Media;
using AtomUI.Theme.Languages;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

/// <summary>
/// The AntDesign view of <see cref="ToggleSwitchToken" />.
/// </summary>
[ControlTokenRule]
internal sealed class ToggleSwitchTokenRule : IControlTokenRule<AntDesignToken, ToggleSwitchToken>
{
    public void Apply(in TokenEvaluationContext<AntDesignToken> context, ControlTokenTarget<ToggleSwitchToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        var fontSize      = global.FontSize;
        var lineHeight    = global.RelativeLineHeight;
        var controlHeight = global.ControlHeight;

        var    height       = fontSize * lineHeight;
        var    heightSM     = controlHeight / 2;
        double padding      = 2; // Fixed value
        var    handleSize   = height - padding * 2;
        var    handleSizeSM = heightSM - padding * 2;

        token.TrackHeight     = height;
        token.TrackHeightSM   = heightSM;

        token.TrackMinWidth   = handleSize * 2 + padding * 4;
        token.TrackMinWidthSM = handleSizeSM * 2 + padding * 2;
        token.TrackPadding    = padding; // Fixed value
        token.HandleBg = global.ColorWhite;
        token.HandleCheckedBg = token.HandleBg;
        token.HandleHoverBg = token.HandleBg;
        token.HandleCheckedHoverBg = token.HandleCheckedBg;

        token.TrackBg                 = global.ColorTextQuaternary;
        token.TrackHoverBg            = global.ColorTextTertiary;
        token.TrackCheckedHoverBg     = global.ColorPrimaryHover;
        token.TrackBorderColor        = Colors.Transparent;
        token.TrackCheckedBorderColor = Colors.Transparent;
        token.TrackBorderThickness    = 0;
        token.TrackCornerRadius       = null;
        token.HandleCornerRadius      = null;
        token.HandleSize      = new Size(handleSize, handleSize);
        token.HandleSizeSM    = new Size(handleSizeSM, handleSizeSM);
        token.EnableHandlePressStretch = true;

        token.InnerMinMargin   = handleSize / 2;
        token.InnerMaxMargin   = handleSize + padding * 3;
        token.InnerMinMarginSM = handleSizeSM / 2 - padding;
        token.InnerMaxMarginSM = handleSizeSM + padding * 3;

        token.SwitchColor            = global.ColorPrimary;
        token.UncheckedContentColor  = global.ColorTextLightSolid;
        token.SwitchDisabledOpacity  = global.OpacityLoading;
        token.ExtraInfoFontSize   = global.FontSizeSM;

        token.ExtraInfoFontSizeSM = token.ExtraInfoFontSize - 1;

        token.HandleShadow = new BoxShadow
        {
            OffsetX = 0,
            OffsetY = 2,
            Blur    = 4,
            Color   = Color.FromArgb((int)(255 * 0.2), 0, 35, 11)
        };

        token.ContentIconSize            = token.TrackHeightSM;
        token.ContentIconSizeSM          = token.TrackHeightSM - global.UniformlyPaddingXXS;
        token.LoadingAnimationDuration   = TimeSpan.FromMilliseconds(1200);
        token.OffStateLoadIndicatorColor = ColorUtils.FromRgbF(0.4,0.0, 0.0, 0.0);
    }
}
