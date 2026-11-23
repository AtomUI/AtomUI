using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>The shadcn view of <see cref="CardTabStripToken"/>.</summary>
[ControlDerivation]
internal sealed class ShadcnCardTabStripDerivation : IControlDerivation<ShadcnToken, CardTabStripToken>
{
    public void Derive(in DerivationContext<ShadcnToken> context, ControlView<CardTabStripToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        // Card tabs cap the radius ladder at 8.
        global.BorderRadiusSM = ShadcnCardTabRadius.Limit(global.BorderRadiusSM);
        global.BorderRadiusMD = ShadcnCardTabRadius.Limit(global.BorderRadiusMD);
        global.BorderRadiusLG = ShadcnCardTabRadius.Limit(global.BorderRadiusLG);
        ShadcnAbstractCardTabDerivation.DeriveShared(in context, global, token);
        token.CardSize = global.ControlHeightLG;
        token.CardPadding = ShadcnAbstractCardTabDerivation.CalculateCardPadding(token, global, token.CardSize);
    }
}
