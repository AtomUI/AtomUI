using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>The shadcn view of <see cref="AdornerLayerToken"/>.</summary>
[ControlDerivation]
internal sealed class ShadcnAdornerLayerDerivation : IControlDerivation<ShadcnToken, AdornerLayerToken>
{
    public void Derive(in DerivationContext<ShadcnToken> context, ControlView<AdornerLayerToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        token.FocusOutlineOffset = global.LineWidth;
    }
}
