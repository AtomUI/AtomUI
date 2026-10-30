using System.Xml.Linq;
using AtomUI.Theme;
using AtomUI.Theme.Schema;
using AtomUI.Theme.Styling;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Templates;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AtomUIMasonry = AtomUI.Desktop.Controls.Masonry;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.Masonry;

public class MasonrySemanticPartTests
{
    private const string ItemClass = "semantic-item";
    private const string ItemRoute = "> .semantic-item";

    static MasonrySemanticPartTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Registered_Descriptor_Exposes_Only_The_Approved_Parts()
    {
        var registry = Application.Current.ShouldNotBeNull()
                                  .GetThemeManager().ShouldNotBeNull()
                                  .SemanticParts;

        registry.TryGetControl(typeof(AtomUIMasonry), out var descriptor).ShouldBeTrue();
        descriptor.ShouldNotBeNull();
        descriptor.Parts.Select(static part => part.Name)
                  .ShouldBe(["root", "item"]);

        AssertRoot(descriptor.Parts.Single(static part => part.Name == "root"));
        AssertPart(descriptor.Parts.Single(static part => part.Name == "item"));

        registry.TryGetControl(typeof(AtomUI.Desktop.Controls.MasonryPanel), out _).ShouldBeFalse();
    }

    [Fact]
    public void Template_Declares_No_Static_Semantic_Markers()
    {
        AssertThemeMarkers(
            "src/AtomUI.Desktop.Controls/Masonry/Themes/MasonryTheme.axaml",
            Array.Empty<string>());
    }

    [Fact]
    public void ItemsSource_Containers_Produce_One_Item_Marker_Per_Container()
    {
        var masonry = new AtomUIMasonry
        {
            ColumnCount = 2,
            ColumnGap   = 10,
            RowGap      = 0,
            ItemsSource = new[] { "Alpha", "Beta", "Gamma" },
            ItemTemplate = new FuncDataTemplate<string>(
                _ => true,
                item => new Border
                {
                    Height = 40,
                    Background = Brushes.White,
                    Child = new TextBlock { Text = item }
                })
        };

        ShowInWindow(masonry, () =>
        {
            var items = GetSemanticItems(masonry).ToArray();
            items.Length.ShouldBe(3);
            items.ShouldAllBe(static item => item is ContentPresenter);
        });
    }

    [Fact]
    public void Direct_Child_Items_Are_The_Semantic_Item_Targets_Without_Wrappers()
    {
        var first = new Border
        {
            Height     = 40,
            Background = Brushes.White
        };
        var second = new TextBlock
        {
            Height = 40,
            Text   = "direct"
        };
        var masonry = new AtomUIMasonry
        {
            ColumnCount = 2,
            ColumnGap   = 10,
            RowGap      = 0
        };
        masonry.Items.Add(first);
        masonry.Items.Add(second);

        ShowInWindow(masonry, () =>
        {
            first.Classes.Contains(ItemClass).ShouldBeTrue();
            second.Classes.Contains(ItemClass).ShouldBeTrue();
            GetSemanticItems(masonry).ShouldBe([first, second], ignoreOrder: false);
        });
    }

    [Fact]
    public void Generated_Route_Style_Applies_To_Item_Containers()
    {
        var masonry = new AtomUIMasonry
        {
            ColumnCount = 2,
            ColumnGap   = 10,
            RowGap      = 0,
            ItemsSource = new[] { "Alpha", "Beta" },
            ItemTemplate = new FuncDataTemplate<string>(
                _ => true,
                item => new TextBlock { Text = item })
        };
        masonry.Classes.Add("semantic-owner");
        // 入场动效以 Animation 优先级驱动 Opacity，会覆盖样式值；本测试聚焦生成的专用 Style
        // 是否命中容器，故将动效时长归零禁用动效（等价全局 EnableMotion=false 的降级行为）。
        masonry.SetValue(AtomUIMasonry.MotionDurationProperty, TimeSpan.Zero);
        var ownerStyle = new Style(selector => selector.OfType<AtomUIMasonry>().Class("semantic-owner"));
        ownerStyle.Children.Add(new MasonryItemStyle
        {
            Setters =
            {
                new Setter(Control.TagProperty, "item"),
                new Setter(Control.OpacityProperty, 0.42)
            }
        });
        masonry.Styles.Add(ownerStyle);

        ShowInWindow(masonry, () =>
        {
            var items = GetSemanticItems(masonry).ToArray();
            items.Length.ShouldBe(2);
            items.ShouldAllBe(item => Equals(item.Tag, "item"));
            items.ShouldAllBe(item => item.Opacity == 0.42);
        });
    }

