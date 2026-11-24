using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>The shadcn view of <see cref="CardTabStripToken"/>.</summary>
[ControlTokenRule]
internal sealed class CardTabStripTokenRule : IControlTokenRule<ShadcnToken, CardTabStripToken>
{
    public void Apply(in TokenEvaluationContext<ShadcnToken> context, ControlTokenTarget<CardTabStripToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        // Card tabs cap the radius ladder at 8.
        global.BorderRadiusSM = ShadcnCardTabRadius.Limit(global.BorderRadiusSM);
        global.BorderRadiusMD = ShadcnCardTabRadius.Limit(global.BorderRadiusMD);
        global.BorderRadiusLG = ShadcnCardTabRadius.Limit(global.BorderRadiusLG);
        CardTabTokenCalculator.Calculate(in context, global, token);
        token.CardSize = global.ControlHeightLG;
        token.CardPadding = CardTabTokenCalculator.CalculateCardPadding(token, global, token.CardSize);
    }
}
