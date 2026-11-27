using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.Languages;
using Avalonia.Media;

namespace AtomUI.Controls.Themes.Shadcn;

/// <summary>
/// The shadcn view of <see cref="IconToken" />.
/// </summary>
[ControlTokenRule]
internal sealed class IconTokenRule : IControlTokenRule<ShadcnToken, IconToken>
{
    public void Apply(in TokenEvaluationContext<ShadcnToken> context, ControlTokenTarget<IconToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        // Ant formula kept as is: shadcn/ui has no icon component, so the language has no values of its own.
        token.SecondaryStrokeColor = Colors.White;
        token.SecondaryFillColor   = Color.Parse("#43CCF8");
        token.FallbackColor        = Colors.White;
        token.StrokeWidth          = 4;
        token.StrokeLineCap        = PenLineCap.Round;
        token.StrokeLineJoin       = PenLineJoin.Round;
    }
}
