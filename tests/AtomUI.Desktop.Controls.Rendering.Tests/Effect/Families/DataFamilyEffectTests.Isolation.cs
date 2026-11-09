using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Theme;
using AtomUI.Theme.Configuration;
using AtomUI.Theme.Schema;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.VisualTree;
using Shouldly;
using SkiaSharp;
using Xunit;
using AvaloniaBorder = Avalonia.Controls.Border;
using AvaloniaTextBlock = Avalonia.Controls.TextBlock;

namespace AtomUI.Desktop.Controls.Rendering.Tests.Effect.Families;

/// <summary>
/// Scope isolation of the data family (theme-effect-tests.md, family gate): a <c>Controls[X]</c> override reaches X only, the
/// root Token reaches X, the Token-owner transparent items (ListBoxItem, ListViewItem, TreeViewItem) and the Skeleton follow
/// their host, and registered controls nested in a member keep their own identity.
/// </summary>
public partial class DataFamilyEffectTests
{
    private const string IsolationColor = "#ff00b96b";

    private static ThemeConfig IsolationConfig(
        (ControlTokenIdentity Identity, string Token, string Value)? control = null,
        (string Token, string Value)? root = null) =>
        FeedbackFamilyEffectTests.IsolationConfig(control, root);

    private static void WithIsolationWindow(Control content, Action<ThemeConfigProvider, Func<SKBitmap>> body) =>
        FeedbackFamilyEffectTests.WithIsolationWindow(content, body);

    private static double ChangedRatioOf(SKBitmap before, SKBitmap after, Control control) =>
        FeedbackFamilyEffectTests.ChangedRatioOf(before, after, control);

    private static void AssertReachesOnly(
        ThemeConfigProvider provider,
        Func<SKBitmap> capture,
        (ControlTokenIdentity Identity, string Token, Control Control)[] members,
        string value = IsolationColor)
    {
        foreach (var (identity, token, target) in members)
        {
            provider.Config = IsolationConfig();
            using var before = capture();
            provider.Config = IsolationConfig(control: (identity, token, value));
            using var after = capture();
            foreach (var (_, _, control) in members)
            {
                var ratio = ChangedRatioOf(before, after, control);
                if (ReferenceEquals(control, target))
                {
                    ratio.ShouldBeGreaterThan(0d, $"Controls[{identity}].{token} did not reach {control.GetType().Name}.");
                }
                else
                {
                    ratio.ShouldBe(0d, $"Controls[{identity}].{token} leaked into {control.GetType().Name}.");
                }
            }

            provider.Config = IsolationConfig(root: (token, value));
            using var byRoot = capture();
            ChangedRatioOf(before, byRoot, target).ShouldBeGreaterThan(0d, $"Root {token} did not reach {target.GetType().Name}.");
        }
    }

