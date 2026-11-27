using System.Globalization;
using AtomUI.Utils;

namespace AtomUI.Controls;

/// <summary>
/// The text key of a data item, such as a list item, a tree node, a menu item or a navigation route.
/// Two keys are equal when their texts are equal. A key also equals a string with the same text and has the same
/// hash code as that string.
/// </summary>
public readonly struct EntityKey : IEquatable<EntityKey>
{
    public EntityKey()
    {
        Value = string.Empty;
    }
    
    /// <summary>
    /// Creates a key whose text is <paramref name="value" />, stored as given. Texts that differ in case or in
    /// white space give different keys.
    /// </summary>
    public EntityKey(string value)
    {
        Value = value;
    }

    /// <summary>
    /// The text of the key. It is an empty string for a key created with the parameterless constructor.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Returns a value that indicates whether this key and <paramref name="other" /> have the same text.
    /// </summary>
    public bool Equals(EntityKey other)
    {
        return Value == other.Value;
    }

    /// <summary>
    /// Returns a value that indicates whether <paramref name="obj" /> is an <see cref="EntityKey" /> or a
    /// <see cref="string" /> whose text equals the text of this key.
    /// </summary>
    public override bool Equals(object? obj)
    {
        if (obj is EntityKey other)
        {
            return Equals(other);
        }

        if (obj is string str)
        {
            return Value == str;
        }

        return false;
    }

    /// <summary>
    /// Returns the hash code of the key text.
    /// </summary>
    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    /// <summary>
    /// Returns a value that indicates whether <paramref name="left" /> and <paramref name="right" /> have the same
    /// text.
    /// </summary>
    public static bool operator ==(EntityKey left, EntityKey right)
    {
        return left.Equals(right);
    }

    /// <summary>
    /// Returns a value that indicates whether <paramref name="left" /> and <paramref name="right" /> differ in text.
    /// </summary>
    public static bool operator !=(EntityKey left, EntityKey right)
    {
        return !left.Equals(right);
    }

    /// <summary>
    /// Returns a value that indicates whether the text of <paramref name="left" /> equals <paramref name="right" />.
    /// </summary>
    public static bool operator ==(EntityKey left, string right)
    {
        return left.Equals(new EntityKey(right));
    }

    /// <summary>
    /// Returns a value that indicates whether the text of <paramref name="left" /> differs from
    /// <paramref name="right" />.
    /// </summary>
    public static bool operator !=(EntityKey left, string right)
    {
        return !left.Equals(new EntityKey(right));
    }

    /// <summary>
    /// Creates a key whose text is <paramref name="value" />.
    /// </summary>
    public static implicit operator EntityKey(string value)
    {
        return new EntityKey(value);
    }
    
    /// <summary>
    /// Returns the key text.
    /// </summary>
    public override string ToString()
    {
        return Value;
    }
    
    /// <summary>
    /// Converts the string form of a key to an <see cref="EntityKey" />.
    /// </summary>
    /// <param name="s">
    /// The key text. Leading and trailing white space is ignored.
    /// </param>
    /// <exception cref="System.FormatException">
    /// <paramref name="s" /> is empty or holds more than one token, for example text that contains white space or a
    /// comma.
    /// </exception>
    public static EntityKey Parse(string s)
    {
        using (var tokenizer = new SpanStringTokenizer(s, CultureInfo.InvariantCulture, exceptionMessage: "Invalid EntityKey."))
        {
            return new EntityKey(
                tokenizer.ReadString()
            );
        }
    }
}