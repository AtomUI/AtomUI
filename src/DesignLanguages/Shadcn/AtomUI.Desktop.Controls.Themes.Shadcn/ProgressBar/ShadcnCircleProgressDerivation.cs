using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>The shadcn view of <see cref="CircleProgressToken"/>.</summary>
[ControlDerivation]
internal sealed class ShadcnCircleProgressDerivation : IControlDerivation<ShadcnToken, CircleProgressToken>
{
    public void Derive(in DerivationContext<ShadcnToken> context, ControlView<CircleProgressToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        ShadcnAbstractCircleProgressDerivation.DeriveShared(in context, global, token);
    }
}
