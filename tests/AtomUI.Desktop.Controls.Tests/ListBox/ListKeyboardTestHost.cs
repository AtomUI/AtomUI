using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests;

// Headless host for list keyboard navigation tests (ListBox, ListView, Transfer list panels): the list sits between
// two buttons so Tab enters and leaves it, keys go through the framework input pipeline (focused element, bubbling).
internal static class ListKeyboardTestHost
{
    public sealed class Host
    {
        public required AvaloniaWindow Window { get; init; }
        public required Desktop.Controls.Button Before { get; init; }
        public required Desktop.Controls.Button After { get; init; }

        public bool Press(Key key, RawInputModifiers modifiers = RawInputModifiers.None)
            => ListKeyboardTestHost.Press(Window, key, modifiers);

        public void Press(Key key, int times)
        {
            for (var i = 0; i < times; i++)
            {
                Press(key);
            }
        }

        public IInputElement? Focused => Window.FocusManager?.GetFocusedElement();

        public void Click(Control target) => ListKeyboardTestHost.Click(Window, target);
    }

    public static void Run(Control content, Action<Host> body, double width = 420, double height = 360)
    {
        var before = new Desktop.Controls.Button { Content = "Before" };
        var after  = new Desktop.Controls.Button { Content = "After" };
        var layout = new StackPanel { Spacing = 8, Margin = new Thickness(8), Children = { before, content, after } };
        // VisualLayerManager hosts the AdornerLayer that draws the themed keyboard focus outline.
        var window = new AvaloniaWindow
        {
            Width   = width,
            Height  = height,
            Content = new VisualLayerManager { Child = layout }
        };

        try
        {
            window.Show();
            Pump();
            body(new Host { Window = window, Before = before, After = after });
        }
        finally
        {
            window.Close();
            Pump();
        }
    }

    public static bool Press(AvaloniaWindow window, Key key, RawInputModifiers modifiers = RawInputModifiers.None)
    {
        var handled = false;
        void Observe(object? sender, KeyEventArgs e) => handled = e.Handled;
        window.AddHandler(InputElement.KeyDownEvent, Observe, RoutingStrategies.Bubble, handledEventsToo: true);
        try
        {
            var physicalKey = ToPhysicalKey(key);
            window.KeyPress(key, modifiers, physicalKey, null);
            window.KeyRelease(key, modifiers, physicalKey, null);
            Pump();
        }
        finally
        {
            window.RemoveHandler(InputElement.KeyDownEvent, Observe);
        }

        return handled;
    }

    public static void Click(AvaloniaWindow window, Control target)
    {
        var point = target.TranslatePoint(new Point(Math.Min(40, target.Bounds.Width / 2), target.Bounds.Height / 2),
            window);
        point.ShouldNotBeNull();
        window.MouseMove(point.Value);
        window.MouseDown(point.Value, MouseButton.Left);
        window.MouseUp(point.Value, MouseButton.Left);
        Pump();
    }

    public static void Pump(int passes = 4)
    {
        for (var i = 0; i < passes; i++)
        {
            Dispatcher.UIThread.RunJobs();
        }
    }

    // The panel must really virtualize: fewer containers realized than items.
    public static void ShouldBeVirtualizing(ItemsControl itemsControl)
    {
        itemsControl.ItemsPanelRoot.ShouldBeAssignableTo<VirtualizingPanel>();
        var realized = itemsControl.GetRealizedContainers().Count();
        realized.ShouldBeGreaterThan(0, "the list must have realized its viewport");
        realized.ShouldBeLessThan(itemsControl.ItemCount, "the panel must virtualize");
    }

    public static void ScrollTo(ItemsControl itemsControl, double offsetY)
    {
        var scrollViewer = FindScrollViewer(itemsControl);
        scrollViewer.Offset = new Vector(scrollViewer.Offset.X, offsetY);
        itemsControl.UpdateLayout();
        Pump();
    }

    public static void ScrollToEnd(ItemsControl itemsControl)
    {
        var scrollViewer = FindScrollViewer(itemsControl);
        // Several passes: the virtualizing panel re-estimates the extent while scrolling.
        for (var i = 0; i < 6; i++)
        {
            scrollViewer.Offset = new Vector(scrollViewer.Offset.X, scrollViewer.Extent.Height);
            itemsControl.UpdateLayout();
            Pump();
        }
    }

    public static ScrollViewer FindScrollViewer(ItemsControl itemsControl)
    {
        var panel = itemsControl.ItemsPanelRoot.ShouldNotBeNull();
        return panel.GetVisualAncestors().OfType<ScrollViewer>().First();
    }

    // Index of the realized container that holds keyboard focus (or contains the focused element); -1 otherwise.
    public static int FocusedIndex(ItemsControl itemsControl)
    {
        var focused = TopLevel.GetTopLevel(itemsControl)?.FocusManager?.GetFocusedElement() as Visual;
        for (var current = focused; current is not null; current = current.GetVisualParent())
        {
            if (current is Control control && itemsControl.IndexFromContainer(control) is var index and >= 0)
            {
                return index;
            }
        }

        return -1;
    }

    // The platform command modifier (Ctrl, or Cmd where the platform hotkeys say so) as raw input modifiers.
    public static RawInputModifiers CommandModifier()
    {
        var modifiers = Application.Current?.PlatformSettings?.HotkeyConfiguration.CommandModifiers ?? KeyModifiers.Control;
        var raw       = RawInputModifiers.None;
        if (modifiers.HasFlag(KeyModifiers.Control))
        {
            raw |= RawInputModifiers.Control;
        }
        if (modifiers.HasFlag(KeyModifiers.Meta))
        {
            raw |= RawInputModifiers.Meta;
        }
        return raw;
    }

    public static List<FocusOutlineAdorner> FocusOutlines(Visual target)
    {
        var layer = AdornerLayer.GetAdornerLayer(target).ShouldNotBeNull();
        return layer.Children.OfType<FocusOutlineAdorner>().ToList();
    }

    private static PhysicalKey ToPhysicalKey(Key key) => key switch
    {
        Key.Down     => PhysicalKey.ArrowDown,
        Key.Up       => PhysicalKey.ArrowUp,
        Key.Left     => PhysicalKey.ArrowLeft,
        Key.Right    => PhysicalKey.ArrowRight,
        Key.Home     => PhysicalKey.Home,
        Key.End      => PhysicalKey.End,
        Key.PageUp   => PhysicalKey.PageUp,
        Key.PageDown => PhysicalKey.PageDown,
        Key.Enter    => PhysicalKey.Enter,
        Key.Space    => PhysicalKey.Space,
        Key.Tab      => PhysicalKey.Tab,
        Key.A        => PhysicalKey.A,
        _            => PhysicalKey.None
    };
}
