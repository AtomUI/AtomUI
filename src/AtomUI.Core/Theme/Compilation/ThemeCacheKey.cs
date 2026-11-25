using System.Runtime.CompilerServices;
using AtomUI.Theme.Configuration;
using AtomUI.Theme.Definitions;
using AtomUI.Theme.Languages;
using AtomUI.Theme.Schema;

namespace AtomUI.Theme.Compilation;

/// <summary>
/// The axes of a configuration as a cache key. The language is compared by its registration identity (unique per
/// descriptor instance) and its revision; the other axes by value.
/// </summary>
internal readonly record struct AxesCacheKey(
    object? LanguageIdentity,
    Guid LanguageRevision,
    DesignStyle? Style,
    ThemeAppearance? Appearance,
    DesignDensity? Density,
    DesignIdiom? Idiom)
{
    internal static AxesCacheKey Create(ThemeAxes axes) => new(
        axes.Language?.CompilationIdentity,
        axes.Language?.Revision ?? Guid.Empty,
        axes.Style,
        axes.Appearance,
        axes.Density,
        axes.Idiom);
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

    internal static TokenCacheKey Create(LanguageTokenValue token) => new(token.Name, -1, TokenStage.Language, token.Value);
}

internal sealed class NormalizedControlConfigCacheKey : IEquatable<NormalizedControlConfigCacheKey>
{
    // 输入在规范化边界冻结；弱键不延长配置或其 registry 的生命周期。
    private static readonly ConditionalWeakTable<NormalizedControlThemeConfig, NormalizedControlConfigCacheKey> s_keys = new();
    private readonly TokenCacheKey[] _globalTokens;
    private readonly TokenCacheKey[] _ownTokens;
    private readonly int _hashCode;

    private NormalizedControlConfigCacheKey(NormalizedControlThemeConfig config)
    {
        Identity      = config.Identity;
        Appearance    = config.Appearance;
        Density       = config.Density;
        _globalTokens = CacheKeyArray.Tokens(config.GlobalTokens);
        _ownTokens    = CacheKeyArray.Tokens(config.OwnTokens);
        _hashCode     = ComputeHashCode();
        EstimatedRetainedBytes = ThemeRetainedBytesEstimator.Add(
            CacheKeyArray.Estimate(_globalTokens, _ownTokens), CacheKeyArray.EstimateIdentity(Identity));
        EstimatedInputRetainedBytes = ThemeRetainedBytesEstimator.EstimateConfig(config);
    }

    internal long EstimatedRetainedBytes { get; }
    internal long EstimatedInputRetainedBytes { get; }
    internal ControlTokenIdentity Identity { get; }
    internal ThemeAppearance? Appearance { get; }
    internal DesignDensity? Density { get; }

    internal static NormalizedControlConfigCacheKey Create(NormalizedControlThemeConfig config)
    {
        return s_keys.GetValue(config, static value => new NormalizedControlConfigCacheKey(value));
    }

    public bool Equals(NormalizedControlConfigCacheKey? other)
    {
        return ReferenceEquals(this, other) ||
               other is not null &&
               Identity == other.Identity &&
               Appearance == other.Appearance &&
               Density == other.Density &&
               _globalTokens.AsSpan().SequenceEqual(other._globalTokens) &&
               _ownTokens.AsSpan().SequenceEqual(other._ownTokens);
    }

    public override bool Equals(object? obj) => Equals(obj as NormalizedControlConfigCacheKey);

    public override int GetHashCode() => _hashCode;

    private int ComputeHashCode()
    {
        var hash = new HashCode();
        hash.Add(Identity);
        hash.Add(Appearance);
        hash.Add(Density);
        CacheKeyArray.Add(ref hash, _globalTokens);
        CacheKeyArray.Add(ref hash, _ownTokens);
        return hash.ToHashCode();
    }
}

