using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;
using Avalonia.Animation.Easings;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>
/// The PrimeNG view of <see cref="SplitViewToken" />.
/// </summary>
[ControlTokenRule]
internal sealed class SplitViewTokenRule : IControlTokenRule<PrimeNGToken, SplitViewToken>
{
    public void Apply(in TokenEvaluationContext<PrimeNGToken> context, ControlTokenTarget<SplitViewToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        // Ant Design formula kept as is: PrimeUIX defines no values for this control, so the rule takes the Ant
        // Design formula over the PrimeNG global tokens.
        token.OpenPaneThemeLength     = 320;
        token.CompactPaneThemeLength  = 48;
        token.PaneOpenMotionDuration  = TimeSpan.FromMilliseconds(200);
        token.PaneCloseMotionDuration = TimeSpan.FromMilliseconds(100);
        token.PaneMotionEasing        = Easing.Parse("0.1,0.9,0.2,1.0");
    }
}
