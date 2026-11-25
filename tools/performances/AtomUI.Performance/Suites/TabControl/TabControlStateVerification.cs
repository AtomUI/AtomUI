using AtomUI.Controls;
using AtomUI.Desktop.Controls;
using AtomUI.Icons.AntDesign;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using AvaloniaButton = Avalonia.Controls.Button;
using AvaloniaControl = Avalonia.Controls.Control;
using AtomTabControl = AtomUI.Desktop.Controls.TabControl;
using AtomTabItem = AtomUI.Desktop.Controls.TabItem;
using AtomTabStrip = AtomUI.Desktop.Controls.TabStrip;
using AtomTabStripItem = AtomUI.Desktop.Controls.TabStripItem;

namespace AtomUI.Performance;

internal static partial class Program
{
    private static bool RunTabControlStateVerification()
    {
        var failures = new List<string>();
        VerifyTabItemLazySlots(failures);
        VerifyTabItemCloseBehavior(failures);
        VerifyTabStripItemLazySlots(failures);
        VerifyTabStripItemCloseBehavior(failures);
        VerifyCardAddButtonDoesNotDuplicateRetemplateHandlers(failures);
        VerifyTabControlOverflowPopupCleanup(failures);
        VerifyTabOverflowPopupReopensAfterVisualReattach(CreateOverflowTabControl(), "TabControl", failures);
        VerifyTabOverflowPopupReopensAfterVisualReattach(CreateOverflowTabStrip(), "TabStrip", failures);

        if (failures.Count == 0)
        {
            Console.WriteLine("TabControl state verification passed.");
            return true;
        }

        Console.Error.WriteLine("TabControl state verification failed:");
        foreach (var failure in failures)
        {
            Console.Error.WriteLine($"- {failure}");
        }
        return false;
    }

    private static void VerifyTabItemLazySlots(ICollection<string> failures)
    {
        var tabItem = new AtomTabItem
        {
            Header = "Tab"
        };

        using var realized = RealizeControl(tabItem);
        Expect(FindVisualByName<IconPresenter>(tabItem, "ItemIconPresenter") == null,
            "Default TabItem without Icon should not create ItemIconPresenter.",
            failures);
        Expect(FindVisualByName<IconButton>(tabItem, "PART_ItemCloseButton") == null,
            "Default non-closable TabItem should not create PART_ItemCloseButton.",
            failures);

        var icon = new AppleOutlined();
        tabItem.Icon = icon;
        RefreshLayout(realized.Window);
        var iconPresenter = FindVisualByName<IconPresenter>(tabItem, "ItemIconPresenter");
        Expect(iconPresenter != null,
            "TabItem should create ItemIconPresenter when Icon is assigned.",
            failures);
        Expect(ReferenceEquals(iconPresenter?.Icon, icon),
            "TabItem ItemIconPresenter should mirror the assigned Icon.",
            failures);

        tabItem.Icon = null;
        RefreshLayout(realized.Window);
        Expect(FindVisualByName<IconPresenter>(tabItem, "ItemIconPresenter") == null,
            "TabItem should remove ItemIconPresenter when Icon is cleared.",
            failures);
        Expect(icon.GetVisualParent() == null,
            "Removed TabItem icon should not keep a visual parent.",
            failures);

        tabItem.IsClosable = true;
        tabItem.IsAutoHideCloseButton = false;
        RefreshLayout(realized.Window);
        var closeButton = FindVisualByName<IconButton>(tabItem, "PART_ItemCloseButton");
        Expect(closeButton != null,
            "TabItem should create PART_ItemCloseButton when IsClosable is true.",
            failures);
        Expect(closeButton?.Icon != null,
            "Closable TabItem should provide the default close icon.",
            failures);
        Expect(closeButton?.Opacity == 1.0,
            $"Non-auto-hidden TabItem close button should be opaque, actual {closeButton?.Opacity:0.###}.",
            failures);

        tabItem.IsClosable = false;
        RefreshLayout(realized.Window);
        Expect(FindVisualByName<IconButton>(tabItem, "PART_ItemCloseButton") == null,
            "TabItem should remove PART_ItemCloseButton when IsClosable is false.",
            failures);
        Expect(closeButton?.GetVisualParent() == null,
            "Removed TabItem close button should not keep a visual parent.",
            failures);
    }

