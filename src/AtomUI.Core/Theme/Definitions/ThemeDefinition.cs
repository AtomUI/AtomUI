using AtomUI.Theme.Configuration;
using AtomUI.Theme.Languages;
using AtomUI.Theme.Schema;

namespace AtomUI.Theme.Definitions;

internal sealed record BoundTokenValue(
    TokenDescriptor Descriptor,
    object? Value,
    ThemeSourceLocation Location,
    string? GeneratedCanonicalValue = null)
{
    internal string CanonicalValue => GeneratedCanonicalValue ?? Descriptor.Format(Value);

    internal NormalizedTokenValue Normalize() => new(Descriptor, Value, CanonicalValue);
}

/// <summary>
/// The bound configuration of one control in one section of a theme definition.
/// </summary>
internal sealed class ControlThemeDefinition
{
    internal ControlThemeDefinition(
        ControlTokenDescriptor descriptor,
        ThemeAppearance? appearance,
        DesignDensity? density,
        IEnumerable<BoundTokenValue> globalTokens,
        IEnumerable<BoundTokenValue> ownTokens,
        ThemeSourceLocation location)
    {
        Descriptor   = descriptor;
        Appearance   = appearance;
        Density      = density;
        GlobalTokens = Array.AsReadOnly(globalTokens.ToArray());
        OwnTokens    = Array.AsReadOnly(ownTokens.ToArray());
        Location     = location;
    }

    public ControlTokenDescriptor Descriptor { get; }
    public ThemeAppearance? Appearance { get; }
    public DesignDensity? Density { get; }
    public IReadOnlyList<BoundTokenValue> GlobalTokens { get; }
    public IReadOnlyList<BoundTokenValue> OwnTokens { get; }
    public ThemeSourceLocation Location { get; }
}

/// <summary>
/// The bound values of one section: the base section or a condition section.
/// </summary>
internal sealed class ThemeSectionDefinition
{
    internal static ThemeSectionDefinition Empty { get; } = new(
        Array.Empty<BoundTokenValue>(), Array.Empty<BoundTokenValue>(), Array.Empty<ControlThemeDefinition>());

    internal ThemeSectionDefinition(
        IEnumerable<BoundTokenValue> tokens,
        IEnumerable<BoundTokenValue> languageTokens,
        IEnumerable<ControlThemeDefinition> controls)
    {
        Tokens         = Array.AsReadOnly(tokens.ToArray());
        LanguageTokens = Array.AsReadOnly(languageTokens.ToArray());
        Controls       = Array.AsReadOnly(controls.ToArray());
    }

    public IReadOnlyList<BoundTokenValue> Tokens { get; }
    public IReadOnlyList<BoundTokenValue> LanguageTokens { get; }
    public IReadOnlyList<ControlThemeDefinition> Controls { get; }
}

/// <summary>
/// A bound <c>&lt;When&gt;</c> section: its condition and values.
/// </summary>
internal sealed class ThemeConditionDefinition
{
    internal ThemeConditionDefinition(
        DesignStyle? style,
        ThemeAppearance? appearance,
        DesignDensity? density,
        DesignIdiom? idiom,
        ThemeSectionDefinition values,
        ThemeSourceLocation location)
    {
        Style      = style;
        Appearance = appearance;
        Density    = density;
        Idiom      = idiom;
        Values     = values;
        Location   = location;
    }

    public DesignStyle? Style { get; }
    public ThemeAppearance? Appearance { get; }
    public DesignDensity? Density { get; }
    public DesignIdiom? Idiom { get; }
    public ThemeSectionDefinition Values { get; }
    public ThemeSourceLocation Location { get; }

    /// <summary>
    /// Whether the section applies to an evaluation unit of <paramref name="variant" /> (unset attributes always
    /// match).
    /// </summary>
    public bool Matches(in DesignVariant variant) =>
        (Style is null || Style == variant.Style) &&
        (Appearance is null || Appearance == variant.Appearance) &&
        (Density is null || Density == variant.Density) &&
        (Idiom is null || Idiom == variant.Idiom);
}

/// <summary>
/// The bound values of a definition: the base section and the condition sections in document order.
/// </summary>
internal sealed record PreparedThemeValues(
    ThemeSectionDefinition Base,
    IReadOnlyList<ThemeConditionDefinition> Conditions);

