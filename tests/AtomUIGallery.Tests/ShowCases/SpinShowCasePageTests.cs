using System.Collections;
using System.Reflection;
using AtomUI.Toolkits.GalleryBase.Controls;
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
using AtomUISpin = AtomUI.Desktop.Controls.Spin;
using AtomUISpinIndicator = AtomUI.Desktop.Controls.SpinIndicator;
using AtomUITextBlock = AtomUI.Desktop.Controls.TextBlock;
using Ellipse = Avalonia.Controls.Shapes.Ellipse;
using SpinShowCase = AtomUIGallery.ShowCases.Spin.SpinShowCase;
using SpinViewModel = AtomUIGallery.ShowCases.Spin.SpinViewModel;

namespace AtomUIGallery.Tests.ShowCases;

public class SpinShowCasePageTests
{

    [Fact]
    public void Spin_ShowCase_Declares_The_Semantic_Previews_And_Style_Example()
    {
        var source = ReadRepoFile(
            "controlgallery/AtomUIGallery/ShowCases/Feedback/Spin/Views/SpinShowCase.axaml");
        var english = ReadRepoFile(
            "controlgallery/AtomUIGallery/ShowCases/Feedback/Spin/Localization/en-US.xlf");
        var semanticSource = ExtractShowCaseItem(source, "spin-semantic-part");

        source.ShouldContain("<gallery:GalleryShowCaseHost.SemanticPartsContentTemplate>");
        source.ShouldContain("Name=\"SpinSemanticPreview\"");
        source.ShouldContain("SemanticOwner=\"{Binding #SpinSemanticOwner}\"");
        source.ShouldContain("SemanticOwnerType=\"{x:Type atom:Spin}\"");
        source.ShouldContain("Name=\"SpinIndicatorSemanticPreview\"");
        source.ShouldContain("SemanticOwner=\"{Binding #SpinIndicatorSemanticOwner}\"");
        source.ShouldContain("SemanticOwnerType=\"{x:Type atom:SpinIndicator}\"");
        CountOccurrences(source, "<gallery:SemanticPartDescription").ShouldBe(8);
        foreach (var path in new[] { "root", "container", "mask", "section", "indicator", "description", "content" })
        {
            source.ShouldContain($"Path=\"{path}\"");
        }

        semanticSource.ShouldContain("SourceKey=\"spin-semantic-part\"");
        semanticSource.ShouldContain("SpinShowCaseLangResource SemanticPartStyleTitle");
        semanticSource.ShouldContain("SpinShowCaseLangResource SemanticPartStyleDescription");
        semanticSource.ShouldContain("Orientation=\"Horizontal\"");
        semanticSource.ShouldContain("Spacing=\"16\"");
        semanticSource.ShouldContain("Property=\"DotBgBrush\"");
        semanticSource.ShouldContain("Value=\"#00d4ff\"");
        semanticSource.ShouldContain("Value=\"#722ed1\"");
        semanticSource.ShouldContain("SizeType=\"Small\"");
        semanticSource.ShouldContain("Selector=\"atom|Spin.semantic-function-demo[SizeType=Small]\"");
        semanticSource.ShouldNotContain("/template/");
        semanticSource.ShouldNotContain("classNames", Case.Insensitive);
        semanticSource.ShouldNotContain("semantic dom", Case.Insensitive);
        english.ShouldContain("Custom Semantic Part styling");
        english.ShouldContain("Root element with the loading region's base layout, alignment and theme entry styles.");
        english.ShouldContain("Container element with opacity and blur transition styles for the nested content.");
        english.ShouldContain("Mask element with background, opacity and other overlay dimming styles.");
        english.ShouldContain("Section element with centered alignment, spacing and other loading area layout styles.");
        english.ShouldContain("Indicator element with size, motion duration and other spinning indicator styles.");
        english.ShouldContain("Description element with font size, foreground and line height styles for the tip text.");
        english.ShouldNotContain("semantic dom", Case.Insensitive);
        english.ShouldNotContain("classNames", Case.Insensitive);
    }

