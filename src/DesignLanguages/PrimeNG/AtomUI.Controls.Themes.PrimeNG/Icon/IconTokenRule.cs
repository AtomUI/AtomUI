using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;
using Avalonia.Media;

namespace AtomUI.Controls.Themes.PrimeNG;

/// <summary>
/// The PrimeNG view of <see cref="IconToken" />.
/// </summary>
[ControlTokenRule]
internal sealed class IconTokenRule : IControlTokenRule<PrimeNGToken, IconToken>
{
    public void Apply(in TokenEvaluationContext<PrimeNGToken> context, ControlTokenTarget<IconToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        // Ant Design formula kept as is: PrimeUIX defines no values for this control, so the rule takes the Ant
        // Design formula over the PrimeNG global tokens.
        token.SecondaryStrokeColor = Colors.White;
        token.SecondaryFillColor   = Color.Parse("#43CCF8");
        token.FallbackColor        = Colors.White;
        token.StrokeWidth          = 4;
        token.StrokeLineCap        = PenLineCap.Round;
        token.StrokeLineJoin       = PenLineJoin.Round;
    }
}
