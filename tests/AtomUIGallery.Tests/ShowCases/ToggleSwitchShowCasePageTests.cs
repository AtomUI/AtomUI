using System.Collections;
using System.Reflection;
using AtomUI;
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
using AtomToggleSwitch = AtomUI.Desktop.Controls.ToggleSwitch;
using ToggleSwitchShowCase = AtomUIGallery.ShowCases.ToggleSwitch.ToggleSwitchShowCase;
using ToggleSwitchViewModel = AtomUIGallery.ShowCases.ToggleSwitch.ToggleSwitchViewModel;

namespace AtomUIGallery.Tests.ShowCases;

public class ToggleSwitchShowCasePageTests
{

    [Fact]
    public void ToggleSwitch_ShowCase_Declares_The_Semantic_Previews_And_Style_Example()
    {
        var source = ReadRepoFile(
            "controlgallery/AtomUIGallery/ShowCases/DataEntry/ToggleSwitch/Views/ToggleSwitchShowCase.axaml");
        var english = ReadRepoFile(
            "controlgallery/AtomUIGallery/ShowCases/DataEntry/ToggleSwitch/Localization/en-US.xlf");
        var semanticSource = ExtractShowCaseItem(source, "toggleswitch-semantic-part");

        source.ShouldContain("<gallery:GalleryShowCaseHost.SemanticPartsContentTemplate>");
        source.ShouldContain("Name=\"ToggleSwitchSemanticPreview\"");
        source.ShouldContain("SemanticOwner=\"{Binding #ToggleSwitchSemanticOwner}\"");
        source.ShouldContain("SemanticOwnerType=\"{x:Type atom:ToggleSwitch}\"");
        source.ShouldContain("OnContent=\"ON\"");
        source.ShouldContain("OffContent=\"OFF\"");
        source.ShouldContain("IsChecked=\"True\"");
        CountOccurrences(source, "<gallery:SemanticPartDescription").ShouldBe(3);
        foreach (var path in new[] { "root", "content", "indicator" })
        {
            source.ShouldContain($"Path=\"{path}\"");
        }

        semanticSource.ShouldContain("SourceKey=\"toggleswitch-semantic-part\"");
        semanticSource.ShouldContain("ToggleSwitchShowCaseLangResource SemanticPartStyleTitle");
        semanticSource.ShouldContain("ToggleSwitchShowCaseLangResource SemanticPartStyleDescription");
        semanticSource.ShouldContain("Selector=\"atom|ToggleSwitch.semantic-object\"");
        semanticSource.ShouldContain("Selector=\"atom|ToggleSwitch.semantic-function\"");
        CountOccurrences(semanticSource, "Selector=\"atom|ToggleSwitch.semantic-mui\"").ShouldBe(1);
        semanticSource.ShouldContain("Selector=\"atom|ToggleSwitch.semantic-mui:checked\"");
        semanticSource.ShouldContain("Property=\"GrooveBackground\"");
        semanticSource.ShouldContain("Value=\"#F5D2D2\"");
        semanticSource.ShouldContain("Value=\"#BDE3C3\"");
        semanticSource.ShouldContain("Value=\"#801976d2\"");
        semanticSource.ShouldContain("Value=\"#1976d2\"");
        semanticSource.ShouldContain("Property=\"Background\"");
        semanticSource.ShouldContain("Property=\"TrackHeight\"");
        semanticSource.ShouldContain("Value=\"14\"");
        semanticSource.ShouldContain("Property=\"TrackMinWidth\"");
        semanticSource.ShouldContain("Value=\"32\"");
        semanticSource.ShouldContain("Property=\"TrackPadding\"");
        semanticSource.ShouldContain("Value=\"-3\"");
        semanticSource.ShouldContain("Property=\"KnobSize\"");
        semanticSource.ShouldContain("Value=\"20,20\"");
        semanticSource.ShouldContain("Property=\"Effect\"");
        semanticSource.ShouldContain("<DropShadowEffect");
        semanticSource.ShouldContain("SizeType=\"Small\"");
        semanticSource.ShouldContain("Width=\"40\"");
        CountOccurrences(semanticSource, "<atom:ToggleSwitchContentStyle").ShouldBe(0);
        CountOccurrences(semanticSource, "<atom:ToggleSwitchIndicatorStyle").ShouldBe(1);
        semanticSource.ShouldContain("x:SetterTargetType=\"TemplatedControl\"");
        CountOccurrences(semanticSource, "<atom:ToggleSwitch ").ShouldBe(3);
        semanticSource.ShouldNotContain("/template/");
        semanticSource.ShouldNotContain("classNames", Case.Insensitive);
        semanticSource.ShouldNotContain("semantic dom", Case.Insensitive);
        english.ShouldContain("Custom Semantic Part styling");
        english.ShouldContain("Root element with min-width, height, line-height, vertical alignment, background color, border, border radius, cursor style, transition animations, user selection and other basic switch container styles");
        english.ShouldContain("Content element with block display, overflow hidden, border radius, height, padding, transition animations and other switch content area layout and styles");
        english.ShouldContain("Indicator element with absolute positioning, width, height, background color, border radius, shadow, transition animations and other switch handle styles and interactive effects");
        english.ShouldNotContain("semantic dom", Case.Insensitive);
        english.ShouldNotContain("classNames", Case.Insensitive);
    }

