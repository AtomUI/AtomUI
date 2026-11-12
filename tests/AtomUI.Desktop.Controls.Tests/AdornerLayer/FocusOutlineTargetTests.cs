using AtomUI.Controls.Primitives;
using AtomUI.Desktop.Controls.Internal.DateViewer;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.AdornerLayerTests;

// Keyboard stops and focus outline targets that follow Ant Design: one stop per toggle, no outline on
// controls with their own focus visual, and outlines on the keyboard active item of trees and menus.
public class FocusOutlineTargetTests
{
    static FocusOutlineTargetTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Collapse_Header_Mode_Has_One_Stop_Per_Item_On_The_Header()
    {
        var first    = new Desktop.Controls.CollapseItem { Header = "First", Content = "Body" };
        var second   = new Desktop.Controls.CollapseItem { Header = "Second", Content = "Body" };
        var collapse = new Desktop.Controls.Collapse { IsMotionEnabled = false };
        collapse.Items.Add(first);
        collapse.Items.Add(second);

        ShowInWindow(collapse, (_, before) =>
        {
            CollectTabStops(before, collapse).ShouldBe([first, second]);
        });
    }

    [Fact]
    public void Collapse_Icon_Mode_Stop_Is_The_Expand_Button_And_Toggles()
    {
        var item     = new Desktop.Controls.CollapseItem { Header = "First", Content = "Body" };
        var collapse = new Desktop.Controls.Collapse { IsMotionEnabled = false, TriggerType = CollapseTriggerType.Icon };
        collapse.Items.Add(item);

        ShowInWindow(collapse, (window, before) =>
        {
            var stops = CollectTabStops(before, collapse);
            stops.Count.ShouldBe(1);
            var expandButton = stops[0].ShouldBeOfType<IconButton>();
            expandButton.Name.ShouldBe("PART_ExpandButton");

            Focus(expandButton, NavigationMethod.Tab);
            AdornedBy(GetFocusOutline(expandButton)).ShouldBeSameAs(expandButton);

            window.KeyPress(Key.Enter, RawInputModifiers.None, PhysicalKey.Enter, null);
            Dispatcher.UIThread.RunJobs();
            item.IsSelected.ShouldBeTrue();
        });
    }

    [Fact]
    public void Expander_Header_Mode_Header_Is_The_Stop_And_Keys_Toggle()
    {
        var expander = new Desktop.Controls.Expander
        {
            Header = "Header", Content = "Body", IsMotionEnabled = false, Width = 240
        };

        ShowInWindow(expander, (window, before) =>
        {
            CollectTabStops(before, expander).ShouldBe([expander]);

            Focus(expander, NavigationMethod.Tab);
            AdornedBy(GetFocusOutline(expander)).Name.ShouldBe("PART_HeaderDecorator");

            window.KeyPress(Key.Enter, RawInputModifiers.None, PhysicalKey.Enter, null);
            Dispatcher.UIThread.RunJobs();
            expander.IsExpanded.ShouldBeTrue();

            window.KeyPress(Key.Space, RawInputModifiers.None, PhysicalKey.Space, null);
            Dispatcher.UIThread.RunJobs();
            expander.IsExpanded.ShouldBeFalse();
        });
    }

    [Fact]
    public void Expander_Outline_Follows_The_Header_Corners_Of_The_Rounded_Frame()
    {
        var expander = new Desktop.Controls.Expander
        {
            Header = "Header", Content = "Body", IsMotionEnabled = false, Width = 240
        };

        ShowInWindow(expander, (_, _) =>
        {
            var frame  = expander.GetVisualDescendants().OfType<DashedBorder>().First(b => b.Name == "PART_Frame");
            var radius = frame.CornerRadius;
            radius.TopLeft.ShouldBeGreaterThan(0);
            var shape = (IFocusOutlineShapeProvider)expander;

            shape.FocusOutlineCornerRadius.ShouldBe(radius);

            expander.IsExpanded = true;
            Dispatcher.UIThread.RunJobs();
            shape.FocusOutlineCornerRadius.ShouldBe(new CornerRadius(radius.TopLeft, radius.TopRight, 0, 0));

            expander.ExpandDirection = ExpandDirection.Up;
            Dispatcher.UIThread.RunJobs();
            shape.FocusOutlineCornerRadius.ShouldBe(new CornerRadius(0, 0, radius.BottomRight, radius.BottomLeft));
        });
    }

