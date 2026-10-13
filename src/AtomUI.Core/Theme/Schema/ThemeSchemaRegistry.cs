using System.Collections.Frozen;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using AtomUI.Theme.Algorithms;
using AtomUI.Theme.Resources;

namespace AtomUI.Theme.Schema;

internal sealed class ThemeSchemaRegistry
{
    private readonly FrozenDictionary<string, TokenDescriptor> _globalTokensByName;
    private readonly FrozenDictionary<ControlTokenIdentity, ControlTokenDescriptor> _controlsByIdentity;
    private readonly FrozenDictionary<Type, ControlTokenDescriptor> _controlsByType;
    private readonly FrozenDictionary<ThemeAlgorithm, ThemeAlgorithmDescriptor> _algorithmsByAlgorithm;
    private readonly FrozenDictionary<object, int> _controlResourceSlotsByKey;
    private readonly object?[] _sharedResourceKeys;
    private readonly SchemaPrefixState _resourceKeySchemaPrefix;

    internal ThemeSchemaRegistry(
        IEnumerable<TokenDescriptor> globalTokens,
        IEnumerable<ControlTokenDescriptor> controls,
        IEnumerable<ThemeAlgorithmDescriptor> algorithms,
        IEnumerable<ControlThemeAssetDescriptor>? themeAssets = null)
        : this(globalTokens, controls, algorithms, themeAssets, null)
    {
    }

    /// <summary>
    /// 全部输入均携带编译期包指纹的启动路径：Revision 由常量组合而来，不再逐字段遍历。
    /// 内容构建与公共入口完全一致，仅 Revision 计算走组合域。
    /// </summary>
    internal ThemeSchemaRegistry(
        IEnumerable<TokenDescriptor> globalTokens,
        IEnumerable<ControlTokenDescriptor> controls,
        IEnumerable<ThemeAlgorithmDescriptor> algorithms,
        IEnumerable<ControlThemeAssetDescriptor>? themeAssets,
        ulong globalSchemaFingerprint,
        IReadOnlyList<PackageSchemaFingerprint> packageFingerprints)
        : this(globalTokens, controls, algorithms, themeAssets, (globalSchemaFingerprint, packageFingerprints))
    {
    }

