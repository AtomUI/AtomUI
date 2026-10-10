using System.Collections.Concurrent;
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
    private ConcurrentDictionary<(int ControlSlot, SharedTokenKind Kind), object>? _controlSharedResourceKeys;

    internal ThemeSchemaRegistry(
        IEnumerable<TokenDescriptor> globalTokens,
        IEnumerable<ControlTokenDescriptor> controls,
        IEnumerable<ThemeAlgorithmDescriptor> algorithms,
        IEnumerable<ControlThemeAssetDescriptor>? themeAssets = null)
    {
        ArgumentNullException.ThrowIfNull(globalTokens);
        ArgumentNullException.ThrowIfNull(controls);
        ArgumentNullException.ThrowIfNull(algorithms);
        themeAssets ??= Array.Empty<ControlThemeAssetDescriptor>();

        var globalArray = globalTokens.OrderBy(static descriptor => descriptor.Slot).ToArray();
        var controlInputs = controls.OrderBy(static descriptor => descriptor.Identity.Catalog, StringComparer.Ordinal)
                                    .ThenBy(static descriptor => descriptor.Identity.Id, StringComparer.Ordinal)
                                    .ToArray();
        var algorithmArray = algorithms.OrderBy(static descriptor => descriptor.Algorithm).ToArray();
        var assetArray = themeAssets.OrderBy(
            static descriptor => descriptor.AssetUri.ToString(),
            StringComparer.Ordinal).ToArray();

        ValidateDenseSlots(globalArray, static descriptor => descriptor.Slot, "global Token");

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
        }
        var controlMap = BuildUniqueMap(
            controlArray,
            static descriptor => descriptor.Identity,
            EqualityComparer<ControlTokenIdentity>.Default,
            "Control");

        var algorithmMap = BuildUniqueMap(
            algorithmArray,
            static descriptor => descriptor.Algorithm,
            EqualityComparer<ThemeAlgorithm>.Default,
            "algorithm");

        GlobalTokens          = globalTokensView;
        Controls              = Array.AsReadOnly(controlArray);
        Algorithms            = Array.AsReadOnly(algorithmArray);
        Revision              = ComputeRevision(globalArray, controlArray, algorithmArray, assetArray);
        _globalTokensByName   = globalMap.ToFrozenDictionary(StringComparer.Ordinal);
        _controlsByIdentity   = controlMap.ToFrozenDictionary();
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
        // Registry construction already validated and ordered the global slot table.
        return ComputeResourceKeySchemaFingerprintCore(descriptor, GlobalTokens.Count, GlobalTokens);
    }

    private static ulong ComputeResourceKeySchemaFingerprintCore(
        ControlThemeAssetDescriptor descriptor,
        int globalTokenCount,
        IEnumerable<TokenDescriptor> orderedGlobalTokens)
    {
        var fingerprint = new SchemaFingerprintBuilder();
        AddAssetSchema(ref fingerprint, descriptor);
        fingerprint.Add(globalTokenCount);
        foreach (var token in orderedGlobalTokens)
        {
            fingerprint.Add(token.Name);
        }
        return fingerprint.Value;
    }

    // Generated factories supply their compiled snapshot, never the current registry's globals.
    internal static ulong ComputeGeneratedResourceKeySchemaFingerprint(
        ControlThemeAssetDescriptor descriptor, IReadOnlyList<string> compiledGlobalTokenNames)
    {
        var fingerprint = new SchemaFingerprintBuilder();
        AddAssetSchema(ref fingerprint, descriptor);
        fingerprint.Add(compiledGlobalTokenNames.Count);
        foreach (var name in compiledGlobalTokenNames)
        {
            fingerprint.Add(name);
        }

        return fingerprint.Value;
    }

    internal static ulong ComputeGeneratedContractFingerprint(
        ControlThemeAssetDescriptor descriptor, IReadOnlyList<string> compiledGlobalTokenNames)
    {
        var fingerprint = new SchemaFingerprintBuilder();
        fingerprint.Add("AtomUI.GeneratedControlThemeAssetContract");
        fingerprint.Add(1);
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
        fingerprint.Add(compiledGlobalTokenNames.Count);
        foreach (var name in compiledGlobalTokenNames)
        {
            fingerprint.Add(name);
        }

        return fingerprint.Value;
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

    internal object GetControlSharedResourceKey(int controlSlot, SharedTokenKind kind)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(controlSlot);
        if ((uint)controlSlot >= (uint)Controls.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(controlSlot));
        }

        if (!Enum.IsDefined(kind))
        {
            throw new ArgumentOutOfRangeException(nameof(kind));
        }

        // Production lookup resolves unbound keys directly; allocate bound keys only on request.
        var keys = LazyInitializer.EnsureInitialized(ref _controlSharedResourceKeys);
        return keys.GetOrAdd((controlSlot, kind), static key => new ControlTokenResourceKey(key.ControlSlot, key.Kind));
    }

    internal object GetControlSharedResourceKey(
        ControlTokenIdentity identity,
        SharedTokenKind kind)
    {
        if (!TryGetControl(identity, out var descriptor))
        {
            throw new KeyNotFoundException($"Control Token identity '{identity}' is not registered.");
        }
        return GetControlSharedResourceKey(descriptor.Slot, kind);
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

    private static ThemeSchemaRevision ComputeRevision(
        IReadOnlyList<TokenDescriptor> globalTokens,
        IReadOnlyList<ControlTokenDescriptor> controls,
        IReadOnlyList<ThemeAlgorithmDescriptor> algorithms,
        IReadOnlyList<ControlThemeAssetDescriptor> themeAssets)
    {
        var fingerprint = new SchemaFingerprintBuilder();
        fingerprint.Add(globalTokens.Count);
        foreach (var token in globalTokens)
        {
            AddToken(ref fingerprint, token);
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
                AddToken(ref fingerprint, token);
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

    private static void AddToken(ref SchemaFingerprintBuilder fingerprint, TokenDescriptor token)
    {
        fingerprint.Add(token.Name);
        fingerprint.Add(token.Slot);
        fingerprint.Add((byte)token.Stage);
        fingerprint.Add(token.ValueType.Assembly.GetName().Name ?? string.Empty);
        fingerprint.Add(token.ValueType.FullName ?? token.ValueType.Name);
        AddResourceKey(ref fingerprint, token.ResourceKey);
    }

    private static void AddResourceKey(ref SchemaFingerprintBuilder fingerprint, object resourceKey)
    {
        var keyType = resourceKey.GetType();
        fingerprint.Add(keyType.Assembly.GetName().Name ?? string.Empty);
        fingerprint.Add(keyType.FullName ?? keyType.Name);
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

    private struct SchemaFingerprintBuilder
    {
        private const ulong Offset = 14695981039346656037UL;
        private const ulong Prime = 1099511628211UL;
        private ulong _value;

        internal ulong Value => _value == 0 ? Offset : _value;

        internal void Add(string value)
        {
            Add(value.Length);
            foreach (var character in value)
            {
                Add((ushort)character);
            }
        }

        internal void Add(int value) => Add(unchecked((ulong)(uint)value));

        internal void Add(byte value) => Add((ulong)value);

        internal void Add(ushort value) => Add((ulong)value);

        internal void Add(ulong value)
        {
            if (_value == 0)
            {
                _value = Offset;
            }

            unchecked
            {
                for (var shift = 0; shift < 64; shift += 8)
                {
                    _value ^= (byte)(value >> shift);
                    _value *= Prime;
                }
            }
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