    private static void VerifyTabItemCloseBehavior(ICollection<string> failures)
    {
        var tabControl = new AtomTabControl
        {
            IsTabClosable = true
        };
        tabControl.Items.Add(new AtomTabItem { Header = "Tab 1", IsClosable = true });
        tabControl.Items.Add(new AtomTabItem { Header = "Tab 2", IsClosable = true });
        var closedCount = 0;
        tabControl.Closed += (_, _) => closedCount++;

        using var realized = RealizeControl(tabControl);
        var firstItem = tabControl.Items[0] as AtomTabItem;
        var closeButton = firstItem is null
            ? null
            : FindVisualByName<IconButton>(firstItem, "PART_ItemCloseButton");
        closeButton?.RaiseEvent(new RoutedEventArgs(AvaloniaButton.ClickEvent, closeButton));
        RefreshLayout(realized.Window);

        Expect(tabControl.Items.Count == 1,
            $"TabItem close button should remove one tab, actual count {tabControl.Items.Count}.",
            failures);
        Expect(closedCount == 1,
            $"TabItem close button should raise one Closed event, actual {closedCount}.",
            failures);
    }

    private static void VerifyTabStripItemLazySlots(ICollection<string> failures)
    {
        var tabStripItem = new AtomTabStripItem
        {
            Content = "Tab"
        };

        using var realized = RealizeControl(tabStripItem);
        Expect(FindVisualByName<IconPresenter>(tabStripItem, "ItemIconPresenter") == null,
            "Default TabStripItem without Icon should not create ItemIconPresenter.",
            failures);
        Expect(FindVisualByName<IconButton>(tabStripItem, "PART_ItemCloseButton") == null,
            "Default non-closable TabStripItem should not create PART_ItemCloseButton.",
            failures);

        var icon = new AndroidOutlined();
        tabStripItem.Icon = icon;
        RefreshLayout(realized.Window);
        var iconPresenter = FindVisualByName<IconPresenter>(tabStripItem, "ItemIconPresenter");
        Expect(iconPresenter != null,
            "TabStripItem should create ItemIconPresenter when Icon is assigned.",
            failures);
        Expect(ReferenceEquals(iconPresenter?.Icon, icon),
            "TabStripItem ItemIconPresenter should mirror the assigned Icon.",
            failures);

        tabStripItem.Icon = null;
        RefreshLayout(realized.Window);
        Expect(FindVisualByName<IconPresenter>(tabStripItem, "ItemIconPresenter") == null,
            "TabStripItem should remove ItemIconPresenter when Icon is cleared.",
            failures);
        Expect(icon.GetVisualParent() == null,
            "Removed TabStripItem icon should not keep a visual parent.",
            failures);

        tabStripItem.IsClosable = true;
        tabStripItem.IsAutoHideCloseButton = false;
        RefreshLayout(realized.Window);
        var closeButton = FindVisualByName<IconButton>(tabStripItem, "PART_ItemCloseButton");
        Expect(closeButton != null,
            "TabStripItem should create PART_ItemCloseButton when IsClosable is true.",
            failures);
        Expect(closeButton?.Icon != null,
            "Closable TabStripItem should provide the default close icon.",
            failures);

        tabStripItem.IsClosable = false;
        RefreshLayout(realized.Window);
        Expect(FindVisualByName<IconButton>(tabStripItem, "PART_ItemCloseButton") == null,
            "TabStripItem should remove PART_ItemCloseButton when IsClosable is false.",
            failures);
        Expect(closeButton?.GetVisualParent() == null,
            "Removed TabStripItem close button should not keep a visual parent.",
            failures);
    }

    private static void VerifyTabStripItemCloseBehavior(ICollection<string> failures)
    {
        var tabStrip = new AtomTabStrip
        {
            IsTabClosable = true
        };
        tabStrip.Items.Add(new AtomTabStripItem { Content = "Tab 1", IsClosable = true });
        tabStrip.Items.Add(new AtomTabStripItem { Content = "Tab 2", IsClosable = true });
        var closedCount = 0;
        tabStrip.Closed += (_, _) => closedCount++;

        using var realized = RealizeControl(tabStrip);
        var firstItem = tabStrip.Items[0] as AtomTabStripItem;
        var closeButton = firstItem is null
            ? null
            : FindVisualByName<IconButton>(firstItem, "PART_ItemCloseButton");
        closeButton?.RaiseEvent(new RoutedEventArgs(AvaloniaButton.ClickEvent, closeButton));
        RefreshLayout(realized.Window);

        Expect(tabStrip.Items.Count == 1,
            $"TabStripItem close button should remove one tab, actual count {tabStrip.Items.Count}.",
            failures);
        Expect(closedCount == 1,
            $"TabStripItem close button should raise one Closed event, actual {closedCount}.",
            failures);
    }

