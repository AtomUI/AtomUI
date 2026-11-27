using AtomUI.Controls;
using Avalonia.Media;
using Border = Avalonia.Controls.Border;
using Flex = Avalonia.Controls.Flex;
using FlexBasis = Avalonia.Controls.FlexBasis;
using FlexDirection = Avalonia.Controls.FlexDirection;

namespace AtomUI.Performance;

internal static partial class Program
{
    private static bool RunFlexPanelStateVerification()
    {
        var failures = new List<string>();
        VerifyFlexPanelOrderIsStable(failures);
        VerifyFlexPanelInvisibleChildrenAreSkipped(failures);
        VerifyFlexPanelWrapsToNextLine(failures);
        VerifyFlexPanelGrowDistribution(failures);

        if (failures.Count == 0)
        {
            Console.WriteLine("FlexPanel state verification passed.");
            return true;
        }

        Console.Error.WriteLine("FlexPanel state verification failed:");
        foreach (var failure in failures)
        {
            Console.Error.WriteLine($"- {failure}");
        }

        return false;
    }

    private static void VerifyFlexPanelOrderIsStable(ICollection<string> failures)
    {
        var first = CreateNamedFlexPanelBorder("first", width: 40, height: 20);
        var second = CreateNamedFlexPanelBorder("second", width: 40, height: 20);
        var third = CreateNamedFlexPanelBorder("third", width: 40, height: 20);
        var fourth = CreateNamedFlexPanelBorder("fourth", width: 40, height: 20);
        Flex.SetOrder(first, 0);
        Flex.SetOrder(second, -1);
        Flex.SetOrder(third, 0);
        Flex.SetOrder(fourth, -1);

        var panel = new FlexPanel
        {
            Width     = 240,
            Direction = FlexDirection.Row
        };
        panel.Children.Add(first);
        panel.Children.Add(second);
        panel.Children.Add(third);
        panel.Children.Add(fourth);

        using var realized = RealizeControl(panel);
        RefreshLayout(realized.Window);

        Expect(second.Bounds.X < fourth.Bounds.X &&
               fourth.Bounds.X < first.Bounds.X &&
               first.Bounds.X < third.Bounds.X,
            $"FlexPanel should sort by Flex.Order and keep source order for equal values; actual x: first={first.Bounds.X}, second={second.Bounds.X}, third={third.Bounds.X}, fourth={fourth.Bounds.X}.",
            failures);
    }

    private static void VerifyFlexPanelInvisibleChildrenAreSkipped(ICollection<string> failures)
    {
        var first = CreateNamedFlexPanelBorder("first", width: 40, height: 20);
        var hidden = CreateNamedFlexPanelBorder("hidden", width: 200, height: 20);
        hidden.IsVisible = false;
        var second = CreateNamedFlexPanelBorder("second", width: 40, height: 20);

        var panel = new FlexPanel
        {
            Width     = 400,
            Direction = FlexDirection.Row
        };
        panel.Children.Add(first);
        panel.Children.Add(hidden);
        panel.Children.Add(second);

        using var realized = RealizeControl(panel);
        RefreshLayout(realized.Window);

        Expect(Math.Abs(second.Bounds.X - first.Bounds.Right) < 0.001,
            $"Invisible FlexPanel child should not consume main-axis space; first right={first.Bounds.Right}, second x={second.Bounds.X}.",
            failures);
    }

    private static void VerifyFlexPanelWrapsToNextLine(ICollection<string> failures)
    {
        var first = CreateNamedFlexPanelBorder("first", width: 60, height: 20);
        var second = CreateNamedFlexPanelBorder("second", width: 60, height: 20);

        var panel = new FlexPanel
        {
            Width     = 100,
            Direction = FlexDirection.Row,
            Wrap      = FlexWrap.Wrap
        };
        panel.Children.Add(first);
        panel.Children.Add(second);

        using var realized = RealizeControl(panel);
        RefreshLayout(realized.Window);

        Expect(second.Bounds.Y > first.Bounds.Y,
            $"FlexPanel should wrap overflowing row item to the next line; first y={first.Bounds.Y}, second y={second.Bounds.Y}.",
            failures);
    }

    private static void VerifyFlexPanelGrowDistribution(ICollection<string> failures)
    {
        var first = CreateNamedFlexPanelBorder("first", width: double.NaN, height: 20);
        var second = CreateNamedFlexPanelBorder("second", width: double.NaN, height: 20);
        Flex.SetBasis(first, new FlexBasis(50));
        Flex.SetBasis(second, new FlexBasis(50));
        Flex.SetGrow(first, 1);
        Flex.SetGrow(second, 2);

        var panel = new FlexPanel
        {
            Width     = 300,
            Direction = FlexDirection.Row
        };
        panel.Children.Add(first);
        panel.Children.Add(second);

        using var realized = RealizeControl(panel);
        RefreshLayout(realized.Window);

        Expect(second.Bounds.Width > first.Bounds.Width,
            $"FlexPanel grow factor should allocate more width to the larger grow item; first width={first.Bounds.Width}, second width={second.Bounds.Width}.",
            failures);
    }

    private static Border CreateNamedFlexPanelBorder(string name, double width, double height)
    {
        return new Border
        {
            Name       = name,
            Width      = width,
            Height     = height,
            Background = Brushes.DodgerBlue
        };
    }
}
