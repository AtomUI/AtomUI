using System.Runtime.CompilerServices;
using AtomUI.Theme.Configuration;
using AtomUI.Theme.Definitions;
using AtomUI.Theme.Algorithms;
using AtomUI.Theme.Schema;

namespace AtomUI.Theme.Compilation;

internal readonly record struct AlgorithmCacheKey(
    ThemeAlgorithm Algorithm,
    int Revision,
    ThemeAppearanceEffect AppearanceEffect)
{
    internal static AlgorithmCacheKey Create(ThemeAlgorithmDescriptor descriptor)
    {
        return new AlgorithmCacheKey(
            descriptor.Algorithm,
            descriptor.Revision,
            descriptor.AppearanceEffect);
    }
}

internal readonly record struct TokenCacheKey(
    string Name,
    int Slot,
    TokenStage Stage,
    string CanonicalValue)
{
    internal static TokenCacheKey Create(NormalizedTokenValue token)
    {
        return new TokenCacheKey(
            token.Descriptor.Name,
            token.Descriptor.Slot,
            token.Descriptor.Stage,
            token.CanonicalValue);
    }

    internal static TokenCacheKey Create(BoundTokenValue token)
    {
        return new TokenCacheKey(
            token.Descriptor.Name,
            token.Descriptor.Slot,
            token.Descriptor.Stage,
            token.Descriptor.Format(token.Value));
    }
}

internal sealed class NormalizedControlConfigCacheKey : IEquatable<NormalizedControlConfigCacheKey>
{
    // 输入在规范化边界冻结；弱键不延长配置或其 registry 的生命周期。
    private static readonly ConditionalWeakTable<NormalizedControlThemeConfig, NormalizedControlConfigCacheKey> s_keys = new();
    private readonly AlgorithmCacheKey[] _algorithms;
    private readonly TokenCacheKey[] _globalTokens;
    private readonly TokenCacheKey[] _ownTokens;
    private readonly int _hashCode;

    private NormalizedControlConfigCacheKey(NormalizedControlThemeConfig config)
    {
        Identity      = config.Identity;
        AlgorithmMode = config.AlgorithmMode;
        _algorithms   = CacheKeyArray.Algorithms(config.Algorithms);
        _globalTokens = CacheKeyArray.Tokens(config.GlobalTokens);
        _ownTokens    = CacheKeyArray.Tokens(config.OwnTokens);
        _hashCode     = ComputeHashCode();
        EstimatedRetainedBytes = ThemeRetainedBytesEstimator.Add(
            CacheKeyArray.Estimate(_algorithms, _globalTokens, _ownTokens), CacheKeyArray.EstimateIdentity(Identity));
        EstimatedInputRetainedBytes = ThemeRetainedBytesEstimator.EstimateConfig(config);
    }

    internal long EstimatedRetainedBytes { get; }
    internal long EstimatedInputRetainedBytes { get; }
    internal ControlTokenIdentity Identity { get; }
    internal ControlAlgorithmMode AlgorithmMode { get; }

    internal static NormalizedControlConfigCacheKey Create(NormalizedControlThemeConfig config)
    {
        return s_keys.GetValue(config, static value => new NormalizedControlConfigCacheKey(value));
    }

    public bool Equals(NormalizedControlConfigCacheKey? other)
    {
        return ReferenceEquals(this, other) ||
               other is not null &&
               Identity == other.Identity &&
               AlgorithmMode == other.AlgorithmMode &&
               _algorithms.AsSpan().SequenceEqual(other._algorithms) &&
               _globalTokens.AsSpan().SequenceEqual(other._globalTokens) &&
               _ownTokens.AsSpan().SequenceEqual(other._ownTokens);
    }

    public override bool Equals(object? obj) => Equals(obj as NormalizedControlConfigCacheKey);

    public override int GetHashCode() => _hashCode;

