using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>The PrimeNG view of <see cref="CompactSpaceAddOnToken"/>.</summary>
[ControlDerivation]
internal sealed class PrimeNGCompactSpaceAddOnDerivation : IControlDerivation<PrimeNGToken, CompactSpaceAddOnToken>
{
    public void Derive(in DerivationContext<PrimeNGToken> context, ControlView<CompactSpaceAddOnToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        token.AddonBg       = global.ColorFillAlter;

        token.AddOnPadding   = new Thickness(global.UniformlyPaddingSM, 0);
        token.AddOnPaddingSM = new Thickness(global.ControlPaddingHorizontalSM, 0);
        token.AddOnPaddingLG = new Thickness(global.ControlPaddingHorizontal, 0);
    }
}
