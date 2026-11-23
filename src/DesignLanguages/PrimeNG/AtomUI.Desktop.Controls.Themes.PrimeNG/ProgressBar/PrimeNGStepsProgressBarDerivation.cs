using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>The PrimeNG view of <see cref="StepsProgressBarToken"/>.</summary>
[ControlDerivation]
internal sealed class PrimeNGStepsProgressBarDerivation : IControlDerivation<PrimeNGToken, StepsProgressBarToken>
{
    public void Derive(in DerivationContext<PrimeNGToken> context, ControlView<StepsProgressBarToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        // PrimeNG: the control-global overrides, read by the formulas below.
        PrimeNGProgressTokens<StepsProgressBarToken>.EvaluateGlobal(in context, global);
        PrimeNGAbstractLineProgressDerivation.DeriveShared(in context, global, token);
        token.ProgressStepMarginInlineEnd = global.UniformlyMarginXXS / 2;

        token.ProgressStepMinWidth = token.ProgressStepMarginInlineEnd;
        // PrimeNG: the preset's control values, over the formulas.
        PrimeNGProgressTokens<StepsProgressBarToken>.EvaluateOwn(in context, global, token);
    }
}
