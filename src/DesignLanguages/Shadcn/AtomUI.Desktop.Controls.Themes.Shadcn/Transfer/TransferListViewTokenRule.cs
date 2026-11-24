using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>The shadcn view of <see cref="TransferListViewToken"/>.</summary>
[ControlTokenRule]
internal sealed class TransferListViewTokenRule : IControlTokenRule<ShadcnToken, TransferListViewToken>
{
    public void Apply(in TokenEvaluationContext<ShadcnToken> context, ControlTokenTarget<TransferListViewToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        ListViewTokenCalculator.Calculate(in context, global, token);

        token.PaginationMargin = new Thickness(0, global.UniformlyMarginXXS);
    }
}
