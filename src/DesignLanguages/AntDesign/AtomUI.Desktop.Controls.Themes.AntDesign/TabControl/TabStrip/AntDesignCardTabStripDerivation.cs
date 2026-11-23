using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

/// <summary>The AntDesign view of <see cref="CardTabStripToken"/>.</summary>
[ControlDerivation]
internal sealed class AntDesignCardTabStripDerivation : IControlDerivation<AntDesignToken, CardTabStripToken>
{
    public void Derive(in DerivationContext<AntDesignToken> context, ControlView<CardTabStripToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        AntDesignAbstractCardTabDerivation.DeriveShared(in context, global, token);
        token.CardSize = global.ControlHeightLG;
        token.CardPadding = AntDesignAbstractCardTabDerivation.CalculateCardPadding(token, global, token.CardSize);
    }
}
