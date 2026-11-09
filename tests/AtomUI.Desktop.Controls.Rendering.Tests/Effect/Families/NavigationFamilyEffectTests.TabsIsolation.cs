using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Theme;
using AtomUI.Theme.Schema;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Styling;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AvaloniaBorder = Avalonia.Controls.Border;

namespace AtomUI.Desktop.Controls.Rendering.Tests.Effect.Families;

/// <summary>
/// Scope isolation of the tab controls: item and host identities reach their own member only, the owner-transparent
/// overflow popup follows its host, the registered close button keeps its identity, and the named item themes resolve from
/// the nearest resource scope (E3).
/// </summary>
public partial class NavigationFamilyEffectTests
{
    [Fact]
    public void Controls_Tab_Items_And_Card_Hosts_Change_Only_Their_Member()
    {
        var tabControl = CreateTabs(TabKind.Card, addButton: true);
        var tabStrip   = CreateTabs(TabKind.CardStrip, addButton: true);
        var root = new StackPanel { Spacing = 24, Children = { tabControl, tabStrip } };
        WithIsolationWindow(root, (provider, capture) =>
        {
            var cases = new (ControlTokenIdentity Identity, string Token, Control Target, Control Other)[]
            {
                // Item globals: the disabled tab text colour of each item identity.
                (TabItemTokens.Identity, "ColorTextDisabled", TabOf(tabControl, "Disabled"), TabOf(tabStrip, "Disabled")),
                (TabStripItemTokens.Identity, "ColorTextDisabled", TabOf(tabStrip, "Disabled"), TabOf(tabControl, "Disabled")),
                // Host globals: the card add button border of each host identity.
                (CardTabControlTokens.Identity, "ColorBorderSecondary", AddButtonOf(tabControl), AddButtonOf(tabStrip)),
                (CardTabStripTokens.Identity, "ColorBorderSecondary", AddButtonOf(tabStrip), AddButtonOf(tabControl))
            };
            foreach (var (identity, token, target, other) in cases)
            {
                provider.Config = IsolationConfig();
                using var before = capture();
                provider.Config = IsolationConfig(control: (identity, token, IsolationColor));
                using var after = capture();
                ChangedRatioOf(before, after, target).ShouldBeGreaterThan(TokenEffectHarness.ThinLineThresholdRatio,
                    $"Controls[{identity}].{token} did not reach its member.");
                ChangedRatioOf(before, after, other).ShouldBe(0d, $"Controls[{identity}].{token} leaked into the other member.");

                provider.Config = IsolationConfig(root: (token, IsolationColor));
                using var byRoot = capture();
                ChangedRatioOf(before, byRoot, target).ShouldBeGreaterThan(TokenEffectHarness.ThinLineThresholdRatio,
                    $"Root {token} did not reach the member of {identity}.");
            }
        });
    }

    [Fact]
    public void Tab_Overflow_Menu_Follows_Its_Host()
    {
        // The overflow menu opens in an owner-transparent popup of the host's scroll viewer: Controls[TabStrip] reaches
        // a TabStrip's overflow menu, Controls[TabControl] does not.
        var strip = CreateTabs(TabKind.Strip, count: 20, width: 360);
        var root  = OverlayLayerHost(strip);
        WithIsolationWindow(root, (provider, capture) =>
        {
            OpenOverflow(strip);
            var menu = OverflowMenuOf(strip);
            ThemeScope.GetTokenOwner(menu).ShouldBe(TabStripTokens.Identity);
            using var before = capture();

            provider.Config = IsolationConfig(control: (TabControlTokens.Identity, "ColorBgElevated", IsolationColor));
            using var byTabControl = capture();
            ChangedRatioOf(before, byTabControl, menu).ShouldBe(0d, "Controls[TabControl].ColorBgElevated reached a TabStrip overflow menu.");

            provider.Config = IsolationConfig(control: (TabStripTokens.Identity, "ColorBgElevated", IsolationColor));
            using var byTabStrip = capture();
            ChangedRatioOf(before, byTabStrip, menu).ShouldBeGreaterThan(TokenEffectHarness.DefaultThresholdRatio,
                "Controls[TabStrip].ColorBgElevated did not reach its overflow menu.");
        });
    }

    [Fact]
    public void Tab_Close_Button_Keeps_Its_Identity_And_Follows_The_Tab_Icon_Size()
    {
        var tabs = CreateTabs(TabKind.Line);
        WithIsolationWindow(tabs, (provider, capture) =>
        {
            var tab   = TabOf(tabs, "Closable");
            var close = Descendant<IconButton>(tab, "PART_ItemCloseButton");
            ThemeScope.GetTokenOwner(close).ShouldBe(IconButtonTokens.Identity);
            // Tab items use named themes (TabItemTheme / CardTabItemTheme), which carry no owner injection: the item
            // inherits its host as owner and reads its own globals explicitly through TabItemTokenResource.
            ThemeScope.GetTokenOwner(tab).ShouldBe(TabControlTokens.Identity);
            using var before = capture();

            provider.Config = IsolationConfig(control: (IconButtonTokens.Identity, "IconSizeSM", "24"));
            using var byButton = capture();
            ChangedRatioOf(before, byButton, tab).ShouldBe(0d, "Controls[IconButton].IconSizeSM reached the tab close icon.");

            provider.Config = IsolationConfig(control: (TabItemTokens.Identity, "IconSizeSM", "24"));
            using var byTab = capture();
            ChangedRatioOf(before, byTab, close).ShouldBeGreaterThan(TokenEffectHarness.DefaultThresholdRatio,
                "Controls[TabItem].IconSizeSM did not reach the tab close icon.");
        });
    }

    [Theory]
    [InlineData(TabKind.Line, "TabItemTheme")]
    [InlineData(TabKind.Card, "CardTabItemTheme")]
    [InlineData(TabKind.Strip, "TabStripItemTheme")]
    [InlineData(TabKind.CardStrip, "CardTabStripItemTheme")]
    public void Tab_Item_Theme_Comes_From_The_Nearest_Resource_Scope(TabKind kind, string key)
    {
        // E3: the items looked their named theme up on Application.Current, so a theme supplied in a local resource
        // scope was ignored. They now resolve it from the item itself.
        var local = new ControlTheme(kind is TabKind.Line or TabKind.Card ? typeof(TabItem) : typeof(TabStripItem));
        var host  = CreateTabs(kind);
        var scope = new AvaloniaBorder { Child = host };
        scope.Resources[key] = local;
        WithProvider(scope, _ =>
        {
            foreach (var item in ((ItemsControl)host).Items.OfType<Control>())
            {
                item.Theme.ShouldBeSameAs(local);
            }
        });
    }
}
