namespace AtomUI.Desktop.Controls;

/// <summary>
/// Implemented by the data behind an item in the filter list of a <see cref="CascaderView" />, so that the default
/// <c>FilterValueSelector</c> can read the path of the item.
/// </summary>
public interface ICascaderItemInfo
{
    /// <summary>
    /// The path of the option as text, from the root down to the option. By default the filter list matches the filter
    /// value against this text and shows it for the item.
    /// </summary>
    string Path { get; }
}