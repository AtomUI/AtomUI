using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>The PrimeNG view of <see cref="RangeDateViewerToken"/>.</summary>
[ControlTokenRule]
internal sealed class RangeDateViewerTokenRule : IControlTokenRule<PrimeNGToken, RangeDateViewerToken>
{
    public void Apply(in TokenEvaluationContext<PrimeNGToken> context, ControlTokenTarget<RangeDateViewerToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        // PrimeNG: the control-global overrides, read by the formulas below.
        PrimeNGRangeDateViewerTokens.EvaluateGlobal(in context, global);
        DateViewerTokenCalculator.Calculate(in context, global, token);
        token.CellHeight = global.ControlHeightSM;
        token.CellRowHeight = DateViewerTokenCalculator.CalculateCellRowHeight(token, global, token.CellHeight);
        token.PanelSpacing = global.UniformlyMargin;
        // PrimeNG: the preset's control values, over the formulas.
        PrimeNGRangeDateViewerTokens.EvaluateOwn(in context, global, token);
    }
}