    [Fact]
    public void ToggleSwitch_Semantic_Previews_Are_Materialized_Only_After_The_Tab_Is_Selected()
    {
        AvaloniaTestApp.EnsureInitialized();

        var page = new ToggleSwitchShowCase
        {
            DataContext = new ToggleSwitchViewModel(new TestScreen())
        };

        ShowInWindow(page, 1280, 900, () =>
        {
            page.GetVisualDescendants().OfType<SemanticPartPreview>().ShouldBeEmpty();
            page.GetVisualDescendants()
                .OfType<AtomToggleSwitch>()
                .ShouldNotContain(static toggleSwitch => toggleSwitch.Name == "ToggleSwitchSemanticOwner");

            var host = page.GetVisualDescendants().OfType<GalleryShowCaseHost>().Single();
            host.SelectedTab = GalleryShowCaseTab.SemanticParts;
            Dispatcher.UIThread.RunJobs();

            page.GetVisualDescendants().OfType<SemanticPartPreview>().ShouldHaveSingleItem();
            var preview = page.GetVisualDescendants()
                              .OfType<SemanticPartPreview>()
                              .Single(static candidate => candidate.Name == "ToggleSwitchSemanticPreview");
            var semanticSwitch = preview.SemanticOwner.ShouldBeOfType<AtomToggleSwitch>();
            semanticSwitch.Name.ShouldBe("ToggleSwitchSemanticOwner");
            semanticSwitch.GetVisualDescendants()
                          .OfType<TemplatedControl>()
                          .Single(static indicator => indicator.Classes.Contains("semantic-indicator"))
                          .IsEffectivelyVisible.ShouldBeTrue();
            semanticSwitch.GetVisualDescendants()
                          .OfType<ContentPresenter>()
                          .Count(static content => content.Classes.Contains("semantic-content"))
                          .ShouldBe(2);

            var previewItemsValue = typeof(SemanticPartPreview)
                                    .GetProperty("Items", BindingFlags.Instance | BindingFlags.NonPublic)
                                    ?.GetValue(preview);
            previewItemsValue.ShouldNotBeNull();
            var previewItems = previewItemsValue.ShouldBeAssignableTo<IEnumerable>()
                                                .Cast<object>()
                                                .ToArray();
            previewItems.Length.ShouldBe(3);

            host.SelectedTab = GalleryShowCaseTab.Examples;
            Dispatcher.UIThread.RunJobs();
            Assert.All(page.GetVisualDescendants().OfType<SemanticPartPreview>(), preview => Assert.False(preview.IsEffectivelyVisible));
        });
    }

