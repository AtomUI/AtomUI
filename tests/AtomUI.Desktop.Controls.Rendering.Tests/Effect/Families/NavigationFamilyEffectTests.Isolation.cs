using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Theme;
using AtomUI.Theme.Configuration;
using AtomUI.Theme.Schema;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AvaloniaMenuItemToggleType = Avalonia.Controls.MenuItemToggleType;

namespace AtomUI.Desktop.Controls.Rendering.Tests.Effect.Families;

/// <summary>
/// Scope isolation of the navigation family (theme-effect-tests.md, family gate): a <c>Controls[X]</c> override reaches X
/// only, the root Token reaches X, overrides reach the submenus X opens in (owner-transparent) popups, and registered
/// controls nested in a member keep their own identity.
/// </summary>
public partial class NavigationFamilyEffectTests
{
    private const string IsolationColor = "#ff00b96b";

    private static ThemeConfig IsolationConfig(
        (ControlTokenIdentity Identity, string Token, string Value)? control = null,
        (string Token, string Value)? root = null) =>
        FeedbackFamilyEffectTests.IsolationConfig(control, root);

    private static void WithIsolationWindow(Control content, Action<ThemeConfigProvider, Func<SkiaSharp.SKBitmap>> body) =>
        FeedbackFamilyEffectTests.WithIsolationWindow(content, body);

    private static double ChangedRatioOf(SkiaSharp.SKBitmap before, SkiaSharp.SKBitmap after, Control control) =>
        FeedbackFamilyEffectTests.ChangedRatioOf(before, after, control);

