using System.Collections;
using System.Reflection;
using AtomUI.Controls;
using AtomUI.Controls.Commons;
using AtomUI.Desktop.Controls;
using AtomUI.Toolkits.GalleryBase.Controls;
using AtomUIGallery.ShowCases.Skeleton;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Presenters;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using ReactiveUI;
using Shouldly;
using Xunit;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUIGallery.Tests.ShowCases;

public class SkeletonShowCasePageTests
{

    [Fact]
    public void Skeleton_ShowCase_Declares_The_Ant_Design_Semantic_Previews_And_Style_Example()
    {
        var source = ReadRepoFile(
            "controlgallery/AtomUIGallery/ShowCases/Feedback/Skeleton/Views/SkeletonShowCase.axaml");
        var english = ReadRepoFile(
            "controlgallery/AtomUIGallery/ShowCases/Feedback/Skeleton/Localization/en-US.xlf");
        var semanticSource = ExtractShowCaseItem(source, "skeleton-semantic-part");

        source.ShouldContain("<gallery:GalleryShowCaseHost.SemanticPartsContentTemplate>");
        source.ShouldContain("Name=\"SkeletonSemanticPreview\"");
        source.ShouldContain("SemanticOwner=\"{Binding #SkeletonSemanticOwner}\"");
        source.ShouldContain("SemanticOwnerType=\"{x:Type atom:Skeleton}\"");
        source.ShouldContain("Name=\"SkeletonElementSemanticPreview\"");
        source.ShouldContain("SemanticOwner=\"{Binding #SkeletonAvatarSemanticOwner}\"");
        source.ShouldContain("SelectionChanged=\"HandleSkeletonElementChanged\"");
        CountOccurrences(source, "<gallery:SemanticPartDescription").ShouldBe(8);
        foreach (var path in new[] { "root", "header", "section", "avatar", "title", "paragraph", "content" })
        {
            source.ShouldContain($"Path=\"{path}\"");
        }

        semanticSource.ShouldContain("SourceKey=\"skeleton-semantic-part\"");
        semanticSource.ShouldContain("SkeletonShowCaseLangResource SemanticPartStyleTitle");
        semanticSource.ShouldContain("SkeletonShowCaseLangResource SemanticPartStyleDescription");
        semanticSource.ShouldContain("ColumnDefinitions=\"*,*\"");
        semanticSource.ShouldContain("ColumnSpacing=\"16\"");
        semanticSource.ShouldContain("Value=\"10\"");
        semanticSource.ShouldContain("Value=\"12\"");
        semanticSource.ShouldContain("Value=\"0,0,0,12\"");
        semanticSource.ShouldContain("Value=\"#aaa\"");
        semanticSource.ShouldContain("Value=\"1\"");
        semanticSource.ShouldContain("Value=\"rgba(229, 243, 254, 0.3)\"");
        semanticSource.ShouldContain("Value=\"rgba(229, 243, 254, 0.5)\"");
        semanticSource.ShouldContain("Value=\"20\"");
        semanticSource.ShouldContain("Value=\"20\"");
        semanticSource.ShouldContain("ParagraphRows=\"3\"");
        semanticSource.ShouldNotContain("/template/");
        semanticSource.ShouldNotContain("classNames", Case.Insensitive);
        semanticSource.ShouldNotContain("semantic dom", Case.Insensitive);
        english.ShouldContain("Custom Semantic Part styling");
        english.ShouldContain("Root element with table display, width, animation effects, border radius and other skeleton container basic styles.");
        english.ShouldContain("Header element with table cell, padding, vertical alignment and other avatar placeholder area layout styles.");
        english.ShouldContain("Section element with skeleton content area layout styles.");
        english.ShouldContain("Avatar element with inline-block display, vertical alignment, background color, size, border radius and other avatar placeholder styles.");
        english.ShouldContain("Title element with width, height, background color, border radius and other title placeholder styles.");
        english.ShouldContain("Paragraph element with padding, list item styles, background color, border radius and other paragraph placeholder styles.");
        english.ShouldNotContain("semantic dom", Case.Insensitive);
        english.ShouldNotContain("classNames", Case.Insensitive);
    }

