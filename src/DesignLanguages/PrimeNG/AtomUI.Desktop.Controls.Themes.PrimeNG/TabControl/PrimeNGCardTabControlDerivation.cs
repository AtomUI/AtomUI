using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>The PrimeNG view of <see cref="CardTabControlToken"/>.</summary>
[ControlDerivation]
internal sealed class PrimeNGCardTabControlDerivation : IControlDerivation<PrimeNGToken, CardTabControlToken>
{
    public void Derive(in DerivationContext<PrimeNGToken> context, ControlView<CardTabControlToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        PrimeNGAbstractCardTabDerivation.DeriveShared(in context, global, token);
        token.CardSize = global.ControlHeightLG;
        token.CardPadding = PrimeNGAbstractCardTabDerivation.CalculateCardPadding(token, global, token.CardSize);
        token.TabAndContentGutter = global.UniformlyMarginSM;
    }
}
