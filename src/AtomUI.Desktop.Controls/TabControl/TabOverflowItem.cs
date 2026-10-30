using Avalonia.Controls.Templates;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Represents an immutable projection of a tab shown in an overflow popup.
/// </summary>
public sealed class TabOverflowItem
{
    internal TabOverflowItem(
        object? item,
        object? header,
        IDataTemplate? headerTemplate,
        bool isEnabled,
        bool isSelected,
        bool isClosable)
    {
        Item = item;
        Header = header;
        HeaderTemplate = headerTemplate;
        IsEnabled = isEnabled;
        IsSelected = isSelected;
        IsClosable = isClosable;
    }

    /// <summary>
    /// Gets the logical item represented by this projection.
    /// </summary>
    public object? Item { get; }

    /// <summary>
    /// Gets the effective tab header.
    /// </summary>
    public object? Header { get; }

    /// <summary>
    /// Gets the effective tab header template.
    /// </summary>
    public IDataTemplate? HeaderTemplate { get; }

    /// <summary>
    /// Gets whether the source tab is enabled.
    /// </summary>
    public bool IsEnabled { get; }

    /// <summary>
    /// Gets whether the source tab is selected.
    /// </summary>
    public bool IsSelected { get; }

    /// <summary>
    /// Gets whether the source tab can be closed.
    /// </summary>
    public bool IsClosable { get; }
}
