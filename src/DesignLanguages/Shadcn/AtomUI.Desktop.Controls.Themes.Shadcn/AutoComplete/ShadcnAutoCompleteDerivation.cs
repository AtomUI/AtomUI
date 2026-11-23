using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>The shadcn view of <see cref="AutoCompleteToken"/>.</summary>
[ControlDerivation]
internal sealed class ShadcnAutoCompleteDerivation : IControlDerivation<ShadcnToken, AutoCompleteToken>
{
    public void Derive(in DerivationContext<ShadcnToken> context, ControlView<AutoCompleteToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        ShadcnInputFamily.DeriveGlobal(in context, global);
        ShadcnAbstractAutoCompleteDerivation.DeriveShared(in context, global, token);
    }
}