/// <summary>
/// A theme definition bound to the registry: the language it belongs to, the axes it fixes, the seeds it locks, its
/// base section and its condition sections. It is the lowest value layer of every evaluation unit that uses it;
/// <see cref="ValuesFor" /> gives that layer for one variant.
/// </summary>
internal sealed class PreparedThemeDefinition
{
    private readonly object _valuesGate = new();
    private PreparedThemeValues? _values;
    private Func<PreparedThemeValues>? _createValues;
    private bool _creatingValues;

    internal PreparedThemeDefinition(
        string id, string name, DesignLanguageDescriptor language, DesignStyle? style, ThemeAppearance? appearance,
        DesignDensity? density, IReadOnlySet<string> lockedSeeds,
        ThemeSectionDefinition values, IEnumerable<ThemeConditionDefinition> conditions, ThemeSourceLocation location)
        : this(id, name, language, style, appearance, density, lockedSeeds, location,
            () => throw new InvalidOperationException("Definition values were already initialized."))
    {
        _values = new PreparedThemeValues(values, Array.AsReadOnly(conditions.ToArray()));
        _createValues = null;
    }

    internal PreparedThemeDefinition(
        string id, string name, DesignLanguageDescriptor language, DesignStyle? style, ThemeAppearance? appearance,
        DesignDensity? density, IReadOnlySet<string> lockedSeeds, ThemeSourceLocation location,
        Func<PreparedThemeValues> createValues)
    {
        ArgumentNullException.ThrowIfNull(language);
        ArgumentNullException.ThrowIfNull(lockedSeeds);
        Id           = id;
        Name         = name;
        Language     = language;
        Style        = style;
        Appearance   = appearance;
        Density      = density;
        LockedSeeds  = lockedSeeds;
        Location     = location;
        _createValues = createValues;
    }

    public string Id { get; }
    public string Name { get; }

    /// <summary>
    /// The language the theme belongs to.
    /// </summary>
    public DesignLanguageDescriptor Language { get; }

    /// <summary>
    /// The fixed style, or <see langword="null" /> when open.
    /// </summary>
    public DesignStyle? Style { get; }

    /// <summary>
    /// The fixed appearance, or <see langword="null" /> when open.
    /// </summary>
    public ThemeAppearance? Appearance { get; }

    /// <summary>
    /// The fixed density, or <see langword="null" /> when open.
    /// </summary>
    public DesignDensity? Density { get; }

    /// <summary>
    /// The adjustable seeds the theme locks: the end user's selection cannot replace them.
    /// </summary>
    public IReadOnlySet<string> LockedSeeds { get; }

    public ThemeSourceLocation Location { get; }

    /// <summary>
    /// The axes the theme writes: its language and the axes it fixes.
    /// </summary>
    internal ThemeAxes Axes => new(Language, Style, Appearance, Density, null);

    public ThemeSectionDefinition Base => GetValues().Base;

    public IReadOnlyList<ThemeConditionDefinition> Conditions => GetValues().Conditions;

    /// <summary>
    /// Every control the definition configures, in any section.
    /// </summary>
    public IReadOnlyCollection<ControlTokenIdentity> ControlIdentities =>
        field ??= Base.Controls.Select(static control => control.Descriptor.Identity)
                      .Concat(Conditions.SelectMany(static condition => condition.Values.Controls)
                                        .Select(static control => control.Descriptor.Identity))
                      .ToHashSet();

    /// <summary>
    /// The value layer this definition gives an evaluation unit of <paramref name="variant" />.
    /// </summary>
    internal DefinitionLayer ValuesFor(in DesignVariant variant)
    {
        var values = GetValues();
        if (values.Conditions.Count == 0)
        {
            return DefinitionLayer.For(values.Base, null);
        }

        List<ThemeConditionDefinition>? matching = null;
        foreach (var condition in values.Conditions)
        {
            if (condition.Matches(in variant))
            {
                (matching ??= []).Add(condition);
            }
        }

        return DefinitionLayer.For(values.Base, matching);
    }

    private PreparedThemeValues GetValues()
    {
        if (Volatile.Read(ref _values) is { } values)
        {
            return values;
        }
        lock (_valuesGate)
        {
            if (_values is { } existing)
            {
                return existing;
            }
            if (_creatingValues)
            {
                throw new InvalidOperationException("A recursive compiled theme definition query is not supported.");
            }
            _creatingValues = true;
            try
            {
                var created = _createValues!();
                Volatile.Write(ref _values, created);
                _createValues = null;
                return created;
            }
            finally
            {
                _creatingValues = false;
            }
        }
    }
}

