namespace AtomUI.Theme.DesignTokens;

/// <summary>
/// Marks a design language's token class; its seed properties carry <see cref="DesignTokenKindAttribute" />.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class LanguageDesignTokenAttribute : Attribute
{
}
