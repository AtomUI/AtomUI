using System.Collections;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
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
using AtomRadioButton = AtomUI.Desktop.Controls.RadioButton;
using RadioButtonShowCase = AtomUIGallery.ShowCases.RadioButton.RadioButtonShowCase;
using RadioButtonViewModel = AtomUIGallery.ShowCases.RadioButton.RadioButtonViewModel;

namespace AtomUIGallery.Tests.ShowCases;

public class RadioButtonShowCasePageTests
{

    [Fact]
    public void RadioButton_ShowCase_Declares_The_Semantic_Previews_And_Style_Example()
    {
        var source = ReadRepoFile(
            "controlgallery/AtomUIGallery/ShowCases/DataEntry/RadioButton/Views/RadioButtonShowCase.axaml");
        var english = ReadRepoFile(
            "controlgallery/AtomUIGallery/ShowCases/DataEntry/RadioButton/Localization/en-US.xlf");
        var semanticSource = ExtractShowCaseItem(source, "radio-button-semantic-part");

        source.ShouldContain("<gallery:GalleryShowCaseHost.SemanticPartsContentTemplate>");
        source.ShouldContain("Name=\"RadioButtonSemanticPreview\"");
        source.ShouldContain("SemanticOwner=\"{Binding #RadioButtonSemanticOwner}\"");
        source.ShouldContain("SemanticOwnerType=\"{x:Type atom:RadioButton}\"");
        CountOccurrences(source, "<gallery:SemanticPartDescription").ShouldBe(3);
        foreach (var path in new[] { "root", "icon", "label" })
        {
            source.ShouldContain($"Path=\"{path}\"");
        }

        semanticSource.ShouldContain("SourceKey=\"radio-button-semantic-part\"");
        semanticSource.ShouldContain("RadioButtonShowCaseLangResource SemanticPartStyleTitle");
        semanticSource.ShouldContain("RadioButtonShowCaseLangResource SemanticPartStyleDescription");
        semanticSource.ShouldContain("Selector=\"atom|RadioButton.semantic-fixed\"");
        semanticSource.ShouldContain("Selector=\"atom|RadioButton.semantic-conditional\"");
        semanticSource.ShouldContain("Selector=\"atom|RadioButton.semantic-conditional:pointerover\"");
        semanticSource.ShouldContain("Selector=\"atom|RadioButton.semantic-conditional:checked\"");
        semanticSource.ShouldContain("Value=\"#faad14\"");
        semanticSource.ShouldContain("Value=\"#1677ff\"");
        semanticSource.ShouldContain("Value=\"Bold\"");
        semanticSource.ShouldContain("Value=\"6\"");
        semanticSource.ShouldContain("ColorTextDisabled");
        semanticSource.ShouldContain("Property=\"CornerRadius\"");
        semanticSource.ShouldContain("Property=\"BorderBrush\"");
        semanticSource.ShouldContain("Property=\"Background\"");
        semanticSource.ShouldContain("Property=\"Foreground\"");
        semanticSource.ShouldContain("Property=\"FontWeight\"");
        CountOccurrences(semanticSource, "<atom:RadioButtonIconStyle").ShouldBe(4);
        CountOccurrences(semanticSource, "<atom:RadioButtonLabelStyle").ShouldBe(3);
        semanticSource.ShouldContain("x:SetterTargetType=\"TemplatedControl\"");
        semanticSource.ShouldContain("x:SetterTargetType=\"ContentPresenter\"");
        CountOccurrences(semanticSource, "<atom:RadioButton ").ShouldBe(2);
        semanticSource.ShouldContain("<atom:RadioButtonGroup Orientation=\"Vertical\"");
        semanticSource.ShouldNotContain("/template/");
        semanticSource.ShouldNotContain("classNames", Case.Insensitive);
        semanticSource.ShouldNotContain("semantic dom", Case.Insensitive);
        english.ShouldContain("Custom Semantic Part styling");
        english.ShouldContain("Root element with layout styles, cursor styles, disabled text color and other basic container styles");
        english.ShouldContain("Icon element with border radius, transition animations, border styles, hover states, focus states and other interactive styles");
        english.ShouldContain("Label element with padding, text color, disabled states, alignment and other text styles");
        english.ShouldNotContain("semantic dom", Case.Insensitive);
        english.ShouldNotContain("classNames", Case.Insensitive);
    }

