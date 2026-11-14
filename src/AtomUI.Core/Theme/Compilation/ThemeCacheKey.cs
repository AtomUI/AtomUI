using System.Runtime.CompilerServices;
using AtomUI.Theme.Configuration;
using AtomUI.Theme.Definitions;
using AtomUI.Theme.Schema;

namespace AtomUI.Theme.Compilation;

// The descriptor's CompilationIdentity is unique per descriptor instance and the descriptor's algorithm
// identity is immutable, so the identity itself is not stored: it would only add two string comparisons
// and 16 bytes per key without changing equality.
internal readonly record struct AlgorithmCacheKey(
    int Revision,
    ThemeAppearanceEffect AppearanceEffect,
    object CompilationIdentity)
{
    internal static AlgorithmCacheKey Create(ThemeAlgorithmDescriptor descriptor)
    {
        return new AlgorithmCacheKey(
            descriptor.Revision,
            descriptor.AppearanceEffect,
            descriptor.CompilationIdentity);
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
            token.CanonicalValue);
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
    private readonly string? _controlRevision;
    private readonly object? _controlBindingIdentity;

    private NormalizedThemeConfigCacheKey(NormalizedThemeConfig config)
    {
        Inherit              = config.Inherit;
        AlgorithmsSpecified = config.AlgorithmsSpecified;
        Fingerprint          = config.Fingerprint;
        _algorithms          = CacheKeyArray.Algorithms(config.Algorithms);
        _globalTokens        = CacheKeyArray.Tokens(config.GlobalTokens);
        _controlRevision = config.ControlRevision;
        _controlBindingIdentity = config.ControlBindingIdentity;
        _controls = config.HasDeferredControls ? Array.Empty<NormalizedControlConfigCacheKey>() : config.Controls.Select(NormalizedControlConfigCacheKey.Create).ToArray();
        _hashCode            = ComputeHashCode();
        EstimatedRetainedBytes = ThemeRetainedBytesEstimator.Add(CacheKeyArray.Estimate(_algorithms, _globalTokens),
            ThemeRetainedBytesEstimator.EstimateValue(_controlRevision));
        EstimatedRetainedBytes = ThemeRetainedBytesEstimator.Add(EstimatedRetainedBytes,
            ThemeRetainedBytesEstimator.EstimateKnownDeferredBindingInputs(_controlBindingIdentity));
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
            _controlRevision != other._controlRevision ||
            !Equals(_controlBindingIdentity, other._controlBindingIdentity) ||
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
        hash.Add(_controlRevision, StringComparer.Ordinal);
        hash.Add(_controlBindingIdentity);
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
    private static readonly ConditionalWeakTable<PreparedThemeDefinition, ThemeDefinitionCacheKey> s_keys = new();
    private readonly AlgorithmCacheKey[] _algorithms;
    private readonly TokenCacheKey[] _tokens;
    private readonly ControlDefinitionCacheKey[] _controls;
    private readonly int _hashCode;
    private readonly string? _controlRevision;
    private readonly object? _controlBindingIdentity;

    private ThemeDefinitionCacheKey(PreparedThemeDefinition definition)
    {
        Id                  = definition.Id;
        Name                = definition.Name;
        DeclaredAppearance  = definition.DeclaredAppearance;
        EffectiveAppearance = definition.EffectiveAppearance;
        IsDefault           = definition.IsDefault;
        _algorithms         = CacheKeyArray.Algorithms(definition.Algorithms);
        _tokens             = CacheKeyArray.Tokens(definition.Tokens);
        _controlRevision = definition.ControlRevision;
        _controlBindingIdentity = _controlRevision is null ? null : definition;
        _controls           = _controlRevision is not null ? Array.Empty<ControlDefinitionCacheKey>() : definition.Controls
                                        .OrderBy(static control => control.Descriptor.Identity.Catalog, StringComparer.Ordinal)
                                        .ThenBy(static control => control.Descriptor.Identity.Id, StringComparer.Ordinal)
                                        .Select(static control => new ControlDefinitionCacheKey(control))
                                        .ToArray();
        _hashCode = ComputeHashCode();
        EstimatedInputRetainedBytes = ThemeRetainedBytesEstimator.EstimateDefinition(definition);
        EstimatedRetainedBytes = ThemeRetainedBytesEstimator.Add(CacheKeyArray.Estimate(_algorithms, _tokens),
            ThemeRetainedBytesEstimator.EstimateValue(Id) + ThemeRetainedBytesEstimator.EstimateValue(Name) + ThemeRetainedBytesEstimator.EstimateValue(_controlRevision));
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

    internal static ThemeDefinitionCacheKey Create(PreparedThemeDefinition definition)
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
            _controlRevision != other._controlRevision ||
            !ReferenceEquals(_controlBindingIdentity, other._controlBindingIdentity) ||
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
        hash.Add(_controlRevision, StringComparer.Ordinal);
        hash.Add(_controlBindingIdentity);
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
        RegistryIdentity = input.Registry.BindingIdentity;
        var configKey     = NormalizedThemeConfigCacheKey.Create(input.EffectiveConfig);
        _algorithms       = configKey.AlgorithmKeys;
        _globalTokens     = configKey.GlobalTokenKeys;
        _hashCode         = ComputeHashCode();
        EstimatedRetainedBytes = CacheKeyArray.Estimate(_algorithms, _globalTokens);
    }

    internal long EstimatedRetainedBytes { get; }
    internal ThemeSchemaRevision RegistryRevision { get; }
    private object RegistryIdentity { get; }

    internal static GlobalCompilationCacheKey Create(ThemeCompileInput input)
    {
        return new GlobalCompilationCacheKey(input);
    }

    public bool Equals(GlobalCompilationCacheKey? other)
    {
        return ReferenceEquals(this, other) ||
               other is not null &&
               RegistryRevision == other.RegistryRevision &&
               ReferenceEquals(RegistryIdentity, other.RegistryIdentity) &&
               _algorithms.AsSpan().SequenceEqual(other._algorithms) &&
               _globalTokens.AsSpan().SequenceEqual(other._globalTokens);
    }

    public override bool Equals(object? obj) => Equals(obj as GlobalCompilationCacheKey);

    public override int GetHashCode() => _hashCode;

    private int ComputeHashCode()
    {
        var hash = new HashCode();
        hash.Add(RegistryRevision);
        hash.Add(RegistryIdentity);
        CacheKeyArray.Add(ref hash, _algorithms);
        CacheKeyArray.Add(ref hash, _globalTokens);
        return hash.ToHashCode();
    }
}

internal readonly struct ThemeSnapshotCacheKey : IEquatable<ThemeSnapshotCacheKey>
{
    private readonly ThemeDefinitionCacheKey _definition;
    private readonly NormalizedThemeConfigCacheKey _effectiveConfig;
    private readonly object _registryIdentity;

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
        _registryIdentity  = input.Registry.BindingIdentity;
        _definition        = ThemeDefinitionCacheKey.Create(input.Definition);
        _effectiveConfig   = NormalizedThemeConfigCacheKey.Create(input.EffectiveConfig);
    }

