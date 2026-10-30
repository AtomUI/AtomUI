using System.Reflection;
using AtomUI.Controls.Primitives;
using AtomUI.Animations;
using Avalonia.Controls;
using Avalonia.Threading;
using Avalonia.VisualTree;
using AtomUI.Data;
using Shouldly;
using Xunit;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.Drawer;

public class DrawerAotBindingTests
{
    static DrawerAotBindingTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Drawer_Default_OpenOn_Does_Not_Use_One_Time_TopLevel_Lookup()
    {
        var source = File.ReadAllText(GetRepoFile("src/AtomUI.Desktop.Controls/Drawer/Drawer.cs"));

        source.ShouldNotContain("TopLevel.GetTopLevel(this)");
        source.ShouldContain("BindVisualAncestor");
    }

    [Fact]
    public void BindVisualAncestor_Can_Bind_Detached_Drawer_OpenOn()
    {
        var drawer = new AtomUI.Desktop.Controls.Drawer();

        using var binding = BindUtils.BindVisualAncestor(
            drawer,
            drawer,
            AtomUI.Desktop.Controls.Drawer.OpenOnProperty,
            typeof(TopLevel),
            priority: Avalonia.Data.BindingPriority.Template);

        drawer.OpenOn.ShouldBeNull();
    }

    [Fact]
    public void Drawer_Reopen_After_Placement_Change_Hides_Reused_Motion_Actor_Before_Open_Animation()
    {
        var drawer = new AtomUI.Desktop.Controls.Drawer
        {
            Placement       = DrawerPlacement.Right,
            IsMotionEnabled = false,
            Width           = 1,
            Height          = 1
        };

        var window = CreateWindow(drawer);

        try
        {
            drawer.IsOpen = true;
            Dispatcher.UIThread.RunJobs();

            var layer = ScopeAwareAdornerLayer.GetLayer(drawer);
            layer.ShouldNotBeNull();

            var container = layer.Children.Single();
            var staleMotionActor = container.GetVisualDescendants()
                                            .OfType<Border>()
                                            .Single(control => control.Name == "PART_Surface");
            staleMotionActor.Opacity = 1.0;
            SetInternalProperty(container, "IsMotionEnabled", false);
            layer.Children.Remove(container);

            drawer.IsOpen = false;
            Dispatcher.UIThread.RunJobs();

            layer.Children.Count.ShouldBe(0);

            drawer.IsMotionEnabled = true;
            drawer.Placement       = DrawerPlacement.Left;
            drawer.IsMotionEnabled.ShouldBeTrue();
            drawer.IsOpen          = true;

            var motionActor = layer.GetVisualDescendants()
                                   .OfType<Border>()
                                   .Single(control => control.Name == "PART_Surface");

            motionActor.Opacity.ShouldBe(
                0.0,
                "A reused DrawerContainer must be hidden immediately when reattached, before the async open animation starts.");
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Drawer_Close_Closes_Open_Child_Drawer()
    {
        var childDrawer = new AtomUI.Desktop.Controls.Drawer
        {
            IsMotionEnabled = false,
            Width           = 1,
            Height          = 1
        };

        var parentDrawer = new AtomUI.Desktop.Controls.Drawer
        {
            IsMotionEnabled = false,
            Content         = childDrawer,
            Width           = 1,
            Height          = 1
        };

        var window = CreateWindow(parentDrawer);

        try
        {
            parentDrawer.IsOpen = true;
            Dispatcher.UIThread.RunJobs();

            childDrawer.IsOpen = true;
            Dispatcher.UIThread.RunJobs();

            childDrawer.IsOpen.ShouldBeTrue();

            parentDrawer.IsOpen = false;
            Dispatcher.UIThread.RunJobs();

            childDrawer.IsOpen.ShouldBeFalse(
                "Closing a parent Drawer must close its open child drawer so it cannot reopen from stale state on the next parent attach.");
        }
        finally
        {
            window.Close();
        }
    }

    private static AvaloniaWindow CreateWindow(Control content)
    {
        var window = new AvaloniaWindow
        {
            Width   = 480,
            Height  = 360,
            Content = content
        };

        window.Show();
        Dispatcher.UIThread.RunJobs();
        return window;
    }

    private static void SetInternalProperty(object target, string propertyName, object? value)
    {
        var property = target.GetType().GetProperty(
            propertyName,
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        property.ShouldNotBeNull();
        property.SetValue(target, value);
    }

    private static string GetRepoFile(string relativePath)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var candidate = Path.Combine(directory.FullName, relativePath);
            if (File.Exists(candidate))
            {
                return candidate;
            }

            directory = directory.Parent;
        }

        throw new FileNotFoundException($"Could not find repository file: {relativePath}");
    }
}
