using System.Collections;
using System.Reflection;
using AtomUI;
using AtomUI.Controls;
using AtomUI.Controls.Commons;
using AtomUI.Desktop.Controls;
using AtomUI.Theme;
using AtomUI.Toolkits.GalleryBase.Controls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using ReactiveUI;
using Shouldly;
using Xunit;
using AvaloniaWindow = Avalonia.Controls.Window;
using AtomUISeparator = AtomUI.Desktop.Controls.Separator;
using AtomUITextBlock = AtomUI.Desktop.Controls.TextBlock;
using SeparatorShowCase = AtomUIGallery.ShowCases.Separator.SeparatorShowCase;
using SeparatorViewModel = AtomUIGallery.ShowCases.Separator.SeparatorViewModel;

namespace AtomUIGallery.Tests.ShowCases;

public class SeparatorShowCasePageTests
{

    [Fact]
    public void Separator_ShowCase_Declares_The_Semantic_Previews_And_Style_Example()
    {
        var source = ReadRepoFile(
            "controlgallery/AtomUIGallery/ShowCases/General/Separator/Views/SeparatorShowCase.axaml");
        var english = ReadRepoFile(
            "controlgallery/AtomUIGallery/ShowCases/General/Separator/Localization/en-US.xlf");
        var semanticSource = ExtractShowCaseItem(source, "separator-semantic-part");

        source.ShouldContain("<gallery:GalleryShowCaseHost.SemanticPartsContentTemplate>");
        source.ShouldContain("Name=\"SeparatorSemanticPreview\"");
        source.ShouldContain("SemanticOwner=\"{Binding #SeparatorSemanticOwner}\"");
        source.ShouldContain("SemanticOwnerType=\"{x:Type atom:Separator}\"");
        CountOccurrences(source, "<gallery:SemanticPartDescription").ShouldBe(3);
        foreach (var path in new[] { "root", "rail", "content" })
        {
            source.ShouldContain($"Path=\"{path}\"");
        }

        semanticSource.ShouldContain("SourceKey=\"separator-semantic-part\"");
        semanticSource.ShouldContain("SeparatorShowCaseLangResource SemanticPartStyleTitle");
        semanticSource.ShouldContain("SeparatorShowCaseLangResource SemanticPartStyleDescription");
        semanticSource.ShouldContain("Value=\"#7cb305\"");
        semanticSource.ShouldContain("Value=\"#722ed1\"");
        semanticSource.ShouldContain("Value=\"Dashed\"");
        semanticSource.ShouldContain("Value=\"0.85\"");
        semanticSource.ShouldContain("Value=\"0.6\"");
        semanticSource.ShouldContain("Property=\"FontStyle\"");
        semanticSource.ShouldContain("Property=\"FontWeight\"");
        semanticSource.ShouldContain("atom:SeparatorRailStyle");
        semanticSource.ShouldContain("atom:SeparatorContentStyle");
        CountOccurrences(semanticSource, "<atom:Separator ").ShouldBe(4);
        semanticSource.ShouldNotContain("/template/");
        semanticSource.ShouldNotContain("classNames", Case.Insensitive);
        semanticSource.ShouldNotContain("semantic dom", Case.Insensitive);
        english.ShouldContain("Custom Semantic Part styling");
        english.ShouldContain("Root element with border-top style, divider styling and other basic divider container styles");
        english.ShouldContain("Background rail element with border-top style and other divider connection line styles");
        english.ShouldContain("Content element with inline-block display, padding and other divider text content styles");
        english.ShouldNotContain("semantic dom", Case.Insensitive);
        english.ShouldNotContain("classNames", Case.Insensitive);
    }

    [Fact]
    public void Separator_Semantic_Previews_Are_Materialized_Only_After_The_Tab_Is_Selected()
    {
        AvaloniaTestApp.EnsureInitialized();

        var page = new SeparatorShowCase
        {
            DataContext = new SeparatorViewModel(new TestScreen())
        };

        ShowInWindow(page, 1280, 900, () =>
        {
            page.GetVisualDescendants().OfType<SemanticPartPreview>().ShouldBeEmpty();
            page.GetVisualDescendants()
                .OfType<AtomUISeparator>()
                .ShouldNotContain(static separator => separator.Name == "SeparatorSemanticOwner");

            var host = page.GetVisualDescendants().OfType<GalleryShowCaseHost>().Single();
            host.SelectedTab = GalleryShowCaseTab.SemanticParts;
            Dispatcher.UIThread.RunJobs();

            page.GetVisualDescendants().OfType<SemanticPartPreview>().ShouldHaveSingleItem();
            var preview = page.GetVisualDescendants()
                              .OfType<SemanticPartPreview>()
                              .Single(static candidate => candidate.Name == "SeparatorSemanticPreview");
            var semanticSeparator = preview.SemanticOwner.ShouldBeOfType<AtomUISeparator>();
            semanticSeparator.Name.ShouldBe("SeparatorSemanticOwner");
            semanticSeparator.GetVisualDescendants()
                             .OfType<AtomUITextBlock>()
                             .Single(static text => text.Classes.Contains("semantic-content"))
                             .IsEffectivelyVisible.ShouldBeTrue();

            var previewItemsValue = typeof(SemanticPartPreview)
                                    .GetProperty("Items", BindingFlags.Instance | BindingFlags.NonPublic)
                                    ?.GetValue(preview);
            previewItemsValue.ShouldNotBeNull();
            var previewItems = previewItemsValue.ShouldBeAssignableTo<IEnumerable>()
                                                .Cast<object>()
                                                .ToArray();
            previewItems.Length.ShouldBe(3);

            host.SelectedTab = GalleryShowCaseTab.Examples;
            Dispatcher.UIThread.RunJobs();
            Assert.All(page.GetVisualDescendants().OfType<SemanticPartPreview>(), preview => Assert.False(preview.IsEffectivelyVisible));
        });
    }

