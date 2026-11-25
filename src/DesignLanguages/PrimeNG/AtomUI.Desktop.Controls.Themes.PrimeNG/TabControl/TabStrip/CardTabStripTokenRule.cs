using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>The PrimeNG view of <see cref="CardTabStripToken"/>.</summary>
[ControlTokenRule]
internal sealed class CardTabStripTokenRule : IControlTokenRule<PrimeNGToken, CardTabStripToken>
{
    public void Apply(in TokenEvaluationContext<PrimeNGToken> context, ControlTokenTarget<CardTabStripToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        CardTabTokenCalculator.Calculate(in context, global, token);
        token.CardSize = global.ControlHeightLG;
        token.CardPadding = CardTabTokenCalculator.CalculateCardPadding(token, global, token.CardSize);
    }
}
