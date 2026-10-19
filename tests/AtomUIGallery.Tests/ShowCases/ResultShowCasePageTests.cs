using AtomUI.Toolkits.GalleryBase.Controls;
using AtomUIGallery.ShowCases.Result;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Threading;
using Avalonia.VisualTree;
using ReactiveUI;
using Shouldly;
using Xunit;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUIGallery.Tests.ShowCases;

public class ResultShowCasePageTests
{

    [Fact]
    public void Result_ShowCase_Declares_The_Ant_Design_Semantic_Preview_And_Style_Example()
    {
        var source = ReadRepoFile(
            "controlgallery/AtomUIGallery/ShowCases/Feedback/Result/Views/ResultShowCase.axaml");
        var english = ReadRepoFile(
            "controlgallery/AtomUIGallery/ShowCases/Feedback/Result/Localization/en-US.xlf");
        var semanticSource = ExtractShowCaseItem(source, "result-semantic-part");

        source.ShouldContain("<gallery:GalleryShowCaseHost.SemanticPartsContentTemplate>");
        source.ShouldContain("Name=\"ResultSemanticPreview\"");
        source.ShouldContain("SemanticOwnerType=\"{x:Type atom:Result}\"");
        source.ShouldContain("Name=\"ResultSemanticOwner\"");
        source.ShouldContain("Header=\"title\"");
        source.ShouldContain("SubHeader=\"subTitle\"");
        source.ShouldContain("Content=\"extra\"");
        source.ShouldContain("Text=\"The Content of Result\"");
        CountOccurrences(source, "<gallery:SemanticPartDescription").ShouldBe(6);
        foreach (var path in new[] { "root", "icon", "title", "subTitle", "extra", "body" })
        {
            source.ShouldContain($"Path=\"{path}\"");
        }

        semanticSource.ShouldContain("SourceKey=\"result-semantic-part\"");
        semanticSource.ShouldContain("ResultShowCaseLangResource SemanticPartStyleTitle");
        semanticSource.ShouldContain("ResultShowCaseLangResource SemanticPartStyleDescription");
        semanticSource.ShouldContain("Orientation=\"Vertical\"");
        semanticSource.ShouldContain("Spacing=\"16\"");
        semanticSource.ShouldContain("Selector=\"atom|Result.semantic-object-demo\"");
        semanticSource.ShouldContain("<Setter Property=\"BorderThickness\" Value=\"2\" />");
        semanticSource.ShouldContain("<Setter Property=\"StrokeDashArray\" Value=\"4,2\" />");
        semanticSource.ShouldContain("<Setter Property=\"Padding\" Value=\"16\" />");
        semanticSource.ShouldContain("<atom:ResultTitleStyle x:SetterTargetType=\"ContentPresenter\">");
        semanticSource.ShouldContain("<Setter Property=\"FontStyle\" Value=\"Italic\" />");
        semanticSource.ShouldContain("<Setter Property=\"Foreground\" Value=\"#1890ff\" />");
        semanticSource.ShouldContain("<atom:ResultSubTitleStyle x:SetterTargetType=\"ContentPresenter\">");
        semanticSource.ShouldContain("<Setter Property=\"FontWeight\" Value=\"Bold\" />");
        semanticSource.ShouldContain("<atom:ResultIconStyle x:SetterTargetType=\"Control\">");
        semanticSource.ShouldContain("<Setter Property=\"Opacity\" Value=\"0.8\" />");
        semanticSource.ShouldContain("<atom:ResultExtraStyle x:SetterTargetType=\"ContentPresenter\">");
        semanticSource.ShouldContain("<Setter Property=\"Background\" Value=\"#f0f0f0\" />");
        semanticSource.ShouldContain("<Setter Property=\"Padding\" Value=\"8\" />");
        semanticSource.ShouldContain("<atom:ResultBodyStyle x:SetterTargetType=\"ContentPresenter\">");
        semanticSource.ShouldContain("<Setter Property=\"Background\" Value=\"#fafafa\" />");
        semanticSource.ShouldContain("<Setter Property=\"Padding\" Value=\"12\" />");
        semanticSource.ShouldContain("Selector=\"atom|Result.semantic-function-demo\"");
        semanticSource.ShouldContain("<Setter Property=\"Background\" Value=\"#f6ffed\" />");
        semanticSource.ShouldContain("<Setter Property=\"BorderBrush\" Value=\"#52c41a\" />");
        semanticSource.ShouldContain("Selector=\"atom|Result.semantic-function-demo[Status=Error]\"");
        semanticSource.ShouldContain("<Setter Property=\"Background\" Value=\"#fff2f0\" />");
        semanticSource.ShouldContain("<Setter Property=\"BorderBrush\" Value=\"#ff4d4f\" />");
        semanticSource.ShouldContain("Name=\"ResultObjectSemanticDemo\"");
        semanticSource.ShouldContain("Status=\"Info\"");
        semanticSource.ShouldContain("Header=\"classNames Object\"");
        semanticSource.ShouldContain("SubHeader=\"This is a subtitle\"");
        semanticSource.ShouldContain("Name=\"ResultFunctionSemanticDemo\"");
        semanticSource.ShouldContain("Status=\"Success\"");
        semanticSource.ShouldContain("Header=\"classNames Function\"");
        semanticSource.ShouldContain("SubHeader=\"Dynamic class names\"");
        CountOccurrences(semanticSource, "Content=\"Action\"").ShouldBe(2);
        CountOccurrences(semanticSource, "<atom:ResultBodyStyle").ShouldBe(1);
        semanticSource.ShouldNotContain("/template/ .semantic-");
        semanticSource.ShouldNotContain("#Header");
        semanticSource.ShouldNotContain("PART_StatusIconPresenter");

        english.ShouldContain(
            "Root element with text alignment, layout styles and other basic container styles");
        english.ShouldContain(
            "Title element with font size, text color, line height, text alignment and other text styles");
        english.ShouldContain(
            "Subtitle element with font size, text color, line height and other text styles");
        english.ShouldContain(
            "Content element with margin, padding, background color and other content area styles");
        english.ShouldContain(
            "Action area element with margin, text alignment, inner element spacing and other layout styles");
        english.ShouldContain(
            "Icon element with margin, text alignment, font size, status colors and other icon styles");
        english.ShouldContain("Custom Semantic Part styling");
        english.ShouldNotContain("semantic dom", Case.Insensitive);
        english.ShouldNotContain("objects/functions through classNames", Case.Insensitive);
    }