    [Fact]
    public void Separator_Semantic_Style_Example_Applies_The_Official_Style_Values()
    {
        AvaloniaTestApp.EnsureInitialized();

        var page = new SeparatorShowCase
        {
            DataContext = new SeparatorViewModel(new TestScreen())
        };

        ShowInWindow(page, 1280, 1400, () =>
        {
            var panel = page.GetVisualDescendants().OfType<ShowCasePanel>().Single();
            var item = panel.Children
                            .OfType<ShowCaseItem>()
                            .Single(static candidate => candidate.SourceKey == "separator-semantic-part");
            item.MaterializeDeferredContent();
            Dispatcher.UIThread.RunJobs();

            var demos = page.GetVisualDescendants()
                            .OfType<AtomUISeparator>()
                            .Where(static separator => separator.Classes.Contains("semantic-demo"))
                            .ToArray();
            demos.Length.ShouldBe(4);

            var allPartsDemo = demos[0];
            AssertSolidColor(allPartsDemo.LineColor, "#7cb305");
            var allPartsContent = allPartsDemo.GetVisualDescendants()
                                              .OfType<AtomUITextBlock>()
                                              .Single(static text => text.Classes.Contains("semantic-content"));
            allPartsContent.FontStyle.ShouldBe(FontStyle.Italic);

            var titlePlacementDemo = demos[1];
            titlePlacementDemo.TitlePosition.ShouldBe(SeparatorTitlePosition.Left);
            AssertSolidColor(titlePlacementDemo.LineColor, "#722ed1");
            var titlePlacementContent = titlePlacementDemo.GetVisualDescendants()
                                                          .OfType<AtomUITextBlock>()
                                                          .Single(static text => text.Classes.Contains("semantic-content"));
            titlePlacementContent.FontWeight.ShouldBe(FontWeight.Bold);

            var dashedRailDemo = demos[2];
            dashedRailDemo.Variant.ShouldBe(SeparatorVariant.Dashed);
            var dashedRails = dashedRailDemo.GetVisualDescendants()
                                             .OfType<SeparatorRail>()
                                             .Where(static rail => rail.Classes.Contains("semantic-rail"))
                                             .ToArray();
            dashedRails.Length.ShouldBe(2);
            foreach (var rail in dashedRails)
            {
                rail.Opacity.ShouldBe(0.85);
            }

            var smallSizeDemo = demos[3];
            smallSizeDemo.SizeType.ShouldBe(CustomizableSizeType.Small);
            smallSizeDemo.Opacity.ShouldBe(0.6);
        });
    }

    [Fact]
    public void Separator_Semantic_Preview_Highlights_Every_Titled_Content_Instance()
    {
        AvaloniaTestApp.EnsureInitialized();

        var page = new SeparatorShowCase
        {
            DataContext = new SeparatorViewModel(new TestScreen())
        };

        ShowInWindow(page, 1280, 900, () =>
        {
            var host = page.GetVisualDescendants().OfType<GalleryShowCaseHost>().Single();
            host.SelectedTab = GalleryShowCaseTab.SemanticParts;
            Dispatcher.UIThread.RunJobs();

            var preview = page.GetVisualDescendants()
                              .OfType<SemanticPartPreview>()
                              .Single(static candidate => candidate.Name == "SeparatorSemanticPreview");
            var contentItem = ReadPreviewItems(preview).Single(static item => ReadItemPath(item) == "content");

            InvokeSetHoveredPart(preview, contentItem, true);
            Dispatcher.UIThread.RunJobs();

            var session = ReadHighlightSession(preview).ShouldNotBeNull();
            ReadTotalMatchCount(session).ShouldBe(3);
        });
    }

