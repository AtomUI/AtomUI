namespace AtomUI.Theme.DesignTokens;

/// <summary>
/// Marks a token property with its role in the token derivation. The source generators use it to collect
/// the tokens of a token class and to tell seeds from derived tokens.
/// </summary>
[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple=false)]
public class DesignTokenKindAttribute : Attribute
{
    /// <summary>
    /// The role of the marked property, as passed to the constructor.
    /// </summary>
    public DesignTokenKind Kind { get; }

    /// <summary>
    /// Marks a property as a token of the given kind.
    /// </summary>
    /// <param name="kind">
    /// The role of the property in the token derivation.
    /// </param>
    public DesignTokenKindAttribute(DesignTokenKind kind)
    {
        Kind = kind;
    }
}