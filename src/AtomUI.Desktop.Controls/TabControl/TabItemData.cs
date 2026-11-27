using AtomUI.Controls;
using Avalonia.Controls;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Describes one tab of a tab control or tab strip as data. The label is <see cref="AtomUI.Controls.IHeadered.Header" />.
/// When an item of the <c>Items</c> of such a control implements this interface, the control copies the
/// values onto the tab item it creates for that item.
/// </summary>
public interface ITabItemData : IHeadered
{
    /// <summary>
    /// Icon shown at the start of the tab, before its label; <see langword="null" /> shows no icon.
    /// </summary>
    PathIcon? Icon { get; }
    /// <summary>
    /// Icon of the close button of the tab. It is used only while <see cref="IsClosable" /> is
    /// <see langword="true" />.
    /// </summary>
    PathIcon? CloseIcon { get; }
    /// <summary>
    /// Whether the tab accepts interaction; <see langword="false" /> disables it.
    /// </summary>
    bool IsEnabled { get; }
    /// <summary>
    /// Whether the tab shows a close button.
    /// </summary>
    bool IsClosable { get; }
    /// <summary>
    /// Whether the close button of a closable tab stays transparent until the pointer is over the tab. It has no
    /// effect while <see cref="IsClosable" /> is <see langword="false" />.
    /// </summary>
    bool IsAutoHideCloseButton { get; }
}

/// <summary>
/// A ready-made tab description for the <c>Items</c> of a tab control or tab strip, carrying the label, the
/// icons and the flags of one tab. Set the values in the object initializer; they are read-only afterwards.
/// Derive from this class to add application data to a tab.
/// </summary>
public class TabItemData : ITabItemData
{
    /// <summary>
    /// Text or content shown as the label of the tab.
    /// </summary>
    public object? Header { get; init; }
    /// <summary>
    /// Icon shown at the start of the tab, before its label; <see langword="null" /> shows no icon.
    /// </summary>
    public PathIcon? Icon { get; init; }
    /// <summary>
    /// Icon of the close button of the tab. It is used only while <see cref="IsClosable" /> is
    /// <see langword="true" />.
    /// </summary>
    public PathIcon? CloseIcon { get; init; }
    /// <summary>
    /// Whether the tab accepts interaction; <see langword="false" /> disables it. The default is
    /// <see langword="true" />.
    /// </summary>
    public bool IsEnabled { get; init; } = true;
    /// <summary>
    /// Whether the tab shows a close button. The default is <see langword="false" />.
    /// </summary>
    public bool IsClosable { get; init; }
    /// <summary>
    /// Whether the close button of a closable tab stays transparent until the pointer is over the tab. It has no
    /// effect while <see cref="IsClosable" /> is <see langword="false" />. The default is
    /// <see langword="false" />, which paints the button as soon as the tab is closable.
    /// </summary>
    public bool IsAutoHideCloseButton { get; init; }
}