    [Fact]
    public void Separator_Semantic_Preview_Highlights_Every_Rail_Instance()
    {
        AvaloniaTestApp.EnsureInitialized();

        var page = new SeparatorShowCase
        {
            DataContext = new SeparatorViewModel(new TestScreen())
        };

        ShowInWindow(page, 1280, 900, () =>
        {
            var host = page.GetVisualDescendants().OfType<GalleryShowCaseHost>().Single();
            host.SelectedTab = GalleryShowCaseTab.SemanticParts;
            Dispatcher.UIThread.RunJobs();

            var preview = page.GetVisualDescendants()
                              .OfType<SemanticPartPreview>()
                              .Single(static candidate => candidate.Name == "SeparatorSemanticPreview");
            var railItem = ReadPreviewItems(preview).Single(static item => ReadItemPath(item) == "rail");

            InvokeSetHoveredPart(preview, railItem, true);
            Dispatcher.UIThread.RunJobs();

            var session = ReadHighlightSession(preview).ShouldNotBeNull();
            // 1 untitled + 3 titled (2 rails each) + 3 vertical (1 rail each) = 10 visible rails.
            ReadTotalMatchCount(session).ShouldBe(10);
        });
    }

    private static string ExtractSeparatorExampleItems(string source)
    {
        const string firstItemMarker = "<gallery:ShowCaseItem";
        var firstItemStart = source.IndexOf(firstItemMarker, StringComparison.Ordinal);
        firstItemStart.ShouldBeGreaterThanOrEqualTo(0);

        const string semanticItemMarker = "SourceKey=\"separator-semantic-part\"";
        var semanticItemStart = source.IndexOf(semanticItemMarker, firstItemStart, StringComparison.Ordinal);
        semanticItemStart.ShouldBeGreaterThan(firstItemStart);
        var semanticItemStartTag = source.LastIndexOf(firstItemMarker, semanticItemStart, StringComparison.Ordinal);
        semanticItemStartTag.ShouldBeGreaterThan(firstItemStart);

        return source[firstItemStart..semanticItemStartTag];
    }

    private static string ExtractShowCaseItem(string source, string sourceKey)
    {
        var sourceKeyIndex = source.IndexOf($"SourceKey=\"{sourceKey}\"", StringComparison.Ordinal);
        sourceKeyIndex.ShouldBeGreaterThanOrEqualTo(0);

        var itemStart = source.LastIndexOf("<gallery:ShowCaseItem", sourceKeyIndex, StringComparison.Ordinal);
        itemStart.ShouldBeGreaterThanOrEqualTo(0);

        const string itemEndMarker = "</gallery:ShowCaseItem>";
        var itemEnd = source.IndexOf(itemEndMarker, sourceKeyIndex, StringComparison.Ordinal);
        itemEnd.ShouldBeGreaterThan(sourceKeyIndex);

        return source[itemStart..(itemEnd + itemEndMarker.Length)];
    }

    private static string NormalizeMarkup(string source)
    {
        return ShowCaseSnapshotMarkup.Normalize(source);
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

    private static void AssertSolidColor(IBrush? actual, string expected)
    {
        actual.ShouldNotBeNull()
              .ShouldBeAssignableTo<ISolidColorBrush>()
              .Color.ShouldBe(Color.Parse(expected));
    }

    private static object[] ReadPreviewItems(SemanticPartPreview preview)
    {
        return ReadInternalProperty(preview, "Items")
              .ShouldBeAssignableTo<IEnumerable>()!
              .Cast<object>()
              .ToArray();
    }

    private static string ReadItemPath(object item)
    {
        return ReadInternalProperty(item, "Path").ShouldNotBeNull().ToString()!;
    }

    private static void InvokeSetHoveredPart(SemanticPartPreview preview, object item, bool isHovered)
    {
        ReadInternalMethod(preview, "SetHoveredPart").Invoke(preview, [item, isHovered]);
    }

    private static object? ReadHighlightSession(SemanticPartPreview preview)
    {
        return ReadInternalProperty(preview, "ActiveHighlightSession");
    }

    private static int ReadTotalMatchCount(object session)
    {
        return ReadInternalProperty(session, "TotalMatchCount").ShouldBeAssignableTo<int>();
    }

    private static object? ReadInternalProperty(object instance, string name)
    {
        var property = instance.GetType().GetProperty(name, BindingFlags.Instance | BindingFlags.NonPublic)
                       ?? instance.GetType().GetProperty(name, BindingFlags.Instance | BindingFlags.Public);
        return property.ShouldNotBeNull().GetValue(instance);
    }

    private static MethodInfo ReadInternalMethod(object instance, string name)
    {
        return instance.GetType().GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic)
               ?? instance.GetType().GetMethod(name, BindingFlags.Instance | BindingFlags.Public)
               ?? throw new MissingMethodException(instance.GetType().FullName, name);
    }

    private static string ReadRepoFile(string relativePath)
    {
        var path = GetRepoFile(relativePath);
        File.Exists(path).ShouldBeTrue($"Expected repository file to exist: {relativePath}");
        return File.ReadAllText(path);
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

    private static void ShowInWindow(Control content, double width, double height, Action assertion)
    {
        var visualLayerManager = new VisualLayerManager
        {
            EnableAdornerLayer = true,
            Child = content
        };
        var window = new AvaloniaWindow
        {
            Content = visualLayerManager,
            Width = width,
            Height = height
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
            Dispatcher.UIThread.RunJobs();
        }
    }

    private sealed class TestScreen : IScreen
    {
        public RoutingState Router { get; } = new();
    }
}