    private int ComputeHashCode()
    {
        var hash = new HashCode();
        hash.Add(Identity);
        hash.Add(AlgorithmMode);
        CacheKeyArray.Add(ref hash, _algorithms);
        CacheKeyArray.Add(ref hash, _globalTokens);
        CacheKeyArray.Add(ref hash, _ownTokens);
        return hash.ToHashCode();
    }
}

internal sealed class NormalizedThemeConfigCacheKey : IEquatable<NormalizedThemeConfigCacheKey>
{
    private static readonly ConditionalWeakTable<NormalizedThemeConfig, NormalizedThemeConfigCacheKey> s_keys = new();
    private readonly AlgorithmCacheKey[] _algorithms;
    private readonly TokenCacheKey[] _globalTokens;
    private readonly NormalizedControlConfigCacheKey[] _controls;
    private readonly int _hashCode;

    private NormalizedThemeConfigCacheKey(NormalizedThemeConfig config)
    {
        Inherit              = config.Inherit;
        AlgorithmsSpecified = config.AlgorithmsSpecified;
        Fingerprint          = config.Fingerprint;
        _algorithms          = CacheKeyArray.Algorithms(config.Algorithms);
        _globalTokens        = CacheKeyArray.Tokens(config.GlobalTokens);
        _controls            = config.Controls.Select(NormalizedControlConfigCacheKey.Create).ToArray();
        _hashCode            = ComputeHashCode();
        EstimatedRetainedBytes = CacheKeyArray.Estimate(_algorithms, _globalTokens);
        EstimatedInputRetainedBytes = ThemeRetainedBytesEstimator.EstimateConfig(config);
        foreach (var control in _controls)
        {
            EstimatedRetainedBytes = ThemeRetainedBytesEstimator.Add(EstimatedRetainedBytes, ThemeRetainedBytesEstimator.Add(8, control.EstimatedRetainedBytes));
        }
    }

    internal long EstimatedRetainedBytes { get; }
    internal long EstimatedInputRetainedBytes { get; }
    internal AlgorithmCacheKey[] AlgorithmKeys => _algorithms;
    internal TokenCacheKey[] GlobalTokenKeys => _globalTokens;
    internal bool Inherit { get; }
    internal bool AlgorithmsSpecified { get; }
    internal ThemeConfigFingerprint Fingerprint { get; }

    internal static NormalizedThemeConfigCacheKey Create(NormalizedThemeConfig config)
    {
        return s_keys.GetValue(config, static value => new NormalizedThemeConfigCacheKey(value));
    }

    public bool Equals(NormalizedThemeConfigCacheKey? other)
    {
        if (ReferenceEquals(this, other))
        {
            return true;
        }
        if (other is null ||
            Fingerprint != other.Fingerprint ||
            Inherit != other.Inherit ||
            AlgorithmsSpecified != other.AlgorithmsSpecified ||
            !_algorithms.AsSpan().SequenceEqual(other._algorithms) ||
            !_globalTokens.AsSpan().SequenceEqual(other._globalTokens) ||
            _controls.Length != other._controls.Length)
        {
            return false;
        }

        for (var index = 0; index < _controls.Length; index++)
        {
            if (!_controls[index].Equals(other._controls[index]))
            {
                return false;
            }
        }

        return true;
    }

    public override bool Equals(object? obj) => Equals(obj as NormalizedThemeConfigCacheKey);

    public override int GetHashCode() => _hashCode;

    private int ComputeHashCode()
    {
        var hash = new HashCode();
        hash.Add(Fingerprint);
        hash.Add(Inherit);
        hash.Add(AlgorithmsSpecified);
        CacheKeyArray.Add(ref hash, _algorithms);
        CacheKeyArray.Add(ref hash, _globalTokens);
        foreach (var control in _controls)
        {
            hash.Add(control);
        }
        return hash.ToHashCode();
    }
}

