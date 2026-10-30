using AtomUI.Controls.Primitives;
using Avalonia.Controls;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AtomUIPagination = AtomUI.Desktop.Controls.Pagination;
using AtomUISimplePagination = AtomUI.Desktop.Controls.SimplePagination;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.Pagination;

public class PaginationRootFrameTests
{
    static PaginationRootFrameTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Pagination_Root_Dash_Properties_Propagate_To_The_Template_Root_Border()
    {
        AssertDashPropagation(new AtomUIPagination
        {
            Total       = 50,
            CurrentPage = 1
        });
    }

    [Fact]
    public void SimplePagination_Root_Dash_Properties_Propagate_To_The_Template_Root_Border()
    {
        AssertDashPropagation(new AtomUISimplePagination
        {
            Total = 50
        });
    }

    [Fact]
    public void Pagination_Root_Dash_Properties_Default_To_Neutral_Values()
    {
        AssertDashDefaults(new AtomUIPagination
        {
            Total       = 50,
            CurrentPage = 1
        });
    }

    [Fact]
    public void SimplePagination_Root_Dash_Properties_Default_To_Neutral_Values()
    {
        AssertDashDefaults(new AtomUISimplePagination
        {
            Total = 50
        });
    }

    private static void AssertDashPropagation(AbstractPagination pagination)
    {
        pagination.BorderDashArray  = new[] { 4d, 2d };
        pagination.BorderDashOffset = 1.5;

        using var window = Show(pagination);

        var rootBorder = pagination.GetVisualChildren().OfType<DashedBorder>().Single();
        rootBorder.StrokeDashArray.ShouldBe(new[] { 4d, 2d });
        rootBorder.StrokeDaskOffset.ShouldBe(1.5);
    }

    private static void AssertDashDefaults(AbstractPagination pagination)
    {
        pagination.BorderDashArray.ShouldBeNull();
        pagination.BorderDashOffset.ShouldBe(0);

        using var window = Show(pagination);

        var rootBorder = pagination.GetVisualChildren().OfType<DashedBorder>().Single();
        rootBorder.StrokeDashArray.ShouldBeNull();
        rootBorder.StrokeDaskOffset.ShouldBe(0);
    }

    private static IDisposable Show(Control content)
    {
        var window = new AvaloniaWindow
        {
            Width   = 640,
            Height  = 260,
            Content = content
        };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        return new WindowLifetime(window);
    }

    private sealed class WindowLifetime(AvaloniaWindow window) : IDisposable
    {
        public void Dispose()
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }
}
