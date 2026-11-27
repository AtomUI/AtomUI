using System.ComponentModel;
using AtomUI.Theme.DesignTokens;

namespace AtomUI.Theme.Schema;

/// <summary>
/// Describes one design token for the theme system: its name, its slot in the token class that owns it, its
/// stage, its value type and its resource key. The descriptor also converts the token's value to and from literal
/// text and reads and writes it on a token instance.
/// </summary>
public sealed class TokenDescriptor
{
    private readonly TokenValueCodec _codec;
    private readonly TokenValueAccessor _accessor;

    /// <summary>
    /// Creates a descriptor whose value is converted, read and written through per-token delegates.
    /// </summary>
    /// <param name="name">
    /// The token name; it must be a valid schema identifier.
    /// </param>
    /// <param name="slot">
    /// The position of the token in the token class that owns it; it must not be negative.
    /// </param>
    /// <param name="stage">
    /// The stage the token belongs to.
    /// </param>
    /// <param name="valueType">
    /// The declared type of the token's value.
    /// </param>
    /// <param name="resourceKey">
    /// The key under which the token's value is exposed as a theme resource.
    /// </param>
    /// <param name="parser">
    /// Converts the literal text of a value into the value.
    /// </param>
    /// <param name="formatter">
    /// Converts a value into its literal text.
    /// </param>
    /// <param name="getter">
    /// Reads the value from a token instance.
    /// </param>
    /// <param name="setter">
    /// Writes the value to a token instance.
    /// </param>
    /// <param name="resourceProjector">
    /// Converts the value of a token instance into the value a theme resource exposes.
    /// </param>
    public TokenDescriptor(
        string name,
        int slot,
        TokenStage stage,
        Type valueType,
        object resourceKey,
        Func<string, object?> parser,
        Func<object?, string> formatter,
        Func<AbstractDesignToken, object?> getter,
        Action<AbstractDesignToken, object?> setter,
        Func<AbstractDesignToken, object?> resourceProjector)
    {
        SchemaIdentifier.Validate(name, nameof(name));
        ArgumentOutOfRangeException.ThrowIfNegative(slot);
        ArgumentNullException.ThrowIfNull(valueType);
        ArgumentNullException.ThrowIfNull(resourceKey);
        ArgumentNullException.ThrowIfNull(parser);
        ArgumentNullException.ThrowIfNull(formatter);
        ArgumentNullException.ThrowIfNull(getter);
        ArgumentNullException.ThrowIfNull(setter);
        ArgumentNullException.ThrowIfNull(resourceProjector);

        Name        = name;
        Slot        = slot;
        Stage       = stage;
        ValueType   = valueType;
        ResourceKey = resourceKey;
        _codec      = new DelegateTokenValueCodec(valueType, parser, formatter);
        _accessor   = new DelegateTokenValueAccessor(getter, setter, resourceProjector);
    }

    private TokenDescriptor(
        string name,
        int slot,
        TokenStage stage,
        TokenValueCodec codec,
        object resourceKey,
        TokenValueAccessor accessor)
    {
        SchemaIdentifier.Validate(name, nameof(name));
        ArgumentOutOfRangeException.ThrowIfNegative(slot);
        ArgumentNullException.ThrowIfNull(codec);
        ArgumentNullException.ThrowIfNull(resourceKey);
        ArgumentNullException.ThrowIfNull(accessor);

        Name        = name;
        Slot        = slot;
        Stage       = stage;
        ValueType   = codec.ValueType;
        ResourceKey = resourceKey;
        _codec      = codec;
        _accessor   = accessor;
    }

