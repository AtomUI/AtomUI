using System.Collections;

namespace AtomUI.Controls.Data;

/// <summary>
/// Reads, writes and compares one named data member of items of one type, without reflection.
/// </summary>
/// <remarks>
/// <para>
/// An <see cref="IDataMemberAccessorDescriptor" /> holds the accessors of a data type. Data-bound controls use
/// them in place of reflection on a member path, which NativeAOT and trimming do not support.
/// </para>
/// </remarks>
public interface IDataMemberAccessor
{
    /// <summary>
    /// Gets the key that identifies the member within its descriptor. Generated accessors use the name of the member.
    /// </summary>
    string Path { get; }

    /// <summary>
    /// Gets the declared type of the member's value. A nullable value type keeps its nullable type, so that a consumer
    /// can order <see langword="null" /> values.
    /// </summary>
    Type ValueType { get; }

    /// <summary>
    /// Gets the comparer that orders the values of the member, or <see langword="null" /> when the accessor has none.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The comparer receives the values that <see cref="GetValue" /> returns, not the items that hold them.
    /// </para>
    /// </remarks>
    IComparer? Comparer { get; }

    /// <summary>
    /// Gets whether <see cref="SetValue" /> can write the member.
    /// </summary>
    bool CanWrite { get; }

    /// <summary>
    /// Reads the member's value from an item.
    /// </summary>
    /// <param name="instance">
    /// The item to read. It must be an instance of the data type of the accessor.
    /// </param>
    /// <returns>
    /// The member's value.
    /// </returns>
    object? GetValue(object instance);

    /// <summary>
    /// Writes a member value to an item.
    /// </summary>
    /// <param name="instance">
    /// The item to update. It must be an instance of the data type of the accessor.
    /// </param>
    /// <param name="value">
    /// The value to write. It must have the type <see cref="ValueType" />; <see langword="null" /> writes the default
    /// value of that type.
    /// </param>
    /// <exception cref="InvalidOperationException">
    /// <see cref="CanWrite" /> is <see langword="false" />.
    /// </exception>
    void SetValue(object instance, object? value);
}
