using System.ComponentModel;
using AtomUI.Theme.Schema;

namespace AtomUI.Theme.Definitions.Compiled;

/// <summary>Receives the typed values of a compiled theme program, each to the destination its declaration bound.</summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class CompiledThemeValueWriter
{
    private CompiledThemeBinding? _binding;
    private readonly Dictionary<int, BoundTokenValue> _values = new();
    private readonly ThemeSourceLocation _location;
    private int _section = -1;

    internal CompiledThemeValueWriter(CompiledThemeBinding binding, ThemeSourceLocation location)
    {
        _binding = binding;
        _location = location;
    }

    /// <summary>Opens the section the following values belong to, in the order they were declared.</summary>
    public void BeginSection(int index)
    {
        ObjectDisposedException.ThrowIf(_binding is null, this);
        if (index != _section + 1 || index >= _binding.Sections.Count)
        {
            throw new ThemeSchemaException("Compiled theme sections must be written in their declared order.");
        }
        _section = index;
    }

    public void SetGlobal<T>(int tokenId, T value, string canonicalValue) =>
        Set(-1, tokenId, value, canonicalValue, CompiledTokenKind.Global);

    public void SetLanguage<T>(int tokenId, T value, string canonicalValue) =>
        Set(-1, tokenId, value, canonicalValue, CompiledTokenKind.Language);

    public void SetControlGlobal<T>(int controlId, int tokenId, T value, string canonicalValue)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(controlId);
        Set(controlId, tokenId, value, canonicalValue, CompiledTokenKind.ControlGlobal);
    }

    public void SetOwn<T>(int controlId, int tokenId, T value, string canonicalValue)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(controlId);
        Set(controlId, tokenId, value, canonicalValue, CompiledTokenKind.Own);
    }

    private void Set<T>(int controlId, int tokenId, T value, string canonicalValue, CompiledTokenKind kind)
    {
        ObjectDisposedException.ThrowIf(_binding is null, this);
        ArgumentNullException.ThrowIfNull(canonicalValue);
        if (!_binding.Tokens.TryGetValue(tokenId, out var token))
        {
            // A control value skipped by the global-only startup registry has no destination.
            if (controlId >= 0 && _binding.Controls.TryGetValue(controlId, out var control) && control.Descriptor is null)
            {
                return;
            }
            throw new ThemeSchemaException("Compiled theme value has no declared destination.");
        }
        if (_values.ContainsKey(tokenId) || token.ControlId != controlId || token.Kind != kind ||
            token.Section != _section || token.Descriptor.ValueType != typeof(T))
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

        ThemeSectionDefinition Section(int index)
        {
            var tokens = _values.Where(value => _binding.Tokens[value.Key] is { Section: var section, Kind: CompiledTokenKind.Global } && section == index)
                .Select(static value => value.Value);
            var languageTokens = _values.Where(value => _binding.Tokens[value.Key] is { Section: var section, Kind: CompiledTokenKind.Language } && section == index)
                .Select(static value => value.Value);
            var controls = _binding.Controls.Where(control => control.Value.Section == index && control.Value.Descriptor is not null)
                .Select(control => new ControlThemeDefinition(control.Value.Descriptor!, control.Value.Appearance, control.Value.Density,
                    _values.Where(value => _binding.Tokens[value.Key].ControlId == control.Key &&
                                           _binding.Tokens[value.Key].Kind == CompiledTokenKind.ControlGlobal).Select(static value => value.Value),
                    _values.Where(value => _binding.Tokens[value.Key].ControlId == control.Key &&
                                           _binding.Tokens[value.Key].Kind == CompiledTokenKind.Own).Select(static value => value.Value),
                    _location));
            return new ThemeSectionDefinition(tokens, languageTokens, controls);
        }

        var conditions = new List<ThemeConditionDefinition>(_binding.Sections.Count - 1);
        for (var index = 1; index < _binding.Sections.Count; index++)
        {
            var section = _binding.Sections[index];
            conditions.Add(new ThemeConditionDefinition(section.Style, section.Appearance, section.Density, section.Idiom,
                Section(index), _location));
        }
        return new PreparedThemeValues(Section(0), conditions);
    }

    internal void Close()
    {
        _binding = null;
        _values.Clear();
    }
}