    [Fact]
    public void Expander_Icon_Mode_Stop_Is_The_Expand_Button()
    {
        var expander = new Desktop.Controls.Expander
        {
            Header = "Header", Content = "Body", IsMotionEnabled = false, Width = 240,
            TriggerType = ExpanderTriggerType.Icon
        };

        ShowInWindow(expander, (_, before) =>
        {
            var stops = CollectTabStops(before, expander);
            stops.Count.ShouldBe(1);
            stops[0].ShouldBeOfType<IconButton>().Name.ShouldBe("PART_ExpandButton");
        });
    }

    [Fact]
    public void Closable_Tag_Stop_Is_The_Close_Button()
    {
        // Ant Design renders the close icon as role=button with tabIndex 0; the tag itself is not a stop.
        var tag = new Desktop.Controls.Tag { Text = "Tag", IsClosable = true };

        ShowInWindow(tag, (_, before) =>
        {
            var stops = CollectTabStops(before, tag);
            stops.Count.ShouldBe(1);
            var closeButton = stops[0].ShouldBeOfType<IconButton>();
            closeButton.Name.ShouldBe("PART_CloseButton");

            Focus(closeButton, NavigationMethod.Tab);
            AdornedBy(GetFocusOutline(closeButton)).ShouldBeSameAs(closeButton);
            closeButton.CornerRadius.TopLeft.ShouldBeGreaterThan(0);
        });
    }

    // Date and time pickers are left out: their frame is still their only keyboard stop, pending the picker
    // keyboard design (audit item "keyboard P1").
    public static TheoryData<string> InputHosts => new()
    {
        nameof(Desktop.Controls.LineEdit), nameof(Desktop.Controls.SearchEdit), nameof(Desktop.Controls.TextArea),
        "ComboBox", "EditableComboBox", nameof(Desktop.Controls.Select), nameof(Desktop.Controls.TreeSelect),
        nameof(Desktop.Controls.Cascader), nameof(Desktop.Controls.NumericUpDown), nameof(Desktop.Controls.Mentions),
        nameof(Desktop.Controls.AutoComplete)
    };

    // Input frames are focusable only so that a click on their padding focuses the input; they must not add a
    // keyboard stop of their own (a Tab press would land on an invisible element). Every host stays reachable.
    [Theory]
    [MemberData(nameof(InputHosts))]
    public void Input_Frames_Are_Not_Tab_Stops(string hostName)
    {
        Control host = hostName switch
        {
            nameof(Desktop.Controls.LineEdit) => new Desktop.Controls.LineEdit { Text = "text" },
            nameof(Desktop.Controls.SearchEdit) => new Desktop.Controls.SearchEdit { Text = "text" },
            nameof(Desktop.Controls.TextArea) => new Desktop.Controls.TextArea { Text = "text" },
            "ComboBox" => new Desktop.Controls.ComboBox { ItemsSource = new[] { "A", "B" } },
            "EditableComboBox" => new Desktop.Controls.ComboBox { ItemsSource = new[] { "A", "B" }, IsEditable = true },
            nameof(Desktop.Controls.Select) => new Desktop.Controls.Select(),
            nameof(Desktop.Controls.TreeSelect) => new Desktop.Controls.TreeSelect(),
            nameof(Desktop.Controls.Cascader) => new Desktop.Controls.Cascader(),
            nameof(Desktop.Controls.NumericUpDown) => new Desktop.Controls.NumericUpDown(),
            nameof(Desktop.Controls.Mentions) => new Desktop.Controls.Mentions(),
            _ => new Desktop.Controls.AutoComplete()
        };
        host.Width = 260;

        ShowInWindow(host, (_, before) =>
        {
            var stops = CollectTabStops(before, host);
            var description = string.Join(" > ", stops.Select(s => $"{s.GetType().Name}#{(s as Control)?.Name}"));
            stops.ShouldNotBeEmpty(description);
            stops.OfType<Control>().Where(s => s is InputControlFrame)
                 .ShouldBeEmpty($"{hostName}: {description}");
            // Ant Design: the select arrow and the number step handlers are not focusable either.
            stops.OfType<Control>()
                 .Where(s => s.Name is "PART_OpenIndicatorButton" or "PART_IncreaseButton" or "PART_DecreaseButton")
                 .ShouldBeEmpty($"{hostName}: {description}");
        });
    }

