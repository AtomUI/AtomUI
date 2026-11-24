using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>The shadcn view of <see cref="TransferTreeViewToken"/>.</summary>
[ControlTokenRule]
internal sealed class TransferTreeViewTokenRule : IControlTokenRule<ShadcnToken, TransferTreeViewToken>
{
    public void Apply(in TokenEvaluationContext<ShadcnToken> context, ControlTokenTarget<TransferTreeViewToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        TreeViewTokenCalculator.Calculate(in context, global, token);
    }
}