    private ThemeSchemaRegistry(
        IEnumerable<TokenDescriptor> globalTokens,
        IEnumerable<ControlTokenDescriptor> controls,
        IEnumerable<ThemeAlgorithmDescriptor> algorithms,
        IEnumerable<ControlThemeAssetDescriptor>? themeAssets,
        (ulong GlobalSchemaFingerprint, IReadOnlyList<PackageSchemaFingerprint> Packages)? combinedRevisionEvidence)
    {
        ArgumentNullException.ThrowIfNull(globalTokens);
        ArgumentNullException.ThrowIfNull(controls);
        ArgumentNullException.ThrowIfNull(algorithms);
        themeAssets ??= Array.Empty<ControlThemeAssetDescriptor>();

        // Generated registrations already arrive canonically ordered; the O(n) check lets that input
        // skip the sort while any other input keeps the original OrderBy behavior unchanged.
        var globalArray = ToOrderedArray(
            globalTokens,
            static descriptor => descriptor.Slot,
            Comparer<int>.Default,
            static source => source.OrderBy(static descriptor => descriptor.Slot).ToArray());
        var controlInputs = ToOrderedArray(
            controls,
            static descriptor => descriptor.Identity,
            ControlIdentityKeyComparer.Instance,
            static source => source.OrderBy(static descriptor => descriptor.Identity.Catalog, StringComparer.Ordinal)
                                   .ThenBy(static descriptor => descriptor.Identity.Id, StringComparer.Ordinal)
                                   .ToArray());
        var algorithmArray = ToOrderedArray(
            algorithms,
            static descriptor => (int)descriptor.Algorithm,
            Comparer<int>.Default,
            static source => source.OrderBy(static descriptor => descriptor.Algorithm).ToArray());
        var assetArray = ToOrderedArray(
            themeAssets,
            static descriptor => descriptor.AssetUri.ToString(),
            StringComparer.Ordinal,
            static source => source.OrderBy(
                    static descriptor => descriptor.AssetUri.ToString(),
                    StringComparer.Ordinal)
                .ToArray());

        ValidateDenseSlots(globalArray, static descriptor => descriptor.Slot, "global Token");

        // Capture the [version head + count + names] prefix once; per-asset registered
        // fingerprints continue from it instead of re-hashing the global names per asset.
        {
            var prefix = new SchemaFingerprintBuilder();
            prefix.Add(1); // Typed asset schema format, independent of the generated registration ABI.
            prefix.Add(globalArray.Length);
            foreach (var descriptor in globalArray)
            {
                prefix.Add(descriptor.Name);
            }
            _resourceKeySchemaPrefix = prefix.Save();
        }

        var globalMap = BuildUniqueMap(
            globalArray,
            static descriptor => descriptor.Name,
            StringComparer.Ordinal,
            "global Token");
        foreach (var descriptor in globalArray)
        {
            if (descriptor.Stage == TokenStage.Control)
            {
                throw new ThemeSchemaException(
                    $"Global Token '{descriptor.Name}' cannot use the Control stage.");
            }
        }

        var controlInputMap = BuildUniqueMap(
            controlInputs,
            static descriptor => descriptor.Identity,
            EqualityComparer<ControlTokenIdentity>.Default,
            "Control");
        foreach (var descriptor in controlInputs)
        {
            ValidateControlDescriptor(descriptor, globalMap);
        }
        ValidateThemeAssets(controlInputMap, assetArray);

        var globalTokensView = Array.AsReadOnly(globalArray);
        var controlArray = new ControlTokenDescriptor[controlInputs.Length];
        for (var slot = 0; slot < controlInputs.Length; slot++)
        {
            controlArray[slot] = controlInputs[slot].Bind(slot);
            controlInputMap[controlArray[slot].Identity] = controlArray[slot];
        }

        var algorithmMap = BuildUniqueMap(
            algorithmArray,
            static descriptor => descriptor.Algorithm,
            EqualityComparer<ThemeAlgorithm>.Default,
            "algorithm");

        GlobalTokens          = globalTokensView;
        Controls              = Array.AsReadOnly(controlArray);
        Algorithms            = Array.AsReadOnly(algorithmArray);
        ThemeAssets           = Array.AsReadOnly(assetArray);
        if (combinedRevisionEvidence is { } evidence)
        {
            ArgumentNullException.ThrowIfNull(evidence.Packages);
            Revision = ComputeCombinedRevision(evidence.GlobalSchemaFingerprint, evidence.Packages);
        }
        else
        {
            Revision = ComputeRevision(globalArray, controlArray, algorithmArray, assetArray);
        }
        _globalTokensByName   = globalMap.ToFrozenDictionary(StringComparer.Ordinal);
        _controlsByIdentity   = controlInputMap.ToFrozenDictionary();
        _controlsByType       = BuildUniqueMap(
                                    controlArray,
                                    static descriptor => descriptor.ControlType,
                                    EqualityComparer<Type>.Default,
                                    "Control CLR type")
                                .ToFrozenDictionary();
        _algorithmsByAlgorithm = algorithmMap.ToFrozenDictionary();
        _controlResourceSlotsByKey = CreateControlResourceSlotMap(controlArray);
        _sharedResourceKeys = CreateSharedResourceKeys(globalArray);
    }

    public IReadOnlyList<TokenDescriptor> GlobalTokens { get; }
    public IReadOnlyList<ControlTokenDescriptor> Controls { get; }
    public IReadOnlyList<ThemeAlgorithmDescriptor> Algorithms { get; }
    public ThemeSchemaRevision Revision { get; }
    // A structural revision does not identify descriptor callbacks or binding ownership.
    // The key carries this small identity instead of retaining the entire registry.
    internal object BindingIdentity { get; } = new();
    internal IReadOnlyList<ControlThemeAssetDescriptor> ThemeAssets { get; }

