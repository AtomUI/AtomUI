using AtomUI.Theme.Schema;

namespace AtomUI.Theme.DesignTokens;

/// <summary>Retains this Own Token property as a compatibility view of a canonical Own Token.</summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
public sealed class ControlTokenAliasAttribute : Attribute
{
    public ControlTokenAliasAttribute(string canonicalName)
    {
        SchemaIdentifier.Validate(canonicalName, nameof(canonicalName));
        CanonicalName = canonicalName;
    }

    public string CanonicalName { get; }
}
