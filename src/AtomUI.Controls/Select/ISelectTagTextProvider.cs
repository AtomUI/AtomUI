namespace AtomUI.Controls;

/// <summary>
/// Provides the text of the tag shown for a selected item. The tag panel of a <c>TreeSelect</c> or
/// <c>Cascader</c> creates a tag for each selected item that implements this interface, such as a tree node or a
/// cascader option.
/// </summary>
public interface ISelectTagTextProvider
{
    /// <summary>
    /// Gets the text to display in the tag.
    /// </summary>
    string? TagText { get; }
}
