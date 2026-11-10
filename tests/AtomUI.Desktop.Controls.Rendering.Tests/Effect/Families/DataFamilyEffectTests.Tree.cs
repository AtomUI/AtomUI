using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Icons.AntDesign;
using AtomUI.Controls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;

namespace AtomUI.Desktop.Controls.Rendering.Tests.Effect.Families;

/// <summary>
/// TreeView: TreeViewItem is Token-owner transparent, and its identity-less node header and switcher read the tree's globals
/// through <c>OwnerTokenResource</c> (they are shared with TreeSelect and the transfer tree); every read runs as a TreeView
/// row.
/// </summary>
public partial class DataFamilyEffectTests
{
    private const string TreeViewThemes = "src/AtomUI.Desktop.Controls/TreeView/Themes/";

    internal static readonly OwnerReadList s_treeViewItemHeader = new(TreeViewThemes + "TreeViewItemHeaderTheme.axaml",
        ["BorderRadius", "ColorBgContainerDisabled", "ColorPrimary", "ColorText", "ColorTextDisabled", "IconSize"]);

    internal static readonly OwnerReadList s_nodeSwitcherButton = new(TreeViewThemes + "NodeSwitcherButtonTheme.axaml",
        ["BorderRadius", "ColorPrimary", "ColorText", "ColorTextDisabled", "IconSize", "IconSizeXS"]);

    /// <summary>
    /// A tree with an expanded parent (with icon), a selected child, a hovered-able child and a disabled selected child.
    /// </summary>
    internal static TreeView CreateTreeView(
        bool showLine = false,
        bool rotation = true,
        ItemToggleType toggle = ItemToggleType.None,
        bool empty = false, bool transfer = false)
    {
        TreeView tree = transfer ? new TransferTreeView() : new TreeView();
        tree.Width = 280;
        tree.IsShowLine = showLine;
        tree.IsShowIcon = true;
        tree.IsSwitcherRotation = rotation;
        tree.ToggleType = toggle;
        if (!empty && transfer)
        {
            tree.ItemsSource = new[]
            {
                new TreeItemNode
                {
                    Header = "Parent node", ItemKey = "parent", IsExpanded = true, Icon = new FolderOutlined(),
                    Children =
                    {
                        new TreeItemNode { Header = "Leaf one", ItemKey = "leaf1", IsLeaf = true, Icon = new FileOutlined() },
                        new TreeItemNode { Header = "Leaf two", ItemKey = "leaf2", IsLeaf = true, Icon = new FileOutlined() },
                        new TreeItemNode { Header = "Disabled leaf", ItemKey = "disabled", IsLeaf = true, Icon = new FileOutlined(), IsEnabled = false }
                    }
                },
                new TreeItemNode
                {
                    Header = "Second parent", ItemKey = "second", Icon = new FolderOutlined(),
                    Children = { new TreeItemNode { Header = "Hidden leaf", ItemKey = "hidden", IsLeaf = true } }
                }
            };
        }
        else if (!empty)
        {
            var parent = new TreeViewItem { Header = "Parent node", IsExpanded = true, Icon = new FolderOutlined() };
            parent.Items.Add(new TreeViewItem { Header = "Leaf one", Icon = new FileOutlined() });
            parent.Items.Add(new TreeViewItem { Header = "Leaf two", Icon = new FileOutlined() });
            parent.Items.Add(new TreeViewItem { Header = "Disabled leaf", Icon = new FileOutlined(), IsEnabled = false });
            var second = new TreeViewItem { Header = "Second parent", Icon = new FolderOutlined() };
            second.Items.Add(new TreeViewItem { Header = "Hidden leaf" });
            tree.Items.Add(parent);
            tree.Items.Add(second);
        }
        else
        {
            tree.IsShowEmptyIndicator = true;
        }

        return tree;
    }

    private static TreeViewItemHeader HeaderOf(Control tree, string header) =>
        tree.GetVisualDescendants().OfType<TreeViewItemHeader>().First(item => Equals(item.Content is ITreeItemNode node ? node.Header : item.Content, header));

    private static TreeViewItem ItemOf(Control tree, string header) =>
        tree.GetVisualDescendants().OfType<TreeViewItem>().First(item => Equals(item.Header is ITreeItemNode node ? node.Header : item.Header, header));

    private static void Select(Control tree, string header) => ItemOf(tree, header).IsSelected = true;

    public static IEnumerable<object[]> TreeViewRows() =>
        HostRows("TreeView", ["TreeViewItem"], [s_treeViewItemHeader, s_nodeSwitcherButton]);

    [Theory]
    [MemberData(nameof(TreeViewRows))]
    public void TreeView_Token_Has_Effect(string token, bool isOwn)
    {
        _ = isOwn;
        var scenario = TreeViewScenario(token);
        Run(scenario.Create, scenario.Arrange, TreeViewTokens.Identity, token, scenario.Options);
    }

    public static IEnumerable<object[]> TransferTreeViewRows() =>
        HostRows("TransferTreeView", ["TreeViewItem"], [s_treeViewItemHeader, s_nodeSwitcherButton]);

    [Theory]
    [MemberData(nameof(TransferTreeViewRows))]
    public void TransferTreeView_Token_Has_Effect(string token, bool isOwn)
    {
        _ = isOwn;
        var scenario = TreeViewScenario(token, transfer: true);
        Run(scenario.Create, scenario.Arrange, TransferTreeViewTokens.Identity, token, scenario.Options);
    }

