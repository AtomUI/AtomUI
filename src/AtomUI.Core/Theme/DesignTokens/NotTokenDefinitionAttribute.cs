namespace AtomUI.Theme.DesignTokens;

/// <summary>
/// Marks a property of a token class that is not a token. The token source generator skips it when it
/// collects the tokens of the class.
/// </summary>
[AttributeUsage(AttributeTargets.Property, Inherited = false)]
public class NotTokenDefinitionAttribute : Attribute
{
}