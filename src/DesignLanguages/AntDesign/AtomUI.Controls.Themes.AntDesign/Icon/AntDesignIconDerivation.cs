using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme.Languages;
using Avalonia.Media;

namespace AtomUI.Controls.Themes.AntDesign;

/// <summary>The AntDesign view of <see cref="IconToken"/>.</summary>
[ControlDerivation]
internal sealed class AntDesignIconDerivation : IControlDerivation<AntDesignToken, IconToken>
{
    public void Derive(in DerivationContext<AntDesignToken> context, ControlView<IconToken> view)
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
