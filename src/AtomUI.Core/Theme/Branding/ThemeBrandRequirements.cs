using System.Globalization;

namespace AtomUI.Theme.Branding;

public sealed record ThemeBrandRequirements
{
    public ThemeBrandRequirements(string schemaRevision, string definitionDigest, string? programDigest,
        ThemeStyleCapabilities capabilities)
    {
        ArgumentNullException.ThrowIfNull(schemaRevision);
        ArgumentException.ThrowIfNullOrWhiteSpace(definitionDigest);
        if (schemaRevision.Length != 16 || schemaRevision.Any(static ch => !Uri.IsHexDigit(ch)) ||
            !ulong.TryParse(schemaRevision, NumberStyles.HexNumber,
                CultureInfo.InvariantCulture, out var value))
        {
            throw new ArgumentException("A schema revision requires sixteen hexadecimal digits.", nameof(schemaRevision));
        }
        if (definitionDigest.Length > 1024 || programDigest?.Length > 1024)
        {
            throw new ArgumentException("A definition digest is too long.");
        }
        const ThemeStyleCapabilities known = ThemeStyleCapabilities.QualifiedStyles |
            ThemeStyleCapabilities.CompiledTemplates | ThemeStyleCapabilities.RegisteredStaticAssets |
            ThemeStyleCapabilities.ScopedResources;
        if ((capabilities & ~known) != 0)
        {
            throw new ArgumentOutOfRangeException(nameof(capabilities));
        }
        SchemaRevision = value.ToString("X16", CultureInfo.InvariantCulture);
        DefinitionDigest = definitionDigest;
        ProgramDigest = programDigest;
        Capabilities = capabilities;
    }

    public string SchemaRevision { get; }
    public string DefinitionDigest { get; }
    public string? ProgramDigest { get; }
    public ThemeStyleCapabilities Capabilities { get; }
}
