using AtomUI.Desktop.Controls;
using AtomUI.Toolkits.GalleryBase.Controls;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AvaloniaWindow = Avalonia.Controls.Window;
using Button = Avalonia.Controls.Button;

namespace AtomUI.Toolkits.GalleryBase.Tests.Controls;

public class ShowCaseZoomOverlayHostTests
{
    public ShowCaseZoomOverlayHostTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void ZoomRequest_Moves_Content_To_Overlay_Stage()
    {
        var content = new Border();
        var item = new ShowCaseItem { Title = "Zoom", Content = content };
        var host = new ShowCaseZoomOverlayHost { PageContent = new StackPanel { Children = { item } } };

        ShowInWindow(host, () =>
        {
            RaiseZoom(item);

            var overlay = FindOverlay(host);
            overlay.IsOpen.ShouldBeTrue();
            overlay.Title.ShouldBe("Zoom");
            overlay.StageContent.ShouldBeSameAs(content);
            item.Content.ShouldBeNull();
        });
    }

    [Fact]
    public void ZoomRequest_Preserves_The_Item_DataContext_On_The_Overlay()
    {
        var dataContext = new object();
        var content = new Border();
        var item = new ShowCaseItem
        {
            Title = "DC", Content = content, DataContext = dataContext
        };
        var host = new ShowCaseZoomOverlayHost { PageContent = new StackPanel { Children = { item } } };

        ShowInWindow(host, () =>
        {
            RaiseZoom(item);

            var overlay = FindOverlay(host);
            overlay.DataContext.ShouldBeSameAs(dataContext);
            content.DataContext.ShouldBe(dataContext);
        });
    }

    [Fact]
    public void CloseRequest_Restores_Content_To_The_Item()
    {
        var content = new Border();
        var item = new ShowCaseItem { Title = "Zoom", Content = content };
        var host = new ShowCaseZoomOverlayHost { PageContent = new StackPanel { Children = { item } } };

        ShowInWindow(host, () =>
        {
            RaiseZoom(item);
            var overlay = FindOverlay(host);

            var closeButton = overlay.GetVisualDescendants()
                                     .OfType<IconButton>()
                                     .Single(button => button.Name == "PART_CloseButton");
            closeButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent) { Source = closeButton });
            Dispatcher.UIThread.RunJobs();

            overlay.IsOpen.ShouldBeFalse();
            overlay.StageContent.ShouldBeNull();
            item.Content.ShouldBeSameAs(content);
        });
    }

    [Fact]
    public void Detaching_The_Zoomed_Item_Forces_Restore_And_Close()
    {
        var content = new Border();
        var item = new ShowCaseItem { Title = "Zoom", Content = content };
        var panel = new StackPanel { Children = { item } };
        var host = new ShowCaseZoomOverlayHost { PageContent = panel };

        ShowInWindow(host, () =>
        {
            RaiseZoom(item);
            var overlay = FindOverlay(host);
            overlay.IsOpen.ShouldBeTrue();

            panel.Children.Remove(item);
            Dispatcher.UIThread.RunJobs();

            overlay.IsOpen.ShouldBeFalse();
            item.Content.ShouldBeSameAs(content);
        });
    }

    [Fact]
    public void Second_ZoomRequest_Is_Ignored_While_Open()
    {
        var firstContent = new Border();
        var secondContent = new Border();
        var first = new ShowCaseItem { Title = "First", Content = firstContent };
        var second = new ShowCaseItem { Title = "Second", Content = secondContent };
        var host = new ShowCaseZoomOverlayHost
        {
            PageContent = new StackPanel { Children = { first, second } }
        };

        ShowInWindow(host, () =>
        {
            RaiseZoom(first);
            RaiseZoom(second);
            Dispatcher.UIThread.RunJobs();

            var overlay = FindOverlay(host);
            overlay.StageContent.ShouldBeSameAs(firstContent);
            second.Content.ShouldBeSameAs(secondContent);
        });
    }

    [Fact]
    public void ZoomRequest_Captures_The_Content_Card_Layout_Width()
    {
        var content = new Border { Width = 240, Height = 120 };
        var item = new ShowCaseItem { Title = "Zoom", Content = content };
        var host = new ShowCaseZoomOverlayHost { PageContent = new StackPanel { Children = { item } } };

        ShowInWindow(host, () =>
        {
            RaiseZoom(item);

            var overlay = FindOverlay(host);
            overlay.StageMeasureWidth.ShouldBe(240, 0.5);
        });
    }

    [Fact]
    public void ZoomRequest_Keeps_Page_Scope_Styles_Applied_To_The_Content()
    {
        // 复刻 Grid 展示页场景：示例格子的样式声明在页面级 Styles（类选择器），
        // reparent 进 overlay 后必须保持逻辑挂载，页面级样式继续命中
        var content = new Border
        {
            Classes = { "demo-cell" },
            Width   = 100,
            Height  = 40
        };
        var item = new ShowCaseItem { Title = "Zoom", Content = content };
        var pageScope = new Avalonia.Controls.Grid { Children = { item } };
        pageScope.Styles.Add(new Avalonia.Styling.Style(
            selector => selector.OfType<Avalonia.Controls.Border>().Class("demo-cell"))
        {
            Setters =
            {
                new Avalonia.Styling.Setter(Avalonia.Controls.Border.BackgroundProperty, Avalonia.Media.Brushes.CadetBlue)
            }
        });
        var host = new ShowCaseZoomOverlayHost { PageContent = pageScope };

        ShowInWindow(host, () =>
        {
            content.Background.ShouldBe(Avalonia.Media.Brushes.CadetBlue,
                "precondition: page-scoped style must apply before zooming");

            RaiseZoom(item);

            var overlay = FindOverlay(host);
            overlay.IsOpen.ShouldBeTrue();
            content.Background.ShouldBe(Avalonia.Media.Brushes.CadetBlue,
                "page-scoped styles must keep applying while the content is elevated into the zoom overlay");
        });
    }

    private static void RaiseZoom(ShowCaseItem item)
    {
        var zoomButton = item.GetVisualDescendants()
                             .OfType<IconButton>()
                             .Single(button => button.Name == "PART_ZoomButton");
        zoomButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent) { Source = zoomButton });
        Dispatcher.UIThread.RunJobs();
    }

    private static ShowCaseZoomOverlay FindOverlay(ShowCaseZoomOverlayHost host)
    {
        return host.GetVisualDescendants()
                   .OfType<ShowCaseZoomOverlay>()
                   .Single();
    }

    private static void ShowInWindow(Control content, Action assertion)
    {
        var window = new AvaloniaWindow { Width = 500, Height = 500, Content = content };
        try
        {
            window.Show();
            content.ApplyTemplate();
            Dispatcher.UIThread.RunJobs();
            assertion();
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }
}