internal sealed class ControlDefinitionCacheKey : IEquatable<ControlDefinitionCacheKey>
{
    private readonly AlgorithmCacheKey[] _algorithms;
    private readonly TokenCacheKey[] _globalTokens;
    private readonly TokenCacheKey[] _ownTokens;
    private readonly int _hashCode;

    internal ControlDefinitionCacheKey(ControlThemeDefinition definition)
    {
        Identity      = definition.Descriptor.Identity;
        AlgorithmMode = definition.AlgorithmMode;
        _algorithms   = CacheKeyArray.Algorithms(definition.Algorithms);
        _globalTokens = CacheKeyArray.Tokens(definition.GlobalTokens);
        _ownTokens    = CacheKeyArray.Tokens(definition.OwnTokens);
        _hashCode     = ComputeHashCode();
        EstimatedRetainedBytes = ThemeRetainedBytesEstimator.Add(
            CacheKeyArray.Estimate(_algorithms, _globalTokens, _ownTokens), CacheKeyArray.EstimateIdentity(Identity));
    }

    internal long EstimatedRetainedBytes { get; }
    internal ControlTokenIdentity Identity { get; }
    internal ControlAlgorithmMode AlgorithmMode { get; }

    public bool Equals(ControlDefinitionCacheKey? other)
    {
        return ReferenceEquals(this, other) ||
               other is not null &&
               Identity == other.Identity &&
               AlgorithmMode == other.AlgorithmMode &&
               _algorithms.AsSpan().SequenceEqual(other._algorithms) &&
               _globalTokens.AsSpan().SequenceEqual(other._globalTokens) &&
               _ownTokens.AsSpan().SequenceEqual(other._ownTokens);
    }

    public override bool Equals(object? obj) => Equals(obj as ControlDefinitionCacheKey);

    public override int GetHashCode() => _hashCode;

    private int ComputeHashCode()
    {
        var hash = new HashCode();
        hash.Add(Identity);
        hash.Add(AlgorithmMode);
        CacheKeyArray.Add(ref hash, _algorithms);
        CacheKeyArray.Add(ref hash, _globalTokens);
        CacheKeyArray.Add(ref hash, _ownTokens);
        return hash.ToHashCode();
    }
}

internal sealed class ThemeDefinitionCacheKey : IEquatable<ThemeDefinitionCacheKey>
{
    // Bound definition 是绑定阶段封存的输入，后续修订必须创建新实例。
    private static readonly ConditionalWeakTable<BoundThemeDefinition, ThemeDefinitionCacheKey> s_keys = new();
    private readonly AlgorithmCacheKey[] _algorithms;
    private readonly TokenCacheKey[] _tokens;
    private readonly ControlDefinitionCacheKey[] _controls;
    private readonly int _hashCode;

    private ThemeDefinitionCacheKey(BoundThemeDefinition definition)
    {
        Id                  = definition.Id;
        Name                = definition.Name;
        DeclaredAppearance  = definition.DeclaredAppearance;
        EffectiveAppearance = definition.EffectiveAppearance;
        IsDefault           = definition.IsDefault;
        _algorithms         = CacheKeyArray.Algorithms(definition.Algorithms);
        _tokens             = CacheKeyArray.Tokens(definition.Tokens);
        _controls           = definition.Controls
                                        .OrderBy(static control => control.Descriptor.Identity.Catalog, StringComparer.Ordinal)
                                        .ThenBy(static control => control.Descriptor.Identity.Id, StringComparer.Ordinal)
                                        .Select(static control => new ControlDefinitionCacheKey(control))
                                        .ToArray();
        _hashCode = ComputeHashCode();
        EstimatedInputRetainedBytes = ThemeRetainedBytesEstimator.EstimateDefinition(definition);
        EstimatedRetainedBytes = ThemeRetainedBytesEstimator.Add(CacheKeyArray.Estimate(_algorithms, _tokens),
            ThemeRetainedBytesEstimator.EstimateValue(Id) + ThemeRetainedBytesEstimator.EstimateValue(Name));
        foreach (var control in _controls)
        {
            EstimatedRetainedBytes = ThemeRetainedBytesEstimator.Add(EstimatedRetainedBytes, ThemeRetainedBytesEstimator.Add(8, control.EstimatedRetainedBytes));
        }
    }

