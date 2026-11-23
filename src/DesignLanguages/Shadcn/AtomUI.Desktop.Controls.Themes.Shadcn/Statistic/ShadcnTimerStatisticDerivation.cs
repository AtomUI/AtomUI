using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>The shadcn view of <see cref="TimerStatisticToken"/>.</summary>
[ControlDerivation]
internal sealed class ShadcnTimerStatisticDerivation : IControlDerivation<ShadcnToken, TimerStatisticToken>
{
    public void Derive(in DerivationContext<ShadcnToken> context, ControlView<TimerStatisticToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        ShadcnAbstractStatisticDerivation.DeriveShared(in context, global, token);
    }
}