internal sealed class NormalizedThemeConfigCacheKey : IEquatable<NormalizedThemeConfigCacheKey>
{
    private static readonly ConditionalWeakTable<NormalizedThemeConfig, NormalizedThemeConfigCacheKey> s_keys = new();
    private readonly TokenCacheKey[] _globalTokens;
    private readonly TokenCacheKey[] _languageTokens;
    private readonly NormalizedControlConfigCacheKey[] _controls;
    private readonly ThemeDefinitionCacheKey? _definition;
    private readonly int _hashCode;

    private NormalizedThemeConfigCacheKey(NormalizedThemeConfig config)
    {
        Inherit         = config.Inherit;
        Fingerprint     = config.Fingerprint;
        Axes            = AxesCacheKey.Create(config.Axes);
        _globalTokens   = CacheKeyArray.Tokens(config.GlobalTokens);
        _languageTokens = config.LanguageTokens.Select(TokenCacheKey.Create).ToArray();
        _definition     = config.Definition is null ? null : ThemeDefinitionCacheKey.Create(config.Definition);
        _controls       = config.Controls.Select(NormalizedControlConfigCacheKey.Create).ToArray();
        _hashCode       = ComputeHashCode();
        EstimatedRetainedBytes = CacheKeyArray.Estimate(_globalTokens, _languageTokens);
        EstimatedInputRetainedBytes = ThemeRetainedBytesEstimator.EstimateConfig(config);
        foreach (var control in _controls)
        {
            EstimatedRetainedBytes = ThemeRetainedBytesEstimator.Add(EstimatedRetainedBytes,
                ThemeRetainedBytesEstimator.Add(8, control.EstimatedRetainedBytes));
        }
    }

    internal long EstimatedRetainedBytes { get; }
    internal long EstimatedInputRetainedBytes { get; }
    internal AxesCacheKey Axes { get; }
    internal TokenCacheKey[] GlobalTokenKeys => _globalTokens;
    internal TokenCacheKey[] LanguageTokenKeys => _languageTokens;
    internal ThemeDefinitionCacheKey? DefinitionKey => _definition;
    internal bool Inherit { get; }
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
            Axes != other.Axes ||
            !Equals(_definition, other._definition) ||
            !_globalTokens.AsSpan().SequenceEqual(other._globalTokens) ||
            !_languageTokens.AsSpan().SequenceEqual(other._languageTokens) ||
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
        hash.Add(Axes);
        hash.Add(_definition);
        CacheKeyArray.Add(ref hash, _globalTokens);
        CacheKeyArray.Add(ref hash, _languageTokens);
        foreach (var control in _controls)
        {
            hash.Add(control);
        }
        return hash.ToHashCode();
    }
}

internal sealed class ControlDefinitionCacheKey : IEquatable<ControlDefinitionCacheKey>
{
    private readonly TokenCacheKey[] _globalTokens;
    private readonly TokenCacheKey[] _ownTokens;
    private readonly int _hashCode;

    internal ControlDefinitionCacheKey(ControlThemeDefinition definition)
    {
        Identity      = definition.Descriptor.Identity;
        Appearance    = definition.Appearance;
        Density       = definition.Density;
        _globalTokens = CacheKeyArray.Tokens(definition.GlobalTokens);
        _ownTokens    = CacheKeyArray.Tokens(definition.OwnTokens);
        _hashCode     = ComputeHashCode();
        EstimatedRetainedBytes = ThemeRetainedBytesEstimator.Add(
            CacheKeyArray.Estimate(_globalTokens, _ownTokens), CacheKeyArray.EstimateIdentity(Identity));
    }

    internal long EstimatedRetainedBytes { get; }
    internal ControlTokenIdentity Identity { get; }
    internal ThemeAppearance? Appearance { get; }
    internal DesignDensity? Density { get; }

    public bool Equals(ControlDefinitionCacheKey? other)
    {
        return ReferenceEquals(this, other) ||
               other is not null &&
               Identity == other.Identity &&
               Appearance == other.Appearance &&
               Density == other.Density &&
               _globalTokens.AsSpan().SequenceEqual(other._globalTokens) &&
               _ownTokens.AsSpan().SequenceEqual(other._ownTokens);
    }

