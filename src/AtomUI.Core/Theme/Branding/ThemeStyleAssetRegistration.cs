using AtomUI.Theme.Schema;
using AtomUI.Theme.Styling;
using Avalonia.Styling;

namespace AtomUI.Theme.Branding;

public sealed class ThemeStyleAssetRegistration
{
    public ThemeStyleAssetRegistration(string id, string revision, Type controlType,
        ControlTokenIdentity ownerIdentity, Func<Style> factory,
        ThemeStyleCapabilities requiredCapabilities = ThemeStyleCapabilities.QualifiedStyles)
    {
        SchemaIdentifier.Validate(id, nameof(id));
        ThemeBrandIdentifiers.ValidateRevision(revision);
        SchemaIdentifier.Validate(ownerIdentity.Catalog, nameof(ownerIdentity));
        SchemaIdentifier.Validate(ownerIdentity.Id, nameof(ownerIdentity));
        if (ownerIdentity.OwnerType is not null && ownerIdentity.OwnerType != controlType)
        {
            throw new ArgumentException("The asset owner type and identity disagree.", nameof(ownerIdentity));
        }
        const ThemeStyleCapabilities known = ThemeStyleCapabilities.QualifiedStyles |
            ThemeStyleCapabilities.CompiledTemplates | ThemeStyleCapabilities.RegisteredStaticAssets |
            ThemeStyleCapabilities.ScopedResources;
        if ((requiredCapabilities & ~known) != 0)
        {
            throw new ArgumentOutOfRangeException(nameof(requiredCapabilities));
        }
        Asset = new ThemeStyleAsset(id, revision, controlType, factory,
            requiredCapabilities | ThemeStyleCapabilities.QualifiedStyles);
        OwnerIdentity = ownerIdentity;
        RequiredCapabilities = requiredCapabilities | ThemeStyleCapabilities.QualifiedStyles;
    }

    public string Id => Asset.Id;
    public string Revision => Asset.Revision;
    public Type ControlType => Asset.ControlType;
    public ControlTokenIdentity OwnerIdentity { get; }
    public ThemeStyleCapabilities RequiredCapabilities { get; }
    internal ThemeStyleAsset Asset { get; }
}
