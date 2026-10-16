using System.ComponentModel;
using AtomUI.Theme.Algorithms;
using AtomUI.Theme.Configuration;
using AtomUI.Theme.Schema;
using System.Reflection;

namespace AtomUI.Theme.Definitions.Compiled;

[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class CompiledThemeBindingBuilder
{
    private ThemeSchemaRegistry? _registry;
    private Dictionary<object, TokenDescriptor>? _globalTokens;
    private readonly Dictionary<int, CompiledTokenBinding> _tokens = new();
    private readonly int? _selectedControl;
    private readonly Dictionary<int, ControlTokenIdentity> _declaredControls = new();
    private readonly Dictionary<int, CompiledControlBinding> _controls = new();
    private readonly HashSet<(int Control, string Name)> _tokenNames = new();
    private readonly HashSet<ControlTokenIdentity> _controlIdentities = new();
    private readonly Dictionary<int, Dictionary<object, TokenDescriptor>> _ownTokens = new();
    private IReadOnlyList<ThemeAlgorithmDescriptor>? _algorithms;

    internal CompiledThemeBindingBuilder(ThemeSchemaRegistry registry, int? selectedControl = null)
    {
        _registry = registry;
        _selectedControl = selectedControl;
    }

    public void SetAlgorithms(IReadOnlyList<ThemeAlgorithm> algorithms)
    {
        EnsureOpen();
        if (_algorithms is not null)
        {
            throw new InvalidOperationException("Compiled theme algorithms have already been declared.");
        }
        _algorithms = BindAlgorithms(algorithms);
    }

    public void RequireGlobal<T>(int tokenId, object resourceKey, string expectedTokenName,
        TokenStage stage, int valueContractVersion) =>
        RequireToken<T>(-1, tokenId, resourceKey, expectedTokenName, stage, valueContractVersion, false);

    public void RequireControlGlobal<T>(int controlId, int tokenId, object resourceKey, string expectedTokenName,
        TokenStage stage, int valueContractVersion)
    {
        RequireToken<T>(controlId, tokenId, resourceKey, expectedTokenName, stage, valueContractVersion, false);
    }

    public void RequireOwn<T>(int controlId, int tokenId, object resourceKey, string expectedTokenName,
        int valueContractVersion)
    {
        RequireToken<T>(controlId, tokenId, resourceKey, expectedTokenName, TokenStage.Control, valueContractVersion, true);
    }

    public void RequireControl(int controlId, ControlTokenIdentity identity, string expectedCatalog, string expectedId,
        string ownerAssemblyIdentity, string ownerMetadataName, ControlAlgorithmMode mode,
        IReadOnlyList<ThemeAlgorithm> algorithms)
    {
        EnsureOpen();
        if (controlId < 0 || controlId >= ThemeDocumentReaderOptions.DefaultMaxElements || !_declaredControls.TryAdd(controlId, identity))
        {
            throw new ArgumentOutOfRangeException(nameof(controlId), "Compiled control ids must be nonnegative and unique.");
        }
        ArgumentNullException.ThrowIfNull(algorithms);
        if (!Enum.IsDefined(mode) || ((algorithms.Count != 0) != (mode == ControlAlgorithmMode.Custom)))
        {
            throw new ThemeSchemaException($"Compiled theme Control '{identity}' has an invalid algorithm policy.");
        }
        var owner = identity.OwnerType;
        var ownerAssemblyName = new AssemblyName(ownerAssemblyIdentity).Name;
        if (owner is null || identity.Catalog != expectedCatalog || identity.Id != expectedId ||
            owner.FullName != ownerMetadataName || ownerAssemblyName is null ||
            !string.Equals(owner.Assembly.GetName().Name, ownerAssemblyName, StringComparison.Ordinal))
        {
            throw new ThemeSchemaException($"Compiled theme Control '{expectedCatalog}:{expectedId}' does not match the registered owner.");
        }
        if (_selectedControl == -1)
        {
            if (!_controlIdentities.Add(identity))
            {
                throw new ThemeSchemaException($"Compiled theme Control '{identity}' is duplicated.");
            }
            if (!_registry!.IsGlobalOnly &&
                !_registry.MatchesControlRequirement(identity, owner, ownerAssemblyName, ownerMetadataName))
            {
                throw new ThemeSchemaException($"Compiled theme Control '{expectedCatalog}:{expectedId}' is not enabled or does not match the registered owner.");
            }
            return;
        }
        if (_selectedControl is { } selected && selected != controlId)
        {
            return;
        }
        if (!_registry!.TryGetControl(identity, out var descriptor) || descriptor.ControlType != owner)
        {
            throw new ThemeSchemaException($"Compiled theme Control '{expectedCatalog}:{expectedId}' does not match the registered owner.");
        }
        if (!_controlIdentities.Add(identity))
        {
            throw new ThemeSchemaException($"Compiled theme Control '{identity}' is duplicated.");
        }
        _controls.Add(controlId, new CompiledControlBinding(descriptor, mode, BindAlgorithms(algorithms)));
    }

    private void RequireToken<T>(int controlId, int tokenId, object resourceKey, string name,
        TokenStage stage, int valueContractVersion, bool own)
    {
        EnsureOpen();
        if (_selectedControl is { } selected && selected != controlId)
        {
            return;
        }
        if (controlId >= 0)
        {
            GetControl(controlId);
        }
        ArgumentNullException.ThrowIfNull(resourceKey);
        SchemaIdentifier.Validate(name, nameof(name));
        if (tokenId < 0 || _tokens.ContainsKey(tokenId) || tokenId >= ThemeDocumentReaderOptions.DefaultMaxElements)
        {
            throw new ArgumentOutOfRangeException(nameof(tokenId), "Compiled token ids must be nonnegative and unique.");
        }
        if (!resourceKey.GetType().IsEnum || !Enum.IsDefined(stage) || own != (stage == TokenStage.Control))
        {
            throw new ThemeSchemaException($"Compiled theme Token '{name}' has an invalid key or stage.");
        }
        TokenDescriptor? descriptor;
        if (own)
        {
            if (!_ownTokens.TryGetValue(controlId, out var tokens))
            {
                tokens = GetControl(controlId).Descriptor.OwnTokens.ToDictionary(static token => token.ResourceKey);
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
        if (!_tokenNames.Add((controlId, name)))
        {
            throw new ThemeSchemaException($"Compiled theme Token '{name}' is duplicated.");
        }
        _tokens.Add(tokenId, new CompiledTokenBinding(descriptor, controlId, own));
    }

    private CompiledControlBinding GetControl(int controlId)
    {
        EnsureOpen();
        if (!_controls.ContainsKey(controlId))
        {
            throw new ArgumentOutOfRangeException(nameof(controlId));
        }
        return _controls[controlId];
    }

    private IReadOnlyList<ThemeAlgorithmDescriptor> BindAlgorithms(IReadOnlyList<ThemeAlgorithm> algorithms)
    {
        ArgumentNullException.ThrowIfNull(algorithms);
        if (algorithms.Count > ThemeDocumentReaderOptions.DefaultMaxElements)
        {
            throw new ArgumentOutOfRangeException(nameof(algorithms));
        }
        var result = new ThemeAlgorithmDescriptor[algorithms.Count];
        for (var index = 0; index < result.Length; index++)
        {
            if (!_registry!.TryGetAlgorithm(algorithms[index], out var descriptor))
            {
                throw new ThemeSchemaException($"Compiled theme algorithm '{algorithms[index]}' is not registered.");
            }
            result[index] = descriptor;
        }
        return Array.AsReadOnly(result);
    }

    internal CompiledThemeBinding Complete(ThemeAppearance appearance)
    {
        EnsureOpen();
        var algorithms = _algorithms ?? Array.Empty<ThemeAlgorithmDescriptor>();
        if (ThemeDefinitionBinder.ResolveAppearance(algorithms) != appearance)
        {
            throw new ThemeSchemaException("Compiled theme appearance does not match the registered algorithms.");
        }
        foreach (var control in _controls)
        {
            if (control.Value.Mode == ControlAlgorithmMode.Unspecified && !_tokens.Values.Any(token => token.ControlId == control.Key))
            {
                throw new ThemeSchemaException("Compiled Control must declare an algorithm policy or Token.");
            }
        }
        return new CompiledThemeBinding(algorithms, new Dictionary<int, CompiledTokenBinding>(_tokens),
            new Dictionary<int, CompiledControlBinding>(_controls), new Dictionary<int, ControlTokenIdentity>(_declaredControls), _selectedControl);
    }

    internal void Close()
    {
        _registry = null;
        _globalTokens = null;
        _algorithms = null;
        _tokens.Clear();
        _controls.Clear();
        _declaredControls.Clear();
        _tokenNames.Clear();
        _controlIdentities.Clear();
        _ownTokens.Clear();
    }

    private void EnsureOpen() => ObjectDisposedException.ThrowIf(_registry is null, this);
}

internal sealed record CompiledTokenBinding(TokenDescriptor Descriptor, int ControlId, bool Own);
internal sealed record CompiledControlBinding(ControlTokenDescriptor Descriptor, ControlAlgorithmMode Mode,
    IReadOnlyList<ThemeAlgorithmDescriptor> Algorithms);
internal sealed record CompiledThemeBinding(IReadOnlyList<ThemeAlgorithmDescriptor> Algorithms,
    IReadOnlyDictionary<int, CompiledTokenBinding> Tokens, IReadOnlyDictionary<int, CompiledControlBinding> Controls,
    IReadOnlyDictionary<int, ControlTokenIdentity> DeclaredControls, int? SelectedControl);