    [Fact]
    public void Controls_NavMenu_And_Menu_Main_Colours_Change_Only_Their_Member()
    {
        var navMenu = CreateNavMenu(NavMenuMode.Inline);
        var menu    = CreateMenu();
        var root = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing     = 24,
            Children    = { navMenu, new Panel { Width = 360, VerticalAlignment = VerticalAlignment.Top, Children = { menu } } }
        };
        WithIsolationWindow(root, (provider, capture) =>
        {
            // ColorText feeds both members' item text colours (NavMenu ItemColor, Menu TopLevelItemColor).
            var members = new (ControlTokenIdentity Identity, Control Control)[]
            {
                (NavMenuTokens.Identity, navMenu),
                (MenuTokens.Identity, menu)
            };
            foreach (var (identity, target) in members)
            {
                provider.Config = IsolationConfig();
                using var before = capture();
                provider.Config = IsolationConfig(control: (identity, "ColorText", IsolationColor));
                using var after = capture();
                foreach (var (_, control) in members)
                {
                    var ratio = ChangedRatioOf(before, after, control);
                    if (ReferenceEquals(control, target))
                    {
                        ratio.ShouldBeGreaterThan(TokenEffectHarness.ThinLineThresholdRatio,
                            $"Controls[{identity}].ColorText did not reach {control.GetType().Name}.");
                    }
                    else
                    {
                        ratio.ShouldBe(0d, $"Controls[{identity}].ColorText leaked into {control.GetType().Name}.");
                    }
                }

                provider.Config = IsolationConfig();
                using var rootBefore = capture();
                provider.Config = IsolationConfig(root: ("ColorText", IsolationColor));
                using var rootAfter = capture();
                ChangedRatioOf(rootBefore, rootAfter, target).ShouldBeGreaterThan(TokenEffectHarness.ThinLineThresholdRatio,
                    $"Root ColorText did not reach {target.GetType().Name}.");
            }
        });
    }

    [Fact]
    public void NavMenu_Overrides_Reach_Its_Submenu_Popup_And_Not_A_Menu_Popup()
    {
        // The NavMenu submenu popup (an owner-transparent Popup) and the menu bar's popup are open side by side:
        // Controls[NavMenu] reaches the NavMenu popup's items and frame only, Controls[Menu] the menu popup only.
        var navMenu = CreateNavMenu(NavMenuMode.Vertical);
        var menu    = CreateMenu();
        var stack = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing     = 260,
            Children    = { menu, navMenu }
        };
        var root = OverlayLayerHost(stack, 900, 700);
        WithIsolationWindow(root, (provider, capture) =>
        {
            OpenPinned(navMenu);
            menu.IsPopupPinnedOpen = true;
            var navPopup  = PopupFrameOf(navMenu);
            var menuPopup = MenuPopupFrameOf(menu);
            var popupHeader = HeaderOf(ItemOf(navMenu, "Option 1"));
            // The header inside the popup inherits the NavMenu as owner (Popup and the headers are owner-transparent).
            ThemeScope.GetTokenOwner(popupHeader).ShouldBe(NavMenuTokens.Identity);
            ThemeScope.GetTokenOwner(navPopup).ShouldBe(NavMenuTokens.Identity);
            using var before = capture();

            foreach (var token in new[] { "ColorText", "ColorBgElevated" })
            {
                provider.Config = IsolationConfig(control: (NavMenuTokens.Identity, token, IsolationColor));
                using var byNavMenu = capture();
                ChangedRatioOf(before, byNavMenu, navPopup).ShouldBeGreaterThan(TokenEffectHarness.ThinLineThresholdRatio,
                    $"Controls[NavMenu].{token} did not reach the NavMenu submenu popup.");
                ChangedRatioOf(before, byNavMenu, menuPopup).ShouldBe(0d, $"Controls[NavMenu].{token} leaked into the menu popup.");

                provider.Config = IsolationConfig(control: (MenuTokens.Identity, token, IsolationColor));
                using var byMenu = capture();
                ChangedRatioOf(before, byMenu, menuPopup).ShouldBeGreaterThan(TokenEffectHarness.ThinLineThresholdRatio,
                    $"Controls[Menu].{token} did not reach the menu popup.");
                ChangedRatioOf(before, byMenu, navPopup).ShouldBe(0d, $"Controls[Menu].{token} leaked into the NavMenu popup.");
            }
        });
    }

    [Fact]
    public void Menu_Item_Check_Box_Keeps_Its_Identity()
    {
        // A checkable menu item hosts a registered CheckBox: Controls[Menu] / Controls[MenuItem] do not reach its check
        // colour, Controls[CheckBox] does.
        var menu = CreateMenu();
        var file = menu.Items.OfType<MenuItem>().First();
        file.Items.Insert(0, new MenuItem { Header = "Checked", ToggleType = AvaloniaMenuItemToggleType.CheckBox, IsChecked = true });
        var root = OverlayLayerHost(menu);
        WithIsolationWindow(root, (provider, capture) =>
        {
            menu.IsPopupPinnedOpen = true;
            var item     = PopupItemOf(menu, "Checked");
            var checkBox = WaitForVisual(() => item.GetVisualDescendants().OfType<CheckBox>()
                                                   .FirstOrDefault(static box => box.IsEffectivelyVisible), "The item check box");
            ThemeScope.GetTokenOwner(checkBox).ShouldBe(CheckBoxTokens.Identity);
            using var before = capture();

            foreach (var identity in new[] { MenuTokens.Identity, MenuItemTokens.Identity })
            {
                provider.Config = IsolationConfig(control: (identity, "ColorPrimary", IsolationColor));
                using var byMenu = capture();
                ChangedRatioOf(before, byMenu, checkBox).ShouldBe(0d, $"Controls[{identity}].ColorPrimary reached the item check box.");
            }

            provider.Config = IsolationConfig(control: (CheckBoxTokens.Identity, "ColorPrimary", IsolationColor));
            using var byCheckBox = capture();
            ChangedRatioOf(before, byCheckBox, checkBox).ShouldBeGreaterThan(TokenEffectHarness.DefaultThresholdRatio,
                "Controls[CheckBox].ColorPrimary did not reach the item check box.");
        });
    }

    [Fact]
    public void Menu_Items_In_Context_Menus_And_Flyouts_Follow_The_Menu_Own_Tokens()
    {
        // Menu Own Tokens style every menu popup: a context menu's items follow Controls[Menu].ItemColor; Controls[NavMenu]
        // does not reach them.
        var root = ContextMenuHost(withMenuBar: false);
        WithIsolationWindow(root, (provider, capture) =>
        {
            var contextMenu = OpenContextMenu(root);
            using var before = capture();

            provider.Config = IsolationConfig(control: (NavMenuTokens.Identity, "ItemColor", IsolationColor));
            using var byNavMenu = capture();
            ChangedRatioOf(before, byNavMenu, contextMenu).ShouldBe(0d, "Controls[NavMenu].ItemColor reached a context menu.");

            provider.Config = IsolationConfig(control: (MenuTokens.Identity, "ItemColor", IsolationColor));
            using var byMenu = capture();
            ChangedRatioOf(before, byMenu, contextMenu).ShouldBeGreaterThan(TokenEffectHarness.ThinLineThresholdRatio,
                "Controls[Menu].ItemColor did not reach the context menu items.");
        });
    }
}
