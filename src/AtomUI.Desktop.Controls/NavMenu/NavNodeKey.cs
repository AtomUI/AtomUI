namespace AtomUI.Desktop.Controls;

/// <summary>
/// A key that identifies a navigation node by a string.
/// </summary>
/// <remarks>
/// <para>
/// Two keys are equal when their strings are equal, compared ordinally, and a key is also equal to a
/// <c>string</c> with the same text.
/// </para>
/// </remarks>
public readonly struct NavNodeKey : IEquatable<NavNodeKey>
{
    /// <summary>
    /// Creates a key whose <see cref="Value" /> is <paramref name="value" />.
    /// </summary>
    /// <param name="value">
    /// The key text, stored as given. It is neither validated nor normalized.
    /// </param>
    public NavNodeKey(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Gets the key text. Equality, the hash code and <see cref="ToString" /> are all derived from this value.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Returns whether this key and <paramref name="other" /> have the same text, compared ordinally.
    /// </summary>
    /// <param name="other">
    /// The key to compare with this key.
    /// </param>
    /// <returns>
    /// <see langword="true" /> when both keys have the same text; otherwise <see langword="false" />.
    /// </returns>
    public bool Equals(NavNodeKey other)
    {
        return Value == other.Value;
    }

    /// <summary>
    /// Returns whether <paramref name="obj" /> is a key or a <c>string</c> with the same text as this key.
    /// </summary>
    /// <param name="obj">
    /// A <see cref="NavNodeKey" />, a <c>string</c> or a value of any other type.
    /// </param>
    /// <returns>
    /// <see langword="true" /> when <paramref name="obj" /> is a <see cref="NavNodeKey" /> or a <c>string</c>
    /// with the same text; <see langword="false" /> for values of any other type.
    /// </returns>
    public override bool Equals(object? obj)
    {
        if (obj is NavNodeKey other)
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
    /// Returns the hash code of the key text, so that a key and an equal <c>string</c> share it.
    /// </summary>
    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    /// <summary>
    /// Returns whether two keys have the same text.
    /// </summary>
    public static bool operator ==(NavNodeKey left, NavNodeKey right)
    {
        return left.Equals(right);
    }

    /// <summary>
    /// Returns whether two keys have different text.
    /// </summary>
    public static bool operator !=(NavNodeKey left, NavNodeKey right)
    {
        return !left.Equals(right);
    }

    /// <summary>
    /// Returns whether a key and a <c>string</c> have the same text.
    /// </summary>
    public static bool operator ==(NavNodeKey left, string right)
    {
        return left.Equals(new NavNodeKey(right));
    }

    /// <summary>
    /// Returns whether a key and a <c>string</c> have different text.
    /// </summary>
    public static bool operator !=(NavNodeKey left, string right)
    {
        return !left.Equals(new NavNodeKey(right));
    }

    /// <summary>
    /// Returns <see cref="Value" />, the key text.
    /// </summary>
    public override string ToString()
    {
        return Value;
    }
}