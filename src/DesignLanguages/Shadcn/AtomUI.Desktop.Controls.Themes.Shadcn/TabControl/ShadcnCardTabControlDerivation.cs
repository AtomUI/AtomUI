using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>The shadcn view of <see cref="CardTabControlToken"/>.</summary>
[ControlDerivation]
internal sealed class ShadcnCardTabControlDerivation : IControlDerivation<ShadcnToken, CardTabControlToken>
{
    public void Derive(in DerivationContext<ShadcnToken> context, ControlView<CardTabControlToken> view)
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
        token.TabAndContentGutter = global.UniformlyMarginSM;
    }
}