    internal long EstimatedRetainedBytes { get; }
    internal long EstimatedInputRetainedBytes { get; }
    internal string Id { get; }
    internal string Name { get; }
    internal ThemeAppearance DeclaredAppearance { get; }
    internal ThemeAppearance EffectiveAppearance { get; }
    internal bool IsDefault { get; }

    internal static ThemeDefinitionCacheKey Create(BoundThemeDefinition definition)
    {
        return s_keys.GetValue(definition, static value => new ThemeDefinitionCacheKey(value));
    }

    public bool Equals(ThemeDefinitionCacheKey? other)
    {
        if (ReferenceEquals(this, other))
        {
            return true;
        }
        if (other is null ||
            !string.Equals(Id, other.Id, StringComparison.Ordinal) ||
            !string.Equals(Name, other.Name, StringComparison.Ordinal) ||
            DeclaredAppearance != other.DeclaredAppearance ||
            EffectiveAppearance != other.EffectiveAppearance ||
            IsDefault != other.IsDefault ||
            !_algorithms.AsSpan().SequenceEqual(other._algorithms) ||
            !_tokens.AsSpan().SequenceEqual(other._tokens) ||
            _controls.Length != other._controls.Length)
        {
            return false;
        }

        for (var index = 0; index < _controls.Length; index++)
        {
            if (!_controls[index].Equals(other._controls[index]))
            {
                return false;
            }
        }

        return true;
    }

    public override bool Equals(object? obj) => Equals(obj as ThemeDefinitionCacheKey);

    public override int GetHashCode() => _hashCode;

    private int ComputeHashCode()
    {
        var hash = new HashCode();
        hash.Add(Id, StringComparer.Ordinal);
        hash.Add(Name, StringComparer.Ordinal);
        hash.Add(DeclaredAppearance);
        hash.Add(EffectiveAppearance);
        hash.Add(IsDefault);
        CacheKeyArray.Add(ref hash, _algorithms);
        CacheKeyArray.Add(ref hash, _tokens);
        foreach (var control in _controls)
        {
            hash.Add(control);
        }
        return hash.ToHashCode();
    }
}

internal sealed class GlobalCompilationCacheKey : IEquatable<GlobalCompilationCacheKey>
{
    private readonly AlgorithmCacheKey[] _algorithms;
    private readonly TokenCacheKey[] _globalTokens;
    private readonly int _hashCode;

    private GlobalCompilationCacheKey(ThemeCompileInput input)
    {
        RegistryRevision = input.Registry.Revision;
        var configKey     = NormalizedThemeConfigCacheKey.Create(input.EffectiveConfig);
        _algorithms       = configKey.AlgorithmKeys;
        _globalTokens     = configKey.GlobalTokenKeys;
        _hashCode         = ComputeHashCode();
        EstimatedRetainedBytes = CacheKeyArray.Estimate(_algorithms, _globalTokens);
    }

    internal long EstimatedRetainedBytes { get; }
    internal ThemeSchemaRevision RegistryRevision { get; }

    internal static GlobalCompilationCacheKey Create(ThemeCompileInput input)
    {
        return new GlobalCompilationCacheKey(input);
    }

    public bool Equals(GlobalCompilationCacheKey? other)
    {
        return ReferenceEquals(this, other) ||
               other is not null &&
               RegistryRevision == other.RegistryRevision &&
               _algorithms.AsSpan().SequenceEqual(other._algorithms) &&
               _globalTokens.AsSpan().SequenceEqual(other._globalTokens);
    }

    public override bool Equals(object? obj) => Equals(obj as GlobalCompilationCacheKey);

    public override int GetHashCode() => _hashCode;

