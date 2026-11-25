using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

/// <summary>The AntDesign view of <see cref="TransferListViewToken"/>.</summary>
[ControlTokenRule]
internal sealed class TransferListViewTokenRule : IControlTokenRule<AntDesignToken, TransferListViewToken>
{
    public void Apply(in TokenEvaluationContext<AntDesignToken> context, ControlTokenTarget<TransferListViewToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        ListViewTokenCalculator.Calculate(in context, global, token);

        token.PaginationMargin = new Thickness(0, global.UniformlyMarginXXS);
    }
}
