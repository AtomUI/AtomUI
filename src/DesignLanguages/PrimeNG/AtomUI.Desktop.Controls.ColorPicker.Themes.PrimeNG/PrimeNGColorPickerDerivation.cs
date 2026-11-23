using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>The PrimeNG view of <see cref="ColorPickerToken"/>.</summary>
[ControlDerivation]
internal sealed class PrimeNGColorPickerDerivation : IControlDerivation<PrimeNGToken, ColorPickerToken>
{
    public void Derive(in DerivationContext<PrimeNGToken> context, ControlView<ColorPickerToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        PrimeNGAbstractColorPickerDerivation.DeriveShared(in context, global, token);
    }
}
