using System.ComponentModel;
using AtomUI.Theme.Schema;

namespace AtomUI.Theme.Definitions.Compiled;

[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class CompiledThemeValueWriter
{
    private CompiledThemeBinding? _binding;
    private BoundTokenValue?[] _values;
    private readonly ThemeSourceLocation _location;

    internal CompiledThemeValueWriter(CompiledThemeBinding binding, ThemeSourceLocation location)
    {
        _binding = binding;
        _values = new BoundTokenValue?[binding.Tokens.Length];
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
        if ((uint)tokenId >= (uint)_values.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(tokenId));
        }
        var token = _binding.Tokens[tokenId];
        if (_values[tokenId] is not null || token.ControlId != controlId || token.Own != own || token.Descriptor.ValueType != typeof(T))
        {
            throw new ThemeSchemaException("Compiled theme value does not match its unique binding destination.");
        }
        _values[tokenId] = new BoundTokenValue(token.Descriptor, TokenValueCodec<T>.Copy(value), _location, canonicalValue);
    }

    internal PreparedThemeValues Complete()
    {
        ObjectDisposedException.ThrowIf(_binding is null, this);
        if (_values.Any(static value => value is null))
        {
            throw new ThemeSchemaException("Compiled theme factory did not write every declared Token.");
        }
        var globals = new List<BoundTokenValue>();
        var controlGlobals = new List<BoundTokenValue>[_binding.Controls.Length];
        var controlOwn = new List<BoundTokenValue>[_binding.Controls.Length];
        for (var index = 0; index < controlGlobals.Length; index++)
        {
            controlGlobals[index] = new List<BoundTokenValue>();
            controlOwn[index] = new List<BoundTokenValue>();
        }
        for (var index = 0; index < _values.Length; index++)
        {
            var token = _binding.Tokens[index];
            var values = token.ControlId < 0 ? globals : token.Own ? controlOwn[token.ControlId] : controlGlobals[token.ControlId];
            values.Add(_values[index]!);
        }
        var controls = new ControlThemeDefinition[_binding.Controls.Length];
        for (var index = 0; index < controls.Length; index++)
        {
            var control = _binding.Controls[index];
            controls[index] = new ControlThemeDefinition(control.Descriptor, control.Mode, control.Algorithms,
                controlGlobals[index], controlOwn[index], _location);
        }
        return new PreparedThemeValues(Array.AsReadOnly(globals.ToArray()), Array.AsReadOnly(controls));
    }

    internal void Close()
    {
        _binding = null;
        _values = Array.Empty<BoundTokenValue>();
    }
}
