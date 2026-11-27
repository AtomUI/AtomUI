namespace AtomUI.Desktop.Controls;

/// <summary>
/// The sort state of a column as <see cref="DataGridColumn.SortState" /> reports it: the direction the
/// column is sorted in and the position of its sort among the active sorts, or <see cref="None" /> when
/// the column is not sorted.
/// </summary>
public readonly struct DataGridColumnSortState : IEquatable<DataGridColumnSortState>
{
    private readonly int _priorityPlusOne;

    internal DataGridColumnSortState(DataGridSortDirection direction, int priority)
    {
        DataGridQueryValidation.ValidateDirection(direction, nameof(direction));
        if (priority < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(priority));
        }

        Direction = direction;
        _priorityPlusOne = checked(priority + 1);
    }

    /// <summary>
    /// The state of a column that is not sorted: <see cref="Direction" /> is <see langword="null" /> and
    /// <see cref="Priority" /> is -1. It is the <see langword="default" /> value of the struct.
    /// </summary>
    public static DataGridColumnSortState None => default;

    /// <summary>
    /// Gets the direction the column is sorted in, or <see langword="null" /> when the column is not sorted.
    /// </summary>
    public DataGridSortDirection? Direction { get; }

    /// <summary>
    /// Gets the zero-based position of the column among the active sorts of the <see cref="DataGridQuery" />,
    /// or -1 when the column is not sorted.
    /// </summary>
    public int Priority => Direction.HasValue ? _priorityPlusOne - 1 : -1;

    /// <summary>
    /// Returns whether both states have the same <see cref="Direction" /> and <see cref="Priority" />.
    /// </summary>
    public bool Equals(DataGridColumnSortState other) =>
        Direction == other.Direction && Priority == other.Priority;

    /// <summary>
    /// Returns whether <paramref name="obj" /> is a <see cref="DataGridColumnSortState" /> equal to this state.
    /// </summary>
    public override bool Equals(object? obj) =>
        obj is DataGridColumnSortState other && Equals(other);

    /// <summary>
    /// Returns a hash code computed from <see cref="Direction" /> and <see cref="Priority" />.
    /// </summary>
    public override int GetHashCode() => HashCode.Combine(Direction, Priority);

    /// <summary>
    /// Returns whether <paramref name="left" /> and <paramref name="right" /> have the same direction and priority.
    /// </summary>
    public static bool operator ==(
        DataGridColumnSortState left,
        DataGridColumnSortState right) => left.Equals(right);

    /// <summary>
    /// Returns whether <paramref name="left" /> and <paramref name="right" /> differ in direction or priority.
    /// </summary>
    public static bool operator !=(
        DataGridColumnSortState left,
        DataGridColumnSortState right) => !left.Equals(right);
}
