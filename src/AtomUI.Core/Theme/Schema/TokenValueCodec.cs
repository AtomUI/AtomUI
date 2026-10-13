using System.ComponentModel;

namespace AtomUI.Theme.Schema;

[EditorBrowsable(EditorBrowsableState.Never)]
public abstract class TokenValueCodec
{
    private protected TokenValueCodec()
    {
    }

    internal abstract Type ValueType { get; }

    internal abstract object? Parse(string value);

    internal abstract string Format(object? value);
    internal virtual bool SupportsCompiledValues => false;
}

[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class TokenValueCodec<T> : TokenValueCodec
{
    public static readonly TokenValueCodec<T> Instance = new();

    private TokenValueCodec()
    {
    }

    /// <summary>Copies a supported generated value without formatting or parsing a literal.</summary>
    public static T Copy(T value)
    {
        if ((typeof(T).IsValueType && typeof(T) != typeof(Avalonia.Media.BoxShadows) &&
             typeof(T) != typeof(Avalonia.Media.BoxShadows?)) || value is null || value is string)
        {
            return value;
        }
        if (TokenValueOperations.TryCopy(value, out var copy) && copy is T typed)
        {
            return typed;
        }
        throw new NotSupportedException($"Token type '{typeof(T).FullName}' does not support generated typed copies.");
    }

    internal override Type ValueType => typeof(T);
    internal override bool SupportsCompiledValues => TokenValueOperations.IsCompiledType(typeof(T));

    internal override object? Parse(string value) => ThemeTokenValueParser.Parse<T>(value);

    internal override string Format(object? value) => ThemeTokenValueFormatter.Format((T)value!);
}

internal sealed class DelegateTokenValueCodec : TokenValueCodec
{
    private readonly Func<string, object?> _parser;
    private readonly Func<object?, string> _formatter;

    internal DelegateTokenValueCodec(Type valueType, Func<string, object?> parser, Func<object?, string> formatter)
    {
        ValueType  = valueType;
        _parser    = parser;
        _formatter = formatter;
    }

    internal override Type ValueType { get; }

    internal override object? Parse(string value) => _parser(value);

    internal override string Format(object? value) => _formatter(value);
}