    private static void VerifyCardAddButtonDoesNotDuplicateRetemplateHandlers(ICollection<string> failures)
    {
        var tabControl = new CardTabControl
        {
            IsShowAddTabButton = true
        };
        tabControl.Items.Add(new AtomTabItem { Header = "Tab 1" });
        var tabControlAddCount = 0;
        tabControl.AddTabRequest += (_, _) => tabControlAddCount++;

        using (var realized = RealizeControl(tabControl))
        {
            tabControl.ApplyTemplate();
            RefreshLayout(realized.Window);
            var addButton = FindVisualByName<IconButton>(tabControl, "PART_AddTabButton");
            addButton?.RaiseEvent(new RoutedEventArgs(AvaloniaButton.ClickEvent, addButton));
            Expect(tabControlAddCount == 1,
                $"CardTabControl add button should raise one AddTabRequest after re-template, actual {tabControlAddCount}.",
                failures);
        }

        var tabStrip = new CardTabStrip
        {
            IsShowAddTabButton = true
        };
        tabStrip.Items.Add(new AtomTabStripItem { Content = "Tab 1" });
        var tabStripAddCount = 0;
        tabStrip.AddTabRequest += (_, _) => tabStripAddCount++;

        using (var realized = RealizeControl(tabStrip))
        {
            tabStrip.ApplyTemplate();
            RefreshLayout(realized.Window);
            var addButton = FindVisualByName<IconButton>(tabStrip, "PART_AddTabButton");
            addButton?.RaiseEvent(new RoutedEventArgs(AvaloniaButton.ClickEvent, addButton));
            Expect(tabStripAddCount == 1,
                $"CardTabStrip add button should raise one AddTabRequest after re-template, actual {tabStripAddCount}.",
                failures);
        }
    }

    private static void VerifyTabControlOverflowPopupCleanup(ICollection<string> failures)
    {
        var tabControl = CreateOverflowTabControl();
        using var realized = RealizeTabOverflowControl(tabControl);
        var context = OpenTabOverflowPopup(tabControl, realized.Window, failures, "TabControl", "initial");
        var scrollViewer = FindVisualByName<TabScrollViewer>(tabControl, "PART_TabsContainer");
        if (context is null || scrollViewer is null)
        {
            return;
        }

        var item = context.Items[0];
        context.Dismiss();
        RefreshLayout(realized.Window);

        Expect(!scrollViewer.IsOverflowPopupOpen && scrollViewer.OverflowPopup?.IsOpen == false,
            "Dismissed TabControl overflow popup should close its actual host and open state.",
            failures);
        Expect(context.Items.Count == 0 && context.SelectedItem is null,
            "Closed TabControl overflow context should release its item and selection snapshot.",
            failures);
        Expect(!context.TryActivate(item) && !context.TryClose(item),
            "Closed TabControl overflow context should reject stale item actions.",
            failures);

        tabControl.SelectedIndex = 2;
        RefreshLayout(realized.Window);
        Expect(context.Items.Count == 0,
            "Closed TabControl overflow context should not republish after owner selection changes.",
            failures);
    }

