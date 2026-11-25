using AtomUI.Theme.Schema;

namespace AtomUI.Theme.DesignTokens;

/// <summary>
/// Implemented by the generated half of every terminal control token class that is bound to a control.
/// </summary>
/// <remarks>
/// Lets control token stages be registered without reflection: <see cref="ControlTokenAlgorithmDescriptor"/> reads
/// the control a token class belongs to from this static member.
/// </remarks>
public interface IControlToken
{
    /// <summary>
    /// The token identity of the control this token class belongs to.
    /// </summary>
    static abstract ControlTokenIdentity ControlIdentity { get; }
}
