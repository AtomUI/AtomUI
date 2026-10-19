using System.Collections;
using System.Reflection;
using AtomUI.Toolkits.GalleryBase.Controls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using ReactiveUI;
using Shouldly;
using Xunit;
using AvaloniaWindow = Avalonia.Controls.Window;
using AtomSlider = AtomUI.Desktop.Controls.Slider;
using AtomSliderThumb = AtomUI.Desktop.Controls.SliderThumb;
using SliderShowCase = AtomUIGallery.ShowCases.Slider.SliderShowCase;
using SliderViewModel = AtomUIGallery.ShowCases.Slider.SliderViewModel;

namespace AtomUIGallery.Tests.ShowCases;

public class SliderShowCasePageTests
{

    [Fact]
    public void Slider_ShowCase_Declares_The_Semantic_Previews_And_Style_Example()
    {
        var source = ReadRepoFile(
            "controlgallery/AtomUIGallery/ShowCases/DataEntry/Slider/Views/SliderShowCase.axaml");
        var english = ReadRepoFile(
            "controlgallery/AtomUIGallery/ShowCases/DataEntry/Slider/Localization/en-US.xlf");
        var semanticSource = ExtractShowCaseItemBySourceKey(source, "slider-semantic-part");

        source.ShouldContain("<gallery:GalleryShowCaseHost.SemanticPartsContentTemplate>");
        source.ShouldContain("Name=\"SliderSemanticPreview\"");
        source.ShouldContain("SemanticOwner=\"{Binding #SliderSemanticOwner}\"");
        source.ShouldContain("SemanticOwnerType=\"{x:Type atom:Slider}\"");
        source.ShouldContain("IsRangeMode=\"True\"");
        source.ShouldContain("RangeValues=\"{Binding SemanticPartPreviewRangeValues, Mode=OneWay}\"");
        CountOccurrences(source, "<gallery:SemanticPartDescription").ShouldBe(5);
        foreach (var path in new[] { "root", "track", "tracks", "rail", "handle" })
        {
            source.ShouldContain($"Path=\"{path}\"");
        }

        semanticSource.ShouldContain("SourceKey=\"slider-semantic-part\"");
        semanticSource.ShouldContain("SliderShowCaseLangResource SemanticPartStyleTitle");
        semanticSource.ShouldContain("SliderShowCaseLangResource SemanticPartStyleDescription");
        semanticSource.ShouldContain("Selector=\"atom|Slider.semantic-style-demo-h\"");
        semanticSource.ShouldContain("Selector=\"atom|Slider.semantic-style-demo-v\"");
        semanticSource.ShouldContain("Orientation=\"Vertical\"");
        semanticSource.ShouldContain("IsDirectionReversed=\"True\"");
        semanticSource.ShouldContain("Color=\"#91CAFF\"");
        semanticSource.ShouldContain("Value=\"#1677FF\"");
        semanticSource.ShouldContain("Color=\"#722CC0\"");
        semanticSource.ShouldContain("Value=\"#722ED1\"");
        semanticSource.ShouldContain("Property=\"Background\"");
        semanticSource.ShouldContain("Property=\"BorderBrush\"");
        semanticSource.ShouldContain("Property=\"OutlineBrush\"");
        semanticSource.ShouldContain("Property=\"OutlineThickness\"");
        semanticSource.ShouldContain("Property=\"Width\"");
        semanticSource.ShouldContain("Property=\"Height\"");
        CountOccurrences(semanticSource, "<atom:SliderTrackStyle").ShouldBe(2);
        CountOccurrences(semanticSource, "<atom:SliderHandleStyle").ShouldBe(2);
        semanticSource.ShouldContain("x:SetterTargetType=\"Border\"");
        semanticSource.ShouldContain("x:SetterTargetType=\"atom:SliderThumb\"");
        CountOccurrences(semanticSource, "<atom:Slider ").ShouldBe(2);
        semanticSource.ShouldNotContain("/template/");
        semanticSource.ShouldNotContain("classNames", Case.Insensitive);
        semanticSource.ShouldNotContain("semantic dom", Case.Insensitive);
        english.ShouldContain("Custom Semantic Part styling");
        english.ShouldContain(
            "Root element with relative positioning, height, margin, padding, cursor style and touch action control");
        english.ShouldContain(
            "Track selection bar element with absolute positioning, background color, border radius and transition animation styles");
        english.ShouldContain(
            "Multi-segment track container element with absolute positioning and transition animation styles");
        english.ShouldContain(
            "Background rail element with absolute positioning, background color, border radius and transition animation styles");
        english.ShouldContain(
            "Slider handle control element with absolute positioning, size, outline, user selection, background color, border shadow, border radius, cursor style and transition animation");
        english.ShouldNotContain("semantic dom", Case.Insensitive);
        english.ShouldNotContain("classNames", Case.Insensitive);
    }

