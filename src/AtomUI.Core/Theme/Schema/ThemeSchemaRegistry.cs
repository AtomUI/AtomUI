using System.Collections.Frozen;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.CompilerServices;
using AtomUI.Theme.Algorithms;
using AtomUI.Theme.Resources;
using AtomUI.Registration;

namespace AtomUI.Theme.Schema;

internal sealed class ThemeSchemaRegistry
{
    private readonly StartupPackageIndex? _startupIndex;
    private readonly FrozenDictionary<string, TokenDescriptor> _globalTokensByName;
    private readonly FrozenDictionary<ControlTokenIdentity, ControlTokenDescriptor> _controlsByIdentity;
    private readonly FrozenDictionary<Type, ControlTokenDescriptor> _controlsByType;
    private readonly FrozenDictionary<ThemeAlgorithm, ThemeAlgorithmDescriptor> _algorithmsByAlgorithm;
    // The control stage state is null when no control stage is registered, so that the generic machinery over the
    // value-tuple key is never instantiated (nor jitted) for the built-in themes. Set only by AttachControlAlgorithms,
    // called from the constructor.
    private FrozenDictionary<(ThemeAlgorithm Algorithm, ControlTokenIdentity Control), ControlTokenAlgorithmDescriptor>?
        _controlAlgorithmsByKey;
    private ReadOnlyCollection<ControlTokenAlgorithmDescriptor>? _controlAlgorithms;
    // A field rather than an auto-property's hidden one so that AttachControlAlgorithms and
    // FoldControlAlgorithmsIntoRevision can fold the stages into it. Constructor-only: written only by the
    // constructors and those two helpers they call.
    private ThemeSchemaRevision _revision;
    private readonly FrozenDictionary<object, int> _controlResourceSlotsByKey;
    private readonly object?[] _sharedResourceKeys;
    private readonly SchemaPrefixState _resourceKeySchemaPrefix;

    internal ThemeSchemaRegistry(IEnumerable<TokenDescriptor> globalTokens, StartupPackageIndex index,
        IEnumerable<ThemeAlgorithmDescriptor> algorithms)
        : this(globalTokens, index, algorithms, null)
    {
    }

    // Overloads instead of optional parameters keep the existing constructor shapes callable by exact signature.
    internal ThemeSchemaRegistry(IEnumerable<TokenDescriptor> globalTokens, StartupPackageIndex index,
        IEnumerable<ThemeAlgorithmDescriptor> algorithms,
        IEnumerable<ControlTokenAlgorithmDescriptor>? controlAlgorithms)
        // Chains straight to the private constructor: a forwarding overload in between would be one more jitted
        // method on every startup.
        : this(globalTokens, Array.Empty<ControlTokenDescriptor>(), algorithms, null, null, controlAlgorithms)
    {
        _startupIndex = index;
        Controls = new StartupControlList(index);
        // Neither the compiled stamp nor Core's global fingerprint sees algorithms or control stages registered
        // by packages.
        _revision = FoldPackageAlgorithms(index.CompiledStamp is { } stamp ? new ThemeSchemaRevision(stamp) :
            ComputeCombinedRevision(Generated.AtomUICore.GeneratedThemeSchema.GlobalSchemaFingerprint,
            index.Packages.Select(package => new PackageSchemaFingerprint(package.Definition.Id,
                package.Definition.Data.SchemaFingerprint)).Concat(index.ManualSchemaStamp is { } manualStamp
                    ? [new PackageSchemaFingerprint("$manual", manualStamp)] : Array.Empty<PackageSchemaFingerprint>()).ToArray()),
            Algorithms);
        if (HasControlAlgorithms)
        {
            FoldControlAlgorithmsIntoRevision();
        }
    }

    internal bool HasSameControlSlot(ThemeSchemaRegistry previous, int slot)
    {
        if ((uint)slot >= (uint)previous.Controls.Count)
        {
            return false;
        }
        if (_startupIndex is { } current && previous._startupIndex is { } old)
        {
            return ReferenceEquals(current.Controls[slot], old.Controls[slot]);
        }
        var descriptor = Controls[slot];
        var previousDescriptor = previous.Controls[slot];
        return descriptor.Identity == previousDescriptor.Identity && descriptor.ControlType == previousDescriptor.ControlType &&
            ReferenceEquals(descriptor.CompilationIdentity, previousDescriptor.CompilationIdentity);
    }

