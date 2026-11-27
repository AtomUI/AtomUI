namespace AtomUI.Controls;

/// <summary>
/// The values of the fields of a form, each stored under its field name. A form exposes them as
/// <see cref="AbstractForm.Values" /> and in the arguments of its submit and validation events, and takes them in
/// through <see cref="AbstractForm.InitialValues" /> and <see cref="AbstractForm.SetFormValues(IFormValues)" />.
/// <see cref="FormValues" /> is the mutable implementation.
/// </summary>
public interface IFormValues : IEnumerable<KeyValuePair<string, object>>
{
    /// <summary>
    /// Gets or sets the value stored under <paramref name="key" />. Setting a key that is not stored yet adds it.
    /// A stored value can be <see langword="null" />; use <see cref="TryGetValue" /> or <see cref="ContainsKey" /> to
    /// tell a missing key from a stored <see langword="null" />. Reading a key that is not stored throws
    /// <see cref="System.Collections.Generic.KeyNotFoundException" /> in <see cref="FormValues" />.
    /// </summary>
    /// <param name="key">
    /// The field name.
    /// </param>
    object? this[string key] { get; set; }

    /// <summary>
    /// The names of the fields that have a stored entry, including those whose value is <see langword="null" />.
    /// </summary>
    ICollection<string> Keys { get; }

    /// <summary>
    /// The stored values, one for each name in <see cref="Keys" />, including <see langword="null" /> values.
    /// </summary>
    ICollection<object?> Values { get; }

    /// <summary>
    /// The number of stored entries, counting those whose value is <see langword="null" />.
    /// </summary>
    int Count { get; }

    /// <summary>
    /// Determines whether an entry is stored under <paramref name="key" />, which is also the case when its value is
    /// <see langword="null" />.
    /// </summary>
    /// <param name="key">
    /// The field name.
    /// </param>
    /// <returns>
    /// <see langword="true" /> when the key is stored, otherwise <see langword="false" />.
    /// </returns>
    bool ContainsKey(string key);

    /// <summary>
    /// Stores <paramref name="value" /> under <paramref name="key" />. The key must not be stored yet; use the
    /// indexer to replace a stored value. <see cref="FormValues" /> throws <see cref="System.ArgumentException" />
    /// when the key is stored already.
    /// </summary>
    /// <param name="key">
    /// The field name.
    /// </param>
    /// <param name="value">
    /// The value, which can be <see langword="null" />.
    /// </param>
    void Add(string key, object? value);

    /// <summary>
    /// Removes the entry stored under <paramref name="key" />.
    /// </summary>
    /// <param name="key">
    /// The field name.
    /// </param>
    /// <returns>
    /// <see langword="true" /> when the key was stored and is now removed, otherwise <see langword="false" />.
    /// </returns>
    bool Remove(string key);

    /// <summary>
    /// Gets the value stored under <paramref name="key" /> when the key is stored.
    /// </summary>
    /// <param name="key">
    /// The field name.
    /// </param>
    /// <param name="value">
    /// The stored value, which can be <see langword="null" />, when the key is stored; otherwise
    /// <see langword="null" />.
    /// </param>
    /// <returns>
    /// <see langword="true" /> when the key is stored, otherwise <see langword="false" />.
    /// </returns>
    bool TryGetValue(string key, out object? value);
}