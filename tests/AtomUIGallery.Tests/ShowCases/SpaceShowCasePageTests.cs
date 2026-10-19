using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using AtomUI.Toolkits.GalleryBase.Controls;
using AtomUIGallery.ShowCases.Space;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using ReactiveUI;
using Shouldly;
using Xunit;
using AvaloniaTextBlock = Avalonia.Controls.TextBlock;
using AvaloniaWindow = Avalonia.Controls.Window;
using AtomUIButton = AtomUI.Desktop.Controls.Button;
using AtomUISpace = AtomUI.Desktop.Controls.Space;

namespace AtomUIGallery.Tests.ShowCases;

public class SpaceShowCasePageTests
{

    [Fact]
    public void Space_Semantic_Preview_Is_Materialized_On_Tab_Selection()
    {
        AvaloniaTestApp.EnsureInitialized();

        var page = new SpaceShowCase
        {
            DataContext = new SpaceViewModel(new TestScreen())
        };

        ShowInWindow(page, 1280, 900, () =>
        {
            page.GetVisualDescendants().OfType<SemanticPartPreview>().ShouldBeEmpty();
            page.GetVisualDescendants()
                .OfType<AtomUISpace>()
                .ShouldNotContain(static space => space.Name == "SpaceSemanticOwner");

            var host = page.GetVisualDescendants().OfType<GalleryShowCaseHost>().Single();
            host.SelectedTab = GalleryShowCaseTab.SemanticParts;
            Dispatcher.UIThread.RunJobs();

            page.GetVisualDescendants().OfType<SemanticPartPreview>().ShouldHaveSingleItem();
            var preview = page.GetVisualDescendants()
                              .OfType<SemanticPartPreview>()
                              .Single(static candidate => candidate.Name == "SpaceSemanticPreview");
            var semanticSpace = preview.SemanticOwner.ShouldBeOfType<AtomUISpace>();
            semanticSpace.Name.ShouldBe("SpaceSemanticOwner");
            semanticSpace.HorizontalAlignment.ShouldBe(HorizontalAlignment.Center);
            semanticSpace.VerticalAlignment.ShouldBe(VerticalAlignment.Center);

            var previewStageHost = preview.GetVisualDescendants()
                                          .OfType<ContentPresenter>()
                                          .Single(static candidate => candidate.Name == "PART_PreviewContentHost");
            previewStageHost.VerticalAlignment.ShouldBe(VerticalAlignment.Stretch);
            previewStageHost.VerticalContentAlignment.ShouldBe(VerticalAlignment.Center);
            semanticSpace.GetVisualDescendants()
                         .Count(static item => item.Classes.Contains("semantic-item"))
                         .ShouldBe(3);
            semanticSpace.GetVisualDescendants()
                         .Count(static separator => separator.Classes.Contains("semantic-separator"))
                         .ShouldBe(2);

            host.SelectedTab = GalleryShowCaseTab.Examples;
            Dispatcher.UIThread.RunJobs();
            Assert.All(page.GetVisualDescendants().OfType<SemanticPartPreview>(), preview => Assert.False(preview.IsEffectivelyVisible));
        });
    }

