using System.Collections;
using System.Reflection;
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
using AtomCheckBox = AtomUI.Desktop.Controls.CheckBox;
using CheckBoxShowCase = AtomUIGallery.ShowCases.CheckBox.CheckBoxShowCase;
using CheckBoxViewModel = AtomUIGallery.ShowCases.CheckBox.CheckBoxViewModel;

namespace AtomUIGallery.Tests.ShowCases;

public class CheckBoxShowCasePageTests
{

    [Fact]
    public void CheckBox_ShowCase_Declares_The_Semantic_Previews_And_Style_Example()
    {
        var source = ReadRepoFile(
            "controlgallery/AtomUIGallery/ShowCases/DataEntry/CheckBox/Views/CheckBoxShowCase.axaml");
        var english = ReadRepoFile(
            "controlgallery/AtomUIGallery/ShowCases/DataEntry/CheckBox/Localization/en-US.xlf");
        var semanticSource = ExtractShowCaseItem(source, "checkbox-semantic-part");

        source.ShouldContain("<gallery:GalleryShowCaseHost.SemanticPartsContentTemplate>");
        source.ShouldContain("Name=\"CheckBoxSemanticPreview\"");
        source.ShouldContain("SemanticOwner=\"{Binding #CheckBoxSemanticOwner}\"");
        source.ShouldContain("SemanticOwnerType=\"{x:Type atom:CheckBox}\"");
        CountOccurrences(source, "<gallery:SemanticPartDescription").ShouldBe(3);
        foreach (var path in new[] { "root", "icon", "label" })
        {
            source.ShouldContain($"Path=\"{path}\"");
        }

        semanticSource.ShouldContain("SourceKey=\"checkbox-semantic-part\"");
        semanticSource.ShouldContain("CheckBoxShowCaseLangResource SemanticPartStyleTitle");
        semanticSource.ShouldContain("CheckBoxShowCaseLangResource SemanticPartStyleDescription");
        semanticSource.ShouldContain("Selector=\"atom|CheckBox.semantic-fixed\"");
        semanticSource.ShouldContain("Selector=\"atom|CheckBox.semantic-conditional\"");
        semanticSource.ShouldContain("Selector=\"atom|CheckBox.semantic-conditional:checked\"");
        semanticSource.ShouldContain("Value=\"#faad14\"");
        semanticSource.ShouldContain("Value=\"#1677ff\"");
        semanticSource.ShouldContain("Value=\"6\"");
        semanticSource.ShouldContain("Value=\"Bold\"");
        semanticSource.ShouldContain("Property=\"CornerRadius\"");
        semanticSource.ShouldContain("Property=\"BorderBrush\"");
        semanticSource.ShouldContain("Property=\"Background\"");
        semanticSource.ShouldContain("Property=\"Foreground\"");
        semanticSource.ShouldContain("Property=\"FontWeight\"");
        CountOccurrences(semanticSource, "<atom:CheckBoxIconStyle").ShouldBe(3);
        CountOccurrences(semanticSource, "<atom:CheckBoxLabelStyle").ShouldBe(2);
        semanticSource.ShouldContain("x:SetterTargetType=\"TemplatedControl\"");
        semanticSource.ShouldContain("x:SetterTargetType=\"ContentPresenter\"");
        CountOccurrences(semanticSource, "<atom:CheckBox ").ShouldBe(2);
        semanticSource.ShouldNotContain("/template/");
        semanticSource.ShouldNotContain("classNames", Case.Insensitive);
        semanticSource.ShouldNotContain("semantic dom", Case.Insensitive);
        english.ShouldContain("Custom Semantic Part styling");
        english.ShouldContain("Root element with inline-flex layout, baseline alignment, cursor style, reset styles and other basic checkbox container styles");
        english.ShouldContain("Checkbox icon element with size, direction, background, border, border-radius, transitions, and checked state checkmark styles");
        english.ShouldContain("Label text element with padding and spacing styles relative to the checkbox");
        english.ShouldNotContain("semantic dom", Case.Insensitive);
        english.ShouldNotContain("classNames", Case.Insensitive);
    }

