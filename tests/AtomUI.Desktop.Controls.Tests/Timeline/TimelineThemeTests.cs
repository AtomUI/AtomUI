using System.Xml.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.Timeline;

public class TimelineThemeTests
{
    static TimelineThemeTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Theme_Propagates_Orientation_And_Isolates_Vertical_Spacing()
    {
        var timelineTheme = ReadRepoFile(
            "src/AtomUI.Desktop.Controls/Timeline/Themes/TimelineTheme.axaml");
        var itemTheme = ReadRepoFile(
            "src/AtomUI.Desktop.Controls/Timeline/Themes/TimelineItemTheme.axaml");

        timelineTheme.ShouldContain(
            "Orientation=\"{Binding Orientation, RelativeSource={RelativeSource AncestorType=atom:Timeline}}\"");
        timelineTheme.ShouldContain("BorderThickness=\"{TemplateBinding BorderThickness}\"");
        itemTheme.ShouldContain("Orientation=\"{TemplateBinding Orientation}\"");
        itemTheme.ShouldContain(
            "IndicatorSpacing=\"{atom:SharedTokenResource UniformlyPaddingXS}\"");
        itemTheme.ShouldContain("Selector=\"^[Orientation=Vertical]\"");
        itemTheme.ShouldContain("Selector=\"^[Orientation=Horizontal]\"");
    }

    [Fact]
    public void Vertical_Item_Spacing_Lives_On_The_Item_Padding_Not_The_Text_Nodes()
    {
        var itemTheme = ReadRepoFile(
            "src/AtomUI.Desktop.Controls/Timeline/Themes/TimelineItemTheme.axaml");

        itemTheme.ShouldContain("<Setter Property=\"Padding\" Value=\"{atom:TimelineTokenResource ItemPaddingBottom}\" />");
        CountOccurrences(itemTheme, "ItemPaddingBottomLG").ShouldBe(2);
        itemTheme.ShouldContain("AxisOverflow=\"{TemplateBinding Padding}\"");
        // Alignment setters on text nodes are valid; spacing must stay on the item.
        var styles = XDocument.Parse(itemTheme).Descendants()
            .Where(element => element.Name.LocalName == "Style" &&
                (element.Attribute("Selector")?.Value.Contains("#Label") == true ||
                 element.Attribute("Selector")?.Value.Contains("#ContentPresenter") == true));
        styles.SelectMany(style => style.Elements())
            .Where(element => element.Name.LocalName == "Setter")
            .Where(setter => setter.Attribute("Property")?.Value is "Padding" or "Margin")
            .ShouldBeEmpty();
    }

    [Fact]
    public void Item_Padding_Keeps_The_Wrapper_Tight_And_Extends_The_Indicator()
    {
        var timeline = new Desktop.Controls.Timeline
        {
            Width = 400,
            Items =
            {
                new Desktop.Controls.TimelineItem
                {
                    Label   = "2015-09-01",
                    Content = "Create a services"
                },
                new Desktop.Controls.TimelineItem
                {
                    Label   = "2015-09-01 09:12:11",
                    Content = "Solve initial network problems"
                }
            }
        };

        ShowInWindow(timeline, () =>
        {
            var items = timeline.GetVisualDescendants()
                                .OfType<Desktop.Controls.TimelineItem>()
                                .ToArray();
            items.Length.ShouldBe(2);

            // 非末项：间距由 item Padding 承载，wrapper/section 紧凑，
            // 指示器延伸到整个 item 高度（rail 跨越间距保持连续）。
            var firstItem = items[0];
            var firstWrapper   = GetNamedPart(firstItem, "RootLayout");
            var firstSection   = GetNamedPart(firstItem, "Section");
            var firstIndicator = GetNamedPart(firstItem, "Indicator");
            firstWrapper.Bounds.Height.ShouldBe(firstSection.Bounds.Height, 0.01);
            firstItem.Bounds.Height.ShouldBeGreaterThan(firstWrapper.Bounds.Height);
            firstIndicator.Bounds.Height.ShouldBe(firstItem.Bounds.Height, 0.01);
            firstIndicator.Bounds.Bottom.ShouldBe(firstItem.Bounds.Bottom, 0.01);

            // 末项：对齐上游 li:last-child，不再保留下内边距。
            var lastItem    = items[1];
            var lastWrapper = GetNamedPart(lastItem, "RootLayout");
            lastItem.Padding.ShouldBe(new Thickness(0));
            lastWrapper.Bounds.Height.ShouldBe(lastItem.Bounds.Height, 0.01);
        });
    }

    private static Control GetNamedPart(Visual root, string name)
    {
        return root.GetVisualDescendants()
                   .OfType<Control>()
                   .Single(part => part.Name == name);
    }

