using System.Collections;

namespace AtomUI.Controls;

/// <summary>
/// A mutable, dictionary-backed implementation of <see cref="IFormValues" />: field values keyed by field
/// name, compared ordinally, and a value can be <see langword="null" />. An application can create an
/// instance to pass values through <see cref="AbstractForm.InitialValues" /> or
/// <see cref="AbstractForm.SetFormValues(IFormValues)" />.
/// </summary>
public class FormValues : IFormValues
{
    private readonly Dictionary<string, object?> _dictionary = new ();

    /// <summary>
    /// Gets or sets the value of the field <paramref name="key" />. Setting a key that is not stored yet adds it.
    /// Getting a key that is not stored throws, whereas a stored value can be <see langword="null" />. Use
    /// <see cref="TryGetValue" /> or <see cref="ContainsKey" /> to tell a missing key from a stored
    /// <see langword="null" />.
    /// </summary>
    /// <param name="key">
    /// The field name.
    /// </param>
    /// <exception cref="System.Collections.Generic.KeyNotFoundException">
    /// The key is not stored.
    /// </exception>
    public object? this[string key]
    {
        get => _dictionary[key];
        set => _dictionary[key] = value;
    }

    /// <summary>
    /// The stored field names, as a live view of the store that reflects later changes.
    /// </summary>
    public ICollection<string> Keys => _dictionary.Keys;
    /// <summary>
    /// The stored values, as a live view of the store that reflects later changes.
    /// </summary>
    public ICollection<object?> Values => _dictionary.Values;
    /// <inheritdoc />
    public int Count => _dictionary.Count;

    /// <summary>
    /// Adds a field value. The key must not be stored yet; assign through the indexer to replace a stored value.
    /// </summary>
    /// <param name="key">
    /// The field name.
    /// </param>
    /// <param name="value">
    /// The value, which can be <see langword="null" />.
    /// </param>
    /// <exception cref="System.ArgumentException">
    /// <paramref name="key" /> is already stored.
    /// </exception>
    public void Add(string key, object? value) => _dictionary.Add(key, value);
    /// <summary>
    /// Removes the field <paramref name="key" /> from the store.
    /// </summary>
    /// <param name="key">
    /// The field name.
    /// </param>
    /// <returns>
    /// <see langword="true" /> when the key was stored and is now removed, otherwise <see langword="false" />.
    /// </returns>
    public bool Remove(string key) => _dictionary.Remove(key);
    /// <inheritdoc />
    public bool ContainsKey(string key) => _dictionary.ContainsKey(key);
    /// <summary>
    /// Gets the value of the field <paramref name="key" /> when the key is stored.
    /// </summary>
    /// <param name="key">
    /// The field name.
    /// </param>
    /// <param name="value">
    /// The stored value, which can be <see langword="null" />, when the key is found; otherwise
    /// <see langword="null" />.
    /// </param>
    /// <returns>
    /// <see langword="true" /> when the key is stored, otherwise <see langword="false" />.
    /// </returns>
    public bool TryGetValue(string key, out object? value) => _dictionary.TryGetValue(key, out value);

    /// <inheritdoc />
    public IEnumerator<KeyValuePair<string, object>> GetEnumerator() => _dictionary.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}