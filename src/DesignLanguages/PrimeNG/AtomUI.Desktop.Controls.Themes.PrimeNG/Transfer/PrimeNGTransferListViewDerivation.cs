using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>The PrimeNG view of <see cref="TransferListViewToken"/>.</summary>
[ControlDerivation]
internal sealed class PrimeNGTransferListViewDerivation : IControlDerivation<PrimeNGToken, TransferListViewToken>
{
    public void Derive(in DerivationContext<PrimeNGToken> context, ControlView<TransferListViewToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        // PrimeNG: the control-global overrides, read by the formulas below.
        PrimeNGTransferListViewTokens.EvaluateGlobal(in context, global);
        PrimeNGAbstractListViewDerivation.DeriveShared(in context, global, token);

        token.PaginationMargin = new Thickness(0, global.UniformlyMarginXXS);
        // PrimeNG: the preset's control values, over the formulas.
        PrimeNGTransferListViewTokens.EvaluateOwn(in context, global, token);
    }
}
