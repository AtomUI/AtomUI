using AtomUI.Theme.Schema;

namespace AtomUI.Theme.Branding;

public sealed record ThemeStyleAssetReference
{
    public ThemeStyleAssetReference(string id, string revision)
    {
        SchemaIdentifier.Validate(id, nameof(id));
        ThemeBrandIdentifiers.ValidateRevision(revision);
        Id = id;
        Revision = revision;
    }

    public string Id { get; }
    public string Revision { get; }
}

internal static class ThemeBrandIdentifiers
{
    internal static void ValidateRevision(string revision)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(revision);
        if (revision.Length > 128 || revision.Any(static value =>
                !(value is >= 'A' and <= 'Z' or >= 'a' and <= 'z' or >= '0' and <= '9' or '.' or '_' or '-' or '+' or ':')))
        {
            throw new ArgumentException("A revision must be a bounded ASCII identifier.", nameof(revision));
        }
    }
}
