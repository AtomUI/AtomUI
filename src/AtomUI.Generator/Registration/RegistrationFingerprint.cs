namespace AtomUI.Generator;

// Compiled-contract domain only; final exact-AQN fingerprints are materialized in Core.
// Must match AtomUI.Core ThemeSchemaRegistry.SchemaFingerprintBuilder: each UTF-16 unit and integer
// is widened to UInt64 and added as eight little-endian bytes. This is deliberately not byte/UTF-8 hashing.
// Field order must stay byte-identical to ThemeSchemaRegistry.ComputeGeneratedContractFingerprint:
// [contract version head] → [global names shared prefix] → [asset-specific fields].
internal static class RegistrationFingerprint
{
    internal static ulong ComputeContract(RegistrationAsset asset, IReadOnlyList<string> globalNames)
    {
        var hash = new Fnv1a();
        WriteContractPrefix(ref hash, globalNames);
        hash.Text(asset.Id); hash.Text(asset.Uri); hash.Number((ulong)asset.Exports.Count);
        foreach (var export in asset.Exports.OrderBy(value => value.Target.MetadataName, StringComparer.Ordinal)
                     .ThenBy(value => value.KeyType is null ? 1 : 0)
                     .ThenBy(value => value.KeyType?.MetadataName ?? value.Key, StringComparer.Ordinal))
        { hash.Text(export.Target.MetadataName); hash.Number(export.KeyType is null ? 0UL : 1UL); hash.Text(export.KeyType?.MetadataName ?? export.Key!); }
        hash.Number((ulong)asset.Owners.Count); foreach (var owner in asset.Owners.OrderBy(value => value.Catalog, StringComparer.Ordinal).ThenBy(value => value.Id, StringComparer.Ordinal))
        {
            WriteOwner(ref hash, owner);
        }

        hash.Number((ulong)asset.Bindings.Count); foreach (var binding in asset.Bindings.OrderBy(value => value.Owner.Catalog, StringComparer.Ordinal).ThenBy(value => value.Owner.Id, StringComparer.Ordinal)
                     .ThenBy(value => value.Property, StringComparer.Ordinal).ThenBy(value => value.Target.MetadataName, StringComparer.Ordinal))
        { WriteOwner(ref hash, binding.Owner); hash.Text(binding.Property); hash.Text(binding.Target.MetadataName); }

        return hash.Value;
    }

    // [contract version head] → [global names]；与 Core 侧 HashGlobalTokenNames 的契约前缀逐位一致。
    internal static ulong ComputeContractPrefix(IReadOnlyList<string> globalNames)
    {
        var hash = new Fnv1a();
        WriteContractPrefix(ref hash, globalNames);
        return hash.Value;
    }

    // [resource-key version head] → [global names]；与 Core 侧 HashGlobalTokenNames 的资源键前缀逐位一致。
    internal static ulong ComputeResourceKeyPrefix(IReadOnlyList<string> globalNames)
    {
        var hash = new Fnv1a();
        WriteResourceKeyPrefix(ref hash, globalNames);
        return hash.Value;
    }

    // Per-package compile-time schema identity over the registered controls (catalog, id, type,
    // own tokens in slot order) and assets (id, uri, contract fingerprint). Only controls carrying
    // a token descriptor reach the runtime registry, mirroring ComputeRevision's input domain.
    // Control slots are omitted: a slot is a derived function of the identity set, so any change
    // that alters slots also alters a hashed identity. Core combines these constants under the
    // "AtomUI.PackageSchemaRevision" domain; this hash's own header versions the package format.
    internal static ulong ComputePackageSchema(
        string packageId,
        string catalog,
        IReadOnlyList<RegistrationControl> controls,
        IReadOnlyList<ControlThemeInfo> tokens,
        IReadOnlyList<RegistrationAsset> assets)
    {
        var hash = new Fnv1a();
        hash.Text("AtomUI.PackageSchema");
        hash.Number(1);
        hash.Text(packageId);
        var registered = controls
            .Select(control => (Control: control, Token: tokens.FirstOrDefault(token =>
                token.HasDescriptor && token.ControlTypeName == control.Type.Name)))
            .Where(pair => pair.Token is not null)
            .OrderBy(pair => pair.Token!.ControlName, StringComparer.Ordinal)
            .ToArray();
        hash.Number((ulong)registered.Length);
        foreach (var (control, token) in registered)
        {
            hash.Text(catalog);
            hash.Text(token!.ControlName);
            hash.Text(control.Type.AssemblyQualifiedName);
            var ownTokens = token.OwnSchemaTokens.OrderBy(ownToken => ownToken.Name, StringComparer.Ordinal).ToArray();
            hash.Number((ulong)ownTokens.Length);
            for (var slot = 0; slot < ownTokens.Length; slot++)
            {
                var ownToken = ownTokens[slot];
                hash.Text(ownToken.Name);
                hash.Number((ulong)slot);
                hash.Number((ulong)(int)ownToken.Stage);
                hash.Text(ownToken.ValueType);
                hash.Text(ownToken.DeclaringType);
                hash.Text(token.ControlNamespace + ".DesignTokens." + token.TokenKindType + "." + ownToken.Name);
            }
        }
        hash.Number((ulong)assets.Count);
        foreach (var asset in assets.OrderBy(asset => asset.Uri, StringComparer.Ordinal))
        {
            hash.Text(asset.Id);
            hash.Text(asset.Uri);
            hash.Number(asset.CompiledContractFingerprint);
        }
        return hash.Value;
    }

