using AtomUI.Controls.Commons;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Shouldly;
using Xunit;

namespace AtomUI.Controls.Tests.Space;

public class AbstractSpaceTests
{
    static AbstractSpaceTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    private sealed class TestSpace : AbstractSpace
    {
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
    public void Defaults_keep_the_wrapping_layout_and_start_distribution()
    {
        var space = new TestSpace();

        space.IsWrap.ShouldBeTrue();
        space.Justify.ShouldBe(SpaceJustify.Start);
        space.ItemsAlignment.ShouldBe(SpaceItemsAlignment.Start);
        space.Orientation.ShouldBe(Orientation.Horizontal);
    }

    [Theory]
    [InlineData(SpaceJustify.Start, 0.0, 8.0)]
    [InlineData(SpaceJustify.End, 164.0, 8.0)]
    [InlineData(SpaceJustify.Center, 82.0, 8.0)]
    [InlineData(SpaceJustify.Between, 0.0, 90.0)]
    [InlineData(SpaceJustify.Around, 164.0 / 6, 8.0 + 164.0 / 3)]
    [InlineData(SpaceJustify.Evenly, 41.0, 49.0)]
    public void Justify_positions(SpaceJustify justify, double firstX, double gap)
    {
        var space = new TestSpace
        {
            Width       = 300,
            ItemSpacing = 8,
            Justify     = justify,
            Children    = { Item(40, 20), Item(40, 20), Item(40, 20) }
        };

        Layout(space, 300, 100);

        space.Children[0].Bounds.X.ShouldBe(firstX, 0.01);
        space.Children[1].Bounds.X.ShouldBe(firstX + 40 + gap, 0.01);
        space.Children[2].Bounds.X.ShouldBe(firstX + 2 * (40 + gap), 0.01);
        foreach (var child in space.Children)
        {
            child.Bounds.Width.ShouldBe(40, 0.01);
            child.Bounds.Y.ShouldBe(0, 0.01);
        }
    }

    [Fact]
    public void Between_with_single_child_is_start()
    {
        var space = new TestSpace
        {
            Width    = 300,
            Justify  = SpaceJustify.Between,
            Children = { Item(40, 20) }
        };

        Layout(space, 300, 100);

        space.Children[0].Bounds.X.ShouldBe(0, 0.01);
        space.Children[0].Bounds.Width.ShouldBe(40, 0.01);
    }

    [Theory]
    [InlineData(SpaceJustify.Start)]
    [InlineData(SpaceJustify.End)]
    [InlineData(SpaceJustify.Center)]
    [InlineData(SpaceJustify.Between)]
    [InlineData(SpaceJustify.Around)]
    [InlineData(SpaceJustify.Evenly)]
    public void Overflow_justify_does_not_shift_or_shrink(SpaceJustify justify)
    {
        // 3 * 120 + 2 * 8 = 376 > 300: R is clamped to 0, so every value lays out like Start.
        var space = new TestSpace
        {
            Width       = 300,
            ItemSpacing = 8,
            IsWrap      = false,
            Justify     = justify,
            Children    = { Item(120, 20), Item(120, 20), Item(120, 20) }
        };

        Layout(space, 300, 100);

        space.Children[0].Bounds.X.ShouldBe(0, 0.01);
        space.Children[1].Bounds.X.ShouldBe(128, 0.01);
        space.Children[2].Bounds.X.ShouldBe(256, 0.01);
        foreach (var child in space.Children)
        {
            child.Bounds.Width.ShouldBe(120, 0.01);
        }
    }

    [Fact]
    public void Hidden_children_take_no_space_or_spacing()
    {
        var hidden = Item(40, 20);
        hidden.IsVisible = false;
        var space = new TestSpace
        {
            Width       = 300,
            ItemSpacing = 8,
            Children    = { Item(40, 20), hidden, Item(40, 20) }
        };

        Layout(space, 300, 100);
        space.Children[0].Bounds.X.ShouldBe(0, 0.01);
        space.Children[2].Bounds.X.ShouldBe(48, 0.01);
        space.DesiredSize.Width.ShouldBe(300, 0.01);

        // n = 2 visible children: R = 300 - 80 - 8 = 212, all of it goes into the single gap.
        space.Justify = SpaceJustify.Between;
        Layout(space, 300, 100);
        space.Children[0].Bounds.X.ShouldBe(0, 0.01);
        space.Children[2].Bounds.X.ShouldBe(260, 0.01);

        space.Justify = SpaceJustify.Start;
        space.Width   = double.NaN;
        Layout(space, 1000, 100);
        space.DesiredSize.Width.ShouldBe(88, 0.01);
    }

    [Fact]
    public void Wrap_off_keeps_single_line()
    {
        var space = new TestSpace
        {
            ItemSpacing = 8,
            LineSpacing = 4,
            IsWrap      = false,
            Children    = { Item(40, 20), Item(40, 20), Item(40, 20) }
        };

        Layout(space, 100, 100);

        space.Children[0].Bounds.Y.ShouldBe(0, 0.01);
        space.Children[1].Bounds.Y.ShouldBe(0, 0.01);
        space.Children[2].Bounds.Y.ShouldBe(0, 0.01);
        space.Children[2].Bounds.X.ShouldBe(96, 0.01);

        // The natural size of an unwrapped line is the whole content, not the available length.
        space.Measure(Size.Infinity);
        space.DesiredSize.Width.ShouldBe(136, 0.01);
        space.DesiredSize.Height.ShouldBe(20, 0.01);

        space.IsWrap = true;
        Layout(space, 100, 100);
        space.Children[2].Bounds.Y.ShouldBe(24, 0.01);
        space.Children[2].Bounds.X.ShouldBe(0, 0.01);
    }

    [Theory]
    [InlineData(SpaceItemsAlignment.Start, 0.0, 0.0, 44.0, 44.0)]
    [InlineData(SpaceItemsAlignment.Center, 10.0, 0.0, 54.0, 44.0)]
    [InlineData(SpaceItemsAlignment.End, 20.0, 0.0, 64.0, 44.0)]
    public void Wrapped_lines_align_in_their_own_line(SpaceItemsAlignment alignment, double ay, double by, double cy, double dy)
    {
        // Line 1 (height 40): A(20) B(40). Line 2 (height 30) starts at 40 + LineSpacing 4: C(10) D(30).
        var space = new TestSpace
        {
            ItemSpacing    = 8,
            LineSpacing    = 4,
            ItemsAlignment = alignment,
            Children       = { Item(40, 20), Item(40, 40), Item(40, 10), Item(40, 30) }
        };

        Layout(space, 100, 200);

        space.Children[0].Bounds.Y.ShouldBe(ay, 0.01);
        space.Children[1].Bounds.Y.ShouldBe(by, 0.01);
        space.Children[2].Bounds.Y.ShouldBe(cy, 0.01);
        space.Children[3].Bounds.Y.ShouldBe(dy, 0.01);
        space.Children[2].Bounds.X.ShouldBe(0, 0.01);
        space.Children[3].Bounds.X.ShouldBe(48, 0.01);
    }

    [Theory]
    [InlineData(SpaceJustify.Start, 0.0, 8.0)]
    [InlineData(SpaceJustify.End, 164.0, 8.0)]
    [InlineData(SpaceJustify.Center, 82.0, 8.0)]
    [InlineData(SpaceJustify.Between, 0.0, 90.0)]
    [InlineData(SpaceJustify.Evenly, 41.0, 49.0)]
    public void Vertical_orientation_justify(SpaceJustify justify, double firstY, double gap)
    {
        // Main axis is Y here: the same distribution table applies to heights.
        var space = new TestSpace
        {
            Orientation = Orientation.Vertical,
            Height      = 300,
            ItemSpacing = 8,
            Justify     = justify,
            Children    = { Item(20, 40), Item(20, 40), Item(20, 40) }
        };

        Layout(space, 100, 300);

        var expectedStep = 40 + gap;
        space.Children[0].Bounds.Y.ShouldBe(firstY, 0.01);
        space.Children[1].Bounds.Y.ShouldBe(firstY + expectedStep, 0.01);
        space.Children[2].Bounds.Y.ShouldBe(firstY + 2 * expectedStep, 0.01);
        foreach (var child in space.Children)
        {
            child.Bounds.X.ShouldBe(0, 0.01);
            child.Bounds.Height.ShouldBe(40, 0.01);
        }
    }
}
