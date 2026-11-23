using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>The shadcn view of <see cref="CheckableTagToken"/>.</summary>
[ControlDerivation]
internal sealed class ShadcnCheckableTagDerivation : IControlDerivation<ShadcnToken, CheckableTagToken>
{
    public void Derive(in DerivationContext<ShadcnToken> context, ControlView<CheckableTagToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        ShadcnAbstractTagDerivation.DeriveShared(in context, global, token);
    }
}
