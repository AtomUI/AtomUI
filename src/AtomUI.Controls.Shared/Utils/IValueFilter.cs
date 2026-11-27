namespace AtomUI.Controls.Utils;

/// <summary>
/// Decides whether an item of a filtering control matches the current filter text.
/// </summary>
/// <remarks>
/// A control selects the value of an item with a <see cref="DefaultFilterValueSelector" /> and passes it together
/// with the filter text to <see cref="Filter" />. An implementation receives only these two arguments and must not
/// depend on the control or on the item itself.
/// </remarks>
public interface IValueFilter
{
    /// <summary>
    /// Gets the built-in matching rule this filter applies, or <see cref="ValueFilterMode.Custom" /> when the filter
    /// implements its own rule.
    /// </summary>
    /// <remarks>
    /// When completion is enabled, AutoComplete takes the first option of the filtered view as the match for the typed
    /// text if the mode is <see cref="ValueFilterMode.StartsWith" /> or
    /// <see cref="ValueFilterMode.StartsWithCaseSensitive" />. For every other mode it searches the view for an option
    /// that starts with the text.
    /// </remarks>
    ValueFilterMode Mode => ValueFilterMode.Custom;
    /// <summary>
    /// Tests whether <paramref name="value" /> matches <paramref name="filterValue" />.
    /// </summary>
    /// <param name="value">
    /// The value of one item: the item itself, or the value that its filter value selector returns.
    /// </param>
    /// <param name="filterValue">
    /// The current filter text.
    /// </param>
    /// <returns>
    /// <see langword="true" /> when the item matches the filter; otherwise <see langword="false" />.
    /// </returns>
    bool Filter(object? value, object? filterValue);
}

/// <summary>
/// Maps an item to the value that an <see cref="IValueFilter" /> compares with the filter text.
/// </summary>
/// <param name="value">
/// The item being filtered.
/// </param>
/// <returns>
/// The value to compare, for example the header text of the item or text composed from several of its fields.
/// </returns>
/// <remarks>
/// The selector is a delegate and not a property path, so calling it needs no reflection.
/// </remarks>
public delegate object? DefaultFilterValueSelector(object? value);