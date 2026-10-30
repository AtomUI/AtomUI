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
using AtomUIExpandIcon = AtomUI.Desktop.Controls.IconButton;
using AtomUIExpander = AtomUI.Desktop.Controls.Expander;
using ExpanderShowCase = AtomUIGallery.ShowCases.Expander.ExpanderShowCase;
using ExpanderViewModel = AtomUIGallery.ShowCases.Expander.ExpanderViewModel;

namespace AtomUIGallery.Tests.ShowCases;

public class ExpanderShowCasePageTests
{

    [Fact]
    public void Expander_ShowCase_Declares_The_Semantic_Previews_And_Style_Example()
    {
        var source = ReadRepoFile(
            "controlgallery/AtomUIGallery/ShowCases/DataDisplay/Expander/Views/ExpanderShowCase.axaml");
        var english = ReadRepoFile(
            "controlgallery/AtomUIGallery/ShowCases/DataDisplay/Expander/Localization/en-US.xlf");
        var semanticSource = ExtractShowCaseItem(source, "expander-semantic-part");

        source.ShouldContain("<gallery:GalleryShowCaseHost.SemanticPartsContentTemplate>");
        source.ShouldContain("Name=\"ExpanderSemanticPreview\"");
        source.ShouldContain("SemanticOwner=\"{Binding #ExpanderSemanticOwner}\"");
        source.ShouldContain("SemanticOwnerType=\"{x:Type atom:Expander}\"");
        CountOccurrences(source, "<gallery:SemanticPartDescription").ShouldBe(5);
        foreach (var path in new[] { "root", "header", "icon", "title", "body" })
        {
            source.ShouldContain($"Path=\"{path}\"");
        }

        semanticSource.ShouldContain("SourceKey=\"expander-semantic-part\"");
        semanticSource.ShouldContain("ExpanderShowCaseLangResource SemanticPartStyleTitle");
        semanticSource.ShouldContain("ExpanderShowCaseLangResource SemanticPartStyleDescription");
        semanticSource.ShouldContain("Selector=\"atom|Expander.semantic-object\"");
        semanticSource.ShouldContain("Selector=\"atom|Expander.semantic-function\"");
        semanticSource.ShouldContain("Value=\"#f0f0f0\"");
        semanticSource.ShouldContain("Value=\"#141414\"");
        semanticSource.ShouldContain("Value=\"#f5efff\"");
        semanticSource.ShouldContain("SizeType=\"Large\"");
        semanticSource.ShouldContain("Property=\"Background\"");
        semanticSource.ShouldContain("Property=\"Padding\"");
        semanticSource.ShouldContain("Property=\"Foreground\"");
        CountOccurrences(semanticSource, "<atom:ExpanderHeaderStyle").ShouldBe(2);
        CountOccurrences(semanticSource, "<atom:ExpanderTitleStyle").ShouldBe(2);
        CountOccurrences(semanticSource, "<atom:ExpanderBodyStyle").ShouldBe(1);
        CountOccurrences(semanticSource, "<atom:ExpanderIconStyle").ShouldBe(1);
        semanticSource.ShouldContain("x:SetterTargetType=\"atom:PixelAlignedBorder\"");
        semanticSource.ShouldContain("x:SetterTargetType=\"ContentPresenter\"");
        semanticSource.ShouldContain("x:SetterTargetType=\"atom:IconButton\"");
        CountOccurrences(semanticSource, "<atom:Expander ").ShouldBe(2);
        semanticSource.ShouldNotContain("/template/");
        semanticSource.ShouldNotContain("classNames", Case.Insensitive);
        semanticSource.ShouldNotContain("semantic dom", Case.Insensitive);
        english.ShouldContain("Custom Semantic Part styling");
        english.ShouldContain("Root element with border thickness, visual mode and expansion state that control the overall appearance of the panel");
        english.ShouldContain("Header element with padding, background color, font size, line height and cursor style for the panel header");
        english.ShouldContain("Icon element with icon size, placement, margin and rotation transforms for the expand/collapse arrow");
        english.ShouldContain("Title element with layout, foreground color and typography styles for the header text");
        english.ShouldContain("Body element with padding, background color and text styles for the panel content area; it materializes on first expansion.");
        english.ShouldNotContain("semantic dom", Case.Insensitive);
        english.ShouldNotContain("classNames", Case.Insensitive);
    }

    [Fact]
    public void Expander_Semantic_Previews_Are_Materialized_Only_After_The_Tab_Is_Selected()
    {
        AvaloniaTestApp.EnsureInitialized();

        var page = new ExpanderShowCase
        {
            DataContext = new ExpanderViewModel(new TestScreen())
        };

        ShowInWindow(page, 1280, 900, () =>
        {
            page.GetVisualDescendants().OfType<SemanticPartPreview>().ShouldBeEmpty();
            page.GetVisualDescendants()
                .OfType<AtomUIExpander>()
                .ShouldNotContain(static expander => expander.Name == "ExpanderSemanticOwner");

            var host = page.GetVisualDescendants().OfType<GalleryShowCaseHost>().Single();
            host.SelectedTab = GalleryShowCaseTab.SemanticParts;
            Dispatcher.UIThread.RunJobs();

            page.GetVisualDescendants().OfType<SemanticPartPreview>().ShouldHaveSingleItem();
            var preview = page.GetVisualDescendants()
                              .OfType<SemanticPartPreview>()
                              .Single(static candidate => candidate.Name == "ExpanderSemanticPreview");
            var semanticExpander = preview.SemanticOwner.ShouldBeOfType<AtomUIExpander>();
            semanticExpander.Name.ShouldBe("ExpanderSemanticOwner");
            semanticExpander.IsExpanded.ShouldBeTrue();
            semanticExpander.GetVisualDescendants()
                            .OfType<Control>()
                            .Single(static header => header.Classes.Contains("semantic-header"))
                            .IsEffectivelyVisible.ShouldBeTrue();
            // 预览中的 Expander 显式 IsExpanded=True，因此 Optional 的 body 节点已经物化。
            semanticExpander.GetVisualDescendants()
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
            Assert.All(page.GetVisualDescendants().OfType<SemanticPartPreview>(),
                       candidate => Assert.False(candidate.IsEffectivelyVisible));
        });
    }

