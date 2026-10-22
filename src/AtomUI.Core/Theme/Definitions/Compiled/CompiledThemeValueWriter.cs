using System.ComponentModel;
using AtomUI.Theme.Configuration;
using AtomUI.Theme.Schema;

namespace AtomUI.Theme.Definitions.Compiled;

[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class CompiledThemeValueWriter
{
    private CompiledThemeBinding? _binding;
    private readonly Dictionary<int, BoundTokenValue> _values = new();
    private readonly ThemeSourceLocation _location;

    internal CompiledThemeValueWriter(CompiledThemeBinding binding, ThemeSourceLocation location)
    {
        _binding = binding;
        _location = location;
    }

    public void SetGlobal<T>(int tokenId, T value, string canonicalValue) =>
        Set(-1, tokenId, value, canonicalValue, false);

    public void SetControlGlobal<T>(int controlId, int tokenId, T value, string canonicalValue)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(controlId);
        Set(controlId, tokenId, value, canonicalValue, false);
    }

    public void SetOwn<T>(int controlId, int tokenId, T value, string canonicalValue)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(controlId);
        Set(controlId, tokenId, value, canonicalValue, true);
    }

    private void Set<T>(int controlId, int tokenId, T value, string canonicalValue, bool own)
    {
        ObjectDisposedException.ThrowIf(_binding is null, this);
        ArgumentNullException.ThrowIfNull(canonicalValue);
        if (!_binding.Tokens.TryGetValue(tokenId, out var token))
        {
            if (_binding.SelectedControl is { } selected && selected != controlId)
            {
                return;
            }
            throw new ThemeSchemaException("Compiled theme value has no declared destination.");
        }
        if (_values.ContainsKey(tokenId) || token.ControlId != controlId || token.Own != own || token.Descriptor.ValueType != typeof(T))
        {
            throw new ThemeSchemaException("Compiled theme value does not match its unique binding destination.");
        }
        _values[tokenId] = new BoundTokenValue(token.Descriptor, TokenValueCodec<T>.Copy(value), _location, canonicalValue);
    }

    internal PreparedThemeValues Complete()
    {
        ObjectDisposedException.ThrowIf(_binding is null, this);
        if (_values.Count != _binding.Tokens.Count)
        {
            throw new ThemeSchemaException("Compiled theme factory did not write every declared Token.");
        }
        var globals = _values.Where(value => _binding.Tokens[value.Key].ControlId < 0).Select(value => value.Value).ToArray();
        var controls = _binding.Controls.Select(control => new ControlThemeDefinition(control.Value.Descriptor,
            control.Value.Mode, control.Value.Algorithms,
            _values.Where(value => _binding.Tokens[value.Key].ControlId == control.Key && !_binding.Tokens[value.Key].Own).Select(value => value.Value),
            NormalizeOwn(control.Value.Descriptor,
                _values.Where(value => _binding.Tokens[value.Key].ControlId == control.Key && _binding.Tokens[value.Key].Own)
                    .Select(value => value.Value)), _location)).ToArray();
        return new PreparedThemeValues(Array.AsReadOnly(globals), Array.AsReadOnly(controls));
    }

    private static IEnumerable<BoundTokenValue> NormalizeOwn(ControlTokenDescriptor control, IEnumerable<BoundTokenValue> values)
    {
        if (control.OwnTokenAliases.Count == 0)
        {
            return values;
        }
        var diagnostics = new List<ThemeDefinitionDiagnostic>();
        var result = ControlTokenAliasNormalizer.Normalize(control, values.ToArray(), "ATMTHM3004", diagnostics);
        if (diagnostics.Count != 0)
        {
            throw new ThemeSchemaException(diagnostics[0].Message);
        }
        return result;
    }

    internal void Close()
    {
        _binding = null;
        _values.Clear();
    }
}