    public static TheoryData<string> SelfFocusedControls => new()
    {
        nameof(Desktop.Controls.Slider),
        nameof(Desktop.Controls.OtpLineEdit),
        nameof(Desktop.Controls.Carousel),
        nameof(Desktop.Controls.Calendar)
    };

    // These controls draw their own focus state (handle ring, active cell shadow, dot opacity, cell border).
    [Theory]
    [MemberData(nameof(SelfFocusedControls))]
    public void Controls_With_Own_Focus_Visual_Add_No_Outline(string controlName)
    {
        Control control = controlName switch
        {
            nameof(Desktop.Controls.Slider) => new Desktop.Controls.Slider { Width = 200, Value = 30 },
            nameof(Desktop.Controls.OtpLineEdit) => new Desktop.Controls.OtpLineEdit { Length = 4 },
            nameof(Desktop.Controls.Carousel) => CreateCarousel(),
            _ => new Desktop.Controls.Calendar { Fullscreen = true }
        };

        ShowInWindow(control, (_, before) =>
        {
            InputElement target = controlName switch
            {
                nameof(Desktop.Controls.Slider) => control.GetVisualDescendants().OfType<SliderThumb>().First(),
                nameof(Desktop.Controls.Carousel) => control.GetVisualDescendants().OfType<CarouselPageIndicator>().First(),
                nameof(Desktop.Controls.Calendar) => control.GetVisualDescendants().OfType<DateViewerCell>().First(c => c.Focusable),
                _ => control
            };

            Focus(target, NavigationMethod.Tab);
            FindFocusOutlines(target).ShouldBeEmpty($"{controlName}: {target.GetType().Name}");
        });
    }

    [Fact]
    public void Carousel_Indicator_Keyboard_Focus_Uses_Hover_Opacity()
    {
        var carousel = CreateCarousel();

        ShowInWindow(carousel, (_, _) =>
        {
            var indicator = carousel.GetVisualDescendants().OfType<CarouselPageIndicator>()
                                    .First(i => !i.IsSelected);
            var idle = indicator.FrameOpacity;

            Focus(indicator, NavigationMethod.Tab);

            indicator.FrameOpacity.ShouldBe(0.75, 0.001);
            idle.ShouldBeLessThan(indicator.FrameOpacity);
        });
    }

    // Compact cells draw no focus border of their own (it would vanish on the selected primary fill), so the
    // shared outline wraps the date box, as in the picker panels; content cells keep their top primary line.
    [Fact]
    public void Compact_Calendar_Cell_Outline_Wraps_The_Date_Box()
    {
        var calendar = new Desktop.Controls.Calendar { Fullscreen = false };

        ShowInWindow(calendar, (_, _) =>
        {
            var cell = calendar.GetVisualDescendants().OfType<DateViewerCell>().First(c => c.Focusable);

            Focus(cell, NavigationMethod.Directional);

            AdornedBy(GetFocusOutline(cell)).Name.ShouldBe("PART_CellInner");
        });
    }