    private static void VerifyTabOverflowPopupReopensAfterVisualReattach(
        AvaloniaControl innerOwner,
        string label,
        ICollection<string> failures)
    {
        var outerTabControl = new AtomTabControl { IsMotionEnabled = false };
        outerTabControl.Items.Add(new AtomTabItem
        {
            Header = label,
            Content = innerOwner
        });
        outerTabControl.Items.Add(new AtomTabItem
        {
            Header = "Other",
            Content = new Avalonia.Controls.TextBlock { Text = "Other content" }
        });

        using var realized = RealizeTabOverflowControl(outerTabControl);
        var firstContext = OpenTabOverflowPopup(innerOwner, realized.Window, failures, label, "initial");
        var scrollViewer = FindVisualByName<TabScrollViewer>(innerOwner, "PART_TabsContainer");
        var firstPopup = scrollViewer?.OverflowPopup;

        outerTabControl.SelectedIndex = 1;
        RefreshLayout(realized.Window);

        Expect(firstPopup?.IsOpen == false && scrollViewer?.IsOverflowPopupOpen == false,
            $"Open {label} overflow popup should close when detached by a parent tab switch.",
            failures);
        Expect(scrollViewer is not null && scrollViewer.OverflowPopupContext is null &&
               firstContext is { Items.Count: 0, SelectedItem: null },
            $"Detached {label} overflow popup should release its context and snapshot.",
            failures);

        outerTabControl.SelectedIndex = 0;
        RefreshLayout(realized.Window);

        var secondContext = OpenTabOverflowPopup(innerOwner, realized.Window, failures, label, "reattached");
        Expect(firstContext is not null && secondContext is not null &&
               !ReferenceEquals(firstContext, secondContext),
            $"Reattached {label} overflow popup should open a fresh context after a parent tab switch.",
            failures);

        outerTabControl.SelectedIndex = 1;
        RefreshLayout(realized.Window);
    }

    private static AtomTabControl CreateOverflowTabControl()
    {
        var tabControl = new AtomTabControl
        {
            Width = 260,
            IsMotionEnabled = false
        };
        for (var i = 0; i < 16; i++)
        {
            tabControl.Items.Add(new AtomTabItem
            {
                Header  = $"Long Tab Header {i + 1}",
                Content = $"Content {i + 1}"
            });
        }

        return tabControl;
    }

    private static AtomTabStrip CreateOverflowTabStrip()
    {
        var tabStrip = new AtomTabStrip
        {
            Width = 260,
            IsMotionEnabled = false
        };
        for (var i = 0; i < 16; i++)
        {
            tabStrip.Items.Add(new AtomTabStripItem
            {
                Content = $"Long Tab Header {i + 1}"
            });
        }

        return tabStrip;
    }

    private static RealizedScenario RealizeTabOverflowControl(AvaloniaControl owner)
    {
        // The styled Window supplies a real popup overlay host in headless mode.
        var window = new AtomUI.Desktop.Controls.Window
        {
            Width = MeasureSize.Width,
            Height = 900,
            Content = owner,
            ShowInTaskbar = false
        };
        window.Show();
        RefreshLayout(window);
        return new RealizedScenario(window, [owner]);
    }

    private static TabOverflowPopupContext? OpenTabOverflowPopup(
        AvaloniaControl owner,
        Avalonia.Controls.Window window,
        ICollection<string> failures,
        string label,
        string phase)
    {
        var scrollViewer = FindVisualByName<TabScrollViewer>(owner, "PART_TabsContainer");
        var menuIndicator = scrollViewer is null
            ? null
            : FindVisualByName<IconButton>(scrollViewer, "PART_ScrollMenuIndicator");
        Expect(scrollViewer != null,
            $"Overflow {label} should realize PART_TabsContainer during {phase} open.",
            failures);
        Expect(menuIndicator?.IsVisible == true,
            $"Overflow {label} should show the scroll menu indicator during {phase} open.",
            failures);
        if (scrollViewer is null || menuIndicator is null)
        {
            return null;
        }

        try
        {
            menuIndicator.RaiseEvent(new RoutedEventArgs(AvaloniaButton.ClickEvent, menuIndicator));
        }
        catch (Exception exception)
        {
            failures.Add(
                $"Overflow {label} popup threw during {phase} click: {exception.GetType().Name}: {exception.Message}");
            return null;
        }
        finally
        {
            RefreshLayout(window);
        }

        Expect(scrollViewer.IsOverflowPopupOpen && scrollViewer.OverflowPopup?.IsOpen == true,
            $"Overflow {label} should open its actual popup during {phase} click.",
            failures);
        var context = scrollViewer.OverflowPopupContext;
        Expect(context is { Items.Count: > 0 },
            $"Overflow {label} should publish a populated context during {phase} click.",
            failures);
        return context is { Items.Count: > 0 } ? context : null;
    }

    private static bool IsMissingPopupHostException(InvalidOperationException exception)
    {
        return exception.Message.Contains("IPopupImpl") ||
               exception.Message.Contains("overlay layer");
    }

}
