namespace AtomUI.Theme.DesignTokens;

/// <summary>
/// The base of every control token class: the values and behaviour switches specific to one control. The class only
/// declares the tokens; every design language computes them in a control token rule
/// (<see cref="AtomUI.Theme.Languages.IControlTokenRule{TLanguageToken,TToken}"/>).
/// </summary>
public abstract class AbstractControlDesignToken : AbstractDesignToken
{
}
