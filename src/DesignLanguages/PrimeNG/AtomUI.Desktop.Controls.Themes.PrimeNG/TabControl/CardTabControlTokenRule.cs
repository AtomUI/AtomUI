using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>The PrimeNG view of <see cref="CardTabControlToken"/>.</summary>
[ControlTokenRule]
internal sealed class CardTabControlTokenRule : IControlTokenRule<PrimeNGToken, CardTabControlToken>
{
    public void Apply(in TokenEvaluationContext<PrimeNGToken> context, ControlTokenTarget<CardTabControlToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        CardTabTokenCalculator.Calculate(in context, global, token);
        token.CardSize = global.ControlHeightLG;
        token.CardPadding = CardTabTokenCalculator.CalculateCardPadding(token, global, token.CardSize);
        token.TabAndContentGutter = global.UniformlyMarginSM;
    }
}
