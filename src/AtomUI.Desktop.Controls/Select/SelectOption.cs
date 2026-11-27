using AtomUI.Controls.Data;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// An option of a <see cref="Select" />: the item that the dropdown lists and that the select stores as selected.
/// </summary>
public interface ISelectOption : IListItemData
{
    /// <summary>
    /// The text shown for the option: the dropdown list shows it as the label of the option, a single-selection
    /// select shows it as its selected value, and the default filter value selector returns it.
    /// </summary>
    object? Header { get; }
    /// <summary>
    /// Whether the <see cref="Select" /> created this option at runtime from the filter text in
    /// <see cref="SelectMode.Tags" /> mode, so that the text can be chosen as a new tag. The select removes such
    /// an option from the option list when it is no longer selected and no longer matches the filter text.
    /// </summary>
    bool IsDynamicAdded { get; }
}

/// <summary>
/// The built-in <see cref="ISelectOption" /> implementation. The select creates one for the option that it
/// offers from the filter text in <see cref="SelectMode.Tags" /> mode.
/// </summary>
public class SelectOption : ListItemData, ISelectOption, IGroupListItemData
{
    /// <summary>
    /// The text that stands for the option. It can only be set when the option is created.
    /// </summary>
    public object? Header { get; init; }

    /// <summary>
    /// Whether the select created this option at runtime from the filter text in
    /// <see cref="SelectMode.Tags" /> mode. It can only be set when the option is created.
    /// </summary>
    public bool IsDynamicAdded { get; init; } = false;

    bool IGroupListItemData.IsGroupItem { get; set; } = false;
}
