using AtomUI.Theme.DesignTokens;

namespace AtomUI.Theme.Languages;

/// <summary>
/// The token objects a rule can write during control evaluation. <see cref="Own"/> holds the control tokens.
/// Reading <see cref="Global"/> gives this control's value; writing it overrides the value for this control only, and
/// only Map and Alias tokens may be written (spec 6.1): a seed write throws.
/// </summary>
public sealed class ControlTokenTarget<TToken>
    where TToken : AbstractControlDesignToken
{
    public ControlTokenTarget(TToken own, DesignToken global)
    {
        ArgumentNullException.ThrowIfNull(own);
        ArgumentNullException.ThrowIfNull(global);
        Own    = own;
        Global = global;
    }

    public TToken Own { get; }

    public DesignToken Global { get; }
}