    public override bool Equals(object? obj) => Equals(obj as ControlDefinitionCacheKey);

    public override int GetHashCode() => _hashCode;

    private int ComputeHashCode()
    {
        var hash = new HashCode();
        hash.Add(Identity);
        hash.Add(Appearance);
        hash.Add(Density);
        CacheKeyArray.Add(ref hash, _globalTokens);
        CacheKeyArray.Add(ref hash, _ownTokens);
        return hash.ToHashCode();
    }
}

/// <summary>One section of a definition as a cache key: its condition (empty for the base section) and its values.</summary>
internal sealed class ThemeSectionCacheKey : IEquatable<ThemeSectionCacheKey>
{
    private readonly TokenCacheKey[] _tokens;
    private readonly TokenCacheKey[] _languageTokens;
    private readonly ControlDefinitionCacheKey[] _controls;
    private readonly (DesignStyle? Style, ThemeAppearance? Appearance, DesignDensity? Density, DesignIdiom? Idiom) _condition;
    private readonly int _hashCode;

    internal ThemeSectionCacheKey(ThemeSectionDefinition section, ThemeConditionDefinition? condition)
    {
        _tokens         = CacheKeyArray.Tokens(section.Tokens);
        _languageTokens = CacheKeyArray.Tokens(section.LanguageTokens);
        _controls       = section.Controls
                                 .OrderBy(static control => control.Descriptor.Identity.Catalog, StringComparer.Ordinal)
                                 .ThenBy(static control => control.Descriptor.Identity.Id, StringComparer.Ordinal)
                                 .Select(static control => new ControlDefinitionCacheKey(control))
                                 .ToArray();
        _condition = condition is null
            ? default
            : (condition.Style, condition.Appearance, condition.Density, condition.Idiom);
        var hash = new HashCode();
        hash.Add(_condition);
        CacheKeyArray.Add(ref hash, _tokens);
        CacheKeyArray.Add(ref hash, _languageTokens);
        foreach (var control in _controls)
        {
            hash.Add(control);
        }
        _hashCode = hash.ToHashCode();
        EstimatedRetainedBytes = CacheKeyArray.Estimate(_tokens, _languageTokens);
        foreach (var control in _controls)
        {
            EstimatedRetainedBytes = ThemeRetainedBytesEstimator.Add(EstimatedRetainedBytes,
                ThemeRetainedBytesEstimator.Add(8, control.EstimatedRetainedBytes));
        }
    }

    internal long EstimatedRetainedBytes { get; }