    [Fact]
    public void Date_Viewer_Cell_Outline_Wraps_The_Value_Frame()
    {
        var dateViewer = new Desktop.Controls.DateViewer();

        ShowInWindow(dateViewer, (_, _) =>
        {
            var cell = dateViewer.GetVisualDescendants().OfType<DateViewerCell>().First(c => c.Focusable);

            Focus(cell, NavigationMethod.Directional);

            AdornedBy(GetFocusOutline(cell)).Name.ShouldBe("PART_ValueFrame");
        });
    }

    [Fact]
    public void Line_Tab_Outline_Wraps_The_Tab_Button_Row()
    {
        var first   = new Desktop.Controls.TabItem { Header = "First", Content = "1" };
        var tabs    = new Desktop.Controls.TabControl { IsMotionEnabled = false, Width = 300 };
        tabs.Items.Add(first);
        tabs.Items.Add(new Desktop.Controls.TabItem { Header = "Second", Content = "2" });

        ShowInWindow(tabs, (_, _) =>
        {
            Focus(first, NavigationMethod.Tab);

            var adorned = AdornedBy(GetFocusOutline(first));
            adorned.ShouldBeOfType<StackPanel>();
            adorned.TemplatedParent.ShouldBeSameAs(first);
            FocusOutlineAdorner.GetIsInset(adorned).ShouldBeFalse();
        });
    }

    [Fact]
    public void Card_Tab_Outline_Is_Inset_On_The_Card_Frame()
    {
        var first = new Desktop.Controls.TabItem { Header = "First", Content = "1" };
        var tabs  = new Desktop.Controls.CardTabControl { IsMotionEnabled = false, Width = 300 };
        tabs.Items.Add(first);
        tabs.Items.Add(new Desktop.Controls.TabItem { Header = "Second", Content = "2" });

        ShowInWindow(tabs, (_, _) =>
        {
            Focus(first, NavigationMethod.Tab);

            var adorned = AdornedBy(GetFocusOutline(first));
            adorned.Name.ShouldBe("Frame");
            FocusOutlineAdorner.GetIsInset(adorned).ShouldBeTrue();
        });
    }

    [Fact]
    public void Tab_Strip_Items_Use_The_Same_Outline_Targets()
    {
        var lineItem = new Desktop.Controls.TabStripItem { Content = "Line" };
        var cardItem = new Desktop.Controls.TabStripItem { Content = "Card" };
        var line     = new Desktop.Controls.TabStrip { IsMotionEnabled = false, Width = 300 };
        var card     = new Desktop.Controls.CardTabStrip { IsMotionEnabled = false, Width = 300 };
        line.Items.Add(lineItem);
        card.Items.Add(cardItem);

        ShowInWindow(new StackPanel { Children = { line, card } }, (_, _) =>
        {
            Focus(lineItem, NavigationMethod.Tab);
            var lineTarget = AdornedBy(GetFocusOutline(lineItem));
            lineTarget.Name.ShouldBe("ContentLayout");
            FocusOutlineAdorner.GetIsInset(lineTarget).ShouldBeFalse();

            Focus(cardItem, NavigationMethod.Tab);
            var cardTarget = AdornedBy(GetFocusOutline(cardItem));
            cardTarget.Name.ShouldBe("Frame");
            FocusOutlineAdorner.GetIsInset(cardTarget).ShouldBeTrue();
        });
    }

    [Fact]
    public void Tree_View_Keyboard_Focus_Outlines_The_Active_Node_Content()
    {
        var treeView = CreateTreeView(out var first, out var second);

        ShowInWindow(treeView, (window, before) =>
        {
            Focus(treeView, NavigationMethod.Tab);
            var outline = GetFocusOutline(treeView);
            AdornedBy(outline).ShouldBeSameAs(GetHeaderContentFrame(first));

            window.KeyPress(Key.Down, RawInputModifiers.None, PhysicalKey.ArrowDown, null);
            Dispatcher.UIThread.RunJobs();
            AdornedBy(GetFocusOutline(treeView)).ShouldBeSameAs(GetHeaderContentFrame(second));

            Focus(before, NavigationMethod.Tab);
            FindFocusOutlines(treeView).Where(a => AdornedBy(a).TemplatedParent is TreeViewItemHeader)
                                       .ShouldBeEmpty();
        });
    }

