using AtomUI.Controls;
using AtomUI.Desktop.Controls.Themes;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;

namespace AtomUI.Desktop.Controls;

internal class WindowResizer : TemplatedControl
{
    public Window? TargetWindow { get; set; }
    private Panel? _rootLayout;

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        if (_rootLayout != null)
        {
            foreach (var child in _rootLayout.Children)
            {
                child.PointerPressed -= HandleResizeHandlePressed;
            }
        }
        _rootLayout = e.NameScope.Find<Panel>(WindowResizerThemeConstants.RootLayoutPart);
        if (_rootLayout != null)
        {
            foreach (var child in _rootLayout.Children)
            {
                child.PointerPressed += HandleResizeHandlePressed;
            }
        }
    }

    private void HandleResizeHandlePressed(object? sender, PointerPressedEventArgs e)
    {
        if (TargetWindow == null || !TargetWindow.CanResize || TargetWindow.WindowState != WindowState.Normal)
        {
            return;
        }

        if (sender is not Border border || border.Tag is not ResizeHandleLocation location)
        {
            return;
        }

        if (VisualRoot is not Window window)
        {
            return;
        }

        var windowEdge = location switch
        {
            ResizeHandleLocation.North => WindowEdge.North,
            ResizeHandleLocation.South => WindowEdge.South,
            ResizeHandleLocation.West => WindowEdge.West,
            ResizeHandleLocation.East => WindowEdge.East,
            ResizeHandleLocation.NorthWest => WindowEdge.NorthWest,
            ResizeHandleLocation.NorthEast => WindowEdge.NorthEast,
            ResizeHandleLocation.SouthWest => WindowEdge.SouthWest,
            ResizeHandleLocation.SouthEast => WindowEdge.SouthEast,
            _ => throw new ArgumentOutOfRangeException()
        };

        window.BeginResizeDrag(windowEdge, e);
        e.Handled = true;
    }
}