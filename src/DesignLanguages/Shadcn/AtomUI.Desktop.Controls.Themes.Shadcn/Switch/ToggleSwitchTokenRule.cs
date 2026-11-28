using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Media;
using AtomUI.Theme.Languages;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>
/// The shadcn view of <see cref="ToggleSwitchToken" />.
/// </summary>
[ControlTokenRule]
internal sealed class ToggleSwitchTokenRule : IControlTokenRule<ShadcnToken, ToggleSwitchToken>
{
    public void Apply(in TokenEvaluationContext<ShadcnToken> context, ControlTokenTarget<ToggleSwitchToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        // The switch needs enough contrast for the thumb and optional content on the neutral checked track (dark, neutral
        // theme).
        var checkedTrack = global.ColorPrimary;
        if (context.Variant.IsDark && global.ColorPrimary == context.Language.Primary)
        {
            checkedTrack               = ShadcnRecipes.CompositeOver(context.Language.Primary, 0.45, context.Language.Background);
            global.ColorPrimaryHover   = ShadcnRecipes.CompositeOver(context.Language.Primary, 0.48, context.Language.Background);
            global.ColorTextLightSolid = context.Language.Foreground;
        }
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

        // Ant Design formula kept as is: no registry value is merged into the track border or the handle sizes;
        // the registry `switch` values are in the commented groups.
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

        token.SwitchColor            = checkedTrack;
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
        // registry switch: the thumb does not stretch while the unchecked track is pressed.
        token.EnableHandlePressStretch = false;
        var mutedText = global.ColorTextSecondary;
        if (context.Variant.IsDark)
        {
            var switchSurface = global.ColorBgContainer;
            token.TrackBg      = ShadcnRecipes.CompositeOver(mutedText, 0.44, switchSurface);
            token.TrackHoverBg = ShadcnRecipes.CompositeOver(mutedText, 0.50, switchSurface);
        }
        else
        {
            token.TrackBg      = mutedText;
            token.TrackHoverBg = ShadcnRecipes.CompositeOver(global.ColorText, 0.08, mutedText);
        }
    }
}
