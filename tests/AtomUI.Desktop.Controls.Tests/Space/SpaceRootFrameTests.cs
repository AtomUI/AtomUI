using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using Shouldly;
using Xunit;
using AtomUISpace = AtomUI.Desktop.Controls.Space;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.Space;

public class SpaceRootFrameTests
{
    static SpaceRootFrameTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Root_Frame_Inset_Offsets_Children_And_Inflates_Desired_Size()
    {
        var space = new AtomUISpace
        {
            Padding         = new Thickness(8),
            BorderThickness = new Thickness(2),
            Children =
            {
                new Border
                {
                    Width  = 40,
                    Height = 20
                }
            }
        };

        using var window = Show(space);

        var child = space.Children[0];
        child.Bounds.X.ShouldBe(10);
        child.Bounds.Y.ShouldBe(10);
        space.DesiredSize.Width.ShouldBe(60, 0.01);
        space.DesiredSize.Height.ShouldBe(40, 0.01);
    }

    [Fact]
    public void Default_Frame_Values_Keep_Legacy_Layout_Unchanged()
    {
        var space = new AtomUISpace
        {
            Children =
            {
                new Border
                {
                    Width  = 40,
                    Height = 20
                }
            }
        };

        using var window = Show(space);

        var child = space.Children[0];
        child.Bounds.X.ShouldBe(0);
        child.Bounds.Y.ShouldBe(0);
        space.DesiredSize.Width.ShouldBe(40, 0.01);
        space.DesiredSize.Height.ShouldBe(20, 0.01);
    }

    [Fact]
    public void Root_Frame_Appearance_Api_Is_Exposed_For_Ant_Design_Styling_Alignment()
    {
        var space = new AtomUISpace
        {
            Background      = Brushes.White,
            BorderBrush     = Brushes.Black,
            BorderThickness = new Thickness(2),
            CornerRadius    = new CornerRadius(4),
            Padding         = new Thickness(8),
            BorderDashArray = new double[] { 4, 2 },
            BorderDashOffset = 1
        };

        space.Background.ShouldBe(Brushes.White);
        space.BorderBrush.ShouldBe(Brushes.Black);
        space.BorderThickness.ShouldBe(new Thickness(2));
        space.CornerRadius.ShouldBe(new CornerRadius(4));
        space.Padding.ShouldBe(new Thickness(8));
        space.BorderDashArray.ShouldBe(new double[] { 4, 2 }, ignoreOrder: false);
        space.BorderDashOffset.ShouldBe(1);
    }

    private static IDisposable Show(Control content)
    {
        var window = new AvaloniaWindow
        {
            Width   = 480,
            Height  = 260,
            Content = content
        };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        return new WindowLifetime(window);
    }

    private sealed class WindowLifetime(AvaloniaWindow window) : IDisposable
    {
        public void Dispose()
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }
}