    // 全局编译只依赖这些不可变描述符，不依赖控件/资产的 registry Revision。
    // 使用精确对象身份也覆盖自定义 setter/factory 的行为，不能仅比较名字或版本号。
    internal bool HasSameGlobalSchema(ThemeSchemaRegistry other)
    {
        if (ReferenceEquals(this, other))
        {
            return true;
        }
        if (GlobalTokens.Count != other.GlobalTokens.Count || Algorithms.Count != other.Algorithms.Count)
        {
            return false;
        }
        for (var index = 0; index < GlobalTokens.Count; index++)
        {
            if (!ReferenceEquals(GlobalTokens[index], other.GlobalTokens[index]))
            {
                return false;
            }
        }
        for (var index = 0; index < Algorithms.Count; index++)
        {
            if (!ReferenceEquals(Algorithms[index], other.Algorithms[index]))
            {
                return false;
            }
        }
        return true;
    }

    internal static ulong ComputeResourceKeySchemaFingerprint(
        ControlThemeAssetDescriptor descriptor,
        IReadOnlyList<TokenDescriptor> globalTokens)
    {
        ArgumentNullException.ThrowIfNull(descriptor);
        ArgumentNullException.ThrowIfNull(globalTokens);

        return ComputeResourceKeySchemaFingerprintCore(
            descriptor, globalTokens.Count, globalTokens.OrderBy(static token => token.Slot));
    }

    internal ulong ComputeRegisteredResourceKeySchemaFingerprint(ControlThemeAssetDescriptor descriptor)
    {
        ArgumentNullException.ThrowIfNull(descriptor);
        // Registry construction captured the [version head + global names] prefix from the
        // validated, slot-ordered global table; continue from it instead of re-hashing per asset.
        var fingerprint = SchemaFingerprintBuilder.Restore(_resourceKeySchemaPrefix);
        AddAssetSchemaCore(ref fingerprint, descriptor);
        return fingerprint.Value;
    }

    private static ulong ComputeResourceKeySchemaFingerprintCore(
        ControlThemeAssetDescriptor descriptor,
        int globalTokenCount,
        IEnumerable<TokenDescriptor> orderedGlobalTokens)
    {
        // Field order must stay byte-identical to the generated variant below and to the
        // prefix produced by HashGlobalTokenNames: [version head] → [global names] → [asset schema].
        var fingerprint = new SchemaFingerprintBuilder();
        fingerprint.Add(1); // Typed asset schema format, independent of the generated registration ABI.
        fingerprint.Add(globalTokenCount);
        foreach (var token in orderedGlobalTokens)
        {
            fingerprint.Add(token.Name);
        }
        AddAssetSchemaCore(ref fingerprint, descriptor);
        return fingerprint.Value;
    }

    // Generated factories supply their compiled snapshot, never the current registry's globals.
    internal static ulong ComputeGeneratedResourceKeySchemaFingerprint(
        ControlThemeAssetDescriptor descriptor, IReadOnlyList<string> compiledGlobalTokenNames)
    {
        var fingerprint = new SchemaFingerprintBuilder();
        fingerprint.Add(1); // Typed asset schema format, independent of the generated registration ABI.
        fingerprint.Add(compiledGlobalTokenNames.Count);
        foreach (var name in compiledGlobalTokenNames)
        {
            fingerprint.Add(name);
        }
        AddAssetSchemaCore(ref fingerprint, descriptor);

        return fingerprint.Value;
    }

    // Field order must stay byte-identical to AtomUI.Generator RegistrationFingerprint.ComputeContract:
    // [contract version head] → [global names shared prefix] → [asset-specific fields].
    internal static ulong ComputeGeneratedContractFingerprint(
        ControlThemeAssetDescriptor descriptor, IReadOnlyList<string> compiledGlobalTokenNames)
    {
        var fingerprint = new SchemaFingerprintBuilder();
        fingerprint.Add("AtomUI.GeneratedControlThemeAssetContract");
        fingerprint.Add(1);
        fingerprint.Add(compiledGlobalTokenNames.Count);
        foreach (var name in compiledGlobalTokenNames)
        {
            fingerprint.Add(name);
        }
        AddGeneratedContractAssetSchema(ref fingerprint, descriptor);

        return fingerprint.Value;
    }