    [Fact]
    public void Skeleton_Semantic_Previews_Are_Materialized_Only_After_The_Tab_Is_Selected()
    {
        AvaloniaTestApp.EnsureInitialized();

        var page = new SkeletonShowCase
        {
            DataContext = new SkeletonViewModel(new TestScreen())
        };

        ShowInWindow(page, 1280, 900, () =>
        {
            page.GetVisualDescendants().OfType<SemanticPartPreview>().ShouldBeEmpty();

            var host = page.GetVisualDescendants().OfType<GalleryShowCaseHost>().Single();
            host.SelectedTab = GalleryShowCaseTab.SemanticParts;
            for (var i = 0; i < 3; i++)
            {
                Dispatcher.UIThread.RunJobs();
            }

            // 语义 tab 激活时语义内容已挂载，直接以宿主为根断言。
            var activeContent = host;
            activeContent.GetVisualDescendants().OfType<SemanticPartPreview>().Count().ShouldBe(2);
            var semanticSkeleton = activeContent.GetVisualDescendants()
                                                .OfType<AtomUI.Desktop.Controls.Skeleton>()
                                                .Single(static skeleton => skeleton.Name == "SkeletonSemanticOwner");
            semanticSkeleton.GetVisualDescendants().OfType<SkeletonAvatar>().ShouldHaveSingleItem();
            semanticSkeleton.GetVisualDescendants().OfType<SkeletonTitle>().ShouldHaveSingleItem();
            semanticSkeleton.GetVisualDescendants().OfType<SkeletonParagraph>().ShouldHaveSingleItem();
            var semanticTitle = semanticSkeleton.GetVisualDescendants().OfType<SkeletonTitle>().Single();
            var semanticParagraph = semanticSkeleton.GetVisualDescendants().OfType<SkeletonParagraph>().Single();
            semanticTitle.IsEffectivelyVisible.ShouldBeTrue();
            semanticParagraph.IsEffectivelyVisible.ShouldBeTrue();
            semanticSkeleton.Bounds.Width.ShouldBeGreaterThan(300);
            semanticTitle.Bounds.Width.ShouldBeGreaterThan(0);
            semanticParagraph.Bounds.Width.ShouldBeGreaterThan(0);

            var semanticPreview = activeContent.GetVisualDescendants()
                                                .OfType<SemanticPartPreview>()
                                                .Single(static preview => preview.Name == "SkeletonSemanticPreview");
            var previewItemsValue = typeof(SemanticPartPreview)
                                    .GetProperty("Items", BindingFlags.Instance | BindingFlags.NonPublic)
                                    ?.GetValue(semanticPreview);
            previewItemsValue.ShouldNotBeNull();
            var previewItems = previewItemsValue.ShouldBeAssignableTo<IEnumerable>()
                                                 .Cast<object>()
                                                 .ToArray();
            var headerItem = previewItems.Single(item =>
            {
                var path = item.GetType().GetProperty("Path")?.GetValue(item) as string;
                return path == "header";
            });
            typeof(SemanticPartPreview)
                .GetMethod("SetHoveredPart", BindingFlags.Instance | BindingFlags.NonPublic)
                .ShouldNotBeNull()
                .Invoke(semanticPreview, [headerItem, true]);
            Dispatcher.UIThread.RunJobs();

            var header = semanticSkeleton.GetVisualDescendants()
                                         .OfType<DockPanel>()
                                         .Single(static panel => panel.Classes.Contains("semantic-header"));
            var section = semanticSkeleton.GetVisualDescendants()
                                          .OfType<StackPanel>()
                                          .Single(static panel => panel.Classes.Contains("semantic-section"));
            header.Parent.ShouldBeSameAs(section.Parent);
            header.Bounds.X.ShouldBe(0);
            section.Bounds.X.ShouldBeGreaterThanOrEqualTo(header.Bounds.Right);
            section.Bounds.Width.ShouldBeGreaterThan(header.Bounds.Width);
            header.Bounds.Width.ShouldBeLessThan(semanticSkeleton.Bounds.Width);
            var headerAdorners = AdornerLayer.GetAdornerLayer(header)
                                             .ShouldNotBeNull()
                                             .Children
                                             .Where(static child => child.GetType().Name == "SemanticPartAdorner")
                                             .Select(AdornerLayer.GetAdornedElement)
                                             .ToArray();
            headerAdorners.ShouldHaveSingleItem().ShouldBeSameAs(header);

            host.SelectedTab = GalleryShowCaseTab.Examples;
            Dispatcher.UIThread.RunJobs();
            Assert.All(page.GetVisualDescendants().OfType<SemanticPartPreview>(), preview => Assert.False(preview.IsEffectivelyVisible));
        });
    }

