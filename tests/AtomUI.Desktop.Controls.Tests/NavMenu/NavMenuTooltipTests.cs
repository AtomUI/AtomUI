using System.ComponentModel;
using AtomUI.Controls;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Templates;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;

namespace AtomUI.Desktop.Controls.Tests.NavMenu;

public class NavMenuTooltipTests
{
    static NavMenuTooltipTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Collapsed_Leaf_Uses_Explicit_Tooltip_And_Menu_Tooltip_Settings()
    {
        var node = new NavMenuNode
        {
            Header  = "Dashboard",
            Tooltip = "Open dashboard"
        };
        var menu = CreateCollapsedMenu(node);
        menu.CollapsedTooltipPlacement        = PlacementMode.Left;
        menu.CollapsedTooltipShowDelay        = 250;
        menu.CollapsedTooltipBetweenShowDelay = 75;

        ShowInWindow(menu, () =>
        {
            var container = menu.ContainerFromItem(node).ShouldBeOfType<NavMenuItem>();
            var header    = container.ItemHeader.ShouldNotBeNull();

            container.EffectiveCollapsedTooltip.ShouldBe("Open dashboard");
            ToolTip.GetTip(header).ShouldBe("Open dashboard");
            ToolTip.GetPlacement(header).ShouldBe(PlacementMode.Left);
            ToolTip.GetShowDelay(header).ShouldBe(250);
            ToolTip.GetBetweenShowDelay(header).ShouldBe(75);
        });
    }

    [Fact]
    public void Collapsed_Leaf_Falls_Back_To_Node_Header_When_Tooltip_Is_Unset()
    {
        var node = new NavMenuNode { Header = "Settings" };
        var menu = CreateCollapsedMenu(node);

        ShowInWindow(menu, () =>
        {
            var container = menu.ContainerFromItem(node).ShouldBeOfType<NavMenuItem>();
            var header    = container.ItemHeader.ShouldBeOfType<VerticalNavMenuItemHeader>();
            var collapsedTitle = header.GetVisualDescendants()
                .OfType<ContentPresenter>()
                .Single(presenter => presenter.Name == "CollapsedTitlePresenter");

            container.NodeHeader.ShouldBe("Settings");
            container.EffectiveCollapsedTooltip.ShouldBe("Settings");
            ToolTip.GetTip(header).ShouldBe("Settings");
            collapsedTitle.Content.ShouldBe("S");
        });
    }

    [Fact]
    public void Node_Tooltip_ToolTip_Instance_Passes_Through_To_Header()
    {
        // ToolTip 实例作为节点 Tooltip：实例自身显式设置的呈现类附加属性
        // 在打开时优先于宿主（PART_Header）的值，NavMenu 只负责原样透传
        var tip = new ToolTip { Content = "Open dashboard" };
        ToolTip.SetPlacement(tip, PlacementMode.Left);
        var node = new NavMenuNode
        {
            Header  = "Dashboard",
            Tooltip = tip
        };
        var menu = CreateCollapsedMenu(node);

        ShowInWindow(menu, () =>
        {
            var container = menu.ContainerFromItem(node).ShouldBeOfType<NavMenuItem>();
            var header    = container.ItemHeader.ShouldNotBeNull();

            container.EffectiveCollapsedTooltip.ShouldBeSameAs(tip);
            ToolTip.GetTip(header).ShouldBeSameAs(tip);
        });
    }

    [Fact]
    public void Tooltip_Is_Suppressed_For_Disabled_Expanded_And_Submenu_Items()
    {
        var disabledNode = new NavMenuNode
        {
            Header           = "Disabled",
            Tooltip          = "Disabled tooltip",
            IsTooltipEnabled = false
        };
        var parentNode = new NavMenuNode
        {
            Header  = "Parent",
            Tooltip = "Parent tooltip"
        };
        parentNode.Children.Add(new NavMenuNode { Header = "Child" });

        var menu = CreateCollapsedMenu(disabledNode, parentNode);

        ShowInWindow(menu, () =>
        {
            var disabledContainer = menu.ContainerFromItem(disabledNode).ShouldBeOfType<NavMenuItem>();
            var parentContainer   = menu.ContainerFromItem(parentNode).ShouldBeOfType<NavMenuItem>();

            disabledContainer.EffectiveCollapsedTooltip.ShouldBeNull();
            ToolTip.GetTip(disabledContainer.ItemHeader.ShouldNotBeNull()).ShouldBeNull();
            parentContainer.HasSubMenu.ShouldBeTrue();
            parentContainer.EffectiveCollapsedTooltip.ShouldBeNull();
            ToolTip.GetTip(parentContainer.ItemHeader.ShouldNotBeNull()).ShouldBeNull();

            menu.IsInlineCollapsed = false;
            Dispatcher.UIThread.RunJobs();

            disabledNode.IsTooltipEnabled = true;
            Dispatcher.UIThread.RunJobs();
            disabledContainer.EffectiveCollapsedTooltip.ShouldBeNull();
        });
    }

