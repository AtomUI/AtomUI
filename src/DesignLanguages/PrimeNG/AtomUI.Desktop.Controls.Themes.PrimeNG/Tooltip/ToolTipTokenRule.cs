using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>
/// The PrimeNG view of <see cref="ToolTipToken" />.
/// </summary>
[ControlTokenRule]
internal sealed class ToolTipTokenRule : IControlTokenRule<PrimeNGToken, ToolTipToken>
{
    public void Apply(in TokenEvaluationContext<PrimeNGToken> context, ControlTokenTarget<ToolTipToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        // PrimeNG: the control-global overrides (seeds included), read by the formulas below.
        PrimeNGToolTipTokens.EvaluateGlobal(in context, global);

        // Ant Design formula kept as is: the PrimeNG tooltip stage at the end of the method replaces every value of
        // this group (MotionDuration only under Nora).
        token.ToolTipMaxWidth   = 250;
        token.ToolTipFontSize   = global.FontSize;
        token.ToolTipMinHeight  = global.ControlHeight;
        token.ToolTipColor      = global.ColorTextLightSolid;
        token.ToolTipBackground = global.ColorBgSpotlight;
        token.ToolTipCornerRadius = new CornerRadius(4);
        token.ContentPadding = new Thickness(global.UniformlyPaddingSM, global.UniformlyPaddingSM / 2 + 2);
        token.MotionDuration = global.MotionDurationFast;
        token.ToolTipShadows = global.BoxShadowsSecondary;
        token.MarginToAnchor       = 4;
        token.ArrowSize            = global.SizePopupArrow;
        token.ArrowTipRadius       = global.BorderRadiusXS.TopLeft;
        token.ArrowBaseRadius      = global.BorderRadiusOuter.TopLeft;
        token.ArrowStrokeThickness = 1;
        // PrimeNG: the preset's control values, over the formulas.
        PrimeNGToolTipTokens.EvaluateOwn(in context, global, token);
    }
}
