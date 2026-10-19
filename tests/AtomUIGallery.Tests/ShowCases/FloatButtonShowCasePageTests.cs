using System.Collections;
using System.Reflection;
using AtomUI.Controls;
using AtomUI.Desktop.Controls;
using AtomUI.Theme;
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
using AtomUIFloatButton = AtomUI.Desktop.Controls.FloatButton;
using AtomUIBackTopFloatButton = AtomUI.Desktop.Controls.BackTopFloatButton;
using AtomUIFloatButtonGroup = AtomUI.Desktop.Controls.FloatButtonGroup;
using FloatButtonShowCase = AtomUIGallery.ShowCases.FloatButton.FloatButtonShowCase;
using FloatButtonViewModel = AtomUIGallery.ShowCases.FloatButton.FloatButtonViewModel;

namespace AtomUIGallery.Tests.ShowCases;

public class FloatButtonShowCasePageTests
{

    [Fact]
    public void FloatButton_ShowCase_Declares_The_Semantic_Previews_And_Style_Example()
    {
        var source = ReadRepoFile(
            "controlgallery/AtomUIGallery/ShowCases/General/FloatButton/Views/FloatButtonShowCase.axaml");
        var english = ReadRepoFile(
            "controlgallery/AtomUIGallery/ShowCases/General/FloatButton/Localization/en-US.xlf");
        var semanticSource = ExtractShowCaseItem(source, "float-button-semantic-part");

        source.ShouldContain("<gallery:GalleryShowCaseHost.SemanticPartsContentTemplate>");
        source.ShouldContain("Name=\"FloatButtonSemanticPreview\"");
        source.ShouldContain("SemanticOwner=\"{Binding #FloatButtonSemanticOwner}\"");
        source.ShouldContain("SemanticOwnerType=\"{x:Type atom:FloatButton}\"");
        source.ShouldContain("Name=\"FloatButtonGroupSemanticPreview\"");
        source.ShouldContain("SemanticOwner=\"{Binding #FloatButtonGroupSemanticOwner}\"");
        source.ShouldContain("SemanticOwnerType=\"{x:Type atom:FloatButtonGroup}\"");
        source.ShouldContain("Name=\"BackTopFloatButtonSemanticPreview\"");
        source.ShouldContain("SemanticOwner=\"{Binding #BackTopFloatButtonSemanticOwner}\"");
        source.ShouldContain("SemanticOwnerType=\"{x:Type atom:BackTopFloatButton}\"");
        CountOccurrences(source, "<gallery:SemanticPartDescription").ShouldBe(9);
        foreach (var path in new[] { "root", "icon", "content", "trigger", "list" })
        {
            source.ShouldContain($"Path=\"{path}\"");
        }

        semanticSource.ShouldContain("SourceKey=\"float-button-semantic-part\"");
        semanticSource.ShouldContain("FloatButtonShowCaseLangResource SemanticPartStyleTitle");
        semanticSource.ShouldContain("FloatButtonShowCaseLangResource SemanticPartStyleDescription");
        semanticSource.ShouldContain("Value=\"0 1 2 0 #0D000000\"");
        semanticSource.ShouldContain("Value=\"#171717\"");
        semanticSource.ShouldContain("Value=\"#FFFFFF\"");
        semanticSource.ShouldContain("Selector=\"atom|FloatButton.semantic-function-demo[ButtonType=Primary]\"");
        semanticSource.ShouldContain("Property=\"IconBrush\"");
        semanticSource.ShouldNotContain("/template/");
        semanticSource.ShouldNotContain("classNames", Case.Insensitive);
        semanticSource.ShouldNotContain("semantic dom", Case.Insensitive);
        english.ShouldContain("Custom Semantic Part styling");
        english.ShouldContain("Root element with the float button's shape, type, shadow and theme entry styles.");
        english.ShouldContain("Icon element with brush, size and other icon styles.");
        english.ShouldContain("Content element with font size, foreground, line height and other description styles, available in the square shape.");
        english.ShouldContain("List element with background, corner radius, spacing and other menu item container styles.");
        english.ShouldContain("Trigger element with the group's main button styles, shown in click or hover trigger mode.");
        english.ShouldNotContain("semantic dom", Case.Insensitive);
        english.ShouldNotContain("classNames", Case.Insensitive);
    }