    internal bool UsesStartupIndex => _startupIndex is not null;
    internal bool MatchesControlRequirement(ControlTokenIdentity identity, Type ownerType,
        string ownerAssemblyName, string ownerMetadataName)
    {
        if (_startupIndex is { } index)
        {
            return index.MatchesControlRequirement(identity, ownerType, ownerAssemblyName, ownerMetadataName);
        }
        return _controlsByIdentity.TryGetValue(identity, out var descriptor) &&
               descriptor.ControlType == ownerType &&
               ControlTokenIdentityCanonicalizer.Matches(identity, ownerType) &&
               string.Equals(ownerType.FullName, ownerMetadataName, StringComparison.Ordinal) &&
               string.Equals(ownerType.Assembly.GetName().Name, ownerAssemblyName, StringComparison.Ordinal);
    }

    internal void NotifyControlFirstUse(int slot) => _startupIndex?.NotifyFirstUse(slot);
    internal ControlTokenIdentity GetControlIdentity(int slot) =>
        _startupIndex is { } index ? index.Controls[slot].Identity : Controls[slot].Identity;
    internal bool ControlHasOwnTokens(int slot) =>
        _startupIndex is { } index ? index.Controls[slot].Data.OwnTokens.Count != 0 : Controls[slot].HasOwnTokens;

    private sealed class StartupControlList(StartupPackageIndex index) : IReadOnlyList<ControlTokenDescriptor>
    {
        public int Count => index.ControlCount;
        public ControlTokenDescriptor this[int slot] => index.Controls[slot].GetDescriptor();
        public IEnumerator<ControlTokenDescriptor> GetEnumerator()
        {
            for (var slot = 0; slot < Count; slot++)
            {
                yield return this[slot];
            }
        }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }

    internal ThemeSchemaRegistry(
        IEnumerable<TokenDescriptor> globalTokens,
        IEnumerable<ControlTokenDescriptor> controls,
        IEnumerable<ThemeAlgorithmDescriptor> algorithms,
        IEnumerable<ControlThemeAssetDescriptor>? themeAssets = null)
        : this(globalTokens, controls, algorithms, themeAssets, null, null)
    {
    }

    internal ThemeSchemaRegistry(
        IEnumerable<TokenDescriptor> globalTokens,
        IEnumerable<ControlTokenDescriptor> controls,
        IEnumerable<ThemeAlgorithmDescriptor> algorithms,
        IEnumerable<ControlThemeAssetDescriptor>? themeAssets,
        IEnumerable<ControlTokenAlgorithmDescriptor>? controlAlgorithms)
        : this(globalTokens, controls, algorithms, themeAssets, null, controlAlgorithms)
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
        : this(globalTokens, controls, algorithms, themeAssets, (globalSchemaFingerprint, packageFingerprints), null)
    {
    }

    internal ThemeSchemaRegistry(
        IEnumerable<TokenDescriptor> globalTokens,
        IEnumerable<ControlTokenDescriptor> controls,
        IEnumerable<ThemeAlgorithmDescriptor> algorithms,
        IEnumerable<ControlThemeAssetDescriptor>? themeAssets,
        ulong globalSchemaFingerprint,
        IReadOnlyList<PackageSchemaFingerprint> packageFingerprints,
        IEnumerable<ControlTokenAlgorithmDescriptor>? controlAlgorithms)
        : this(globalTokens, controls, algorithms, themeAssets, (globalSchemaFingerprint, packageFingerprints),
            controlAlgorithms)
    {
    }

