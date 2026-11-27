namespace AtomUI.Theme.DesignTokens;

/// <summary>
/// Marks an Own Token field as a promised-but-not-yet-consumed extension point. Every Own Token field must be
/// consumed by the package's theme AXAML or code, marked with this attribute, or deleted; the generator reports
/// <c>ATOMUIGEN042</c> otherwise.
/// </summary>
[AttributeUsage(AttributeTargets.Property, Inherited = false)]
public sealed class ReservedTokenAttribute(string reason) : Attribute
{
    /// <summary>
    /// Why the field exists although nothing consumes it yet, for example the control feature that will read it.
    /// </summary>
    public string Reason { get; } = reason;
}
