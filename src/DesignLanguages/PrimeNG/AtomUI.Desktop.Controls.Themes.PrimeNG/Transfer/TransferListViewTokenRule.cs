using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>
/// The PrimeNG view of <see cref="TransferListViewToken" />.
/// </summary>
[ControlTokenRule]
internal sealed class TransferListViewTokenRule : IControlTokenRule<PrimeNGToken, TransferListViewToken>
{
    public void Apply(in TokenEvaluationContext<PrimeNGToken> context, ControlTokenTarget<TransferListViewToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        // PrimeNG: no control-global overrides; EvaluateGlobal writes nothing.
        PrimeNGTransferListViewTokens.EvaluateGlobal(in context, global);
        ListViewTokenCalculator.Calculate(in context, global, token);

        token.PaginationMargin = new Thickness(0, global.UniformlyMarginXXS);
        // PrimeNG: the preset's control values, over the formulas.
        PrimeNGTransferListViewTokens.EvaluateOwn(in context, global, token);
    }
}
