using AtomUI.Controls;
using Avalonia.Media;
using Border = Avalonia.Controls.Border;

namespace AtomUI.Performance;

internal static partial class Program
{
    private static IReadOnlyList<PerfScenario> CreateFlexPanelScenarios()
    {
        return
        [
            new PerfScenario("FlexPanel.Row.Border8", _ => CreateFlexPanelRow(8)),
            new PerfScenario("FlexPanel.Wrap.Border24", _ => CreateFlexPanelWrap(24)),
            new PerfScenario("FlexPanel.Ordered.Border24", _ => CreateFlexPanelOrdered(24)),
            new PerfScenario("FlexPanel.GrowShrink.Border18", _ => CreateFlexPanelGrowShrink(18)),
            new PerfScenario("FlexPanel.ColumnWrap.Border24", _ => CreateFlexPanelColumnWrap(24))
        ];
    }

    private static FlexPanel CreateFlexPanelRow(int count)
    {
        var panel = new FlexPanel
        {
            Width         = 720,
            Direction     = FlexDirection.Row,
            ColumnSpacing = 8,
            AlignItems    = AlignItems.Stretch
        };

        AddFlexPanelBorders(panel, count, width: 64, height: 36);
        return panel;
    }

    private static FlexPanel CreateFlexPanelWrap(int count)
    {
        var panel = new FlexPanel
        {
            Width         = 360,
            Direction     = FlexDirection.Row,
            Wrap          = FlexWrap.Wrap,
            ColumnSpacing = 8,
            RowSpacing    = 8
        };

        AddFlexPanelBorders(panel, count, width: 72, height: 32);
        return panel;
    }

    private static FlexPanel CreateFlexPanelOrdered(int count)
    {
        var panel = new FlexPanel
        {
            Width         = 720,
            Direction     = FlexDirection.Row,
            Wrap          = FlexWrap.Wrap,
            ColumnSpacing = 8,
            RowSpacing    = 8
        };

        for (var i = 0; i < count; i++)
        {
            var item = CreateFlexPanelBorder(width: 64, height: 32, i);
            Flex.SetOrder(item, i % 4 == 0 ? -1 : i % 3);
            panel.Children.Add(item);
        }

        return panel;
    }

    private static FlexPanel CreateFlexPanelGrowShrink(int count)
    {
        var panel = new FlexPanel
        {
            Width         = 520,
            Direction     = FlexDirection.Row,
            Wrap          = FlexWrap.Wrap,
            ColumnSpacing = 8,
            RowSpacing    = 8
        };

        for (var i = 0; i < count; i++)
        {
            var item = CreateFlexPanelBorder(width: double.NaN, height: 36, i);
            item.MinWidth = 24;
            Flex.SetBasis(item, new FlexBasis(i % 3 == 0 ? 120 : 80));
            Flex.SetGrow(item, i % 2 == 0 ? 1.0 : 2.0);
            Flex.SetShrink(item, 1.0);
            panel.Children.Add(item);
        }

        return panel;
    }

    private static FlexPanel CreateFlexPanelColumnWrap(int count)
    {
        var panel = new FlexPanel
        {
            Width         = 360,
            Height        = 260,
            Direction     = FlexDirection.Column,
            Wrap          = FlexWrap.Wrap,
            ColumnSpacing = 8,
            RowSpacing    = 8
        };

        AddFlexPanelBorders(panel, count, width: 72, height: 32);
        return panel;
    }

    private static void AddFlexPanelBorders(FlexPanel panel, int count, double width, double height)
    {
        for (var i = 0; i < count; i++)
        {
            panel.Children.Add(CreateFlexPanelBorder(width, height, i));
        }
    }

    private static Border CreateFlexPanelBorder(double width, double height, int index)
    {
        return new Border
        {
            Width      = width,
            Height     = height,
            Background = index % 2 == 0 ? Brushes.DodgerBlue : Brushes.RoyalBlue
        };
    }
}