    // The selected background hides the active background, so on a selected row the outline is the only
    // keyboard position cue; it must be drawn there too.
    [Fact]
    public void Tree_View_Outlines_The_Active_Node_When_It_Is_Selected()
    {
        var treeView = CreateTreeView(out var first, out var second);
        first.IsSelected = true;

        ShowInWindow(treeView, (window, before) =>
        {
            Focus(treeView, NavigationMethod.Tab);
            first.IsSelected.ShouldBeTrue();
            AdornedBy(GetFocusOutline(treeView)).ShouldBeSameAs(GetHeaderContentFrame(first));

            // Single selection follows the active node, so the newly active node is selected as well.
            window.KeyPress(Key.Down, RawInputModifiers.None, PhysicalKey.ArrowDown, null);
            Dispatcher.UIThread.RunJobs();
            second.IsSelected.ShouldBeTrue();
            AdornedBy(GetFocusOutline(treeView)).ShouldBeSameAs(GetHeaderContentFrame(second));

            // Ctrl + arrow moves only the active node; the outline follows it while the selection stays.
            window.KeyPress(Key.Up, RawInputModifiers.Control, PhysicalKey.ArrowUp, null);
            Dispatcher.UIThread.RunJobs();
            second.IsSelected.ShouldBeTrue();
            first.IsSelected.ShouldBeFalse();
            AdornedBy(GetFocusOutline(treeView)).ShouldBeSameAs(GetHeaderContentFrame(first));

            Focus(before, NavigationMethod.Tab);
            FindFocusOutlines(treeView).Where(a => AdornedBy(a).TemplatedParent is TreeViewItemHeader)
                                       .ShouldBeEmpty();
        });
    }

    // NavMenu hides the active background on the selected item, so the outline must still mark it.
    [Fact]
    public void Nav_Menu_Outlines_The_Active_Item_When_It_Is_Selected()
    {
        var menu = CreateNavMenu(NavMenuMode.Inline, out _, out var second);

        ShowInWindow(menu, (window, _) =>
        {
            Focus(menu, NavigationMethod.Tab);
            window.KeyPress(Key.Down, RawInputModifiers.None, PhysicalKey.ArrowDown, null);
            window.KeyPress(Key.Down, RawInputModifiers.None, PhysicalKey.ArrowDown, null);
            window.KeyPress(Key.Enter, RawInputModifiers.None, PhysicalKey.Enter, null);
            Dispatcher.UIThread.RunJobs();

            menu.SelectedItem.ShouldBeSameAs(second);
            var secondContainer = (NavMenuItem)menu.ContainerFromItem(second)!;
            secondContainer.IsKeyboardActive.ShouldBeTrue();
            AdornedBy(GetFocusOutline(menu)).ShouldBeSameAs(secondContainer.ItemHeader);
        });
    }

    [Fact]
    public void Tree_View_Pointer_Focus_Shows_No_Outline_Until_A_Navigation_Key()
    {
        var treeView = CreateTreeView(out _, out var second);

        ShowInWindow(treeView, (window, _) =>
        {
            Focus(treeView, NavigationMethod.Pointer);
            FindFocusOutlines(treeView).ShouldBeEmpty();

            window.KeyPress(Key.Down, RawInputModifiers.None, PhysicalKey.ArrowDown, null);
            Dispatcher.UIThread.RunJobs();
            FindFocusOutlines(treeView).Count.ShouldBe(1);
        });
    }

