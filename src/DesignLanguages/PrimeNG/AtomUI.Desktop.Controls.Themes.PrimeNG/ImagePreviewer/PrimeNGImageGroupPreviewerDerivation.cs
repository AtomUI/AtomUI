using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>The PrimeNG view of <see cref="ImageGroupPreviewerToken"/>.</summary>
[ControlDerivation]
internal sealed class PrimeNGImageGroupPreviewerDerivation : IControlDerivation<PrimeNGToken, ImageGroupPreviewerToken>
{
    public void Derive(in DerivationContext<PrimeNGToken> context, ControlView<ImageGroupPreviewerToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        PrimeNGAbstractImagePreviewerDerivation.DeriveShared(in context, global, token);
    }
}
