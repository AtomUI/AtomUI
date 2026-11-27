namespace AtomUI.Controls.Data;

/// <summary>
/// Describes the data members of one type as a list of named accessors, so that data-bound controls can resolve a
/// member path without reflection.
/// </summary>
/// <remarks>
/// <para>
/// A descriptor comes from <see cref="GenerateDataMemberAccessorsAttribute" />, or from a hand-written
/// <see cref="DataMemberAccessorDescriptor{TData}" />. Controls receive one explicitly or look it up in
/// <see cref="DataMemberAccessorRegistry" />.
/// </para>
/// </remarks>
public interface IDataMemberAccessorDescriptor
{
    /// <summary>
    /// Gets the type whose members the accessors describe. <see cref="DataMemberAccessorRegistry" /> uses it as the
    /// key of the descriptor.
    /// </summary>
    Type DataType { get; }

    /// <summary>
    /// Gets a delegate that creates a new item of the data type for <see cref="IListCollectionView.AddNew" />, or
    /// <see langword="null" /> when the data type cannot be instantiated.
    /// </summary>
    Func<object>? NewItemFactory { get; }

    /// <summary>
    /// Gets the accessors of the data type, one per member.
    /// </summary>
    IReadOnlyList<IDataMemberAccessor> Accessors { get; }

    /// <summary>
    /// Looks up an accessor by its <see cref="IDataMemberAccessor.Path" />, case-sensitively.
    /// </summary>
    /// <param name="path">
    /// The path of the accessor to find.
    /// </param>
    /// <param name="accessor">
    /// The accessor with that path, or <see langword="null" /> when the method returns <see langword="false" />.
    /// </param>
    /// <returns>
    /// <see langword="true" /> when an accessor with that path exists; otherwise <see langword="false" />.
    /// </returns>
    bool TryGetAccessor(string path, out IDataMemberAccessor accessor);
}
