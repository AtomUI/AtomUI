namespace AtomUI.Theme.Schema;

/// <summary>A retained Own Token name that projects the value of a canonical Own Token.</summary>
public sealed class ControlTokenAlias
{
    public ControlTokenAlias(string name, string canonicalName)
    {
        SchemaIdentifier.Validate(name, nameof(name));
        SchemaIdentifier.Validate(canonicalName, nameof(canonicalName));
        Name = name;
        CanonicalName = canonicalName;
    }

    public string Name { get; }
    public string CanonicalName { get; }
}