    [Fact]
    public void Skeleton_Semantic_Style_Example_Applies_The_Official_Style_Values()
    {
        AvaloniaTestApp.EnsureInitialized();

        var page = new SkeletonShowCase
        {
            DataContext = new SkeletonViewModel(new TestScreen())
        };

        ShowInWindow(page, 1280, 1400, () =>
        {
            var panel = page.GetVisualDescendants().OfType<ShowCasePanel>().Single();
            var item = panel.Children
                            .OfType<ShowCaseItem>()
                            .Single(static candidate => candidate.SourceKey == "skeleton-semantic-part");
            item.MaterializeDeferredContent();
            Dispatcher.UIThread.RunJobs();

            var styleDemo = page.GetVisualDescendants()
                                .OfType<Skeleton>()
                                .Single(static skeleton => skeleton.Classes.Contains("semantic-style-demo"));
            styleDemo.CornerRadius.ShouldBe(new CornerRadius(10));
            styleDemo.Padding.ShouldBe(new Thickness(12));

            var styleHeader = FindSemanticElement<DockPanel>(styleDemo, "semantic-header");
            styleHeader.Margin.ShouldBe(new Thickness(0, 0, 0, 12));

            var styleAvatar = styleDemo.GetVisualDescendants().OfType<SkeletonAvatar>().Single();
            AssertSolidColor(styleAvatar.BorderBrush, "#aaa");
            styleAvatar.BorderThickness.ShouldBe(new Thickness(1));

            var styleTitle = styleDemo.GetVisualDescendants().OfType<SkeletonTitle>().Single();
            AssertSolidColor(styleTitle.BorderBrush, "#aaa");
            styleTitle.BorderThickness.ShouldBe(new Thickness(1));

            var functionDemo = page.GetVisualDescendants()
                                   .OfType<Skeleton>()
                                   .Single(static skeleton => skeleton.Classes.Contains("semantic-function-demo"));
            AssertSolidColor(functionDemo.BorderBrush, "rgba(229, 243, 254, 0.3)");
            functionDemo.BorderThickness.ShouldBe(new Thickness(1));

            var functionTitle = functionDemo.GetVisualDescendants().OfType<SkeletonTitle>().Single();
            AssertSolidColor(functionTitle.Background, "rgba(229, 243, 254, 0.5)");
            functionTitle.Height.ShouldBe(20);
            functionTitle.CornerRadius.ShouldBe(new CornerRadius(20));

            var functionParagraph = functionDemo.GetVisualDescendants().OfType<SkeletonParagraph>().Single();
            AssertSolidColor(functionParagraph.Background, "rgba(229, 243, 254, 0.5)");
            var expectedParagraphColor = Color.Parse("rgba(229, 243, 254, 0.5)");
            var paragraphLines = functionParagraph.GetVisualDescendants()
                                                   .OfType<SkeletonLine>()
                                                   .ToArray();
            paragraphLines.ShouldNotBeEmpty();
            foreach (var line in paragraphLines)
            {
                AssertSolidColor(line.Background, expectedParagraphColor);
            }
        });
    }

