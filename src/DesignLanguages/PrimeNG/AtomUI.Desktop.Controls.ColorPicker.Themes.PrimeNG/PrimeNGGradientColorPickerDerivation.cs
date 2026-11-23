using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>The PrimeNG view of <see cref="GradientColorPickerToken"/>.</summary>
[ControlDerivation]
internal sealed class PrimeNGGradientColorPickerDerivation : IControlDerivation<PrimeNGToken, GradientColorPickerToken>
{
    public void Derive(in DerivationContext<PrimeNGToken> context, ControlView<GradientColorPickerToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        PrimeNGAbstractColorPickerDerivation.DeriveShared(in context, global, token);
    }
}
