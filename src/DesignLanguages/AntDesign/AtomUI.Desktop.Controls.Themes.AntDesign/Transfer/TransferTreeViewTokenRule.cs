using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

/// <summary>The AntDesign view of <see cref="TransferTreeViewToken"/>.</summary>
[ControlTokenRule]
internal sealed class TransferTreeViewTokenRule : IControlTokenRule<AntDesignToken, TransferTreeViewToken>
{
    public void Apply(in TokenEvaluationContext<AntDesignToken> context, ControlTokenTarget<TransferTreeViewToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        TreeViewTokenCalculator.Calculate(in context, global, token);
    }
}
