using System.Collections;
using System.Reflection;
using AtomUI.Controls;
using AtomUI.Toolkits.GalleryBase.Controls;
using Avalonia;
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
using AtomUICollapse = AtomUI.Desktop.Controls.Collapse;
using AtomUICollapseItem = AtomUI.Desktop.Controls.CollapseItem;
using CollapseShowCase = AtomUIGallery.ShowCases.Collapse.CollapseShowCase;
using CollapseViewModel = AtomUIGallery.ShowCases.Collapse.CollapseViewModel;

namespace AtomUIGallery.Tests.ShowCases;

public class CollapseShowCasePageTests
{

    [Fact]
    public void Collapse_ShowCase_Declares_The_Semantic_Previews_And_Style_Example()
    {
        var source = ReadRepoFile(
            "controlgallery/AtomUIGallery/ShowCases/DataDisplay/Collapse/Views/CollapseShowCase.axaml");
        var english = ReadRepoFile(
            "controlgallery/AtomUIGallery/ShowCases/DataDisplay/Collapse/Localization/en-US.xlf");
        var semanticSource = ExtractShowCaseItem(source, "collapse-semantic-part");

        source.ShouldContain("<gallery:GalleryShowCaseHost.SemanticPartsContentTemplate>");
        source.ShouldContain("Name=\"CollapseSemanticPreview\"");
        source.ShouldContain("SemanticOwner=\"{Binding #CollapseSemanticOwner}\"");
        source.ShouldContain("SemanticOwnerType=\"{x:Type atom:Collapse}\"");
        CountOccurrences(source, "<gallery:SemanticPartDescription").ShouldBe(5);
        foreach (var path in new[] { "root", "header", "icon", "title", "body" })
        {
            source.ShouldContain($"Path=\"{path}\"");
        }

        semanticSource.ShouldContain("SourceKey=\"collapse-semantic-part\"");
        semanticSource.ShouldContain("CollapseShowCaseLangResource SemanticPartStyleTitle");
        semanticSource.ShouldContain("CollapseShowCaseLangResource SemanticPartStyleDescription");
        semanticSource.ShouldContain("Selector=\"atom|Collapse.semantic-object\"");
        semanticSource.ShouldContain("Selector=\"atom|Collapse.semantic-function\"");
        semanticSource.ShouldContain("Value=\"#fafafa\"");
        semanticSource.ShouldContain("Value=\"#e0e0e0\"");
        semanticSource.ShouldContain("Value=\"#f0f0f0\"");
        semanticSource.ShouldContain("Value=\"#141414\"");
        semanticSource.ShouldContain("Value=\"#ffffff\"");
        semanticSource.ShouldContain("Value=\"#696fc7\"");
        semanticSource.ShouldContain("Value=\"#f5efff\"");
        semanticSource.ShouldContain("SizeType=\"Large\"");
        semanticSource.ShouldContain("Property=\"Background\"");
        semanticSource.ShouldContain("Property=\"Padding\"");
        semanticSource.ShouldContain("Property=\"Foreground\"");
        CountOccurrences(semanticSource, "<atom:CollapseHeaderStyle").ShouldBe(2);
        CountOccurrences(semanticSource, "<atom:CollapseTitleStyle").ShouldBe(2);
        semanticSource.ShouldContain("x:SetterTargetType=\"atom:PixelAlignedBorder\"");
        semanticSource.ShouldContain("x:SetterTargetType=\"ContentPresenter\"");
        CountOccurrences(semanticSource, "<atom:Collapse ").ShouldBe(2);
        semanticSource.ShouldNotContain("/template/");
        semanticSource.ShouldNotContain("classNames", Case.Insensitive);
        semanticSource.ShouldNotContain("semantic dom", Case.Insensitive);
        english.ShouldContain("Custom Semantic Part styling");
        english.ShouldContain("Root element with border, border-radius, background color and container styles that control the overall layout and appearance of collapse panels");
        english.ShouldContain("Header element with flex layout, padding, color, line-height, cursor style, transition animations and other interactive styles for panel headers");
        english.ShouldContain("Icon element with font size, transition animations, rotation transforms and other styles and animations for expand/collapse arrows");
        english.ShouldContain("Title element with flex auto layout and margin styles for title text layout and typography");
        english.ShouldContain("Body element with padding, color, background color and other styles for panel content area display");
        english.ShouldNotContain("semantic dom", Case.Insensitive);
        english.ShouldNotContain("classNames", Case.Insensitive);
    }

