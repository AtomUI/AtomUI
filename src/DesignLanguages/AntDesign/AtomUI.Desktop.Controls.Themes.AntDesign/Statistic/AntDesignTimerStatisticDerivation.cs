using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

/// <summary>The AntDesign view of <see cref="TimerStatisticToken"/>.</summary>
[ControlDerivation]
internal sealed class AntDesignTimerStatisticDerivation : IControlDerivation<AntDesignToken, TimerStatisticToken>
{
    public void Derive(in DerivationContext<AntDesignToken> context, ControlView<TimerStatisticToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        AntDesignAbstractStatisticDerivation.DeriveShared(in context, global, token);
    }
}
