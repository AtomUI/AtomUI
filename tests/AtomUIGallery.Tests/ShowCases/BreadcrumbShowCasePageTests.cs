using AtomUI.Toolkits.GalleryBase.Controls;
using AtomUIGallery.ShowCases.Breadcrumb;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using ReactiveUI;
using Shouldly;
using Xunit;
using AtomUIBreadcrumb = AtomUI.Desktop.Controls.Breadcrumb;
using AtomUIBreadcrumbItem = AtomUI.Desktop.Controls.BreadcrumbItem;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUIGallery.Tests.ShowCases;

public class BreadcrumbShowCasePageTests
{

    [Fact]
    public void Breadcrumb_ShowCase_Declares_A_Deferred_Semantic_Part_Preview()
    {
        var source = ReadRepoFile("controlgallery/AtomUIGallery/ShowCases/Navigation/Breadcrumb/Views/BreadcrumbShowCase.axaml");

        source.ShouldContain("<gallery:GalleryShowCaseHost.SemanticPartsContentTemplate>");
        source.ShouldContain("<gallery:SemanticPartPreview");
        source.ShouldContain("SemanticOwner=\"{Binding #BreadcrumbSemanticOwner}\"");
        source.ShouldContain("SemanticOwnerType=\"{x:Type atom:Breadcrumb}\"");
        // 预览画布默认居中内容（SemanticPartPreview.PreviewContentAlignment=Center），
        // 面包屑宿主自身再以对齐属性显式居中。
        source.ShouldContain("HorizontalAlignment=\"Center\"");
        source.ShouldContain("VerticalAlignment=\"Center\"");
        source.ShouldContain("Kind=HomeOutlined");
        source.ShouldContain("Kind=UserOutlined");
        source.ShouldContain("SourceKey=\"breadcrumb-semantic-part\"");
        CountOccurrences(source, "<gallery:SemanticPartDescription").ShouldBe(3);
        source.ShouldContain("Path=\"root\"");
        source.ShouldContain("Path=\"item\"");
        source.ShouldContain("Path=\"separator\"");
    }

    [Fact]
    public void Breadcrumb_Semantic_Preview_Is_Materialized_Only_After_The_Tab_Is_Selected()
    {
        AvaloniaTestApp.EnsureInitialized();

        var page = new BreadcrumbShowCase
        {
            DataContext = new BreadcrumbViewModel(new TestScreen())
        };

        ShowInWindow(page, 1280, 800, () =>
        {
            page.GetVisualDescendants().OfType<SemanticPartPreview>().ShouldBeEmpty();
            page.GetVisualDescendants()
                .OfType<AtomUIBreadcrumb>()
                .ShouldNotContain(static breadcrumb => breadcrumb.Name == "BreadcrumbSemanticOwner");

            var host = page.GetVisualDescendants().OfType<GalleryShowCaseHost>().Single();
            host.SelectedTab = GalleryShowCaseTab.SemanticParts;
            Dispatcher.UIThread.RunJobs();

            page.GetVisualDescendants().OfType<SemanticPartPreview>().Count().ShouldBe(1);
            page.GetVisualDescendants()
                .OfType<AtomUIBreadcrumb>()
                .Count(static breadcrumb => breadcrumb.Name == "BreadcrumbSemanticOwner")
                .ShouldBe(1);
        });
    }

