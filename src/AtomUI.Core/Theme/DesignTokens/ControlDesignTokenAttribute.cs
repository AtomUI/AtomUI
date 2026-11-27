namespace AtomUI.Theme.DesignTokens;

/// <summary>
/// Marks a class that declares the own design tokens of a control, one <c>partial</c> property per token. Source
/// generators read the attribute at compile time to implement those properties and to generate the resource keys
/// of the tokens. It is not inherited, so every derived token class must carry it as well.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class ControlDesignTokenAttribute : Attribute
{
}
