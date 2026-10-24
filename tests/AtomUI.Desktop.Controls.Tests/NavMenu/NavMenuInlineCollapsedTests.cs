using System.Reflection;
using AtomUI.Controls;
using AtomUI.Controls.Primitives;
using AtomUI.Icons.AntDesign;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Headless;
using Avalonia.Layout;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;

namespace AtomUI.Desktop.Controls.Tests.NavMenu;

public class NavMenuInlineCollapsedTests
{
    static NavMenuInlineCollapsedTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void InlineCollapsedWidth_Defaults_To_Token_Value_And_Can_Be_Overridden_Locally()
    {
        var defaultMenu = new AtomUI.Desktop.Controls.NavMenu
        {
            Mode              = NavMenuMode.Inline,
            IsMotionEnabled   = false,
            IsInlineCollapsed = true,
            Width             = 240
        };
        defaultMenu.Items.Add(new NavMenuNode
        {
            Header  = "Default",
            ItemKey = "default"
        });

        var customMenu = new AtomUI.Desktop.Controls.NavMenu
        {
            Mode                 = NavMenuMode.Inline,
            IsMotionEnabled      = false,
            IsInlineCollapsed    = true,
            InlineCollapsedWidth = 72
        };
        customMenu.Items.Add(new NavMenuNode
        {
            Header  = "Custom",
            ItemKey = "custom"
        });

        var stack = new StackPanel
        {
            Children =
            {
                defaultMenu,
                customMenu
            }
        };
        var window = new Avalonia.Controls.Window
        {
            Width   = 320,
            Height  = 240,
            Content = stack
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            defaultMenu.InlineCollapsedWidth.ShouldBe(48);
            defaultMenu.Bounds.Width.ShouldBe(48, 0.5);
            customMenu.Bounds.Width.ShouldBe(72, 0.5);
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void InlineCollapsed_Toggle_Restores_Explicit_Expanded_Width()
    {
        var menu = new AtomUI.Desktop.Controls.NavMenu
        {
            Mode            = NavMenuMode.Inline,
            IsMotionEnabled = false,
            MinWidth        = 160,
            Width           = 240
        };
        menu.Items.Add(new NavMenuNode
        {
            Header  = "Default",
            ItemKey = "default"
        });

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

            menu.Bounds.Width.ShouldBe(240, 0.5);

            menu.IsInlineCollapsed = true;
            Dispatcher.UIThread.RunJobs();

            menu.Bounds.Width.ShouldBe(48, 0.5);

            menu.IsInlineCollapsed = false;
            Dispatcher.UIThread.RunJobs();

            menu.Width.ShouldBe(240);
            menu.Bounds.Width.ShouldBe(240, 0.5);
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void InlineCollapsed_Toggle_With_Motion_Animates_Width_Instead_Of_Jumping_To_Target()
    {
        var menu = new AtomUI.Desktop.Controls.NavMenu
        {
            Mode              = NavMenuMode.Inline,
            IsMotionEnabled   = true,
            IsInlineCollapsed = false,
            Width             = 300
        };
        menu.Items.Add(new NavMenuNode
        {
            Header  = "Default",
            ItemKey = "default"
        });

        var window = new Avalonia.Controls.Window
        {
            Width   = 420,
            Height  = 240,
            Content = menu
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            menu.Bounds.Width.ShouldBe(300, 0.5);

            menu.IsInlineCollapsed = true;
            Dispatcher.UIThread.RunJobs();
            Thread.Sleep(20);
            Dispatcher.UIThread.RunJobs();

            menu.Bounds.Width.ShouldBeGreaterThan(menu.InlineCollapsedWidth,
                "motion enabled inline collapsed should expose an intermediate layout width instead of jumping from expanded width to the collapsed target in one frame.");
            WaitForWidthBetween(menu, menu.InlineCollapsedWidth, 300);

            WaitForWidth(menu, menu.InlineCollapsedWidth);
            WaitForInlineCollapsedLayoutWidthCleared(menu);

            menu.Bounds.Width.ShouldBe(menu.InlineCollapsedWidth, 0.5);
            menu.GetBaseValue(Layoutable.WidthProperty).Value.ShouldBe(300);
            double.IsNaN(menu.InlineCollapsedLayoutWidth).ShouldBeTrue(
                "the internal animated layout width must be cleared after collapse motion completes so the collapsed steady state is controlled by InlineCollapsedWidth.");

            menu.IsInlineCollapsed = false;
            Dispatcher.UIThread.RunJobs();
            Thread.Sleep(20);
            Dispatcher.UIThread.RunJobs();

            menu.Bounds.Width.ShouldBeLessThan(300);
            WaitForWidthBetween(menu, menu.InlineCollapsedWidth, 300);

            WaitForWidth(menu, 300);
            WaitForInlineCollapsedLayoutWidthCleared(menu);

            menu.Bounds.Width.ShouldBe(300, 0.5);
            menu.GetBaseValue(Layoutable.WidthProperty).Value.ShouldBe(300);
            double.IsNaN(menu.InlineCollapsedLayoutWidth).ShouldBeTrue(
                "the internal animated layout width must be cleared after expand motion completes so future Width changes are not constrained by stale animation state.");
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void InlineCollapsed_Coerces_Effective_Width_And_Preserves_Base_Width_For_Expand()
    {
        var menu = new AtomUI.Desktop.Controls.NavMenu
        {
            Mode            = NavMenuMode.Inline,
            IsMotionEnabled = false,
            MinWidth        = 160,
            Width           = 240
        };
        menu.Items.Add(new NavMenuNode
        {
            Header  = "Default",
            ItemKey = "default"
        });

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

            menu.GetBaseValue(Layoutable.WidthProperty).Value.ShouldBe(240);
            menu.MinWidth.ShouldBe(160);

            menu.IsInlineCollapsed = true;
            Dispatcher.UIThread.RunJobs();

            menu.Width.ShouldBe(48);
            menu.MinWidth.ShouldBe(48);
            menu.GetBaseValue(Layoutable.WidthProperty).Value.ShouldBe(240);
            menu.GetBaseValue(Layoutable.MinWidthProperty).Value.ShouldBe(160);

            menu.IsInlineCollapsed = false;
            Dispatcher.UIThread.RunJobs();

            menu.Width.ShouldBe(240);
            menu.MinWidth.ShouldBe(160);
            menu.GetBaseValue(Layoutable.WidthProperty).Value.ShouldBe(240);
            menu.GetBaseValue(Layoutable.MinWidthProperty).Value.ShouldBe(160);
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void InlineCollapsed_Uses_Effective_Vertical_Mode_Without_Changing_Public_Mode()
    {
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

        var menu = new AtomUI.Desktop.Controls.NavMenu
        {
            Mode              = NavMenuMode.Inline,
            IsInlineCollapsed = true,
            IsMotionEnabled   = false
        };
        menu.Items.Add(parent);

        var window = new Avalonia.Controls.Window
        {
            Width   = 320,
            Height  = 240,
            Content = CreatePopupOverlayHost(menu)
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            menu.Mode.ShouldBe(NavMenuMode.Inline);
            menu.EffectiveMode.ShouldBe(NavMenuMode.Vertical);
            menu.InteractionHandler.ShouldBeOfType<DefaultNavMenuInteractionHandler>();

            var parentContainer = (NavMenuItem)menu.ContainerFromItem(parent)!;
            parentContainer.Mode.ShouldBe(NavMenuMode.Vertical);

            ((INavMenuItem)parentContainer).Open();
            Dispatcher.UIThread.RunJobs();

            parentContainer.Popup.ShouldNotBeNull();
            parentContainer.Popup.IsOpen.ShouldBeTrue(
                "inline collapsed reuses the popup branch instead of realizing inline child visuals in the root tree.");
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void InlineCollapsed_Toggle_Preserves_SelectedItem_And_Restores_Open_Inline_Path()
    {
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
        var root = new NavMenuNode
        {
            Header  = "Root",
            ItemKey = "root"
        };
        root.Children.Add(parent);

        var menu = new AtomUI.Desktop.Controls.NavMenu
        {
            Mode            = NavMenuMode.Inline,
            IsMotionEnabled = true,
            Width           = 240
        };
        menu.Items.Add(root);

        var window = new Avalonia.Controls.Window
        {
            Width   = 320,
            Height  = 240,
            Content = CreatePopupOverlayHost(menu)
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            var rootContainer = (NavMenuItem)menu.ContainerFromItem(root)!;
            rootContainer.Open();
            Dispatcher.UIThread.RunJobs();

            var parentContainer = (NavMenuItem)rootContainer.ContainerFromItem(parent)!;
            parentContainer.Open();
            Dispatcher.UIThread.RunJobs();

            var childContainer = (NavMenuItem)parentContainer.ContainerFromItem(child)!;
            menu.InteractionHandler.ShouldNotBeNull();
            menu.SelectNavMenuItem(childContainer);
            Dispatcher.UIThread.RunJobs();

            menu.SelectedItem.ShouldBeSameAs(child);
            rootContainer.IsSubMenuOpen.ShouldBeTrue();
            parentContainer.IsSubMenuOpen.ShouldBeTrue();
            rootContainer.IsInSelectedPath.ShouldBeTrue();
            parentContainer.IsInSelectedPath.ShouldBeTrue();

            for (var cycle = 0; cycle < 2; cycle++)
            {
                menu.IsInlineCollapsed = true;
                WaitForWidth(menu, menu.InlineCollapsedWidth);

                menu.Mode.ShouldBe(NavMenuMode.Inline);
                menu.EffectiveMode.ShouldBe(NavMenuMode.Vertical);
                menu.SelectedItem.ShouldBeSameAs(child);
                rootContainer.IsSubMenuOpen.ShouldBeFalse();
                parentContainer.IsSubMenuOpen.ShouldBeFalse(
                    "entering collapsed state closes inline child visuals in the root tree.");
                rootContainer.IsInSelectedPath.ShouldBeTrue();

                menu.IsInlineCollapsed = false;
                Dispatcher.UIThread.RunJobs();

                rootContainer   = menu.ContainerFromItem(root).ShouldBeOfType<NavMenuItem>();
                parentContainer = rootContainer.ContainerFromItem(parent).ShouldBeOfType<NavMenuItem>();
                parentContainer.IsSubMenuOpen.ShouldBeTrue(
                    $"expand cycle {cycle + 1} must restore the nested path before width motion completes.");
                WaitForWidth(menu, 240);

                rootContainer   = menu.ContainerFromItem(root).ShouldBeOfType<NavMenuItem>();
                parentContainer = rootContainer.ContainerFromItem(parent).ShouldBeOfType<NavMenuItem>();
                childContainer  = parentContainer.ContainerFromItem(child).ShouldBeOfType<NavMenuItem>();

                menu.EffectiveMode.ShouldBe(NavMenuMode.Inline);
                menu.SelectedItem.ShouldBeSameAs(child);
                rootContainer.IsSubMenuOpen.ShouldBeTrue(
                    $"leaving collapsed state must restore the root inline open path on cycle {cycle + 1}.");
                parentContainer.IsSubMenuOpen.ShouldBeTrue(
                    $"leaving collapsed state must restore the cached inline open path on cycle {cycle + 1}.");
                rootContainer.IsInSelectedPath.ShouldBeTrue();
                parentContainer.IsInSelectedPath.ShouldBeTrue();
                childContainer.IsSelected.ShouldBeTrue();
            }
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void InlineCollapsed_Toggle_Then_Selecting_Sibling_Clears_Previous_Realized_Selection()
    {
        var firstChild = new NavMenuNode
        {
            Header  = "First",
            ItemKey = "first"
        };
        var secondChild = new NavMenuNode
        {
            Header  = "Second",
            ItemKey = "second"
        };
        var parent = new NavMenuNode
        {
            Header  = "Parent",
            ItemKey = "parent"
        };
        parent.Children.Add(firstChild);
        parent.Children.Add(secondChild);
        var root = new NavMenuNode
        {
            Header  = "Root",
            ItemKey = "root"
        };
        root.Children.Add(parent);

        var menu = new AtomUI.Desktop.Controls.NavMenu
        {
            Mode            = NavMenuMode.Inline,
            IsMotionEnabled = true,
            Width           = 240
        };
        menu.Items.Add(root);

        var window = new Avalonia.Controls.Window
        {
            Width   = 320,
            Height  = 240,
            Content = CreatePopupOverlayHost(menu)
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            var rootContainer = menu.ContainerFromItem(root).ShouldBeOfType<NavMenuItem>();
            rootContainer.Open();
            Dispatcher.UIThread.RunJobs();

            var parentContainer = rootContainer.ContainerFromItem(parent).ShouldBeOfType<NavMenuItem>();
            parentContainer.Open();
            Dispatcher.UIThread.RunJobs();

            var firstContainer = parentContainer.ContainerFromItem(firstChild).ShouldBeOfType<NavMenuItem>();
            menu.InteractionHandler.ShouldNotBeNull();
            menu.SelectNavMenuItem(firstContainer);
            Dispatcher.UIThread.RunJobs();

            for (var cycle = 0; cycle < 2; cycle++)
            {
                menu.IsInlineCollapsed = true;
                WaitForWidth(menu, menu.InlineCollapsedWidth);

                menu.IsInlineCollapsed = false;
                WaitForWidth(menu, 240);
            }

            rootContainer   = menu.ContainerFromItem(root).ShouldBeOfType<NavMenuItem>();
            parentContainer = rootContainer.ContainerFromItem(parent).ShouldBeOfType<NavMenuItem>();
            firstContainer  = parentContainer.ContainerFromItem(firstChild).ShouldBeOfType<NavMenuItem>();
            var secondContainer = parentContainer.ContainerFromItem(secondChild).ShouldBeOfType<NavMenuItem>();

            firstContainer.IsSelected.ShouldBeTrue();
            menu.InteractionHandler.ShouldNotBeNull();
            menu.SelectNavMenuItem(secondContainer);
            Dispatcher.UIThread.RunJobs();

            menu.SelectedItem.ShouldBeSameAs(secondChild);
            firstContainer.IsSelected.ShouldBeFalse(
                "selecting a new leaf must clear the previously selected leaf after template recreation.");
            secondContainer.IsSelected.ShouldBeTrue();
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void InlineCollapsed_Opening_Popup_Projects_Selected_State_To_New_Leaf_Container()
    {
        var borderBeam = new NavMenuNode
        {
            Header  = "BorderBeam",
            ItemKey = "border-beam"
        };
        var splash = new NavMenuNode
        {
            Header  = "Splash",
            ItemKey = "splash"
        };
        var other = new NavMenuNode
        {
            Header  = "Other",
            ItemKey = "other"
        };
        other.Children.Add(borderBeam);
        other.Children.Add(splash);
        var components = new NavMenuNode
        {
            Header  = "Components",
            ItemKey = "components"
        };
        components.Children.Add(other);

        var menu = new AtomUI.Desktop.Controls.NavMenu
        {
            Mode            = NavMenuMode.Inline,
            IsMotionEnabled = true,
            Width           = 240
        };
        menu.Items.Add(components);

        var window = new Avalonia.Controls.Window
        {
            Width   = 480,
            Height  = 320,
            Content = CreatePopupOverlayHost(menu)
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            var componentsContainer = menu.ContainerFromItem(components).ShouldBeOfType<NavMenuItem>();
            componentsContainer.Open();
            Dispatcher.UIThread.RunJobs();

            var otherContainer = componentsContainer.ContainerFromItem(other).ShouldBeOfType<NavMenuItem>();
            otherContainer.Open();
            Dispatcher.UIThread.RunJobs();

            var splashContainer = otherContainer.ContainerFromItem(splash).ShouldBeOfType<NavMenuItem>();
            menu.InteractionHandler.ShouldNotBeNull();
            menu.SelectNavMenuItem(splashContainer);
            Dispatcher.UIThread.RunJobs();

            menu.SelectedItem.ShouldBeSameAs(splash);
            splashContainer.IsSelected.ShouldBeTrue();

            menu.IsInlineCollapsed = true;
            WaitForWidth(menu, menu.InlineCollapsedWidth);

            componentsContainer = menu.ContainerFromItem(components).ShouldBeOfType<NavMenuItem>();
            for (var cycle = 0; cycle < 2; cycle++)
            {
                menu.InteractionHandler.ShouldNotBeNull();
                componentsContainer.Open();
                Dispatcher.UIThread.RunJobs();

                otherContainer = componentsContainer.ContainerFromItem(other).ShouldBeOfType<NavMenuItem>();
                otherContainer.Open();
                Dispatcher.UIThread.RunJobs();

                splashContainer = otherContainer.ContainerFromItem(splash).ShouldBeOfType<NavMenuItem>();
                menu.SelectedItem.ShouldBeSameAs(splash);
                splashContainer.IsSelected.ShouldBeTrue(
                    $"popup realization cycle {cycle + 1} must project the persistent selected node to its current leaf container.");

                otherContainer.Close();
                componentsContainer.Close();
                Dispatcher.UIThread.RunJobs();
            }
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void InlineCollapsed_Opening_Sibling_Submenu_Preserves_Selected_Path()
    {
        var space = new NavMenuNode
        {
            Header  = "Space",
            ItemKey = "space"
        };
        var layout = new NavMenuNode
        {
            Header  = "Layout",
            ItemKey = "layout"
        };
        layout.Children.Add(space);

        var autoComplete = new NavMenuNode
        {
            Header  = "AutoComplete",
            ItemKey = "auto-complete"
        };
        var dataEntry = new NavMenuNode
        {
            Header  = "Data Entry",
            ItemKey = "data-entry"
        };
        dataEntry.Children.Add(autoComplete);

        var components = new NavMenuNode
        {
            Header  = "Components",
            ItemKey = "components"
        };
        components.Children.Add(layout);
        components.Children.Add(dataEntry);

        var menu = new AtomUI.Desktop.Controls.NavMenu
        {
            Mode            = NavMenuMode.Inline,
            IsMotionEnabled = false,
            Width           = 240
        };
        menu.Items.Add(components);

        var window = new Avalonia.Controls.Window
        {
            Width   = 480,
            Height  = 320,
            Content = CreatePopupOverlayHost(menu)
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            var componentsContainer = menu.ContainerFromItem(components).ShouldBeOfType<NavMenuItem>();
            componentsContainer.Open();
            Dispatcher.UIThread.RunJobs();

            var layoutContainer = componentsContainer.ContainerFromItem(layout).ShouldBeOfType<NavMenuItem>();
            layoutContainer.Open();
            Dispatcher.UIThread.RunJobs();

            var spaceContainer = layoutContainer.ContainerFromItem(space).ShouldBeOfType<NavMenuItem>();
            menu.InteractionHandler.ShouldNotBeNull();
            menu.SelectNavMenuItem(spaceContainer);
            Dispatcher.UIThread.RunJobs();

            menu.IsInlineCollapsed = true;
            WaitForWidth(menu, menu.InlineCollapsedWidth);

            componentsContainer = menu.ContainerFromItem(components).ShouldBeOfType<NavMenuItem>();
            componentsContainer.Open();
            Dispatcher.UIThread.RunJobs();

            layoutContainer = componentsContainer.ContainerFromItem(layout).ShouldBeOfType<NavMenuItem>();
            var dataEntryContainer = componentsContainer.ContainerFromItem(dataEntry).ShouldBeOfType<NavMenuItem>();
            layoutContainer.IsInSelectedPath.ShouldBeTrue();

            dataEntryContainer.Open();
            Dispatcher.UIThread.RunJobs();

            menu.SelectedItem.ShouldBeSameAs(space);
            layoutContainer.IsInSelectedPath.ShouldBeTrue(
                "opening a sibling submenu must not replace the route's selected path.");
            dataEntryContainer.IsInSelectedPath.ShouldBeFalse();
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void InlineCollapsed_Selecting_Leaf_In_Sibling_Branch_Clears_Previous_Selected_Path()
    {
        var space = new NavMenuNode
        {
            Header  = "Space",
            ItemKey = "space"
        };
        var layout = new NavMenuNode
        {
            Header  = "Layout",
            ItemKey = "layout"
        };
        layout.Children.Add(space);

        var buttonSpinner = new NavMenuNode
        {
            Header  = "ButtonSpinner",
            ItemKey = "button-spinner"
        };
        var navigation = new NavMenuNode
        {
            Header  = "Navigation",
            ItemKey = "navigation"
        };
        navigation.Children.Add(buttonSpinner);

        var components = new NavMenuNode
        {
            Header  = "Components",
            ItemKey = "components"
        };
        components.Children.Add(layout);
        components.Children.Add(navigation);

        var menu = new AtomUI.Desktop.Controls.NavMenu
        {
            Mode            = NavMenuMode.Inline,
            IsMotionEnabled = false,
            Width           = 240
        };
        menu.Items.Add(components);

        var window = new Avalonia.Controls.Window
        {
            Width   = 480,
            Height  = 320,
            Content = CreatePopupOverlayHost(menu)
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            var componentsContainer = menu.ContainerFromItem(components).ShouldBeOfType<NavMenuItem>();
            componentsContainer.Open();
            Dispatcher.UIThread.RunJobs();

            var layoutContainer = componentsContainer.ContainerFromItem(layout).ShouldBeOfType<NavMenuItem>();
            layoutContainer.Open();
            Dispatcher.UIThread.RunJobs();

            var spaceContainer = layoutContainer.ContainerFromItem(space).ShouldBeOfType<NavMenuItem>();
            menu.InteractionHandler.ShouldNotBeNull();
            menu.SelectNavMenuItem(spaceContainer);
            Dispatcher.UIThread.RunJobs();

            menu.IsInlineCollapsed = true;
            WaitForWidth(menu, menu.InlineCollapsedWidth);

            componentsContainer = menu.ContainerFromItem(components).ShouldBeOfType<NavMenuItem>();
            componentsContainer.Open();
            Dispatcher.UIThread.RunJobs();

            layoutContainer = componentsContainer.ContainerFromItem(layout).ShouldBeOfType<NavMenuItem>();
            var navigationContainer = componentsContainer.ContainerFromItem(navigation).ShouldBeOfType<NavMenuItem>();
            layoutContainer.IsInSelectedPath.ShouldBeTrue();

            navigationContainer.Open();
            Dispatcher.UIThread.RunJobs();

            layoutContainer.IsSubMenuOpen.ShouldBeFalse(
                "opening the sibling branch must close the popup that contains the previous selected leaf.");
            var buttonSpinnerContainer = navigationContainer.ContainerFromItem(buttonSpinner)
                                                                  .ShouldBeOfType<NavMenuItem>();
            menu.SelectNavMenuItem(buttonSpinnerContainer);
            Dispatcher.UIThread.RunJobs();

            menu.SelectedItem.ShouldBeSameAs(buttonSpinner);
            buttonSpinnerContainer.IsSelected.ShouldBeTrue();
            navigationContainer.IsInSelectedPath.ShouldBeTrue();
            layoutContainer.IsInSelectedPath.ShouldBeFalse(
                "selecting a leaf in a sibling popup branch must clear the previous branch's selected-path state even when its leaf container is no longer realized.");
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void InlineCollapsed_Clearing_Unrealized_Selection_Clears_Previous_Selected_Path()
    {
        var space = new NavMenuNode
        {
            Header  = "Space",
            ItemKey = "space"
        };
        var layout = new NavMenuNode
        {
            Header  = "Layout",
            ItemKey = "layout"
        };
        layout.Children.Add(space);

        var navigation = new NavMenuNode
        {
            Header  = "Navigation",
            ItemKey = "navigation"
        };
        navigation.Children.Add(new NavMenuNode
        {
            Header  = "ButtonSpinner",
            ItemKey = "button-spinner"
        });

        var components = new NavMenuNode
        {
            Header  = "Components",
            ItemKey = "components"
        };
        components.Children.Add(layout);
        components.Children.Add(navigation);

        var menu = new AtomUI.Desktop.Controls.NavMenu
        {
            Mode            = NavMenuMode.Inline,
            IsMotionEnabled = false,
            Width           = 240
        };
        menu.Items.Add(components);

        var window = new Avalonia.Controls.Window
        {
            Width   = 480,
            Height  = 320,
            Content = CreatePopupOverlayHost(menu)
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            var componentsContainer = menu.ContainerFromItem(components).ShouldBeOfType<NavMenuItem>();
            componentsContainer.Open();
            Dispatcher.UIThread.RunJobs();

            var layoutContainer = componentsContainer.ContainerFromItem(layout).ShouldBeOfType<NavMenuItem>();
            layoutContainer.Open();
            Dispatcher.UIThread.RunJobs();

            var spaceContainer = layoutContainer.ContainerFromItem(space).ShouldBeOfType<NavMenuItem>();
            menu.InteractionHandler.ShouldNotBeNull();
            menu.SelectNavMenuItem(spaceContainer);
            Dispatcher.UIThread.RunJobs();

            menu.IsInlineCollapsed = true;
            WaitForWidth(menu, menu.InlineCollapsedWidth);

            componentsContainer = menu.ContainerFromItem(components).ShouldBeOfType<NavMenuItem>();
            componentsContainer.Open();
            Dispatcher.UIThread.RunJobs();

            layoutContainer = componentsContainer.ContainerFromItem(layout).ShouldBeOfType<NavMenuItem>();
            var navigationContainer = componentsContainer.ContainerFromItem(navigation).ShouldBeOfType<NavMenuItem>();
            layoutContainer.IsInSelectedPath.ShouldBeTrue();

            navigationContainer.Open();
            Dispatcher.UIThread.RunJobs();

            layoutContainer.IsSubMenuOpen.ShouldBeFalse();
            menu.SelectedItem = null;
            Dispatcher.UIThread.RunJobs();

            layoutContainer.IsInSelectedPath.ShouldBeFalse(
                "clearing a selection must remove the previous selected-path state even when the selected leaf container is no longer realized.");
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void InlineCollapsed_Pointer_Opening_Sibling_Submenu_Does_Not_Project_Selected_State()
    {
        var space = new NavMenuNode
        {
            Header  = "Space",
            ItemKey = "space"
        };
        var layout = new NavMenuNode
        {
            Header  = "Layout",
            ItemKey = "layout"
        };
        layout.Children.Add(space);

        var dataEntry = new NavMenuNode
        {
            Header  = "Data Entry",
            ItemKey = "data-entry"
        };
        dataEntry.Children.Add(new NavMenuNode
        {
            Header  = "AutoComplete",
            ItemKey = "auto-complete"
        });

        var components = new NavMenuNode
        {
            Header  = "Components",
            ItemKey = "components"
        };
        components.Children.Add(layout);
        components.Children.Add(dataEntry);

        var menu = new AtomUI.Desktop.Controls.NavMenu
        {
            Mode            = NavMenuMode.Inline,
            IsMotionEnabled = false,
            Width           = 240
        };
        menu.Items.Add(components);

        var window = new Avalonia.Controls.Window
        {
            Width   = 480,
            Height  = 320,
            Content = CreatePopupOverlayHost(menu)
        };

        var originalShowDelay = DefaultNavMenuInteractionHandler.MenuShowDelay;
        DefaultNavMenuInteractionHandler.MenuShowDelay = TimeSpan.Zero;
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            var componentsContainer = menu.ContainerFromItem(components).ShouldBeOfType<NavMenuItem>();
            componentsContainer.Open();
            Dispatcher.UIThread.RunJobs();

            var layoutContainer = componentsContainer.ContainerFromItem(layout).ShouldBeOfType<NavMenuItem>();
            layoutContainer.Open();
            Dispatcher.UIThread.RunJobs();

            var spaceContainer = layoutContainer.ContainerFromItem(space).ShouldBeOfType<NavMenuItem>();
            menu.InteractionHandler.ShouldNotBeNull();
            menu.SelectNavMenuItem(spaceContainer);
            Dispatcher.UIThread.RunJobs();

            menu.IsInlineCollapsed = true;
            WaitForWidth(menu, menu.InlineCollapsedWidth);

            componentsContainer = menu.ContainerFromItem(components).ShouldBeOfType<NavMenuItem>();
            componentsContainer.Open();
            Dispatcher.UIThread.RunJobs();

            var dataEntryContainer = componentsContainer.ContainerFromItem(dataEntry).ShouldBeOfType<NavMenuItem>();
            var dataEntryHeader = GetItemHeader(dataEntryContainer);
            MouseMove(dataEntryHeader, window);
            Dispatcher.UIThread.RunJobs();

            dataEntryContainer.IsSubMenuOpen.ShouldBeTrue(
                "the regression scenario must reach the nested popup-open state shown in the Gallery screenshot.");
            dataEntryContainer.Popup.ShouldNotBeNull();
            dataEntryContainer.Popup.IsOpen.ShouldBeTrue();
            dataEntryContainer.IsSelected.ShouldBeFalse();
            dataEntryContainer.IsInSelectedPath.ShouldBeFalse();
            dataEntryContainer.IsKeyboardActive.ShouldBeFalse();
            menu.SelectedItem.ShouldBeSameAs(space);
        }
        finally
        {
            DefaultNavMenuInteractionHandler.MenuShowDelay = originalShowDelay;
            window.Close();
        }
    }

    [Fact]
    public void InlineCollapsed_Handler_Recreation_Does_Not_Leave_Previous_Item_Selected()
    {
        var option1 = new NavMenuNode
        {
            Header  = "Option 1",
            ItemKey = "Option1",
            Icon    = new PieChartOutlined()
        };
        var option2 = new NavMenuNode
        {
            Header  = "Option 2",
            ItemKey = "Option2",
            Icon    = new PieChartOutlined()
        };

        var menu = new AtomUI.Desktop.Controls.NavMenu
        {
            Mode                = NavMenuMode.Inline,
            IsMotionEnabled     = false,
            DefaultSelectedPath = new TreeNodePath("/Option1")
        };
        menu.Items.Add(option1);
        menu.Items.Add(option2);

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

            var option1Container = (NavMenuItem)menu.ContainerFromItem(option1)!;
            var option2Container = (NavMenuItem)menu.ContainerFromItem(option2)!;

            option1Container.IsSelected.ShouldBeTrue();
            option2Container.IsSelected.ShouldBeFalse();

            menu.IsInlineCollapsed = true;
            Dispatcher.UIThread.RunJobs();

            menu.InteractionHandler.ShouldNotBeNull();
            menu.SelectNavMenuItem(option2Container);
            Dispatcher.UIThread.RunJobs();

            option1Container.IsSelected.ShouldBeFalse(
                "recreating the interaction handler for inline collapsed mode must not lose the previous selected item, otherwise the next selection cannot clear it.");
            option2Container.IsSelected.ShouldBeTrue();
            menu.SelectedItem.ShouldBeSameAs(option2);
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void InlineCollapsed_Does_Not_Affect_Vertical_Or_Horizontal_Effective_Mode()
    {
        foreach (var mode in new[] { NavMenuMode.Vertical, NavMenuMode.Horizontal })
        {
            var menu = new AtomUI.Desktop.Controls.NavMenu
            {
                Mode              = mode,
                IsInlineCollapsed = true
            };

            menu.EffectiveMode.ShouldBe(mode);
        }
    }

    [Fact]
    public void InlineCollapsed_Icon_Is_Centered_In_Item_Header()
    {
        var item = new NavMenuNode
        {
            Header  = "Dashboard",
            ItemKey = "dashboard",
            Icon    = new PieChartOutlined()
        };

        var menu = new AtomUI.Desktop.Controls.NavMenu
        {
            Mode              = NavMenuMode.Inline,
            IsMotionEnabled   = false,
            IsInlineCollapsed = true,
            IsDarkStyle       = true
        };
        menu.Items.Add(item);
        menu.SelectedItem = item;

        var window = new Avalonia.Controls.Window
        {
            Width   = 240,
            Height  = 240,
            Content = menu
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            var itemContainer = (Control)menu.ContainerFromItem(item)!;
            var itemHeader    = GetItemHeader(itemContainer);
            var frame = itemHeader.GetVisualDescendants()
                                  .OfType<Border>()
                                  .Single(border => border.Name == "Frame");
            var iconPresenter = itemHeader.GetVisualDescendants()
                                          .OfType<IconPresenter>()
                                          .Single(presenter => presenter.Name == "ItemIconPresenter");

            var iconPosition = iconPresenter.TranslatePoint(default, frame);
            iconPosition.ShouldNotBeNull();

            var frameCenter = frame.Bounds.Width / 2;
            var iconCenter  = iconPosition.Value.X + iconPresenter.Bounds.Width / 2;

            iconCenter.ShouldBe(frameCenter, 0.5,
                "the collapsed inline header hides text and indicator visuals, so the icon should be centered in the visible item frame rather than in the original text-layout column.");
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void InlineCollapsed_Root_Group_Items_Use_Collapsed_Icon_Layout_Through_Transparent_Groups()
    {
        var item = new NavMenuNode
        {
            Header  = "Dashboard",
            ItemKey = "dashboard",
            Icon    = new PieChartOutlined()
        };
        var nestedItem = new NavMenuNode
        {
            Header  = "Projects",
            ItemKey = "projects",
            Icon    = new PieChartOutlined()
        };
        var nestedGroup = new NavMenuGroup
        {
            Header = "Nested workspace"
        };
        nestedGroup.Entries.Add(nestedItem);
        var group = new NavMenuGroup
        {
            Header = "Workspace"
        };
        group.Entries.Add(item);
        group.Entries.Add(nestedGroup);

        var menu = new AtomUI.Desktop.Controls.NavMenu
        {
            Mode              = NavMenuMode.Inline,
            IsMotionEnabled   = false,
            IsInlineCollapsed = true
        };
        menu.Items.Add(group);

        var window = new Avalonia.Controls.Window
        {
            Width   = 240,
            Height  = 240,
            Content = menu
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            var groupContainer = (ItemsControl)menu.ContainerFromItem(group)!;
            var itemContainer  = (Control)groupContainer.ContainerFromItem(item)!;
            var itemHeader     = GetItemHeader(itemContainer);
            var nestedGroupContainer = (ItemsControl)groupContainer.ContainerFromItem(nestedGroup)!;
            var nestedItemContainer  = (Control)nestedGroupContainer.ContainerFromItem(nestedItem)!;
            var nestedItemHeader     = GetItemHeader(nestedItemContainer);
            var frame = itemHeader.GetVisualDescendants()
                                  .OfType<Border>()
                                  .Single(border => border.Name == "Frame");
            var iconPresenter = itemHeader.GetVisualDescendants()
                                          .OfType<IconPresenter>()
                                          .Single(presenter => presenter.Name == "ItemIconPresenter");
            var nestedIconPresenter = nestedItemHeader.GetVisualDescendants()
                                                       .OfType<IconPresenter>()
                                                       .Single(presenter => presenter.Name == "ItemIconPresenter");

            var iconPosition = iconPresenter.TranslatePoint(default, frame);
            iconPosition.ShouldNotBeNull();

            var frameCenter = frame.Bounds.Width / 2;
            var iconCenter  = iconPosition.Value.X + iconPresenter.Bounds.Width / 2;

            itemHeader.IsInlineCollapsed.ShouldBeTrue(
                "a group is structurally transparent, so a root group item must receive the root inline-collapsed state.");
            nestedItemHeader.IsInlineCollapsed.ShouldBeTrue(
                "nested transparent root groups must continue forwarding the root inline-collapsed state.");
            iconPresenter.Bounds.Width.ShouldBe(16, 0.5,
                "root group items must use the same CollapsedIconSize as direct root items.");
            iconPresenter.Bounds.Height.ShouldBe(16, 0.5,
                "root group items must use the same CollapsedIconSize as direct root items.");
            iconCenter.ShouldBe(frameCenter, 0.5,
                "root group items must center their collapsed icon in the visible item frame.");
            nestedIconPresenter.Bounds.Width.ShouldBe(16, 0.5,
                "transparent group depth must not change the root collapsed icon size.");
            nestedIconPresenter.Bounds.Height.ShouldBe(16, 0.5,
                "transparent group depth must not change the root collapsed icon size.");
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void InlineCollapsed_Hides_Footer_And_Restores_It_After_Expand()
    {
        var menu = new AtomUI.Desktop.Controls.NavMenu
        {
            Mode              = NavMenuMode.Inline,
            IsMotionEnabled   = false,
            IsInlineCollapsed = true,
            Footer            = new TextBlock { Text = "Current workspace" }
        };
        menu.Items.Add(new NavMenuNode
        {
            Header  = "Dashboard",
            ItemKey = "dashboard"
        });

        var window = new Avalonia.Controls.Window
        {
            Width   = 240,
            Height  = 240,
            Content = menu
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            var footerPresenter = menu.GetVisualDescendants()
                                      .OfType<ContentPresenter>()
                                      .Single(presenter => presenter.Name == "PART_FooterPresenter");

            footerPresenter.IsVisible.ShouldBeFalse(
                "the expanded footer has no collapsed representation and must not render partial content inside the collapsed width.");

            menu.IsInlineCollapsed = false;
            Dispatcher.UIThread.RunJobs();

            footerPresenter.IsVisible.ShouldBeTrue(
                "expanding the menu must restore the configured footer without rebuilding its content.");
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void InlineCollapsed_Popup_Group_Item_Keeps_Normal_Vertical_Icon_Layout()
    {
        var child = new NavMenuNode
        {
            Header  = "Profile",
            ItemKey = "profile",
            Icon    = new PieChartOutlined()
        };
        var group = new NavMenuGroup
        {
            Header = "Account"
        };
        group.Entries.Add(child);
        var parent = new NavMenuNode
        {
            Header  = "Settings",
            ItemKey = "settings",
            Icon    = new PieChartOutlined()
        };
        parent.Entries.Add(group);

        var menu = new AtomUI.Desktop.Controls.NavMenu
        {
            Mode              = NavMenuMode.Inline,
            IsMotionEnabled   = false,
            IsInlineCollapsed = true
        };
        menu.Items.Add(parent);

        var window = new Avalonia.Controls.Window
        {
            Width   = 480,
            Height  = 320,
            Content = CreatePopupOverlayHost(menu)
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            var parentContainer = (NavMenuItem)menu.ContainerFromItem(parent)!;
            parentContainer.Open();
            Dispatcher.UIThread.RunJobs();

            var groupContainer = (ItemsControl)parentContainer.ContainerFromItem(group)!;
            var childContainer = (Control)groupContainer.ContainerFromItem(child)!;
            var childHeader    = GetItemHeader(childContainer);
            var iconPresenter = childHeader.GetVisualDescendants()
                                           .OfType<IconPresenter>()
                                           .Single(presenter => presenter.Name == "ItemIconPresenter");

            childHeader.IsInlineCollapsed.ShouldBeFalse(
                "groups inside a popup are not root-transparent entries and must keep normal vertical item visuals.");
            iconPresenter.Bounds.Width.ShouldBe(14, 0.5,
                "popup group items must keep ItemIconSize instead of the root-only CollapsedIconSize.");
            iconPresenter.Bounds.Height.ShouldBe(14, 0.5,
                "popup group items must keep ItemIconSize instead of the root-only CollapsedIconSize.");
        }
        finally
        {
            window.Close();
        }
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

    private static BaseNavMenuItemHeader GetItemHeader(Control container)
    {
        var field = container.GetType().GetField(
            "_itemHeader",
            BindingFlags.Instance | BindingFlags.NonPublic);
        field.ShouldNotBeNull();
        return (BaseNavMenuItemHeader)field.GetValue(container)!;
    }

    private static void MouseMove(Control control, Avalonia.Controls.Window window)
    {
        var point = control.TranslatePoint(
            new Point(control.Bounds.Width / 2, control.Bounds.Height / 2),
            window);
        point.ShouldNotBeNull();
        window.MouseMove(point.Value);
    }

    private static void WaitForWidth(Control control, double expectedWidth)
    {
        var deadline = DateTime.UtcNow + TimeSpan.FromSeconds(2);
        while (DateTime.UtcNow < deadline)
        {
            // RunJobs alone does not advance idle dispatcher timers in the headless host.
            Dispatcher.UIThread.Post(static () => { }, DispatcherPriority.Background);
            Dispatcher.UIThread.RunJobs(DispatcherPriority.SystemIdle);
            if (Math.Abs(control.Bounds.Width - expectedWidth) <= 0.5)
            {
                return;
            }

            Thread.Sleep(20);
        }

        Dispatcher.UIThread.RunJobs(DispatcherPriority.SystemIdle);
    }

    private static void WaitForWidthBetween(Control control, double lowerBound, double upperBound)
    {
        var deadline = DateTime.UtcNow + TimeSpan.FromSeconds(1);
        while (DateTime.UtcNow < deadline)
        {
            Dispatcher.UIThread.Post(static () => { }, DispatcherPriority.Background);
            Dispatcher.UIThread.RunJobs(DispatcherPriority.SystemIdle);
            if (control.Bounds.Width > lowerBound + 0.5 &&
                control.Bounds.Width < upperBound - 0.5)
            {
                return;
            }

            Thread.Sleep(10);
        }

        control.Bounds.Width.ShouldBeGreaterThan(lowerBound);
        control.Bounds.Width.ShouldBeLessThan(upperBound);
    }

    private static void WaitForInlineCollapsedLayoutWidthCleared(AtomUI.Desktop.Controls.NavMenu menu)
    {
        var deadline = DateTime.UtcNow + TimeSpan.FromSeconds(1);
        while (DateTime.UtcNow < deadline)
        {
            Dispatcher.UIThread.Post(static () => { }, DispatcherPriority.Background);
            Dispatcher.UIThread.RunJobs(DispatcherPriority.SystemIdle);
            if (double.IsNaN(menu.InlineCollapsedLayoutWidth))
            {
                return;
            }

            Thread.Sleep(10);
        }

        double.IsNaN(menu.InlineCollapsedLayoutWidth).ShouldBeTrue();
    }
}