    private int ComputeHashCode()
    {
        var hash = new HashCode();
        hash.Add(RegistryRevision);
        CacheKeyArray.Add(ref hash, _algorithms);
        CacheKeyArray.Add(ref hash, _globalTokens);
        return hash.ToHashCode();
    }
}

internal readonly struct ThemeSnapshotCacheKey : IEquatable<ThemeSnapshotCacheKey>
{
    private readonly ThemeDefinitionCacheKey _definition;
    private readonly NormalizedThemeConfigCacheKey _effectiveConfig;

    private ThemeSnapshotCacheKey(ThemeCompileInput input)
    {
        var appearance = ResolveAppearance(input.EffectiveConfig.Algorithms);
        Fingerprint = ThemeContentFingerprint.Compute(
            input.DefinitionRevision,
            input.EffectiveConfig,
            input.Registry.Revision,
            appearance);
        DefinitionRevision = input.DefinitionRevision;
        RegistryRevision   = input.Registry.Revision;
        _definition        = ThemeDefinitionCacheKey.Create(input.Definition);
        _effectiveConfig   = NormalizedThemeConfigCacheKey.Create(input.EffectiveConfig);
    }

    internal long EstimatedRetainedBytes => ThemeRetainedBytesEstimator.Add(
        ThemeRetainedBytesEstimator.Add(64, _definition.EstimatedRetainedBytes), _effectiveConfig.EstimatedRetainedBytes);
    internal ThemeContentFingerprint Fingerprint { get; }
    internal ThemeDefinitionRevision DefinitionRevision { get; }
    internal ThemeSchemaRevision RegistryRevision { get; }

    internal static ThemeSnapshotCacheKey Create(ThemeCompileInput input)
    {
        return new ThemeSnapshotCacheKey(input);
    }

    public bool Equals(ThemeSnapshotCacheKey other)
    {
        return Fingerprint == other.Fingerprint &&
               DefinitionRevision == other.DefinitionRevision &&
               RegistryRevision == other.RegistryRevision &&
               _definition.Equals(other._definition) &&
               _effectiveConfig.Equals(other._effectiveConfig);
    }

    public override bool Equals(object? obj)
    {
        return obj is ThemeSnapshotCacheKey other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Fingerprint, RegistryRevision, _definition, _effectiveConfig);
    }

    private static ThemeAppearance ResolveAppearance(
        IReadOnlyList<ThemeAlgorithmDescriptor> algorithms)
    {
        var appearance = ThemeAppearance.Light;
        foreach (var algorithm in algorithms)
        {
            appearance = algorithm.AppearanceEffect switch
            {
                ThemeAppearanceEffect.Light => ThemeAppearance.Light,
                ThemeAppearanceEffect.Dark  => ThemeAppearance.Dark,
                _                           => appearance
            };
        }
        return appearance;
    }
}

internal readonly struct ControlCompilationCacheKey : IEquatable<ControlCompilationCacheKey>
{
    private readonly GlobalCompilationCacheKey _global;
    private readonly NormalizedControlConfigCacheKey? _controlConfig;

    private ControlCompilationCacheKey(
        GlobalCompilationCacheKey global,
        ControlTokenDescriptor descriptor,
        NormalizedControlThemeConfig? controlConfig,
        ThemeAppearance appearance)
    {
        ArgumentNullException.ThrowIfNull(global);

        _global       = global;
        Identity      = descriptor.Identity;
        ControlSlot   = descriptor.Slot;
        Appearance    = appearance;
        _controlConfig = controlConfig is null
            ? null
            : NormalizedControlConfigCacheKey.Create(controlConfig);
    }

    internal ControlTokenIdentity Identity { get; }
    internal long EstimatedRetainedBytes => ThemeRetainedBytesEstimator.Add(
        ThemeRetainedBytesEstimator.Add(64 + CacheKeyArray.EstimateIdentity(Identity), _global.EstimatedRetainedBytes),
        _controlConfig?.EstimatedRetainedBytes ?? 0);
    internal int ControlSlot { get; }
    internal ThemeAppearance Appearance { get; }

    internal static ControlCompilationCacheKey Create(
        GlobalCompilationCacheKey global,
        ControlTokenDescriptor descriptor,
        NormalizedControlThemeConfig? controlConfig,
        ThemeAppearance appearance)
    {
        return new ControlCompilationCacheKey(global, descriptor, controlConfig, appearance);
    }

    public bool Equals(ControlCompilationCacheKey other)
    {
        return ControlSlot == other.ControlSlot &&
               Appearance == other.Appearance &&
               Identity == other.Identity &&
               _global.Equals(other._global) &&
               Equals(_controlConfig, other._controlConfig);
    }

    public override bool Equals(object? obj)
    {
        return obj is ControlCompilationCacheKey other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(_global, Identity, ControlSlot, Appearance, _controlConfig);
    }
}

