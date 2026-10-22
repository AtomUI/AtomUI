using AtomUI.Theme.Schema;

namespace AtomUI.Theme.DesignTokens;

[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
public sealed class ControlTokenDefaultAttribute : Attribute
{
    public ControlTokenDefaultAttribute(string tokenName, params string[] ownInputs)
    {
        SchemaIdentifier.Validate(tokenName, nameof(tokenName));
        ArgumentNullException.ThrowIfNull(ownInputs);
        foreach (var input in ownInputs)
        {
            SchemaIdentifier.Validate(input, nameof(ownInputs));
        }
        TokenName = tokenName;
        OwnInputs = Array.AsReadOnly(ownInputs.ToArray());
    }

    public string TokenName { get; }
    public IReadOnlyList<string> OwnInputs { get; }
    public bool ReplacesInheritedDefault { get; set; }
}