    /// <summary>
    /// Generated-code entry point: the value codec is shared per value type and the accessor per token class,
    /// so a descriptor carries no per-token delegates.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static TokenDescriptor CreateGenerated(
        string name,
        int slot,
        TokenStage stage,
        TokenValueCodec codec,
        object resourceKey,
        TokenValueAccessor accessor) =>
        new(name, slot, stage, codec, resourceKey, accessor);

    /// <summary>
    /// Generated-code entry point that also records the generated value contract.
    /// <paramref name="valueContractVersion" /> must be <c>1</c> and the codec must support compiled values. A theme
    /// definition binds to the token only when it was built for the recorded contract version.
    /// </summary>
    /// <exception cref="ArgumentException">
    /// <paramref name="valueContractVersion" /> is not <c>1</c>, or <paramref name="codec" /> does not support compiled
    /// values.
    /// </exception>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static TokenDescriptor CreateGenerated(
        string name, int slot, TokenStage stage, TokenValueCodec codec, object resourceKey,
        TokenValueAccessor accessor, int valueContractVersion)
    {
        ArgumentNullException.ThrowIfNull(codec);
        if (valueContractVersion != 1 || !codec.SupportsCompiledValues)
        {
            throw new ArgumentException("The codec does not support the generated value contract.", nameof(valueContractVersion));
        }
        return new TokenDescriptor(name, slot, stage, codec, resourceKey, accessor)
        {
            GeneratedValueContractVersion = valueContractVersion
        };
    }

    internal int GeneratedValueContractVersion { get; private init; }

    /// <summary>
    /// The name of the token. Lookups by name compare it ordinally.
    /// </summary>
    public string Name { get; }
    /// <summary>
    /// The position of the token in the token class that owns it. The generated accessor uses it to select the token
    /// member to read or write, and global and language tokens are ordered by it.
    /// </summary>
    public int Slot { get; }
    /// <summary>
    /// The stage the token belongs to. The theme compiler reads it to reject writes to seeds, and it is part of the
    /// theme cache key and of the registry revision.
    /// </summary>
    public TokenStage Stage { get; }
    /// <summary>
    /// The declared type of the token's value.
    /// </summary>
    public Type ValueType { get; }
    /// <summary>
    /// The key under which the token's value is exposed as a theme resource. Resource lookups and the resources of a
    /// compiled theme use it.
    /// </summary>
    public object ResourceKey { get; }

    /// <summary>
    /// Converts the literal text of the token's value back into a value, as <see cref="Format" /> writes it.
    /// </summary>
    public object? Parse(string value) => _codec.Parse(value);

    /// <summary>
    /// Converts a value into the literal text that <see cref="Parse" /> reads back.
    /// </summary>
    public string Format(object? value) => _codec.Format(value);

    internal object? CopyValue(object? value)
    {
        if (value is null)
        {
            return null;
        }
        // A runtime brush/easing subtype does not certify its declared codec contract.
        // Dynamic descriptors must retain concrete types their accessors can accept.
        if (TokenValueOperations.IsCompiledType(ValueType) &&
            TokenValueOperations.TryCopy(value, out var copy) &&
            (copy is null || ValueType.IsInstanceOfType(copy)))
        {
            return copy;
        }
        return _codec.Parse(_codec.Format(value));
    }

    /// <summary>
    /// Reads the value of the token at <see cref="Slot" /> from a token instance.
    /// </summary>
    public object? GetValue(AbstractDesignToken token)
    {
        ArgumentNullException.ThrowIfNull(token);
        return _accessor.GetValue(token, Slot);
    }

    /// <summary>
    /// Writes the value of the token at <see cref="Slot" /> to a token instance.
    /// </summary>
    public void SetValue(AbstractDesignToken token, object? value)
    {
        ArgumentNullException.ThrowIfNull(token);
        _accessor.SetValue(token, Slot, value);
    }

    /// <summary>
    /// Converts the value of the token at <see cref="Slot" /> into the value a theme resource exposes. The theme
    /// compiler stores that value under <see cref="ResourceKey" />.
    /// </summary>
    public object? ProjectResourceValue(AbstractDesignToken token)
    {
        ArgumentNullException.ThrowIfNull(token);
        return _accessor.ProjectResourceValue(token, Slot);
    }
}
