using System.Collections;

namespace AtomUI.Desktop.Controls;

public class FormValues : IFormValues
{
    private readonly Dictionary<string, object?> _dictionary = new ();

    public object? this[string key]
    {
        get => _dictionary[key];
        set => _dictionary[key] = value;
    }

    public ICollection<string> Keys => _dictionary.Keys;
    public ICollection<object?> Values => _dictionary.Values;
    public int Count => _dictionary.Count;

    public void Add(string key, object? value) => _dictionary.Add(key, value);
    public bool Remove(string key) => _dictionary.Remove(key);
    public bool ContainsKey(string key) => _dictionary.ContainsKey(key);
    public bool TryGetValue(string key, out object? value) => _dictionary.TryGetValue(key, out value);

    public IEnumerator<KeyValuePair<string, object>> GetEnumerator() => _dictionary.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}