    private static IEnumerable<Control> GetSemanticItems(AtomUIMasonry masonry)
    {
        return masonry.GetVisualDescendants()
                      .OfType<Control>()
                      .Where(static control => control.Classes.Contains(ItemClass));
    }

    private static void ShowInWindow(Control content, Action assertion)
    {
        var window = new AvaloniaWindow
        {
            Width   = 420,
            Height  = 260,
            Content = content
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            Dispatcher.UIThread.RunJobs();
            assertion();
        }
        finally
        {
            window.Close();
        }
    }

    private static void AssertRoot(SemanticPartDescriptor part)
    {
        part.Path.ShouldBe("root");
        part.SelectorClass.ShouldBeNull();
        part.SelectorRoute.ShouldBeNull();
        part.ContractType.ShouldBe(typeof(AtomUIMasonry));
        part.Cardinality.ShouldBe(SemanticPartCardinality.Single);
        part.Customization.ShouldBe(SemanticPartCustomization.Root);
        part.CrossVisualRoot.ShouldBeFalse();
        part.RuntimeCreated.ShouldBeFalse();
        part.StyleType.ShouldBeNull();
    }

    private static void AssertPart(SemanticPartDescriptor part)
    {
        part.Path.ShouldBe("item");
        part.SelectorClass.ShouldBe(ItemClass);
        part.SelectorRoute.ShouldBe(ItemRoute);
        part.ContractType.ShouldBe(typeof(Control));
        part.Cardinality.ShouldBe(SemanticPartCardinality.Multiple);
        part.Customization.ShouldBe(SemanticPartCustomization.Selector);
        part.CrossVisualRoot.ShouldBeFalse();
        part.RuntimeCreated.ShouldBeTrue();
        part.Since.ShouldBe("6.2.0");
        part.StyleType.ShouldBe(typeof(MasonryItemStyle));
    }

    private static void AssertThemeMarkers(string relativePath, string[] expectedMarkers)
    {
        var document = XDocument.Load(GetRepoFile(relativePath), LoadOptions.SetLineInfo);
        var literalSemanticMarkers = document.Descendants()
                                             .Attributes()
                                             .Where(static attribute =>
                                                 attribute.Name.LocalName == "Classes" &&
                                                 attribute.Value.Split(
                                                             (char[]?)null,
                                                             StringSplitOptions.RemoveEmptyEntries)
                                                         .Any(static value => value.StartsWith(
                                                             "semantic-",
                                                             StringComparison.Ordinal)))
                                             .ToArray();
        var classPropertyMarkers = document.Descendants()
                                           .SelectMany(static element => element.Attributes()
                                               .Where(static attribute => attribute.Name.LocalName.StartsWith(
                                                   "Classes.semantic-",
                                                   StringComparison.Ordinal))
                                               .Select(attribute => (Element: element, Attribute: attribute)))
                                           .ToArray();

        literalSemanticMarkers.ShouldBeEmpty();
        classPropertyMarkers.ShouldAllBe(static marker =>
            string.Equals(marker.Attribute.Value, "true", StringComparison.OrdinalIgnoreCase));
        classPropertyMarkers.Select(static marker =>
                                $"{marker.Attribute.Name.LocalName["Classes.".Length..]}:{marker.Element.Name.LocalName}")
                            .OrderBy(static value => value, StringComparer.Ordinal)
                            .ShouldBe(expectedMarkers);
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

        return Path.Combine(AppContext.BaseDirectory, relativePath);
    }
}
