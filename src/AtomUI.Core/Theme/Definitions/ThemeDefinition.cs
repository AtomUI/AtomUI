using AtomUI.Theme.Configuration;
using AtomUI.Theme.Schema;

namespace AtomUI.Theme.Definitions;

internal sealed record BoundTokenValue(
    TokenDescriptor Descriptor,
    object? Value,
    ThemeSourceLocation Location,
    string? GeneratedCanonicalValue = null)
{
    internal string CanonicalValue => GeneratedCanonicalValue ?? Descriptor.Format(Value);
}

internal sealed class ControlThemeDefinition
{
    internal ControlThemeDefinition(
        ControlTokenDescriptor descriptor,
        ControlAlgorithmMode algorithmMode,
        IEnumerable<ThemeAlgorithmDescriptor> algorithms,
        IEnumerable<BoundTokenValue> globalTokens,
        IEnumerable<BoundTokenValue> ownTokens,
        ThemeSourceLocation location)
    {
        Descriptor    = descriptor;
        AlgorithmMode = algorithmMode;
        Algorithms    = Array.AsReadOnly(algorithms.ToArray());
        GlobalTokens  = Array.AsReadOnly(globalTokens.ToArray());
        OwnTokens     = Array.AsReadOnly(ownTokens.ToArray());
        Location      = location;
    }

    public ControlTokenDescriptor Descriptor { get; }
    public ControlAlgorithmMode AlgorithmMode { get; }
    public IReadOnlyList<ThemeAlgorithmDescriptor> Algorithms { get; }
    public IReadOnlyList<BoundTokenValue> GlobalTokens { get; }
    public IReadOnlyList<BoundTokenValue> OwnTokens { get; }
    public ThemeSourceLocation Location { get; }
}

internal sealed record PreparedThemeValues(
    IReadOnlyList<BoundTokenValue> Tokens,
    IReadOnlyList<ControlThemeDefinition> Controls);

internal sealed class PreparedThemeDefinition
{
    private readonly object _valuesGate = new();
    private PreparedThemeValues? _values;
    private Func<PreparedThemeValues>? _createValues;
    private bool _creatingValues;
    private readonly Func<ControlTokenIdentity, ControlThemeDefinition>? _createControl;
    private readonly Dictionary<ControlTokenIdentity, ControlThemeDefinition> _controlValues = new();
    private IReadOnlyList<ControlTokenIdentity>? _controlIdentities;
    internal string? ControlRevision { get; }

    internal PreparedThemeDefinition(string id, string name, ThemeAppearance declaredAppearance,
        ThemeAppearance effectiveAppearance, bool isDefault, IEnumerable<ThemeAlgorithmDescriptor> algorithms,
        ThemeSourceLocation location, Func<IReadOnlyList<BoundTokenValue>> createGlobals,
        IReadOnlyList<ControlTokenIdentity> controlIdentities,
        Func<ControlTokenIdentity, ControlThemeDefinition> createControl, string controlRevision)
        : this(id, name, declaredAppearance, effectiveAppearance, isDefault, algorithms, location,
            () => new PreparedThemeValues(createGlobals(), Array.Empty<ControlThemeDefinition>()))
    {
        _controlIdentities = Array.AsReadOnly(controlIdentities.ToArray());
        _createControl = createControl;
        ControlRevision = controlRevision;
    }

    internal IReadOnlyList<ControlTokenIdentity> ControlIdentities
    {
        get
        {
            if (_controlIdentities is { } identities) { return identities; }
            lock (_valuesGate)
            {
                return _controlIdentities ??= Array.AsReadOnly(GetValues().Controls.Select(control => control.Descriptor.Identity).ToArray());
            }
        }
    }

    internal ControlThemeDefinition GetControl(ControlTokenIdentity identity)
    {
        if (_createControl is null) { return GetValues().Controls.Single(control => control.Descriptor.Identity == identity); }
        lock (_valuesGate)
        {
            if (!_controlValues.TryGetValue(identity, out var value))
            {
                value = _createControl(identity);
                _controlValues.Add(identity, value);
            }
            return value;
        }
    }

    internal PreparedThemeDefinition(
        string id, string name, ThemeAppearance declaredAppearance, ThemeAppearance effectiveAppearance,
        bool isDefault, IEnumerable<ThemeAlgorithmDescriptor> algorithms,
        IEnumerable<BoundTokenValue> tokens, IEnumerable<ControlThemeDefinition> controls,
        ThemeSourceLocation location)
        : this(id, name, declaredAppearance, effectiveAppearance, isDefault, algorithms, location,
            () => throw new InvalidOperationException("Definition values were already initialized."))
    {
        _values = new PreparedThemeValues(Array.AsReadOnly(tokens.ToArray()), Array.AsReadOnly(controls.ToArray()));
        _createValues = null;
    }

    internal PreparedThemeDefinition(
        string id, string name, ThemeAppearance declaredAppearance, ThemeAppearance effectiveAppearance,
        bool isDefault, IEnumerable<ThemeAlgorithmDescriptor> algorithms, ThemeSourceLocation location,
        Func<PreparedThemeValues> createValues)
    {
        Id = id;
        Name = name;
        DeclaredAppearance = declaredAppearance;
        EffectiveAppearance = effectiveAppearance;
        IsDefault = isDefault;
        Algorithms = Array.AsReadOnly(algorithms.ToArray());
        Location = location;
        _createValues = createValues;
    }

    public string Id { get; }
    public string Name { get; }
    public ThemeAppearance DeclaredAppearance { get; }
    public ThemeAppearance EffectiveAppearance { get; }
    public bool IsDefault { get; }
    public IReadOnlyList<ThemeAlgorithmDescriptor> Algorithms { get; }
    public IReadOnlyList<BoundTokenValue> Tokens => GetValues().Tokens;
    public IReadOnlyList<ControlThemeDefinition> Controls => _createControl is null ? GetValues().Controls : ControlIdentities.Select(GetControl).ToArray();
    public ThemeSourceLocation Location { get; }

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