    [Fact]
    public void Collapse_Semantic_Previews_Are_Materialized_Only_After_The_Tab_Is_Selected()
    {
        AvaloniaTestApp.EnsureInitialized();

        var page = new CollapseShowCase
        {
            DataContext = new CollapseViewModel(new TestScreen())
        };

        ShowInWindow(page, 1280, 900, () =>
        {
            page.GetVisualDescendants().OfType<SemanticPartPreview>().ShouldBeEmpty();
            page.GetVisualDescendants()
                .OfType<AtomUICollapse>()
                .ShouldNotContain(static collapse => collapse.Name == "CollapseSemanticOwner");

            var host = page.GetVisualDescendants().OfType<GalleryShowCaseHost>().Single();
            host.SelectedTab = GalleryShowCaseTab.SemanticParts;
            Dispatcher.UIThread.RunJobs();

            page.GetVisualDescendants().OfType<SemanticPartPreview>().ShouldHaveSingleItem();
            var preview = page.GetVisualDescendants()
                              .OfType<SemanticPartPreview>()
                              .Single(static candidate => candidate.Name == "CollapseSemanticPreview");
            var semanticCollapse = preview.SemanticOwner.ShouldBeOfType<AtomUICollapse>();
            semanticCollapse.Name.ShouldBe("CollapseSemanticOwner");
            semanticCollapse.GetVisualDescendants()
                            .OfType<Control>()
                            .Single(static header => header.Classes.Contains("semantic-header"))
                            .IsEffectivelyVisible.ShouldBeTrue();
            semanticCollapse.GetVisualDescendants()
                            .OfType<Control>()
                            .Single(static body => body.Classes.Contains("semantic-body"))
                            .IsEffectivelyVisible.ShouldBeTrue();

            var previewItemsValue = typeof(SemanticPartPreview)
                                    .GetProperty("Items", BindingFlags.Instance | BindingFlags.NonPublic)
                                    ?.GetValue(preview);
            previewItemsValue.ShouldNotBeNull();
            var previewItems = previewItemsValue.ShouldBeAssignableTo<IEnumerable>()
                                                .Cast<object>()
                                                .ToArray();
            previewItems.Length.ShouldBe(5);

            host.SelectedTab = GalleryShowCaseTab.Examples;
            Dispatcher.UIThread.RunJobs();
            Assert.All(page.GetVisualDescendants().OfType<SemanticPartPreview>(), preview => Assert.False(preview.IsEffectivelyVisible));
        });
    }

