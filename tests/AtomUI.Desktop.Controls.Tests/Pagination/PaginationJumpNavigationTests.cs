using AtomUI.Icons.AntDesign;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.Pagination;

public class PaginationJumpNavigationTests
{
    static PaginationJumpNavigationTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Real_Template_Renders_Ant_Design_Window_And_Routes_Pointer_And_Enter_Jumps()
    {
        using var fixture = CreateFixture();
        var items = GetVisibleNavigationItems(fixture.Pagination);

        items.Select(item => (item.PaginationItemType, item.PageNumber)).ShouldBe(
        [
            (PaginationItemType.Previous, 5),
            (PaginationItemType.PageIndicator, 1),
            (PaginationItemType.JumpPrevious, 1),
            (PaginationItemType.PageIndicator, 5),
            (PaginationItemType.PageIndicator, 6),
            (PaginationItemType.PageIndicator, 7),
            (PaginationItemType.JumpNext, 11),
            (PaginationItemType.PageIndicator, 50),
            (PaginationItemType.Next, 7)
        ]);

        Click(fixture.Window, items.Single(item => item.PaginationItemType == PaginationItemType.JumpNext));
        fixture.Pagination.CurrentPage.ShouldBe(11);

        var jumpPrevious = GetVisibleNavigationItems(fixture.Pagination)
            .Single(item => item.PaginationItemType == PaginationItemType.JumpPrevious);
        jumpPrevious.Focus(NavigationMethod.Tab).ShouldBeTrue();
        fixture.Window.KeyPress(Key.Enter, RawInputModifiers.None, PhysicalKey.Enter, null);
        fixture.Pagination.CurrentPage.ShouldBe(6);
    }

    [Fact]
    public void Jump_Items_Swap_Visuals_For_Pointer_And_Keyboard_Focus()
    {
        using var fixture = CreateFixture(isMotionEnabled: false);
        var jumpNext = GetVisibleNavigationItems(fixture.Pagination)
            .Single(item => item.PaginationItemType == PaginationItemType.JumpNext);
        var (ellipsis, jumpIcon) = GetJumpVisuals(jumpNext);

        ellipsis.Opacity.ShouldBe(1);
        jumpIcon.Opacity.ShouldBe(0);

        fixture.Window.MouseMove(CenterOf(jumpNext, fixture.Window));
        ellipsis.Opacity.ShouldBe(0);
        jumpIcon.Opacity.ShouldBe(1);

        fixture.Window.MouseMove(new Point(0, 0));
        var jumpPrevious = GetVisibleNavigationItems(fixture.Pagination)
            .Single(item => item.PaginationItemType == PaginationItemType.JumpPrevious);
        jumpPrevious.Focus(NavigationMethod.Tab).ShouldBeTrue();
        (ellipsis, jumpIcon) = GetJumpVisuals(jumpPrevious);
        ellipsis.Opacity.ShouldBe(0);
        jumpIcon.Opacity.ShouldBe(1);
        jumpPrevious.IsFocused.ShouldBeTrue();
        AutomationProperties.GetName(jumpPrevious).ShouldBe("Previous 5 Pages");
    }

    [Fact]
    public void Disabled_Jump_Item_Keeps_Ellipsis_And_Cannot_Navigate()
    {
        using var fixture = CreateFixture(isEnabled: false, isMotionEnabled: false);
        var jumpNext = GetVisibleNavigationItems(fixture.Pagination)
            .Single(item => item.PaginationItemType == PaginationItemType.JumpNext);

        fixture.Window.MouseMove(CenterOf(jumpNext, fixture.Window));
        var (ellipsis, jumpIcon) = GetJumpVisuals(jumpNext);
        ellipsis.Opacity.ShouldBe(1);
        jumpIcon.Opacity.ShouldBe(0);

        Click(fixture.Window, jumpNext);
        fixture.Pagination.CurrentPage.ShouldBe(6);
    }

    [Fact]
    public void Clicking_A_Stable_Jump_Item_Preserves_Pointer_Over_And_Double_Arrow()
    {
        using var fixture = CreateFixture(currentPage: 39, isMotionEnabled: false);
        var jumpPrevious = GetVisibleNavigationItems(fixture.Pagination)
            .Single(item => item.PaginationItemType == PaginationItemType.JumpPrevious);

        fixture.Window.MouseMove(CenterOf(jumpPrevious, fixture.Window));
        jumpPrevious.IsPointerOver.ShouldBeTrue();
        var (ellipsisBefore, jumpIconBefore) = GetJumpVisuals(jumpPrevious);
        var ellipsisIconBefore = jumpPrevious.Icon;
        var doubleArrowIconBefore = jumpPrevious.JumpIcon;
        ellipsisBefore.Opacity.ShouldBe(0);
        jumpIconBefore.Opacity.ShouldBe(1);

        Click(fixture.Window, jumpPrevious);

        fixture.Pagination.CurrentPage.ShouldBe(34);
        var jumpPreviousAfter = GetVisibleNavigationItems(fixture.Pagination)
            .Single(item => item.PaginationItemType == PaginationItemType.JumpPrevious);
        jumpPreviousAfter.ShouldBeSameAs(jumpPrevious);
        jumpPreviousAfter.Icon.ShouldBeSameAs(ellipsisIconBefore);
        jumpPreviousAfter.JumpIcon.ShouldBeSameAs(doubleArrowIconBefore);
        jumpPreviousAfter.IsPointerOver.ShouldBeTrue();
        var (ellipsisAfter, jumpIconAfter) = GetJumpVisuals(jumpPreviousAfter);
        ellipsisAfter.Opacity.ShouldBe(0);
        jumpIconAfter.Opacity.ShouldBe(1);
    }