    public bool Equals(ThemeSectionCacheKey? other)
    {
        if (ReferenceEquals(this, other))
        {
            return true;
        }
        if (other is null || _condition != other._condition ||
            !_tokens.AsSpan().SequenceEqual(other._tokens) ||
            !_languageTokens.AsSpan().SequenceEqual(other._languageTokens) ||
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

    public override bool Equals(object? obj) => Equals(obj as ThemeSectionCacheKey);

    public override int GetHashCode() => _hashCode;
}

internal sealed class ThemeDefinitionCacheKey : IEquatable<ThemeDefinitionCacheKey>
{
    // Bound definition 是绑定阶段封存的输入，后续修订必须创建新实例。
    private static readonly ConditionalWeakTable<PreparedThemeDefinition, ThemeDefinitionCacheKey> s_keys = new();
    private readonly ThemeSectionCacheKey[] _sections;
    private readonly string[] _lockedSeeds;
    private readonly int _hashCode;

    private ThemeDefinitionCacheKey(PreparedThemeDefinition definition)
    {
        Id          = definition.Id;
        Name        = definition.Name;
        Axes        = AxesCacheKey.Create(definition.Axes);
        _lockedSeeds = definition.LockedSeeds.Order(StringComparer.Ordinal).ToArray();
        _sections = new ThemeSectionCacheKey[definition.Conditions.Count + 1];
        _sections[0] = new ThemeSectionCacheKey(definition.Base, null);
        for (var index = 0; index < definition.Conditions.Count; index++)
        {
            var condition = definition.Conditions[index];
            _sections[index + 1] = new ThemeSectionCacheKey(condition.Values, condition);
        }
        var hash = new HashCode();
        hash.Add(Id, StringComparer.Ordinal);
        hash.Add(Name, StringComparer.Ordinal);
        hash.Add(Axes);
        foreach (var seed in _lockedSeeds)
        {
            hash.Add(seed, StringComparer.Ordinal);
        }
        foreach (var section in _sections)
        {
            hash.Add(section);
        }
        _hashCode = hash.ToHashCode();
        EstimatedInputRetainedBytes = ThemeRetainedBytesEstimator.EstimateDefinition(definition);
        EstimatedRetainedBytes = ThemeRetainedBytesEstimator.EstimateValue(Id) + ThemeRetainedBytesEstimator.EstimateValue(Name);
        foreach (var section in _sections)
        {
            EstimatedRetainedBytes = ThemeRetainedBytesEstimator.Add(EstimatedRetainedBytes,
                ThemeRetainedBytesEstimator.Add(8, section.EstimatedRetainedBytes));
        }
    }

    internal long EstimatedRetainedBytes { get; }
    internal long EstimatedInputRetainedBytes { get; }
    internal string Id { get; }
    internal string Name { get; }
    internal AxesCacheKey Axes { get; }

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
            !string.Equals(Name, other.Name, StringComparison.Ordinal) ||
            Axes != other.Axes ||
            !_lockedSeeds.AsSpan().SequenceEqual(other._lockedSeeds) ||
            _sections.Length != other._sections.Length)
        {
            return false;
        }

        for (var index = 0; index < _sections.Length; index++)
        {
            if (!_sections[index].Equals(other._sections[index]))
            {
                return false;
            }
        }

        return true;
    }

    public override bool Equals(object? obj) => Equals(obj as ThemeDefinitionCacheKey);

    public override int GetHashCode() => _hashCode;
}

/// <summary>
/// The inputs of one global compilation: the registry, the resolved variant and language, the definition layer and
/// the explicit global and language tokens.
/// </summary>
internal sealed class GlobalCompilationCacheKey : IEquatable<GlobalCompilationCacheKey>
{
    private readonly TokenCacheKey[] _globalTokens;
    private readonly TokenCacheKey[] _languageTokens;
    private readonly ThemeDefinitionCacheKey? _definition;
    private readonly int _hashCode;

    private GlobalCompilationCacheKey(ThemeCompileInput input)
    {
        RegistryRevision = input.Registry.Revision;
        RegistryIdentity = input.Registry.BindingIdentity;
        var configKey     = NormalizedThemeConfigCacheKey.Create(input.EffectiveConfig);
        Language          = input.EffectiveConfig.Language!.CompilationIdentity;
        Variant           = input.EffectiveConfig.Axes.ResolveVariant();
        _globalTokens     = configKey.GlobalTokenKeys;
        _languageTokens   = configKey.LanguageTokenKeys;
        _definition       = configKey.DefinitionKey;
        _hashCode         = ComputeHashCode();
        EstimatedRetainedBytes = CacheKeyArray.Estimate(_globalTokens, _languageTokens);
    }

