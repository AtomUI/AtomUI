using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;
using Avalonia.Media;

namespace AtomUI.Controls.Themes.PrimeNG;

/// <summary>The PrimeNG view of <see cref="IconToken"/>.</summary>
[ControlDerivation]
internal sealed class PrimeNGIconDerivation : IControlDerivation<PrimeNGToken, IconToken>
{
    public void Derive(in DerivationContext<PrimeNGToken> context, ControlView<IconToken> view)
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