    [Fact]
    public void Collapse_Semantic_Style_Example_Applies_The_Official_Style_Values()
    {
        AvaloniaTestApp.EnsureInitialized();

        var page = new CollapseShowCase
        {
            DataContext = new CollapseViewModel(new TestScreen())
        };

        ShowInWindow(page, 1280, 900, () =>
        {
            var panel = page.GetVisualDescendants().OfType<ShowCasePanel>().Single();
            var item = panel.Children
                            .OfType<ShowCaseItem>()
                            .Single(static candidate => candidate.SourceKey == "collapse-semantic-part");
            item.MaterializeDeferredContent();
            Dispatcher.UIThread.RunJobs();

            var demos = page.GetVisualDescendants()
                            .OfType<AtomUICollapse>()
                            .Where(static collapse => collapse.Classes.Contains("semantic-object") ||
                                                      collapse.Classes.Contains("semantic-function"))
                            .ToArray();
            demos.Length.ShouldBe(2);

            var defaultDemo = demos.Single(static collapse => collapse.Classes.Contains("semantic-object"));
            defaultDemo.SizeType.ShouldBe(CustomizableSizeType.Middle);
            var defaultFrame = FindFrame(defaultDemo);
            AssertSolidColor(defaultFrame.Background, "#fafafa");
            AssertSolidColor(defaultFrame.BorderBrush, "#e0e0e0");
            defaultFrame.CornerRadius.ShouldBe(new CornerRadius(8));
            var defaultHeader = FindHeader(defaultDemo);
            AssertSolidColor(defaultHeader.Background, "#f0f0f0");
            defaultHeader.Padding.ShouldBe(new Thickness(16, 12));
            AssertSolidColor(FindTitle(defaultDemo).Foreground, "#141414");
            GetContainerAt(defaultDemo, 0).IsSelected.ShouldBeTrue();
            GetContainerAt(defaultDemo, 1).IsSelected.ShouldBeFalse();
            GetContainerAt(defaultDemo, 2).IsSelected.ShouldBeFalse();
            FindItemHeader(defaultDemo, 0).CornerRadius.ShouldBe(new CornerRadius(8, 8, 0, 0));
            FindItemHeader(defaultDemo, 2).CornerRadius.ShouldBe(new CornerRadius(0, 0, 8, 8));
            // The body marker lives inside the content motion actor and materializes only
            // while a panel is expanded, so expand the last panel before reading its frame.
            defaultDemo.IsMotionEnabled = false;
            GetContainerAt(defaultDemo, 2).IsSelected = true;
            Dispatcher.UIThread.RunJobs();
            FindItemContent(defaultDemo, 0).CornerRadius.ShouldBe(default(CornerRadius));
            FindItemContent(defaultDemo, 2).CornerRadius.ShouldBe(new CornerRadius(0, 0, 8, 8));

            var largeDemo = demos.Single(static collapse => collapse.Classes.Contains("semantic-function"));
            largeDemo.SizeType.ShouldBe(CustomizableSizeType.Large);
            var largeFrame = FindFrame(largeDemo);
            AssertSolidColor(largeFrame.Background, "#ffffff");
            AssertSolidColor(largeFrame.BorderBrush, "#696fc7");
            largeFrame.CornerRadius.ShouldBe(new CornerRadius(8));
            var largeHeader = FindHeader(largeDemo);
            AssertSolidColor(largeHeader.Background, "#f5efff");
            largeHeader.Padding.ShouldBe(new Thickness(16, 12));
            AssertSolidColor(FindTitle(largeDemo).Foreground, "#141414");
            GetContainerAt(largeDemo, 0).IsSelected.ShouldBeFalse();
            GetContainerAt(largeDemo, 1).IsSelected.ShouldBeTrue();
            GetContainerAt(largeDemo, 2).IsSelected.ShouldBeFalse();
            FindItemHeader(largeDemo, 0).CornerRadius.ShouldBe(new CornerRadius(8, 8, 0, 0));
            FindItemHeader(largeDemo, 2).CornerRadius.ShouldBe(new CornerRadius(0, 0, 8, 8));
            largeDemo.IsMotionEnabled = false;
            GetContainerAt(largeDemo, 2).IsSelected = true;
            Dispatcher.UIThread.RunJobs();
            FindItemContent(largeDemo, 1).CornerRadius.ShouldBe(default(CornerRadius));
            FindItemContent(largeDemo, 2).CornerRadius.ShouldBe(new CornerRadius(0, 0, 8, 8));
        });
    }

    private static AtomUICollapseItem GetContainerAt(AtomUICollapse collapse, int index)
    {
        return collapse.ContainerFromIndex(index).ShouldBeOfType<AtomUICollapseItem>();
    }

    private static AtomUI.Controls.Primitives.PixelAlignedBorder FindFrame(AtomUICollapse owner)
    {
        return owner.GetVisualDescendants()
                    .OfType<AtomUI.Controls.Primitives.PixelAlignedBorder>()
                    .Single(static border => border.Name == "PART_Frame");
    }

    private static AtomUI.Controls.Primitives.PixelAlignedBorder FindHeader(AtomUICollapse owner)
    {
        return owner.GetVisualDescendants()
                    .OfType<AtomUI.Controls.Primitives.PixelAlignedBorder>()
                    .First(static header => header.Classes.Contains("semantic-header"));
    }

    private static ContentPresenter FindTitle(AtomUICollapse owner)
    {
        return owner.GetVisualDescendants()
                    .OfType<ContentPresenter>()
                    .First(static title => title.Classes.Contains("semantic-title"));
    }

    private static AtomUI.Controls.Primitives.PixelAlignedBorder FindItemHeader(AtomUICollapse owner, int index)
    {
        return GetContainerAt(owner, index).GetVisualDescendants()
                                           .OfType<AtomUI.Controls.Primitives.PixelAlignedBorder>()
                                           .Single(static header => header.Classes.Contains("semantic-header"));
    }

    private static AtomUI.Controls.Primitives.PixelAlignedBorder FindItemContent(AtomUICollapse owner, int index)
    {
        return GetContainerAt(owner, index).GetVisualDescendants()
                                           .OfType<AtomUI.Controls.Primitives.PixelAlignedBorder>()
                                           .Single(static content => content.Classes.Contains("semantic-body"));
    }

    private static string ExtractCollapseExampleItems(string source)
    {
        const string firstItemMarker = "<gallery:ShowCaseItem";
        var firstItemStart = source.IndexOf(firstItemMarker, StringComparison.Ordinal);
        firstItemStart.ShouldBeGreaterThanOrEqualTo(0);

        const string semanticItemMarker = "SourceKey=\"collapse-semantic-part\"";
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
