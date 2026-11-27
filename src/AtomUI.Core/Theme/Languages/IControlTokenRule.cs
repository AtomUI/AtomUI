using AtomUI.Theme.DesignTokens;

namespace AtomUI.Theme.Languages;

/// <summary>
/// Supplies one control's token values for a design language. The rule writes every property of
/// <typeparamref name="TToken" /> and may override Map and Alias tokens for that control.
/// The compiler freezes explicit values before applying the rule.
/// </summary>
public interface IControlTokenRule<TLanguageToken, TToken>
    where TLanguageToken : LanguageDesignToken
    where TToken : AbstractControlDesignToken
{
    /// <summary>
    /// Derives the token values of one control. The compiler calls it for each control compilation, after it
    /// wrote and froze the control's explicit values.
    /// </summary>
    /// <param name="context">
    /// The variant the control is derived for, and the language tokens derived for it.
    /// </param>
    /// <param name="target">
    /// The control's token object and the global tokens the control sees.
    /// </param>
    void Apply(in TokenEvaluationContext<TLanguageToken> context, ControlTokenTarget<TToken> target);
}
