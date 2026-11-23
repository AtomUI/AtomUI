using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>The PrimeNG view of <see cref="TimelineToken"/>.</summary>
[ControlDerivation]
internal sealed class PrimeNGTimelineDerivation : IControlDerivation<PrimeNGToken, TimelineToken>
{
    public void Derive(in DerivationContext<PrimeNGToken> context, ControlView<TimelineToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        // PrimeNG: the control-global overrides, read by the formulas below.
        PrimeNGTimelineTokens.EvaluateGlobal(in context, global);

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
