using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

/// <summary>
/// The AntDesign view of <see cref="TreeTransferToken" />.
/// </summary>
[ControlTokenRule]
internal sealed class TreeTransferTokenRule : IControlTokenRule<AntDesignToken, TreeTransferToken>
{
    public void Apply(in TokenEvaluationContext<AntDesignToken> context, ControlTokenTarget<TreeTransferToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        TransferTokenCalculator.Calculate(in context, global, token);
        token.ListWidthLG = 250;
    }
}