internal static class CacheKeyArray
{
    internal static AlgorithmCacheKey[] Algorithms(IReadOnlyList<ThemeAlgorithmDescriptor> algorithms)
    {
        var result = new AlgorithmCacheKey[algorithms.Count];
        for (var index = 0; index < result.Length; index++)
        {
            result[index] = AlgorithmCacheKey.Create(algorithms[index]);
        }
        return result;
    }

    internal static TokenCacheKey[] Tokens(IReadOnlyList<NormalizedTokenValue> tokens)
    {
        // Normalized 配置保证 schema slot 有序且唯一，不重复执行排序。
        var result = new TokenCacheKey[tokens.Count];
        for (var index = 0; index < result.Length; index++)
        {
            result[index] = TokenCacheKey.Create(tokens[index]);
        }
        return result;
    }

    internal static TokenCacheKey[] Tokens(IReadOnlyList<BoundTokenValue> tokens)
    {
        var result = new TokenCacheKey[tokens.Count];
        for (var index = 0; index < result.Length; index++)
        {
            result[index] = TokenCacheKey.Create(tokens[index]);
        }
        Array.Sort(result, CompareToken);
        return result;
    }

    internal static long EstimateIdentity(ControlTokenIdentity identity)
    {
        return ThemeRetainedBytesEstimator.Add(
            ThemeRetainedBytesEstimator.EstimateValue(identity.Catalog),
            ThemeRetainedBytesEstimator.EstimateValue(identity.Id));
    }

    internal static long Estimate(AlgorithmCacheKey[] algorithms, TokenCacheKey[] tokens,
        TokenCacheKey[]? ownTokens = null)
    {
        var bytes = 96L + algorithms.Length * 16L;
        bytes = ThemeRetainedBytesEstimator.Add(bytes, EstimateTokens(tokens));
        if (ownTokens is not null)
        {
            bytes = ThemeRetainedBytesEstimator.Add(bytes, EstimateTokens(ownTokens));
        }
        return bytes;
    }

    private static long EstimateTokens(TokenCacheKey[] tokens)
    {
        var bytes = 32L + tokens.Length * 32L;
        foreach (var token in tokens)
        {
            bytes = ThemeRetainedBytesEstimator.Add(bytes, ThemeRetainedBytesEstimator.EstimateValue(token.Name));
            bytes = ThemeRetainedBytesEstimator.Add(bytes, ThemeRetainedBytesEstimator.EstimateValue(token.CanonicalValue));
        }
        return bytes;
    }

    internal static void Add<T>(ref HashCode hash, IReadOnlyList<T> values)
    {
        hash.Add(values.Count);
        foreach (var value in values)
        {
            hash.Add(value);
        }
    }

    private static int CompareToken(TokenCacheKey left, TokenCacheKey right)
    {
        var slot = left.Slot.CompareTo(right.Slot);
        return slot != 0
            ? slot
            : string.Compare(left.Name, right.Name, StringComparison.Ordinal);
    }
}
