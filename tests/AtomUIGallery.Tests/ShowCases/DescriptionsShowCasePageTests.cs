using AtomUI.Toolkits.GalleryBase.Controls;
using AtomUIGallery.ShowCases.Descriptions;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using ReactiveUI;
using Shouldly;
using Xunit;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUIGallery.Tests.ShowCases;

public class DescriptionsShowCasePageTests
{

    [Fact]
    public void Descriptions_ShowCase_Declares_A_Deferred_Semantic_Part_Preview_And_Example()
    {
        var source = ReadRepoFile(
            "controlgallery/AtomUIGallery/ShowCases/DataDisplay/Descriptions/Views/DescriptionsShowCase.axaml");

        source.ShouldContain("<gallery:GalleryShowCaseHost.SemanticPartsContentTemplate>");
        source.ShouldContain("Name=\"DescriptionsSemanticPreview\"");
        source.ShouldContain("SemanticOwnerType=\"{x:Type atom:Descriptions}\"");
        source.ShouldContain("Name=\"DescriptionsSemanticOwner\"");
        CountOccurrences(source, "<gallery:SemanticPartDescription").ShouldBe(6);
        foreach (var path in new[] { "root", "header", "title", "extra", "label", "content" })
        {
            source.ShouldContain($"Path=\"{path}\"");
        }

        source.ShouldContain("SourceKey=\"descriptions-semantic-part\"");
        source.ShouldContain("DescriptionsShowCaseLangResource SemanticPartStyleTitle");
        source.ShouldContain("DescriptionsShowCaseLangResource SemanticPartStyleDescription");
        source.ShouldContain("<atom:DescriptionsLabelStyle x:SetterTargetType=\"ContentPresenter\">");
        source.ShouldNotContain("atom|Descriptions.semantic-demo /template/ .semantic-header");
        source.ShouldNotContain("atom|Descriptions.semantic-demo /template/ .semantic-title");
        source.ShouldNotContain("atom|Descriptions.semantic-demo /template/ .semantic-extra");
        source.ShouldNotContain("atom|Descriptions.semantic-demo /template/ .semantic-scope-items > .semantic-scope-item /template/ .semantic-content");
        source.ShouldNotContain("atom|Descriptions.semantic-demo .semantic-label");
        source.ShouldNotContain("atom|Descriptions.semantic-demo .semantic-content");
        source.ShouldNotContain("ContentPresenter.semantic-");
        source.ShouldNotContain(":is(ContentPresenter)");
        source.ShouldNotContain("/template/ .semantic-");
    }

    [Fact]
    public void Descriptions_Semantic_Preview_Is_Materialized_Only_After_The_Tab_Is_Selected()
    {
        AvaloniaTestApp.EnsureInitialized();

        var page = new DescriptionsShowCase
        {
            DataContext = new DescriptionsViewModel(new TestScreen())
        };

        ShowInWindow(page, 1280, 900, () =>
        {
            page.GetVisualDescendants().OfType<SemanticPartPreview>().ShouldBeEmpty();
            page.GetVisualDescendants()
                .OfType<AtomUI.Desktop.Controls.Descriptions>()
                .ShouldNotContain(static descriptions => descriptions.Name == "DescriptionsSemanticOwner");

            var host = page.GetVisualDescendants().OfType<GalleryShowCaseHost>().Single();
            host.SelectedTab = GalleryShowCaseTab.SemanticParts;
            Dispatcher.UIThread.RunJobs();

            page.GetVisualDescendants().OfType<SemanticPartPreview>().ShouldHaveSingleItem();
            page.GetVisualDescendants()
                .OfType<AtomUI.Desktop.Controls.Descriptions>()
                .Count(static descriptions => descriptions.Name == "DescriptionsSemanticOwner")
                .ShouldBe(1);

            host.SelectedTab = GalleryShowCaseTab.Examples;
            Dispatcher.UIThread.RunJobs();
            Assert.All(page.GetVisualDescendants().OfType<SemanticPartPreview>(), preview => Assert.False(preview.IsEffectivelyVisible));
        });
    }

    [Fact]
    public void Descriptions_Semantic_Preview_Content_Does_Not_Overlap_The_Parts_Pane()
    {
        AvaloniaTestApp.EnsureInitialized();

        var page = new DescriptionsShowCase
        {
            DataContext = new DescriptionsViewModel(new TestScreen())
        };

        ShowInWindow(page, 1280, 900, () =>
        {
            var host = page.GetVisualDescendants().OfType<GalleryShowCaseHost>().Single();
            host.SelectedTab = GalleryShowCaseTab.SemanticParts;
            Dispatcher.UIThread.RunJobs();

            var preview = page.GetVisualDescendants().OfType<SemanticPartPreview>().Single();
            var owner = preview.GetVisualDescendants()
                               .OfType<AtomUI.Desktop.Controls.Descriptions>()
                               .Single(static descriptions => descriptions.Name == "DescriptionsSemanticOwner");
            var previewStage = preview.GetVisualDescendants()
                                      .OfType<Border>()
                                      .Single(static border => border.Name == "PART_PreviewStage");
            var availableContentWidth = previewStage.Bounds.Width -
                                        previewStage.Padding.Left -
                                        previewStage.Padding.Right;

            owner.Bounds.Width.ShouldBeLessThanOrEqualTo(availableContentWidth);
        });
    }

