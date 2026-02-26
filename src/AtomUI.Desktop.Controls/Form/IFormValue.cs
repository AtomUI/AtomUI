namespace AtomUI.Desktop.Controls;

public interface IFormValue : IEnumerable<KeyValuePair<string, object>>
{
    /// <summary>
    /// 获取或设置与指定键关联的值。
    /// </summary>
    object? this[string key] { get; set; }

    /// <summary>
    /// 获取所有键的集合。
    /// </summary>
    ICollection<string> Keys { get; }

    /// <summary>
    /// 获取所有值的集合。
    /// </summary>
    ICollection<object?> Values { get; }

    /// <summary>
    /// 获取包含的键值对数量。
    /// </summary>
    int Count { get; }

    /// <summary>
    /// 判断是否包含指定键。
    /// </summary>
    bool ContainsKey(string key);

    /// <summary>
    /// 添加一个键值对。
    /// </summary>
    void Add(string key, object value);

    /// <summary>
    /// 移除指定键的键值对。
    /// </summary>
    bool Remove(string key);

    /// <summary>
    /// 尝试获取与指定键关联的值。
    /// </summary>
    bool TryGetValue(string key, out object? value);
}