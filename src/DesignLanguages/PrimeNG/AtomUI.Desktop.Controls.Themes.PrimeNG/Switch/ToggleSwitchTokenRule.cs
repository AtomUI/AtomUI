using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Media;
using AtomUI.Theme.Languages;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>
/// The PrimeNG view of <see cref="ToggleSwitchToken" />.
/// </summary>
[ControlTokenRule]
internal sealed class ToggleSwitchTokenRule : IControlTokenRule<PrimeNGToken, ToggleSwitchToken>
{
    public void Apply(in TokenEvaluationContext<PrimeNGToken> context, ControlTokenTarget<ToggleSwitchToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        // PrimeNG: the control-global overrides (MotionDurationMid), read by the ToggleSwitch theme (the knob
        // transitions) and not by the formulas below.
        PrimeNGToggleSwitchTokens.EvaluateGlobal(in context, global);
        // Ant Design formula kept as is: the PrimeNG toggle switch stage at the end of the method replaces the
        // geometry (track, padding, handle sizes, inner margins), the track and handle colors and the handle shadow.
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
        token.LoadingAnimationDuration   = TimeSpan.FromMilliseconds(1200); // milliseconds
        token.OffStateLoadIndicatorColor = ColorUtils.FromRgbF(0.4,0.0, 0.0, 0.0);
        // PrimeNG: the preset's control values, over the formulas.
        PrimeNGToggleSwitchTokens.EvaluateOwn(in context, global, token);
    }
}
