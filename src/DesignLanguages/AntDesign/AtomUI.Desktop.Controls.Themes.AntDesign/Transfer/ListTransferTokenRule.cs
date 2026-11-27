using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

/// <summary>
/// The AntDesign view of <see cref="ListTransferToken" />.
/// </summary>
[ControlTokenRule]
internal sealed class ListTransferTokenRule : IControlTokenRule<AntDesignToken, ListTransferToken>
{
    public void Apply(in TokenEvaluationContext<AntDesignToken> context, ControlTokenTarget<ListTransferToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        TransferTokenCalculator.Calculate(in context, global, token);
        token.ListWidthLG = 250;
    }
}