    [Fact]
    public void Expander_Semantic_Style_Example_Applies_The_Official_Style_Values()
    {
        AvaloniaTestApp.EnsureInitialized();

        var page = new ExpanderShowCase
        {
            DataContext = new ExpanderViewModel(new TestScreen())
        };

        ShowInWindow(page, 1280, 900, () =>
        {
            var panel = page.GetVisualDescendants().OfType<ShowCasePanel>().Single();
            var item = panel.Children
                            .OfType<ShowCaseItem>()
                            .Single(static candidate => candidate.SourceKey == "expander-semantic-part");
            item.MaterializeDeferredContent();
            Dispatcher.UIThread.RunJobs();

            var demos = page.GetVisualDescendants()
                            .OfType<AtomUIExpander>()
                            .Where(static expander => expander.Classes.Contains("semantic-object") ||
                                                      expander.Classes.Contains("semantic-function"))
                            .ToArray();
            demos.Length.ShouldBe(2);

            var defaultDemo = demos.Single(static expander => expander.Classes.Contains("semantic-object"));
            defaultDemo.SizeType.ShouldBe(CustomizableSizeType.Middle);
            defaultDemo.IsExpanded.ShouldBeTrue();
            var defaultHeader = FindHeaderDecorator(defaultDemo);
            AssertSolidColor(defaultHeader.Background, "#f0f0f0");
            defaultHeader.Padding.ShouldBe(new Thickness(16, 12));
            AssertSolidColor(FindTitlePresenter(defaultDemo).Foreground, "#141414");
            FindBodyPresenter(defaultDemo).Padding.ShouldBe(new Thickness(16, 12));

            var largeDemo = demos.Single(static expander => expander.Classes.Contains("semantic-function"));
            largeDemo.SizeType.ShouldBe(CustomizableSizeType.Large);
            largeDemo.IsExpanded.ShouldBeTrue();
            var largeHeader = FindHeaderDecorator(largeDemo);
            AssertSolidColor(largeHeader.Background, "#f5efff");
            largeHeader.Padding.ShouldBe(new Thickness(16, 12));
            AssertSolidColor(FindTitlePresenter(largeDemo).Foreground, "#141414");
            // 主题为 PART_ExpandButton 设置了 IconSizeSM，此处必须由示例的 ExpanderIconStyle 覆盖。
            var largeIcon = FindExpandIcon(largeDemo);
            largeIcon.IconWidth.ShouldBe(20);
            largeIcon.IconHeight.ShouldBe(20);
        });
    }

    private static string ExtractExpanderExampleItems(string source)
    {
        const string firstItemMarker = "<gallery:ShowCaseItem";

        var firstItemStart = source.IndexOf(firstItemMarker, StringComparison.Ordinal);
        firstItemStart.ShouldBeGreaterThanOrEqualTo(0);

        // 语义结构示例（SourceKey="expander-semantic-part"）不属于 Examples 快照，
        // 与 Collapse 页面一致：快照只覆盖示例面板中的控件演示内容。
        const string semanticItemMarker = "SourceKey=\"expander-semantic-part\"";
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

    private static AtomUI.Controls.Primitives.PixelAlignedBorder FindHeaderDecorator(AtomUIExpander owner)
    {
        return owner.GetVisualDescendants()
                    .OfType<AtomUI.Controls.Primitives.PixelAlignedBorder>()
                    .Single(static header => header.Classes.Contains("semantic-header"));
    }

    private static ContentPresenter FindTitlePresenter(AtomUIExpander owner)
    {
        return owner.GetVisualDescendants()
                    .OfType<ContentPresenter>()
                    .Single(static title => title.Classes.Contains("semantic-title"));
    }

    private static ContentPresenter FindBodyPresenter(AtomUIExpander owner)
    {
        return owner.GetVisualDescendants()
                    .OfType<ContentPresenter>()
                    .Single(static body => body.Classes.Contains("semantic-body"));
    }

    private static AtomUIExpandIcon FindExpandIcon(AtomUIExpander owner)
    {
        return owner.GetVisualDescendants()
                    .OfType<AtomUIExpandIcon>()
                    .Single(static icon => icon.Classes.Contains("semantic-icon"));
    }

    private static void AssertSolidColor(IBrush? actual, string expected)
    {
        actual.ShouldNotBeNull()
              .ShouldBeAssignableTo<ISolidColorBrush>()
              .Color.ShouldBe(Color.Parse(expected));
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
