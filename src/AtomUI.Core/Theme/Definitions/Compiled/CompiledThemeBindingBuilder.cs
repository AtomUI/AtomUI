using System.ComponentModel;
using System.Reflection;
using AtomUI.Theme.Languages;
using AtomUI.Theme.Schema;

namespace AtomUI.Theme.Definitions.Compiled;

/// <summary>
/// Receives the declarations of a compiled theme program and checks each against the registry: the sections in
/// document order, the controls, and every token's name, type, stage and value contract.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class CompiledThemeBindingBuilder
{
    private ThemeSchemaRegistry? _registry;
    private readonly DesignLanguageDescriptor _language;
    private Dictionary<object, TokenDescriptor>? _globalTokens;
    private readonly Dictionary<int, CompiledTokenBinding> _tokens = new();
    private readonly Dictionary<int, CompiledControlBinding> _controls = new();
    private readonly List<CompiledSectionBinding> _sections = new();
    private readonly HashSet<(int Section, int Control, string Name)> _tokenNames = new();
    private readonly HashSet<(int Section, ControlTokenIdentity Identity)> _controlIdentities = new();
    private readonly Dictionary<int, Dictionary<object, TokenDescriptor>> _ownTokens = new();

    internal CompiledThemeBindingBuilder(ThemeSchemaRegistry registry, DesignLanguageDescriptor language)
    {
        _registry = registry;
        _language = language;
    }

    private int CurrentSection => _sections.Count == 0
        ? throw new InvalidOperationException("A compiled theme section must be opened before its values.")
        : _sections.Count - 1;

    /// <summary>Opens a section: 0 is the base section, later ones are condition sections in document order.</summary>
    /// <param name="appearance">The condition's appearance as its numeric value, or -1 when the condition does not name one.</param>
    public void BeginSection(int index, string? style, int appearance, string? density, string? idiom)
    {
        EnsureOpen();
        var hasCondition = style is not null || appearance != -1 || density is not null || idiom is not null;
        if (index != _sections.Count || (index == 0) == hasCondition)
        {
            throw new ArgumentOutOfRangeException(nameof(index),
                "Compiled theme sections must be declared in order: the base section first without a condition, then condition sections.");
        }
        if (appearance is < -1 or > 1)
        {
            throw new ArgumentOutOfRangeException(nameof(appearance));
        }
        DesignStyle? boundStyle = style is null ? null : new DesignStyle(style);
        DesignDensity? boundDensity = density is null ? null : new DesignDensity(density);
        DesignIdiom? boundIdiom = idiom is null ? null : new DesignIdiom(idiom);
        if ((boundStyle is { } s && !_language.Axes.Styles.Contains(s)) ||
            (boundDensity is { } d && !_language.Axes.Densities.Contains(d)) ||
            (boundIdiom is { } i && !_language.Axes.Idioms.Contains(i)))
        {
            throw new ThemeSchemaException($"A compiled condition section names a value '{_language.Id}' does not declare.");
        }
        _sections.Add(new CompiledSectionBinding(boundStyle, appearance < 0 ? null : (ThemeAppearance)appearance, boundDensity,
            boundIdiom));
    }

    public void RequireGlobal<T>(int tokenId, object resourceKey, string expectedTokenName,
        TokenStage stage, int valueContractVersion) =>
        RequireToken<T>(-1, tokenId, resourceKey, expectedTokenName, stage, valueContractVersion, CompiledTokenKind.Global);

    public void RequireLanguage<T>(int tokenId, string expectedTokenName, TokenStage stage, int valueContractVersion)
    {
        EnsureOpen();
        SchemaIdentifier.Validate(expectedTokenName, nameof(expectedTokenName));
        if (!_language.TryGetLanguageToken(expectedTokenName, out var descriptor) || descriptor.ValueType != typeof(T) ||
            descriptor.Stage != stage || valueContractVersion != 1 || descriptor.GeneratedValueContractVersion != valueContractVersion)
        {
            throw new ThemeSchemaException(
                $"Compiled theme language Token '{expectedTokenName}' does not match '{_language.Id}' (type, stage or value contract).");
        }
        Add(tokenId, -1, expectedTokenName, descriptor, CompiledTokenKind.Language);
    }

    public void RequireControlGlobal<T>(int controlId, int tokenId, object resourceKey, string expectedTokenName,
        TokenStage stage, int valueContractVersion) =>
        RequireToken<T>(controlId, tokenId, resourceKey, expectedTokenName, stage, valueContractVersion, CompiledTokenKind.ControlGlobal);

    public void RequireOwn<T>(int controlId, int tokenId, object resourceKey, string expectedTokenName,
        int valueContractVersion) =>
        RequireToken<T>(controlId, tokenId, resourceKey, expectedTokenName, TokenStage.Control, valueContractVersion, CompiledTokenKind.Own);

    /// <param name="appearance">The fixed appearance as its numeric value, or -1 when the control does not fix one.</param>
    public void RequireControl(int controlId, ControlTokenIdentity identity, string expectedCatalog, string expectedId,
        string ownerAssemblyIdentity, string ownerMetadataName, int appearance, string? density)
    {
        EnsureOpen();
        if (controlId < 0 || controlId >= ThemeDocumentReaderOptions.DefaultMaxElements || _controls.ContainsKey(controlId))
        {
            throw new ArgumentOutOfRangeException(nameof(controlId), "Compiled control ids must be nonnegative and unique.");
        }
        if (appearance is < -1 or > 1 || (CurrentSection != 0 && (appearance != -1 || density is not null)))
        {
            throw new ThemeSchemaException($"Compiled theme Control '{identity}' fixes an axis outside the base section.");
        }
        var owner = identity.OwnerType;
        var ownerAssemblyName = new AssemblyName(ownerAssemblyIdentity).Name;
        if (owner is null || identity.Catalog != expectedCatalog || identity.Id != expectedId ||
            owner.FullName != ownerMetadataName || ownerAssemblyName is null ||
            !string.Equals(owner.Assembly.GetName().Name, ownerAssemblyName, StringComparison.Ordinal))
        {
            throw new ThemeSchemaException($"Compiled theme Control '{expectedCatalog}:{expectedId}' does not match the registered owner.");
        }
        if (!_controlIdentities.Add((CurrentSection, identity)))
        {
            throw new ThemeSchemaException($"Compiled theme Control '{identity}' is duplicated.");
        }
        DesignDensity? boundDensity = density is null ? null : new DesignDensity(density);
        if (boundDensity is { } value && !_language.Axes.Densities.Contains(value))
        {
            throw new ThemeSchemaException($"Compiled theme Control '{identity}' fixes the density '{value}', which '{_language.Id}' does not declare.");
        }
        if (_registry!.IsGlobalOnly)
        {
            // The global-only registry of startup preparation has no control descriptors; control values are skipped.
            _controls.Add(controlId, new CompiledControlBinding(null, CurrentSection, null, null));
            return;
        }
        if (!_registry.TryGetControl(identity, out var descriptor) || descriptor.ControlType != owner ||
            !_registry.MatchesControlRequirement(identity, owner, ownerAssemblyName, ownerMetadataName))
        {
            throw new ThemeSchemaException($"Compiled theme Control '{expectedCatalog}:{expectedId}' is not enabled or does not match the registered owner.");
        }
        _controls.Add(controlId, new CompiledControlBinding(descriptor, CurrentSection,
            appearance < 0 ? null : (ThemeAppearance)appearance, boundDensity));
    }

    private void RequireToken<T>(int controlId, int tokenId, object resourceKey, string name,
        TokenStage stage, int valueContractVersion, CompiledTokenKind kind)
    {
        EnsureOpen();
        ArgumentNullException.ThrowIfNull(resourceKey);
        SchemaIdentifier.Validate(name, nameof(name));
        var own = kind == CompiledTokenKind.Own;
        if (!resourceKey.GetType().IsEnum || !Enum.IsDefined(stage) || own != (stage == TokenStage.Control))
        {
            throw new ThemeSchemaException($"Compiled theme Token '{name}' has an invalid key or stage.");
        }
        TokenDescriptor? descriptor;
        if (controlId >= 0)
        {
            if (!_controls.TryGetValue(controlId, out var control))
            {
                throw new ArgumentOutOfRangeException(nameof(controlId));
            }
            if (control.Descriptor is null)
            {
                // Global-only registry: control values are not bound.
                return;
            }
        }
        if (own)
        {
            if (!_ownTokens.TryGetValue(controlId, out var tokens))
            {
                tokens = _controls[controlId].Descriptor!.OwnTokens.ToDictionary(static token => token.ResourceKey);
                _ownTokens.Add(controlId, tokens);
            }
            tokens.TryGetValue(resourceKey, out descriptor);
        }
        else
        {
            _globalTokens ??= _registry!.GlobalTokens.ToDictionary(static token => token.ResourceKey);
            _globalTokens.TryGetValue(resourceKey, out descriptor);
        }
        if (descriptor is null || descriptor.Name != name || descriptor.ValueType != typeof(T) ||
            descriptor.Stage != stage || valueContractVersion != 1 || descriptor.GeneratedValueContractVersion != valueContractVersion)
        {
            throw new ThemeSchemaException($"Compiled theme Token '{name}' does not match the registered type, key, stage or value contract.");
        }
        Add(tokenId, controlId, name, descriptor, kind);
    }

    private void Add(int tokenId, int controlId, string name, TokenDescriptor descriptor, CompiledTokenKind kind)
    {
        if (tokenId < 0 || _tokens.ContainsKey(tokenId) || tokenId >= ThemeDocumentReaderOptions.DefaultMaxElements)
        {
            throw new ArgumentOutOfRangeException(nameof(tokenId), "Compiled token ids must be nonnegative and unique.");
        }
        var section = controlId >= 0 ? _controls[controlId].Section : CurrentSection;
        if (!_tokenNames.Add((section, controlId, (kind == CompiledTokenKind.Language ? "$" : "") + name)))
        {
            throw new ThemeSchemaException($"Compiled theme Token '{name}' is duplicated.");
        }
        _tokens.Add(tokenId, new CompiledTokenBinding(descriptor, section, controlId, kind));
    }

    internal CompiledThemeBinding Complete()
    {
        EnsureOpen();
        if (_sections.Count == 0)
        {
            throw new ThemeSchemaException("A compiled theme program declares no base section.");
        }
        return new CompiledThemeBinding(_sections.ToArray(), new Dictionary<int, CompiledTokenBinding>(_tokens),
            new Dictionary<int, CompiledControlBinding>(_controls));
    }

    internal void Close()
    {
        _registry = null;
        _globalTokens = null;
        _tokens.Clear();
        _controls.Clear();
        _sections.Clear();
        _tokenNames.Clear();
        _controlIdentities.Clear();
        _ownTokens.Clear();
    }

    private void EnsureOpen() => ObjectDisposedException.ThrowIf(_registry is null, this);
}

internal enum CompiledTokenKind : byte
{
    Global,
    Language,
    ControlGlobal,
    Own
}

internal sealed record CompiledTokenBinding(TokenDescriptor Descriptor, int Section, int ControlId, CompiledTokenKind Kind);

/// <summary>A declared control; its descriptor is <see langword="null"/> under the global-only startup registry.</summary>
internal sealed record CompiledControlBinding(ControlTokenDescriptor? Descriptor, int Section, ThemeAppearance? Appearance,
    DesignDensity? Density);

internal sealed record CompiledSectionBinding(DesignStyle? Style, ThemeAppearance? Appearance, DesignDensity? Density,
    DesignIdiom? Idiom);

internal sealed record CompiledThemeBinding(IReadOnlyList<CompiledSectionBinding> Sections,
    IReadOnlyDictionary<int, CompiledTokenBinding> Tokens, IReadOnlyDictionary<int, CompiledControlBinding> Controls);
