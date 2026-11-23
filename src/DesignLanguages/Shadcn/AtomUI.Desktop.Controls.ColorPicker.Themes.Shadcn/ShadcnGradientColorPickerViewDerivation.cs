using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>The shadcn view of <see cref="GradientColorPickerViewToken"/>.</summary>
[ControlDerivation]
internal sealed class ShadcnGradientColorPickerViewDerivation : IControlDerivation<ShadcnToken, GradientColorPickerViewToken>
{
    public void Derive(in DerivationContext<ShadcnToken> context, ControlView<GradientColorPickerViewToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        ShadcnAbstractColorPickerViewDerivation.DeriveShared(in context, global, token);
        token.ColorPickerHandlerSizeSM = 12;
        token.ColorPickerPresetColorSize = 24;
        token.ColorPickerSliderTrackSize = 8;
        token.ColorPickerSliderSize = token.ColorPickerHandlerSizeSM;
        ShadcnAbstractColorPickerViewDerivation.CalculateDependentSizes(token, global, token.ColorPickerHandlerSizeSM, token.ColorPickerPresetColorSize, token.ColorPickerSliderTrackSize);
    }
}
