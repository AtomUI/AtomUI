using Avalonia.Metadata;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// One entry of a column filter: a text to display with the value it stands for, optionally with nested
/// entries that form a filter tree.
/// </summary>
public class DataGridFilterItem
{
    private List<DataGridFilterItem>? _children;

    /// <summary>
    /// The text that the filter shows for this entry.
    /// </summary>
    public string Text { get; set; } = string.Empty;
    /// <summary>
    /// The value that an entry without children filters by. An entry that has children is not a filter value
    /// itself, and an entry whose value is <see langword="null" /> adds nothing to the filter.
    /// </summary>
    public object? Value { get; set; }
    
    /// <summary>
    /// The nested entries of this one, shown as the level below it in the filter. Child elements written in
    /// XAML are collected here. The list is created when it is read for the first time.
    /// </summary>
    [Content]
    public List<DataGridFilterItem> Children
    {
        get => _children ??= new List<DataGridFilterItem>();
        set => _children = value;
    }

    internal bool HasChildren => _children is { Count: > 0 };
}
