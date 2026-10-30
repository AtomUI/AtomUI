using AtomUI.Toolkits.GalleryBase.Controls;
using AtomUIGallery.ShowCases.QRCode;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Threading;
using Avalonia.VisualTree;
using ReactiveUI;
using Shouldly;
using Xunit;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUIGallery.Tests.ShowCases;

public class QRCodeShowCasePageTests
{

    [Fact]
    public void QRCode_ShowCase_Declares_The_Official_Deferred_Semantic_Preview_And_Style_Example()
    {
        var source = ReadRepoFile(
            "controlgallery/AtomUIGallery/ShowCases/DataDisplay/QRCode/Views/QRCodeShowCase.axaml");
        var semanticSource = ExtractShowCaseItem(source, "qr-code-semantic-part");

        source.ShouldContain("<gallery:GalleryShowCaseHost.SemanticPartsContentTemplate>");
        source.ShouldContain("Name=\"QRCodeSemanticPreview\"");
        source.ShouldContain("SemanticOwnerType=\"{x:Type atom:QRCode}\"");
        source.ShouldContain("Name=\"QRCodeSemanticOwner\"");
        source.ShouldContain("Value=\"https://ant.design\"");
        source.ShouldContain("Status=\"Loading\"");
        CountOccurrences(source, "<gallery:SemanticPartDescription").ShouldBe(2);
        source.ShouldContain("Path=\"root\"");
        source.ShouldContain("Path=\"cover\"");

        semanticSource.ShouldContain("SourceKey=\"qr-code-semantic-part\"");
        semanticSource.ShouldContain("QRCodeShowCaseLangResource SemanticPartStyleTitle");
        semanticSource.ShouldContain("QRCodeShowCaseLangResource SemanticPartStyleDescription");
        semanticSource.ShouldContain("Orientation=\"Horizontal\"");
        semanticSource.ShouldContain("Spacing=\"16\"");
        CountOccurrences(semanticSource, "Value=\"https://ant.design/\"").ShouldBe(2);
        CountOccurrences(semanticSource, "Size=\"160\"").ShouldBe(2);
        semanticSource.ShouldContain("Classes=\"semantic-style-class semantic-object-styles\"");
        semanticSource.ShouldContain("Classes=\"semantic-style-class semantic-function-styles\"");
        semanticSource.ShouldContain(
            "Icon=\"{SvgImage avares://AtomUIGallery/Assets/AvatarShowCase/AntDesign.svg}\"");
        semanticSource.ShouldContain("<Setter Property=\"BorderBrush\" Value=\"#1890FF\" />");
        semanticSource.ShouldContain("<Setter Property=\"BorderBrush\" Value=\"#FF4D4F\" />");
        semanticSource.ShouldContain("<Setter Property=\"BorderThickness\" Value=\"2\" />");
        semanticSource.ShouldContain("<Setter Property=\"CornerRadius\" Value=\"8\" />");
        semanticSource.ShouldContain("<Setter Property=\"Padding\" Value=\"16\" />");
        semanticSource.ShouldContain("<Setter Property=\"Background\" Value=\"#1A1890FF\" />");
        semanticSource.ShouldContain("<Setter Property=\"Background\" Value=\"#1AFF4D4F\" />");
        semanticSource.ShouldNotContain("ATOMUI-LOGO");
        semanticSource.ShouldNotContain("/template/ .semantic-");
        semanticSource.ShouldNotContain("QRCodeCoverStyle");
    }

    [Fact]
    public void QRCode_Semantic_Preview_Is_Materialized_Only_After_The_Tab_Is_Selected()
    {
        AvaloniaTestApp.EnsureInitialized();

        var page = new QRCodeShowCase
        {
            DataContext = new QRCodeViewModel(new TestScreen())
        };

        ShowInWindow(page, 1280, 900, () =>
        {
            page.GetVisualDescendants().OfType<SemanticPartPreview>().ShouldBeEmpty();
            page.GetVisualDescendants()
                .OfType<AtomUI.Desktop.Controls.QRCode>()
                .ShouldNotContain(static qrCode => qrCode.Name == "QRCodeSemanticOwner");

            var host = page.GetVisualDescendants().OfType<GalleryShowCaseHost>().Single();
            host.SelectedTab = GalleryShowCaseTab.SemanticParts;
            Dispatcher.UIThread.RunJobs();

            page.GetVisualDescendants().OfType<SemanticPartPreview>().ShouldHaveSingleItem();
            page.GetVisualDescendants()
                .OfType<AtomUI.Desktop.Controls.QRCode>()
                .Count(static qrCode => qrCode.Name == "QRCodeSemanticOwner")
                .ShouldBe(1);

            host.SelectedTab = GalleryShowCaseTab.Examples;
            Dispatcher.UIThread.RunJobs();
            Assert.All(page.GetVisualDescendants().OfType<SemanticPartPreview>(), preview => Assert.False(preview.IsEffectivelyVisible));
        });
    }

    [Fact]
    public void QRCode_Semantic_Style_Example_Materializes_The_Official_Svg_Icon()
    {
        AvaloniaTestApp.EnsureInitialized();

        var page = new QRCodeShowCase
        {
            DataContext = new QRCodeViewModel(new TestScreen())
        };

        ShowInWindow(page, 1280, 900, () =>
        {
            var item = page.GetVisualDescendants()
                           .OfType<ShowCaseItem>()
                           .Single(static item => item.SourceKey == "qr-code-semantic-part");

            Should.NotThrow(item.MaterializeDeferredContent);
            var qrCodes = item.GetVisualDescendants()
                              .OfType<AtomUI.Desktop.Controls.QRCode>()
                              .ToArray();
            qrCodes.Length.ShouldBe(2);
            qrCodes[1].Icon.ShouldNotBeNull();
        });
    }

    private static string ExtractQRCodeExampleItems(string source)
    {
        const string firstItemMarker  = "<gallery:ShowCaseItem";
        var firstItemStart = source.IndexOf(firstItemMarker, StringComparison.Ordinal);
        firstItemStart.ShouldBeGreaterThanOrEqualTo(0);

        const string semanticItemMarker = "SourceKey=\"qr-code-semantic-part\"";
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