    /// <summary>
    /// 对编译期全局 Token 名列表计算两个生成指纹（契约 / 资源键）的公共前缀中间状态，
    /// 供差分测试核对与生成代码的前缀续算使用。
    /// </summary>
    internal static (SchemaPrefixState ContractPrefix, SchemaPrefixState ResourceKeyPrefix) HashGlobalTokenNames(
        IReadOnlyList<string> globalTokenNames)
    {
        ArgumentNullException.ThrowIfNull(globalTokenNames);

        var contract = new SchemaFingerprintBuilder();
        contract.Add("AtomUI.GeneratedControlThemeAssetContract");
        contract.Add(1);
        contract.Add(globalTokenNames.Count);
        foreach (var name in globalTokenNames)
        {
            contract.Add(name);
        }

        var resourceKey = new SchemaFingerprintBuilder();
        resourceKey.Add(1); // Typed asset schema format, independent of the generated registration ABI.
        resourceKey.Add(globalTokenNames.Count);
        foreach (var name in globalTokenNames)
        {
            resourceKey.Add(name);
        }

        return (contract.Save(), resourceKey.Save());
    }

    internal static void AddGeneratedContractAssetSchema(
        ref SchemaFingerprintBuilder fingerprint,
        ControlThemeAssetDescriptor descriptor)
    {
        fingerprint.Add(descriptor.AssetId);
        fingerprint.Add(descriptor.AssetUri.ToString());
        fingerprint.Add(descriptor.ExportedThemes.Count);
        // This layer is independently ordered by metadata names, not reference/runtime AQN order.
        foreach (var export in descriptor.ExportedThemes.OrderBy(static value => value.TargetType.FullName, StringComparer.Ordinal)
                     .ThenBy(static value => value.ResourceKey is Type ? 0 : 1)
                     .ThenBy(static value => value.ResourceKey is Type type ? type.FullName : (string)value.ResourceKey, StringComparer.Ordinal))
        {
            fingerprint.Add(export.TargetType.FullName!);
            fingerprint.Add(export.ResourceKey is Type ? (byte)1 : (byte)0);
            fingerprint.Add(export.ResourceKey is Type type ? type.FullName! : (string)export.ResourceKey);
        }
        fingerprint.Add(descriptor.RequiredTokenOwners.Count);
        foreach (var owner in descriptor.RequiredTokenOwners.OrderBy(static value => value.Catalog, StringComparer.Ordinal).ThenBy(static value => value.Id, StringComparer.Ordinal))
        {
            fingerprint.Add(owner.Catalog);
            fingerprint.Add(owner.Id);
            fingerprint.Add(owner.OwnerType!.FullName!);
        }
        fingerprint.Add(descriptor.SemanticThemeBindings.Count);
        foreach (var binding in descriptor.SemanticThemeBindings.OrderBy(static value => value.OwnerIdentity.Catalog, StringComparer.Ordinal)
                     .ThenBy(static value => value.OwnerIdentity.Id, StringComparer.Ordinal)
                     .ThenBy(static value => value.PropertyName, StringComparer.Ordinal)
                     .ThenBy(static value => value.TargetType.FullName, StringComparer.Ordinal))
        {
            fingerprint.Add(binding.OwnerIdentity.Catalog);
            fingerprint.Add(binding.OwnerIdentity.Id);
            fingerprint.Add(binding.OwnerIdentity.OwnerType!.FullName!);
            fingerprint.Add(binding.PropertyName);
            fingerprint.Add(binding.TargetType.FullName!);
        }
    }


    internal bool TryGetGlobalToken(
        string name,
        [NotNullWhen(true)] out TokenDescriptor? descriptor)
    {
        return _globalTokensByName.TryGetValue(name, out descriptor);
    }

    internal bool TryGetControl(
        ControlTokenIdentity identity,
        [NotNullWhen(true)] out ControlTokenDescriptor? descriptor)
    {
        if (_controlsByIdentity.TryGetValue(identity, out descriptor) &&
            ControlTokenIdentityCanonicalizer.Matches(identity, descriptor.ControlType))
        {
            return true;
        }
        descriptor = null;
        return false;
    }

    internal bool TryGetControl(
        Type controlType,
        [NotNullWhen(true)] out ControlTokenDescriptor? descriptor)
    {
        ArgumentNullException.ThrowIfNull(controlType);
        return _controlsByType.TryGetValue(controlType, out descriptor);
    }

    internal bool TryGetAlgorithm(
        ThemeAlgorithm algorithm,
        [NotNullWhen(true)] out ThemeAlgorithmDescriptor? descriptor)
    {
        return _algorithmsByAlgorithm.TryGetValue(algorithm, out descriptor);
    }