    [Fact]
    public void Descriptions_Semantic_Part_Example_Matches_The_Approved_Two_Size_Comparison()
    {
        AvaloniaTestApp.EnsureInitialized();

        var page = new DescriptionsShowCase
        {
            DataContext = new DescriptionsViewModel(new TestScreen())
        };

        ShowInWindow(page, 1280, 1400, () =>
        {
            var panel = page.GetVisualDescendants().OfType<ShowCasePanel>().Single();
            var item = panel.Children
                            .OfType<ShowCaseItem>()
                            .Single(static candidate => candidate.SourceKey == "descriptions-semantic-part");
            item.MaterializeDeferredContent();
            Dispatcher.UIThread.RunJobs();

            var owners = page.GetVisualDescendants()
                             .OfType<AtomUI.Desktop.Controls.Descriptions>()
                             .Where(static descriptions => descriptions.Classes.Contains("semantic-demo"))
                             .ToArray();
            owners.Length.ShouldBe(2);

            var small = owners.Single(static descriptions => descriptions.Classes.Contains("semantic-demo-small"));
            var styledDefault = owners.Single(static descriptions =>
                descriptions.Classes.Contains("semantic-demo-default"));

            small.SizeType.ShouldBe(AtomUI.SizeType.Small);
            styledDefault.SizeType.ShouldBe(AtomUI.SizeType.Large);
            small.Items.Count.ShouldBe(3);
            styledDefault.Items.Count.ShouldBe(3);
            small.Header.ShouldBe(styledDefault.Header);
            small.Padding.ShouldBe(new Avalonia.Thickness(10));
            styledDefault.Padding.ShouldBe(new Avalonia.Thickness(10));
            styledDefault.BorderThickness.ShouldBe(new Avalonia.Thickness(1));
            styledDefault.CornerRadius.ShouldBe(new Avalonia.CornerRadius(8));
            styledDefault.BorderBrush.ShouldNotBeNull()
                         .ShouldBeAssignableTo<ISolidColorBrush>()
                         .Color.ShouldBe(Avalonia.Media.Color.Parse("#CDC1FF"));

            var smallLabels = GetItemPartTargets(small, "semantic-label");
            var styledLabels = GetItemPartTargets(styledDefault, "semantic-label");
            smallLabels.Length.ShouldBe(3);
            styledLabels.Length.ShouldBe(3);
            foreach (var label in smallLabels)
            {
                label.Foreground.ShouldNotBeNull()
                     .ShouldBeAssignableTo<ISolidColorBrush>()
                     .Color.ShouldBe(Colors.Black);
            }
            foreach (var label in styledLabels)
            {
                label.Foreground.ShouldNotBeNull()
                       .ShouldBeAssignableTo<ISolidColorBrush>()
                       .Color.ShouldBe(Avalonia.Media.Color.Parse("#A294F9"));
            }
        });
    }

    private static string ExtractDescriptionsExampleItems(string source)
    {
        const string firstItemMarker  = "<gallery:ShowCaseItem";
        var firstItemStart = source.IndexOf(firstItemMarker, StringComparison.Ordinal);
        firstItemStart.ShouldBeGreaterThanOrEqualTo(0);

        const string semanticItemMarker = "SourceKey=\"descriptions-semantic-part\"";
        var semanticItemStart = source.IndexOf(semanticItemMarker, firstItemStart, StringComparison.Ordinal);
        semanticItemStart.ShouldBeGreaterThan(firstItemStart);
        var panelCloseStart = source.LastIndexOf("<gallery:ShowCaseItem", semanticItemStart, StringComparison.Ordinal);
        panelCloseStart.ShouldBeGreaterThan(firstItemStart);

        return source[firstItemStart..panelCloseStart];
    }

    private static ContentPresenter[] GetItemPartTargets(
        AtomUI.Desktop.Controls.Descriptions owner,
        string marker)
    {
        var itemsScope = owner.GetVisualDescendants()
                              .OfType<Avalonia.Controls.Grid>()
                              .Single(control =>
                                  ReferenceEquals(control.TemplatedParent, owner) &&
                                  control.Classes.Contains("semantic-scope-items"));
        return itemsScope.GetVisualChildren()
                         .OfType<Control>()
                         .Where(static control => control.Classes.Contains("semantic-scope-item"))
                         .SelectMany(control => control.GetVisualDescendants()
                                                   .OfType<ContentPresenter>()
                                                   .Where(presenter =>
                                                       ReferenceEquals(presenter.TemplatedParent, control) &&
                                                       presenter.Classes.Contains(marker)))
                         .ToArray();
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
