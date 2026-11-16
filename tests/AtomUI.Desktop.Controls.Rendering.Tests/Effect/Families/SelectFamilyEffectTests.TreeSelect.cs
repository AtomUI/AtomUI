using AtomUI.Icons.AntDesign;
using AtomUI.Controls;
using AtomUI.Desktop.Controls.DesignTokens;
using Avalonia.Controls;
using Avalonia.VisualTree;
using Xunit;

namespace AtomUI.Desktop.Controls.Rendering.Tests.Effect.Families;

/// <summary>
/// TreeSelect. Its drop-down hosts a tree view of the tree family (TreeView / TreeViewItem themes), whose Tokens belong to
/// that family; the TreeSelect rows cover its surface, popup frame and shared select parts.
/// </summary>
public partial class SelectFamilyEffectTests
{
    public static IEnumerable<object[]> TreeSelectRows() => RowsOf("TreeSelect");

    [Theory]
    [MemberData(nameof(TreeSelectRows))]
    public void TreeSelect_Token_Has_Effect(string token, bool isOwn)
    {
        _ = isOwn;
        Run(TreeSelectSetup(token), TreeSelectTokens.Identity, token);
    }

    private static TreeItemNode[] TreeNodes(int count)
    {
        var nodes = new List<TreeItemNode>
        {
            new() { Header = "Parent", Value = "parent", Children = [new TreeItemNode { Header = "Child", Value = "child" }] },
            new() { Header = "Leaf", Value = "leaf" },
            new() { Header = "Other", Value = "other" }
        };
        for (var index = nodes.Count; index < count; index++)
        {
            nodes.Add(new TreeItemNode { Header = $"Node {index}", Value = $"node{index}" });
        }

        return nodes.ToArray();
    }

