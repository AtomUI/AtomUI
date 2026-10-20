using AtomUI.Toolkits.GalleryBase.Controls;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Templates;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AtomUIButton = AtomUI.Desktop.Controls.Button;
using AtomUIToolTip = AtomUI.Desktop.Controls.ToolTip;

namespace AtomUI.Toolkits.GalleryBase.Tests.Controls;

public class SemanticPartPreviewLayoutTests
{
    static SemanticPartPreviewLayoutTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Preview_Content_Stays_Within_The_Host_Height_Clamp()
    {
        var preview = new SemanticPartPreview
        {
            Width = 900,
            Title = "DatePicker",
            PreviewStageMinHeight = 560,
            PreviewContentAlignment = VerticalAlignment.Top,
            PreviewContent = new Border
            {
                Height = 560,
                Background = Brushes.Transparent
            }
        };

        // 模拟 GalleryShowCaseHost 把语义内容钳制到视口剩余高度的场景。
        var host = new ContentPresenter { MaxHeight = 500, Content = preview };
        var window = new Window
        {
            Width = 980,
            Height = 720,
            Content = new StackPanel
            {
                Children = { new Border { Height = 120 }, host }
            }
        };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            Dispatcher.UIThread.RunJobs();

            preview.Bounds.Height.ShouldBeLessThanOrEqualTo(500,
                "preview card must respect the host height clamp instead of spilling out of the viewport");

            var stage = preview.GetVisualDescendants()
                               .OfType<Border>()
                               .First(control => control.Name == "PART_PreviewStage");
            stage.Bounds.Height.ShouldBeLessThanOrEqualTo(500);
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Preview_Stage_Grows_To_The_Configured_MinHeight_When_Space_Allows()
    {
        var preview = new SemanticPartPreview
        {
            Width = 900,
            Title = "DatePicker",
            PreviewStageMinHeight = 560,
            PreviewContentAlignment = VerticalAlignment.Top,
            PreviewContent = new TextBox()
        };

        var window = new Window
        {
            Width = 980,
            Height = 900,
            Content = preview
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            Dispatcher.UIThread.RunJobs();

            var stage = preview.GetVisualDescendants()
                               .OfType<Border>()
                               .First(control => control.Name == "PART_PreviewStage");
            stage.Bounds.Height.ShouldBeGreaterThanOrEqualTo(560);
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Preview_Content_Centered_ToolTip_Hugs_Its_Text_Width()
    {
        // 回归：预览舞台以 Stretch 测量内容，ToolTip 若不显式非 Stretch 对齐会被
        // 撑到 ToolTipMaxWidth(250) 满宽，胶囊宽度远超 "prompt text" 所需。
        var preview = new SemanticPartPreview
        {
            Width = 900,
            Title = "Tooltip",
            PreviewContent = new AtomUIToolTip
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                Content                = "prompt text"
            }
        };

        var window = new Window
        {
            Width  = 980,
            Height = 600,
            Content = preview
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            Dispatcher.UIThread.RunJobs();

            var toolTip = (AtomUIToolTip)preview.PreviewContent!;
            toolTip.Bounds.Width.ShouldBeGreaterThan(0);
            toolTip.Bounds.Width.ShouldBeLessThan(250,
                "居中对齐的 ToolTip 预览内容必须收缩到文本宽度，而不是被拉伸到 ToolTipMaxWidth 满宽");
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Semantic_Tab_Shows_Page_ScrollBar_When_Window_Is_Too_Short_For_The_Preview()
    {
        // 复现真机 bug：单个 Preview 的 Semantic Parts 页签在窗口过矮时，
        // 钉住弹层被裁切且页面垂直滚动条不出现，内容不可达。
        var host = new GalleryShowCaseHost
        {
            Width = 1000,
            Header = new Border { Height = 80, Background = Brushes.Transparent },
            SemanticPartsContentTemplate = new FuncDataTemplate<object>(
                (_, _) => new SemanticPartPreview
                {
                    Title = "TimePicker",
                    PreviewStageMinHeight = 560,
                    PreviewContentAlignment = VerticalAlignment.Top,
                    PreviewContent = new AtomUIButton { Content = "Preview" }
                },
                supportsRecycling: false),
            SelectedTab = GalleryShowCaseTab.SemanticParts
        };

        var window = new Window
        {
            Width = 1000,
            // 视口剩余高度 ≈ 620 - 80(header) - ~44(sticky) ≈ 496，
            // 小于 Preview 需要的最小高度 560 + 54(检查面板上下内边距)。
            Height = 620,
            Content = host
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            Dispatcher.UIThread.RunJobs();

            var pageScrollViewer = host.GetVisualDescendants()
                                       .OfType<ScrollViewer>()
                                       .Single(viewer => viewer.Name == "PART_ScrollViewer");
            pageScrollViewer.Extent.Height.ShouldBeGreaterThan(
                pageScrollViewer.Viewport.Height,
                "window shorter than the preview minimum must surface the page vertical scrollbar " +
                "instead of clipping the pinned popup with no way to scroll");

            var preview = host.GetVisualDescendants().OfType<SemanticPartPreview>().Single();
            var stage = preview.GetVisualDescendants()
                               .OfType<Border>()
                               .First(control => control.Name == "PART_PreviewStage");
            stage.Bounds.Height.ShouldBeGreaterThanOrEqualTo(
                560,
                "the configured PreviewStageMinHeight must stay reachable through page scrolling");
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Semantic_Tab_Keeps_Viewport_Filled_Without_Page_ScrollBar_When_Window_Is_Tall_Enough()
    {
        var host = new GalleryShowCaseHost
        {
            Width = 1000,
            Header = new Border { Height = 80, Background = Brushes.Transparent },
            SemanticPartsContentTemplate = new FuncDataTemplate<object>(
                (_, _) => new SemanticPartPreview
                {
                    Title = "TimePicker",
                    PreviewStageMinHeight = 560,
                    PreviewContentAlignment = VerticalAlignment.Top,
                    PreviewContent = new AtomUIButton { Content = "Preview" }
                },
                supportsRecycling: false),
            SelectedTab = GalleryShowCaseTab.SemanticParts
        };

        var window = new Window
        {
            Width = 1000,
            // 视口剩余高度 ≈ 900 - 80 - ~44 ≈ 776 > 614：限高填充模式保持，
            // 页面不应出现滚动条。
            Height = 900,
            Content = host
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            Dispatcher.UIThread.RunJobs();

            var pageScrollViewer = host.GetVisualDescendants()
                                       .OfType<ScrollViewer>()
                                       .Single(viewer => viewer.Name == "PART_ScrollViewer");
            pageScrollViewer.Extent.Height.ShouldBeLessThanOrEqualTo(
                pageScrollViewer.Viewport.Height + 1,
                "tall window keeps the bounded fill layout and must not grow a page scrollbar");

            var preview = host.GetVisualDescendants().OfType<SemanticPartPreview>().Single();
            var stage = preview.GetVisualDescendants()
                               .OfType<Border>()
                               .First(control => control.Name == "PART_PreviewStage");
            stage.Bounds.Height.ShouldBeGreaterThanOrEqualTo(560);
        }
        finally
        {
            window.Close();
        }
    }
}
