using System.Globalization;
using System.Diagnostics.CodeAnalysis;
using AtomUI.Theme.Languages;
using AtomUI.Theme.Schema;

namespace AtomUI.Theme.Configuration;

internal readonly record struct ThemeConfigFingerprint(ulong Value)
{
    public override string ToString() => Value.ToString("X16", CultureInfo.InvariantCulture);
}

internal sealed class NormalizedTokenValue : IEquatable<NormalizedTokenValue>
{
    internal NormalizedTokenValue(TokenDescriptor descriptor, object? value, string canonicalValue)
    {
        ArgumentNullException.ThrowIfNull(descriptor);
        ArgumentNullException.ThrowIfNull(canonicalValue);
        Descriptor     = descriptor;
        Value          = value;
        CanonicalValue = canonicalValue;
    }

    public TokenDescriptor Descriptor { get; }
    public object? Value { get; }
    internal string CanonicalValue { get; }

    public bool Equals(NormalizedTokenValue? other)
    {
        return ReferenceEquals(this, other) ||
               other is not null &&
               ReferenceEquals(Descriptor, other.Descriptor) &&
               string.Equals(CanonicalValue, other.CanonicalValue, StringComparison.Ordinal);
    }

    public override bool Equals(object? obj) => Equals(obj as NormalizedTokenValue);

    public override int GetHashCode() => HashCode.Combine(Descriptor, CanonicalValue);
}

/// <summary>
/// An explicit language token value as written. It is bound to the effective language's schema when a theme is
/// compiled, because a layer may set it without naming the language and it applies only while that language is used.
/// </summary>
internal sealed record LanguageTokenValue(string Name, string Value);

/// <summary>
/// The axes a configuration layer writes: the language and its style, appearance, density and idiom. A
/// <see langword="null"/> member is not written by the layer and is inherited (spec 7.7).
/// </summary>
internal readonly record struct ThemeAxes(
    DesignLanguageDescriptor? Language,
    DesignStyle? Style,
    ThemeAppearance? Appearance,
    DesignDensity? Density,
    DesignIdiom? Idiom)
{
    internal static ThemeAxes None => default;

    internal bool IsEmpty => Language is null && Style is null && Appearance is null && Density is null && Idiom is null;

    /// <summary>This layer's axes over <paramref name="lower"/>: what this layer writes wins.</summary>
    internal ThemeAxes Over(ThemeAxes lower) => new(
        Language ?? lower.Language,
        // A layer that changes the language does not keep a style of the other language.
        Style ?? (Language is not null && lower.Language is not null && !ReferenceEquals(Language, lower.Language)
            ? null
            : lower.Style),
        Appearance ?? lower.Appearance,
        Density ?? lower.Density,
        Idiom ?? lower.Idiom);

    /// <summary>The variant these axes select; unwritten axes take the language's defaults.</summary>
    internal DesignVariant ResolveVariant()
    {
        var language = Language ?? throw new InvalidOperationException("A theme compilation requires a design language.");
        return new DesignVariant(
            Style ?? language.Axes.DefaultStyle,
            Appearance ?? ThemeAppearance.Light,
            Density ?? language.DefaultVariant(DesignIdiom.Desktop).Density,
            Idiom ?? DesignIdiom.Desktop);
    }

    public bool Equals(ThemeAxes other) =>
        ReferenceEquals(Language, other.Language) && Style == other.Style && Appearance == other.Appearance &&
        Density == other.Density && Idiom == other.Idiom;

    public override int GetHashCode() => HashCode.Combine(Language?.Id, Style, Appearance, Density, Idiom);
}

internal sealed class NormalizedControlThemeConfig : IEquatable<NormalizedControlThemeConfig>
{
    private readonly NormalizedTokenValue[] _globalTokens;
    private readonly NormalizedTokenValue[] _ownTokens;

    internal NormalizedControlThemeConfig(
        ControlTokenIdentity identity,
        ThemeAppearance? appearance,
        DesignDensity? density,
        IEnumerable<NormalizedTokenValue> globalTokens,
        IEnumerable<NormalizedTokenValue> ownTokens)
    {
        ArgumentNullException.ThrowIfNull(globalTokens);
        ArgumentNullException.ThrowIfNull(ownTokens);

        Identity      = identity;
        Appearance    = appearance;
        Density       = density;
        _globalTokens = SortTokens(globalTokens);
        _ownTokens    = SortTokens(ownTokens);
        GlobalTokens  = Array.AsReadOnly(_globalTokens);
        OwnTokens     = Array.AsReadOnly(_ownTokens);
    }

