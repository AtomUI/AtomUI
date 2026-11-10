using AtomUI.Controls;
using AtomUI.Controls.Data;
using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Icons.AntDesign;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;

namespace AtomUI.Desktop.Controls.Rendering.Tests.Effect.Families;

public partial class DisplayFamilyEffectTests
{
    private static readonly string[] ListViewOwnerReads = ["BorderRadius", "BorderRadiusLG", "BorderRadiusSM", "BorderThickness", "ColorSplit", "EnableMotion", "Padding", "PaddingXL"];
    private static readonly string[] ListItemOwnerReads = ["ColorPrimary", "ColorSplit", "ControlHeight", "ControlHeightLG", "ControlHeightSM", "FontSize", "FontSizeSM", "IconSize", "LineWidth"];
    private static readonly string[] TreeViewOwnerReads = ["ColorPrimary", "EnableMotion", "MotionDurationSlow", "PaddingXL"];
    private static readonly string[] TreeItemOwnerReads = ["BorderThickness", "ColorBorder"];
    private static readonly string[] TreeHeaderOwnerReads = ["BorderRadius", "ColorBgContainerDisabled", "ColorPrimary", "ColorText", "ColorTextDisabled", "IconSize"];
    private static readonly string[] TreeSwitcherOwnerReads = ["BorderRadius", "ColorPrimary", "ColorText", "ColorTextDisabled", "IconSize", "IconSizeXS"];

    [Fact]
    public void Transfer_Inherited_Owner_Reads_Match_Their_Actual_Themes()
    {
        foreach (var (path, tokens) in new (string, string[])[]
        {
            ("ListView/Themes/ListViewTheme.axaml", ListViewOwnerReads),
            ("ListView/Themes/ListViewItemTheme.axaml", ListItemOwnerReads),
            ("TreeView/Themes/TreeViewTheme.axaml", TreeViewOwnerReads),
            ("TreeView/Themes/TreeViewItemTheme.axaml", TreeItemOwnerReads),
            ("TreeView/Themes/TreeViewItemHeaderTheme.axaml", TreeHeaderOwnerReads),
            ("TreeView/Themes/NodeSwitcherButtonTheme.axaml", TreeSwitcherOwnerReads)
        })
        {
            ThemeOwnerReads.Read("src/AtomUI.Desktop.Controls/" + path).Order().ShouldBe(tokens.Order());
        }
    }

    // The TransferListItem template replaces the split-line / selected-icon visuals. ColorSplit, LineWidth,
    // ColorPrimary and IconSize of that base template therefore have no Transfer visual consumer. The outer list's
    // BorderThickness is deliberately zero. These suppressed inherited defaults are guarded by the contract below.
    public static IEnumerable<object[]> TransferListOwnerRows() => new[]
    {
        "BorderRadius", "BorderRadiusLG", "BorderRadiusSM", "EnableMotion", "Padding", "PaddingXL",
        "ControlHeight", "ControlHeightLG", "ControlHeightSM", "FontSize", "FontSizeSM"
    }.Select(token => new object[] { token });

    [Theory, MemberData(nameof(TransferListOwnerRows))]
    public void TransferListView_Inherited_Global_Has_Effect(string token)
    {
        Run(new Scenario(() =>
        {
            if (token is "FontSize" or "FontSizeSM")
            {
                var host = (ListTransfer)CreateTransfer(grouped: true);
                host.SizeType = token == "FontSize" ? CustomizableSizeType.Large : CustomizableSizeType.Middle;
                ((TransferListView)host.SourceView!).SizeType = host.SizeType;
                return host;
            }
            var view = new TransferListView
            {
                IsGroupEnabled = token is "FontSize" or "FontSizeSM",
                Width = 220, Height = token is "Padding" or "PaddingXL" ? double.NaN : 180, Background = Brushes.LightGray,
                SizeType = token switch
                {
                    "BorderRadiusLG" or "PaddingXL" or "ControlHeightLG" or "FontSize" => CustomizableSizeType.Large,
                    "BorderRadiusSM" or "ControlHeightSM" => CustomizableSizeType.Small,
                    _ => CustomizableSizeType.Middle
                },
                ItemsSource = token is "Padding" or "PaddingXL" ? Array.Empty<IListItemData>() :
                new IListItemData[] {new ListItemData { ItemKey = "record1", Content = "First record", Group = "Group" },
                 new ListItemData { ItemKey = "record2", Content = "Second record", Group = "Group" } }
            };
            return view;
        }, c =>
        {
            if (token is "FontSize" or "FontSizeSM")
            {
                c.GetVisualDescendants().OfType<TransferListItem>().Any(item => item.IsGroupItem).ShouldBeTrue();
            }
        }, new EffectOptions
        {
            Target = static c => c is TransferListView ? c : Part<TransferListView>(c),
            Perturb = token.StartsWith("BorderRadius", StringComparison.Ordinal) ? static _ => new CornerRadius(40) : null
        }),
            TransferListViewTokens.Identity, token);
    }