    [Fact]
    public void Spin_Semantic_Previews_Are_Materialized_Only_After_The_Tab_Is_Selected()
    {
        AvaloniaTestApp.EnsureInitialized();

        var page = new SpinShowCase
        {
            DataContext = new SpinViewModel(new TestScreen())
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
            var semanticSpin = activeContent.GetVisualDescendants()
                                            .OfType<AtomUISpin>()
                                            .Single(static spin => spin.Name == "SpinSemanticOwner");
            semanticSpin.Bounds.Width.ShouldBeGreaterThan(300);
            semanticSpin.GetVisualDescendants()
                        .OfType<AtomUISpinIndicator>()
                        .Single(static indicator => indicator.Classes.Contains("semantic-indicator"))
                        .IsEffectivelyVisible.ShouldBeTrue();
            var mask = semanticSpin.GetVisualDescendants()
                                   .OfType<Border>()
                                   .Single(static border => border.Classes.Contains("semantic-mask"));
            mask.IsEffectivelyVisible.ShouldBeTrue();
            var container = semanticSpin.GetVisualDescendants()
                                        .OfType<ContentPresenter>()
                                        .Single(static presenter => presenter.Classes.Contains("semantic-container"));
            var section = semanticSpin.GetVisualDescendants()
                                      .OfType<StackPanel>()
                                      .Single(static panel => panel.Classes.Contains("semantic-section"));
            section.IsEffectivelyVisible.ShouldBeTrue();
            var description = semanticSpin.GetVisualDescendants()
                                          .OfType<AtomUITextBlock>()
                                          .Single(static text => text.Classes.Contains("semantic-description"));
            description.IsEffectivelyVisible.ShouldBeTrue();

            var maskRect = GetBoundsInOwner(mask, semanticSpin);
            var containerRect = GetBoundsInOwner(container, semanticSpin);
            maskRect.Contains(containerRect).ShouldBeTrue(
                $"mask {maskRect} should cover container {containerRect}");
            var sectionRect = GetBoundsInOwner(section, semanticSpin);
            maskRect.Contains(sectionRect).ShouldBeTrue(
                $"mask {maskRect} should cover section {sectionRect}");
            var descriptionRect = GetBoundsInOwner(description, semanticSpin);
            maskRect.Contains(descriptionRect).ShouldBeTrue(
                $"mask {maskRect} should cover description {descriptionRect}");

            var semanticPreview = activeContent.GetVisualDescendants()
                                               .OfType<SemanticPartPreview>()
                                               .Single(static preview => preview.Name == "SpinSemanticPreview");
            var previewItemsValue = typeof(SemanticPartPreview)
                                    .GetProperty("Items", BindingFlags.Instance | BindingFlags.NonPublic)
                                    ?.GetValue(semanticPreview);
            previewItemsValue.ShouldNotBeNull();
            var previewItems = previewItemsValue.ShouldBeAssignableTo<IEnumerable>()
                                                .Cast<object>()
                                                .ToArray();
            previewItems.Length.ShouldBe(6);
            var maskItem = previewItems.Single(item =>
            {
                var path = item.GetType().GetProperty("Path")?.GetValue(item) as string;
                return path == "mask";
            });
            typeof(SemanticPartPreview)
                .GetMethod("SetHoveredPart", BindingFlags.Instance | BindingFlags.NonPublic)
                .ShouldNotBeNull()
                .Invoke(semanticPreview, [maskItem, true]);
            Dispatcher.UIThread.RunJobs();

            var maskAdorners = AdornerLayer.GetAdornerLayer(mask)
                                           .ShouldNotBeNull()
                                           .Children
                                           .Where(static child => child.GetType().Name == "SemanticPartAdorner")
                                           .Select(AdornerLayer.GetAdornedElement)
                                           .ToArray();
            maskAdorners.ShouldHaveSingleItem().ShouldBeSameAs(mask);

            var indicatorPreview = activeContent.GetVisualDescendants()
                                                .OfType<SemanticPartPreview>()
                                                .Single(static preview => preview.Name == "SpinIndicatorSemanticPreview");
            var indicatorOwner = indicatorPreview.SemanticOwner.ShouldBeOfType<AtomUISpinIndicator>();
            indicatorOwner.GetVisualDescendants()
                          .OfType<Control>()
                          .Where(static control => control.Classes.Contains("semantic-content"))
                          .Count().ShouldBe(2);

            host.SelectedTab = GalleryShowCaseTab.Examples;
            Dispatcher.UIThread.RunJobs();
            Assert.All(page.GetVisualDescendants().OfType<SemanticPartPreview>(), preview => Assert.False(preview.IsEffectivelyVisible));
        });
    }

    [Fact]
    public void Spin_Semantic_Style_Example_Applies_The_Official_Style_Values()
    {
        AvaloniaTestApp.EnsureInitialized();

        var page = new SpinShowCase
        {
            DataContext = new SpinViewModel(new TestScreen())
        };

        ShowInWindow(page, 1280, 1400, () =>
        {
            var panel = page.GetVisualDescendants().OfType<ShowCasePanel>().Single();
            var item = panel.Children
                            .OfType<ShowCaseItem>()
                            .Single(static candidate => candidate.SourceKey == "spin-semantic-part");
            item.MaterializeDeferredContent();
            Dispatcher.UIThread.RunJobs();

            var styleDemo = page.GetVisualDescendants()
                                .OfType<AtomUISpin>()
                                .Single(static spin => spin.Classes.Contains("semantic-style-demo"));
            AssertIndicatorDotFill(styleDemo, "#00d4ff");

            var functionDemo = page.GetVisualDescendants()
                                   .OfType<AtomUISpin>()
                                   .Single(static spin => spin.Classes.Contains("semantic-function-demo"));
            functionDemo.SizeType.ShouldBe(CustomizableSizeType.Small);
            AssertIndicatorDotFill(functionDemo, "#722ed1");
        });
    }

    private static string ExtractSpinExampleItems(string source)
    {
        const string firstItemMarker = "<gallery:ShowCaseItem";
        var firstItemStart = source.IndexOf(firstItemMarker, StringComparison.Ordinal);
        firstItemStart.ShouldBeGreaterThanOrEqualTo(0);

        const string semanticItemMarker = "SourceKey=\"spin-semantic-part\"";
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

    private static Rect GetBoundsInOwner(Visual element, Visual owner)
    {
        var origin = element.TranslatePoint(new Point(), owner).ShouldNotBeNull();
        return new Rect(origin, element.Bounds.Size);
    }

    private static void AssertIndicatorDotFill(AtomUISpin spin, string expected)
    {
        var expectedColor = Color.Parse(expected);
        var indicator = spin.GetVisualDescendants()
                            .OfType<AtomUISpinIndicator>()
                            .Single(static candidate => candidate.Classes.Contains("semantic-indicator"));
        var dots = indicator.GetVisualDescendants()
                            .OfType<Ellipse>()
                            .Where(static dot => dot.Classes.Contains("SpinIndicatorDot"))
                            .ToArray();
        dots.Length.ShouldBe(4);
        foreach (var dot in dots)
        {
            dot.Fill.ShouldNotBeNull()
                .ShouldBeAssignableTo<ISolidColorBrush>()
                .Color.ShouldBe(expectedColor);
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
