using AtomUI.Toolkits.GalleryBase.Controls;
using AtomUIGallery.ShowCases.Splitter;
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
using AtomUISplitter = AtomUI.Desktop.Controls.Splitter;

namespace AtomUIGallery.Tests.ShowCases;

public class SplitterShowCasePageTests
{

    [Fact]
    public void Splitter_Semantic_Preview_Is_Materialized_On_Tab_Selection()
    {
        AvaloniaTestApp.EnsureInitialized();

        var page = new SplitterShowCase
        {
            DataContext = new SplitterViewModel(new TestScreen())
        };

        ShowInWindow(page, 1280, 900, () =>
        {
            page.GetVisualDescendants().OfType<SemanticPartPreview>().ShouldBeEmpty();
            page.GetVisualDescendants()
                .OfType<AtomUISplitter>()
                .ShouldNotContain(static splitter => splitter.Name == "SplitterSemanticOwner");

            var host = page.GetVisualDescendants().OfType<GalleryShowCaseHost>().Single();
            host.SelectedTab = GalleryShowCaseTab.SemanticParts;
            Dispatcher.UIThread.RunJobs();

            page.GetVisualDescendants().OfType<SemanticPartPreview>().ShouldHaveSingleItem();
            var preview = page.GetVisualDescendants()
                              .OfType<SemanticPartPreview>()
                              .Single(static candidate => candidate.Name == "SplitterSemanticPreview");
            var semanticSplitter = preview.SemanticOwner.ShouldBeOfType<AtomUISplitter>();
            semanticSplitter.Name.ShouldBe("SplitterSemanticOwner");
            semanticSplitter.GetVisualDescendants()
                            .Count(static panel => panel.Classes.Contains("semantic-panel"))
                            .ShouldBe(2);
            semanticSplitter.GetVisualDescendants()
                            .Count(static dragger => dragger.Classes.Contains("semantic-dragger"))
                            .ShouldBe(1);

            host.SelectedTab = GalleryShowCaseTab.Examples;
            Dispatcher.UIThread.RunJobs();
            Assert.All(page.GetVisualDescendants().OfType<SemanticPartPreview>(), preview => Assert.False(preview.IsEffectivelyVisible));
        });
    }

    [Fact]
    public void Splitter_ShowCase_Declares_The_Semantic_Preview_And_Style_Example()
    {
        var source = ReadRepoFile(
            "controlgallery/AtomUIGallery/ShowCases/Layout/Splitter/Views/SplitterShowCase.axaml");
        var english = ReadRepoFile(
            "controlgallery/AtomUIGallery/ShowCases/Layout/Splitter/Localization/en-US.xlf");
        var semanticSource = ExtractShowCaseItemBySourceKey(source, "splitter-semantic-part");

        source.ShouldContain("<gallery:GalleryShowCaseHost.SemanticPartsContentTemplate>");
        source.ShouldContain("Name=\"SplitterSemanticPreview\"");
        source.ShouldContain("SemanticOwner=\"{Binding #SplitterSemanticOwner}\"");
        source.ShouldContain("SemanticOwnerType=\"{x:Type atom:Splitter}\"");
        CountOccurrences(source, "<gallery:SemanticPartDescription").ShouldBe(3);
        foreach (var path in new[] { "root", "panel", "dragger" })
        {
            source.ShouldContain($"Path=\"{path}\"");
        }

        semanticSource.ShouldContain("SourceKey=\"splitter-semantic-part\"");
        semanticSource.ShouldContain("SplitterShowCaseLangResource SemanticPartStyleTitle");
        semanticSource.ShouldContain("SplitterShowCaseLangResource SemanticPartStyleDescription");
        semanticSource.ShouldContain("Selector=\"atom|Splitter.semantic-style-demo-default\"");
        semanticSource.ShouldContain("Selector=\"atom|Splitter.semantic-style-demo-dashed\"");
        CountOccurrences(semanticSource, "Orientation=\"Vertical\"").ShouldBe(3);
        semanticSource.ShouldNotContain("Orientation=\"Horizontal\"");
        semanticSource.ShouldContain("Property=\"Background\"");
        semanticSource.ShouldContain("Value=\"#FFFBE6\"");
        semanticSource.ShouldContain("Value=\"#66C2DFFC\"");
        semanticSource.ShouldContain("Property=\"BorderBrush\"");
        semanticSource.ShouldContain("Value=\"#E0000000\"");
        semanticSource.ShouldContain("Property=\"BorderThickness\"");
        semanticSource.ShouldContain("Property=\"BorderDashArray\"");
        semanticSource.ShouldContain("Value=\"4,2\"");
        CountOccurrences(semanticSource, "<atom:SplitterPanelStyle").ShouldBe(0);
        CountOccurrences(semanticSource, "<atom:SplitterDraggerStyle").ShouldBe(1);
        semanticSource.ShouldContain("x:SetterTargetType=\"atom:Thumb\"");
        semanticSource.ShouldNotContain("x:SetterTargetType=\"Border\"");
        CountOccurrences(semanticSource, "<atom:Splitter ").ShouldBe(2);
        CountOccurrences(semanticSource, "Classes=\"splitter-surface\"").ShouldBe(2);
        semanticSource.ShouldContain("Classes=\"splitter-demo-label\"");
        semanticSource.ShouldContain("Classes=\"splitter-label\"");
        semanticSource.ShouldNotContain("Width=\"480\"");
        semanticSource.ShouldNotContain("HorizontalAlignment=\"Left\"");
        semanticSource.ShouldNotContain("/template/");
        semanticSource.ShouldNotContain("semantic dom", Case.Insensitive);
        english.ShouldContain("Custom Semantic Part styling");
        english.ShouldContain(
            "Mirrors the Ant Design style-class demo: customize the root background and the dragger through generated Semantic Part styles, and outline the root with a dashed border.");
        english.ShouldNotContain("semantic dom", Case.Insensitive);
    }