    [Fact]
    public void Nav_Menu_Outline_Moves_From_Root_To_The_Keyboard_Active_Item()
    {
        var menu = CreateNavMenu(NavMenuMode.Inline, out var first, out _);

        ShowInWindow(menu, (window, before) =>
        {
            Focus(menu, NavigationMethod.Tab);
            // Ant Design outlines the menu root until navigation reaches an item.
            AdornedBy(GetFocusOutline(menu)).ShouldBeSameAs(menu);

            window.KeyPress(Key.Down, RawInputModifiers.None, PhysicalKey.ArrowDown, null);
            Dispatcher.UIThread.RunJobs();
            var firstContainer = (NavMenuItem)menu.ContainerFromItem(first)!;
            AdornedBy(GetFocusOutline(menu)).ShouldBeSameAs(firstContainer.ItemHeader);

            Focus(before, NavigationMethod.Tab);
            FindFocusOutlines(menu).Where(a => AdornedBy(a) is BaseNavMenuItemHeader || AdornedBy(a) == menu)
                                   .ShouldBeEmpty();
            firstContainer.IsKeyboardFocusOutlineVisible.ShouldBeFalse();
        });
    }

    [Fact]
    public void Nav_Menu_Pointer_Press_Hides_The_Keyboard_Outline()
    {
        var menu = CreateNavMenu(NavMenuMode.Inline, out var first, out var second);

        ShowInWindow(menu, (window, _) =>
        {
            Focus(menu, NavigationMethod.Tab);
            window.KeyPress(Key.Down, RawInputModifiers.None, PhysicalKey.ArrowDown, null);
            Dispatcher.UIThread.RunJobs();
            FindFocusOutlines(menu).Count.ShouldBe(1);

            var secondHeader = ((NavMenuItem)menu.ContainerFromItem(second)!).ItemHeader!;
            var point        = secondHeader.TranslatePoint(new Point(4, 4), window)!.Value;
            window.MouseDown(point, MouseButton.Left);
            window.MouseUp(point, MouseButton.Left);
            Dispatcher.UIThread.RunJobs();

            FindFocusOutlines(menu).ShouldBeEmpty();
            ((NavMenuItem)menu.ContainerFromItem(first)!).IsKeyboardFocusOutlineVisible.ShouldBeFalse();
        });
    }

    [Fact]
    public void Nav_Menu_Submenu_Item_Is_Outlined_Inside_Its_Popup()
    {
        var child  = new NavMenuNode { Header = "Child", ItemKey = "child" };
        var parent = new NavMenuNode { Header = "Parent", ItemKey = "parent" };
        parent.Entries.Add(child);
        var menu = new Desktop.Controls.NavMenu { Mode = NavMenuMode.Vertical, IsMotionEnabled = false };
        menu.Items.Add(parent);

        ShowInWindow(menu, (window, _) =>
        {
            Focus(menu, NavigationMethod.Tab);
            window.KeyPress(Key.Down, RawInputModifiers.None, PhysicalKey.ArrowDown, null);
            window.KeyPress(Key.Right, RawInputModifiers.None, PhysicalKey.ArrowRight, null);
            Dispatcher.UIThread.RunJobs();

            var parentContainer = (NavMenuItem)menu.ContainerFromItem(parent)!;
            var childContainer  = (NavMenuItem)parentContainer.ContainerFromItem(child)!;
            parentContainer.IsSubMenuOpen.ShouldBeTrue();
            childContainer.IsKeyboardFocusOutlineVisible.ShouldBeTrue();

            var childHeader = childContainer.ItemHeader!;
            var popupLayer  = AdornerLayer.GetAdornerLayer(childHeader);
            popupLayer.ShouldNotBeNull();
            popupLayer.ShouldNotBeSameAs(AdornerLayer.GetAdornerLayer(menu));
            popupLayer!.Children.OfType<FocusOutlineAdorner>().Select(AdornedBy).ShouldBe([childHeader]);
            parentContainer.IsKeyboardFocusOutlineVisible.ShouldBeFalse();
        });
    }