    [Fact]
    public void RadioButton_Semantic_Previews_Are_Materialized_Only_After_The_Tab_Is_Selected()
    {
        AvaloniaTestApp.EnsureInitialized();

        var page = new RadioButtonShowCase
        {
            DataContext = new RadioButtonViewModel(new TestScreen())
        };

        ShowInWindow(page, 1280, 900, () =>
        {
            page.GetVisualDescendants().OfType<SemanticPartPreview>().ShouldBeEmpty();
            page.GetVisualDescendants()
                .OfType<AtomRadioButton>()
                .ShouldNotContain(static radioButton => radioButton.Name == "RadioButtonSemanticOwner");

            var host = page.GetVisualDescendants().OfType<GalleryShowCaseHost>().Single();
            host.SelectedTab = GalleryShowCaseTab.SemanticParts;
            Dispatcher.UIThread.RunJobs();

            page.GetVisualDescendants().OfType<SemanticPartPreview>().ShouldHaveSingleItem();
            var preview = page.GetVisualDescendants()
                              .OfType<SemanticPartPreview>()
                              .Single(static candidate => candidate.Name == "RadioButtonSemanticPreview");
            var semanticRadioButton = preview.SemanticOwner.ShouldBeOfType<AtomRadioButton>();
            semanticRadioButton.Name.ShouldBe("RadioButtonSemanticOwner");
            semanticRadioButton.GetVisualDescendants()
                               .OfType<TemplatedControl>()
                               .Single(static icon => icon.Classes.Contains("semantic-icon"))
                               .IsEffectivelyVisible.ShouldBeTrue();
            semanticRadioButton.GetVisualDescendants()
                               .OfType<ContentPresenter>()
                               .Single(static label => label.Classes.Contains("semantic-label"))
                               .IsEffectivelyVisible.ShouldBeTrue();

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
    public void RadioButton_Semantic_Style_Example_Applies_The_Official_Style_Values()
    {
        AvaloniaTestApp.EnsureInitialized();

        var page = new RadioButtonShowCase
        {
            DataContext = new RadioButtonViewModel(new TestScreen())
        };

        ShowInWindow(page, 1280, 900, () =>
        {
            var panel = page.GetVisualDescendants().OfType<ShowCasePanel>().Single();
            var item = panel.Children
                            .OfType<ShowCaseItem>()
                            .Single(static candidate => candidate.SourceKey == "radio-button-semantic-part");
            item.MaterializeDeferredContent();
            Dispatcher.UIThread.RunJobs();

            var demos = page.GetVisualDescendants()
                            .OfType<AtomRadioButton>()
                            .Where(static radioButton => radioButton.Classes.Contains("semantic-fixed") ||
                                                         radioButton.Classes.Contains("semantic-conditional"))
                            .ToArray();
            demos.Length.ShouldBe(2);

            var fixedDemo = demos.Single(static radioButton => radioButton.Classes.Contains("semantic-fixed"));
            fixedDemo.IsChecked.ShouldBe(true);
            FindIcon(fixedDemo).CornerRadius.ShouldBe(new CornerRadius(6));
            AssertSolidColor(FindLabel(fixedDemo).Foreground, "#1677ff");

            var conditionalDemo = demos.Single(static radioButton => radioButton.Classes.Contains("semantic-conditional"));
            conditionalDemo.IsChecked.ShouldBe(false);
            AssertSolidColor(FindIcon(conditionalDemo).BorderBrush, "#faad14");
            FindLabel(conditionalDemo).FontWeight.ShouldBe(FontWeight.Bold);

            conditionalDemo.IsMotionEnabled = false;
            conditionalDemo.IsChecked = true;
            Dispatcher.UIThread.RunJobs();

            fixedDemo.IsChecked.ShouldBe(false);
            conditionalDemo.IsChecked.ShouldBe(true);
            AssertSolidColor(FindIcon(conditionalDemo).BorderBrush, "#faad14");
            AssertSolidColor(FindIcon(conditionalDemo).Background, "#faad14");
            AssertSolidColor(FindLabel(conditionalDemo).Foreground, "#faad14");
            FindLabel(conditionalDemo).FontWeight.ShouldBe(FontWeight.Bold);
        });
    }

    private static string ExtractRadioButtonExampleItems(string source)
    {
        const string firstItemMarker  = "<gallery:ShowCaseItem";
        const string panelCloseMarker = "</gallery:ShowCasePanel>";

        var firstItemStart = source.IndexOf(firstItemMarker, StringComparison.Ordinal);
        firstItemStart.ShouldBeGreaterThanOrEqualTo(0);

        var panelCloseStart = source.IndexOf(panelCloseMarker, firstItemStart, StringComparison.Ordinal);
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

    private static string ComputeSha256(string source)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(source));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    private static string ReadSnapshotHash(string source)
    {
        return source
            .Split('\n')
            .First(line => line.StartsWith("sha256:", StringComparison.Ordinal))
            .Split(':', 2)[1]
            .Trim();
    }

    private static int ReadSnapshotCount(string source)
    {
        return int.Parse(source
            .Split('\n')
            .First(line => line.StartsWith("count:", StringComparison.Ordinal))
            .Split(':', 2)[1]
            .Trim());
    }

    private static TemplatedControl FindIcon(AtomRadioButton owner)
    {
        return owner.GetVisualDescendants()
                    .OfType<TemplatedControl>()
                    .Single(static icon => icon.Classes.Contains("semantic-icon"));
    }

    private static ContentPresenter FindLabel(AtomRadioButton owner)
    {
        return owner.GetVisualDescendants()
                    .OfType<ContentPresenter>()
                    .Single(static label => label.Classes.Contains("semantic-label"));
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