    public static IEnumerable<object[]> TransferTreeOwnerRows() => new[]
    {
        "EnableMotion", "MotionDurationSlow", "PaddingXL", "BorderRadius", "ColorBgContainerDisabled", "ColorPrimary", "ColorText", "ColorTextDisabled", "IconSize", "IconSizeXS"
    }.Select(token => new object[] { token });

    [Theory, MemberData(nameof(TransferTreeOwnerRows))]
    public void TransferTreeView_Inherited_Global_Has_Effect(string token)
    {
        Run(new Scenario(() => new TransferTreeView
        {
            Width = 250, Height = token == "PaddingXL" ? double.NaN : 180, Background = Brushes.LightGray, IsShowIcon = true, IsShowLeafIcon = true,
            ItemsSource = token == "PaddingXL" ? Array.Empty<TreeItemNode>() :
            new TreeItemNode[] {new TreeItemNode { ItemKey = "parent", Header = "Parent", Icon = new FolderOutlined(), Children = [new TreeItemNode { ItemKey = "child", Header = "Child", IsLeaf = true }] } }
        }, c =>
        {
            if (token == "PaddingXL")
            {
                return;
            }
            var item = Part<TransferTreeViewItem>(c);
            var header = Part<TransferTreeViewItemHeader>(item);
            switch (token)
            {
                case "ColorPrimary": item.IsLoading = true; break;
                case "ColorTextDisabled": item.IsEnabled = false; break;
                case "ColorBgContainerDisabled": item.IsMasked = true; item.IsSelected = true; break;
                case "BorderRadius": header.IsHover = true; break;
            }
        }, new EffectOptions
        {
            Region = token is "ColorPrimary" or "IconSizeXS" ? static c => Part<NodeSwitcherButton>(c) :
                token is "BorderRadius" or "ColorBgContainerDisabled" or "ColorTextDisabled" or "ColorText" or "IconSize" ? static c => Part<TransferTreeViewItemHeader>(c) : null,
            Perturb = token == "BorderRadius" ? static _ => new CornerRadius(20) : null
        }), TransferTreeViewTokens.Identity, token);
    }

    [Fact]
    public void Transfer_Suppressed_Inherited_Borders_And_Selected_Icon_Stay_Unrendered()
    {
        FeedbackFamilyEffectTests.WithProvider(CreateTransfer(), _ =>
        {
            var view = Part<TransferListView>(_.Child!);
            view.BorderThickness.ShouldBe(default);
            var item = Part<TransferListItem>(view);
            item.GetVisualDescendants().OfType<Control>().ShouldNotContain(c => c.Name == "SplitLineFrame");
            item.GetVisualDescendants().OfType<IconTemplatePresenter>().ShouldBeEmpty();
            Part<CheckBox>(item).ShouldNotBeNull();
        });
        FeedbackFamilyEffectTests.WithProvider(CreateTransfer(tree: true), provider =>
        {
            var item = Part<TransferTreeViewItem>(provider.Child!);
            var header = Part<TransferTreeViewItemHeader>(item);
            var frame = Part<Control>(header, "Frame");
            // The reused base item border does not feed this replacement header frame.
            ((ISolidColorBrush)header.BorderBrush!).Color.A.ShouldBe((byte)0);
            ((AtomUI.Controls.Primitives.PixelAlignedBorder)frame).BorderBrush.ShouldBeNull();
        });
    }
}