    private static int CountOccurrences(string source, string value)
    {
        var count      = 0;
        var startIndex = 0;
        while (true)
        {
            var matchIndex = source.IndexOf(value, startIndex, StringComparison.Ordinal);
            if (matchIndex < 0)
            {
                return count;
            }

            count++;
            startIndex = matchIndex + value.Length;
        }
    }

    [Fact]
    public void Indicator_And_Item_Do_Not_Clip_The_Overflowing_Rail()
    {
        var timeline = new Desktop.Controls.Timeline
        {
            Width = 400,
            Items =
            {
                new Desktop.Controls.TimelineItem { Label = "2024-01-01", Content = "Item 1" },
                new Desktop.Controls.TimelineItem { Label = "2024-08-12", Content = "Item 2" }
            }
        };

        ShowInWindow(timeline, () =>
        {
            var item = timeline.GetVisualDescendants()
                               .OfType<Desktop.Controls.TimelineItem>()
                               .First();
            var indicator = item.GetVisualDescendants()
                                .OfType<Control>()
                                .Single(control => control.Name == "Indicator");

            // rail 要连续延伸到下一项节点顶边，必须跨出 indicator 与 item 的
            // 底部边界，因此这两层都不能裁剪（TemplatedControl 默认 ClipToBounds=true
            // 会把溢出的 rail 段切断，形成节点之间的视觉缺口）。
            indicator.ClipToBounds.ShouldBeFalse();
            item.ClipToBounds.ShouldBeFalse();
        });
    }

    [Fact]
    public void Root_Border_Surface_Renders_The_Owner_Border()
    {
        var timeline = new Desktop.Controls.Timeline
        {
            Width          = 400,
            BorderBrush    = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.FromRgb(0xA2, 0x94, 0xF9)),
            BorderThickness = new Avalonia.Thickness(1),
            Items =
            {
                new Desktop.Controls.TimelineItem
                {
                    Label   = "2015-09-01",
                    Content = "Create a services site"
                }
            }
        };

        ShowInWindow(timeline, () =>
        {
            var frame = timeline.GetVisualDescendants()
                                .OfType<Border>()
                                .Single(border => border.Name == "Frame");
            frame.BorderThickness.ShouldBe(new Avalonia.Thickness(1));
            frame.BorderBrush.ShouldNotBeNull();
        });
    }

    [Fact]
    public void Orientation_Propagates_Through_The_Runtime_Theme()
    {
        var item = new Desktop.Controls.TimelineItem
        {
            Label   = "Label",
            Content = "Content"
        };
        var timeline = new Desktop.Controls.Timeline
        {
            Width       = 600,
            Orientation = Orientation.Horizontal,
            Items       = { item }
        };

        ShowInWindow(timeline, () =>
        {
            var stackPanel = timeline.GetVisualDescendants()
                                     .OfType<StackPanel>()
                                     .Single(panel => panel.GetType().Name == "TimelineStackPanel");
            var itemPanel = item.GetVisualDescendants()
                                .OfType<Panel>()
                                .Single(panel => panel.Name == "Section");
            var indicator = item.GetVisualDescendants()
                                .OfType<Control>()
                                .Single(control => control.Name == "Indicator");

            stackPanel.Orientation.ShouldBe(Orientation.Horizontal);
            GetOrientation(itemPanel).ShouldBe(Orientation.Horizontal);
            GetOrientation(indicator).ShouldBe(Orientation.Horizontal);

            timeline.Orientation = Orientation.Vertical;
            Dispatcher.UIThread.RunJobs();

            stackPanel.Orientation.ShouldBe(Orientation.Vertical);
            GetOrientation(itemPanel).ShouldBe(Orientation.Vertical);
            GetOrientation(indicator).ShouldBe(Orientation.Vertical);
        });
    }

    private static Orientation GetOrientation(Control control)
    {
        return control.GetType()
                      .GetProperty("Orientation")!
                      .GetValue(control)
                      .ShouldBeOfType<Orientation>();
    }

    private static void ShowInWindow(Control content, Action assertion)
    {
        var window = new AvaloniaWindow
        {
            Width   = 760,
            Height  = 320,
            Content = content
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            assertion();
        }
        finally
        {
            window.Close();
        }
    }

    private static string ReadRepoFile(string relativePath)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var candidate = Path.Combine(directory.FullName, relativePath);
            if (File.Exists(candidate))
            {
                return File.ReadAllText(candidate);
            }

            directory = directory.Parent;
        }

        throw new FileNotFoundException($"Could not find repository file: {relativePath}");
    }
}
