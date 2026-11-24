using AtomUI.Theme.DesignTokens;

namespace AtomUI.Theme.Languages;

/// <summary>
/// Supplies one control's token values for a design language. The rule writes every property of
/// <typeparamref name="TToken"/> and may override Map and Alias tokens for that control.
/// The compiler freezes explicit values before applying the rule.
/// </summary>
public interface IControlTokenRule<TLanguageToken, TToken>
    where TLanguageToken : LanguageDesignToken
    where TToken : AbstractControlDesignToken
{
    void Apply(in TokenEvaluationContext<TLanguageToken> context, ControlTokenTarget<TToken> target);
}
