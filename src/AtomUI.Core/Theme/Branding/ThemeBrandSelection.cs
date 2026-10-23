namespace AtomUI.Theme.Branding;

public sealed record ThemeBrandSelection
{
    internal ThemeBrandSelection(string id, string revision)
    {
        Id = id;
        Revision = revision;
    }

    public string Id { get; }
    public string Revision { get; }
}
