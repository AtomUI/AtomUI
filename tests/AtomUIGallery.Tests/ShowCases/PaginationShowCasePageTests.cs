using System.Text.RegularExpressions;
using AtomUI.Toolkits.GalleryBase.Controls;
using AtomUIGallery.ShowCases.Pagination;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using ReactiveUI;
using Shouldly;
using Xunit;
using AvaloniaWindow = Avalonia.Controls.Window;
using AtomUIPagination = AtomUI.Desktop.Controls.Pagination;
using AtomUINumericUpDown = AtomUI.Desktop.Controls.NumericUpDown;
using AtomUISimplePagination = AtomUI.Desktop.Controls.SimplePagination;

namespace AtomUIGallery.Tests.ShowCases;

public class PaginationShowCasePageTests
{

    [Fact]
    public void Pagination_Custom_Size_Changer_Example_Uses_Compiled_Context_And_Updates_PageSize()
    {
        var source = ReadRepoFile(
            "controlgallery/AtomUIGallery/ShowCases/Navigation/Pagination/Views/PaginationShowCase.axaml");
        var customSource = ExtractShowCaseItemBySourceKey(source, "pagination-custom-size-changer");

        customSource.ShouldContain("SourceKey=\"pagination-custom-size-changer\"");
        customSource.ShouldContain("PaginationShowCaseLangResource CustomComponentTitle");
        customSource.ShouldContain("PaginationShowCaseLangResource CustomComponentDescription");
        customSource.ShouldContain("<atom:Pagination.SizeChangerTemplate>");
        customSource.ShouldContain("DataTemplate x:DataType=\"atom:PaginationSizeChangerContext\"");
        customSource.ShouldContain("<atom:NumericUpDown");
        customSource.ShouldContain("Minimum=\"1\"");
        customSource.ShouldContain("Increment=\"1\"");
        customSource.ShouldContain("FormatString=\"0\"");
        customSource.ShouldContain(
            "AutomationProperties.Name=\"{gallery:PaginationShowCaseLangResource CustomComponentAutomationName}\"");
        customSource.ShouldContain("SizeType=\"{Binding SizeType}\"");
        customSource.ShouldContain("Value=\"{Binding PageSize, Mode=TwoWay}\"");

        AssertCustomComponentLocalization("en-US.xlf", "Custom component",
            "Replace the page size changer with a custom NumericUpDown.", "Page size");
        AssertCustomComponentLocalization("zh-CN.xlf", "定制组件",
            "使用自定义 NumericUpDown 替换每页条数切换器。", "每页条数");
        AssertCustomComponentLocalization("zh-TW.xlf", "自訂元件",
            "使用自訂 NumericUpDown 取代每頁筆數切換器。", "每頁筆數");
        AssertCustomComponentLocalization("pt-BR.xlf", "Componente personalizado",
            "Substitua o seletor de itens por página por um NumericUpDown personalizado.", "Itens por página");

        AvaloniaTestApp.EnsureInitialized();
        var page = new PaginationShowCase
        {
            DataContext = new PaginationViewModel(new TestScreen())
        };

        ShowInWindow(page, 1280, 900, () =>
        {
            var panel = page.GetVisualDescendants().OfType<ShowCasePanel>().Single();
            var item = panel.Children
                            .OfType<ShowCaseItem>()
                            .Single(candidate => candidate.SourceKey == "pagination-custom-size-changer");
            item.MaterializeDeferredContent();
            Dispatcher.UIThread.RunJobs();

            var pagination = item.GetVisualDescendants().OfType<AtomUIPagination>().Single();
            var numericUpDown = item.GetVisualDescendants().OfType<AtomUINumericUpDown>().Single();
            numericUpDown.Value.ShouldBe(10m);

            numericUpDown.Value = 25m;
            Dispatcher.UIThread.RunJobs();

            pagination.PageSize.ShouldBe(25);
        });
    }

