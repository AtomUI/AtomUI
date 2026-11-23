using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>The shadcn view of <see cref="AutoCompleteSearchEditToken"/>.</summary>
[ControlDerivation]
internal sealed class ShadcnAutoCompleteSearchEditDerivation : IControlDerivation<ShadcnToken, AutoCompleteSearchEditToken>
{
    public void Derive(in DerivationContext<ShadcnToken> context, ControlView<AutoCompleteSearchEditToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        ShadcnAbstractAutoCompleteDerivation.DeriveShared(in context, global, token);
    }
}