    [Fact]
    public void Controls_X_Colours_Change_Only_X_For_Lists_Trees_And_Skeleton()
    {
        var listBox  = CreateListBox();
        var listView = CreateListView();
        var tree     = CreateTreeView();
        var skeleton = CreateSkeleton();
        skeleton.Width = 260;
        var left  = new StackPanel { Spacing = 12, Children = { listBox, listView } };
        var right = new StackPanel { Spacing = 12, Children = { tree, skeleton } };
        var root  = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 16, Children = { left, right } };
        WithIsolationWindow(root, (provider, capture) =>
        {
            AssertReachesOnly(provider, capture,
            [
                (ListBoxTokens.Identity, "ColorSplit", listBox),
                (ListViewTokens.Identity, "ColorSplit", listView),
                (TreeViewTokens.Identity, "ColorText", tree),
                (SkeletonTokens.Identity, "ColorFillContent", skeleton)
            ]);
        });
    }

    [Fact]
    public void List_And_Tree_Items_Follow_Their_Host_And_Nested_Controls_Keep_Their_Identity()
    {
        var listBox  = CreateListBox(showIndicator: true);
        var listView = CreateListView();
        var tree     = CreateTreeView(toggle: AtomUI.Controls.ItemToggleType.CheckBox);
        var root = new StackPanel { Spacing = 12, Children = { listBox, listView, tree } };
        WithIsolationWindow(root, (provider, capture) =>
        {
            listBox.SelectedIndex = 1;
            var boxItem  = ListBoxItemAt(listBox, 1);
            var viewItem = ListViewItemAt(listView, 0);
            var treeItem = ItemOf(tree, "Leaf one");
            var header   = HeaderOf(tree, "Leaf one");
            var switcher = Descendant<NodeSwitcherButton>(HeaderOf(tree, "Parent node"));
            var checkBox = Descendant<CheckBox>(header);
            ThemeScope.GetTokenOwner(boxItem).ShouldBe(ListBoxTokens.Identity);
            ThemeScope.GetTokenOwner(viewItem).ShouldBe(ListViewTokens.Identity);
            ThemeScope.GetTokenOwner(treeItem).ShouldBe(TreeViewTokens.Identity);
            ThemeScope.GetTokenOwner(header).ShouldBe(TreeViewTokens.Identity);
            ThemeScope.GetTokenOwner(switcher).ShouldBe(TreeViewTokens.Identity);
            ThemeScope.GetTokenOwner(checkBox).ShouldBe(CheckBoxTokens.Identity);
            using var before = capture();

            // The items are transparent: Controls[ListBoxItem] reaches nothing, Controls[ListBox] reaches the check mark.
            provider.Config = IsolationConfig(control: (ListBoxItemTokens.Identity, "ColorPrimary", IsolationColor));
            using var byItem = capture();
            ChangedRatioOf(before, byItem, boxItem).ShouldBe(0d, "Controls[ListBoxItem] reached a transparent list item.");
            provider.Config = IsolationConfig(control: (ListBoxTokens.Identity, "ColorPrimary", IsolationColor));
            using var byListBox = capture();
            ChangedRatioOf(before, byListBox, boxItem).ShouldBeGreaterThan(0d, "Controls[ListBox] did not reach the item.");

            provider.Config = IsolationConfig(control: (ListViewItemTokens.Identity, "ControlHeight", "48"));
            using var byViewItem = capture();
            ChangedRatioOf(before, byViewItem, listView).ShouldBe(0d, "Controls[ListViewItem] reached a transparent list item.");
            provider.Config = IsolationConfig(control: (ListViewTokens.Identity, "ControlHeight", "48"));
            using var byListView = capture();
            ChangedRatioOf(before, byListView, listView).ShouldBeGreaterThan(0d, "Controls[ListView] did not reach the item.");

            // The node header and switcher read the tree's globals; the toggle check box keeps its identity.
            provider.Config = IsolationConfig(control: (TreeViewItemTokens.Identity, "ColorText", IsolationColor));
            using var byTreeItem = capture();
            ChangedRatioOf(before, byTreeItem, switcher).ShouldBe(0d, "Controls[TreeViewItem] reached a transparent tree item.");
            provider.Config = IsolationConfig(control: (TreeViewTokens.Identity, "ColorText", IsolationColor));
            using var byTree = capture();
            ChangedRatioOf(before, byTree, switcher).ShouldBeGreaterThan(0d, "Controls[TreeView] did not reach the switcher.");

            provider.Config = IsolationConfig(control: (TreeViewTokens.Identity, "ColorBorder", IsolationColor));
            using var byTreeBorder = capture();
            ChangedRatioOf(before, byTreeBorder, checkBox).ShouldBe(0d, "Controls[TreeView].ColorBorder reached the toggle CheckBox.");
            provider.Config = IsolationConfig(control: (CheckBoxTokens.Identity, "ColorBorder", IsolationColor));
            using var byCheckBox = capture();
            ChangedRatioOf(before, byCheckBox, checkBox).ShouldBeGreaterThan(0d, "Controls[CheckBox] did not reach the toggle.");
        });
    }

    [Fact]
    public void Skeleton_Is_Token_Owner_Transparent_For_The_Content_It_Wraps()
    {
        // Skeleton wraps loaded content; that content keeps the host's owner, so Controls[Host] reaches identity-less parts
        // inside it. The placeholder parts keep their own identity.
        var content = new AvaloniaBorder { Width = 40, Height = 20 };
        var host    = new Card { Width = 300, Header = "Card", Content = content };
        var loading = CreateSkeleton(avatar: true);
        var root    = new StackPanel { Spacing = 12, Children = { host, loading } };
        WithIsolationWindow(root, (_, _) =>
        {
            Descendant<Skeleton>(host);
            ThemeScope.GetTokenOwner(content).ShouldBe(CardTokens.Identity);
            ThemeScope.GetTokenOwner(loading).ShouldNotBe(SkeletonTokens.Identity);
            ThemeScope.GetTokenOwner(Descendant<SkeletonAvatar>(loading)).ShouldBe(SkeletonAvatarTokens.Identity);
            ThemeScope.GetTokenOwner(Descendant<SkeletonParagraph>(loading)).ShouldBe(SkeletonParagraphTokens.Identity);
        });
    }

    [Fact]
    public void Card_Grid_Items_In_The_Skeleton_Take_The_Card_Motion()
    {
        // Before Skeleton became owner transparent, the card body parts took the Skeleton as owner and their
        // TokenTransitions ignored Controls[Card]; now the grid item shadow transition takes the Card's duration.
        var grid = new CardGridContent
        {
            ColumnDefinitions = new ColumnDefinitions("*,*"),
            IsHoverable       = true
        };
        var item = new CardGridItem { Row = 0, Column = 0, Content = "Grid 1" };
        grid.Items.Add(item);
        var card = new Card { Width = 300, Header = "Card", Content = grid };
        WithProvider(card, provider =>
        {
            WaitFor(() => item.Transitions is { Count: > 0 });
            provider.Config = ControlConfig(CardTokens.Identity, "MotionDurationFast", "00:00:00.4000000");
            WaitFor(() => item.Transitions!.OfType<Avalonia.Animation.TransitionBase>()
                              .All(static transition => transition.Duration == TimeSpan.FromMilliseconds(400)));
            item.Transitions!.OfType<Avalonia.Animation.TransitionBase>()
                .ShouldAllBe(static transition => transition.Duration == TimeSpan.FromMilliseconds(400));
        });
    }

    [Fact]
    public void Dialog_Body_Content_Takes_The_Dialog_As_Owner()
    {
        // The dialog body sits in the surface's Skeleton; with Skeleton owner transparent the content inherits the Dialog.
        var anchor  = new AvaloniaBorder { Width = 40, Height = 20 };
        var content = new AvaloniaTextBlock { Text = "Some contents..." };
        var dialog  = new Dialog { Title = "Modal", Content = content, IsModal = false, PlacementTarget = anchor };
        var root = FeedbackFamilyEffectTests.OverlayLayerHost(new Canvas { Width = 600, Height = 400, Children = { anchor, dialog } },
            600, 400);
        WithIsolationWindow(root, (_, _) =>
        {
            dialog.IsOpen = true;
            FeedbackFamilyEffectTests.WaitForVisual(() => content.IsAttachedToVisualTree() ? content : null, "The dialog content");
            ThemeScope.GetTokenOwner(content).ShouldBe(DialogTokens.Identity);
            dialog.IsOpen = false;
        });
    }

    /// <summary>Every identity-less part read list of the family (a property: the lists live in several partial files).</summary>
    private static OwnerReadList[] s_allOwnerLists => [s_groupHeaderItem, s_treeViewItemHeader, s_nodeSwitcherButton];

    [Fact]
    public void Owner_Read_Lists_Match_The_Part_Themes()
    {
        // Drift guard: a new OwnerTokenResource read in an identity-less part theme must get an effect row through its host.
        foreach (var list in s_allOwnerLists)
        {
            ThemeOwnerReads.Read(list.Theme).Order(StringComparer.Ordinal)
                           .ShouldBe(list.Tokens.Order(StringComparer.Ordinal), list.Theme);
        }
    }
}
