namespace AtomUI.Startup.Data;

// Shared by the compiler and runtime. This model deliberately contains no CLR type,
// resource factory, Avalonia object or application-owned state.
internal sealed class StartupPackageData
{
    internal StartupPackageData(string id, string assemblyName, IEnumerable<StartupControlData> controls,
        IEnumerable<StartupAssetData> assets, ulong schemaFingerprint, IEnumerable<string>? globalResourceTokens = null)
    {
        Id = id;
        AssemblyName = assemblyName;
        Controls = Array.AsReadOnly(controls.ToArray());
        Assets = Array.AsReadOnly(assets.ToArray());
        SchemaFingerprint = schemaFingerprint;
        GlobalResourceTokens = globalResourceTokens is null ? null : Array.AsReadOnly(globalResourceTokens.ToArray());
    }

    internal string Id { get; }
    internal string AssemblyName { get; }
    internal IReadOnlyList<StartupControlData> Controls { get; }
    internal IReadOnlyList<StartupAssetData> Assets { get; }
    internal ulong SchemaFingerprint { get; }
    internal IReadOnlyList<string>? GlobalResourceTokens { get; }
}

internal sealed class StartupControlData
{
    internal StartupControlData(string catalog, string id, string typeName, IEnumerable<string> keys,
        IEnumerable<StartupTokenData> ownTokens, bool hasSemantic, bool hasTokenDescriptor = true,
        IEnumerable<string>? assets = null, string? assemblyName = null, IEnumerable<StartupSemanticThemeData>? semanticThemes = null)
    {
        Catalog = catalog;
        Id = id;
        TypeName = typeName;
        AssemblyName = assemblyName;
        Keys = Array.AsReadOnly(keys.ToArray());
        OwnTokens = Array.AsReadOnly(ownTokens.ToArray());
        HasSemantic = hasSemantic;
        HasTokenDescriptor = hasTokenDescriptor;
        Assets = Array.AsReadOnly((assets ?? Array.Empty<string>()).ToArray());
        SemanticThemes = Array.AsReadOnly((semanticThemes ?? Array.Empty<StartupSemanticThemeData>()).ToArray());
    }

    internal string Catalog { get; }
    internal string Id { get; }
    internal string Identity => Catalog + ":" + Id;
    internal string TypeName { get; }
    internal string? AssemblyName { get; }
    internal IReadOnlyList<string> Keys { get; }
    internal IReadOnlyList<StartupTokenData> OwnTokens { get; }
    internal bool HasSemantic { get; }
    internal bool HasTokenDescriptor { get; }
    internal IReadOnlyList<string> Assets { get; }
    internal IReadOnlyList<StartupSemanticThemeData> SemanticThemes { get; }
}

internal sealed class StartupTokenData
{
    internal StartupTokenData(string name, string valueType, string keyType, string keyMember, string? keyAssembly = null)
    {
        Name = name;
        ValueType = valueType;
        KeyType = keyType;
        KeyMember = keyMember;
        KeyAssembly = keyAssembly;
    }

    internal string Name { get; }
    internal string ValueType { get; }
    internal string KeyType { get; }
    internal string KeyMember { get; }
    internal string? KeyAssembly { get; }
}

internal sealed class StartupOwnerData
{
    internal StartupOwnerData(string identity, string assemblyName, string typeName)
    {
        Identity = identity;
        AssemblyName = assemblyName;
        TypeName = typeName;
    }
    internal string Identity { get; }
    internal string AssemblyName { get; }
    internal string TypeName { get; }
}

internal sealed class StartupSemanticThemeData
{
    internal StartupSemanticThemeData(string property, string targetAssembly, string targetType)
    {
        Property = property;
        TargetAssembly = targetAssembly;
        TargetType = targetType;
    }
    internal string Property { get; }
    internal string TargetAssembly { get; }
    internal string TargetType { get; }
}

internal sealed class StartupSemanticBindingData
{
    internal StartupSemanticBindingData(StartupOwnerData owner, StartupSemanticThemeData theme)
    {
        Owner = owner;
        Theme = theme;
    }
    internal StartupOwnerData Owner { get; }
    internal StartupSemanticThemeData Theme { get; }
}

internal sealed class StartupAssetData
{
    internal StartupAssetData(string id, string uri, int phase, int order, IEnumerable<string>? resourceKeys,
        IEnumerable<string> owners, IEnumerable<string> semanticOwners, IEnumerable<string>? exports = null,
        IEnumerable<StartupOwnerData>? requiredOwners = null, IEnumerable<StartupSemanticBindingData>? semanticBindings = null)
    {
        Id = id;
        Uri = uri;
        Phase = phase;
        Order = order;
        ResourceKeys = resourceKeys is null ? null : Array.AsReadOnly(resourceKeys.ToArray());
        Owners = Array.AsReadOnly(owners.ToArray());
        SemanticOwners = Array.AsReadOnly(semanticOwners.ToArray());
        Exports = Array.AsReadOnly((exports ?? Array.Empty<string>()).ToArray());
        RequiredOwners = Array.AsReadOnly((requiredOwners ?? Array.Empty<StartupOwnerData>()).ToArray());
        SemanticBindings = Array.AsReadOnly((semanticBindings ?? Array.Empty<StartupSemanticBindingData>()).ToArray());
    }

    internal string Id { get; }
    internal string Uri { get; }
    internal int Phase { get; }
    internal int Order { get; }
    internal IReadOnlyList<string>? ResourceKeys { get; }
    internal IReadOnlyList<string> Owners { get; }
    internal IReadOnlyList<string> SemanticOwners { get; }
    internal IReadOnlyList<string> Exports { get; }
    internal IReadOnlyList<StartupOwnerData> RequiredOwners { get; }
    internal IReadOnlyList<StartupSemanticBindingData> SemanticBindings { get; }
}

