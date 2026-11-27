using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>
/// The PrimeNG view of <see cref="DateViewerToken" />.
/// </summary>
[ControlTokenRule]
internal sealed class DateViewerTokenRule : IControlTokenRule<PrimeNGToken, DateViewerToken>
{
    public void Apply(in TokenEvaluationContext<PrimeNGToken> context, ControlTokenTarget<DateViewerToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        // PrimeNG: no control-global overrides; EvaluateGlobal writes nothing.
        PrimeNGDateViewerTokens.EvaluateGlobal(in context, global);
        DateViewerTokenCalculator.Calculate(in context, global, token);
        token.CellHeight = global.ControlHeightSM;
        token.CellRowHeight = DateViewerTokenCalculator.CalculateCellRowHeight(token, global, token.CellHeight);
        token.FooterBorderThickness = new Thickness(0, global.LineWidth, 0, 0);
        // PrimeNG: the preset's control values, over the formulas.
        PrimeNGDateViewerTokens.EvaluateOwn(in context, global, token);
    }
}