    internal long EstimatedRetainedBytes { get; }
    internal ThemeSchemaRevision RegistryRevision { get; }
    private object RegistryIdentity { get; }
    private object Language { get; }
    internal DesignVariant Variant { get; }

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
               ReferenceEquals(Language, other.Language) &&
               Variant == other.Variant &&
               Equals(_definition, other._definition) &&
               _globalTokens.AsSpan().SequenceEqual(other._globalTokens) &&
               _languageTokens.AsSpan().SequenceEqual(other._languageTokens);
    }

    public override bool Equals(object? obj) => Equals(obj as GlobalCompilationCacheKey);

    public override int GetHashCode() => _hashCode;

    private int ComputeHashCode()
    {
        var hash = new HashCode();
        hash.Add(RegistryRevision);
        hash.Add(RegistryIdentity);
        hash.Add(Language);
        hash.Add(Variant);
        hash.Add(_definition);
        CacheKeyArray.Add(ref hash, _globalTokens);
        CacheKeyArray.Add(ref hash, _languageTokens);
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
        Fingerprint = ThemeContentFingerprint.Compute(
            input.DefinitionRevision,
            input.EffectiveConfig,
            input.Registry.Revision);
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
}

internal readonly struct ControlCompilationCacheKey : IEquatable<ControlCompilationCacheKey>
{
    private readonly GlobalCompilationCacheKey _global;
    private readonly NormalizedControlConfigCacheKey? _controlConfig;
    private readonly object _compilationIdentity;
    private readonly object? _ruleIdentity;

    private ControlCompilationCacheKey(
        GlobalCompilationCacheKey global,
        ControlTokenDescriptor descriptor,
        NormalizedControlThemeConfig? controlConfig,
        ControlTokenRuleDescriptor? rule)
    {
        ArgumentNullException.ThrowIfNull(global);

        _global              = global;
        _compilationIdentity = descriptor.CompilationIdentity;
        _ruleIdentity  = rule?.CompilationIdentity;
        Identity             = descriptor.Identity;
        ControlSlot          = descriptor.Slot;
        _controlConfig = controlConfig is null
            ? null
            : NormalizedControlConfigCacheKey.Create(controlConfig);
    }

    internal ControlTokenIdentity Identity { get; }
    internal long EstimatedRetainedBytes => ThemeRetainedBytesEstimator.Add(
        ThemeRetainedBytesEstimator.Add(64 + CacheKeyArray.EstimateIdentity(Identity), _global.EstimatedRetainedBytes),
        _controlConfig?.EstimatedRetainedBytes ?? 0);
    internal long EstimatedInputRetainedBytes => _controlConfig?.EstimatedInputRetainedBytes ?? 0;
    internal int ControlSlot { get; }

    /// <param name="global">The key of the global compilation the control derives from.</param>
    /// <param name="descriptor">The control.</param>
    /// <param name="controlConfig">The control's effective configuration, if any.</param>
    /// <param name="rule">The control rule the language uses for the control.</param>
    internal static ControlCompilationCacheKey Create(
        GlobalCompilationCacheKey global,
        ControlTokenDescriptor descriptor,
        NormalizedControlThemeConfig? controlConfig,
        ControlTokenRuleDescriptor? rule)
    {
        return new ControlCompilationCacheKey(global, descriptor, controlConfig, rule);
    }

    public bool Equals(ControlCompilationCacheKey other)
    {
        return ControlSlot == other.ControlSlot &&
               Identity == other.Identity &&
               ReferenceEquals(_compilationIdentity, other._compilationIdentity) &&
               ReferenceEquals(_ruleIdentity, other._ruleIdentity) &&
               _global.Equals(other._global) &&
               Equals(_controlConfig, other._controlConfig);
    }

    public override bool Equals(object? obj)
    {
        return obj is ControlCompilationCacheKey other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(_global, Identity, ControlSlot, _compilationIdentity, _ruleIdentity, _controlConfig);
    }
}

internal static class CacheKeyArray
{
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

    internal static long Estimate(TokenCacheKey[] tokens, TokenCacheKey[]? secondTokens = null)
    {
        var bytes = 96L;
        bytes = ThemeRetainedBytesEstimator.Add(bytes, EstimateTokens(tokens));
        if (secondTokens is not null)
        {
            bytes = ThemeRetainedBytesEstimator.Add(bytes, EstimateTokens(secondTokens));
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