    [Fact]
    public void Result_Semantic_Preview_Is_Materialized_Only_After_The_Tab_Is_Selected()
    {
        AvaloniaTestApp.EnsureInitialized();

        var page = new ResultShowCase
        {
            DataContext = new ResultViewModel(new TestScreen())
        };

        ShowInWindow(page, 1280, 900, () =>
        {
            page.GetVisualDescendants().OfType<SemanticPartPreview>().ShouldBeEmpty();
            page.GetVisualDescendants()
                .OfType<AtomUI.Desktop.Controls.Result>()
                .ShouldNotContain(static result => result.Name == "ResultSemanticOwner");

            var host = page.GetVisualDescendants().OfType<GalleryShowCaseHost>().Single();
            host.SelectedTab = GalleryShowCaseTab.SemanticParts;
            Dispatcher.UIThread.RunJobs();

            page.GetVisualDescendants().OfType<SemanticPartPreview>().ShouldHaveSingleItem();
            page.GetVisualDescendants()
                .OfType<AtomUI.Desktop.Controls.Result>()
                .Count(static result => result.Name == "ResultSemanticOwner")
                .ShouldBe(1);

            host.SelectedTab = GalleryShowCaseTab.Examples;
            Dispatcher.UIThread.RunJobs();
            Assert.All(page.GetVisualDescendants().OfType<SemanticPartPreview>(), preview => Assert.False(preview.IsEffectivelyVisible));
        });
    }

    private static string ExtractResultExampleItems(string source)
    {
        const string firstItemMarker  = "<gallery:ShowCaseItem";
        var firstItemStart = source.IndexOf(firstItemMarker, StringComparison.Ordinal);
        firstItemStart.ShouldBeGreaterThanOrEqualTo(0);

        const string semanticItemMarker = "SourceKey=\"result-semantic-part\"";
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