    internal long EstimatedRetainedBytes => ThemeRetainedBytesEstimator.Add(
        ThemeRetainedBytesEstimator.Add(64, _definition.EstimatedRetainedBytes), _effectiveConfig.EstimatedRetainedBytes);
    internal long EstimatedInputRetainedBytes => ThemeRetainedBytesEstimator.Add(
        _definition.EstimatedInputRetainedBytes, _effectiveConfig.EstimatedInputRetainedBytes);
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
               ReferenceEquals(_registryIdentity, other._registryIdentity) &&
               _definition.Equals(other._definition) &&
               _effectiveConfig.Equals(other._effectiveConfig);
    }

    public override bool Equals(object? obj)
    {
        return obj is ThemeSnapshotCacheKey other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Fingerprint, RegistryRevision, _registryIdentity, _definition, _effectiveConfig);
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

/// <summary>
/// The compilation identity of a control whose chain runs control token stages: the control descriptor's identity
/// plus the revision and identity of every participating stage, in chain order. Compared by value.
/// </summary>
/// <remarks>
/// A control without control stages keeps the bare descriptor identity in its key, so its key is exactly what it was
/// before control stages existed; only controls with stages pay for this object.
/// </remarks>
internal sealed class ControlStageCompilationIdentity : IEquatable<ControlStageCompilationIdentity>
{
    private readonly object _controlIdentity;
    private readonly (int Revision, object CompilationIdentity)[] _stages;
    private readonly int _hashCode;

    internal ControlStageCompilationIdentity(
        object controlIdentity,
        IReadOnlyList<ControlTokenAlgorithmDescriptor> stages)
    {
        _controlIdentity = controlIdentity;
        _stages          = new (int, object)[stages.Count];
        var hash = new HashCode();
        hash.Add(controlIdentity);
        hash.Add(_stages.Length);
        for (var index = 0; index < _stages.Length; index++)
        {
            _stages[index] = (stages[index].Revision, stages[index].CompilationIdentity);
            hash.Add(_stages[index].Revision);
            hash.Add(_stages[index].CompilationIdentity);
        }
        _hashCode = hash.ToHashCode();
    }

