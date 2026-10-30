using AtomUI.Toolkits.GalleryBase.Controls;
using AtomUIGallery.ShowCases.Statistic;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Threading;
using Avalonia.VisualTree;
using ReactiveUI;
using Shouldly;
using Xunit;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUIGallery.Tests.ShowCases;

public class StatisticShowCasePageTests
{

    [Fact]
    public void Statistic_ShowCase_Declares_The_Official_Semantic_Preview_And_Style_Example()
    {
        var source = ReadRepoFile(
            "controlgallery/AtomUIGallery/ShowCases/DataDisplay/Statistic/Views/StatisticShowCase.axaml");
        var english = ReadRepoFile(
            "controlgallery/AtomUIGallery/ShowCases/DataDisplay/Statistic/Localization/en-US.xlf");
        var semanticSource = ExtractShowCaseItem(source, "statistic-semantic-part");

        source.ShouldContain("<gallery:GalleryShowCaseHost.SemanticPartsContentTemplate>");
        source.ShouldContain("Name=\"StatisticSemanticPreview\"");
        source.ShouldContain("SemanticOwnerType=\"{x:Type atom:Statistic}\"");
        source.ShouldContain("Name=\"StatisticSemanticOwner\"");
        CountOccurrences(source, "<gallery:SemanticPartDescription").ShouldBe(7);
        foreach (var path in new[] { "root", "header", "title", "content", "value", "prefix", "suffix" })
        {
            source.ShouldContain($"Path=\"{path}\"");
        }

        semanticSource.ShouldContain("SourceKey=\"statistic-semantic-part\"");
        semanticSource.ShouldContain("StatisticShowCaseLangResource SemanticPartStyleTitle");
        semanticSource.ShouldContain("StatisticShowCaseLangResource SemanticPartStyleDescription");
        semanticSource.ShouldContain("Orientation=\"Vertical\"");
        semanticSource.ShouldContain("Spacing=\"16\"");
        semanticSource.ShouldContain("<Setter Property=\"BorderBrush\" Value=\"#CCCCCC\" />");
        semanticSource.ShouldContain("<Setter Property=\"BorderThickness\" Value=\"2\" />");
        semanticSource.ShouldContain("<Setter Property=\"Padding\" Value=\"16\" />");
        semanticSource.ShouldContain("<Setter Property=\"CornerRadius\" Value=\"8\" />");
        semanticSource.ShouldContain("<atom:StatisticTitleStyle x:SetterTargetType=\"ContentPresenter\">");
        semanticSource.ShouldContain("<atom:StatisticContentStyle x:SetterTargetType=\"StackPanel\">");
        semanticSource.ShouldContain("<Setter Property=\"StrokeDashArray\" Value=\"4,2\" />");
        semanticSource.ShouldContain("<atom:StatisticValueStyle x:SetterTargetType=\"ContentPresenter\">");
        semanticSource.ShouldContain("Header=\"Monthly Active Users\"");
        semanticSource.ShouldContain("Value=\"93241\"");
        semanticSource.ShouldContain("ValueSuffixAddOn=\"users\"");
        semanticSource.ShouldContain("Header=\"Yearly Loss\"");
        semanticSource.ShouldContain("Value=\"-18.7\"");
        semanticSource.ShouldContain("Precision=\"1\"");
        semanticSource.ShouldContain("ValueSuffixAddOn=\"%\"");
        semanticSource.ShouldContain("Value=\"#1890FF\"");
        semanticSource.ShouldContain("Value=\"#0958D9\"");
        semanticSource.ShouldContain("Value=\"#E6F4FF\"");
        semanticSource.ShouldContain("Value=\"#FF4D4F\"");
        semanticSource.ShouldContain("Value=\"#FF7875\"");
        semanticSource.ShouldContain("Value=\"#FFF1F0\"");
        semanticSource.ShouldNotContain("semantic dom", Case.Insensitive);
        semanticSource.ShouldNotContain("classNames");
        semanticSource.ShouldNotContain("objects/functions");
        semanticSource.ShouldNotContain("/template/ .semantic-");
        english.ShouldContain("Custom Semantic Part styling");
        english.ShouldContain(
            "Use owner-scoped style selectors to customize Statistic's published Semantic Parts.");
        english.ShouldNotContain("Custom semantic dom styling", Case.Insensitive);
        english.ShouldNotContain("classNames");
        english.ShouldNotContain("objects/functions");
    }

    [Fact]
    public void Statistic_Semantic_Preview_Is_Materialized_Only_After_The_Tab_Is_Selected()
    {
        AvaloniaTestApp.EnsureInitialized();

        var page = new StatisticShowCase
        {
            DataContext = new StatisticViewModel(new TestScreen())
        };

        ShowInWindow(page, 1280, 900, () =>
        {
            page.GetVisualDescendants().OfType<SemanticPartPreview>().ShouldBeEmpty();
            page.GetVisualDescendants()
                .OfType<AtomUI.Desktop.Controls.Statistic>()
                .ShouldNotContain(static statistic => statistic.Name == "StatisticSemanticOwner");

            var host = page.GetVisualDescendants().OfType<GalleryShowCaseHost>().Single();
            host.SelectedTab = GalleryShowCaseTab.SemanticParts;
            Dispatcher.UIThread.RunJobs();

            page.GetVisualDescendants().OfType<SemanticPartPreview>().ShouldHaveSingleItem();
            page.GetVisualDescendants()
                .OfType<AtomUI.Desktop.Controls.Statistic>()
                .Count(static statistic => statistic.Name == "StatisticSemanticOwner")
                .ShouldBe(1);

            host.SelectedTab = GalleryShowCaseTab.Examples;
            Dispatcher.UIThread.RunJobs();
            Assert.All(page.GetVisualDescendants().OfType<SemanticPartPreview>(), preview => Assert.False(preview.IsEffectivelyVisible));
        });
    }

    private static string ExtractStatisticExampleItems(string source)
    {
        const string firstItemMarker  = "<gallery:ShowCaseItem";
        var firstItemStart = source.IndexOf(firstItemMarker, StringComparison.Ordinal);
        firstItemStart.ShouldBeGreaterThanOrEqualTo(0);

        const string semanticItemMarker = "SourceKey=\"statistic-semantic-part\"";
        var semanticItemStart = source.IndexOf(semanticItemMarker, firstItemStart, StringComparison.Ordinal);
        semanticItemStart.ShouldBeGreaterThan(firstItemStart);
        var panelCloseStart = source.LastIndexOf(firstItemMarker, semanticItemStart, StringComparison.Ordinal);
        panelCloseStart.ShouldBeGreaterThan(firstItemStart);

        return source[firstItemStart..panelCloseStart];
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
