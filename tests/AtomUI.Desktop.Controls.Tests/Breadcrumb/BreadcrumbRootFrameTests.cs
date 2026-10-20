using System.Reactive.Disposables;
using Avalonia;
using Avalonia.Controls.Presenters;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AtomUIBreadcrumb = AtomUI.Desktop.Controls.Breadcrumb;
using AtomUIBreadcrumbItem = AtomUI.Desktop.Controls.BreadcrumbItem;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.Breadcrumb;

public class BreadcrumbRootFrameTests
{
    static BreadcrumbRootFrameTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Root_Frame_Inset_Offsets_The_Items_Presenter_And_Inflates_Desired_Size()
    {
        var breadcrumb = CreateBreadcrumb();
        breadcrumb.Padding         = new Thickness(8);
        breadcrumb.BorderThickness = new Thickness(2);

        using var window = Show(breadcrumb);

        var presenter = FindPresenter(breadcrumb);
        presenter.Bounds.X.ShouldBe(10);
        presenter.Bounds.Y.ShouldBe(10);
        presenter.Bounds.Width.ShouldBe(breadcrumb.Bounds.Width - 20, 0.01);
        presenter.Bounds.Height.ShouldBe(breadcrumb.Bounds.Height - 20, 0.01);
        breadcrumb.DesiredSize.Width.ShouldBe(presenter.DesiredSize.Width + 20, 0.01);
        breadcrumb.DesiredSize.Height.ShouldBe(presenter.DesiredSize.Height + 20, 0.01);
    }

    [Fact]
    public void Default_Frame_Values_Keep_Legacy_Layout_Unchanged()
    {
        var breadcrumb = CreateBreadcrumb();

        using var window = Show(breadcrumb);

        var presenter = FindPresenter(breadcrumb);
        presenter.Bounds.X.ShouldBe(0);
        presenter.Bounds.Y.ShouldBe(0);
        presenter.Bounds.Width.ShouldBe(breadcrumb.Bounds.Width, 0.01);
        presenter.Bounds.Height.ShouldBe(breadcrumb.Bounds.Height, 0.01);
    }

    [Fact]
    public void Root_Frame_Appearance_Api_Is_Exposed_For_Ant_Design_Styling_Alignment()
    {
        var breadcrumb = new AtomUIBreadcrumb
        {
            Background      = Brushes.White,
            BorderBrush     = Brushes.Black,
            BorderThickness = new Thickness(2),
            CornerRadius    = new CornerRadius(4),
            Padding         = new Thickness(8)
        };

        breadcrumb.Background.ShouldBe(Brushes.White);
        breadcrumb.BorderBrush.ShouldBe(Brushes.Black);
        breadcrumb.BorderThickness.ShouldBe(new Thickness(2));
        breadcrumb.CornerRadius.ShouldBe(new CornerRadius(4));
        breadcrumb.Padding.ShouldBe(new Thickness(8));
    }

    [Fact]
    public void Default_Theme_Stretches_The_Root_Like_An_Ant_Design_Block_Element()
    {
        var breadcrumb = CreateBreadcrumb();
        var window = new AvaloniaWindow
        {
            Width   = 800,
            Height  = 600,
            Content = breadcrumb
        };

        window.Show();
        Dispatcher.UIThread.RunJobs();
        try
        {
            breadcrumb.HorizontalAlignment.ShouldBe(HorizontalAlignment.Stretch);
            breadcrumb.Bounds.Width.ShouldBe(window.Width, 0.01);
        }
        finally
        {
            window.Close();
        }
    }

    private static AtomUIBreadcrumb CreateBreadcrumb()
    {
        return new AtomUIBreadcrumb
        {
            Items =
            {
                new AtomUIBreadcrumbItem { Content = "Home" },
                new AtomUIBreadcrumbItem { Content = "Application" }
            }
        };
    }

    private static ItemsPresenter FindPresenter(AtomUIBreadcrumb breadcrumb)
    {
        return breadcrumb.GetVisualDescendants().OfType<ItemsPresenter>().Single();
    }

    private static IDisposable Show(AtomUIBreadcrumb breadcrumb)
    {
        var window = new AvaloniaWindow
        {
            Width   = 800,
            Height  = 600,
            Content = breadcrumb
        };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        return Disposable.Create(window.Close);
    }
}
