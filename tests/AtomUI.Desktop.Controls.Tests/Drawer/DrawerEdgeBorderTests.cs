using AtomUI.Desktop.Controls.Tests.Window;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AvaloniaWindow = Avalonia.Controls.Window;
using AtomUIWindow = AtomUI.Desktop.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.Drawer;

/// <summary>
/// <c>DrawerToken.EdgeBorderColor</c> / <c>EdgeBorderThickness</c>: a theme can draw a border on the drawer's
/// anchored side only (the shadcn sheet look); the defaults draw nothing, so Ant Design rendering stays.
/// </summary>
public class DrawerEdgeBorderTests
{
    static DrawerEdgeBorderTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Drawer_Defaults_Draw_No_Edge_Border()
    {
        var drawer = new AtomUI.Desktop.Controls.Drawer
        {
            Content = new Border(),
            IsMotionEnabled = false,
            Width = 1,
            Height = 1
        };

        var window = CreateAtomWindow(drawer);
        try
        {
            drawer.IsOpen = true;
            Dispatcher.UIThread.RunJobs();

            var surface = FindSurface(window);
            surface.BorderThickness.ShouldBe(default(Thickness));
        }
        finally
        {
            drawer.IsOpen = false;
            Dispatcher.UIThread.RunJobs();
            window.Close();
        }
    }

    [Theory]
    [InlineData(DrawerPlacement.Left, 0, 0, 1, 0)]
    [InlineData(DrawerPlacement.Right, 1, 0, 0, 0)]
    [InlineData(DrawerPlacement.Top, 0, 0, 0, 1)]
    [InlineData(DrawerPlacement.Bottom, 0, 1, 0, 0)]
    public async Task Drawer_Edge_Border_Draws_Only_On_The_Anchored_Side(
        DrawerPlacement placement, double left, double top, double right, double bottom)
    {
        var manager = (AtomUI.Theme.ThemeManager)Avalonia.Application.Current!.GetThemeManager()!;
        var config = new AtomUI.Theme.Configuration.ThemeConfigBuilder()
                     .WithControl(AtomUI.Desktop.Controls.DesignTokens.DrawerTokens.Identity,
                         new AtomUI.Theme.Configuration.ControlThemeConfigBuilder()
                             .WithToken("EdgeBorderColor", "#FF0000")
                             .WithToken("EdgeBorderThickness", "1")
                             .Build())
                     .Build();
        await manager.ApplyThemeAsync(new AtomUI.Theme.ThemeRequest(
            AtomUI.Theme.IThemeManager.DEFAULT_THEME_ID, config, AtomUI.Theme.ThemeTransitionReason.UserRequest));
        try
        {
            var drawer = new AtomUI.Desktop.Controls.Drawer
            {
                Content = new Border(),
                IsMotionEnabled = false,
                Placement = placement,
                Width = 1,
                Height = 1
            };

            var window = CreateAtomWindow(drawer);
            try
            {
                drawer.IsOpen = true;
                Dispatcher.UIThread.RunJobs();

                var surface = FindSurface(window);
                surface.BorderThickness.ShouldBe(new Thickness(left, top, right, bottom));
                var brush = surface.BorderBrush.ShouldBeAssignableTo<ISolidColorBrush>();
                brush!.Color.ShouldBe(Colors.Red);
            }
            finally
            {
                drawer.IsOpen = false;
                Dispatcher.UIThread.RunJobs();
                window.Close();
            }
        }
        finally
        {
            await manager.ApplyThemeAsync(new AtomUI.Theme.ThemeRequest(
                AtomUI.Theme.IThemeManager.DEFAULT_THEME_ID, null, AtomUI.Theme.ThemeTransitionReason.UserRequest));
        }
    }

    private static Border FindSurface(AvaloniaWindow window)
    {
        return window.GetVisualDescendants().OfType<Border>()
                     .Single(border => border.Name == "PART_Surface");
    }

    private static AvaloniaWindow CreateAtomWindow(Control content)
    {
        var window = new AtomUIWindow
        {
            Width  = 400,
            Height = 300,
            Content = new Avalonia.Controls.Panel { Children = { content } }
        };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        return window;
    }
}