    [Fact]
    public void Pagination_Semantic_Previews_Are_Materialized_On_Tab_Selection()
    {
        AvaloniaTestApp.EnsureInitialized();

        var page = new PaginationShowCase
        {
            DataContext = new PaginationViewModel(new TestScreen())
        };

        ShowInWindow(page, 1280, 900, () =>
        {
            page.GetVisualDescendants().OfType<SemanticPartPreview>().ShouldBeEmpty();

            var host = page.GetVisualDescendants().OfType<GalleryShowCaseHost>().Single();
            host.SelectedTab = GalleryShowCaseTab.SemanticParts;
            Dispatcher.UIThread.RunJobs();

            page.GetVisualDescendants().OfType<SemanticPartPreview>().Count().ShouldBe(2);
            var paginationPreview = page.GetVisualDescendants()
                                        .OfType<SemanticPartPreview>()
                                        .Single(static candidate => candidate.Name == "PaginationSemanticPreview");
            var semanticPagination = paginationPreview.SemanticOwner.ShouldBeOfType<AtomUIPagination>();
            semanticPagination.Name.ShouldBe("PaginationSemanticOwner");
            semanticPagination.HorizontalAlignment.ShouldBe(HorizontalAlignment.Center);
            semanticPagination.VerticalAlignment.ShouldBe(VerticalAlignment.Center);
            semanticPagination.GetVisualDescendants()
                              .Count(static item => item.Classes.Contains("semantic-item") && item.IsVisible)
                              .ShouldBe(7);

            var simplePreview = page.GetVisualDescendants()
                                    .OfType<SemanticPartPreview>()
                                    .Single(static candidate => candidate.Name == "SimplePaginationSemanticPreview");
            var semanticSimplePagination = simplePreview.SemanticOwner.ShouldBeOfType<AtomUISimplePagination>();
            semanticSimplePagination.Name.ShouldBe("SimplePaginationSemanticOwner");
            semanticSimplePagination.GetVisualDescendants()
                                    .Count(static item => item.Classes.Contains("semantic-item") && item.IsVisible)
                                    .ShouldBe(2);
            semanticSimplePagination.GetVisualDescendants()
                                    .Count(static item => item.Classes.Contains("semantic-info") && item.IsVisible)
                                    .ShouldBe(1);

            host.SelectedTab = GalleryShowCaseTab.Examples;
            Dispatcher.UIThread.RunJobs();
            Assert.All(page.GetVisualDescendants().OfType<SemanticPartPreview>(), preview => Assert.False(preview.IsEffectivelyVisible));
        });
    }

