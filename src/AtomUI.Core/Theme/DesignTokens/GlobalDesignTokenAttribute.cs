namespace AtomUI.Theme.DesignTokens;

/// <summary>
/// Marks a class as a global design token class. The token source generator implements its partial token
/// properties and generates the resource keys of its tokens.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple=true)]
public class GlobalDesignTokenAttribute : Attribute
{
}