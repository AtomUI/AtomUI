using AtomUI.Theme.DesignTokens;

namespace AtomUI.Theme.Languages;

/// <summary>
/// How one design language derives one control's view. The derivation writes every property of
/// <typeparamref name="TToken"/> and only the global tokens that differ for this control. Explicit values are frozen
/// before it runs; its writes to them are ignored.
/// </summary>
public interface IControlDerivation<TLanguageToken, TToken>
    where TLanguageToken : LanguageDesignToken
    where TToken : AbstractControlDesignToken
{
    void Derive(in DerivationContext<TLanguageToken> context, ControlView<TToken> view);
}
