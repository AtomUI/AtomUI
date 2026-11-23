using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

/// <summary>The AntDesign view of <see cref="ColorPickerViewToken"/>.</summary>
[ControlDerivation]
internal sealed class AntDesignColorPickerViewDerivation : IControlDerivation<AntDesignToken, ColorPickerViewToken>
{
    public void Derive(in DerivationContext<AntDesignToken> context, ControlView<ColorPickerViewToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        AntDesignAbstractColorPickerViewDerivation.DeriveShared(in context, global, token);
        token.ColorPickerHandlerSizeSM = 12;
        token.ColorPickerPresetColorSize = 24;
        token.ColorPickerSliderTrackSize = 8;
        token.ColorPickerSliderSize = token.ColorPickerHandlerSizeSM;
        AntDesignAbstractColorPickerViewDerivation.CalculateDependentSizes(token, global, token.ColorPickerHandlerSizeSM, token.ColorPickerPresetColorSize, token.ColorPickerSliderTrackSize);
    }
}