    [Fact]
    public void Breadcrumb_Semantic_Style_Example_Applies_The_Official_Style_Values()
    {
        AvaloniaTestApp.EnsureInitialized();

        var page = new BreadcrumbShowCase
        {
            DataContext = new BreadcrumbViewModel(new TestScreen())
        };

        ShowInWindow(page, 1280, 900, () =>
        {
            var panel = page.GetVisualDescendants().OfType<ShowCasePanel>().Single();
            var item = panel.Children
                            .OfType<ShowCaseItem>()
                            .Single(static candidate => candidate.SourceKey == "breadcrumb-semantic-part");
            item.MaterializeDeferredContent();
            Dispatcher.UIThread.RunJobs();

            var demos = page.GetVisualDescendants()
                            .OfType<AtomUIBreadcrumb>()
                            .Where(static breadcrumb => breadcrumb.Classes.Contains("semantic-style-demo-object") ||
                                                        breadcrumb.Classes.Contains("semantic-style-demo-function"))
                            .ToArray();
            demos.Length.ShouldBe(2);

            var objectDemo = demos.Single(static breadcrumb => breadcrumb.Classes.Contains("semantic-style-demo-object"));
            AssertSolidColor(objectDemo.BorderBrush, "#F0F0F0");
            objectDemo.BorderThickness.ShouldBe(new Thickness(1));
            objectDemo.CornerRadius.ShouldBe(new CornerRadius(4));
            objectDemo.Padding.ShouldBe(new Thickness(8));

            var objectItems = objectDemo.GetVisualDescendants()
                                        .OfType<AtomUIBreadcrumbItem>()
                                        .Where(static candidate => candidate.Classes.Contains("semantic-item"))
                                        .ToArray();
            objectItems.Length.ShouldBe(2);
            // The plain item takes the object demo item color; the linked item keeps the token
            // link color on its content presenter (Ant Design `.ant-breadcrumb-item a` semantics).
            AssertContentForeground(objectItems[0], "#1890FF");
            AssertContentForeground(objectItems[1], "#72000000");

            var objectSeparators = objectDemo.GetVisualDescendants()
                                             .OfType<ContentPresenter>()
                                             .Where(static candidate => candidate.Classes.Contains("semantic-separator"))
                                             .ToArray();
            objectSeparators.Length.ShouldBe(1);
            foreach (var separator in objectSeparators)
            {
                AssertSolidColor(separator.Foreground, "#73000000");
            }

            var functionDemo = demos.Single(static breadcrumb => breadcrumb.Classes.Contains("semantic-style-demo-function"));
            AssertSolidColor(functionDemo.BorderBrush, "#F5EFFF");
            functionDemo.BorderThickness.ShouldBe(new Thickness(1));
            functionDemo.CornerRadius.ShouldBe(new CornerRadius(4));
            functionDemo.Padding.ShouldBe(new Thickness(8));

            var functionItems = functionDemo.GetVisualDescendants()
                                            .OfType<AtomUIBreadcrumbItem>()
                                            .Where(static candidate => candidate.Classes.Contains("semantic-item"))
                                            .ToArray();
            functionItems.Length.ShouldBe(3);
            AssertContentForeground(functionItems[0], "#8F87F1");
            AssertContentForeground(functionItems[1], "#72000000");
            AssertContentForeground(functionItems[2], "#8F87F1");

            var functionSeparators = functionDemo.GetVisualDescendants()
                                                 .OfType<ContentPresenter>()
                                                 .Where(static candidate => candidate.Classes.Contains("semantic-separator"))
                                                 .ToArray();
            functionSeparators.Length.ShouldBe(2);
            foreach (var separator in functionSeparators)
            {
                AssertSolidColor(separator.Foreground, "#73000000");
            }
        });
    }

    private static void AssertSolidColor(IBrush? actual, string expected)
    {
        actual.ShouldNotBeNull()
              .ShouldBeAssignableTo<ISolidColorBrush>()
              .Color.ShouldBe(Color.Parse(expected));
    }

    private static void AssertContentForeground(AtomUIBreadcrumbItem item, string expected)
    {
        var content = item.GetVisualDescendants()
                          .OfType<ContentPresenter>()
                          .Single(static presenter => presenter.Name == "Content");
        AssertSolidColor(content.Foreground, expected);
    }

    private static string ExtractBreadcrumbExampleItems(string source)
    {
        const string firstItemMarker  = "<gallery:ShowCaseItem";
        const string panelCloseMarker = "</gallery:ShowCasePanel>";

        var firstItemStart = source.IndexOf(firstItemMarker, StringComparison.Ordinal);
        firstItemStart.ShouldBeGreaterThanOrEqualTo(0);

        var panelCloseStart = source.IndexOf(panelCloseMarker, firstItemStart, StringComparison.Ordinal);
        panelCloseStart.ShouldBeGreaterThan(firstItemStart);

        return source[firstItemStart..panelCloseStart];
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

    private static void ShowInWindow(Control content, int width, int height, Action assertion)
    {
        var window = new AvaloniaWindow
        {
            Width   = width,
            Height  = height,
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

    private sealed class TestScreen : IScreen
    {
        public RoutingState Router { get; } = new();
    }
}
