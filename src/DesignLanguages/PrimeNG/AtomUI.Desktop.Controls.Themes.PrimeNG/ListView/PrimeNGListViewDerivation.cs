using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>The PrimeNG view of <see cref="ListViewToken"/>.</summary>
[ControlDerivation]
internal sealed class PrimeNGListViewDerivation : IControlDerivation<PrimeNGToken, ListViewToken>
{
    public void Derive(in DerivationContext<PrimeNGToken> context, ControlView<ListViewToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        // PrimeNG: the control-global overrides, read by the formulas below.
        PrimeNGListViewTokens.EvaluateGlobal(in context, global);
        PrimeNGAbstractListViewDerivation.DeriveShared(in context, global, token);
        token.GroupHeaderPadding = new Thickness(global.UniformlyPaddingSM, global.UniformlyPaddingXS);
        token.SelectedIndicatorMargin = new Thickness(global.UniformlyMarginXXS, 0, 0, 0);
        token.ItemSelectedIndicatorColor = global.ColorPrimary;
        token.PaginationMargin = new Thickness(0, global.UniformlyMarginXS);
        // PrimeNG: the preset's control values, over the formulas.
        PrimeNGListViewTokens.EvaluateOwn(in context, global, token);
    }
}
