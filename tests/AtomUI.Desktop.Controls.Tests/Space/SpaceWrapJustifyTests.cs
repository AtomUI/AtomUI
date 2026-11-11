using AtomUI.Controls;
using Avalonia;
using Avalonia.Controls;
using Shouldly;
using Xunit;
using AtomUISpace = AtomUI.Desktop.Controls.Space;

namespace AtomUI.Desktop.Controls.Tests.Space;

public class SpaceWrapJustifyTests
{
    static SpaceWrapJustifyTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    private static Border Item(double width, double height)
    {
        return new Border { Width = width, Height = height };
    }

    private static void Layout(Control space, double width, double height)
    {
        space.UseLayoutRounding = false;
        space.Measure(new Size(width, height));
        space.Arrange(new Rect(0, 0, width, height));
    }

    [Fact]
    public void Desktop_wraps_by_default()
    {
        var space = new AtomUISpace
        {
            ItemSpacing = 8,
            LineSpacing = 4,
            Children    = { Item(40, 20), Item(40, 20), Item(40, 20) }
        };

        space.IsWrap.ShouldBeTrue();
        space.Justify.ShouldBe(SpaceJustify.Start);

        Layout(space, 100, 100);

        space.Children[1].Bounds.Y.ShouldBe(0, 0.01);
        space.Children[2].Bounds.X.ShouldBe(0, 0.01);
        space.Children[2].Bounds.Y.ShouldBe(24, 0.01);
    }

    [Fact]
    public void Desktop_wrapped_center_alignment_uses_each_line_origin()
    {
        var space = new AtomUISpace
        {
            ItemSpacing    = 8,
            LineSpacing    = 4,
            ItemsAlignment = SpaceItemsAlignment.Center,
            Children       = { Item(40, 40), Item(40, 20), Item(40, 10) }
        };

        Layout(space, 100, 100);

        space.Children[1].Bounds.Y.ShouldBe(10, 0.01);
        space.Children[2].Bounds.Y.ShouldBe(44, 0.01);
    }

    [Fact]
    public void Desktop_justify_distributes_inside_root_frame_inset()
    {
        var space = new AtomUISpace
        {
            Padding     = new Thickness(10),
            ItemSpacing = 8,
            Justify     = SpaceJustify.End,
            Children    = { Item(40, 20), Item(40, 20) }
        };

        Layout(space, 200, 100);

        // Content length 180, items 88: the line is pushed to the end of the padded area.
        space.Children[0].Bounds.X.ShouldBe(10 + 92, 0.01);
        space.Children[1].Bounds.X.ShouldBe(10 + 92 + 48, 0.01);
        space.Children[0].Bounds.Y.ShouldBe(10, 0.01);
    }
}