    internal bool TryGetControlResourceSlot(object resourceKey, out int controlSlot)
    {
        return _controlResourceSlotsByKey.TryGetValue(resourceKey, out controlSlot);
    }

    internal object GetSharedResourceKey(SharedTokenKind kind)
    {
        var kindSlot = (int)kind;
        if ((uint)kindSlot >= (uint)_sharedResourceKeys.Length ||
            _sharedResourceKeys[kindSlot] is not { } key)
        {
            throw new ArgumentOutOfRangeException(nameof(kind));
        }
        return key;
    }

    private static object?[] CreateSharedResourceKeys(
        IReadOnlyList<TokenDescriptor> globalTokens)
    {
        var kinds = Enum.GetValues<SharedTokenKind>();
        var length = kinds.Length == 0 ? 0 : kinds.Max(static kind => (int)kind) + 1;
        var result = new object?[length];
        foreach (var descriptor in globalTokens)
        {
            if (descriptor.ResourceKey is SharedTokenKind kind)
            {
                result[(int)kind] = descriptor.ResourceKey;
            }
        }
        return result;
    }

    private static FrozenDictionary<object, int> CreateControlResourceSlotMap(
        IReadOnlyList<ControlTokenDescriptor> controls)
    {
        var result = new Dictionary<object, int>();
        foreach (var control in controls)
        {
            foreach (var token in control.OwnTokens)
            {
                if (!result.TryAdd(token.ResourceKey, control.Slot))
                {
                    throw new ThemeSchemaException(
                        $"Control Token resource key '{token.ResourceKey}' is registered more than once.");
                }
            }
        }

        return result.ToFrozenDictionary();
    }

    private static void ValidateThemeAssets(
        IReadOnlyDictionary<ControlTokenIdentity, ControlTokenDescriptor> controlsByIdentity,
        IReadOnlyList<ControlThemeAssetDescriptor> themeAssets)
    {
        foreach (var asset in themeAssets)
        {
            ArgumentNullException.ThrowIfNull(asset);
            foreach (var identity in asset.RequiredTokenOwners)
            {
                if (!controlsByIdentity.TryGetValue(identity, out var descriptor) ||
                    !ControlTokenIdentityCanonicalizer.Matches(identity, descriptor.ControlType))
                {
                    throw new ThemeSchemaException(
                        $"Control theme asset '{asset.AssetUri}' requires unregistered or incorrectly owned Token identity '{identity}'.");
                }
            }
        }
    }

    private static void ValidateControlDescriptor(
        ControlTokenDescriptor descriptor,
        IReadOnlyDictionary<string, TokenDescriptor> globalTokens)
    {
        if (!ControlTokenIdentityCanonicalizer.Matches(descriptor.Identity, descriptor.ControlType))
        {
            throw new ThemeSchemaException($"Control Token identity '{descriptor.Identity}' has an incorrect owner.");
        }
        ValidateDenseSlots(descriptor.OwnTokens, static token => token.Slot, $"{descriptor.Identity} own Token");
        var ownNames = new HashSet<string>(StringComparer.Ordinal);
        foreach (var token in descriptor.OwnTokens)
        {
            if (token.Stage != TokenStage.Control)
            {
                throw new ThemeSchemaException(
                    $"Control Token '{descriptor.Identity}:{token.Name}' must use the Control stage.");
            }

            if (!ownNames.Add(token.Name))
            {
                throw new ThemeSchemaException(
                    $"Control '{descriptor.Identity}' contains duplicate own Token '{token.Name}'.");
            }
            if (globalTokens.ContainsKey(token.Name))
            {
                throw new ThemeSchemaException(
                    $"Control '{descriptor.Identity}' Own Token '{token.Name}' conflicts with a Global Token.");
            }
        }
    }

    private static Dictionary<TKey, TDescriptor> BuildUniqueMap<TDescriptor, TKey>(
        IEnumerable<TDescriptor> descriptors,
        Func<TDescriptor, TKey> keySelector,
        IEqualityComparer<TKey> comparer,
        string kind)
        where TDescriptor : class
        where TKey : notnull
    {
        var map = new Dictionary<TKey, TDescriptor>(comparer);
        foreach (var descriptor in descriptors)
        {
            var key = keySelector(descriptor);
            if (!map.TryAdd(key, descriptor))
            {
                throw new ThemeSchemaException($"Duplicate {kind} identity '{key}'.");
            }
        }

        return map;
    }

