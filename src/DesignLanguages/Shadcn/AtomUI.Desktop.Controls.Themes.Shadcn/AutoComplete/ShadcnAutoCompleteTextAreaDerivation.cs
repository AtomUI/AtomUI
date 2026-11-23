using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>The shadcn view of <see cref="AutoCompleteTextAreaToken"/>.</summary>
[ControlDerivation]
internal sealed class ShadcnAutoCompleteTextAreaDerivation : IControlDerivation<ShadcnToken, AutoCompleteTextAreaToken>
{
    public void Derive(in DerivationContext<ShadcnToken> context, ControlView<AutoCompleteTextAreaToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        ShadcnAbstractAutoCompleteDerivation.DeriveShared(in context, global, token);
    }
}
