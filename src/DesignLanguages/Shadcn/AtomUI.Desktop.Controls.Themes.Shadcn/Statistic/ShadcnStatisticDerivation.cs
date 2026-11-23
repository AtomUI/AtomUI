using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>The shadcn view of <see cref="StatisticToken"/>.</summary>
[ControlDerivation]
internal sealed class ShadcnStatisticDerivation : IControlDerivation<ShadcnToken, StatisticToken>
{
    public void Derive(in DerivationContext<ShadcnToken> context, ControlView<StatisticToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        ShadcnAbstractStatisticDerivation.DeriveShared(in context, global, token);
    }
}
