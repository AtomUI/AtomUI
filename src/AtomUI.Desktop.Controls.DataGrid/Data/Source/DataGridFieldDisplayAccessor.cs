using AtomUI.Controls.Data;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Describes the AOT-safe member access used only to display an item field.
/// </summary>
public sealed class DataGridFieldDisplayAccessor
{
    private readonly Func<object, object?> _getter;
    private readonly Action<object, object?>? _setter;

    private DataGridFieldDisplayAccessor(
        Type itemType,
        Type valueType,
        Func<object, object?> getter,
        Action<object, object?>? setter,
        string? propertyChangedName)
    {
        ItemType = itemType;
        ValueType = valueType;
        _getter = getter;
        _setter = setter;
        PropertyChangedName = ValidatePropertyChangedName(propertyChangedName);
    }

    /// <summary>
    /// The type of data item the accessor reads and writes. It has to equal the item type of the schema that
    /// holds the field.
    /// </summary>
    public Type ItemType { get; }

    /// <summary>
    /// The type of value the getter returns and the setter accepts. It has to equal the value type of the field
    /// schema the accessor is attached to.
    /// </summary>
    public Type ValueType { get; }

    /// <summary>
    /// <see langword="true" /> when a setter was supplied. A write to a read-only accessor throws
    /// <see cref="InvalidOperationException" />.
    /// </summary>
    public bool CanWrite => _setter is not null;

    /// <summary>
    /// Gets the CLR property name observed through INotifyPropertyChanged. Null observes every change.
    /// </summary>
    public string? PropertyChangedName { get; }

    /// <summary>
    /// Creates an accessor from typed delegates, so the field is read and written without reflection.
    /// </summary>
    /// <param name="getter">
    /// Reads the value of the field from an item.
    /// </param>
    /// <param name="setter">
    /// Writes an edited value back. <see langword="null" /> creates a read-only accessor, and a
    /// <see langword="null" /> value arrives as the default value of <typeparamref name="TValue" />.
    /// </param>
    /// <param name="propertyChangedName">
    /// The name of the item property whose <c>INotifyPropertyChanged</c> notification refreshes this field, or
    /// <see langword="null" /> to refresh on every change.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="getter" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="propertyChangedName" /> is empty, not trimmed or contains a control character.
    /// </exception>
    public static DataGridFieldDisplayAccessor Create<TItem, TValue>(
        Func<TItem, TValue> getter,
        Action<TItem, TValue>? setter = null,
        string? propertyChangedName = null)
    {
        ArgumentNullException.ThrowIfNull(getter);
        return new DataGridFieldDisplayAccessor(
            typeof(TItem),
            typeof(TValue),
            item => getter((TItem)item),
            setter is null
                ? null
                : (item, value) => setter((TItem)item, CastValue<TValue>(value)),
            propertyChangedName);
    }

    /// <summary>
    /// Wraps a data member accessor and takes the <c>Path</c> of <paramref name="accessor" /> as the name of the
    /// property whose change notification refreshes the field.
    /// </summary>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="accessor" /> is <see langword="null" />.
    /// </exception>
    public static DataGridFieldDisplayAccessor FromDataMember<TItem>(IDataMemberAccessor accessor) =>
        FromDataMember<TItem>(accessor, accessor?.Path);

    /// <summary>
    /// Wraps a data member accessor, so the field is read and written through that accessor.
    /// </summary>
    /// <param name="accessor">
    /// Supplies the value type, the getter and, when it can write, the setter.
    /// </param>
    /// <param name="propertyChangedName">
    /// The name of the item property whose <c>INotifyPropertyChanged</c> notification refreshes this field, or
    /// <see langword="null" /> to refresh on every change.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="accessor" /> is <see langword="null" />.
    /// </exception>
    public static DataGridFieldDisplayAccessor FromDataMember<TItem>(
        IDataMemberAccessor accessor,
        string? propertyChangedName)
    {
        ArgumentNullException.ThrowIfNull(accessor);
        return new DataGridFieldDisplayAccessor(
            typeof(TItem),
            accessor.ValueType,
            accessor.GetValue,
            accessor.CanWrite ? accessor.SetValue : null,
            propertyChangedName);
    }

    internal object? GetValue(object item) => _getter(item);

    internal void SetValue(object item, object? value)
    {
        if (_setter is null)
        {
            throw new InvalidOperationException("The display accessor is read-only.");
        }
        _setter(item, value);
    }

    private static TValue CastValue<TValue>(object? value) =>
        value is null ? default! : (TValue)value;

    private static string? ValidatePropertyChangedName(string? propertyChangedName)
    {
        if (propertyChangedName is null)
        {
            return null;
        }
        if (propertyChangedName.Length == 0 ||
            !string.Equals(propertyChangedName, propertyChangedName.Trim(), StringComparison.Ordinal) ||
            propertyChangedName.Any(char.IsControl))
        {
            throw new ArgumentException(
                "The property-changed name must be non-empty, trimmed, and contain no control characters.",
                nameof(propertyChangedName));
        }
        return propertyChangedName;
    }
}
