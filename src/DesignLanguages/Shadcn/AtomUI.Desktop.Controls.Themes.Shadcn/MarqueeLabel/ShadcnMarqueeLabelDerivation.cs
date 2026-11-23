using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>The shadcn view of <see cref="MarqueeLabelToken"/>.</summary>
[ControlDerivation]
internal sealed class ShadcnMarqueeLabelDerivation : IControlDerivation<ShadcnToken, MarqueeLabelToken>
{
    public void Derive(in DerivationContext<ShadcnToken> context, ControlView<MarqueeLabelToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        token.CycleSpace   = 200;
        token.DefaultSpeed = 150;
    }
}
