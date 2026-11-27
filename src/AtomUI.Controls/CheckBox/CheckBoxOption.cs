namespace AtomUI.Controls;

/// <summary>
/// Implemented by items whose check box in a check box group takes its label, its enabled state and its initial
/// checked state from the item. The values are read when the check box is prepared; changes made to the item
/// afterwards are not picked up until the check box is prepared again.
/// </summary>
public interface ICheckBoxOption
{
    /// <summary>
    /// Whether the check box of the item can be interacted with.
    /// </summary>
    bool IsEnabled { get; }
    /// <summary>
    /// The label of the check box when the group uses its default item template.
    /// </summary>
    object? Content { get; }
    /// <summary>
    /// The initial checked state of the item. It is used only while the group has no <c>CheckedItems</c> value; once
    /// <c>CheckedItems</c> is set, the checked state of the item comes from that list.
    /// </summary>
    bool IsChecked { get; }
}

/// <summary>
/// Ready-to-use <see cref="ICheckBoxOption" />. A new option is enabled, unchecked and has no content.
/// </summary>
public class CheckBoxOption : ICheckBoxOption
{
    /// <inheritdoc />
    public bool IsEnabled { get; set; } = true;
    /// <inheritdoc />
    public object? Content { get; set; }
    /// <inheritdoc />
    public bool IsChecked { get; set; } = false;
}