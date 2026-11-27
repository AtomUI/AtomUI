using System.Diagnostics;
using Avalonia;
using Avalonia.Interactivity;
using AtomUIContextMenu = AtomUI.Desktop.Controls.ContextMenu;
using AtomUIMenuItem = AtomUI.Desktop.Controls.MenuItem;
using AtomUISelectableTextBlock = AtomUI.Desktop.Controls.SelectableTextBlock;

namespace AtomUI.Toolkits.GalleryBase.Controls;

/// <summary>
/// A text block that shows a context menu with a single Copy command for the selected text. The command is
/// enabled only while <c>CanCopy</c> is true; the state is refreshed each time the menu opens.
/// </summary>
public class GallerySelectableTextBlock : AtomUISelectableTextBlock
{
    private readonly AtomUIContextMenu _contextMenu = new();
    private readonly AtomUIMenuItem _copyMenuItem = new()
    {
        Header    = "Copy",
        IsEnabled = false
    };

    public GallerySelectableTextBlock()
    {
        ConfigureContextMenu();
    }

    /// <summary>
    /// Enables or disables the Copy command of the context menu when <c>CanCopy</c> changes. The base
    /// implementation runs first; an override must call it.
    /// </summary>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == CanCopyProperty)
        {
            UpdateCopyMenuItemState();
        }
    }

    private void ConfigureContextMenu()
    {
        _copyMenuItem.Click += HandleCopyMenuItemClick;
        _contextMenu.Opened += HandleContextMenuOpened;
        _contextMenu.Items.Add(_copyMenuItem);
        ContextMenu = _contextMenu;
        UpdateCopyMenuItemState();
    }

    private void HandleContextMenuOpened(object? sender, EventArgs e)
    {
        UpdateCopyMenuItemState();
    }

    private async void HandleCopyMenuItemClick(object? sender, RoutedEventArgs e)
    {
        e.Handled = true;
        try
        {
            await CopyAsync();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error copying Gallery text: {ex.Message}");
        }
    }

    private void UpdateCopyMenuItemState()
    {
        _copyMenuItem.IsEnabled = CanCopy;
    }
}
