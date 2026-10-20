using AtomUI.Toolkits.GalleryBase.Controls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Threading;
using Avalonia.VisualTree;
using ReactiveUI;
using Shouldly;
using Xunit;
using DataGridShowCase = AtomUIGallery.ShowCases.DataGrid.DataGridShowCase;
using DataGridViewModel = AtomUIGallery.ShowCases.DataGrid.DataGridViewModel;
using AtomUIWindow = AtomUI.Desktop.Controls.Window;

namespace AtomUIGallery.Tests.ShowCases;

public class DataGridSemanticPartHighlightTests
{
    [Fact]
    public void DataGrid_Semantic_Preview_Highlights_All_Declared_Parts()
    {
        AvaloniaTestApp.EnsureInitialized();

        var page = new DataGridShowCase
        {
            DataContext = new DataGridViewModel(new DataGridTestScreen())
        };

        var window = new AtomUIWindow
        {
            Width = 1280,
            Height = 900,
            Content = page
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            var host = page.GetVisualDescendants().OfType<GalleryShowCaseHost>().Single();
            host.SelectedTab = GalleryShowCaseTab.SemanticParts;
            Dispatcher.UIThread.RunJobs();

            var preview = page.GetVisualDescendants()
                              .OfType<SemanticPartPreview>()
                              .Single(static candidate => candidate.Name == "DataGridSemanticPreview");

            var partsPane = preview.GetVisualDescendants()
                                   .OfType<Border>()
                                   .First(static border => border.Name == "PART_PartsPane");
            var cards = partsPane.GetVisualDescendants()
                                 .OfType<UserControl>()
                                 .Where(static card => card.GetType().Name.Contains("SemanticPartPreviewItem"))
                                 .ToArray();
            // root + section/header.wrapper/header.cell/title/body.wrapper/body.row/body.cell/footer/
            // content/pagination.root/pagination.item
            cards.Length.ShouldBe(12);

            var ownerGrid = preview.GetVisualDescendants()
                                   .OfType<AtomUI.Desktop.Controls.DataGrid>()
                                   .Single(static g => g.Name == "SemanticCaseGrid");
            window.UpdateLayout();
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            Dispatcher.UIThread.RunJobs();
            // 自然高度:Small 尺寸 + title/header/3 行分页数据/footer/pagination。
            // 不设固定 Height,画布被宿主钳制时内容随之收缩,断言下限保底结构完整。
            ownerGrid.Bounds.Height.ShouldBeGreaterThanOrEqualTo(220);
            ownerGrid.LoadState.ShouldBe(AtomUI.Desktop.Controls.DataGridLoadState.Ready);
            preview.GetVisualDescendants()
                   .OfType<AtomUI.Desktop.Controls.DataGridRow>()
                   .Count()
                   .ShouldBeGreaterThanOrEqualTo(3);

            // 4 行数据 x pageSize 3,3 行 x 4 列的语义预览表格。
            HoverCard(cards, window, "root");
            GetHighlightCount(window).ShouldBe(1);
            HoverCard(cards, window, "section");
            GetHighlightCount(window).ShouldBe(1);
            HoverCard(cards, window, "header.wrapper");
            GetHighlightCount(window).ShouldBe(1);
            var realizedHeaders = preview.GetVisualDescendants()
                                        .OfType<Avalonia.Controls.Control>()
                                        .Where(static c => c.GetType().Name == "DataGridColumnHeader" &&
                                                           c.IsVisible)
                                        .ToArray();
            // 4 个列头 + 可能存在的填充列头（filler 列头也是渲染出的表头单元格，属于 header.cell）。
            realizedHeaders.Length.ShouldBeGreaterThanOrEqualTo(4);
            HoverCard(cards, window, "header.cell");
            GetHighlightCount(window).ShouldBe(realizedHeaders.Length);
            HoverCard(cards, window, "title");
            GetHighlightCount(window).ShouldBe(1);
            HoverCard(cards, window, "body.wrapper");
            GetHighlightCount(window).ShouldBe(1);
            window.UpdateLayout();
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            Dispatcher.UIThread.RunJobs();
            // 行/单元格的数量取决于预览视口实际物化的行数（虚拟化），按物化数量断言。
            var realizedRows = preview.GetVisualDescendants()
                                      .OfType<AtomUI.Desktop.Controls.DataGridRow>()
                                      .ToArray();
            realizedRows.Length.ShouldBeGreaterThanOrEqualTo(3);
            HoverCard(cards, window, "body.row");
            GetHighlightCount(window).ShouldBe(realizedRows.Length);
            var realizedCells = preview.GetVisualDescendants()
                                      .OfType<AtomUI.Desktop.Controls.DataGridCell>()
                                      .Where(static c => c.IsVisible && c.Bounds.Width > 0)
                                      .ToArray();
            realizedCells.Length.ShouldBeGreaterThanOrEqualTo(realizedRows.Length * 4);
            HoverCard(cards, window, "body.cell");
            GetHighlightCount(window).ShouldBe(realizedCells.Length);
            HoverCard(cards, window, "footer");
            GetHighlightCount(window).ShouldBe(1);
            HoverCard(cards, window, "content");
            GetHighlightCount(window).ShouldBe(1);
            // 分页宿主:可见的底部分页器 1 个（顶部槽位隐藏不占位）。
            HoverCard(cards, window, "pagination.root");
            GetHighlightCount(window).ShouldBe(1);
            // 分页项:数量取决于分页器实际生成的页码项,按预览表内可见项数断言。
            var paginationItems = ownerGrid.GetVisualDescendants()
                                           .OfType<Avalonia.Controls.Control>()
                                           .Where(static c => c.Classes.Contains("semantic-item") && c.IsVisible)
                                           .ToArray();
            paginationItems.Length.ShouldBeGreaterThanOrEqualTo(1);
            HoverCard(cards, window, "pagination.item");
            GetHighlightCount(window).ShouldBe(paginationItems.Length);

            host.SelectedTab = GalleryShowCaseTab.Examples;
            Dispatcher.UIThread.RunJobs();
            GetHighlightCount(window).ShouldBe(0);
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    [Fact]
    public void Semantic_Style_Demo_Uses_Generated_Dedicated_Part_Styles()
    {
        var source = ReadRepoFile(
            "controlgallery/AtomUIGallery/ShowCases/DataDisplay/DataGrid/Views/DataGridShowCase.axaml");

        // 专用 Semantic Part Style 是唯一定制入口：生成类以嵌套 Style 出现并带关键 Setter 值。
        source.ShouldContain("<atom:DataGridTitleStyle x:SetterTargetType=\"atom:PixelAlignedBorder\">");
        source.ShouldContain("<atom:DataGridHeaderWrapperStyle x:SetterTargetType=\"Border\">");
        source.ShouldContain("<atom:DataGridHeaderCellStyle x:SetterTargetType=\"ContentControl\">");
        source.ShouldContain("<atom:DataGridBodyRowStyle x:SetterTargetType=\"TemplatedControl\">");
        source.ShouldContain("<atom:DataGridBodyCellStyle x:SetterTargetType=\"atom:DataGridCell\">");
        source.ShouldContain("<atom:DataGridFooterStyle x:SetterTargetType=\"ContentPresenter\">");
        source.ShouldContain("<atom:DataGridSectionStyle x:SetterTargetType=\"Border\">");
        source.ShouldContain("<atom:DataGridContentStyle x:SetterTargetType=\"Grid\">");
        source.ShouldContain("<Setter Property=\"Background\" Value=\"#E6F4FF\" />");
        source.ShouldContain("<Setter Property=\"FontWeight\" Value=\"SemiBold\" />");

        // 禁止以 Loaded/Unloaded 事件处理器回退定制语义部件。
        var semanticDemo = ExtractElement(source, "semantic-info-table-demo", "</StackPanel>");
        semanticDemo.ShouldNotContain("Loaded=\"");
        semanticDemo.ShouldNotContain("Unloaded=\"");
        semanticDemo.ShouldNotContain("x:Name=\"SemanticStyleCaseGridA\".");
    }

    private static string ExtractElement(string source, string startMarker, string endMarker)
    {
        var start = source.IndexOf(startMarker, StringComparison.Ordinal);
        start.ShouldBeGreaterThanOrEqualTo(0);
        var end = source.IndexOf(endMarker, start, StringComparison.Ordinal);
        end.ShouldBeGreaterThan(start);
        return source[start..end];
    }

    private static int GetHighlightCount(AtomUIWindow window)
    {
        return window.GetVisualDescendants()
                     .Count(static visual => visual.GetType().Name == "SemanticPartAdorner");
    }

    private static void HoverCard(
        UserControl[] cards,
        AtomUIWindow window,
        string path)
    {
        var card = cards.Single(candidate =>
            (string?)candidate.DataContext?.GetType().GetProperty("Path")!.GetValue(candidate.DataContext) == path);
        card.BringIntoView();
        Dispatcher.UIThread.RunJobs();
        card.BringIntoView();
        Dispatcher.UIThread.RunJobs();
        card.BringIntoView();
        Dispatcher.UIThread.RunJobs();
        var center = card.TransformToVisual(window)!.Value
                         .Transform(new Point(card.Bounds.Width / 2, card.Bounds.Height / 2));
        window.MouseMove(new Point(center.X, center.Y));
        Dispatcher.UIThread.RunJobs();
        Dispatcher.UIThread.RunJobs();
        Dispatcher.UIThread.RunJobs();
        Dispatcher.UIThread.RunJobs();
        Dispatcher.UIThread.RunJobs();
    }

    private static string ReadRepoFile(string relativePath)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var candidate = Path.Combine(directory.FullName, relativePath);
            if (File.Exists(candidate))
            {
                return File.ReadAllText(candidate);
            }

            directory = directory.Parent;
        }

        throw new FileNotFoundException($"Could not locate repository file '{relativePath}'.");
    }
}

internal sealed class DataGridTestScreen : IScreen
{
    public RoutingState Router { get; } = new();
}