    // Core-owned global section identity (global tokens in slot order plus algorithms), emitted as
    // the GlobalSchemaFingerprint constant and consumed by the combined startup revision.
    internal static ulong ComputeGlobalSchema(
        IEnumerable<SchemaTokenInfo> globalTokens,
        IEnumerable<ThemeAlgorithmInfo> algorithms)
    {
        var hash = new Fnv1a();
        hash.Text("AtomUI.GlobalSchema");
        hash.Number(1);
        var orderedTokens = globalTokens.OrderBy(token => token.Name, StringComparer.Ordinal).ToArray();
        hash.Number((ulong)orderedTokens.Length);
        for (var slot = 0; slot < orderedTokens.Length; slot++)
        {
            var token = orderedTokens[slot];
            hash.Text(token.Name);
            hash.Number((ulong)slot);
            hash.Number((ulong)(int)token.Stage);
            hash.Text(token.ValueType);
            hash.Text(token.DeclaringType);
            hash.Text("AtomUI.Theme.Resources.SharedTokenKind." + token.Name);
        }
        var orderedAlgorithms = algorithms
            .OrderBy(algorithm => algorithm.AlgorithmValue)
            .ThenBy(algorithm => algorithm.TypeName, StringComparer.Ordinal)
            .ToArray();
        hash.Number((ulong)orderedAlgorithms.Length);
        foreach (var algorithm in orderedAlgorithms)
        {
            hash.Text(algorithm.Algorithm);
            hash.Number((ulong)algorithm.AlgorithmValue);
            hash.Number((ulong)algorithm.Revision);
            hash.Text(algorithm.AppearanceEffect);
            hash.Text(algorithm.TypeName);
        }
        return hash.Value;
    }

    private static void WriteContractPrefix(ref Fnv1a hash, IReadOnlyList<string> globalNames)
    {
        hash.Text("AtomUI.GeneratedControlThemeAssetContract");
        hash.Number(1);
        WriteGlobalNames(ref hash, globalNames);
    }

    private static void WriteResourceKeyPrefix(ref Fnv1a hash, IReadOnlyList<string> globalNames)
    {
        hash.Number(1); // Typed asset schema format, independent of the generated registration ABI.
        WriteGlobalNames(ref hash, globalNames);
    }

    private static void WriteGlobalNames(ref Fnv1a hash, IReadOnlyList<string> globalNames)
    {
        hash.Number((ulong)globalNames.Count);
        foreach (var name in globalNames)
        {
            hash.Text(name);
        }
    }

    private static void WriteOwner(ref Fnv1a hash, RegistrationOwner owner)
    {
        hash.Text(owner.Catalog);
        hash.Text(owner.Id);
        hash.Text(owner.Type.MetadataName);
    }

    private struct Fnv1a
    {
        private ulong _hash;

        internal ulong Value => _hash == 0 ? 14695981039346656037UL : _hash;

        internal void Number(ulong value)
        {
            if (_hash == 0)
            {
                _hash = 14695981039346656037UL;
            }

            for (var index = 0; index < 8; index++) { _hash = unchecked((_hash ^ (byte)value) * 1099511628211UL); value >>= 8; }
        }

        internal void Text(string value)
        {
            Number((ulong)value.Length);
            foreach (var ch in value)
            {
                Number(ch);
            }
        }
    }
}
