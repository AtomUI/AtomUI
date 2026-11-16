using AtomUI.Controls;
using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Theme;
using AtomUI.Theme.Configuration;
using AtomUI.Theme.Resources;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AvaloniaWindow = Avalonia.Controls.Window;
using AtomTreeView = AtomUI.Desktop.Controls.TreeView;
using AtomTreeViewItem = AtomUI.Desktop.Controls.TreeViewItem;
using AtomToggleSwitch = AtomUI.Desktop.Controls.ToggleSwitch;
using AtomSelect = AtomUI.Desktop.Controls.Select;
using AtomCascader = AtomUI.Desktop.Controls.Cascader;
using AtomComboBox = AtomUI.Desktop.Controls.ComboBox;
using AtomListBox = AtomUI.Desktop.Controls.ListBox;
using AtomListView = AtomUI.Desktop.Controls.ListView;
using AtomTreeSelect = AtomUI.Desktop.Controls.TreeSelect;

namespace AtomUI.Desktop.Controls.Tests.Theme;

/// <summary>
/// The selected-state Own tokens a theme with a solid highlight needs: TreeView <c>NodeSelectedColor</c> and
/// ToggleSwitch <c>HandleCheckedBg</c>. Their defaults reproduce the previous rendering exactly.
/// </summary>
public class SelectedStateTokenTests
{
    static SelectedStateTokenTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public async Task ToggleSwitch_HandleCheckedBg_Defaults_To_HandleBg_And_Paints_Only_The_Checked_Handle()
    {
        var manager = GetThemeManager();
        await Apply(manager, null);
        var on  = new AtomToggleSwitch { IsChecked = true };
        var off = new AtomToggleSwitch { IsChecked = false };
        await Show(new StackPanel { Children = { on, off } }, async () =>
        {
            var tokens = ControlTokenAccessor.Capture(on);
            var handle = tokens.GetOwn<Color>(ToggleSwitchTokenKind.HandleBg);
            tokens.GetOwn<Color>(ToggleSwitchTokenKind.HandleCheckedBg).ShouldBe(handle);
            KnobColor(on).ShouldBe(handle);
            KnobColor(off).ShouldBe(handle);

            var control = new ControlThemeConfigBuilder().WithToken("HandleCheckedBg", "#FF123456").Build();
            await Apply(manager, new ThemeConfigBuilder().WithControl(ToggleSwitchTokens.Identity, control).Build());
            Dispatcher.UIThread.RunJobs();
            KnobColor(on).ShouldBe(Color.Parse("#FF123456"));
            KnobColor(off).ShouldBe(handle);

            // An explicit handle background still colours the checked handle unless HandleCheckedBg is set too.
            control = new ControlThemeConfigBuilder().WithToken("HandleBg", "#FF654321").Build();
            await Apply(manager, new ThemeConfigBuilder().WithControl(ToggleSwitchTokens.Identity, control).Build());
            Dispatcher.UIThread.RunJobs();
            ControlTokenAccessor.Capture(on).GetOwn<Color>(ToggleSwitchTokenKind.HandleCheckedBg)
                                .ShouldBe(Color.Parse("#FF654321"));
            KnobColor(on).ShouldBe(Color.Parse("#FF654321"));
        });
        await Apply(manager, null);
    }

    [Fact]
    public async Task TreeView_NodeSelectedColor_Defaults_To_ColorText_And_Colours_Only_The_Selected_Node()
    {
        var manager = GetThemeManager();
        await Apply(manager, null);
        var tree = new AtomTreeView();
        tree.Items.Add(new AtomTreeViewItem { Header = "Selected", IsSelected = true });
        tree.Items.Add(new AtomTreeViewItem { Header = "Other" });
        await Show(tree, async () =>
        {
            var tokens = ControlTokenAccessor.Capture(tree);
            var text = tokens.GetEffectiveGlobal<Color>(SharedTokenKind.ColorText);
            tokens.GetOwn<Color>(TreeViewTokenKind.NodeSelectedColor).ShouldBe(text);
            HeaderColor(tree, "Selected").ShouldBe(text);

            var control = new ControlThemeConfigBuilder().WithToken("NodeSelectedColor", "#FF123456").Build();
            await Apply(manager, new ThemeConfigBuilder().WithControl(TreeViewTokens.Identity, control).Build());
            Dispatcher.UIThread.RunJobs();
            HeaderColor(tree, "Selected").ShouldBe(Color.Parse("#FF123456"));
            HeaderColor(tree, "Other").ShouldNotBe(Color.Parse("#FF123456"));
        });
        await Apply(manager, null);
    }

