using AtomUI.Toolkits.GalleryBase.Controls;
using AtomUIGallery.ShowCases.Alert;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Threading;
using Avalonia.VisualTree;
using ReactiveUI;
using Shouldly;
using Xunit;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUIGallery.Tests.ShowCases;

public class AlertShowCasePageTests
{

    [Fact]
    public void Alert_ShowCase_Declares_The_Official_Semantic_Preview_And_Style_Example()
    {
        var source = ReadRepoFile(
            "controlgallery/AtomUIGallery/ShowCases/Feedback/Alert/Views/AlertShowCase.axaml");
        var english = ReadRepoFile(
            "controlgallery/AtomUIGallery/ShowCases/Feedback/Alert/Localization/en-US.xlf");
        var semanticSource = ExtractShowCaseItem(source, "alert-semantic-part");

        source.ShouldContain("<gallery:GalleryShowCaseHost.SemanticPartsContentTemplate>");
        source.ShouldContain("Name=\"AlertSemanticPreview\"");
        source.ShouldContain("SemanticOwnerType=\"{x:Type atom:Alert}\"");
        source.ShouldContain("Name=\"AlertSemanticOwner\"");
        source.ShouldContain("Message=\"Info Text\"");
        source.ShouldContain("Description=\"Info Description Info Description Info Description Info Description\"");
        source.ShouldContain("Content=\"Accept\"");
        source.ShouldContain("Content=\"Decline\"");
        source.ShouldNotContain("<gallery:SemanticPartPreview.Styles>");
        CountOccurrences(source, "<gallery:SemanticPartDescription").ShouldBe(7);
        foreach (var path in new[] { "root", "icon", "section", "title", "description", "actions", "close" })
        {
            source.ShouldContain($"Path=\"{path}\"");
        }

        semanticSource.ShouldContain("SourceKey=\"alert-semantic-part\"");
        semanticSource.ShouldContain("AlertShowCaseLangResource SemanticPartStyleTitle");
        semanticSource.ShouldContain("AlertShowCaseLangResource SemanticPartStyleDescription");
        semanticSource.ShouldContain("Orientation=\"Vertical\"");
        semanticSource.ShouldContain("Spacing=\"16\"");
        semanticSource.ShouldContain("<Setter Property=\"BorderBrush\" Value=\"#CCCCCC\" />");
        semanticSource.ShouldContain("<Setter Property=\"BorderThickness\" Value=\"2\" />");
        semanticSource.ShouldContain("<Setter Property=\"StrokeDashArray\" Value=\"4,2\" />");
        semanticSource.ShouldContain("<Setter Property=\"Padding\" Value=\"12\" />");
        semanticSource.ShouldContain("<Setter Property=\"CornerRadius\" Value=\"8\" />");
        semanticSource.ShouldContain("<atom:AlertIconStyle x:SetterTargetType=\"atom:Icon\"");
        semanticSource.ShouldContain("<atom:AlertSectionStyle x:SetterTargetType=\"StackPanel\"");
        semanticSource.ShouldContain("<Setter Property=\"Width\" Value=\"18\" />");
        semanticSource.ShouldContain("<Setter Property=\"Height\" Value=\"18\" />");
        semanticSource.ShouldContain("<Setter Property=\"TextElement.FontWeight\" Value=\"Medium\" />");
        semanticSource.ShouldContain("<Setter Property=\"Background\" Value=\"#1A52C41A\" />");
        semanticSource.ShouldContain("<Setter Property=\"BorderBrush\" Value=\"#B7EB8F\" />");
        semanticSource.ShouldContain("<Setter Property=\"FillBrush\" Value=\"#52C41A\" />");
        semanticSource.ShouldContain("Name=\"AlertObjectSemanticDemo\"");
        semanticSource.ShouldContain("Name=\"AlertFunctionSemanticDemo\"");
        semanticSource.ShouldContain("Message=\"Object styles\"");
        semanticSource.ShouldContain("Message=\"Function styles\"");
        semanticSource.ShouldContain("Content=\"Action\"");
        semanticSource.ShouldNotContain("Release completed");
        semanticSource.ShouldNotContain("Review required");
        semanticSource.ShouldNotContain("/template/ .semantic-");
        english.ShouldContain("Custom Semantic Part styling");
        english.ShouldContain(
            "Use owner-scoped styles to customize Alert's published Semantic Parts.");
        semanticSource.ShouldNotContain("semantic dom", Case.Insensitive);
        semanticSource.ShouldNotContain("classNames", Case.Insensitive);
        english.ShouldNotContain("semantic dom", Case.Insensitive);
        english.ShouldNotContain("classNames", Case.Insensitive);
    }

    [Fact]
    public void Alert_Semantic_Preview_Is_Materialized_Only_After_The_Tab_Is_Selected()
    {
        AvaloniaTestApp.EnsureInitialized();

        var page = new AlertShowCase
        {
            DataContext = new AlertViewModel(new TestScreen())
        };

        ShowInWindow(page, 1280, 900, () =>
        {
            page.GetVisualDescendants().OfType<SemanticPartPreview>().ShouldBeEmpty();
            page.GetVisualDescendants()
                .OfType<AtomUI.Desktop.Controls.Alert>()
                .ShouldNotContain(static alert => alert.Name == "AlertSemanticOwner");

            var host = page.GetVisualDescendants().OfType<GalleryShowCaseHost>().Single();
            host.SelectedTab = GalleryShowCaseTab.SemanticParts;
            Dispatcher.UIThread.RunJobs();

            page.GetVisualDescendants().OfType<SemanticPartPreview>().ShouldHaveSingleItem();
            page.GetVisualDescendants()
                .OfType<AtomUI.Desktop.Controls.Alert>()
                .Count(static alert => alert.Name == "AlertSemanticOwner")
                .ShouldBe(1);

            host.SelectedTab = GalleryShowCaseTab.Examples;
            Dispatcher.UIThread.RunJobs();
            Assert.All(page.GetVisualDescendants().OfType<SemanticPartPreview>(), preview => Assert.False(preview.IsEffectivelyVisible));
        });
    }

    private static string ExtractAlertExampleItems(string source)
    {
        const string firstItemMarker  = "<gallery:ShowCaseItem";
        var firstItemStart = source.IndexOf(firstItemMarker, StringComparison.Ordinal);
        firstItemStart.ShouldBeGreaterThanOrEqualTo(0);

        const string semanticItemMarker = "SourceKey=\"alert-semantic-part\"";
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
