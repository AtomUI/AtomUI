using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>
/// The PrimeNG view of <see cref="TimelineToken" />.
/// </summary>
[ControlTokenRule]
internal sealed class TimelineTokenRule : IControlTokenRule<PrimeNGToken, TimelineToken>
{
    public void Apply(in TokenEvaluationContext<PrimeNGToken> context, ControlTokenTarget<TimelineToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        // PrimeNG: no control-global overrides; EvaluateGlobal writes nothing.
        PrimeNGTimelineTokens.EvaluateGlobal(in context, global);

        // Ant Design formula kept as is: the PrimeNG timeline stage at the end of the method replaces only the tail
        // color and width (IndicatorTailColor, IndicatorTailWidth).
        token.IndicatorTailColor = global.ColorSplit;
        token.IndicatorTailWidth = global.LineWidthBold;

        token.IndicatorDotBorderWidth = global.LineWidth * 3;
        token.ItemPaddingBottom       = new Thickness(0, 0, 0, global.UniformlyPadding * 1.25);

        token.ItemPaddingBottomLG     = token.ItemPaddingBottom * 2;

        token.IndicatorStartModeMargin  = new Thickness(0, 0, global.UniformlyMargin, 0);
        token.IndicatorEndModeMargin    = new Thickness(global.UniformlyMargin, 0, 0, 0);
        token.IndicatorMiddleModeMargin = new Thickness(global.UniformlyMargin, 0);

        token.IndicatorSize            = global.SizeMS;
        token.IndicatorDotSize         = 8;
        // PrimeNG: the preset's control values, over the formulas.
        PrimeNGTimelineTokens.EvaluateOwn(in context, global, token);
    }
}
