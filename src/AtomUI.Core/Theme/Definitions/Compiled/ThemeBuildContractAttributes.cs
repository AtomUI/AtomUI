using System.ComponentModel;

namespace AtomUI.Theme.Definitions.Compiled;

/// <summary>
/// Publishes one control of the assembly as an owner of control tokens. The source generator of an assembly that
/// references this one reads it to resolve the tokens owned by that control and to check the owner.
/// </summary>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class ThemeBuildOwnerAttribute(
    int version, string catalog, string id, string ownerAssemblyIdentity, string ownerMetadataName,
    string identityProvider, string schemaDigest) : Attribute
{
    /// <summary>
    /// The version of the attribute's argument list. The source generator writes 1 and rejects any other value.
    /// </summary>
    public int Version { get; } = version;
    /// <summary>
    /// The catalog the control's tokens are filed under, the first part of its
    /// <see cref="AtomUI.Theme.Schema.ControlTokenIdentity" />; the control's <see cref="Id" /> is the second part.
    /// </summary>
    public string Catalog { get; } = catalog;
    /// <summary>
    /// The control's simple name inside the catalog. A compiled theme package requires the control by catalog and id.
    /// </summary>
    public string Id { get; } = id;
    /// <summary>
    /// The full identity of the assembly that declares the owner type. The owner type is resolved in that assembly, and
    /// only its name, culture and public key token are compared, not its version.
    /// </summary>
    public string OwnerAssemblyIdentity { get; } = ownerAssemblyIdentity;
    /// <summary>
    /// The metadata name of the control type that owns the tokens.
    /// </summary>
    public string OwnerMetadataName { get; } = ownerMetadataName;
    /// <summary>
    /// The metadata name, with a <c>global::</c> prefix, of the type that declares the owner's public static
    /// <c>Identity</c> member, a <see cref="AtomUI.Theme.Schema.ControlTokenIdentity" />.
    /// </summary>
    public string IdentityProvider { get; } = identityProvider;
    /// <summary>
    /// The SHA-256 digest, as 64 hex characters, of the owner's identity and of the contracts of its own tokens. It
    /// changes whenever the owner or one of its tokens changes, so a consumer can tell that a token set differs from
    /// the one it was built against.
    /// </summary>
    public string SchemaDigest { get; } = schemaDigest;
}

/// <summary>
/// Publishes one token of the assembly: a global token, a language token or a control's own token. The source
/// generator of an assembly that references this one reads it to check that a token it relies on still exists with
/// the same value type, stage and value contract.
/// </summary>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class ThemeBuildTokenAttribute(
    int version, string catalog, string ownerId, string name, string valueType, string valueTypeAssemblyIdentity,
    string resourceKeyType, string resourceKeyAssemblyIdentity, string resourceKeyMember, int stage,
    int valueContractVersion) : Attribute
{
    /// <summary>
    /// The version of the attribute's argument list. The source generator writes 1 and rejects any other value.
    /// </summary>
    public int Version { get; } = version;
    /// <summary>
    /// The catalog the token is filed under: empty for a global token, <c>$language</c> for a language token, or the
    /// catalog of the owner control for one of its own tokens.
    /// </summary>
    public string Catalog { get; } = catalog;
    /// <summary>
    /// The owner's id inside <see cref="Catalog" />. Empty for a global token; for a language token the language's id.
    /// </summary>
    public string OwnerId { get; } = ownerId;
    /// <summary>
    /// The token's name, which is also the name of the resource key member <see cref="ResourceKeyMember" />.
    /// </summary>
    public string Name { get; } = name;
    /// <summary>
    /// The metadata name of the token's value type, for example <c>System.Double</c>.
    /// </summary>
    public string ValueType { get; } = valueType;
    /// <summary>
    /// The full identity of the assembly that declares the value type. For a nullable value type it is the assembly of
    /// its underlying type.
    /// </summary>
    public string ValueTypeAssemblyIdentity { get; } = valueTypeAssemblyIdentity;
    /// <summary>
    /// The metadata name, with a <c>global::</c> prefix, of the enum type that declares the token's resource key: the
    /// shared key type for a global token, the control's own key type for one of its own tokens. Empty for a language
    /// token, which is bound by name.
    /// </summary>
    public string ResourceKeyType { get; } = resourceKeyType;
    /// <summary>
    /// The full identity of the assembly that declares the resource key type. Empty for a language token.
    /// </summary>
    public string ResourceKeyAssemblyIdentity { get; } = resourceKeyAssemblyIdentity;
    /// <summary>
    /// The name of the constant in the resource key type that keys this token. It equals <see cref="Name" />.
    /// </summary>
    public string ResourceKeyMember { get; } = resourceKeyMember;
    /// <summary>
    /// The token's stage as the integer value of <see cref="AtomUI.Theme.Schema.TokenStage" />: 0 for a seed, 1 for a
    /// Map token, 2 for an Alias token, 3 for a control token and 4 for a language token.
    /// </summary>
    public int Stage { get; } = stage;
    /// <summary>
    /// The version of the literal value contract: 1 when the value type has a literal codec, so a compiled theme package
    /// can write the token's value as a literal; 0 when it cannot.
    /// </summary>
    public int ValueContractVersion { get; } = valueContractVersion;
}

/// <summary>
/// Publishes one design language declared by the assembly and its language token class, so that compiled theme
/// packages naming the language can verify it and bind its language tokens at build time.
/// </summary>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class ThemeBuildLanguageAttribute(int version, string id, string languageTokenType,
    string languageTokenAssemblyIdentity) : Attribute
{
    /// <summary>
    /// The version of the attribute's argument list. The source generator writes 1 and rejects any other value.
    /// </summary>
    public int Version { get; } = version;
    /// <summary>
    /// The language's id, which is also the owner id of its language tokens.
    /// </summary>
    public string Id { get; } = id;
    /// <summary>
    /// The metadata name of the language's token class, the type that declares the language tokens.
    /// </summary>
    public string LanguageTokenType { get; } = languageTokenType;
    /// <summary>
    /// The full identity of the assembly that declares the language's token class.
    /// </summary>
    public string LanguageTokenAssemblyIdentity { get; } = languageTokenAssemblyIdentity;
}

/// <summary>
/// Publishes the manifest of a compiled theme package: the themes, controls and tokens it references, with the
/// contract values it was built against. The startup source generator of an assembly that references the package
/// reads it and checks those references before it generates the startup registration.
/// </summary>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class CompiledThemeManifestAttribute(int version, string packageId, string data) : Attribute
{
    /// <summary>
    /// The format version of the manifest encoding. The generator writes 1.
    /// </summary>
    public int Version { get; } = version;
    /// <summary>
    /// The id of the theme package that carries this attribute.
    /// </summary>
    public string PackageId { get; } = packageId;
    /// <summary>
    /// The encoded manifest, as a string: for every token the package references, its scope, its owner and the contract
    /// values the package was built against.
    /// </summary>
    public string Data { get; } = data;
}