    private NormalizedControlThemeConfig(
        ControlTokenIdentity identity,
        ThemeAppearance? appearance,
        DesignDensity? density,
        NormalizedTokenValue[] globalTokens,
        NormalizedTokenValue[] ownTokens,
        bool takeOwnership)
    {
        Identity      = identity;
        Appearance    = appearance;
        Density       = density;
        _globalTokens = globalTokens;
        _ownTokens    = ownTokens;
        GlobalTokens  = Array.AsReadOnly(_globalTokens);
        OwnTokens     = Array.AsReadOnly(_ownTokens);
    }

    internal static NormalizedControlThemeConfig CreateCanonical(
        ControlTokenIdentity identity,
        ThemeAppearance? appearance,
        DesignDensity? density,
        NormalizedTokenValue[] globalTokens,
        NormalizedTokenValue[] ownTokens)
    {
        ArgumentNullException.ThrowIfNull(globalTokens);
        ArgumentNullException.ThrowIfNull(ownTokens);
        return new NormalizedControlThemeConfig(identity, appearance, density, globalTokens, ownTokens, true);
    }

    public ControlTokenIdentity Identity { get; }

    /// <summary>The appearance this control is fixed to, or <see langword="null"/> to use its scope's.</summary>
    public ThemeAppearance? Appearance { get; }

    /// <summary>The density this control is fixed to, or <see langword="null"/> to use its scope's.</summary>
    public DesignDensity? Density { get; }

    /// <summary>The global tokens this control sees differently; a seed among them makes it its own evaluation unit.</summary>
    public IReadOnlyList<NormalizedTokenValue> GlobalTokens { get; }

    public IReadOnlyList<NormalizedTokenValue> OwnTokens { get; }

    /// <summary>
    /// Whether this control needs its own global evaluation (spec 6.4): it fixes an axis or overrides a seed. Otherwise
    /// its global overrides only replace values and it shares its scope's global tokens.
    /// </summary>
    internal bool IsOwnEvaluationUnit
    {
        get
        {
            if (Appearance is not null || Density is not null)
            {
                return true;
            }

            foreach (var token in _globalTokens)
            {
                if (token.Descriptor.Stage == TokenStage.Seed)
                {
                    return true;
                }
            }

            return false;
        }
    }

    public bool Equals(NormalizedControlThemeConfig? other)
    {
        return ReferenceEquals(this, other) ||
               other is not null &&
               Identity == other.Identity &&
               Identity.OwnerType == other.Identity.OwnerType &&
               Appearance == other.Appearance &&
               Density == other.Density &&
               SequenceEqual(_globalTokens, other._globalTokens) &&
               SequenceEqual(_ownTokens, other._ownTokens);
    }

    public override bool Equals(object? obj) => Equals(obj as NormalizedControlThemeConfig);

    public override int GetHashCode() => HashCode.Combine(Identity, Appearance, Density);

    private static NormalizedTokenValue[] SortTokens(IEnumerable<NormalizedTokenValue> values)
    {
        var result = ThemeConfigArray.Copy(values);
        Array.Sort(result, static (left, right) => left.Descriptor.Slot.CompareTo(right.Descriptor.Slot));
        return result;
    }

    private static bool SequenceEqual<T>(IReadOnlyList<T> left, IReadOnlyList<T> right)
    {
        if (left.Count != right.Count)
        {
            return false;
        }

        var comparer = EqualityComparer<T>.Default;
        for (var index = 0; index < left.Count; index++)
        {
            if (!comparer.Equals(left[index], right[index]))
            {
                return false;
            }
        }

        return true;
    }
}

/// <summary>
/// One configuration layer after binding, or the effective configuration of an evaluation unit after merging. The
/// selected theme definition is kept as its own lowest layer (<see cref="Definition"/>): its condition sections are
/// matched against each evaluation unit's variant when the theme is compiled (spec 7.6, 7.7).
/// </summary>
internal sealed class NormalizedThemeConfig : IEquatable<NormalizedThemeConfig>
{
    private readonly NormalizedTokenValue[] _globalTokens;
    private readonly LanguageTokenValue[] _languageTokens;
    private readonly NormalizedControlThemeConfig[] _controls;
    private readonly IReadOnlyList<ControlTokenIdentity> _controlIdentities;
    internal IReadOnlyList<ControlTokenIdentity> ControlIdentities => _controlIdentities;

