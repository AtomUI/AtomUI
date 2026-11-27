// Keep the CLR name used by existing Desktop clients.
namespace AtomUI.Desktop.Controls;

/// <summary>
/// Lets an item of a collapse supply the header, the selected state and the expand icon visibility of the panel
/// that displays it. A collapse copies these values to the panel when the panel does not have the
/// corresponding property set.
/// </summary>
public interface ICollapseItemData
{
    /// <summary>
    /// Whether the panel that displays the item is selected, which shows the panel content below the header.
    /// In accordion mode only one panel stays selected, so a panel that arrives selected does not take the
    /// selection away from an earlier selected panel.
    /// </summary>
    bool IsSelected { get; }
    /// <summary>
    /// Whether the panel that displays the item is enabled. A collapse does not copy this value to the panel.
    /// </summary>
    bool IsEnabled { get; }
    /// <summary>
    /// Whether the panel that displays the item shows the expand icon in its header.
    /// </summary>
    bool IsShowExpandIcon { get; }
    /// <summary>
    /// The content shown in the header of the panel that displays the item. <see langword="null" /> leaves the
    /// header empty.
    /// </summary>
    object? Header { get; }
}
