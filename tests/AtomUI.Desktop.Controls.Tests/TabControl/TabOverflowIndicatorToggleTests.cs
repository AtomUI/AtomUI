using System.Reflection;
using AtomUI.Controls.Primitives;
using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AtomTabControl = AtomUI.Desktop.Controls.TabControl;
using AtomTabItem = AtomUI.Desktop.Controls.TabItem;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.TabControl;

public class TabOverflowIndicatorToggleTests
{
    static TabOverflowIndicatorToggleTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Pressing_The_Menu_Indicator_While_The_Popup_Is_Open_Keeps_It_Open()
    {
        WithOverflowOwner((owner, window) =>
        {
            var viewer = GetViewer(owner);
            Open(viewer, window);
            var point = IndicatorPoint(owner, window);

            window.MouseMove(point);
            window.MouseDown(point, MouseButton.Left);
            Dispatcher.UIThread.RunJobs();
            viewer.IsOverflowPopupOpen.ShouldBeTrue();

            window.MouseUp(point, MouseButton.Left);
            Dispatcher.UIThread.RunJobs();
            viewer.IsOverflowPopupOpen.ShouldBeTrue();
        });
    }

    [Fact]
    public void Pressing_Outside_The_Menu_While_The_Popup_Is_Open_Still_Dismisses_It()
    {
        WithOverflowOwner((owner, window) =>
        {
            var viewer = GetViewer(owner);
            Open(viewer, window);

            var outside = PointOutsideMenu(viewer, window);
            window.MouseMove(outside);
            window.MouseDown(outside, MouseButton.Left);
            Dispatcher.UIThread.RunJobs();
            viewer.IsOverflowPopupOpen.ShouldBeFalse();
        });
    }

    [Fact]
    public void Clicking_The_Menu_Indicator_While_The_Popup_Is_Closed_Still_Opens_It()
    {
        WithOverflowOwner((owner, window) =>
        {
            var viewer = GetViewer(owner);
            var point = IndicatorPoint(owner, window);

            window.MouseMove(point);
            window.MouseDown(point, MouseButton.Left);
            Dispatcher.UIThread.RunJobs();
            viewer.IsOverflowPopupOpen.ShouldBeFalse();

            window.MouseUp(point, MouseButton.Left);
            Dispatcher.UIThread.RunJobs();
            viewer.IsOverflowPopupOpen.ShouldBeTrue();
        });
    }

    private static Point IndicatorPoint(AtomTabControl owner, AvaloniaWindow window)
    {
        var indicator = owner.GetVisualDescendants().OfType<IconButton>()
                             .Single(button => button.Name == "PART_ScrollMenuIndicator");
        indicator.IsVisible.ShouldBeTrue();
        var translated = indicator.TranslatePoint(
            new Point(indicator.Bounds.Width / 2, indicator.Bounds.Height / 2), window);
        translated.ShouldNotBeNull();
        return translated.Value;
    }

    private static Point PointOutsideMenu(TabScrollViewer viewer, AvaloniaWindow window)
    {
        var menuRoot = viewer.OverflowPopupRoot.ShouldNotBeNull();
        var topLeft = menuRoot.TranslatePoint(new Point(0, 0), window)!.Value;
        var menuRect = new Rect(topLeft, menuRoot.Bounds.Size);
        var windowRect = new Rect(window.ClientSize);
        var candidates = new[]
        {
            new Point(5, 5),
            new Point(window.ClientSize.Width - 5, 5),
            new Point(5, window.ClientSize.Height - 5),
            new Point(window.ClientSize.Width - 5, window.ClientSize.Height - 5),
        };
        return candidates.First(point => windowRect.Contains(point) && !menuRect.Contains(point));
    }

    private static void Open(TabScrollViewer viewer, AvaloniaWindow window)
    {
        var indicator = viewer.GetVisualDescendants().OfType<IconButton>()
                              .Single(button => button.Name == "PART_ScrollMenuIndicator");
        indicator.RaiseEvent(new RoutedEventArgs(IconButton.ClickEvent, indicator));
        Dispatcher.UIThread.RunJobs();
        window.UpdateLayout();
        Dispatcher.UIThread.RunJobs();
        viewer.IsOverflowPopupOpen.ShouldBeTrue();
    }

    private static TabScrollViewer GetViewer(AtomTabControl owner) =>
        owner.GetVisualDescendants().OfType<TabScrollViewer>().ShouldHaveSingleItem();

    private static void WithOverflowOwner(Action<AtomTabControl, AvaloniaWindow> assertion)
    {
        var owner = new AtomTabControl { IsMotionEnabled = false, Width = 100, Height = 240, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top };
        owner.SelectedIndex = 0;
        for (var index = 0; index < 12; index++)
        {
            owner.Items.Add(new AtomTabItem { Header = $"Document {index:00} with a wide header" });
        }

        var host = new ScopeAwareOverlayLayerPanel();
        host.Children.Add(owner);
        var layers = new VisualLayerManager { Child = host };
        typeof(VisualLayerManager).GetProperty("EnablePopupOverlayLayer",
            BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(layers, true);
        var window = new AvaloniaWindow { Width = 480, Height = 360, Content = layers };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            Dispatcher.UIThread.RunJobs();
            assertion(owner, window);
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }
}
