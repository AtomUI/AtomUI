using AtomUI.Controls;
using AtomUI.Desktop.Controls;
using AtomUI.Icons.AntDesign;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Styling;

namespace AtomUI.Performance;

internal static partial class Program
{
    private static IReadOnlyList<PerfScenario> CreateSpaceScenarios()
    {
        return
        [
            new PerfScenario("Space.Horizontal.Button3", _ => CreateButtonSpace(3, Orientation.Horizontal)),
            new PerfScenario("Space.Wrap.Button15", _ => CreateButtonSpace(15, Orientation.Horizontal, width: 360)),
            new PerfScenario("Space.Split.Link3", _ => CreateSplitSpace()),
            new PerfScenario("CompactSpace.LineEdit3", _ => CreateLineEditCompactSpace(Orientation.Horizontal)),
            new PerfScenario("CompactSpace.LineEdit3.Vertical", _ => CreateLineEditCompactSpace(Orientation.Vertical)),
            new PerfScenario("CompactSpace.ButtonGroup", _ => CreateButtonCompactSpace()),
            new PerfScenario("CompactSpace.MixedForm", _ => CreateMixedCompactSpace()),
            new PerfScenario("CompactSpace.SingleChild", _ => CreateSingleChildCompactSpace()),
            new PerfScenario("CompactSpace.WithFiller", _ => CreateCompactSpaceWithFiller())
        ];
    }

    private static Space CreateButtonSpace(int count, Orientation orientation, double width = double.NaN)
    {
        var space = new Space
        {
            Orientation = orientation,
            Width       = width
        };

        for (var i = 0; i < count; i++)
        {
            space.Children.Add(new AtomUI.Desktop.Controls.Button
            {
                Content = $"Button {i + 1}"
            });
        }

        return space;
    }

    private static Space CreateSplitSpace()
    {
        var space = new Space
        {
            Orientation   = Orientation.Horizontal,
            ItemsAlignment = SpaceItemsAlignment.Center,
            SplitTemplate = new SeparatorTemplate()
        };

        space.Children.Add(new HyperLinkTextBlock { Text = "Link 1" });
        space.Children.Add(new HyperLinkTextBlock { Text = "Link 2" });
        space.Children.Add(new HyperLinkTextBlock { Text = "Link 3" });
        return space;
    }

    private static CompactSpace CreateLineEditCompactSpace(Orientation orientation)
    {
        var compactSpace = new CompactSpace
        {
            Orientation = orientation,
            Width       = 520
        };
        compactSpace.Children.Add(new LineEdit { Text = "first" });
        compactSpace.Children.Add(new LineEdit { Text = "middle" });
        compactSpace.Children.Add(new LineEdit { Text = "last" });
        return compactSpace;
    }

    private static CompactSpace CreateButtonCompactSpace()
    {
        var compactSpace = new CompactSpace
        {
            Orientation = Orientation.Horizontal
        };
        compactSpace.Children.Add(new AtomUI.Desktop.Controls.Button { Icon = new LikeOutlined() });
        compactSpace.Children.Add(new AtomUI.Desktop.Controls.Button { Icon = new CommentOutlined() });
        compactSpace.Children.Add(new AtomUI.Desktop.Controls.Button { Icon = new StarOutlined() });
        compactSpace.Children.Add(new AtomUI.Desktop.Controls.Button { Icon = new HeartOutlined() });
        compactSpace.Children.Add(new AtomUI.Desktop.Controls.Button { Icon = new ShareAltOutlined() });
        return compactSpace;
    }

    private static CompactSpace CreateMixedCompactSpace()
    {
        var compactSpace = new CompactSpace
        {
            Orientation = Orientation.Horizontal,
            Width       = 720
        };
        compactSpace.Children.Add(new Select
        {
            PlaceholderText = "Province"
        });
        compactSpace.Children.Add(new LineEdit
        {
            Text = "Xihu District, Hangzhou"
        });
        compactSpace.Children.Add(new AtomUI.Desktop.Controls.NumericUpDown
        {
            Value = 12
        });
        compactSpace.Children.Add(new CompactSpaceAddOn
        {
            Content = "$"
        });
        compactSpace.Children.Add(new CompactSpaceFiller());
        return compactSpace;
    }

    private static CompactSpace CreateSingleChildCompactSpace()
    {
        var compactSpace = new CompactSpace
        {
            Orientation = Orientation.Horizontal
        };
        compactSpace.Children.Add(new LineEdit { Text = "single" });
        return compactSpace;
    }

    private static CompactSpace CreateCompactSpaceWithFiller()
    {
        var compactSpace = new CompactSpace
        {
            Orientation = Orientation.Horizontal,
            Width       = 520
        };
        compactSpace.Children.Add(new LineEdit { Text = "input" });
        compactSpace.Children.Add(new AtomUI.Desktop.Controls.Button
        {
            ButtonType = ButtonType.Primary,
            Content    = "Submit"
        });
        compactSpace.Children.Add(new CompactSpaceFiller());
        return compactSpace;
    }
}

internal sealed class SeparatorTemplate : ITemplate<Control>
{
    public Control Build()
    {
        return new AtomUI.Desktop.Controls.Separator
        {
            Orientation = Orientation.Vertical
        };
    }

    object? ITemplate.Build() => Build();
}