    [Fact]
    public void Splitter_Semantic_Style_Example_Applies_The_Official_Style_Values()
    {
        AvaloniaTestApp.EnsureInitialized();

        var page = new SplitterShowCase
        {
            DataContext = new SplitterViewModel(new TestScreen())
        };

        ShowInWindow(page, 1280, 900, () =>
        {
            var panel = page.GetVisualDescendants().OfType<ShowCasePanel>().Single();
            var item = panel.Children
                            .OfType<ShowCaseItem>()
                            .Single(static candidate => candidate.SourceKey == "splitter-semantic-part");
            item.MaterializeDeferredContent();
            Dispatcher.UIThread.RunJobs();

            var demos = page.GetVisualDescendants()
                            .OfType<AtomUISplitter>()
                            .Where(static splitter => splitter.Classes.Contains("semantic-style-demo-default") ||
                                                    splitter.Classes.Contains("semantic-style-demo-dashed"))
                            .ToArray();
            demos.Length.ShouldBe(2);

            var defaultDemo = demos.Single(static splitter => splitter.Classes.Contains("semantic-style-demo-default"));
            defaultDemo.Height.ShouldBe(200);
            defaultDemo.Orientation.ShouldBe(Avalonia.Layout.Orientation.Vertical);
            AssertSolidColor(defaultDemo.Background, "#FFFBE6");
            AssertDraggerBackground(defaultDemo, "#66C2DFFC");
            var defaultLabels = defaultDemo.GetVisualDescendants()
                                           .OfType<TextBlock>()
                                           .Where(static label => label.Classes.Contains("splitter-demo-label"))
                                           .ToArray();
            defaultLabels.Length.ShouldBe(2);
            foreach (var label in defaultLabels)
            {
                AssertSolidColor(label.Foreground, "#000000");
            }

            var dashedDemo = demos.Single(static splitter => splitter.Classes.Contains("semantic-style-demo-dashed"));
            dashedDemo.Height.ShouldBe(200);
            dashedDemo.Orientation.ShouldBe(Avalonia.Layout.Orientation.Vertical);
            AssertSolidColor(dashedDemo.BorderBrush, "#E0000000");
            dashedDemo.BorderThickness.ShouldBe(new Thickness(2));
            dashedDemo.BorderDashArray.ShouldNotBeNull();
            dashedDemo.BorderDashArray.ShouldBe(new double[] { 4, 2 }, ignoreOrder: false);
        });
    }

    private static void AssertDraggerBackground(AtomUISplitter demo, string draggerColor)
    {
        var dragger = demo.GetVisualDescendants()
                          .OfType<AtomUI.Controls.Primitives.Thumb>()
                          .Single(static candidate => candidate.Classes.Contains("semantic-dragger"));
        AssertSolidColor(dragger.Background, draggerColor);
    }

    private static void AssertSolidColor(IBrush? actual, string expected)
    {
        actual.ShouldNotBeNull()
              .ShouldBeAssignableTo<ISolidColorBrush>()
              .Color.ShouldBe(Color.Parse(expected));
    }

    private static string ExtractSplitterExampleItems(string source)
    {
        const string firstItemMarker   = "<gallery:ShowCaseItem";
        const string semanticItemMarker = "SourceKey=\"splitter-semantic-part\"";

        var firstItemStart = source.IndexOf(firstItemMarker, StringComparison.Ordinal);
        firstItemStart.ShouldBeGreaterThanOrEqualTo(0);

        var semanticItemStart = source.IndexOf(semanticItemMarker, firstItemStart, StringComparison.Ordinal);
        semanticItemStart.ShouldBeGreaterThan(firstItemStart);
        var semanticItemStartTag = source.LastIndexOf(firstItemMarker, semanticItemStart, StringComparison.Ordinal);
        semanticItemStartTag.ShouldBeGreaterThan(firstItemStart);

        return source[firstItemStart..semanticItemStartTag];
    }

    private static string ExtractShowCaseItemBySourceKey(string source, string sourceKey)
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