    [Fact]
    public void Space_ShowCase_Declares_The_Semantic_Preview_And_Style_Example()
    {
        var source = ReadRepoFile(
            "controlgallery/AtomUIGallery/ShowCases/Layout/Space/Views/SpaceShowCase.axaml");
        var english = ReadRepoFile(
            "controlgallery/AtomUIGallery/ShowCases/Layout/Space/Localization/en-US.xlf");
        var semanticSource = ExtractShowCaseItemBySourceKey(source, "space-semantic-part");

        source.ShouldContain("<gallery:GalleryShowCaseHost.SemanticPartsContentTemplate>");
        source.ShouldContain("Name=\"SpaceSemanticPreview\"");
        source.ShouldContain("SemanticOwner=\"{Binding #SpaceSemanticOwner}\"");
        source.ShouldContain("SemanticOwnerType=\"{x:Type atom:Space}\"");
        source.ShouldContain("HorizontalAlignment=\"Center\"");
        CountOccurrences(source, "<gallery:SemanticPartDescription").ShouldBe(3);
        foreach (var path in new[] { "root", "item", "separator" })
        {
            source.ShouldContain($"Path=\"{path}\"");
        }

        semanticSource.ShouldContain("SourceKey=\"space-semantic-part\"");
        semanticSource.ShouldContain("SpaceShowCaseLangResource SemanticPartStyleTitle");
        semanticSource.ShouldContain("SpaceShowCaseLangResource SemanticPartStyleDescription");
        semanticSource.ShouldContain("Selector=\"atom|Space.semantic-style-demo-object\"");
        semanticSource.ShouldContain("Selector=\"atom|Space.semantic-style-demo-function\"");
        semanticSource.ShouldContain("Property=\"Background\"");
        semanticSource.ShouldContain("Value=\"#F0F0F0\"");
        semanticSource.ShouldContain("Value=\"#E6F7FF\"");
        semanticSource.ShouldContain("Property=\"BorderBrush\"");
        semanticSource.ShouldContain("Value=\"#E0000000\"");
        semanticSource.ShouldContain("Value=\"#1890FF\"");
        semanticSource.ShouldContain("Property=\"BorderThickness\"");
        semanticSource.ShouldContain("Property=\"BorderDashArray\"");
        semanticSource.ShouldContain("Value=\"4,2\"");
        semanticSource.ShouldContain("Property=\"Foreground\"");
        semanticSource.ShouldContain("Value=\"#FF0000\"");
        semanticSource.ShouldContain("Property=\"FontWeight\"");
        CountOccurrences(semanticSource, "<atom:SpaceItemStyle ").ShouldBe(1);
        CountOccurrences(semanticSource, "<atom:SpaceSeparatorStyle ").ShouldBe(1);
        semanticSource.ShouldContain("x:SetterTargetType=\"atom:Button\"");
        semanticSource.ShouldContain("x:SetterTargetType=\"atom:TextBlock\"");
        CountOccurrences(semanticSource, "<atom:Space ").ShouldBe(2);
        CountOccurrences(semanticSource, "Orientation=\"Horizontal\"").ShouldBe(2);
        CountOccurrences(semanticSource, "HorizontalAlignment=\"Left\"").ShouldBe(2);
        CountOccurrences(semanticSource, "SizeType=\"Large\"").ShouldBe(1);
        CountOccurrences(semanticSource, "Text=\"•\"").ShouldBe(1);
        CountOccurrences(semanticSource, "Classes=\"space-surface\"").ShouldBe(1);
        semanticSource.ShouldContain("SpaceShowCaseLangResource P2StyledButtonN1");
        semanticSource.ShouldContain("SpaceShowCaseLangResource P2LargeSpaceButtonN1");
        semanticSource.ShouldNotContain("/template/");
        semanticSource.ShouldNotContain("semantic dom", Case.Insensitive);
        english.ShouldContain("Custom Semantic Part styling");
        english.ShouldContain(
            "Mirrors the Ant Design style-class demo: customize the root frame, item background and separator color through generated Semantic Part styles, and outline the root with a dashed border.");
        english.ShouldNotContain("semantic dom", Case.Insensitive);
    }