    internal NormalizedThemeConfig(
        bool inherit,
        ThemeAxes axes,
        IEnumerable<NormalizedTokenValue> globalTokens,
        IEnumerable<LanguageTokenValue> languageTokens,
        IEnumerable<NormalizedControlThemeConfig> controls,
        Definitions.PreparedThemeDefinition? definition = null)
    {
        ArgumentNullException.ThrowIfNull(globalTokens);
        ArgumentNullException.ThrowIfNull(languageTokens);
        ArgumentNullException.ThrowIfNull(controls);

        Inherit         = inherit;
        Axes            = axes;
        Definition      = definition;
        _globalTokens   = ThemeConfigArray.Copy(globalTokens);
        _languageTokens = ThemeConfigArray.Copy(languageTokens);
        _controls       = ThemeConfigArray.Copy(controls);
        Array.Sort(_globalTokens, static (left, right) => left.Descriptor.Slot.CompareTo(right.Descriptor.Slot));
        Array.Sort(_languageTokens, static (left, right) => string.CompareOrdinal(left.Name, right.Name));
        Array.Sort(_controls, static (left, right) => CompareIdentity(left.Identity, right.Identity));
        Controls           = Array.AsReadOnly(_controls);
        _controlIdentities = Array.AsReadOnly(_controls.Select(control => control.Identity).ToArray());
        GlobalTokens   = Array.AsReadOnly(_globalTokens);
        LanguageTokens = Array.AsReadOnly(_languageTokens);
        Fingerprint    = ThemeConfigFingerprintBuilder.Compute(this);
    }

    private NormalizedThemeConfig(
        bool inherit,
        ThemeAxes axes,
        NormalizedTokenValue[] globalTokens,
        LanguageTokenValue[] languageTokens,
        NormalizedControlThemeConfig[] controls,
        Definitions.PreparedThemeDefinition? definition,
        bool takeOwnership)
    {
        Inherit         = inherit;
        Axes            = axes;
        Definition      = definition;
        _globalTokens   = globalTokens;
        _languageTokens = languageTokens;
        _controls       = controls;
        Controls           = Array.AsReadOnly(_controls);
        _controlIdentities = Array.AsReadOnly(_controls.Select(control => control.Identity).ToArray());
        GlobalTokens   = Array.AsReadOnly(_globalTokens);
        LanguageTokens = Array.AsReadOnly(_languageTokens);
        Fingerprint    = ThemeConfigFingerprintBuilder.Compute(this);
    }

    internal static NormalizedThemeConfig CreateCanonical(
        bool inherit,
        ThemeAxes axes,
        NormalizedTokenValue[] globalTokens,
        LanguageTokenValue[] languageTokens,
        NormalizedControlThemeConfig[] controls,
        Definitions.PreparedThemeDefinition? definition = null)
    {
        ArgumentNullException.ThrowIfNull(globalTokens);
        ArgumentNullException.ThrowIfNull(languageTokens);
        ArgumentNullException.ThrowIfNull(controls);
        return new NormalizedThemeConfig(inherit, axes, globalTokens, languageTokens, controls, definition, true);
    }

    internal bool TryGetControl(ControlTokenIdentity identity, [NotNullWhen(true)] out NormalizedControlThemeConfig? control)
    {
        control = Array.Find(_controls, value => value.Identity == identity);
        return control is not null;
    }

    public bool Inherit { get; }

    /// <summary>The axes this configuration writes; in an effective configuration the language is always set.</summary>
    public ThemeAxes Axes { get; }

    /// <summary>The language this configuration writes; always set in an effective configuration.</summary>
    public DesignLanguageDescriptor? Language => Axes.Language;

    /// <summary>
    /// The selected theme definition, the lowest value layer; <see langword="null"/> when none applies (a scope with
    /// <c>Inherit = false</c>, or a configuration layer that is not the theme's defaults).
    /// </summary>
    public Definitions.PreparedThemeDefinition? Definition { get; }

    /// <summary>The explicit global token values above the definition.</summary>
    public IReadOnlyList<NormalizedTokenValue> GlobalTokens { get; }

    /// <summary>The explicit language tokens; bound to the effective language when compiled.</summary>
    public IReadOnlyList<LanguageTokenValue> LanguageTokens { get; }

    public IReadOnlyList<NormalizedControlThemeConfig> Controls { get; }

    public ThemeConfigFingerprint Fingerprint { get; }