    private static EffectSetup TreeSelectSetup(string token)
    {
        var b     = new SetupBuilder();
        var count = 3;
        var width = 240d;
        var size  = SizeOf(token);
        if (size != CustomizableSizeType.Middle)
        {
            b.Init.Add(c => ((TreeSelect)c).SizeType = size);
        }

        void Selected() => b.Init.Add(static c =>
        {
            var treeSelect = (TreeSelect)c;
            treeSelect.SelectedItem = ((TreeItemNode[])treeSelect.ItemsSource!)[1];
        });
        void Status(InputControlStatus status) => b.Init.Add(c => ((TreeSelect)c).Status = status);
        void Variant(InputControlStyleVariant variant) => b.Init.Add(c => ((TreeSelect)c).StyleVariant = variant);
        void LeftAddOn() => b.Init.Add(static c => ((TreeSelect)c).LeftAddOn = "http://");
        void Tags() => b.Init.Add(static c =>
        {
            var treeSelect = (TreeSelect)c;
            var nodes      = (TreeItemNode[])treeSelect.ItemsSource!;
            treeSelect.IsMultiple    = true;
            treeSelect.SelectedItems = [nodes[1], nodes[2]];
        });

        void EmptyMultiple() => b.Init.Add(static c =>
        {
            ((TreeSelect)c).IsMultiple = true;
            ((TreeSelect)c).PlaceholderText = "Placeholder";
        });
        var inputOwn = true;
        switch (token)
        {
            case "MultiModePadding" or "MultiModePaddingLG" or "MultiModePaddingSM" or "MultipleItemHeight"
                or "MultipleItemHeightLG" or "MultipleItemHeightSM" or "MultipleItemBg" or "ColorFillSecondary"
                or "UniformlyPaddingXXS" or "FontHeightLG" or "ControlHeightLG" or "ControlHeightSM":
                Tags();
                break;
            case "MultiModePrefixIndent" or "MultiModePrefixIndentLG" or "MultiModePrefixIndentSM":
                Tags();
                b.Init.Add(static c => ((TreeSelect)c).ContentLeftAddOn = "$");
                break;
            case "MultipleItemColorDisabled" or "MultipleSelectorBgDisabled":
                Tags();
                b.Disabled();
                break;
            case "SingleModePadding" or "SingleModePaddingLG" or "SingleModePaddingSM" or "LineWidth"
                or "ControlPaddingHorizontalSM" or "UniformlyPaddingSM":
                EmptyMultiple();
                break;
            case "ControlPaddingHorizontal":
                b.Init.Add(static c => ((TreeSelect)c).SizeType = CustomizableSizeType.Large);
                EmptyMultiple();
                break;
            case "OptionFontSize":
                Tags();
                break;
            default:
                inputOwn = false;
                break;
        }

        if (!inputOwn && !FrameState(b, token, Status, Variant, LeftAddOn, Selected))
        {
            switch (token)
            {
                case "HeaderHeight" or "TreeItemMargin" or "TreeItemHeaderPadding" or "TreeItemHeaderMargin":
                    b.Popup = true;
                    b.Init.Add(static c => ((TreeSelect)c).IsDefaultExpandAll = true);
                    break;
                case "TreeNodeSwitcherMargin":
                    b.Popup = true;
                    b.Init.Add(static c => ((TreeSelect)c).IsTreeCheckable = true);
                    break;
                case "TreeNodeIconMargin":
                    b.Popup = true;
                    b.Init.Add(static c =>
                    {
                        ((TreeSelect)c).IsShowIcon = true;
                        var nodes = (TreeItemNode[])((TreeSelect)c).ItemsSource!;
                        nodes[0] = nodes[0] with { Icon = new FolderOutlined() };
                    });
                    break;
                case "NodeHoverBg" or "NodeSelectedBg" or "NodeSelectedColor":
                    b.Popup = true;
                    b.Arrange.Add(c =>
                    {
                        var item = TopLevel.GetTopLevel(c)!.GetVisualDescendants().OfType<TreeViewItem>().First();
                        if (token != "NodeHoverBg")
                        {
                            item.IsSelected = true;
                        }
                        else
                        {
                            item.GetVisualDescendants().OfType<TreeViewItemHeader>().First().IsHover = true;
                        }
                    });
                    break;
                case "FilterHighlightColor":
                    b.Popup = true;
                    b.Init.Add(static c => ((TreeSelect)c).IsFilterEnabled = true);
                    b.Arrange.Add(static c => ((TreeSelect)c).FilterValue = "Leaf");
                    break;
                case "ColorBgElevated" or "PopupContentPadding":
                    b.Popup = true;
                    break;
                case "MinPopupWidth":
                    // The popup frame is at least MinPopupWidth wide: it shows for a narrow TreeSelect.
                    width   = 80;
                    b.Popup = true;
                    break;
                case "ControlHeight":
                    // The item height only bounds the drop-down height (item height x page size).
                    count   = 24;
                    b.Init.Add(static c => ((TreeSelect)c).IsDefaultExpandAll = true);
                    b.Popup = true;
                    break;
                case "SpacingXXS":
                    Tags();
                    break;
                case "ColorText" or "FontSize" or "FontSizeLG":
                    Selected();
                    break;
                case "InputContentMargin":
                    b.Init.Add(static c => ((TreeSelect)c).SizeType = CustomizableSizeType.Small);
                    Selected();
                    break;
                case "ColorFillSecondary":
                    Variant(InputControlStyleVariant.Filled);
                    b.Hover();
                    break;
                case "SelectionBackground" or "SelectionForeground":
                    b.Init.Add(static c => ((TreeSelect)c).IsFilterEnabled = true);
                    // The filter input is read-only until the drop-down opens; compare the input, not the drop-down.
                    b.Popup  = true;
                    b.Region = static c => c;
                    SelectFilterText(b);
                    break;
                case "ColorTextPlaceholder":
                    b.Init.Add(static c => ((TreeSelect)c).PlaceholderText = "Placeholder");
                    break;
                case "ColorTextDescription" or "SpacingXS":
                    Tags();
                    b.Init.Add(static c =>
                    {
                        ((TreeSelect)c).MaxCount                = 3;
                        ((TreeSelect)c).IsShowMaxCountIndicator = true;
                    });
                    break;
                case "ColorTextQuaternary" or "IconSize":
                    b.Region = static c => c.GetVisualDescendants().OfType<SelectHandle>().First();
                    break;
                case "ColorTextTertiary":
                    Selected();
                    b.Init.Add(static c => ((TreeSelect)c).IsAllowClear = true);
                    b.Hover();
                    b.OnPart<InputClearIconButton>("PART_ClearButton", ":pointerover");
                    break;
            }
        }

        var nodeCount = count;
        var treeWidth = width;
        return b.Build(
            () => new TreeSelect { Width = treeWidth, ItemsSource = TreeNodes(nodeCount) },
            static root => root.GetSelfAndVisualDescendants().OfType<TreeSelect>().First());
    }
}
