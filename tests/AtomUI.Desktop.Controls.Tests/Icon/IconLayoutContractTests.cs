using AtomUI.Icons.AntDesign;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.IconControls;

public class IconLayoutContractTests
{
    static IconLayoutContractTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Icon_Sizes_From_Width_Height_Without_Template_Children()
    {
        var icon = new SearchOutlined
        {
            Width  = 20,
            Height = 16,
            Margin = new Thickness(3)
        };

        Show(new StackPanel { Children = { icon } }, _ =>
        {
            icon.Bounds.Size.ShouldBe(new Size(20, 16));
            icon.DesiredSize.ShouldBe(new Size(26, 22));
            icon.GetVisualChildren().ShouldBeEmpty();
        });
    }

    [Fact]
    public void Icon_Uses_Token_Size_When_Width_Height_Are_Not_Set()
    {
        var icon = new SearchOutlined();

        Show(new StackPanel { Children = { icon } }, _ =>
        {
            icon.Bounds.Width.ShouldBeGreaterThan(0);
            icon.Bounds.Height.ShouldBeGreaterThan(0);
            icon.Bounds.Width.ShouldBe(icon.Width);
            icon.Bounds.Height.ShouldBe(icon.Height);
        });
    }

    [Fact]
    public void Icon_Is_The_Hit_Test_Target_Across_Its_Bounds()
    {
        var icon = new SearchOutlined
        {
            Width  = 24,
            Height = 24,
            Margin = new Thickness(10, 12, 0, 0)
        };
        var panel = new StackPanel { Children = { icon } };

        Show(panel, window =>
        {
            var origin = icon.TranslatePoint(default, window).ShouldNotBeNull();
            foreach (var offset in new[] { new Point(1, 1), new Point(12, 12), new Point(23, 23) })
            {
                window.InputHitTest(origin + offset).ShouldBeSameAs(icon);
            }
        });
    }

    private static void Show(Control content, Action<AvaloniaWindow> assertion)
    {
        var window = new AvaloniaWindow
        {
            Width   = 160,
            Height  = 120,
            Content = content
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            // Hit testing reads the committed composition scene, so render it before asserting.
            AvaloniaHeadlessPlatform.ForceRenderTimerTick();
            Dispatcher.UIThread.RunJobs();
            assertion(window);
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }
}
