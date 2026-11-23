using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>The PrimeNG view of <see cref="ColorPickerViewToken"/>.</summary>
[ControlDerivation]
internal sealed class PrimeNGColorPickerViewDerivation : IControlDerivation<PrimeNGToken, ColorPickerViewToken>
{
    public void Derive(in DerivationContext<PrimeNGToken> context, ControlView<ColorPickerViewToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        PrimeNGAbstractColorPickerViewDerivation.DeriveShared(in context, global, token);
        token.ColorPickerHandlerSizeSM = 12;
        token.ColorPickerPresetColorSize = 24;
        token.ColorPickerSliderTrackSize = 8;
        token.ColorPickerSliderSize = token.ColorPickerHandlerSizeSM;
        PrimeNGAbstractColorPickerViewDerivation.CalculateDependentSizes(token, global, token.ColorPickerHandlerSizeSM, token.ColorPickerPresetColorSize, token.ColorPickerSliderTrackSize);
    }
}