    private static void ValidateDenseSlots<TDescriptor>(
        IReadOnlyList<TDescriptor> descriptors,
        Func<TDescriptor, int> slotSelector,
        string kind)
    {
        for (var index = 0; index < descriptors.Count; index++)
        {
            var slot = slotSelector(descriptors[index]);
            if (slot != index)
            {
                throw new ThemeSchemaException(
                    $"The {kind} slot table must be contiguous from zero; expected {index}, found {slot}.");
            }
        }
    }

    private static TDescriptor[] ToOrderedArray<TDescriptor, TKey>(
        IEnumerable<TDescriptor> source,
        Func<TDescriptor, TKey> keySelector,
        IComparer<TKey> comparer,
        Func<TDescriptor[], TDescriptor[]> sortFallback)
    {
        var array = source.ToArray();
        if (array.Length == 0)
        {
            return array;
        }
        var previous = keySelector(array[0]);
        for (var index = 1; index < array.Length; index++)
        {
            var current = keySelector(array[index]);
            if (comparer.Compare(previous, current) > 0)
            {
                return sortFallback(array);
            }
            previous = current;
        }
        return array;
    }

    private sealed class ControlIdentityKeyComparer : IComparer<ControlTokenIdentity>
    {
        internal static readonly ControlIdentityKeyComparer Instance = new();

        public int Compare(ControlTokenIdentity left, ControlTokenIdentity right)
        {
            var result = string.CompareOrdinal(left.Catalog, right.Catalog);
            return result != 0 ? result : string.CompareOrdinal(left.Id, right.Id);
        }
    }

    // Combined domain: startup registries whose inputs are entirely compile-time fingerprinted
    // compose the revision from the package constants instead of re-walking every schema field.
    // In-process determinism and content discrimination are the only requirements; combined
    // values never mix with full-walk values in a comparable way.
    private static ThemeSchemaRevision ComputeCombinedRevision(
        ulong globalSchemaFingerprint,
        IReadOnlyList<PackageSchemaFingerprint> packageFingerprints)
    {
        var fingerprint = new SchemaFingerprintBuilder();
        fingerprint.Add("AtomUI.PackageSchemaRevision");
        fingerprint.Add(2); // Combined schema format version, independent of the full-walk format.
        fingerprint.Add(globalSchemaFingerprint);
        fingerprint.Add(packageFingerprints.Count);
        foreach (var package in packageFingerprints)
        {
            fingerprint.Add(package.PackageId);
            fingerprint.Add(package.Fingerprint);
        }
        return new ThemeSchemaRevision(fingerprint.Value);
    }

    private static ThemeSchemaRevision ComputeRevision(
        IReadOnlyList<TokenDescriptor> globalTokens,
        IReadOnlyList<ControlTokenDescriptor> controls,
        IReadOnlyList<ThemeAlgorithmDescriptor> algorithms,
        IReadOnlyList<ControlThemeAssetDescriptor> themeAssets)
    {
        var fingerprint = new SchemaFingerprintBuilder();
        // Only this revision calculation owns the cache; no runtime Types are retained globally.
        var typeNames = new Dictionary<Type, (string AssemblyName, string TypeName)>();
        fingerprint.Add(globalTokens.Count);
        foreach (var token in globalTokens)
        {
            AddToken(ref fingerprint, token, typeNames);
        }

        fingerprint.Add(controls.Count);
        foreach (var control in controls)
        {
            fingerprint.Add(control.Identity.Catalog);
            fingerprint.Add(control.Identity.Id);
            fingerprint.Add(control.ControlType.AssemblyQualifiedName!);
            fingerprint.Add(control.Slot);
            fingerprint.Add(control.OwnTokens.Count);
            foreach (var token in control.OwnTokens)
            {
                AddToken(ref fingerprint, token, typeNames);
            }
        }

        fingerprint.Add(algorithms.Count);
        foreach (var algorithm in algorithms)
        {
            fingerprint.Add((int)algorithm.Algorithm);
            fingerprint.Add(algorithm.Revision);
            fingerprint.Add((byte)algorithm.AppearanceEffect);
        }

        fingerprint.Add(themeAssets.Count);
        foreach (var asset in themeAssets)
        {
            AddAssetSchema(ref fingerprint, asset);
            fingerprint.Add(asset.ResourceKeySchemaFingerprint);
        }

        return new ThemeSchemaRevision(fingerprint.Value);
    }