    [Fact]
    public void Slider_Semantic_Previews_Are_Materialized_Only_After_The_Tab_Is_Selected()
    {
        AvaloniaTestApp.EnsureInitialized();

        var page = new SliderShowCase
        {
            DataContext = new SliderViewModel(new TestScreen())
        };

        ShowInWindow(page, 1280, 900, () =>
        {
            page.GetVisualDescendants().OfType<SemanticPartPreview>().ShouldBeEmpty();
            page.GetVisualDescendants()
                .OfType<AtomSlider>()
                .ShouldNotContain(static slider => slider.Name == "SliderSemanticOwner");

            var host = page.GetVisualDescendants().OfType<GalleryShowCaseHost>().Single();
            host.SelectedTab = GalleryShowCaseTab.SemanticParts;
            Dispatcher.UIThread.RunJobs();

            page.GetVisualDescendants().OfType<SemanticPartPreview>().ShouldHaveSingleItem();
            var preview = page.GetVisualDescendants()
                              .OfType<SemanticPartPreview>()
                              .Single(static candidate => candidate.Name == "SliderSemanticPreview");
            var semanticSlider = preview.SemanticOwner.ShouldBeOfType<AtomSlider>();
            semanticSlider.Name.ShouldBe("SliderSemanticOwner");
            semanticSlider.RangeValues.ShouldBe([20d, 30d, 50d]);
            semanticSlider.GetVisualDescendants()
                          .OfType<Border>()
                          .ShouldContain(static rail => rail.Classes.Contains("semantic-rail"));
            semanticSlider.GetVisualDescendants()
                          .OfType<Border>()
                          .ShouldContain(static tracks => tracks.Classes.Contains("semantic-tracks"));
            semanticSlider.GetVisualDescendants()
                          .OfType<Border>()
                          .Count(static track => track.Classes.Contains("semantic-track"))
                          .ShouldBe(2);
            semanticSlider.GetVisualDescendants()
                          .OfType<AtomSliderThumb>()
                          .Count(static handle => handle.Classes.Contains("semantic-handle"))
                          .ShouldBe(3);

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
    public void Slider_Semantic_Style_Example_Applies_The_Official_Style_Values()
    {
        AvaloniaTestApp.EnsureInitialized();

        var page = new SliderShowCase
        {
            DataContext = new SliderViewModel(new TestScreen())
        };

        ShowInWindow(page, 1280, 900, () =>
        {
            var panel = page.GetVisualDescendants().OfType<ShowCasePanel>().Single();
            var item = panel.Children
                            .OfType<ShowCaseItem>()
                            .Single(static candidate => candidate.SourceKey == "slider-semantic-part");
            item.MaterializeDeferredContent();
            Dispatcher.UIThread.RunJobs();

            var demos = page.GetVisualDescendants()
                            .OfType<AtomSlider>()
                            .Where(static slider => slider.Classes.Contains("semantic-style-demo-h") ||
                                                    slider.Classes.Contains("semantic-style-demo-v"))
                            .ToArray();
            demos.Length.ShouldBe(2);

            var horizontalDemo = demos.Single(static slider => slider.Classes.Contains("semantic-style-demo-h"));
            horizontalDemo.Width.ShouldBe(300);
            horizontalDemo.Value.ShouldBe(30);
            AssertGradient(FindPart(horizontalDemo, "semantic-track"), "#91CAFF", "#1677FF");
            AssertSolidColor(FindHandle(horizontalDemo).BorderBrush, "#1677FF");

            var verticalDemo = demos.Single(static slider => slider.Classes.Contains("semantic-style-demo-v"));
            verticalDemo.Width.ShouldBe(100);
            verticalDemo.Height.ShouldBe(300);
            verticalDemo.Orientation.ShouldBe(Avalonia.Layout.Orientation.Vertical);
            verticalDemo.IsDirectionReversed.ShouldBe(true);
            AssertGradient(FindPart(verticalDemo, "semantic-track"), "#722CC0", "#722ED1");
            var verticalHandle = FindHandle(verticalDemo);
            AssertSolidColor(verticalHandle.BorderBrush, "#722ED1");
            verticalHandle.GetValue(AtomSliderThumb.OutlineThicknessProperty).ShouldBe(new Thickness(2));
            AssertSolidColor(verticalHandle.GetValue(AtomSliderThumb.OutlineBrushProperty), "#722ED1");
        });
    }

    [Fact]
    public void Disabled_Handle_ViewModel_Replaces_DisabledHandles_Snapshot()
    {
        var viewModel = new SliderViewModel(null!);
        var original = viewModel.DisabledHandles;
        var changedProperties = new List<string?>();
        viewModel.PropertyChanged += (_, args) => changedProperties.Add(args.PropertyName);

        viewModel.IsHandle2Disabled = true;

        viewModel.DisabledHandleRangeValues.ShouldBe([20, 50, 80]);
        viewModel.DisabledHandles.ShouldBe([false, true, false]);
        viewModel.DisabledHandles.ShouldNotBeSameAs(original);
        changedProperties.ShouldContain(nameof(SliderViewModel.IsHandle2Disabled));
        changedProperties.ShouldContain(nameof(SliderViewModel.DisabledHandles));
    }

    private static Border FindPart(AtomSlider owner, string semanticClass)
    {
        return owner.GetVisualDescendants()
                    .OfType<Border>()
                    .Single(border => border.Classes.Contains(semanticClass));
    }

    private static AtomSliderThumb FindHandle(AtomSlider owner)
    {
        return owner.GetVisualDescendants()
                    .OfType<AtomSliderThumb>()
                    .Single(static handle => handle.Classes.Contains("semantic-handle"));
    }

    private static void AssertGradient(Border part, string startColor, string endColor)
    {
        var gradient = part.Background.ShouldNotBeNull()
                                .ShouldBeAssignableTo<LinearGradientBrush>();
        gradient.GradientStops.Count.ShouldBe(2);
        gradient.GradientStops[0].Color.ShouldBe(Color.Parse(startColor));
        gradient.GradientStops[1].Color.ShouldBe(Color.Parse(endColor));
    }

    private static void AssertSolidColor(IBrush? actual, string expected)
    {
        actual.ShouldNotBeNull()
              .ShouldBeAssignableTo<ISolidColorBrush>()
              .Color.ShouldBe(Color.Parse(expected));
    }

    private static string ExtractSliderExampleItems(string source)
    {
        const string firstItemMarker  = "<gallery:ShowCaseItem";
        const string semanticItemMarker = "SourceKey=\"slider-semantic-part\"";

        var firstItemStart = source.IndexOf(firstItemMarker, StringComparison.Ordinal);
        firstItemStart.ShouldBeGreaterThanOrEqualTo(0);

        var semanticItemStart = source.IndexOf(semanticItemMarker, firstItemStart, StringComparison.Ordinal);
        semanticItemStart.ShouldBeGreaterThan(firstItemStart);
        var semanticItemStartTag = source.LastIndexOf(firstItemMarker, semanticItemStart, StringComparison.Ordinal);
        semanticItemStartTag.ShouldBeGreaterThan(firstItemStart);

        return source[firstItemStart..semanticItemStartTag];
    }

    private static string ExtractShowCaseItemBySourceKey(string source, string sourceKey)
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

    private static string ExtractShowCaseItem(string source, string titleResource)
    {
        var titleMarker = $"Title=\"{{gallery:SliderShowCaseLangResource {titleResource}}}\"";
        var titleIndex = source.IndexOf(titleMarker, StringComparison.Ordinal);
        titleIndex.ShouldBeGreaterThanOrEqualTo(0);

        var itemStart = source.LastIndexOf("<gallery:ShowCaseItem", titleIndex, StringComparison.Ordinal);
        itemStart.ShouldBeGreaterThanOrEqualTo(0);

        const string itemCloseMarker = "</gallery:ShowCaseItem>";
        var itemClose = source.IndexOf(itemCloseMarker, titleIndex, StringComparison.Ordinal);
        itemClose.ShouldBeGreaterThan(titleIndex);
        return source[itemStart..(itemClose + itemCloseMarker.Length)];
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