internal static class StartupManifestLinker
{
    internal static void Validate(IEnumerable<StartupPackageData> packages, IEnumerable<string>? globalResourceTokens = null)
    {
        var inputs = packages.ToArray();
        var ids = new HashSet<string>(StringComparer.Ordinal);
        var controls = new Dictionary<string, StartupControlData>(StringComparer.Ordinal);
        var definingAssemblies = new Dictionary<string, string>(StringComparer.Ordinal);
        var expectedGlobalTokens = globalResourceTokens?.OrderBy(name => name, StringComparer.Ordinal).ToArray();
        var types = new HashSet<string>(StringComparer.Ordinal);
        var assets = new HashSet<string>(StringComparer.Ordinal);
        var uris = new HashSet<string>(StringComparer.Ordinal);
        var exports = new HashSet<string>(StringComparer.Ordinal);
        foreach (var package in inputs)
        {
            if (!ids.Add(package.Id))
            {
                throw new InvalidDataException("Duplicate startup package '" + package.Id + "'.");
            }
            foreach (var control in package.Controls)
            {
                var hasIdentity = control.HasTokenDescriptor || control.HasSemantic;
                if (hasIdentity && (controls.ContainsKey(control.Identity) ||
                    !types.Add((control.AssemblyName ?? package.AssemblyName) + "|" + control.TypeName)))
                {
                    throw new InvalidDataException("Conflicting startup Control '" + control.Identity + "'.");
                }
                if (hasIdentity)
                {
                    controls.Add(control.Identity, control);
                    definingAssemblies.Add(control.Identity, control.AssemblyName ?? package.AssemblyName);
                }
                if (control.Keys.Count == 0 || control.Keys.Distinct(StringComparer.Ordinal).Count() != control.Keys.Count)
                {
                    throw new InvalidDataException("Control has invalid TypeMap keys: " + control.Identity);
                }
                if (control.OwnTokens.Select(token => token.Name).Distinct(StringComparer.Ordinal).Count() != control.OwnTokens.Count)
                {
                    throw new InvalidDataException("Duplicate Own Token: " + control.Identity);
                }
            }
        }
        foreach (var package in inputs)
        {
            if (package.GlobalResourceTokens is { } names && package.Assets.Count != 0)
            {
                var ordered = names.OrderBy(name => name, StringComparer.Ordinal).ToArray();
                expectedGlobalTokens ??= ordered;
                if (!ordered.SequenceEqual(expectedGlobalTokens))
                {
                    throw new InvalidDataException("Control package '" + package.Id + "' was compiled against an incompatible global resource-key schema.");
                }
            }
            foreach (var asset in package.Assets)
            {
                if (!assets.Add(asset.Id) || !uris.Add(asset.Uri) || !Uri.TryCreate(asset.Uri, UriKind.Absolute, out _) ||
                    asset.Phase < 0 || asset.Phase > 1)
                {
                    throw new InvalidDataException("Conflicting or invalid startup theme asset '" + asset.Id + "'.");
                }
                if (!asset.Owners.OrderBy(owner => owner, StringComparer.Ordinal).SequenceEqual(
                    asset.RequiredOwners.Select(owner => owner.Identity).OrderBy(owner => owner, StringComparer.Ordinal)) ||
                    !asset.SemanticOwners.OrderBy(owner => owner, StringComparer.Ordinal).SequenceEqual(
                    asset.SemanticBindings.Select(binding => binding.Owner.Identity).Distinct().OrderBy(owner => owner, StringComparer.Ordinal)))
                {
                    throw new InvalidDataException("Theme asset '" + asset.Id + "' is missing exact owner or semantic binding contracts.");
                }
                foreach (var owner in asset.RequiredOwners)
                {
                    var control = ValidateOwner(asset, owner, controls, definingAssemblies);
                    if (!control.HasTokenDescriptor)
                    {
                        throw new InvalidDataException("Theme asset '" + asset.Id + "' requires a Token descriptor for '" + owner.Identity + "'.");
                    }
                }
                foreach (var binding in asset.SemanticBindings)
                {
                    var control = ValidateOwner(asset, binding.Owner, controls, definingAssemblies);
                    var expected = binding.Theme;
                    if (!control.HasSemantic || !control.SemanticThemes.Any(theme => theme.Property == expected.Property &&
                        theme.TargetAssembly == expected.TargetAssembly && theme.TargetType == expected.TargetType))
                    {
                        throw new InvalidDataException("Theme asset '" + asset.Id + "' has an incompatible semantic binding '" +
                            binding.Owner.Identity + "." + expected.Property + "'.");
                    }
                }
                foreach (var key in asset.Exports)
                {
                    if (!exports.Add(key))
                    {
                        throw new InvalidDataException("Conflicting startup theme export '" + key + "'.");
                    }
                }
            }
        }
    }
    private static StartupControlData ValidateOwner(StartupAssetData asset, StartupOwnerData owner,
        IReadOnlyDictionary<string, StartupControlData> controls, IReadOnlyDictionary<string, string> assemblies)
    {
        if (!controls.TryGetValue(owner.Identity, out var control) ||
            control.TypeName != owner.TypeName || assemblies[owner.Identity] != owner.AssemblyName)
        {
            throw new InvalidDataException("Theme asset '" + asset.Id + "' requires a different defining type for Control '" + owner.Identity + "'.");
        }
        return control;
    }

}
