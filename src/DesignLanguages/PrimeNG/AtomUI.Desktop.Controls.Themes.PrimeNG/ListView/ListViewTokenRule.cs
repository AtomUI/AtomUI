using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>The PrimeNG view of <see cref="ListViewToken"/>.</summary>
[ControlTokenRule]
internal sealed class ListViewTokenRule : IControlTokenRule<PrimeNGToken, ListViewToken>
{
    public void Apply(in TokenEvaluationContext<PrimeNGToken> context, ControlTokenTarget<ListViewToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        // PrimeNG: the control-global overrides, read by the formulas below.
        PrimeNGListViewTokens.EvaluateGlobal(in context, global);
        ListViewTokenCalculator.Calculate(in context, global, token);
        token.GroupHeaderPadding = new Thickness(global.UniformlyPaddingSM, global.UniformlyPaddingXS);
        token.SelectedIndicatorMargin = new Thickness(global.UniformlyMarginXXS, 0, 0, 0);
        token.ItemSelectedIndicatorColor = global.ColorPrimary;
        token.PaginationMargin = new Thickness(0, global.UniformlyMarginXS);
        // PrimeNG: the preset's control values, over the formulas.
        PrimeNGListViewTokens.EvaluateOwn(in context, global, token);
    }
}