    [Fact]
    public void Space_Semantic_Style_Example_Applies_The_Official_Style_Values()
    {
        AvaloniaTestApp.EnsureInitialized();

        var page = new SpaceShowCase
        {
            DataContext = new SpaceViewModel(new TestScreen())
        };

        ShowInWindow(page, 1280, 900, () =>
        {
            var panel = page.GetVisualDescendants().OfType<ShowCasePanel>().Single();
            var item = panel.Children
                            .OfType<ShowCaseItem>()
                            .Single(static candidate => candidate.SourceKey == "space-semantic-part");
            item.MaterializeDeferredContent();
            Dispatcher.UIThread.RunJobs();

            var demos = page.GetVisualDescendants()
                            .OfType<AtomUISpace>()
                            .Where(static space => space.Classes.Contains("semantic-style-demo-object") ||
                                                  space.Classes.Contains("semantic-style-demo-function"))
                            .ToArray();
            demos.Length.ShouldBe(2);

            var objectDemo = demos.Single(static space => space.Classes.Contains("semantic-style-demo-object"));
            AssertSolidColor(objectDemo.BorderBrush, "#E0000000");
            objectDemo.BorderThickness.ShouldBe(new Thickness(2));
            objectDemo.BorderDashArray.ShouldNotBeNull();
            objectDemo.BorderDashArray.ShouldBe(new double[] { 4, 2 }, ignoreOrder: false);
            objectDemo.Padding.ShouldBe(new Thickness(8));

            var objectItems = objectDemo.GetVisualDescendants()
                                        .OfType<AtomUIButton>()
                                        .Where(static button => button.Classes.Contains("semantic-item"))
                                        .ToArray();
            objectItems.Length.ShouldBe(3);
            foreach (var button in objectItems)
            {
                AssertSolidColor(button.Background, "#F0F0F0");
                button.Padding.ShouldBe(new Thickness(4));
            }

            var objectSeparators = objectDemo.GetVisualDescendants()
                                             .OfType<AvaloniaTextBlock>()
                                             .Where(static separator => separator.Classes.Contains("semantic-separator"))
                                             .ToArray();
            objectSeparators.Length.ShouldBe(2);
            foreach (var separator in objectSeparators)
            {
                AssertSolidColor(separator.Foreground, "#FF0000");
                separator.FontWeight.ShouldBe(FontWeight.Bold);
            }

            var functionDemo = demos.Single(static space => space.Classes.Contains("semantic-style-demo-function"));
            AssertSolidColor(functionDemo.Background, "#E6F7FF");
            AssertSolidColor(functionDemo.BorderBrush, "#1890FF");
            functionDemo.Padding.ShouldBe(new Thickness(8));
            functionDemo.SizeType.ShouldBe(AtomUI.CustomizableSizeType.Large);
            functionDemo.BorderThickness.ShouldBe(new Thickness(0));
        });
    }

    private static void AssertSolidColor(IBrush? actual, string expected)
    {
        actual.ShouldNotBeNull()
              .ShouldBeAssignableTo<ISolidColorBrush>()
              .Color.ShouldBe(Color.Parse(expected));
    }

    private static string ExtractSpaceExampleItems(string source)
    {
        const string firstItemMarker  = "<gallery:ShowCaseItem";
        const string panelCloseMarker = "</gallery:ShowCasePanel>";

        var firstItemStart = source.IndexOf(firstItemMarker, StringComparison.Ordinal);
        firstItemStart.ShouldBeGreaterThanOrEqualTo(0);

        var panelCloseStart = source.IndexOf(panelCloseMarker, firstItemStart, StringComparison.Ordinal);
        panelCloseStart.ShouldBeGreaterThan(firstItemStart);

        return source[firstItemStart..panelCloseStart];
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

    private static string ComputeSha256(string source)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(source));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    private static string ReadSnapshotHash(string source)
    {
        return source
            .Split('\n')
            .First(line => line.StartsWith("sha256:", StringComparison.Ordinal))
            .Split(':', 2)[1]
            .Trim();
    }

    private static int ReadSnapshotCount(string source)
    {
        return int.Parse(source
            .Split('\n')
            .First(line => line.StartsWith("count:", StringComparison.Ordinal))
            .Split(':', 2)[1]
            .Trim());
    }

    private static int CountShowCaseItemElements(string source)
    {
        return Regex.Matches(source, @"<gallery:ShowCaseItem(\s|>)", RegexOptions.CultureInvariant).Count;
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
        var repoRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../"));
        return Path.Combine(repoRoot, relativePath);
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