    [Fact]
    public void ToggleSwitch_Semantic_Style_Example_Applies_The_Official_Style_Values()
    {
        AvaloniaTestApp.EnsureInitialized();

        var page = new ToggleSwitchShowCase
        {
            DataContext = new ToggleSwitchViewModel(new TestScreen())
        };

        ShowInWindow(page, 1280, 900, () =>
        {
            var panel = page.GetVisualDescendants().OfType<ShowCasePanel>().Single();
            var item = panel.Children
                            .OfType<ShowCaseItem>()
                            .Single(static candidate => candidate.SourceKey == "toggleswitch-semantic-part");
            item.MaterializeDeferredContent();
            Dispatcher.UIThread.RunJobs();

            var demos = page.GetVisualDescendants()
                            .OfType<AtomToggleSwitch>()
                            .Where(static toggle => toggle.Classes.Contains("semantic-object") ||
                                                    toggle.Classes.Contains("semantic-function") ||
                                                    toggle.Classes.Contains("semantic-mui"))
                            .ToArray();
            demos.Length.ShouldBe(3);

            var objectDemo = demos.Single(static toggle => toggle.Classes.Contains("semantic-object"));
            AssertSolidColor(objectDemo.GrooveBackground, "#F5D2D2");
            objectDemo.SizeType.ShouldBe(CustomizableSizeType.Small);
            objectDemo.Bounds.Width.ShouldBe(40);

            var functionDemo = demos.Single(static toggle => toggle.Classes.Contains("semantic-function"));
            AssertSolidColor(functionDemo.GrooveBackground, "#BDE3C3");

            var muiDemo = demos.Single(static toggle => toggle.Classes.Contains("semantic-mui"));
            muiDemo.IsChecked.ShouldBe(true);
            AssertSolidColor(muiDemo.GrooveBackground, "#801976d2");
            muiDemo.Bounds.Size.ShouldBe(new Size(32, 14));
            var muiIndicator = muiDemo.GetVisualDescendants()
                                      .OfType<TemplatedControl>()
                                      .Single(static knob => knob.Classes.Contains("semantic-indicator"));
            AssertSolidColor(muiIndicator.Background, "#1976d2");
            muiIndicator.Bounds.ShouldBe(new Rect(15, -3, 20, 20));
            muiIndicator.Effect.ShouldNotBeNull()
                          .ShouldBeAssignableTo<DropShadowEffect>()
                          .Color.ShouldBe(Color.Parse("#4D000000"));
            muiIndicator.GetVisualAncestors()
                        .TakeWhile(visual => !ReferenceEquals(visual, muiDemo))
                        .OfType<Canvas>()
                        .ShouldBeEmpty();
        });
    }

    [Fact]
    public void ToggleSwitch_ShowCase_Toggle_Action_Buttons_Do_Not_Disable_Themselves_During_Click()
    {
        var source          = ReadRepoFile("controlgallery/AtomUIGallery/ShowCases/DataEntry/ToggleSwitch/Views/ToggleSwitchShowCase.axaml");
        var viewModelSource = ReadRepoFile("controlgallery/AtomUIGallery/ShowCases/DataEntry/ToggleSwitch/ViewModels/ToggleSwitchViewModel.cs");

        source.ShouldContain("Click=\"HandleToggleDisabledButtonClick\"");
        source.ShouldContain("Click=\"HandleToggleLoadingButtonClick\"");
        source.ShouldNotContain("Command=\"{Binding ToggleDisabledCommand}\"");
        source.ShouldNotContain("Command=\"{Binding ToggleLoadingCommand}\"");

        viewModelSource.ShouldNotContain("ToggleDisabledCommand");
        viewModelSource.ShouldNotContain("ToggleLoadingCommand");
    }

    private static string ExtractToggleSwitchExampleItems(string source)
    {
        const string firstItemMarker = "<gallery:ShowCaseItem";
        var firstItemStart = source.IndexOf(firstItemMarker, StringComparison.Ordinal);
        firstItemStart.ShouldBeGreaterThanOrEqualTo(0);

        const string semanticItemMarker = "SourceKey=\"toggleswitch-semantic-part\"";
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