    internal long EstimatedRetainedBytes => 48L + _stages.Length * 16L;

    public bool Equals(ControlStageCompilationIdentity? other)
    {
        if (ReferenceEquals(this, other))
        {
            return true;
        }
        if (other is null ||
            !ReferenceEquals(_controlIdentity, other._controlIdentity) ||
            _stages.Length != other._stages.Length)
        {
            return false;
        }
        for (var index = 0; index < _stages.Length; index++)
        {
            if (_stages[index].Revision != other._stages[index].Revision ||
                !ReferenceEquals(_stages[index].CompilationIdentity, other._stages[index].CompilationIdentity))
            {
                return false;
            }
        }
        return true;
    }

    public override bool Equals(object? obj) => Equals(obj as ControlStageCompilationIdentity);

    public override int GetHashCode() => _hashCode;
}

internal readonly struct ControlCompilationCacheKey : IEquatable<ControlCompilationCacheKey>
{
    private readonly GlobalCompilationCacheKey _global;
    private readonly NormalizedControlConfigCacheKey? _controlConfig;
    // The descriptor's CompilationIdentity, or a ControlStageCompilationIdentity when control stages take part.
    private readonly object _compilationIdentity;

    private ControlCompilationCacheKey(
        GlobalCompilationCacheKey global,
        ControlTokenDescriptor descriptor,
        NormalizedControlThemeConfig? controlConfig,
        ThemeAppearance appearance,
        IReadOnlyList<ControlTokenAlgorithmDescriptor>? controlStages)
    {
        ArgumentNullException.ThrowIfNull(global);

        _global       = global;
        _compilationIdentity = controlStages is null
            ? descriptor.CompilationIdentity
            : new ControlStageCompilationIdentity(descriptor.CompilationIdentity, controlStages);
        Identity      = descriptor.Identity;
        ControlSlot   = descriptor.Slot;
        Appearance    = appearance;
        _controlConfig = controlConfig is null
            ? null
            : NormalizedControlConfigCacheKey.Create(controlConfig);
    }

    internal ControlTokenIdentity Identity { get; }
    internal long EstimatedRetainedBytes => ThemeRetainedBytesEstimator.Add(
        ThemeRetainedBytesEstimator.Add(
            ThemeRetainedBytesEstimator.Add(64 + CacheKeyArray.EstimateIdentity(Identity), _global.EstimatedRetainedBytes),
            _controlConfig?.EstimatedRetainedBytes ?? 0),
        (_compilationIdentity as ControlStageCompilationIdentity)?.EstimatedRetainedBytes ?? 0);
    internal long EstimatedInputRetainedBytes => _controlConfig?.EstimatedInputRetainedBytes ?? 0;
    internal int ControlSlot { get; }
    internal ThemeAppearance Appearance { get; }

    /// <param name="global">The key of the global compilation the control derives from.</param>
    /// <param name="descriptor">The control.</param>
    /// <param name="controlConfig">The control's effective configuration, if any.</param>
    /// <param name="appearance">The appearance the control compiles under.</param>
    /// <param name="controlStages">
    /// The control token stages that run for this control, in chain order, or <see langword="null"/> when none does;
    /// see <see cref="ThemeCompiler.ResolveControlStages"/>.
    /// </param>
    internal static ControlCompilationCacheKey Create(
        GlobalCompilationCacheKey global,
        ControlTokenDescriptor descriptor,
        NormalizedControlThemeConfig? controlConfig,
        ThemeAppearance appearance,
        IReadOnlyList<ControlTokenAlgorithmDescriptor>? controlStages = null)
    {
        return new ControlCompilationCacheKey(global, descriptor, controlConfig, appearance, controlStages);
    }

    public bool Equals(ControlCompilationCacheKey other)
    {
        return ControlSlot == other.ControlSlot &&
               Appearance == other.Appearance &&
               Identity == other.Identity &&
               (ReferenceEquals(_compilationIdentity, other._compilationIdentity) ||
                (_compilationIdentity is ControlStageCompilationIdentity stages && stages.Equals(other._compilationIdentity))) &&
               _global.Equals(other._global) &&
               Equals(_controlConfig, other._controlConfig);
    }

    public override bool Equals(object? obj)
    {
        return obj is ControlCompilationCacheKey other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(_global, Identity, ControlSlot, Appearance, _compilationIdentity, _controlConfig);
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
        var bytes = 96L + algorithms.Length * 56L;
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
