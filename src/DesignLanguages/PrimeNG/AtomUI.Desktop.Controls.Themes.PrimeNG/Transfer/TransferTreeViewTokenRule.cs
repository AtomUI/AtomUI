using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>The PrimeNG view of <see cref="TransferTreeViewToken"/>.</summary>
[ControlTokenRule]
internal sealed class TransferTreeViewTokenRule : IControlTokenRule<PrimeNGToken, TransferTreeViewToken>
{
    public void Apply(in TokenEvaluationContext<PrimeNGToken> context, ControlTokenTarget<TransferTreeViewToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        // PrimeNG: the control-global overrides, read by the formulas below.
        PrimeNGTransferTreeViewTokens.EvaluateGlobal(in context, global);
        TreeViewTokenCalculator.Calculate(in context, global, token);
        // PrimeNG: the preset's control values, over the formulas.
        PrimeNGTransferTreeViewTokens.EvaluateOwn(in context, global, token);
    }
}