    [Fact]
    public void Pagination_ShowCase_Declares_The_Semantic_Previews_And_Style_Example()
    {
        var source = ReadRepoFile(
            "controlgallery/AtomUIGallery/ShowCases/Navigation/Pagination/Views/PaginationShowCase.axaml");
        var english = ReadRepoFile(
            "controlgallery/AtomUIGallery/ShowCases/Navigation/Pagination/Localization/en-US.xlf");
        var semanticSource = ExtractShowCaseItemBySourceKey(source, "pagination-semantic-part");

        source.ShouldContain("<gallery:GalleryShowCaseHost.SemanticPartsContentTemplate>");
        source.ShouldContain("Name=\"PaginationSemanticPreview\"");
        source.ShouldContain("Name=\"SimplePaginationSemanticPreview\"");
        source.ShouldContain("SemanticOwner=\"{Binding #PaginationSemanticOwner}\"");
        source.ShouldContain("SemanticOwner=\"{Binding #SimplePaginationSemanticOwner}\"");
        source.ShouldContain("SemanticOwnerType=\"{x:Type atom:Pagination}\"");
        source.ShouldContain("SemanticOwnerType=\"{x:Type atom:SimplePagination}\"");
        CountOccurrences(source, "<gallery:SemanticPartDescription").ShouldBe(5);
        foreach (var path in new[] { "root", "item", "info" })
        {
            source.ShouldContain($"Path=\"{path}\"");
        }

        semanticSource.ShouldContain("SourceKey=\"pagination-semantic-part\"");
        semanticSource.ShouldContain("PaginationShowCaseLangResource SemanticPartStyleTitle");
        semanticSource.ShouldContain("PaginationShowCaseLangResource SemanticPartStyleDescription");
        semanticSource.ShouldContain("Selector=\"atom|Pagination.semantic-style-demo-root\"");
        semanticSource.ShouldContain("Selector=\"atom|Pagination.semantic-style-demo-object\"");
        semanticSource.ShouldContain("Selector=\"atom|Pagination[SizeType=Small]\"");
        semanticSource.ShouldContain("Property=\"BorderBrush\"");
        semanticSource.ShouldContain("Value=\"#CCCCCC\"");
        semanticSource.ShouldContain("Property=\"BorderThickness\"");
        semanticSource.ShouldContain("Property=\"BorderDashArray\"");
        semanticSource.ShouldContain("Value=\"4,2\"");
        semanticSource.ShouldContain("Property=\"Padding\"");
        semanticSource.ShouldContain("Property=\"CornerRadius\"");
        semanticSource.ShouldContain("Value=\"999\"");
        semanticSource.ShouldContain("Property=\"Background\"");
        semanticSource.ShouldContain("Value=\"#4DC8C8C8\"");
        semanticSource.ShouldContain("Property=\"Margin\"");
        semanticSource.ShouldContain("Value=\"0,0,4,0\"");
        semanticSource.ShouldContain("Selector=\"^:selected\"");
        semanticSource.ShouldContain("Value=\"{atom:PaginationTokenResource ItemActiveBg}\"");
        CountOccurrences(semanticSource, "<atom:PaginationItemStyle ").ShouldBe(2);
        semanticSource.ShouldNotContain("SimplePaginationItemStyle");
        CountOccurrences(semanticSource, "x:SetterTargetType=\"ContentControl\"").ShouldBe(2);
        CountOccurrences(semanticSource, "<atom:Pagination ").ShouldBe(2);
        semanticSource.ShouldNotContain("<atom:SimplePagination ");
        CountOccurrences(semanticSource, "SizeType=\"Small\"").ShouldBe(1);
        CountOccurrences(semanticSource, "IsShowSizeChanger=\"True\"").ShouldBe(2);
        CountOccurrences(semanticSource, "Total=\"500\"").ShouldBe(2);
        semanticSource.ShouldNotContain("/template/");
        semanticSource.ShouldNotContain("semantic dom", Case.Insensitive);
        english.ShouldContain("Custom Semantic Part styling");
        english.ShouldContain("Style the root and items through generated Semantic Part styles");
        english.ShouldNotContain("Mirrors the Ant Design style-class demo");
        english.ShouldNotContain("semantic dom", Case.Insensitive);
    }

