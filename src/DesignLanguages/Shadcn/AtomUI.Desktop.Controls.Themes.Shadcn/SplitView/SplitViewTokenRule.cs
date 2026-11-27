using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.Languages;
using Avalonia.Animation.Easings;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>
/// The shadcn view of <see cref="SplitViewToken" />.
/// </summary>
[ControlTokenRule]
internal sealed class SplitViewTokenRule : IControlTokenRule<ShadcnToken, SplitViewToken>
{
    public void Apply(in TokenEvaluationContext<ShadcnToken> context, ControlTokenTarget<SplitViewToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        // Ant Design formula kept as is: shadcn has no SplitView, so the language has no values of its own.
        token.OpenPaneThemeLength     = 320;
        token.CompactPaneThemeLength  = 48;
        token.PaneOpenMotionDuration  = TimeSpan.FromMilliseconds(200);
        token.PaneCloseMotionDuration = TimeSpan.FromMilliseconds(100);
        token.PaneMotionEasing        = Easing.Parse("0.1,0.9,0.2,1.0");
    }
}
