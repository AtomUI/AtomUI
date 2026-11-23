using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>The shadcn view of <see cref="StatisticCountUpToken"/>.</summary>
[ControlDerivation]
internal sealed class ShadcnStatisticCountUpDerivation : IControlDerivation<ShadcnToken, StatisticCountUpToken>
{
    public void Derive(in DerivationContext<ShadcnToken> context, ControlView<StatisticCountUpToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        ShadcnAbstractStatisticValueDerivation.DeriveShared(in context, global, token);
    }
}