/// <summary>
/// The values one definition gives one evaluation unit: the base section, then every matching condition section in
/// document order, later sections replacing earlier ones by token name and control identity.
/// </summary>
internal sealed class DefinitionLayer
{
    private static readonly System.Runtime.CompilerServices.ConditionalWeakTable<ThemeSectionDefinition, DefinitionLayer>
        s_baseOnly = new();

    private DefinitionLayer(
        NormalizedTokenValue[] globalTokens,
        BoundTokenValue[] languageTokens,
        Dictionary<ControlTokenIdentity, NormalizedControlThemeConfig> controls)
    {
        GlobalTokens   = globalTokens;
        LanguageTokens = languageTokens;
        Controls       = controls;
    }

    /// <summary>
    /// The global token values, ordered by slot.
    /// </summary>
    public IReadOnlyList<NormalizedTokenValue> GlobalTokens { get; }

    /// <summary>
    /// The language token values of the definition's language, ordered by slot.
    /// </summary>
    public IReadOnlyList<BoundTokenValue> LanguageTokens { get; }

    public IReadOnlyDictionary<ControlTokenIdentity, NormalizedControlThemeConfig> Controls { get; }

    internal static DefinitionLayer For(ThemeSectionDefinition baseSection, List<ThemeConditionDefinition>? conditions)
    {
        if (conditions is null)
        {
            return s_baseOnly.GetValue(baseSection, static section => Build(section, null));
        }

        return Build(baseSection, conditions);
    }

    private static DefinitionLayer Build(ThemeSectionDefinition baseSection, List<ThemeConditionDefinition>? conditions)
    {
        var global = new Dictionary<int, NormalizedTokenValue>();
        var language = new Dictionary<int, BoundTokenValue>();
        var controls = new Dictionary<ControlTokenIdentity, (ThemeAppearance? Appearance, DesignDensity? Density,
            Dictionary<int, NormalizedTokenValue> Global, Dictionary<int, NormalizedTokenValue> Own)>();

        void Apply(ThemeSectionDefinition section)
        {
            foreach (var token in section.Tokens)
            {
                global[token.Descriptor.Slot] = token.Normalize();
            }
            foreach (var token in section.LanguageTokens)
            {
                language[token.Descriptor.Slot] = token;
            }
            foreach (var control in section.Controls)
            {
                var identity = control.Descriptor.Identity;
                if (!controls.TryGetValue(identity, out var entry))
                {
                    entry = (null, null, new Dictionary<int, NormalizedTokenValue>(), new Dictionary<int, NormalizedTokenValue>());
                }
                entry.Appearance ??= control.Appearance;
                entry.Density ??= control.Density;
                foreach (var token in control.GlobalTokens)
                {
                    entry.Global[token.Descriptor.Slot] = token.Normalize();
                }
                foreach (var token in control.OwnTokens)
                {
                    entry.Own[token.Descriptor.Slot] = token.Normalize();
                }
                controls[identity] = entry;
            }
        }

        Apply(baseSection);
        if (conditions is not null)
        {
            foreach (var condition in conditions)
            {
                Apply(condition.Values);
            }
        }

        var globalArray = global.Values.ToArray();
        Array.Sort(globalArray, static (left, right) => left.Descriptor.Slot.CompareTo(right.Descriptor.Slot));
        var languageArray = language.Values.ToArray();
        Array.Sort(languageArray, static (left, right) => left.Descriptor.Slot.CompareTo(right.Descriptor.Slot));
        var controlMap = new Dictionary<ControlTokenIdentity, NormalizedControlThemeConfig>(controls.Count);
        foreach (var (identity, entry) in controls)
        {
            controlMap.Add(identity, new NormalizedControlThemeConfig(identity, entry.Appearance, entry.Density,
                entry.Global.Values, entry.Own.Values));
        }

        return new DefinitionLayer(globalArray, languageArray, controlMap);
    }
}

internal sealed class ThemeDefinitionBindResult
{
    internal ThemeDefinitionBindResult(
        PreparedThemeDefinition? definition,
        IEnumerable<ThemeDefinitionDiagnostic> diagnostics)
    {
        Definition  = definition;
        Diagnostics = Array.AsReadOnly(diagnostics.ToArray());
    }

    public PreparedThemeDefinition? Definition { get; }
    public IReadOnlyList<ThemeDefinitionDiagnostic> Diagnostics { get; }

    public bool Success =>
        Definition is not null &&
        Diagnostics.All(static diagnostic =>
            diagnostic.Severity != ThemeDefinitionDiagnosticSeverity.Error);
}
