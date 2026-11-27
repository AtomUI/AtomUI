using AtomUI.Theme.DesignTokens;

namespace AtomUI.Theme.Languages;

/// <summary>
/// The token objects a rule can write during control evaluation. <see cref="Own" /> holds the control tokens.
/// Reading <see cref="Global" /> gives this control's value; writing it overrides the value for this control only, and
/// only Map and Alias tokens may be written: a seed write throws.
/// </summary>
public sealed class ControlTokenTarget<TToken>
    where TToken : AbstractControlDesignToken
{
    /// <summary>
    /// Creates the target of one control evaluation.
    /// </summary>
    /// <param name="own">
    /// The control's token object, which the rule writes.
    /// </param>
    /// <param name="global">
    /// The global tokens the control sees; the rule may override Map and Alias tokens in them for this control only.
    /// </param>
    public ControlTokenTarget(TToken own, DesignToken global)
    {
        ArgumentNullException.ThrowIfNull(own);
        ArgumentNullException.ThrowIfNull(global);
        Own    = own;
        Global = global;
    }

    /// <summary>
    /// The control's token object. Explicit control tokens are written and frozen before the rule runs, so
    /// its writes to them are ignored.
    /// </summary>
    public TToken Own { get; }

    /// <summary>
    /// The global tokens as this control sees them. Reading gives this control's value; writing overrides a
    /// Map or Alias token for this control only, and writing a seed throws.
    /// </summary>
    public DesignToken Global { get; }
}