    [Fact]
    public void FloatButton_Semantic_Previews_Are_Materialized_Only_After_The_Tab_Is_Selected()
    {
        AvaloniaTestApp.EnsureInitialized();

        var page = new FloatButtonShowCase
        {
            DataContext = new FloatButtonViewModel(new TestScreen())
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
            activeContent.GetVisualDescendants().OfType<SemanticPartPreview>().Count().ShouldBe(3);

            var semanticButton = activeContent.GetVisualDescendants()
                                              .OfType<AtomUIFloatButton>()
                                              .Single(static button => button.Name == "FloatButtonSemanticOwner");
            semanticButton.GetVisualDescendants()
                          .OfType<IconPresenter>()
                          .Single(static icon => icon.Classes.Contains("semantic-icon"))
                          .IsEffectivelyVisible.ShouldBeTrue();
            semanticButton.GetVisualDescendants()
                          .OfType<ContentPresenter>()
                          .Single(static content => content.Classes.Contains("semantic-content"))
                          .IsEffectivelyVisible.ShouldBeTrue();

            var semanticGroup = activeContent.GetVisualDescendants()
                                             .OfType<AtomUIFloatButtonGroup>()
                                             .Single(static group => group.Name == "FloatButtonGroupSemanticOwner");
            semanticGroup.GetVisualDescendants()
                         .OfType<AtomUIFloatButton>()
                         .Single(static trigger => trigger.Classes.Contains("semantic-trigger"))
                         .IsEffectivelyVisible.ShouldBeTrue();
            var list = semanticGroup.GetVisualDescendants()
                                    .OfType<Control>()
                                    .Single(static control => control.Classes.Contains("semantic-list"));
            list.IsEffectivelyVisible.ShouldBeTrue();
            semanticGroup.GetVisualDescendants()
                         .OfType<StackPanel>()
                         .Single(static panel => panel.Name == "PART_ItemsLayout")
                         .Children.Count.ShouldBe(2);

            var semanticBackTop = activeContent.GetVisualDescendants()
                                               .OfType<AtomUIBackTopFloatButton>()
                                               .Single(static button => button.Name == "BackTopFloatButtonSemanticOwner");
            semanticBackTop.GetVisualDescendants()
                           .OfType<IconPresenter>()
                           .Single(static icon => icon.Classes.Contains("semantic-icon"))
                           .IsEffectivelyVisible.ShouldBeTrue();

            var groupPreview = activeContent.GetVisualDescendants()
                                            .OfType<SemanticPartPreview>()
                                            .Single(static preview => preview.Name == "FloatButtonGroupSemanticPreview");
            var previewItemsValue = typeof(SemanticPartPreview)
                                    .GetProperty("Items", BindingFlags.Instance | BindingFlags.NonPublic)
                                    ?.GetValue(groupPreview);
            previewItemsValue.ShouldNotBeNull();
            var previewItems = previewItemsValue.ShouldBeAssignableTo<IEnumerable>()
                                                .Cast<object>()
                                                .ToArray();
            previewItems.Length.ShouldBe(3);
            var listItem = previewItems.Single(item =>
            {
                var path = item.GetType().GetProperty("Path")?.GetValue(item) as string;
                return path == "list";
            });
            typeof(SemanticPartPreview)
                .GetMethod("SetHoveredPart", BindingFlags.Instance | BindingFlags.NonPublic)
                .ShouldNotBeNull()
                .Invoke(groupPreview, [listItem, true]);
            Dispatcher.UIThread.RunJobs();

            var listAdorners = AdornerLayer.GetAdornerLayer(list)
                                           .ShouldNotBeNull()
                                           .Children
                                           .Where(static child => child.GetType().Name == "SemanticPartAdorner")
                                           .Select(AdornerLayer.GetAdornedElement)
                                           .ToArray();
            listAdorners.ShouldHaveSingleItem().ShouldBeSameAs(list);

            host.SelectedTab = GalleryShowCaseTab.Examples;
            Dispatcher.UIThread.RunJobs();
            Assert.All(page.GetVisualDescendants().OfType<SemanticPartPreview>(), preview => Assert.False(preview.IsEffectivelyVisible));
        });
    }

    [Fact]
    public void FloatButton_Semantic_Style_Example_Applies_The_Official_Style_Values()
    {
        AvaloniaTestApp.EnsureInitialized();

        var page = new FloatButtonShowCase
        {
            DataContext = new FloatButtonViewModel(new TestScreen())
        };

        ShowInWindow(page, 1280, 1400, () =>
        {
            var panel = page.GetVisualDescendants().OfType<ShowCasePanel>().Single();
            var item = panel.Children
                            .OfType<ShowCaseItem>()
                            .Single(static candidate => candidate.SourceKey == "float-button-semantic-part");
            item.MaterializeDeferredContent();
            Dispatcher.UIThread.RunJobs();

            var functionDemo = page.GetVisualDescendants()
                                   .OfType<AtomUIFloatButton>()
                                   .Single(static button => button.Classes.Contains("semantic-function-demo"));
            functionDemo.ButtonType.ShouldBe(FloatButtonType.Primary);
            AssertSolidColor(functionDemo.Background, "#171717");
            functionDemo.BorderThickness.ShouldBe(new Thickness(1));
            functionDemo.CornerRadius.ShouldBe(new CornerRadius(6));
            functionDemo.BorderBrush.ShouldNotBeNull();
            var demoGroup = functionDemo.FindAncestorOfType<AtomUIFloatButtonGroup>().ShouldNotBeNull();
            demoGroup.Shape.ShouldBe(FloatButtonShape.Circle);
            var functionIcon = functionDemo.GetVisualDescendants()
                                           .OfType<IconPresenter>()
                                           .Single(static icon => icon.Classes.Contains("semantic-icon"));
            AssertSolidColor(functionIcon.IconBrush, "#FFFFFF");

            var styleDemo = page.GetVisualDescendants()
                                .OfType<AtomUIFloatButton>()
                                .Single(static button => button.Classes.Contains("semantic-style-demo"));
            styleDemo.BoxShadow.ShouldBe(BoxShadows.Parse("0 1 2 0 #0D000000"));
            styleDemo.ButtonType.ShouldBe(FloatButtonType.Default);
            styleDemo.BorderThickness.ShouldBe(new Thickness(1));
            styleDemo.CornerRadius.ShouldBe(new CornerRadius(6));
            styleDemo.BorderBrush.ShouldNotBeNull();
            var styleIcon = styleDemo.GetVisualDescendants()
                                     .OfType<IconPresenter>()
                                     .Single(static icon => icon.Classes.Contains("semantic-icon"));
            styleIcon.IconBrush.ShouldNotBeNull()
                .ShouldBeAssignableTo<ISolidColorBrush>()
                .Color.ShouldNotBe(Color.Parse("#FFFFFF"));
        });
    }

    private static string ExtractFloatButtonExampleItems(string source)
    {
        const string firstItemMarker = "<gallery:ShowCaseItem";
        var firstItemStart = source.IndexOf(firstItemMarker, StringComparison.Ordinal);
        firstItemStart.ShouldBeGreaterThanOrEqualTo(0);

        const string semanticItemMarker = "SourceKey=\"float-button-semantic-part\"";
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
