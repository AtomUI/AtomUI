using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>The PrimeNG view of <see cref="ImagePreviewerToken"/>.</summary>
[ControlDerivation]
internal sealed class PrimeNGImagePreviewerDerivation : IControlDerivation<PrimeNGToken, ImagePreviewerToken>
{
    public void Derive(in DerivationContext<PrimeNGToken> context, ControlView<ImagePreviewerToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        PrimeNGAbstractImagePreviewerDerivation.DeriveShared(in context, global, token);
        token.ImagePreviewSwitchSize = global.ControlHeightLG;
    }
}