    [Fact]
    public async Task Global_ControlItemColorActive_Defaults_To_ColorText_And_Drives_The_Selected_Text_Tokens()
    {
        var manager = GetThemeManager();
        await Apply(manager, null);
        var select   = new AtomSelect();
        var cascader = new AtomCascader();
        var combo    = new AtomComboBox();
        var list     = new AtomListBox();
        var listView = new AtomListView();
        var tree     = new AtomTreeView();
        var treeSelect = new AtomTreeSelect();
        await Show(new StackPanel { Children = { select, cascader, combo, list, listView, tree, treeSelect } }, async () =>
        {
            var text = ControlTokenAccessor.Capture(list).GetEffectiveGlobal<Color>(SharedTokenKind.ColorText);
            ControlTokenAccessor.Capture(list).GetEffectiveGlobal<Color>(SharedTokenKind.ControlItemColorActive).ShouldBe(text);
            AssertSelectedText(text);

            // One global value reaches every selected-text token.
            await Apply(manager, new ThemeConfigBuilder().WithToken("ControlItemColorActive", "#FF123456").Build());
            Dispatcher.UIThread.RunJobs();
            AssertSelectedText(Color.Parse("#FF123456"));
        });
        await Apply(manager, null);

        void AssertSelectedText(Color expected)
        {
            ControlTokenAccessor.Capture(select).GetOwn<Color>(SelectTokenKind.OptionSelectedColor).ShouldBe(expected);
            ControlTokenAccessor.Capture(cascader).GetOwn<Color>(CascaderTokenKind.OptionSelectedColor).ShouldBe(expected);
            ControlTokenAccessor.Capture(combo).GetOwn<Color>(ComboBoxTokenKind.ItemSelectedColor).ShouldBe(expected);
            ControlTokenAccessor.Capture(list).GetOwn<Color>(ListBoxTokenKind.ItemSelectedColor).ShouldBe(expected);
            ControlTokenAccessor.Capture(listView).GetOwn<Color>(ListViewTokenKind.ItemSelectedColor).ShouldBe(expected);
            ControlTokenAccessor.Capture(tree).GetOwn<Color>(TreeViewTokenKind.NodeSelectedColor).ShouldBe(expected);
            ControlTokenAccessor.Capture(treeSelect).GetOwn<Color>(TreeSelectTokenKind.NodeSelectedColor).ShouldBe(expected);
        }
    }

    private static Color KnobColor(AtomToggleSwitch toggle) =>
        ((ISolidColorBrush)toggle.GetVisualDescendants().OfType<SwitchKnob>().Single().Background!).Color;

    private static Color? HeaderColor(AtomTreeView tree, string header)
    {
        var item = tree.GetVisualDescendants().OfType<TreeViewItemHeader>()
                       .First(candidate => Equals(candidate.Content, header));
        var presenter = item.GetVisualDescendants().OfType<ContentPresenter>()
                            .First(candidate => candidate.Name == "HeaderPresenter");
        return (presenter.Foreground as ISolidColorBrush)?.Color;
    }

    private static async Task Show(Control content, Func<Task> assertion)
    {
        var window = new AvaloniaWindow { Width = 400, Height = 300, Content = content };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            await assertion();
        }
        finally
        {
            window.Close();
        }
    }

    private static ThemeManager GetThemeManager() =>
        Application.Current!.GetThemeManager().ShouldBeOfType<ThemeManager>();

    private static async Task Apply(ThemeManager manager, ThemeConfig? config)
    {
        var result = await manager.ApplyThemeAsync(
            new ThemeRequest(IThemeManager.DEFAULT_THEME_ID, config, ThemeTransitionReason.UserRequest),
            TestContext.Current.CancellationToken);
        result.Status.ShouldBeOneOf(ThemeTransitionStatus.Committed, ThemeTransitionStatus.NoOp);
    }
}
