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

    /// <summary>
    /// Opens a section: 0 is the base section, later ones are condition sections in document order.
    /// The base section takes no condition. A condition section must name at least one axis and applies to the variants
    /// that match every axis it names. Each axis value it names must be declared by the program's language.
    /// </summary>
    /// <param name="index">
    /// The section's position, which must equal the number of sections opened so far.
    /// </param>
    /// <param name="style">
    /// The style the condition applies to, or <see langword="null" /> when the condition does not name one.
    /// </param>
    /// <param name="appearance">
    /// The condition's appearance as its numeric value: 0 is <see cref="ThemeAppearance.Light" /> and 1 is
    /// <see cref="ThemeAppearance.Dark" />. Pass -1 when the condition does not name one.
    /// </param>
    /// <param name="density">
    /// The density the condition applies to, or <see langword="null" /> when the condition does not name one.
    /// </param>
    /// <param name="idiom">
    /// The idiom the condition applies to, or <see langword="null" /> when the condition does not name one.
    /// </param>
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

    /// <summary>
    /// Declares that the program writes the global token <paramref name="expectedTokenName" /> in the open section.
    /// A global token may be declared once per section; declaring it again in a condition section overrides the
    /// earlier value. A declaration that does not match the registered token throws <c>ThemeSchemaException</c>.
    /// </summary>
    /// <typeparam name="T">
    /// The token's value type, which must be the registered value type.
    /// </typeparam>
    /// <param name="tokenId">
    /// The id the value is written under. It must be nonnegative and unique among all tokens of the program.
    /// </param>
    /// <param name="resourceKey">
    /// The enum member that keys the token among the registered global tokens.
    /// </param>
    /// <param name="expectedTokenName">
    /// The name the token is registered under.
    /// </param>
    /// <param name="stage">
    /// The stage the token is registered with. <see cref="TokenStage.Control" /> is rejected.
    /// </param>
    /// <param name="valueContractVersion">
    /// The version of the value contract the token's codec must support. Pass 1; it is the only version that lets a
    /// compiled program write a token value.
    /// </param>
    public void RequireGlobal<T>(int tokenId, object resourceKey, string expectedTokenName,
        TokenStage stage, int valueContractVersion) =>
        RequireToken<T>(-1, tokenId, resourceKey, expectedTokenName, stage, valueContractVersion, CompiledTokenKind.Global);

    /// <summary>
    /// Declares that the program writes the language token <paramref name="expectedTokenName" /> of the program's
    /// language in the open section. A language token may be declared once per section, and it may share its name with
    /// a global token. A declaration that does not match the language's token throws <c>ThemeSchemaException</c>.
    /// </summary>
    /// <typeparam name="T">
    /// The token's value type, which must be the value type the language gives the token.
    /// </typeparam>
    /// <param name="tokenId">
    /// The id the value is written under. It must be nonnegative and unique among all tokens of the program.
    /// </param>
    /// <param name="expectedTokenName">
    /// The name the language registered the token under.
    /// </param>
    /// <param name="stage">
    /// The stage the language registered the token with.
    /// </param>
    /// <param name="valueContractVersion">
    /// The version of the value contract the token's codec must support. Pass 1; it is the only version that lets a
    /// compiled program write a token value.
    /// </param>
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

    /// <summary>
    /// Declares that the control <paramref name="controlId" /> overrides the global token
    /// <paramref name="expectedTokenName" /> with a value of its own; the value applies to that control only. A
    /// declaration that does not match the registered token throws <c>ThemeSchemaException</c>.
    /// Under the global-only registry of startup preparation the declaration has no effect.
    /// </summary>
    /// <typeparam name="T">
    /// The token's value type, which must be the registered value type.
    /// </typeparam>
    /// <param name="controlId">
    /// The id given to the control by <see cref="RequireControl" />.
    /// </param>
    /// <param name="tokenId">
    /// The id the value is written under. It must be nonnegative and unique among all tokens of the program.
    /// </param>
    /// <param name="resourceKey">
    /// The enum member that keys the token among the registered global tokens.
    /// </param>
    /// <param name="expectedTokenName">
    /// The name the token is registered under.
    /// </param>
    /// <param name="stage">
    /// The stage the token is registered with. <see cref="TokenStage.Control" /> is rejected.
    /// </param>
    /// <param name="valueContractVersion">
    /// The version of the value contract the token's codec must support. Pass 1; it is the only version that lets a
    /// compiled program write a token value.
    /// </param>
    public void RequireControlGlobal<T>(int controlId, int tokenId, object resourceKey, string expectedTokenName,
        TokenStage stage, int valueContractVersion) =>
        RequireToken<T>(controlId, tokenId, resourceKey, expectedTokenName, stage, valueContractVersion, CompiledTokenKind.ControlGlobal);

    /// <summary>
    /// Declares that the control <paramref name="controlId" /> sets its own token
    /// <paramref name="expectedTokenName" />, which is looked up among the tokens the control owns. A declaration that
    /// does not match that token throws <c>ThemeSchemaException</c>.
    /// Under the global-only registry of startup preparation the declaration has no effect.
    /// </summary>
    /// <typeparam name="T">
    /// The token's value type, which must be the value type the control gives the token.
    /// </typeparam>
    /// <param name="controlId">
    /// The id given to the control by <see cref="RequireControl" />.
    /// </param>
    /// <param name="tokenId">
    /// The id the value is written under. It must be nonnegative and unique among all tokens of the program.
    /// </param>
    /// <param name="resourceKey">
    /// The enum member that keys the token among the tokens the control owns.
    /// </param>
    /// <param name="expectedTokenName">
    /// The name the control registered the token under.
    /// </param>
    /// <param name="valueContractVersion">
    /// The version of the value contract the token's codec must support. Pass 1; it is the only version that lets a
    /// compiled program write a token value.
    /// </param>
    public void RequireOwn<T>(int controlId, int tokenId, object resourceKey, string expectedTokenName,
        int valueContractVersion) =>
        RequireToken<T>(controlId, tokenId, resourceKey, expectedTokenName, TokenStage.Control, valueContractVersion, CompiledTokenKind.Own);

    /// <summary>
    /// Declares a control that the open section configures and checks it against the registry: it must be registered
    /// for the owner given here, or a <c>ThemeSchemaException</c> is thrown. A control may be declared once per
    /// section, and only in the base section may it fix an appearance or a density.
    /// Under the global-only registry of startup preparation the control is recorded without a descriptor, and the
    /// tokens declared for it are ignored.
    /// </summary>
    /// <param name="controlId">
    /// The id the program uses for the control in <see cref="RequireControlGlobal" />, <see cref="RequireOwn" /> and
    /// the control overloads of <c>CompiledThemeValueWriter.Set*</c>. It must be nonnegative and unique in the program.
    /// </param>
    /// <param name="identity">
    /// The control's identity, such as the <c>Identity</c> of its token class. Its catalog, id and owner type must
    /// match the <c>expected</c> and <c>owner</c> arguments.
    /// </param>
    /// <param name="expectedCatalog">
    /// The catalog the control is expected to belong to.
    /// </param>
    /// <param name="expectedId">
    /// The id the control is expected to have in that catalog.
    /// </param>
    /// <param name="ownerAssemblyIdentity">
    /// The display name of the assembly that owns the control type. Only its simple name is compared.
    /// </param>
    /// <param name="ownerMetadataName">
    /// The full name of the type that owns the control, such as <c>My.Controls.MyButton</c>.
    /// </param>
    /// <param name="appearance">
    /// The fixed appearance as its numeric value: 0 is <see cref="ThemeAppearance.Light" /> and 1 is
    /// <see cref="ThemeAppearance.Dark" />. Pass -1 when the control does not fix one.
    /// </param>
    /// <param name="density">
    /// The fixed density as the language declares it, or <see langword="null" /> when the control does not fix one.
    /// </param>
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

/// <summary>
/// A declared control; its descriptor is <see langword="null" /> under the global-only startup registry.
/// </summary>
internal sealed record CompiledControlBinding(ControlTokenDescriptor? Descriptor, int Section, ThemeAppearance? Appearance,
    DesignDensity? Density);

internal sealed record CompiledSectionBinding(DesignStyle? Style, ThemeAppearance? Appearance, DesignDensity? Density,
    DesignIdiom? Idiom);

internal sealed record CompiledThemeBinding(IReadOnlyList<CompiledSectionBinding> Sections,
    IReadOnlyDictionary<int, CompiledTokenBinding> Tokens, IReadOnlyDictionary<int, CompiledControlBinding> Controls);
