using AtomUI.Controls;
using AtomUI.Controls.Data;
using AtomUI.Desktop.Controls.DesignTokens;
using Avalonia.Controls;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;

namespace AtomUI.Desktop.Controls.Rendering.Tests.Effect.Families;

public partial class DisplayFamilyEffectTests
{
    internal static AbstractTransfer CreateTransfer(bool tree = false, bool pagination = false, bool grouped = false)
    {
        AbstractTransfer transfer = tree ? new TreeTransfer() : new ListTransfer { PageSize = pagination ? 2 : 0, SourceView = grouped ? new TransferListView { IsGroupEnabled = true } : null };
        transfer.SourceTitle = "Source";
        transfer.TargetTitle = "Target";
        transfer.IsFilterEnabled = true;
        transfer.ItemsSource = tree
            ? Enumerable.Range(0, 6).Select(i => (IItemKey)new TreeItemNode { ItemKey = $"key{i}", Header = $"Record {i}", IsLeaf = true }).ToArray()
            : Enumerable.Range(0, 6).Select(i => (IItemKey)new ListItemData { ItemKey = $"key{i}", Content = $"Record {i}", Group = grouped ? "Records" : null }).ToArray();
        transfer.TargetKeys = ["key5"];
        return transfer;
    }

    public static IEnumerable<object[]> ListTransferRows() => Rows("ListTransfer");
    public static IEnumerable<object[]> TreeTransferRows() => Rows("TreeTransfer");
    public static IEnumerable<object[]> TransferListViewRows() => Rows("TransferListView");

    [Theory, MemberData(nameof(ListTransferRows))]
    public void ListTransfer_Token_Has_Effect(string token, bool own)
    {
        _ = own;
        Run(TransferScenario(token), ListTransferTokens.Identity, token);
    }

    [Theory, MemberData(nameof(TreeTransferRows))]
    public void TreeTransfer_Token_Has_Effect(string token, bool own)
    {
        _ = own;
        Run(TransferScenario(token, true), TreeTransferTokens.Identity, token);
    }

    private static Scenario TransferScenario(string token, bool tree = false) => new(
        () => CreateTransfer(tree, token == "ListWidthLG"),
        token switch
        {
            "ColorError" => static c => ((AbstractTransfer)c).Status = InputControlStatus.Error,
            "ColorWarning" => static c => ((AbstractTransfer)c).Status = InputControlStatus.Warning,
            "ColorTextDisabled" => static c => c.IsEnabled = false,
            _ => null
        }, new EffectOptions
        {
            Region = token switch
            {
                "ColorSplit" => static c => Part<Control>(Part<TransferItemDecorator>(c), "HeaderFrame"),
                "FontSizeIcon" => c => c.GetVisualDescendants().OfType<TransferItemDecorator>().Last(),
                "BorderRadiusLG" => static c => Part<TransferItemDecorator>(c),
                "MarginXS" or "ColorTextDescription" => static c => Part<LineEdit>(c, "FilterInput"),
                _ => null
            },
            Perturb = token == "BorderRadiusLG" ? static _ => new Avalonia.CornerRadius(30) : null
        });

    [Theory, MemberData(nameof(TransferListViewRows))]
    public void TransferListView_Token_Has_Effect(string token, bool own)
    {
        _ = own;
        Run(new Scenario(static () => CreateTransfer(pagination: true), null,
            new EffectOptions { Target = static c => Part<TransferListView>(c) }), TransferListViewTokens.Identity, token);
    }

    private static readonly string[] TransferOwnerTokens =
    [
        "EnableMotion", "SpacingXXS", "SpacingXS", "ColorTextDisabled", "ColorTextDescription", "ColorBorder",
        "BorderThickness", "BorderRadiusLG", "ColorSplit", "ColorBgContainer", "FontSizeIcon", "MarginXS", "ColorError", "ColorWarning"
    ];

    public static IEnumerable<object[]> TransferOwnerRows() => TransferOwnerTokens.SelectMany(token =>
        new[] { new object[] { false, token }, new object[] { true, token } });

    [Theory, MemberData(nameof(TransferOwnerRows))]
    public void Transfer_Primitive_Global_Uses_Host(bool tree, string token) =>
        Run(TransferScenario(token, tree), tree ? TreeTransferTokens.Identity : ListTransferTokens.Identity, token);

    [Fact]
    public void Transfer_Primitive_Read_List_Matches_Themes()
    {
        ThemeOwnerReads.Read(
            "src/AtomUI.Desktop.Controls/Transfer/Themes/AbstractTransferTheme.axaml",
            "src/AtomUI.Desktop.Controls/Transfer/Themes/TransferItemDecoratorTheme.axaml",
            "src/AtomUI.Desktop.Controls/Transfer/Themes/TransferSelectDropdownTheme.axaml")
            .Order().ShouldBe(TransferOwnerTokens.Order());
    }
}