    [Fact]
    public void CheckBox_Semantic_Previews_Are_Materialized_Only_After_The_Tab_Is_Selected()
    {
        AvaloniaTestApp.EnsureInitialized();

        var page = new CheckBoxShowCase
        {
            DataContext = new CheckBoxViewModel(new TestScreen())
        };

        ShowInWindow(page, 1280, 900, () =>
        {
            page.GetVisualDescendants().OfType<SemanticPartPreview>().ShouldBeEmpty();
            page.GetVisualDescendants()
                .OfType<AtomCheckBox>()
                .ShouldNotContain(static checkBox => checkBox.Name == "CheckBoxSemanticOwner");

            var host = page.GetVisualDescendants().OfType<GalleryShowCaseHost>().Single();
            host.SelectedTab = GalleryShowCaseTab.SemanticParts;
            Dispatcher.UIThread.RunJobs();

            page.GetVisualDescendants().OfType<SemanticPartPreview>().ShouldHaveSingleItem();
            var preview = page.GetVisualDescendants()
                              .OfType<SemanticPartPreview>()
                              .Single(static candidate => candidate.Name == "CheckBoxSemanticPreview");
            var semanticCheckBox = preview.SemanticOwner.ShouldBeOfType<AtomCheckBox>();
            semanticCheckBox.Name.ShouldBe("CheckBoxSemanticOwner");
            semanticCheckBox.GetVisualDescendants()
                            .OfType<TemplatedControl>()
                            .Single(static icon => icon.Classes.Contains("semantic-icon"))
                            .IsEffectivelyVisible.ShouldBeTrue();
            semanticCheckBox.GetVisualDescendants()
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
    public void CheckBox_Semantic_Style_Example_Applies_The_Official_Style_Values()
    {
        AvaloniaTestApp.EnsureInitialized();

        var page = new CheckBoxShowCase
        {
            DataContext = new CheckBoxViewModel(new TestScreen())
        };

        ShowInWindow(page, 1280, 900, () =>
        {
            var panel = page.GetVisualDescendants().OfType<ShowCasePanel>().Single();
            var item = panel.Children
                            .OfType<ShowCaseItem>()
                            .Single(static candidate => candidate.SourceKey == "checkbox-semantic-part");
            item.MaterializeDeferredContent();
            Dispatcher.UIThread.RunJobs();

            var demos = page.GetVisualDescendants()
                            .OfType<AtomCheckBox>()
                            .Where(static checkBox => checkBox.Classes.Contains("semantic-fixed") ||
                                                      checkBox.Classes.Contains("semantic-conditional"))
                            .ToArray();
            demos.Length.ShouldBe(2);

            var fixedDemo = demos.Single(static checkBox => checkBox.Classes.Contains("semantic-fixed"));
            fixedDemo.IsChecked.ShouldBe(false);
            var fixedIcon = FindIcon(fixedDemo);
            fixedIcon.CornerRadius.ShouldBe(new CornerRadius(6));
            AssertSolidColor(FindLabel(fixedDemo).Foreground, "#1677ff");

            var conditionalDemo = demos.Single(static checkBox => checkBox.Classes.Contains("semantic-conditional"));
            conditionalDemo.IsChecked.ShouldBe(true);
            var conditionalIcon = FindIcon(conditionalDemo);
            AssertSolidColor(conditionalIcon.BorderBrush, "#faad14");
            AssertSolidColor(conditionalIcon.Background, "#faad14");
            var conditionalLabel = FindLabel(conditionalDemo);
            AssertSolidColor(conditionalLabel.Foreground, "#faad14");
            conditionalLabel.FontWeight.ShouldBe(FontWeight.Bold);
        });
    }

    private static string ExtractCheckBoxExampleItems(string source)
    {
        const string firstItemMarker = "<gallery:ShowCaseItem";
        var firstItemStart = source.IndexOf(firstItemMarker, StringComparison.Ordinal);
        firstItemStart.ShouldBeGreaterThanOrEqualTo(0);

        const string semanticItemMarker = "SourceKey=\"checkbox-semantic-part\"";
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

    private static TemplatedControl FindIcon(AtomCheckBox owner)
    {
        return owner.GetVisualDescendants()
                    .OfType<TemplatedControl>()
                    .Single(static icon => icon.Classes.Contains("semantic-icon"));
    }

    private static ContentPresenter FindLabel(AtomCheckBox owner)
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
