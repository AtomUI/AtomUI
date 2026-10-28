using System.Reflection;
using AtomUI.Controls;
using AtomUI.Desktop.Controls.DesignTokens;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Templates;
using Avalonia.Layout;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;

namespace AtomUI.Desktop.Controls.Tests.NavMenu;

public class NavMenuLayoutTests
{
    static NavMenuLayoutTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Theory]
    [InlineData(NavMenuMode.Inline)]
    [InlineData(NavMenuMode.Vertical)]
    public void Root_Item_Inset_Uses_AntDesign_Item_Margins_Without_Outer_Menu_Padding(NavMenuMode mode)
    {
        var menu = new AtomUI.Desktop.Controls.NavMenu
        {
            Mode            = mode,
            IsMotionEnabled = false,
            Width           = 240
        };
        var item = new NavMenuNode
        {
            Header  = "Item",
            ItemKey = "item"
        };
        menu.Items.Add(item);

        var window = new Avalonia.Controls.Window
        {
            Width   = 320,
            Height  = 240,
            Content = menu
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            var itemContainer = (Control)menu.ContainerFromItem(item)!;
            var itemHeader    = GetItemHeader(itemContainer);
            var leftInset     = GetLeft(itemHeader, menu);
            var rightInset    = menu.Bounds.Width - GetRight(itemHeader, menu);
            var topInset      = GetTop(itemHeader, menu);

            leftInset.ShouldBe(4, 0.5,
                "Ant Design vertical/inline root menus do not add outer menu padding; item marginInline is the only horizontal inset.");
            rightInset.ShouldBe(4, 0.5,
                "Ant Design vertical/inline root menus do not add outer menu padding; item marginInline is the only horizontal inset.");
            topInset.ShouldBe(4, 0.5,
                "Ant Design vertical/inline root menus expose one marginXXS before the first item.");
        }
        finally
        {
            window.Close();
        }
    }

    [Theory]
    [InlineData(NavMenuMode.Inline)]
    [InlineData(NavMenuMode.Vertical)]
    public void Vertical_Item_Header_Gap_Collapses_AntDesign_Block_Margins(NavMenuMode mode)
    {
        var menu = new AtomUI.Desktop.Controls.NavMenu
        {
            Mode            = mode,
            IsMotionEnabled = false,
            Width           = 240
        };
        var first = new NavMenuNode
        {
            Header  = "First",
            ItemKey = "first"
        };
        var second = new NavMenuNode
        {
            Header  = "Second",
            ItemKey = "second"
        };
        menu.Items.Add(first);
        menu.Items.Add(second);

        var window = new Avalonia.Controls.Window
        {
            Width   = 320,
            Height  = 240,
            Content = menu
        };

        try
        {
            window.Show();

            var firstContainer  = (Control)menu.ContainerFromItem(first)!;
            var secondContainer = (Control)menu.ContainerFromItem(second)!;
            var firstHeader     = GetItemHeader(firstContainer);
            var secondHeader    = GetItemHeader(secondContainer);

            var gap = GetTop(secondHeader, menu) - GetBottom(firstHeader, menu);

            gap.ShouldBe(4, 0.5,
                "Ant Design uses marginBlock: marginXXS on adjacent block menu items; CSS collapses those adjacent vertical margins to 4px.");
        }
        finally
        {
            window.Close();
        }
    }

    [Theory]
    [InlineData(NavMenuMode.Inline)]
    [InlineData(NavMenuMode.Vertical)]
    public void Explicit_ItemSpacing_Adds_Exactly_To_The_Default_Block_Gap(NavMenuMode mode)
    {
        const double itemSpacing = 7;
        var menu = new AtomUI.Desktop.Controls.NavMenu
        {
            Mode            = mode,
            IsMotionEnabled = false,
            ItemSpacing     = itemSpacing,
            Width           = 240
        };
        var first = new NavMenuNode
        {
            Header  = "First",
            ItemKey = "first"
        };
        var second = new NavMenuNode
        {
            Header  = "Second",
            ItemKey = "second"
        };
        menu.Items.Add(first);
        menu.Items.Add(second);

        var window = new Avalonia.Controls.Window
        {
            Width   = 320,
            Height  = 240,
            Content = menu
        };

        try
        {
            window.Show();

            var firstContainer  = (Control)menu.ContainerFromItem(first)!;
            var secondContainer = (Control)menu.ContainerFromItem(second)!;
            var firstHeader     = GetItemHeader(firstContainer);
            var secondHeader    = GetItemHeader(secondContainer);
            var gap = GetTop(secondHeader, menu) - GetBottom(firstHeader, menu);

            gap.ShouldBe(4 + itemSpacing, 0.5,
                "ItemSpacing is an additional ItemsPanel gap and must not replace the existing Ant Design item margins.");
        }
        finally
        {
            window.Close();
        }
    }

    [Theory]
    [InlineData(NavMenuMode.Inline)]
    [InlineData(NavMenuMode.Vertical)]
    public void Root_Header_And_Footer_Are_Outside_The_Scrollable_Entry_Region(NavMenuMode mode)
    {
        var menu = new AtomUI.Desktop.Controls.NavMenu
        {
            Mode            = mode,
            IsMotionEnabled = false,
            Header          = new Border { Height = 24 },
            Footer          = new Border { Height = 20 },
            Width           = 240,
            Height          = 120
        };
        for (var index = 0; index < 10; index++)
        {
            menu.Items.Add(new NavMenuNode
            {
                Header  = $"Item {index}",
                ItemKey = $"item-{index}"
            });
        }

        var window = new Avalonia.Controls.Window
        {
            Width   = 320,
            Height  = 240,
            Content = menu
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            var headerPresenter = menu.GetVisualDescendants()
                                      .OfType<ContentPresenter>()
                                      .Single(presenter => presenter.Name == "PART_HeaderPresenter");
            var footerPresenter = menu.GetVisualDescendants()
                                      .OfType<ContentPresenter>()
                                      .Single(presenter => presenter.Name == "PART_FooterPresenter");
            var itemsPresenter = menu.GetVisualDescendants()
                                     .OfType<ItemsPresenter>()
                                     .Single(presenter => presenter.Name == "PART_ItemsPresenter");

            headerPresenter.GetVisualAncestors().OfType<AtomUI.Desktop.Controls.ScrollViewer>().ShouldBeEmpty();
            footerPresenter.GetVisualAncestors().OfType<AtomUI.Desktop.Controls.ScrollViewer>().ShouldBeEmpty();
            itemsPresenter.GetVisualAncestors().OfType<AtomUI.Desktop.Controls.ScrollViewer>().ShouldHaveSingleItem();
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Inline_Submenu_First_Child_Header_Gap_Collapses_AntDesign_Block_Margins()
    {
        var menu = new AtomUI.Desktop.Controls.NavMenu
        {
            Mode            = NavMenuMode.Inline,
            IsMotionEnabled = false,
            Width           = 240
        };
        var child = new NavMenuNode
        {
            Header  = "Child",
            ItemKey = "child"
        };
        var parent = new NavMenuNode
        {
            Header  = "Parent",
            ItemKey = "parent"
        };
        parent.Children.Add(child);
        menu.Items.Add(parent);

        var window = new Avalonia.Controls.Window
        {
            Width   = 320,
            Height  = 320,
            Content = menu
        };

        try
        {
            window.Show();

            var parentContainer = (Control)menu.ContainerFromItem(parent)!;
            var parentMenuItem  = (INavMenuItem)parentContainer;
            parentMenuItem.Open();
            Dispatcher.UIThread.RunJobs();

            var parentHeader   = GetItemHeader(parentContainer);
            var childContainer = (Control)((ItemsControl)parentContainer).ContainerFromItem(child)!;
            var childHeader    = GetItemHeader(childContainer);

            var gap = GetTop(childHeader, menu) - GetBottom(parentHeader, menu);

            gap.ShouldBe(4, 0.5,
                "Ant Design does not add an extra top margin to the inline child items container.");
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Inline_Submenu_Child_Header_Gap_Collapses_AntDesign_Block_Margins()
    {
        var menu = new AtomUI.Desktop.Controls.NavMenu
        {
            Mode            = NavMenuMode.Inline,
            IsMotionEnabled = false,
            Width           = 240
        };
        var firstChild = new NavMenuNode
        {
            Header  = "First child",
            ItemKey = "first-child"
        };
        var secondChild = new NavMenuNode
        {
            Header  = "Second child",
            ItemKey = "second-child"
        };
        var parent = new NavMenuNode
        {
            Header  = "Parent",
            ItemKey = "parent"
        };
        parent.Children.Add(firstChild);
        parent.Children.Add(secondChild);
        menu.Items.Add(parent);

        var window = new Avalonia.Controls.Window
        {
            Width   = 320,
            Height  = 320,
            Content = menu
        };

        try
        {
            window.Show();

            var parentContainer = (Control)menu.ContainerFromItem(parent)!;
            var parentMenuItem  = (INavMenuItem)parentContainer;
            parentMenuItem.Open();
            Dispatcher.UIThread.RunJobs();

            var firstChildContainer  = (Control)((ItemsControl)parentContainer).ContainerFromItem(firstChild)!;
            var secondChildContainer = (Control)((ItemsControl)parentContainer).ContainerFromItem(secondChild)!;
            var firstChildHeader     = GetItemHeader(firstChildContainer);
            var secondChildHeader    = GetItemHeader(secondChildContainer);

            var gap = GetTop(secondChildHeader, menu) - GetBottom(firstChildHeader, menu);

            gap.ShouldBe(4, 0.5,
                "Inline submenu children share Ant Design's vertical menu item margin semantics.");
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Explicit_ItemSpacing_Is_Inherited_By_Inline_Submenu_ItemsPanel()
    {
        const double itemSpacing = 7;
        var menu = new AtomUI.Desktop.Controls.NavMenu
        {
            Mode            = NavMenuMode.Inline,
            IsMotionEnabled = false,
            ItemSpacing     = itemSpacing,
            Width           = 240
        };
        var firstChild = new NavMenuNode { Header = "First child", ItemKey = "first-child" };
        var secondChild = new NavMenuNode { Header = "Second child", ItemKey = "second-child" };
        var parent = new NavMenuNode { Header = "Parent", ItemKey = "parent" };
        parent.Children.Add(firstChild);
        parent.Children.Add(secondChild);
        menu.Items.Add(parent);

        var window = new Avalonia.Controls.Window
        {
            Width   = 320,
            Height  = 320,
            Content = menu
        };

        try
        {
            window.Show();

            var parentContainer = (NavMenuItem)menu.ContainerFromItem(parent)!;
            parentContainer.Open();
            Dispatcher.UIThread.RunJobs();

            var firstContainer = (Control)parentContainer.ContainerFromItem(firstChild)!;
            var secondContainer = (Control)parentContainer.ContainerFromItem(secondChild)!;
            var gap = GetTop(GetItemHeader(secondContainer), menu) -
                      GetBottom(GetItemHeader(firstContainer), menu);

            gap.ShouldBe(4 + itemSpacing, 0.5);
            parentContainer.EntryItemSpacing.ShouldBe(itemSpacing);
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Explicit_ItemSpacing_Is_Inherited_By_Group_ItemsPanel()
    {
        const double itemSpacing = 7;
        var first = new NavMenuNode { Header = "First", ItemKey = "first" };
        var second = new NavMenuNode { Header = "Second", ItemKey = "second" };
        var group = new NavMenuGroup { Header = "Section" };
        group.Entries.Add(first);
        group.Entries.Add(second);
        var menu = new AtomUI.Desktop.Controls.NavMenu
        {
            Mode            = NavMenuMode.Inline,
            IsMotionEnabled = false,
            ItemSpacing     = itemSpacing,
            Width           = 240
        };
        menu.Items.Add(group);

        var window = new Avalonia.Controls.Window
        {
            Width   = 320,
            Height  = 320,
            Content = menu
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            var groupContainer = menu.ContainerFromItem(group).ShouldBeOfType<NavMenuGroupItem>();
            var firstContainer = (Control)groupContainer.ContainerFromItem(first)!;
            var secondContainer = (Control)groupContainer.ContainerFromItem(second)!;
            var gap = GetTop(GetItemHeader(secondContainer), menu) -
                      GetBottom(GetItemHeader(firstContainer), menu);

            gap.ShouldBe(4 + itemSpacing, 0.5);
            groupContainer.EntryItemSpacing.ShouldBe(itemSpacing);
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Explicit_ItemSpacing_Is_Inherited_By_Popup_ItemsPanel()
    {
        const double itemSpacing = 7;
        var firstChild = new NavMenuNode { Header = "First child", ItemKey = "first-child" };
        var secondChild = new NavMenuNode { Header = "Second child", ItemKey = "second-child" };
        var parent = new NavMenuNode { Header = "Parent", ItemKey = "parent" };
        parent.Children.Add(firstChild);
        parent.Children.Add(secondChild);
        var menu = new AtomUI.Desktop.Controls.NavMenu
        {
            Mode            = NavMenuMode.Vertical,
            IsMotionEnabled = false,
            ItemSpacing     = itemSpacing,
            Width           = 240
        };
        menu.Items.Add(parent);

        var window = new Avalonia.Controls.Window
        {
            Width   = 640,
            Height  = 480,
            Content = CreatePopupOverlayHost(menu)
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            var parentContainer = (NavMenuItem)menu.ContainerFromItem(parent)!;
            parentContainer.Open();
            Dispatcher.UIThread.RunJobs();

            var firstContainer = (Control)parentContainer.ContainerFromItem(firstChild)!;
            var secondContainer = (Control)parentContainer.ContainerFromItem(secondChild)!;
            var popupFrame = FindPopupFrame(window);
            var gap = GetTop(GetItemHeader(secondContainer), popupFrame) -
                      GetBottom(GetItemHeader(firstContainer), popupFrame);

            gap.ShouldBe(4 + itemSpacing, 0.5);
            parentContainer.EntryItemSpacing.ShouldBe(itemSpacing);
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Horizontal_Root_Keeps_Zero_Spacing_While_Popup_Uses_Vertical_Token_Spacing()
    {
        const double tokenSpacing = 7;
        var firstChild = new NavMenuNode { Header = "First child", ItemKey = "first-child" };
        var secondChild = new NavMenuNode { Header = "Second child", ItemKey = "second-child" };
        var parent = new NavMenuNode { Header = "Parent", ItemKey = "parent" };
        parent.Children.Add(firstChild);
        parent.Children.Add(secondChild);
        var menu = new AtomUI.Desktop.Controls.NavMenu
        {
            Mode            = NavMenuMode.Horizontal,
            IsMotionEnabled = false,
            Width           = 320
        };
        menu.Items.Add(parent);

        var window = new Avalonia.Controls.Window
        {
            Width   = 640,
            Height  = 480,
            Content = CreatePopupOverlayHost(menu)
        };
        window.Resources[NavMenuTokenKind.VerticalItemsPanelSpacing] = tokenSpacing;

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            var parentContainer = (NavMenuItem)menu.ContainerFromItem(parent)!;
            parentContainer.Open();
            Dispatcher.UIThread.RunJobs();

            var firstContainer = (Control)parentContainer.ContainerFromItem(firstChild)!;
            var secondContainer = (Control)parentContainer.ContainerFromItem(secondChild)!;
            var popupFrame = FindPopupFrame(window);
            var gap = GetTop(GetItemHeader(secondContainer), popupFrame) -
                      GetBottom(GetItemHeader(firstContainer), popupFrame);

            menu.ItemSpacing.ShouldBe(0d);
            parentContainer.EntryItemSpacing.ShouldBe(tokenSpacing);
            gap.ShouldBe(4 + tokenSpacing, 0.5);
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Horizontal_Header_And_Footer_Bound_The_Entry_Region()
    {
        var first = new NavMenuNode { Header = "First", ItemKey = "first" };
        var second = new NavMenuNode { Header = "Second", ItemKey = "second" };
        var menu = new AtomUI.Desktop.Controls.NavMenu
        {
            Mode            = NavMenuMode.Horizontal,
            IsMotionEnabled = false,
            Header          = new Border { Width = 40 },
            Footer          = new Border { Width = 50 },
            Width           = 400
        };
        menu.Items.Add(first);
        menu.Items.Add(second);

        var window = new Avalonia.Controls.Window
        {
            Width   = 480,
            Height  = 160,
            Content = menu
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            var headerPresenter = menu.GetVisualDescendants()
                                      .OfType<ContentPresenter>()
                                      .Single(presenter => presenter.Name == "PART_HeaderPresenter");
            var footerPresenter = menu.GetVisualDescendants()
                                      .OfType<ContentPresenter>()
                                      .Single(presenter => presenter.Name == "PART_FooterPresenter");
            var firstHeader = GetItemHeader((Control)menu.ContainerFromItem(first)!);
            var secondHeader = GetItemHeader((Control)menu.ContainerFromItem(second)!);

            GetRight(headerPresenter, menu).ShouldBeLessThanOrEqualTo(GetLeft(firstHeader, menu));
            GetRight(secondHeader, menu).ShouldBeLessThanOrEqualTo(GetLeft(footerPresenter, menu));
        }
        finally
        {
            window.Close();
        }
    }

    [Theory]
    [InlineData(true, 4d)]
    [InlineData(false, 0d)]
    public void Inline_Submenu_Background_Gap_Follows_Item_Background_Mode(
        bool isItemBackgroundEnabled,
        double expectedGap)
    {
        var menu = new AtomUI.Desktop.Controls.NavMenu
        {
            Mode                    = NavMenuMode.Inline,
            IsMotionEnabled         = false,
            IsItemBackgroundEnabled = isItemBackgroundEnabled,
            Width                   = 240
        };
        var firstChild = new NavMenuNode
        {
            Header  = "First child",
            ItemKey = "first-child"
        };
        var secondChild = new NavMenuNode
        {
            Header  = "Second child",
            ItemKey = "second-child"
        };
        var parent = new NavMenuNode
        {
            Header  = "Parent",
            ItemKey = "parent"
        };
        var nextRoot = new NavMenuNode
        {
            Header  = "Next root",
            ItemKey = "next-root"
        };
        parent.Children.Add(firstChild);
        parent.Children.Add(secondChild);
        menu.Items.Add(parent);
        menu.Items.Add(nextRoot);

        var window = new Avalonia.Controls.Window
        {
            Width   = 320,
            Height  = 360,
            Content = menu
        };

        try
        {
            window.Show();

            var parentContainer = (Control)menu.ContainerFromItem(parent)!;
            var parentMenuItem  = (INavMenuItem)parentContainer;
            parentMenuItem.Open();
            menu.SelectedItem = secondChild;
            Dispatcher.UIThread.RunJobs();

            var childItemsFrame   = FindChildItemsFrame(parentContainer);
            var nextRootContainer = (Control)menu.ContainerFromItem(nextRoot)!;
            var nextRootHeader    = GetItemHeader(nextRootContainer);

            var gap = GetTop(nextRootHeader, menu) - GetBottom(childItemsFrame, menu);

            gap.ShouldBe(expectedGap, 0.5,
                "The extra inline submenu block gap is only needed when the submenu background block is visible.");
        }
        finally
        {
            window.Close();
        }
    }

    [Theory]
    [InlineData(NavMenuMode.Vertical)]
    [InlineData(NavMenuMode.Horizontal)]
    public void Popup_Item_Inset_Uses_AntDesign_Item_Margins(NavMenuMode mode)
    {
        var menu = new AtomUI.Desktop.Controls.NavMenu
        {
            Mode            = mode,
            IsMotionEnabled = false,
            Width           = 240
        };
        var firstChild = new NavMenuNode
        {
            Header  = "Item 1",
            ItemKey = "item-1"
        };
        var secondChild = new NavMenuNode
        {
            Header  = "Item 2",
            ItemKey = "item-2"
        };
        var parent = new NavMenuNode
        {
            Header  = "Parent",
            ItemKey = "parent"
        };
        parent.Children.Add(firstChild);
        parent.Children.Add(secondChild);
        menu.Items.Add(parent);

        var window = new Avalonia.Controls.Window
        {
            Width   = 640,
            Height  = 480,
            Content = CreatePopupOverlayHost(menu)
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            var parentContainer = (Control)menu.ContainerFromItem(parent)!;
            var parentMenuItem  = (INavMenuItem)parentContainer;
            parentMenuItem.Open();
            Dispatcher.UIThread.RunJobs();

            var popupFrame          = FindPopupFrame(window);
            var firstChildContainer = (Control)((ItemsControl)parentContainer).ContainerFromItem(firstChild)!;
            var secondChildContainer = (Control)((ItemsControl)parentContainer).ContainerFromItem(secondChild)!;
            var firstChildHeader    = GetItemHeader(firstChildContainer);
            var secondChildHeader   = GetItemHeader(secondChildContainer);

            var leftInset   = GetLeft(firstChildHeader, popupFrame);
            var rightInset  = popupFrame.Bounds.Width - GetRight(firstChildHeader, popupFrame);
            var topInset    = GetTop(firstChildHeader, popupFrame);
            var bottomInset = popupFrame.Bounds.Height - GetBottom(secondChildHeader, popupFrame);

            leftInset.ShouldBe(4, 0.5,
                "Ant Design popup submenus use the item's marginInline as the only horizontal inset.");
            rightInset.ShouldBe(4, 0.5,
                "Ant Design popup submenus use the item's marginInline as the only horizontal inset.");
            topInset.ShouldBe(4, 0.5,
                "Ant Design popup submenus expose one marginXXS before the first item.");
            bottomInset.ShouldBe(4, 0.5,
                "Ant Design popup submenus expose one marginXXS after the last item.");
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void InlineCollapsed_Popup_With_Stretch_HeaderTemplate_Uses_Content_Sized_Width()
    {
        var menu = new AtomUI.Desktop.Controls.NavMenu
        {
            Mode              = NavMenuMode.Inline,
            IsInlineCollapsed = true,
            IsDarkStyle       = true,
            IsMotionEnabled   = false,
            Width             = 300
        };
        var customTemplateChild = new NavMenuNode
        {
            Header         = "Option5",
            ItemKey        = "option-5",
            HeaderTemplate = CreateStretchTagHeaderTemplate()
        };
        var parent = new NavMenuNode
        {
            Header  = "Navigation",
            ItemKey = "navigation"
        };
        parent.Children.Add(customTemplateChild);
        parent.Children.Add(new NavMenuNode
        {
            Header  = "Option 6",
            ItemKey = "option-6"
        });
        parent.Children.Add(new NavMenuNode
        {
            Header  = "Option 7",
            ItemKey = "option-7"
        });
        menu.Items.Add(parent);

        var window = new Avalonia.Controls.Window
        {
            Width   = 1200,
            Height  = 760,
            Content = CreatePopupOverlayHost(menu)
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            var parentContainer = (Control)menu.ContainerFromItem(parent)!;
            ((INavMenuItem)parentContainer).Open();
            Dispatcher.UIThread.RunJobs();

            var popupFrame = FindPopupFrame(window);

            popupFrame.Bounds.Width.ShouldBeLessThan(320,
                "A popup menu must size from child content; a stretchable HeaderTemplate must not receive the popup max width as its desired width.");
        }
        finally
        {
            window.Close();
        }
    }

    private static BaseNavMenuItemHeader GetItemHeader(Control container)
    {
        var field = container.GetType().GetField(
            "_itemHeader",
            BindingFlags.Instance | BindingFlags.NonPublic);
        field.ShouldNotBeNull();
        return (BaseNavMenuItemHeader)field.GetValue(container)!;
    }

    private static double GetTop(Control control, Visual relativeTo)
    {
        var point = control.TranslatePoint(default, relativeTo);
        point.ShouldNotBeNull();
        return point.Value.Y;
    }

    private static double GetLeft(Control control, Visual relativeTo)
    {
        var point = control.TranslatePoint(default, relativeTo);
        point.ShouldNotBeNull();
        return point.Value.X;
    }

    private static double GetRight(Control control, Visual relativeTo)
    {
        return GetLeft(control, relativeTo) + control.Bounds.Width;
    }

    private static double GetBottom(Control control, Visual relativeTo)
    {
        return GetTop(control, relativeTo) + control.Bounds.Height;
    }

    private static Border FindPopupFrame(Visual root)
    {
        return root.GetVisualDescendants()
                   .OfType<Border>()
                   .Single(border => border.Name == "PART_PopupFrame");
    }

    private static Border FindChildItemsFrame(Visual root)
    {
        return root.GetVisualDescendants()
                   .OfType<Border>()
                   .Single(border => border.Name == "PART_ChildItemsFrame" &&
                                     ReferenceEquals(border.TemplatedParent, root));
    }

    private static VisualLayerManager CreatePopupOverlayHost(Control content)
    {
        var visualLayerManager = new VisualLayerManager
        {
            Child = content
        };

        EnablePopupOverlayLayer(visualLayerManager);
        return visualLayerManager;
    }

    private static void EnablePopupOverlayLayer(VisualLayerManager visualLayerManager)
    {
        var property = typeof(VisualLayerManager).GetProperty(
            "EnablePopupOverlayLayer",
            BindingFlags.Instance | BindingFlags.NonPublic);

        property.ShouldNotBeNull();
        property.SetValue(visualLayerManager, true);
    }

    private static IDataTemplate CreateStretchTagHeaderTemplate()
    {
        return new FuncDataTemplate<object?>((_, _) =>
        {
            var textBlock = new TextBlock
            {
                Text              = "Option5",
                VerticalAlignment = VerticalAlignment.Center
            };
            Flex.SetGrow(textBlock, 1);

            var tag = new AtomUI.Desktop.Controls.Tag
            {
                TagColor          = "cyan",
                Variant           = TagVariant.Filled,
                Padding           = new Thickness(4, 0),
                VerticalAlignment = VerticalAlignment.Center,
                Margin            = new Thickness(8, 0, 0, 0),
                Text              = "测试Tag"
            };

            return new FlexPanel
            {
                Direction           = FlexDirection.Row,
                AlignItems          = AlignItems.Center,
                JustifyContent      = JustifyContent.FlexStart,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Children =
                {
                    textBlock,
                    tag
                }
            };
        });
    }
}