    private static Scenario TreeViewScenario(string token, bool transfer = false)
    {
        TreeView CreateTreeView(bool showLine = false, bool rotation = true, ItemToggleType toggle = ItemToggleType.None,
            bool empty = false) => DataFamilyEffectTests.CreateTreeView(showLine, rotation, toggle, empty, transfer);
        switch (token)
        {
            // Hovered node content background.
            case "NodeHoverBg" or "ControlItemBgHover":
                return new Scenario(() => CreateTreeView(), static tree => HeaderOf(tree, "Leaf one").IsHover = true);
            // Selected node background (rounded by BorderRadius).
            case "NodeSelectedBg" or "ControlItemBgActive":
                return new Scenario(() => CreateTreeView(), static tree => Select(tree, "Leaf one"));
            case "BorderRadius":
                return new Scenario(() => CreateTreeView(), static tree => Select(tree, "Leaf one"),
                    new EffectOptions
                    {
                        Perturb = static _ => new CornerRadius(12),
                        Region  = static tree => HeaderOf(tree, "Leaf one")
                    });
            // A disabled selected node: disabled background, text and icon.
            case "ColorBgContainerDisabled" or "ColorTextDisabled":
                return new Scenario(() => CreateTreeView(), static tree => Select(tree, "Disabled leaf"),
                    new EffectOptions { Region = static tree => HeaderOf(tree, "Disabled leaf") });
            // Filtering highlights matched words.
            case "FilterHighlightColor" or "ColorError":
                return new Scenario(() => CreateTreeView(), static tree => ((TreeView)tree).FilterValue = "leaf",
                    // The original disabled label contains the actual split/highlighted "leaf" run.
                    new EffectOptions { Region = static tree => HeaderOf(tree, "Disabled leaf") });
            // Tree lines: drawn with the item's border brush and thickness.
            case "ColorBorder" or "BorderThickness":
                return new Scenario(() => CreateTreeView(showLine: true), null,
                    new EffectOptions { Region = static tree => ItemOf(tree, "Parent node") });
            // The switcher margin only applies with a check box / radio toggle.
            case "TreeNodeSwitcherMargin":
                return new Scenario(() => CreateTreeView(toggle: ItemToggleType.CheckBox));
            // The default (non-rotating) switcher icons use IconSize; the rotating caret IconSizeXS.
            case "IconSize":
                return new Scenario(() => CreateTreeView(rotation: false));
            // The rotating caret of the expanded parent.
            case "IconSizeXS":
                return new Scenario(() => CreateTreeView(), null,
                    new EffectOptions { Region = static tree => Descendant<NodeSwitcherButton>(HeaderOf(tree, "Parent node")) });
            // The drag indicator line (and the dragged node's border) during a drag.
            case "DragIndicatorLineWidth" or "LineWidthFocus" or "ColorPrimary":
                return new Scenario(() => CreateTreeView(), static tree => ShowDragIndicator((TreeView)tree));
            case "PaddingXL":
                return new Scenario(() => CreateTreeView(empty: true));
            default:
                // HeaderHeight / ControlHeightSM, the header margins and paddings, TreeItemMargin, TreeNodeIconMargin,
                // UniformlyPadding*, UniformlyMarginXXS, ColorText (node and switcher icons) / IconSizeXS (switcher), EnableMotion,
                // MotionDurationSlow.
                return new Scenario(() => CreateTreeView());
        }
    }

    /// <summary>Puts the tree into a drag with an insertion line below the first leaf and the leaf marked as dragged.</summary>
    private static void ShowDragIndicator(TreeView tree)
    {
        var leaf   = ItemOf(tree, "Leaf one");
        var header = HeaderOf(tree, "Leaf one");
        var bounds = header.Bounds;
        var bottom = header.TranslatePoint(new Point(0, bounds.Height), tree).ShouldNotBeNull();
        tree.IsDragging = true;
        leaf.IsDragging = true;
        tree.DragIndicatorRenderInfo = new DragIndicatorRenderInfo
        {
            TargetTreeItem = leaf,
            StartPoint     = new Point(8, bottom.Y),
            EndPoint       = new Point(tree.Bounds.Width - 8, bottom.Y)
        };
    }

    [Fact]
    public void Tree_Loading_Switcher_And_Motion_Follow_The_Tree()
    {
        // The loading switcher icon is ColorPrimary; motion and its duration are the tree's (read-back rows).
        var tree = CreateTreeView();
        WithProvider(tree, provider =>
        {
            var item = ItemOf(tree, "Second parent");
            tree.MotionDuration.ShouldBe(TimeSpan.FromMilliseconds(300));
            item.IsLoading = true;
            WaitFor(() => HeaderOf(tree, "Second parent").GetVisualDescendants().OfType<NodeSwitcherButton>().First()
                                                       .IconMode == NodeSwitcherButtonIconMode.Loading);
            provider.Config = ControlConfig(TreeViewTokens.Identity, "MotionDurationSlow", "00:00:00.6000000");
            WaitFor(() => tree.MotionDuration == TimeSpan.FromMilliseconds(600));
            tree.MotionDuration.ShouldBe(TimeSpan.FromMilliseconds(600));
            provider.Config = new AtomUI.Theme.Configuration.ThemeConfigBuilder()
                              .WithControl(TreeViewTokens.Identity, new AtomUI.Theme.Configuration.ControlThemeConfigBuilder()
                                                                     .WithToken("ColorPrimary", "#ff00b96b")
                                                                     .WithToken("EnableMotion", "false").Build())
                              .Build();
            WaitFor(() => !tree.IsMotionEnabled);
            tree.IsMotionEnabled.ShouldBeFalse();
            var switcher = HeaderOf(tree, "Second parent").GetVisualDescendants().OfType<NodeSwitcherButton>().First();
            var presenter = switcher.GetVisualDescendants().OfType<IconPresenter>().First();
            ((Avalonia.Media.ISolidColorBrush)presenter.IconBrush!).Color.ShouldBe(Avalonia.Media.Color.Parse("#ff00b96b"));
        });
    }
}
