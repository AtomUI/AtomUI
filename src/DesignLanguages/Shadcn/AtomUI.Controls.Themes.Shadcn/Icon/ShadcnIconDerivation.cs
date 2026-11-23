using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.Languages;
using Avalonia.Media;

namespace AtomUI.Controls.Themes.Shadcn;

/// <summary>The shadcn view of <see cref="IconToken"/>.</summary>
[ControlDerivation]
internal sealed class ShadcnIconDerivation : IControlDerivation<ShadcnToken, IconToken>
{
    public void Derive(in DerivationContext<ShadcnToken> context, ControlView<IconToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        token.SecondaryStrokeColor = Colors.White;
        token.SecondaryFillColor   = Color.Parse("#43CCF8");
        token.FallbackColor        = Colors.White;
        token.StrokeWidth          = 4;
        token.StrokeLineCap        = PenLineCap.Round;
        token.StrokeLineJoin       = PenLineJoin.Round;
    }
}