    private static void AddAssetSchema(ref SchemaFingerprintBuilder fingerprint, ControlThemeAssetDescriptor asset)
    {
        fingerprint.Add(1); // Typed asset schema format, independent of the generated registration ABI.
        AddAssetSchemaCore(ref fingerprint, asset);
    }

    internal static void AddAssetSchemaCore(ref SchemaFingerprintBuilder fingerprint, ControlThemeAssetDescriptor asset)
    {
        fingerprint.Add(asset.AssetId);
        fingerprint.Add(asset.AssetUri.ToString());
        fingerprint.Add(asset.ExportedThemes.Count);
        foreach (var export in asset.ExportedThemes)
        {
            fingerprint.Add(export.TargetType.AssemblyQualifiedName!);
            fingerprint.Add(export.ResourceKey is Type ? (byte)1 : (byte)0);
            fingerprint.Add(export.ResourceKey is Type type ? type.AssemblyQualifiedName! : (string)export.ResourceKey);
        }
        fingerprint.Add(asset.RequiredTokenOwners.Count);
        foreach (var identity in asset.RequiredTokenOwners)
        {
            fingerprint.Add(identity.Catalog);
            fingerprint.Add(identity.Id);
            fingerprint.Add(identity.OwnerType!.AssemblyQualifiedName!);
        }
        fingerprint.Add(asset.SemanticThemeBindings.Count);
        foreach (var binding in asset.SemanticThemeBindings)
        {
            fingerprint.Add(binding.OwnerIdentity.Catalog);
            fingerprint.Add(binding.OwnerIdentity.Id);
            fingerprint.Add(binding.OwnerIdentity.OwnerType!.AssemblyQualifiedName!);
            fingerprint.Add(binding.PropertyName);
            fingerprint.Add(binding.TargetType.AssemblyQualifiedName!);
        }
    }

    private static void AddToken(
        ref SchemaFingerprintBuilder fingerprint,
        TokenDescriptor token,
        Dictionary<Type, (string AssemblyName, string TypeName)> typeNames)
    {
        fingerprint.Add(token.Name);
        fingerprint.Add(token.Slot);
        fingerprint.Add((byte)token.Stage);
        AddTypeName(ref fingerprint, token.ValueType, typeNames);
        AddResourceKey(ref fingerprint, token.ResourceKey, typeNames);
    }

    private static void AddTypeName(
        ref SchemaFingerprintBuilder fingerprint,
        Type type,
        Dictionary<Type, (string AssemblyName, string TypeName)> typeNames)
    {
        if (!typeNames.TryGetValue(type, out var names))
        {
            names = (type.Assembly.GetName().Name ?? string.Empty, type.FullName ?? type.Name);
            typeNames.Add(type, names);
        }
        fingerprint.Add(names.AssemblyName);
        fingerprint.Add(names.TypeName);
    }

    private static void AddResourceKey(
        ref SchemaFingerprintBuilder fingerprint,
        object resourceKey,
        Dictionary<Type, (string AssemblyName, string TypeName)> typeNames)
    {
        var keyType = resourceKey.GetType();
        AddTypeName(ref fingerprint, keyType, typeNames);
        switch (resourceKey)
        {
            case Enum enumValue:
                fingerprint.Add(unchecked((ulong)Convert.ToInt64(enumValue, CultureInfo.InvariantCulture)));
                break;
            case string stringValue:
                fingerprint.Add(stringValue);
                break;
            default:
                throw new ThemeSchemaException(
                    $"Resource key type '{keyType.FullName}' must be an enum or string to participate in a deterministic schema.");
        }
    }
}

internal sealed class ThemeSchemaException : Exception
{
    internal ThemeSchemaException(string message)
        : base(message)
    {
    }
}