    [Fact]
    public void Skeleton_Element_Semantic_Preview_Switches_The_Owner_And_Content()
    {
        AvaloniaTestApp.EnsureInitialized();

        var page = new SkeletonShowCase
        {
            DataContext = new SkeletonViewModel(new TestScreen())
        };

        ShowInWindow(page, 1280, 900, () =>
        {
            var host = page.GetVisualDescendants().OfType<GalleryShowCaseHost>().Single();
            host.SelectedTab = GalleryShowCaseTab.SemanticParts;
            Dispatcher.UIThread.RunJobs();

            var preview = page.GetVisualDescendants()
                              .OfType<SemanticPartPreview>()
                              .Single(static candidate => candidate.Name == "SkeletonElementSemanticPreview");
            var previewContent = preview.PreviewContent.ShouldBeOfType<StackPanel>();
            var segmented = previewContent.GetVisualDescendants().OfType<Segmented>().Single();
            var previewHost = previewContent.GetVisualDescendants()
                                            .OfType<Grid>()
                                            .Single(static control => control.Name == "SkeletonElementPreviewHost");
            segmented.Items.Count.ShouldBe(5);

            var expectedTypes = new[]
            {
                typeof(SkeletonAvatar),
                typeof(SkeletonButton),
                typeof(SkeletonInput),
                typeof(SkeletonImage),
                typeof(SkeletonNode)
            };

            for (var index = 0; index < expectedTypes.Length; index++)
            {
                segmented.SetCurrentValue(SelectingItemsControl.SelectedIndexProperty, index);
                Dispatcher.UIThread.RunJobs();

                var owner = previewHost.Children.ShouldHaveSingleItem().ShouldBeAssignableTo<Control>();
                owner.GetType().ShouldBe(expectedTypes[index]);
                preview.SemanticOwner.ShouldBeSameAs(owner);
                preview.SemanticOwnerType.ShouldBe(expectedTypes[index]);
                owner.Bounds.Width.ShouldBeGreaterThan(300,
                    $"previewContent={previewContent.Bounds}, segmented={segmented.Bounds}, " +
                    $"host={previewHost.Bounds}, owner={owner.Bounds}");
                previewHost.Bounds.Width.ShouldBeGreaterThanOrEqualTo(owner.Bounds.Width);
                var contentLayers = owner.GetVisualDescendants()
                                          .OfType<Border>()
                                          .Where(static border => border.Classes.Contains("semantic-content"))
                                          .ToArray();
                contentLayers.ShouldNotBeEmpty();
                foreach (var contentLayer in contentLayers)
                {
                    contentLayer.Bounds.Width.ShouldBeLessThan(owner.Bounds.Width);
                    contentLayer.Bounds.X.ShouldBe(0);
                }
            }
        });
    }

    private static string ExtractSkeletonExampleItems(string source)
    {
        const string firstItemMarker = "<gallery:ShowCaseItem";
        var firstItemStart = source.IndexOf(firstItemMarker, StringComparison.Ordinal);
        firstItemStart.ShouldBeGreaterThanOrEqualTo(0);

        const string semanticItemMarker = "SourceKey=\"skeleton-semantic-part\"";
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
        var count = 0;
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

    private static T FindSemanticElement<T>(Control owner, string semanticClass)
        where T : Visual
    {
        return owner.GetVisualDescendants()
                    .OfType<T>()
                    .Single(element => element.Classes.Contains(semanticClass));
    }

    private static void AssertSolidColor(IBrush? actual, string expected)
    {
        AssertSolidColor(actual, Color.Parse(expected));
    }

    private static void AssertSolidColor(IBrush? actual, Color expected)
    {
        actual.ShouldNotBeNull()
            .ShouldBeAssignableTo<ISolidColorBrush>()
            .Color.ShouldBe(expected);
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
