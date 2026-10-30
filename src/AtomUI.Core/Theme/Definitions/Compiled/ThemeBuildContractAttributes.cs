using System.ComponentModel;

namespace AtomUI.Theme.Definitions.Compiled;

[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class ThemeBuildOwnerAttribute(
    int version, string catalog, string id, string ownerAssemblyIdentity, string ownerMetadataName,
    string identityProvider, string schemaDigest) : Attribute
{
    public int Version { get; } = version;
    public string Catalog { get; } = catalog;
    public string Id { get; } = id;
    public string OwnerAssemblyIdentity { get; } = ownerAssemblyIdentity;
    public string OwnerMetadataName { get; } = ownerMetadataName;
    public string IdentityProvider { get; } = identityProvider;
    public string SchemaDigest { get; } = schemaDigest;
}

[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class ThemeBuildTokenAttribute(
    int version, string catalog, string ownerId, string name, string valueType, string valueTypeAssemblyIdentity,
    string resourceKeyType, string resourceKeyAssemblyIdentity, string resourceKeyMember, int stage,
    int valueContractVersion) : Attribute
{
    public int Version { get; } = version;
    public string Catalog { get; } = catalog;
    public string OwnerId { get; } = ownerId;
    public string Name { get; } = name;
    public string ValueType { get; } = valueType;
    public string ValueTypeAssemblyIdentity { get; } = valueTypeAssemblyIdentity;
    public string ResourceKeyType { get; } = resourceKeyType;
    public string ResourceKeyAssemblyIdentity { get; } = resourceKeyAssemblyIdentity;
    public string ResourceKeyMember { get; } = resourceKeyMember;
    public int Stage { get; } = stage;
    public int ValueContractVersion { get; } = valueContractVersion;
}

[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class CompiledThemeManifestAttribute(int version, string packageId, string data) : Attribute
{
    public int Version { get; } = version;
    public string PackageId { get; } = packageId;
    public string Data { get; } = data;
}
