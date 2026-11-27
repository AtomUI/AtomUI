using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

/// <summary>
/// The AntDesign view of <see cref="CardTabStripToken" />.
/// </summary>
[ControlTokenRule]
internal sealed class CardTabStripTokenRule : IControlTokenRule<AntDesignToken, CardTabStripToken>
{
    public void Apply(in TokenEvaluationContext<AntDesignToken> context, ControlTokenTarget<CardTabStripToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        CardTabTokenCalculator.Calculate(in context, global, token);
        token.CardSize = global.ControlHeightLG;
        token.CardPadding = CardTabTokenCalculator.CalculateCardPadding(token, global, token.CardSize);
    }
}
