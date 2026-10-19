using AtomUI.Toolkits.GalleryBase.Controls;
using AtomUIGallery.ShowCases.Empty;
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

public class EmptyShowCasePageTests
{

    [Fact]
    public void Empty_ShowCase_Declares_A_Deferred_Semantic_Part_Preview_And_Style_Example()
    {
        var source = ReadRepoFile(
            "controlgallery/AtomUIGallery/ShowCases/DataDisplay/Empty/Views/EmptyShowCase.axaml");
        var english = ReadRepoFile(
            "controlgallery/AtomUIGallery/ShowCases/DataDisplay/Empty/Localization/en-US.xlf");
        var semanticSource = ExtractShowCaseItem(source, "empty-semantic-part");

        source.ShouldContain("<gallery:GalleryShowCaseHost.SemanticPartsContentTemplate>");
        source.ShouldContain("Name=\"EmptySemanticPreview\"");
        source.ShouldContain("SemanticOwnerType=\"{x:Type atom:Empty}\"");
        source.ShouldContain("Name=\"EmptySemanticOwner\"");
        CountOccurrences(source, "<gallery:SemanticPartDescription").ShouldBe(4);
        foreach (var path in new[] { "root", "image", "description", "footer" })
        {
            source.ShouldContain($"Path=\"{path}\"");
        }

        semanticSource.ShouldContain("SourceKey=\"empty-semantic-part\"");
        semanticSource.ShouldContain("EmptyShowCaseLangResource SemanticPartStyleTitle");
        semanticSource.ShouldContain("EmptyShowCaseLangResource SemanticPartStyleDescription");
        semanticSource.ShouldContain("Orientation=\"Vertical\"");
        semanticSource.ShouldContain("Spacing=\"16\"");
        semanticSource.ShouldContain("EmptyShowCaseLangResource P2DescriptionObjectStyles");
        semanticSource.ShouldContain("EmptyShowCaseLangResource P2DescriptionFunctionStyles");
        semanticSource.ShouldContain("<Setter Property=\"StrokeDashArray\" Value=\"4,2\" />");
        semanticSource.ShouldContain("<Setter Property=\"Background\" Value=\"#F5F5F5\" />");
        semanticSource.ShouldContain("<Setter Property=\"CornerRadius\" Value=\"8\" />");
        semanticSource.ShouldContain("<Setter Property=\"Background\" Value=\"#E6F7FF\" />");
        semanticSource.ShouldContain("<Setter Property=\"BorderBrush\" Value=\"#91D5FF\" />");
        semanticSource.ShouldContain("<Setter Property=\"Foreground\" Value=\"#1890FF\" />");
        semanticSource.ShouldContain("<Setter Property=\"FontWeight\" Value=\"Bold\" />");
        semanticSource.ShouldContain("<Setter Property=\"Margin\" Value=\"0,16,0,0\" />");
        source.ShouldContain("<atom:EmptyImageStyle x:SetterTargetType=\"Control\">");
        semanticSource.ShouldContain("<atom:EmptyDescriptionStyle x:SetterTargetType=\"atom:TextBlock\">");
        semanticSource.ShouldContain("<atom:EmptyFooterStyle x:SetterTargetType=\"ContentPresenter\">");
        semanticSource.ShouldContain("<atom:Empty.Footer>");
        semanticSource.ShouldNotContain("Orientation=\"Horizontal\"");
        semanticSource.ShouldNotContain("Spacing=\"48\"");
        semanticSource.ShouldNotContain("Value=\"#696FC7\"");
        semanticSource.ShouldNotContain("<Setter Property=\"Height\" Value=\"72\" />");
        semanticSource.ShouldNotContain("atom|Empty.semantic-demo /template/ .semantic-image");
        semanticSource.ShouldNotContain("atom|Empty.semantic-demo /template/ .semantic-description");
        semanticSource.ShouldNotContain("atom|Empty.semantic-demo /template/ .semantic-footer");
        semanticSource.ShouldNotContain("/template/ .semantic-");
        english.ShouldContain("Custom Semantic Part styling");
        english.ShouldContain(
            "Use owner-scoped styles to customize Empty's published Semantic Parts.");
        semanticSource.ShouldNotContain("semantic dom", Case.Insensitive);
        semanticSource.ShouldNotContain("classNames", Case.Insensitive);
        english.ShouldNotContain("semantic dom", Case.Insensitive);
        english.ShouldNotContain("classNames", Case.Insensitive);
    }

    [Fact]
    public void Empty_Semantic_Preview_Is_Materialized_Only_After_The_Tab_Is_Selected()
    {
        AvaloniaTestApp.EnsureInitialized();

        var page = new EmptyShowCase
        {
            DataContext = new EmptyViewModel(new TestScreen())
        };

        ShowInWindow(page, 1280, 900, () =>
        {
            page.GetVisualDescendants().OfType<SemanticPartPreview>().ShouldBeEmpty();
            page.GetVisualDescendants()
                .OfType<AtomUI.Desktop.Controls.Empty>()
                .ShouldNotContain(static empty => empty.Name == "EmptySemanticOwner");

            var host = page.GetVisualDescendants().OfType<GalleryShowCaseHost>().Single();
            host.SelectedTab = GalleryShowCaseTab.SemanticParts;
            Dispatcher.UIThread.RunJobs();

            page.GetVisualDescendants().OfType<SemanticPartPreview>().ShouldHaveSingleItem();
            page.GetVisualDescendants()
                .OfType<AtomUI.Desktop.Controls.Empty>()
                .Count(static empty => empty.Name == "EmptySemanticOwner")
                .ShouldBe(1);

            host.SelectedTab = GalleryShowCaseTab.Examples;
            Dispatcher.UIThread.RunJobs();
            Assert.All(page.GetVisualDescendants().OfType<SemanticPartPreview>(), preview => Assert.False(preview.IsEffectivelyVisible));
        });
    }

    private static string ExtractEmptyExampleItems(string source)
    {
        const string firstItemMarker  = "<gallery:ShowCaseItem";

        var firstItemStart = source.IndexOf(firstItemMarker, StringComparison.Ordinal);
        firstItemStart.ShouldBeGreaterThanOrEqualTo(0);

        const string semanticItemMarker = "SourceKey=\"empty-semantic-part\"";
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
