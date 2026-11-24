using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>The shadcn view of <see cref="TreeTransferToken"/>.</summary>
[ControlTokenRule]
internal sealed class TreeTransferTokenRule : IControlTokenRule<ShadcnToken, TreeTransferToken>
{
    public void Apply(in TokenEvaluationContext<ShadcnToken> context, ControlTokenTarget<TreeTransferToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        TransferTokenCalculator.Calculate(in context, global, token);
        token.ListWidthLG = 250;
    }
}