    public bool Equals(NormalizedThemeConfig? other)
    {
        return ReferenceEquals(this, other) ||
               other is not null &&
               Fingerprint == other.Fingerprint &&
               Inherit == other.Inherit &&
               Axes.Equals(other.Axes) &&
               ReferenceEquals(Definition, other.Definition) &&
               SequenceEqual(_globalTokens, other._globalTokens) &&
               SequenceEqual(_languageTokens, other._languageTokens) &&
               SequenceEqual(_controls, other._controls);
    }

    public override bool Equals(object? obj) => Equals(obj as NormalizedThemeConfig);

    public override int GetHashCode() => HashCode.Combine(Fingerprint, Inherit, Axes);

    private static int CompareIdentity(ControlTokenIdentity left, ControlTokenIdentity right)
    {
        var catalog = string.Compare(left.Catalog, right.Catalog, StringComparison.Ordinal);
        return catalog != 0 ? catalog : string.Compare(left.Id, right.Id, StringComparison.Ordinal);
    }

    private static bool SequenceEqual<T>(IReadOnlyList<T> left, IReadOnlyList<T> right)
    {
        if (left.Count != right.Count)
        {
            return false;
        }

        var comparer = EqualityComparer<T>.Default;
        for (var index = 0; index < left.Count; index++)
        {
            if (!comparer.Equals(left[index], right[index]))
            {
                return false;
            }
        }

        return true;
    }
}

internal struct ThemeConfigFingerprintBuilder
{
    private const ulong Offset = 14695981039346656037UL;
    private const ulong Prime = 1099511628211UL;
    private ulong _value;

    internal static ThemeConfigFingerprint Compute(NormalizedThemeConfig config)
    {
        var builder = new ThemeConfigFingerprintBuilder
        {
            _value = Offset
        };
        builder.Add(config.Inherit);
        AddAxes(ref builder, config.Axes);
        AddTokens(ref builder, config.GlobalTokens);
        builder.Add(config.LanguageTokens.Count);
        foreach (var token in config.LanguageTokens)
        {
            builder.Add(token.Name);
            builder.Add(token.Value);
        }

        builder.Add(config.Definition?.Id ?? string.Empty);
        builder.Add(config.Controls.Count);
        foreach (var control in config.Controls)
        {
            builder.Add(control.Identity.Catalog);
            builder.Add(control.Identity.Id);
            builder.Add(control.Appearance is { } appearance ? (int)appearance + 1 : 0);
            builder.Add(control.Density?.Id ?? string.Empty);
            AddTokens(ref builder, control.GlobalTokens);
            AddTokens(ref builder, control.OwnTokens);
        }

        return new ThemeConfigFingerprint(builder._value);
    }

    private static void AddAxes(ref ThemeConfigFingerprintBuilder builder, ThemeAxes axes)
    {
        builder.Add(axes.Language?.Id ?? string.Empty);
        builder.Add(axes.Language?.Revision.ToString("N", CultureInfo.InvariantCulture) ?? string.Empty);
        builder.Add(axes.Style?.Id ?? string.Empty);
        builder.Add(axes.Appearance is { } appearance ? (int)appearance + 1 : 0);
        builder.Add(axes.Density?.Id ?? string.Empty);
        builder.Add(axes.Idiom?.Id ?? string.Empty);
    }

    private static void AddTokens(ref ThemeConfigFingerprintBuilder builder, IReadOnlyList<NormalizedTokenValue> tokens)
    {
        builder.Add(tokens.Count);
        foreach (var token in tokens)
        {
            builder.Add(token.Descriptor.Slot);
            builder.Add(token.Descriptor.Name);
            builder.Add(token.CanonicalValue);
        }
    }

    private void Add(bool value) => Add(value ? 1 : 0);

    private void Add(int value)
    {
        unchecked
        {
            Add((char)value);
            Add((char)(value >> 16));
        }
    }

    private void Add(string value)
    {
        Add(value.Length);
        foreach (var character in value)
        {
            Add(character);
        }
    }

    private void Add(char value)
    {
        unchecked
        {
            _value ^= (byte)value;
            _value *= Prime;
            _value ^= (byte)(value >> 8);
            _value *= Prime;
        }
    }
}

internal static class ThemeConfigArray
{
    internal static T[] Copy<T>(IEnumerable<T> source)
    {
        return source switch
        {
            T[] array => (T[])array.Clone(),
            ICollection<T> collection => CopyCollection(collection),
            _ => new List<T>(source).ToArray()
        };
    }

    private static T[] CopyCollection<T>(ICollection<T> source)
    {
        if (source.Count == 0)
        {
            return Array.Empty<T>();
        }

        var result = new T[source.Count];
        source.CopyTo(result, 0);
        return result;
    }
}