    private ThemeSchemaRegistry(
        IEnumerable<TokenDescriptor> globalTokens,
        IEnumerable<ControlTokenDescriptor> controls,
        IEnumerable<ThemeAlgorithmDescriptor> algorithms,
        IEnumerable<ControlThemeAssetDescriptor>? themeAssets,
        (ulong GlobalSchemaFingerprint, IReadOnlyList<PackageSchemaFingerprint> Packages)? combinedRevisionEvidence,
        IEnumerable<ControlTokenAlgorithmDescriptor>? controlAlgorithms)
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
            static descriptor => descriptor.Algorithm,
            AlgorithmKeyComparer.Instance,
            static source => source.OrderBy(static descriptor => descriptor.Algorithm, AlgorithmKeyComparer.Instance)
                                   .ToArray());
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
        // Validated and ordered here, where it always was; null for no control stage (null or empty input), which
        // then never enters the fold or the map (AttachControlAlgorithms below).
        var controlAlgorithmArray = controlAlgorithms is null ? null : OrderControlAlgorithms(controlAlgorithms);

        GlobalTokens          = globalTokensView;
        Controls              = Array.AsReadOnly(controlArray);
        Algorithms            = Array.AsReadOnly(algorithmArray);
        ThemeAssets           = Array.AsReadOnly(assetArray);
        if (combinedRevisionEvidence is { } evidence)
        {
            ArgumentNullException.ThrowIfNull(evidence.Packages);
            _revision = FoldPackageAlgorithms(
                ComputeCombinedRevision(evidence.GlobalSchemaFingerprint, evidence.Packages), algorithmArray);
        }
        else
        {
            _revision = ComputeRevision(globalArray, controlArray, algorithmArray, assetArray);
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
        if (controlAlgorithmArray is not null)
        {
            HasControlAlgorithms = true;
            AttachControlAlgorithms(controlAlgorithmArray);
        }
        _controlResourceSlotsByKey = CreateControlResourceSlotMap(controlArray);
        _sharedResourceKeys = CreateSharedResourceKeys(globalArray);
    }

    public IReadOnlyList<TokenDescriptor> GlobalTokens { get; }
    public IReadOnlyList<ControlTokenDescriptor> Controls { get; }
    public IReadOnlyList<ThemeAlgorithmDescriptor> Algorithms { get; }

    /// <summary>
    /// The registered control token stages, ordered by algorithm identity and then control identity. A stage whose
    /// algorithm or control is not registered is kept here but never called.
    /// </summary>
    internal IReadOnlyList<ControlTokenAlgorithmDescriptor> ControlAlgorithms =>
        (IReadOnlyList<ControlTokenAlgorithmDescriptor>?)_controlAlgorithms ?? Array.Empty<ControlTokenAlgorithmDescriptor>();

    /// <summary>
    /// Whether any control token stage is registered. A plain field so that the check costs neither a call nor a
    /// jitted getter: every no-stage path tests it before calling into the control stage machinery.
    /// </summary>
    internal readonly bool HasControlAlgorithms;
    public ThemeSchemaRevision Revision => _revision;
    // A structural revision does not identify descriptor callbacks or binding ownership.
    // The key carries this small identity instead of retaining the entire registry.
    internal object BindingIdentity { get; } = new();
    internal IReadOnlyList<ControlThemeAssetDescriptor> ThemeAssets { get; }

    /// <summary>
    /// 启动 Prepare 专用：只含全局 Token 与算法，不含控件描述。主题定义绑定在此模式下跳过控件段，
    /// 因为控件段只影响控件层 Token，全局层结果与完整 registry 相同。
    /// </summary>
    internal bool IsGlobalOnly { get; private init; }

    internal static ThemeSchemaRegistry CreateGlobalOnly(IReadOnlyList<ThemeAlgorithmDescriptor> packageAlgorithms) =>
        new(Generated.AtomUICore.GeneratedThemeSchema.GetGlobalTokens(),
            Array.Empty<ControlTokenDescriptor>(),
            CombineAlgorithms(packageAlgorithms))
        {
            IsGlobalOnly = true
        };

    /// <summary>
    /// Returns Core's generated algorithms followed by the package algorithms registered through the builder.
    /// The descriptor instances are kept as-is so that registries built from the same registrations share them by reference.
    /// </summary>
    internal static IReadOnlyList<ThemeAlgorithmDescriptor> CombineAlgorithms(
        IReadOnlyList<ThemeAlgorithmDescriptor> packageAlgorithms)
    {
        ArgumentNullException.ThrowIfNull(packageAlgorithms);
        var core = Generated.AtomUICore.GeneratedThemeSchema.GetAlgorithms();
        return packageAlgorithms.Count == 0 ? core : core.Concat(packageAlgorithms).ToArray();
    }

    // 全局编译只依赖这些不可变描述符，不依赖控件/资产的 registry Revision。
    // 使用精确对象身份也覆盖自定义 setter/factory 的行为，不能仅比较名字或版本号。
    // 新增的算法不影响已编译的全局结果（链上的算法由配置按身份携带），因此只要求 previous 的
    // 每个算法描述符在这里按引用存在。
    internal bool ExtendsGlobalSchemaOf(ThemeSchemaRegistry previous)
    {
        ArgumentNullException.ThrowIfNull(previous);
        if (ReferenceEquals(this, previous))
        {
            return true;
        }
        if (GlobalTokens.Count != previous.GlobalTokens.Count || Algorithms.Count < previous.Algorithms.Count)
        {
            return false;
        }
        for (var index = 0; index < GlobalTokens.Count; index++)
        {
            if (!ReferenceEquals(GlobalTokens[index], previous.GlobalTokens[index]))
            {
                return false;
            }
        }
        if (Algorithms.Count == previous.Algorithms.Count)
        {
            // Common case: the same algorithm table, compared in order without lookups.
            var sameOrder = true;
            for (var index = 0; index < Algorithms.Count; index++)
            {
                if (!ReferenceEquals(Algorithms[index], previous.Algorithms[index]))
                {
                    sameOrder = false;
                    break;
                }
            }
            if (sameOrder)
            {
                return true;
            }
        }
        foreach (var algorithm in previous.Algorithms)
        {
            if (!_algorithmsByAlgorithm.TryGetValue(algorithm.Algorithm, out var current) ||
                !ReferenceEquals(current, algorithm))
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
        if (_startupIndex is not null)
        {
            return _startupIndex.TryGetControl(identity, out descriptor);
        }
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
        if (_startupIndex is not null)
        {
            return _startupIndex.TryGetControl(controlType, out descriptor);
        }
        return _controlsByType.TryGetValue(controlType, out descriptor);
    }

    internal bool TryGetAlgorithm(
        ThemeAlgorithm algorithm,
        [NotNullWhen(true)] out ThemeAlgorithmDescriptor? descriptor)
    {
        return _algorithmsByAlgorithm.TryGetValue(algorithm, out descriptor);
    }

    internal bool TryGetControlAlgorithm(
        ThemeAlgorithm algorithm,
        ControlTokenIdentity control,
        [NotNullWhen(true)] out ControlTokenAlgorithmDescriptor? descriptor)
    {
        if (_controlAlgorithmsByKey is null)
        {
            descriptor = null;
            return false;
        }
        return _controlAlgorithmsByKey.TryGetValue((algorithm, control), out descriptor);
    }

    /// <summary>
    /// Validates the control token stages a package append brings, where this registry is the appended one and
    /// <paramref name="previous"/> the committed one. A new stage is accepted only if its algorithm or its control
    /// was not registered in <paramref name="previous"/>; a stage for a registered algorithm and a registered
    /// control would change committed values.
    /// </summary>
    /// <remarks>
    /// Stages committed earlier are carried forward and not validated again. A stage whose algorithm or control is
    /// registered nowhere is accepted, as at startup: it cannot be called until a later append registers the
    /// missing side, which is then a new registration.
    /// </remarks>
    internal void ValidateAppendedControlAlgorithms(ThemeSchemaRegistry previous)
    {
        ArgumentNullException.ThrowIfNull(previous);
        foreach (var descriptor in ControlAlgorithms)
        {
            if (previous.TryGetControlAlgorithm(descriptor.Algorithm, descriptor.Control, out var committed) &&
                ReferenceEquals(committed, descriptor))
            {
                continue;
            }
            if (previous.TryGetAlgorithm(descriptor.Algorithm, out _) && previous.TryGetControl(descriptor.Control, out _))
            {
                throw new ThemeLoadException(
                    $"Package append cannot add a control algorithm for the already registered algorithm " +
                    $"'{descriptor.Algorithm}' and control '{descriptor.Control}'.");
            }
        }
    }

    internal bool TryGetControlResourceSlot(object resourceKey, out int controlSlot)
    {
        if (_startupIndex is not null)
        {
            return _startupIndex.TryGetOwnResourceSlot(resourceKey, out controlSlot);
        }
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

    private sealed class AlgorithmKeyComparer : IComparer<ThemeAlgorithm>
    {
        internal static readonly AlgorithmKeyComparer Instance = new();

        public int Compare(ThemeAlgorithm left, ThemeAlgorithm right) => ThemeAlgorithm.Compare(left, right);
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

    // Compile-time revisions (startup image stamp, Core's global fingerprint) only cover Core's
    // algorithms. Algorithms registered by packages are folded in here; a Core-only table keeps
    // the original revision unchanged.
    private static ThemeSchemaRevision FoldPackageAlgorithms(
        ThemeSchemaRevision revision,
        IReadOnlyList<ThemeAlgorithmDescriptor> algorithms)
    {
        var core = Generated.AtomUICore.GeneratedThemeSchema.GetAlgorithms();
        var packageCount = 0;
        foreach (var algorithm in algorithms)
        {
            if (!IsCoreAlgorithm(algorithm, core))
            {
                packageCount++;
            }
        }
        if (packageCount == 0)
        {
            return revision;
        }
        var fingerprint = new SchemaFingerprintBuilder();
        fingerprint.Add("AtomUI.PackageAlgorithmRevision");
        fingerprint.Add(1);
        fingerprint.Add(revision.Value);
        fingerprint.Add(packageCount);
        foreach (var algorithm in algorithms)
        {
            if (IsCoreAlgorithm(algorithm, core))
            {
                continue;
            }
            fingerprint.Add(algorithm.Algorithm.Catalog);
            fingerprint.Add(algorithm.Algorithm.Id);
            fingerprint.Add(algorithm.Revision);
            fingerprint.Add((byte)algorithm.AppearanceEffect);
        }
        return new ThemeSchemaRevision(fingerprint.Value);
    }

    // Null for an empty set: no control stage is represented without any of the stage state.
    private static ControlTokenAlgorithmDescriptor[]? OrderControlAlgorithms(
        IEnumerable<ControlTokenAlgorithmDescriptor> controlAlgorithms)
    {
        var array = controlAlgorithms.ToArray();
        if (array.Length == 0)
        {
            return null;
        }
        foreach (var descriptor in array)
        {
            if (descriptor is null)
            {
                throw new ArgumentException("Control token algorithm descriptors cannot contain null.",
                    nameof(controlAlgorithms));
            }
        }
        Array.Sort(array, static (left, right) =>
        {
            var result = ThemeAlgorithm.Compare(left.Algorithm, right.Algorithm);
            return result != 0 ? result : ControlIdentityKeyComparer.Instance.Compare(left.Control, right.Control);
        });
        return array;
    }

    // Everything the constructor does for a non-empty stage set, kept out of the constructor, which runs (and is
    // jitted) at every startup: the read-only view, the revision fold, then the lookup map, which throws on duplicates
    // at the point where the constructor always built it.
    [MethodImpl(MethodImplOptions.NoInlining)]
    private void AttachControlAlgorithms(ControlTokenAlgorithmDescriptor[] controlAlgorithms)
    {
        _controlAlgorithms      = Array.AsReadOnly(controlAlgorithms);
        _revision               = FoldControlAlgorithms(_revision, controlAlgorithms);
        _controlAlgorithmsByKey = BuildControlAlgorithmMap(controlAlgorithms);
    }

    // The startup-index constructor replaces the revision with one from compile-time stamps; the stages are folded in
    // again here, out of that constructor.
    [MethodImpl(MethodImplOptions.NoInlining)]
    private void FoldControlAlgorithmsIntoRevision() =>
        _revision = FoldControlAlgorithms(_revision, _controlAlgorithms!);

    private static FrozenDictionary<(ThemeAlgorithm Algorithm, ControlTokenIdentity Control), ControlTokenAlgorithmDescriptor>
        BuildControlAlgorithmMap(IReadOnlyList<ControlTokenAlgorithmDescriptor> controlAlgorithms)
    {
        var map = new Dictionary<(ThemeAlgorithm Algorithm, ControlTokenIdentity Control), ControlTokenAlgorithmDescriptor>(
            controlAlgorithms.Count);
        foreach (var descriptor in controlAlgorithms)
        {
            if (!map.TryAdd((descriptor.Algorithm, descriptor.Control), descriptor))
            {
                throw new ThemeSchemaException(
                    $"Duplicate control algorithm '{descriptor.Algorithm}' for control '{descriptor.Control}'.");
            }
        }
        return map.ToFrozenDictionary();
    }

    // Neither compile-time revisions nor the full walk know control stages. They are folded in on every path,
    // in (algorithm, control) order. Called only when stages are registered; with none the caller keeps the
    // revision exactly what it was.
    private static ThemeSchemaRevision FoldControlAlgorithms(
        ThemeSchemaRevision revision,
        IReadOnlyList<ControlTokenAlgorithmDescriptor> controlAlgorithms)
    {
        var fingerprint = new SchemaFingerprintBuilder();
        fingerprint.Add("AtomUI.ControlAlgorithmRevision");
        fingerprint.Add(1);
        fingerprint.Add(revision.Value);
        fingerprint.Add(controlAlgorithms.Count);
        foreach (var descriptor in controlAlgorithms)
        {
            fingerprint.Add(descriptor.Algorithm.Catalog);
            fingerprint.Add(descriptor.Algorithm.Id);
            fingerprint.Add(descriptor.Control.Catalog);
            fingerprint.Add(descriptor.Control.Id);
            fingerprint.Add(descriptor.Revision);
        }
        return new ThemeSchemaRevision(fingerprint.Value);
    }

    private static bool IsCoreAlgorithm(ThemeAlgorithmDescriptor algorithm, IReadOnlyList<ThemeAlgorithmDescriptor> core)
    {
        for (var index = 0; index < core.Count; index++)
        {
            if (ReferenceEquals(core[index], algorithm))
            {
                return true;
            }
        }
        return false;
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
            fingerprint.Add(algorithm.Algorithm.Catalog);
            fingerprint.Add(algorithm.Algorithm.Id);
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
