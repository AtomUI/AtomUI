using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>The PrimeNG view of <see cref="TreeTransferToken"/>.</summary>
[ControlTokenRule]
internal sealed class TreeTransferTokenRule : IControlTokenRule<PrimeNGToken, TreeTransferToken>
{
    public void Apply(in TokenEvaluationContext<PrimeNGToken> context, ControlTokenTarget<TreeTransferToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        TransferTokenCalculator.Calculate(in context, global, token);
        token.ListWidthLG = 250;
    }
}
