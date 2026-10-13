using System.ComponentModel;
using AtomUI.Theme.Algorithms;
using AtomUI.Theme.Configuration;
using AtomUI.Theme.Schema;

namespace AtomUI.Theme.Definitions.Compiled;

[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class CompiledThemeBindingBuilder
{
    private ThemeSchemaRegistry? _registry;
    private Dictionary<object, TokenDescriptor>? _globalTokens;
    private readonly List<CompiledTokenBinding> _tokens = new();
    private readonly List<CompiledControlBinding> _controls = new();
    private readonly HashSet<(int Control, string Name)> _tokenNames = new();
    private readonly HashSet<ControlTokenIdentity> _controlIdentities = new();
    private readonly Dictionary<int, Dictionary<object, TokenDescriptor>> _ownTokens = new();
    private IReadOnlyList<ThemeAlgorithmDescriptor>? _algorithms;

    internal CompiledThemeBindingBuilder(ThemeSchemaRegistry registry)
    {
        _registry = registry;
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
        GetControl(controlId);
        RequireToken<T>(controlId, tokenId, resourceKey, expectedTokenName, stage, valueContractVersion, false);
    }

    public void RequireOwn<T>(int controlId, int tokenId, object resourceKey, string expectedTokenName,
        int valueContractVersion)
    {
        GetControl(controlId);
        RequireToken<T>(controlId, tokenId, resourceKey, expectedTokenName, TokenStage.Control, valueContractVersion, true);
    }

    public void RequireControl(int controlId, ControlTokenIdentity identity, string expectedCatalog, string expectedId,
        string ownerAssemblyIdentity, string ownerMetadataName, ControlAlgorithmMode mode,
        IReadOnlyList<ThemeAlgorithm> algorithms)
    {
        EnsureOpen();
        if (controlId != _controls.Count || controlId >= ThemeDocumentReaderOptions.DefaultMaxElements)
        {
            throw new ArgumentOutOfRangeException(nameof(controlId), "Compiled control ids must be dense and unique.");
        }
        if (identity.OwnerType is not { } owner || identity.Catalog != expectedCatalog || identity.Id != expectedId ||
            owner.FullName != ownerMetadataName || owner.Assembly.FullName != ownerAssemblyIdentity ||
            !_registry!.TryGetControl(identity, out var descriptor) || descriptor.ControlType != owner)
        {
            throw new ThemeSchemaException($"Compiled theme Control '{expectedCatalog}:{expectedId}' does not match the registered owner.");
        }
        if (!_controlIdentities.Add(identity))
        {
            throw new ThemeSchemaException($"Compiled theme Control '{identity}' is duplicated.");
        }
        ArgumentNullException.ThrowIfNull(algorithms);
        if (!Enum.IsDefined(mode) || ((algorithms.Count != 0) != (mode == ControlAlgorithmMode.Custom)))
        {
            throw new ThemeSchemaException($"Compiled theme Control '{identity}' has an invalid algorithm policy.");
        }
        _controls.Add(new CompiledControlBinding(descriptor, mode, BindAlgorithms(algorithms)));
    }

    private void RequireToken<T>(int controlId, int tokenId, object resourceKey, string name,
        TokenStage stage, int valueContractVersion, bool own)
    {
        EnsureOpen();
        ArgumentNullException.ThrowIfNull(resourceKey);
        SchemaIdentifier.Validate(name, nameof(name));
        if (tokenId != _tokens.Count || tokenId >= ThemeDocumentReaderOptions.DefaultMaxElements)
        {
            throw new ArgumentOutOfRangeException(nameof(tokenId), "Compiled token ids must be dense and unique.");
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
        _tokens.Add(new CompiledTokenBinding(descriptor, controlId, own));
    }

    private CompiledControlBinding GetControl(int controlId)
    {
        EnsureOpen();
        if ((uint)controlId >= (uint)_controls.Count)
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
        var hasTokens = new bool[_controls.Count];
        foreach (var token in _tokens)
        {
            if (token.ControlId >= 0)
            {
                hasTokens[token.ControlId] = true;
            }
        }
        for (var index = 0; index < _controls.Count; index++)
        {
            if (_controls[index].Mode == ControlAlgorithmMode.Unspecified && !hasTokens[index])
            {
                throw new ThemeSchemaException("Compiled Control must declare an algorithm policy or Token.");
            }
        }
        return new CompiledThemeBinding(algorithms, _tokens.ToArray(), _controls.ToArray());
    }

    internal void Close()
    {
        _registry = null;
        _globalTokens = null;
        _algorithms = null;
        _tokens.Clear();
        _controls.Clear();
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
    CompiledTokenBinding[] Tokens, CompiledControlBinding[] Controls);
