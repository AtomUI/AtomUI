using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

using AvaloniaDefaultMenuInteractionHandler = Avalonia.Controls.Platform.DefaultMenuInteractionHandler;

public class DefaultMenuInteractionHandler : AvaloniaDefaultMenuInteractionHandler
{
    public DefaultMenuInteractionHandler(bool isContextMenu)
        : base(isContextMenu)
    {
    }

    public DefaultMenuInteractionHandler(
        bool isContextMenu,
        IInputManager? inputManager,
        Action<Action, TimeSpan> delayRun)
        : base(isContextMenu, inputManager, delayRun)
    {
    }
    
    protected override void PointerMoved(object? sender, PointerEventArgs e)
    {
        var item = GetMenuItemCore(e.Source as Control);

        if (item == null)
        {
            return;
        }

        var transformedBounds = item.GetTransformedBounds();
        if (transformedBounds == null)
        {
            return;
        }

        var point = e.GetCurrentPoint(null);

        if (point.Properties.IsLeftButtonPressed
            && transformedBounds.Value.Contains(point.Position) == false)
        {
            var sourceControl  = e.Source as Control;
            var scrollBarThumb = sourceControl.FindAncestorOfType<ScrollBarThumb>();
            if (scrollBarThumb != null)
            {
                return;
            }
            e.Pointer.Capture(null);
        }
    }
    
    internal static MenuItem? GetMenuItemCore(StyledElement? item)
    {
        while (true)
        {
            if (item == null)
            {
                return null;
            }

            if (item is MenuItem menuItem)
            {
                return menuItem;
            }
            item = item.Parent;
        }
    }
}