    [Fact]
    public void Runtime_Node_And_Menu_Changes_Recompute_Effective_Tooltip()
    {
        var node = new NavMenuNode { Header = "Initial" };
        var menu = CreateCollapsedMenu(node);

        ShowInWindow(menu, () =>
        {
            var container = menu.ContainerFromItem(node).ShouldBeOfType<NavMenuItem>();

            container.EffectiveCollapsedTooltip.ShouldBe("Initial");

            node.Header = "Updated header";
            Dispatcher.UIThread.RunJobs();
            container.EffectiveCollapsedTooltip.ShouldBe("Updated header");

            node.Tooltip = "Explicit tooltip";
            Dispatcher.UIThread.RunJobs();
            container.EffectiveCollapsedTooltip.ShouldBe("Explicit tooltip");

            menu.IsCollapsedTooltipEnabled = false;
            Dispatcher.UIThread.RunJobs();
            container.EffectiveCollapsedTooltip.ShouldBeNull();

            menu.IsCollapsedTooltipEnabled = true;
            node.IsTooltipEnabled           = false;
            Dispatcher.UIThread.RunJobs();
            container.EffectiveCollapsedTooltip.ShouldBeNull();

            node.IsTooltipEnabled = true;
            node.Tooltip          = null;
            Dispatcher.UIThread.RunJobs();
            container.EffectiveCollapsedTooltip.ShouldBe("Updated header");
        });
    }

    [Fact]
    public void Observable_Custom_Node_Updates_Projected_Tooltip_State()
    {
        var node = new ObservableTooltipNode("Initial");
        var menu = new AtomUI.Desktop.Controls.NavMenu
        {
            Mode              = NavMenuMode.Inline,
            IsInlineCollapsed = true,
            IsMotionEnabled   = false
        };
        menu.Items.Add(node);

        ShowInWindow(menu, () =>
        {
            var container = menu.ContainerFromItem(node).ShouldBeOfType<NavMenuItem>();

            container.EffectiveCollapsedTooltip.ShouldBe("Initial");

            node.SetTooltip("Custom tooltip");
            container.EffectiveCollapsedTooltip.ShouldBe("Custom tooltip");

            node.SetTooltipEnabled(false);
            container.EffectiveCollapsedTooltip.ShouldBeNull();

            node.SetTooltipEnabled(true);
            node.SetTooltip(null);
            node.SetHeader("Updated");
            container.EffectiveCollapsedTooltip.ShouldBe("Updated");
        });
    }

    [Fact]
    public void Removed_Container_Releases_All_Tooltip_Bindings()
    {
        var node = new NavMenuNode
        {
            Header  = "Initial",
            Tooltip = "Initial tooltip"
        };
        var menu = CreateCollapsedMenu(node);

        ShowInWindow(menu, () =>
        {
            var container = menu.ContainerFromItem(node).ShouldBeOfType<NavMenuItem>();
            var header    = container.ItemHeader.ShouldNotBeNull();
            container.EffectiveCollapsedTooltip.ShouldBe("Initial tooltip");

            menu.Items.Remove(node);
            Dispatcher.UIThread.RunJobs();
            menu.UpdateLayout();

            menu.ContainerFromItem(node).ShouldBeNull();
            container.OwnerMenu.ShouldBeNull();

            node.Header                              = "Detached header";
            node.Tooltip                             = "Detached tooltip";
            node.IsTooltipEnabled                    = false;
            menu.IsCollapsedTooltipEnabled           = false;
            menu.CollapsedTooltipPlacement           = PlacementMode.Left;
            menu.CollapsedTooltipShowDelay           = 10;
            menu.CollapsedTooltipBetweenShowDelay    = 20;
            Dispatcher.UIThread.RunJobs();

            container.NodeHeader.ShouldBeNull();
            container.Tooltip.ShouldBeNull();
            container.IsTooltipEnabled.ShouldBeTrue();
            container.IsCollapsedTooltipEnabled.ShouldBeTrue();
            container.CollapsedTooltipPlacement.ShouldBe(PlacementMode.Right);
            container.CollapsedTooltipShowDelay.ShouldBe(400);
            container.CollapsedTooltipBetweenShowDelay.ShouldBe(100);
            container.EffectiveCollapsedTooltip.ShouldBeNull();
            ToolTip.GetTip(header).ShouldBeNull();
        });
    }

    private static AtomUI.Desktop.Controls.NavMenu CreateCollapsedMenu(params NavMenuNode[] nodes)
    {
        var menu = new AtomUI.Desktop.Controls.NavMenu
        {
            Mode              = NavMenuMode.Inline,
            IsInlineCollapsed = true,
            IsMotionEnabled   = false
        };

        foreach (var node in nodes)
        {
            menu.Items.Add(node);
        }

        return menu;
    }

    private static void ShowInWindow(Control content, Action assertion)
    {
        var window = new Avalonia.Controls.Window
        {
            Width   = 320,
            Height  = 240,
            Content = content
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            assertion();
        }
        finally
        {
            window.Close();
        }
    }

    private sealed class ObservableTooltipNode : INavMenuNode, INotifyPropertyChanged
    {
        private object? _header;
        private object? _tooltip;
        private bool _isTooltipEnabled = true;

        public ObservableTooltipNode(object? header)
        {
            _header = header;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public ITreeNode<INavMenuNode>? ParentNode { get; private set; }
        public object? Header => _header;
        public IDataTemplate? HeaderTemplate => null;
        public object? Tooltip => _tooltip;
        public bool IsTooltipEnabled => _isTooltipEnabled;
        public EntityKey? ItemKey => null;
        public PathIcon? Icon => null;
        public bool IsEnabled => true;
        public IEnumerable<INavMenuNode> Children => [];
        public IEnumerable<INavMenuEntry> Entries => [];

        public void UpdateParentNode(INavMenuNode? parentNode)
        {
            ParentNode = parentNode;
        }

        public void SetHeader(object? value)
        {
            _header = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Header)));
        }

        public void SetTooltip(object? value)
        {
            _tooltip = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Tooltip)));
        }

        public void SetTooltipEnabled(bool value)
        {
            _isTooltipEnabled = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsTooltipEnabled)));
        }
    }
}
