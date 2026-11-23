using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>The PrimeNG view of <see cref="TransferTreeViewToken"/>.</summary>
[ControlDerivation]
internal sealed class PrimeNGTransferTreeViewDerivation : IControlDerivation<PrimeNGToken, TransferTreeViewToken>
{
    public void Derive(in DerivationContext<PrimeNGToken> context, ControlView<TransferTreeViewToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        // PrimeNG: the control-global overrides, read by the formulas below.
        PrimeNGTransferTreeViewTokens.EvaluateGlobal(in context, global);
        PrimeNGAbstractTreeViewDerivation.DeriveShared(in context, global, token);
        // PrimeNG: the preset's control values, over the formulas.
        PrimeNGTransferTreeViewTokens.EvaluateOwn(in context, global, token);
    }
}
