using AtomUI.Theme.DesignTokens;

namespace AtomUI.Theme.Languages;

/// <summary>
/// One control's view: <see cref="Token"/> is its control token, <see cref="Global"/> the global tokens it sees.
/// Reading <see cref="Global"/> gives this control's value; writing it overrides the value for this control only, and
/// only Map and Alias tokens may be written (spec 6.1): a seed write throws.
/// </summary>
public sealed class ControlView<TToken>
    where TToken : AbstractControlDesignToken
{
    public ControlView(TToken token, DesignToken global)
    {
        ArgumentNullException.ThrowIfNull(token);
        ArgumentNullException.ThrowIfNull(global);
        Token  = token;
        Global = global;
    }

    public TToken Token { get; }

    public DesignToken Global { get; }
}
