using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

/// <summary>The AntDesign view of <see cref="BorderBeamToken"/>.</summary>
[ControlTokenRule]
internal sealed class BorderBeamTokenRule : IControlTokenRule<AntDesignToken, BorderBeamToken>
{
    public void Apply(in TokenEvaluationContext<AntDesignToken> context, ControlTokenTarget<BorderBeamToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        token.BeamSize              = 100d;
        token.BeamOpacity           = 0.95d;
        token.MotionDuration        = TimeSpan.FromSeconds(6);
        token.MaxVisibleStopPercent = 70d;
    }
}
