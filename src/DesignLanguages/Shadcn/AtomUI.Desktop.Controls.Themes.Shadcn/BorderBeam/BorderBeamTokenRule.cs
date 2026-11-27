using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>
/// The shadcn view of <see cref="BorderBeamToken" />.
/// </summary>
[ControlTokenRule]
internal sealed class BorderBeamTokenRule : IControlTokenRule<ShadcnToken, BorderBeamToken>
{
    public void Apply(in TokenEvaluationContext<ShadcnToken> context, ControlTokenTarget<BorderBeamToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        // Ant Design formula kept as is: shadcn has no border beam, so the language has no values of its own.
        token.BeamSize              = 100d;
        token.BeamOpacity           = 0.95d;
        token.MotionDuration        = TimeSpan.FromSeconds(6);
        token.MaxVisibleStopPercent = 70d;
    }
}
