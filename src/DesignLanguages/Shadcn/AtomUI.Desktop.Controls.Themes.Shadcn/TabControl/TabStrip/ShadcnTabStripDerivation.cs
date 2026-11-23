using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>The shadcn view of <see cref="TabStripToken"/>.</summary>
[ControlDerivation]
internal sealed class ShadcnTabStripDerivation : IControlDerivation<ShadcnToken, TabStripToken>
{
    public void Derive(in DerivationContext<ShadcnToken> context, ControlView<TabStripToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        ShadcnAbstractLineTabDerivation.DeriveShared(in context, global, token);
    }
}