    [Fact]
    public void Pagination_Semantic_Style_Example_Applies_The_Official_Style_Values()
    {
        AvaloniaTestApp.EnsureInitialized();

        var page = new PaginationShowCase
        {
            DataContext = new PaginationViewModel(new TestScreen())
        };

        ShowInWindow(page, 1280, 900, () =>
        {
            var panel = page.GetVisualDescendants().OfType<ShowCasePanel>().Single();
            var item = panel.Children
                            .OfType<ShowCaseItem>()
                            .Single(static candidate => candidate.SourceKey == "pagination-semantic-part");
            item.MaterializeDeferredContent();
            Dispatcher.UIThread.RunJobs();

            var demoRows = page.GetVisualDescendants()
                               .OfType<AtomUIPagination>()
                               .Where(static pagination => pagination.Classes.Contains("semantic-style-demo-root"))
                               .ToArray();
            demoRows.Length.ShouldBe(2);

            var demoContainer = demoRows[0].GetVisualParent().ShouldBeOfType<StackPanel>();
            demoRows[1].GetVisualParent().ShouldBeSameAs(demoContainer);
            foreach (var row in demoRows)
            {
                row.Bounds.Width.ShouldBe(demoContainer.Bounds.Width, 0.5);
                AssertSolidColor(row.BorderBrush, "#CCCCCC");
                row.BorderThickness.ShouldBe(new Thickness(2));
                row.BorderDashArray.ShouldBe(new[] { 4d, 2d });
                row.Padding.ShouldBe(new Thickness(8));
            }

            var objectDemo = demoRows.Single(
                static pagination => pagination.Classes.Contains("semantic-style-demo-object"));
            objectDemo.SizeType.ShouldBe(CustomizableSizeType.Middle);

            var objectItems = objectDemo.GetVisualDescendants()
                                        .OfType<ContentControl>()
                                        .Where(static control => control.Classes.Contains("semantic-item"))
                                        .ToArray();
            objectItems.Length.ShouldBe(8);
            foreach (var objectItem in objectItems)
            {
                objectItem.CornerRadius.ShouldBe(new CornerRadius(999));
            }

            var functionDemo = demoRows.Single(
                static pagination => !pagination.Classes.Contains("semantic-style-demo-object"));
            functionDemo.SizeType.ShouldBe(CustomizableSizeType.Small);

            var functionItems = functionDemo.GetVisualDescendants()
                                            .OfType<ContentControl>()
                                            .Where(static control => control.Classes.Contains("semantic-item"))
                                            .ToArray();
            functionItems.Length.ShouldBe(8);

            var objectSelected   = objectItems.Single(static item => item is ISelectable { IsSelected: true });
            var functionSelected = functionItems.Single(static item => item is ISelectable { IsSelected: true });
            functionSelected.Background.ShouldBe(objectSelected.Background);
            functionSelected.Margin.ShouldBe(new Thickness(0, 0, 4, 0));

            foreach (var functionItem in functionItems.Where(
                         static item => item is not ISelectable { IsSelected: true }))
            {
                AssertSolidColor(functionItem.Background, "#4DC8C8C8");
                functionItem.Margin.ShouldBe(new Thickness(0, 0, 4, 0));
            }
        });
    }

    private static void AssertSolidColor(IBrush? actual, string expected)
    {
        actual.ShouldNotBeNull()
              .ShouldBeAssignableTo<ISolidColorBrush>()
              .Color.ShouldBe(Color.Parse(expected));
    }

    private static void AssertCustomComponentLocalization(
        string fileName,
        string title,
        string description,
        string automationName)
    {
        var document = XliffTestDocument.Read(
            $"controlgallery/AtomUIGallery/ShowCases/Navigation/Pagination/Localization/{fileName}");
        document["CustomComponentTitle"].ShouldBe(title);
        document["CustomComponentDescription"].ShouldBe(description);
        document["CustomComponentAutomationName"].ShouldBe(automationName);
    }

    private static string ExtractPaginationExampleItems(string source)
    {
        const string firstItemMarker  = "<gallery:ShowCaseItem";
        const string panelCloseMarker = "</gallery:ShowCasePanel>";

        var firstItemStart = source.IndexOf(firstItemMarker, StringComparison.Ordinal);
        firstItemStart.ShouldBeGreaterThanOrEqualTo(0);

        var panelCloseStart = source.IndexOf(panelCloseMarker, firstItemStart, StringComparison.Ordinal);
        panelCloseStart.ShouldBeGreaterThan(firstItemStart);

        return source[firstItemStart..panelCloseStart];
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

    private static string NormalizeMarkup(string source)
    {
        return ShowCaseSnapshotMarkup.Normalize(source);
    }

    private static int CountShowCaseItemElements(string source)
    {
        return Regex.Matches(source, @"<gallery:ShowCaseItem(\s|>)", RegexOptions.CultureInvariant).Count;
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
