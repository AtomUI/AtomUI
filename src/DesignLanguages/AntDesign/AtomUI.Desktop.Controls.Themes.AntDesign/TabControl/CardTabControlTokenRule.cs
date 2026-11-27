using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

/// <summary>
/// The AntDesign view of <see cref="CardTabControlToken" />.
/// </summary>
[ControlTokenRule]
internal sealed class CardTabControlTokenRule : IControlTokenRule<AntDesignToken, CardTabControlToken>
{
    public void Apply(in TokenEvaluationContext<AntDesignToken> context, ControlTokenTarget<CardTabControlToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        CardTabTokenCalculator.Calculate(in context, global, token);
        token.CardSize = global.ControlHeightLG;
        token.CardPadding = CardTabTokenCalculator.CalculateCardPadding(token, global, token.CardSize);
        token.TabAndContentGutter = global.UniformlyMarginSM;
    }
}