    private static Desktop.Controls.Carousel CreateCarousel()
    {
        var carousel = new Desktop.Controls.Carousel { Width = 300, Height = 120, IsMotionEnabled = false };
        carousel.Items.Add(new Desktop.Controls.CarouselPage { Content = "1" });
        carousel.Items.Add(new Desktop.Controls.CarouselPage { Content = "2" });
        carousel.Items.Add(new Desktop.Controls.CarouselPage { Content = "3" });
        return carousel;
    }

    private static Desktop.Controls.TreeView CreateTreeView(out Desktop.Controls.TreeViewItem first,
                                                            out Desktop.Controls.TreeViewItem second)
    {
        first  = new Desktop.Controls.TreeViewItem { Header = "First" };
        second = new Desktop.Controls.TreeViewItem { Header = "Second" };
        var treeView = new Desktop.Controls.TreeView { Width = 240, IsMotionEnabled = false };
        treeView.Items.Add(first);
        treeView.Items.Add(second);
        return treeView;
    }

    private static Desktop.Controls.NavMenu CreateNavMenu(NavMenuMode mode, out NavMenuNode first, out NavMenuNode second)
    {
        first  = new NavMenuNode { Header = "First", ItemKey = "first" };
        second = new NavMenuNode { Header = "Second", ItemKey = "second" };
        var menu = new Desktop.Controls.NavMenu { Mode = mode, IsMotionEnabled = false, Width = 220 };
        menu.Items.Add(first);
        menu.Items.Add(second);
        return menu;
    }

    private static Control GetHeaderContentFrame(Desktop.Controls.TreeViewItem item)
    {
        return item.GetVisualDescendants().OfType<Control>().First(c => c.Name == "PART_HeaderContentFrame");
    }

    // Presses Tab from the element before the control and records each focus stop until focus leaves it.
    private static List<IInputElement> CollectTabStops(InputElement before, Control control)
    {
        var stops = new List<IInputElement>();
        Focus(before, NavigationMethod.Tab);
        var topLevel = TopLevel.GetTopLevel(control)!;
        for (var i = 0; i < 64; i++)
        {
            topLevel.KeyPress(Key.Tab, RawInputModifiers.None, PhysicalKey.Tab, null);
            Dispatcher.UIThread.RunJobs();
            var current = topLevel.FocusManager?.GetFocusedElement();
            if (current is not Visual visual ||
                !(ReferenceEquals(visual, control) || control.IsVisualAncestorOf(visual)))
            {
                break;
            }

            stops.Add(current);
        }

        return stops;
    }

    private static Control AdornedBy(Control adorner) => (Control)AdornerLayer.GetAdornedElement(adorner)!;

    private static FocusOutlineAdorner GetFocusOutline(Visual target)
    {
        var outlines = FindFocusOutlines(target);
        outlines.Count.ShouldBe(1);
        return outlines[0];
    }

    private static List<FocusOutlineAdorner> FindFocusOutlines(Visual target)
    {
        var layer = AdornerLayer.GetAdornerLayer(target);
        layer.ShouldNotBeNull();
        return layer!.Children.OfType<FocusOutlineAdorner>().ToList();
    }

    private static void Focus(InputElement element, NavigationMethod method)
    {
        element.Focus(method).ShouldBeTrue();
        Dispatcher.UIThread.RunJobs();
    }

    // A focusable element before the content anchors Tab order walks and gives focus somewhere to go.
    private static void ShowInWindow(Control content, Action<AvaloniaWindow, InputElement> assertion)
    {
        var before = new Desktop.Controls.Button { Content = "Before" };
        var panel  = new ScopeAwareOverlayLayerPanel { Width = 420, Height = 420 };
        panel.Children.Add(new StackPanel { Margin = new Thickness(20), Spacing = 8, Children = { before, content } });
        var host = new VisualLayerManager { Child = panel };
        var property = typeof(VisualLayerManager).GetProperty(
            "EnablePopupOverlayLayer",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        property.ShouldNotBeNull();
        property.SetValue(host, true);

        var window = new AvaloniaWindow { Width = 460, Height = 460, Content = host };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            assertion(window, before);
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }
}
