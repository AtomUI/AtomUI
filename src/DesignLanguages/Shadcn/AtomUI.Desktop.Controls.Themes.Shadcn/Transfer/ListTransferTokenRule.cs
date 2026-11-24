using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>The shadcn view of <see cref="ListTransferToken"/>.</summary>
[ControlTokenRule]
internal sealed class ListTransferTokenRule : IControlTokenRule<ShadcnToken, ListTransferToken>
{
    public void Apply(in TokenEvaluationContext<ShadcnToken> context, ControlTokenTarget<ListTransferToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        TransferTokenCalculator.Calculate(in context, global, token);
        token.ListWidthLG = 250;
    }
}