    [Fact]
    public void Right_To_Left_Flow_Reverses_Icons_Without_Changing_Jump_Targets()
    {
        using var fixture = CreateFixture();
        fixture.Pagination.FlowDirection = FlowDirection.RightToLeft;
        Dispatcher.UIThread.RunJobs();
        var items = GetVisibleNavigationItems(fixture.Pagination);

        items.Single(item => item.PaginationItemType == PaginationItemType.Previous).Icon.ShouldBeOfType<RightOutlined>();
        items.Single(item => item.PaginationItemType == PaginationItemType.JumpPrevious).JumpIcon.ShouldBeOfType<DoubleRightOutlined>();
        items.Single(item => item.PaginationItemType == PaginationItemType.JumpNext).JumpIcon.ShouldBeOfType<DoubleLeftOutlined>();
        items.Single(item => item.PaginationItemType == PaginationItemType.Next).Icon.ShouldBeOfType<LeftOutlined>();
        items.Single(item => item.PaginationItemType == PaginationItemType.JumpPrevious).PageNumber.ShouldBe(1);
        items.Single(item => item.PaginationItemType == PaginationItemType.JumpNext).PageNumber.ShouldBe(11);
    }

    [Fact]
    public void Less_Items_And_Hidden_Jumpers_Reconfigure_The_Realized_Navigation()
    {
        using var fixture = CreateFixture();
        var pageChangeCount = 0;
        fixture.Pagination.CurrentPageChanged += (_, _) => pageChangeCount++;

        fixture.Pagination.IsShowLessItems = true;
        Dispatcher.UIThread.RunJobs();
        var lessItems = GetVisibleNavigationItems(fixture.Pagination);
        lessItems.Single(item => item.PaginationItemType == PaginationItemType.JumpPrevious)
                 .PageNumber.ShouldBe(3);
        lessItems.Single(item => item.PaginationItemType == PaginationItemType.JumpNext)
                 .PageNumber.ShouldBe(9);

        fixture.Pagination.IsShowPrevNextJumpers = false;
        Dispatcher.UIThread.RunJobs();
        var hiddenJumpers = GetVisibleNavigationItems(fixture.Pagination);
        hiddenJumpers.ShouldNotContain(item =>
            item.PaginationItemType == PaginationItemType.JumpPrevious ||
            item.PaginationItemType == PaginationItemType.JumpNext);
        hiddenJumpers.Where(item => item.PaginationItemType == PaginationItemType.PageIndicator)
                     .Select(item => item.PageNumber)
                     .ShouldBe([1, 5, 6, 7, 50]);
        pageChangeCount.ShouldBe(0);
    }

    private static PaginationFixture CreateFixture(
        int currentPage = 6,
        bool isEnabled = true,
        bool isMotionEnabled = true)
    {
        var pagination = new global::AtomUI.Desktop.Controls.Pagination
        {
            Total = 500,
            CurrentPage = currentPage,
            Width = 700,
            IsEnabled = isEnabled,
            IsMotionEnabled = isMotionEnabled,
            HorizontalAlignment = HorizontalAlignment.Left
        };
        var window = new AvaloniaWindow
        {
            Width = 760,
            Height = 180,
            Background = Brushes.White,
            Content = new Border
            {
                Padding = new Thickness(24),
                Child = pagination
            }
        };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        return new PaginationFixture(window, pagination);
    }

    private static List<PaginationNavItem> GetVisibleNavigationItems(
        global::AtomUI.Desktop.Controls.Pagination pagination)
    {
        return pagination.GetVisualDescendants()
                         .OfType<PaginationNavItem>()
                         .Where(item => item.IsVisible)
                         .ToList();
    }

    private static void Click(AvaloniaWindow window, PaginationNavItem item)
    {
        var center = CenterOf(item, window);
        window.MouseMove(center);
        window.MouseDown(center, MouseButton.Left);
        window.MouseUp(center, MouseButton.Left);
    }

    private static Point CenterOf(PaginationNavItem item, AvaloniaWindow window)
    {
        return item.TranslatePoint(
                   new Point(item.Bounds.Width / 2, item.Bounds.Height / 2),
                   window) ?? throw new InvalidOperationException("Unable to locate pagination item.");
    }

    private static (Control Ellipsis, Control JumpIcon) GetJumpVisuals(
        PaginationNavItem item)
    {
        var controls = item.GetVisualDescendants().OfType<Control>().ToList();
        return (
            controls.Single(control => control.Name == "IconPresenter"),
            controls.Single(control => control.Name == "JumpIconPresenter"));
    }

    private sealed class PaginationFixture : IDisposable
    {
        public PaginationFixture(
            AvaloniaWindow window,
            global::AtomUI.Desktop.Controls.Pagination pagination)
        {
            Window = window;
            Pagination = pagination;
        }

        public AvaloniaWindow Window { get; }

        public global::AtomUI.Desktop.Controls.Pagination Pagination { get; }

        public void Dispose()
        {
            Window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }
}
