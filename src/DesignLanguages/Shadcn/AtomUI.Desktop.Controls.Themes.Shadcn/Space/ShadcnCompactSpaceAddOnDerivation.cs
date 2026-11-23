using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>The shadcn view of <see cref="CompactSpaceAddOnToken"/>.</summary>
[ControlDerivation]
internal sealed class ShadcnCompactSpaceAddOnDerivation : IControlDerivation<ShadcnToken, CompactSpaceAddOnToken>
{
    public void Derive(in DerivationContext<ShadcnToken> context, ControlView<CompactSpaceAddOnToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        token.AddonBg       = global.ColorFillAlter;

        token.AddOnPadding   = new Thickness(global.UniformlyPaddingSM, 0);
        token.AddOnPaddingSM = new Thickness(global.ControlPaddingHorizontalSM, 0);
        token.AddOnPaddingLG = new Thickness(global.ControlPaddingHorizontal, 0);
    }
}
