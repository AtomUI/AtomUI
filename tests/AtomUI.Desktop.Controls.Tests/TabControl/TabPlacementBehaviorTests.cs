using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using Shouldly;
using Xunit;
using AtomCardTabControl = AtomUI.Desktop.Controls.CardTabControl;
using AtomCardTabStrip = AtomUI.Desktop.Controls.CardTabStrip;
using AtomTabControl = AtomUI.Desktop.Controls.TabControl;
using AtomTabItem = AtomUI.Desktop.Controls.TabItem;
using AtomTabStrip = AtomUI.Desktop.Controls.TabStrip;
using AtomTabStripItem = AtomUI.Desktop.Controls.TabStripItem;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.TabControl;

public class TabPlacementBehaviorTests
{
    static TabPlacementBehaviorTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Theory]
    [InlineData(Dock.Left)]
    [InlineData(Dock.Right)]
    public void TabControl_Default_Line_Vertical_Placement_Uses_Compact_Item_Gap_And_Height(Dock placement)
    {
        var tabControl = CreateTabControl(placement);

        ShowInWindow(tabControl, _ =>
        {
            GetVerticalGap(tabControl, 0, 1).ShouldBeLessThanOrEqualTo(4);
            GetVerticalGap(tabControl, 1, 2).ShouldBeLessThanOrEqualTo(4);
            GetContainer<Control>(tabControl, 0).Bounds.Height.ShouldBeLessThanOrEqualTo(28);
            GetContainer<Control>(tabControl, 1).Bounds.Height.ShouldBeLessThanOrEqualTo(28);
        });
    }

    [Theory]
    [InlineData(Dock.Left)]
    [InlineData(Dock.Right)]
    public void TabStrip_Default_Line_Vertical_Placement_Uses_Compact_Item_Gap_And_Height(Dock placement)
    {
        var tabStrip = CreateTabStrip(placement);

        ShowInWindow(tabStrip, _ =>
        {
            GetVerticalGap(tabStrip, 0, 1).ShouldBeLessThanOrEqualTo(4);
            GetVerticalGap(tabStrip, 1, 2).ShouldBeLessThanOrEqualTo(4);
            GetContainer<Control>(tabStrip, 0).Bounds.Height.ShouldBeLessThanOrEqualTo(28);
            GetContainer<Control>(tabStrip, 1).Bounds.Height.ShouldBeLessThanOrEqualTo(28);
        });
    }

    [Fact]
    public void Card_Vertical_Placement_Keeps_Current_Item_Height_Larger_Than_Line()
    {
        var lineTabControl = CreateTabControl(Dock.Left);
        var cardTabControl = CreateCardTabControl(Dock.Left);
        var lineTabStrip   = CreateTabStrip(Dock.Left);
        var cardTabStrip   = CreateCardTabStrip(Dock.Left);

        ShowInWindow(new StackPanel
        {
            Children =
            {
                lineTabControl,
                cardTabControl,
                lineTabStrip,
                cardTabStrip
            }
        }, _ =>
        {
            GetContainer<Control>(cardTabControl, 0).Bounds.Height
                .ShouldBeGreaterThan(GetContainer<Control>(lineTabControl, 0).Bounds.Height);
            GetContainer<Control>(cardTabStrip, 0).Bounds.Height
                .ShouldBeGreaterThan(GetContainer<Control>(lineTabStrip, 0).Bounds.Height);
        });
    }

    [Fact]
    public void TabControl_Changing_Placement_Preserves_Selected_Direct_TabItem()
    {
        var tabControl = CreateTabControl(Dock.Top);

        ShowInWindow(tabControl, _ =>
        {
            var selectedItem = tabControl.Items[1];
            tabControl.SelectedIndex = 1;
            Dispatcher.UIThread.RunJobs();

            foreach (var placement in new[] { Dock.Right, Dock.Bottom, Dock.Left, Dock.Top })
            {
                tabControl.TabStripPlacement = placement;
                Dispatcher.UIThread.RunJobs();

                tabControl.SelectedItem.ShouldBeSameAs(selectedItem);
                tabControl.SelectedIndex.ShouldBe(1);
            }
        });
    }

    [Fact]
    public void TabStrip_Changing_Placement_Preserves_Selected_Direct_TabStripItem()
    {
        var tabStrip = CreateTabStrip(Dock.Top);

        ShowInWindow(tabStrip, _ =>
        {
            var selectedItem = tabStrip.Items[1];
            tabStrip.SelectedIndex = 1;
            Dispatcher.UIThread.RunJobs();

            foreach (var placement in new[] { Dock.Right, Dock.Bottom, Dock.Left, Dock.Top })
            {
                tabStrip.TabStripPlacement = placement;
                Dispatcher.UIThread.RunJobs();

                tabStrip.SelectedItem.ShouldBeSameAs(selectedItem);
                tabStrip.SelectedIndex.ShouldBe(1);
            }
        });
    }

    private static AtomTabControl CreateTabControl(Dock placement)
    {
        var tabControl = new AtomTabControl
        {
            Width             = 360,
            Height            = 220,
            TabStripPlacement = placement,
            SelectedIndex     = 0
        };
        tabControl.Items.Add(new AtomTabItem { Header = "Tab 1", Content = "Content 1" });
        tabControl.Items.Add(new AtomTabItem { Header = "Tab 2", Content = "Content 2" });
        tabControl.Items.Add(new AtomTabItem { Header = "Tab 3", Content = "Content 3" });
        return tabControl;
    }

    private static AtomTabStrip CreateTabStrip(Dock placement)
    {
        var tabStrip = new AtomTabStrip
        {
            Width             = 360,
            Height            = 220,
            TabStripPlacement = placement,
            SelectedIndex     = 0
        };
        tabStrip.Items.Add(new AtomTabStripItem { Content = "Tab 1" });
        tabStrip.Items.Add(new AtomTabStripItem { Content = "Tab 2" });
        tabStrip.Items.Add(new AtomTabStripItem { Content = "Tab 3" });
        return tabStrip;
    }

    private static AtomCardTabControl CreateCardTabControl(Dock placement)
    {
        var tabControl = new AtomCardTabControl
        {
            Width             = 360,
            Height            = 220,
            TabStripPlacement = placement,
            SelectedIndex     = 0
        };
        tabControl.Items.Add(new AtomTabItem { Header = "Tab 1", Content = "Content 1" });
        tabControl.Items.Add(new AtomTabItem { Header = "Tab 2", Content = "Content 2" });
        tabControl.Items.Add(new AtomTabItem { Header = "Tab 3", Content = "Content 3" });
        return tabControl;
    }

    private static AtomCardTabStrip CreateCardTabStrip(Dock placement)
    {
        var tabStrip = new AtomCardTabStrip
        {
            Width             = 360,
            Height            = 220,
            TabStripPlacement = placement,
            SelectedIndex     = 0
        };
        tabStrip.Items.Add(new AtomTabStripItem { Content = "Tab 1" });
        tabStrip.Items.Add(new AtomTabStripItem { Content = "Tab 2" });
        tabStrip.Items.Add(new AtomTabStripItem { Content = "Tab 3" });
        return tabStrip;
    }

    private static double GetVerticalGap(ItemsControl owner, int firstIndex, int secondIndex)
    {
        var first  = GetContainer<Control>(owner, firstIndex);
        var second = GetContainer<Control>(owner, secondIndex);
        var firstTop = first.TranslatePoint(default, owner);
        var secondTop = second.TranslatePoint(default, owner);
        firstTop.ShouldNotBeNull();
        secondTop.ShouldNotBeNull();
        return secondTop.Value.Y - firstTop.Value.Y - first.Bounds.Height;
    }

    private static T GetContainer<T>(ItemsControl owner, int index)
        where T : Control
    {
        RunJobsUntil(() => owner.ContainerFromIndex(index) is T);
        var container = owner.ContainerFromIndex(index);
        container.ShouldNotBeNull();
        container.ShouldBeAssignableTo<T>();
        return (T)container!;
    }

    private static void ShowInWindow(Control content, Action<AvaloniaWindow> assertion)
    {
        var window = new AvaloniaWindow
        {
            Width   = 520,
            Height  = 360,
            Content = content
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            assertion(window);
        }
        finally
        {
            window.Close();
        }
    }

    private static void RunJobsUntil(Func<bool> condition, int maxPasses = 128)
    {
        for (var i = 0; i < maxPasses; i++)
        {
            Dispatcher.UIThread.RunJobs();
            if (condition())
            {
                return;
            }
        }
    }
}
