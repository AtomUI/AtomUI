using AtomUI.Controls;
using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Icons.AntDesign;
using AtomUI.Theme.Resources;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AvaloniaBorder = Avalonia.Controls.Border;

namespace AtomUI.Desktop.Controls.Rendering.Tests.Effect.Families;

/// <summary>
/// Menu, its items, groups and separators, ContextMenu and the MenuFlyoutPresenter. The Menu Own Tokens style the menu
/// bar's top-level items and every menu popup (the menu bar's submenus, context menus and drop-down flyouts read them
/// through <c>MenuTokenResource</c>); MenuItem, MenuItemGroup, MenuSeparator, ContextMenu and MenuFlyoutPresenter keep
/// their identity and read their own global Tokens. Popups open through the overlay popup layer (pinned open).
/// </summary>
public partial class NavigationFamilyEffectTests
{
    // ---- Menu ---------------------------------------------------------------------------------

    public static IEnumerable<object[]> MenuRows() => MapRows("Menu");

    [Theory]
    [MemberData(nameof(MenuRows))]
    public void Menu_Token_Has_Effect(string token, bool isOwn)
    {
        _ = isOwn;
        if (token is "ContextMenuOffsetX" or "ContextMenuOffsetY" or "SpacingXXS")
        {
            AssertContextMenuOffsetFollowsToken(token);
            return;
        }

        var scenario = MenuScenario(token);
        Run(scenario.Create, scenario.Arrange, MenuTokens.Identity, token, scenario.Options);
    }

    /// <summary>
    /// Context menu offsets are applied when the context menu opens (Avalonia copies them onto its popup), so the effect
    /// is asserted as a gap: reopened after the override, the context menu moves by exactly the offset change. The Menu
    /// Own Tokens style every menu popup, so the override goes to Controls[Menu]; SpacingXXS derives both offsets.
    /// </summary>
    private static void AssertContextMenuOffsetFollowsToken(string token)
    {
        var root = ContextMenuHost(withMenuBar: true);
        WithProvider(root, provider =>
        {
            var menu   = root.GetVisualDescendants().OfType<Menu>().First();
            var target = TopLevel.GetTopLevel(root)!.GetVisualDescendants().OfType<AvaloniaBorder>()
                                 .First(static border => border.ContextMenu is not null);
            var accessor = ControlTokenAccessor.Capture(menu);
            var before   = accessor.GetOwn<double>(MenuTokenKind.ContextMenuOffsetX);
            var beforeY  = accessor.GetOwn<double>(MenuTokenKind.ContextMenuOffsetY);
            var origin   = OpenAndLocate();

            var current = token == "SpacingXXS"
                ? accessor.GetEffectiveGlobal<double>(SharedTokenKind.SpacingXXS)
                : token == "ContextMenuOffsetX" ? before : beforeY;
            provider.Config = new AtomUI.Theme.Configuration.ThemeConfigBuilder()
                              .WithToken(nameof(SharedTokenKind.EnableMotion), "false")
                              .WithControl(MenuTokens.Identity, new AtomUI.Theme.Configuration.ControlThemeConfigBuilder()
                                                                .WithToken(token, (current * 4 + 8).ToString(System.Globalization.CultureInfo.InvariantCulture))
                                                                .Build())
                              .Build();
            WaitFor(() => ControlTokenAccessor.Capture(menu).GetOwn<double>(
                token == "ContextMenuOffsetY" ? MenuTokenKind.ContextMenuOffsetY : MenuTokenKind.ContextMenuOffsetX) !=
                (token == "ContextMenuOffsetY" ? beforeY : before));
            var after  = ControlTokenAccessor.Capture(menu);
            var deltaX = after.GetOwn<double>(MenuTokenKind.ContextMenuOffsetX) - before;
            var deltaY = after.GetOwn<double>(MenuTokenKind.ContextMenuOffsetY) - beforeY;
            (deltaX + deltaY).ShouldBeGreaterThan(0, $"Controls[Menu].{token} did not reach the context menu offsets.");

            var moved = OpenAndLocate();
            (moved.X - origin.X).ShouldBe(deltaX, 0.5, $"Controls[Menu].{token}: the reopened context menu moved horizontally by {moved.X - origin.X}.");
            (moved.Y - origin.Y).ShouldBe(deltaY, 0.5, $"Controls[Menu].{token}: the reopened context menu moved vertically by {moved.Y - origin.Y}.");

            Point OpenAndLocate()
            {
                var contextMenu = (ContextMenu)target.ContextMenu!;
                contextMenu.IsPopupPinnedOpen = false;
                contextMenu.Close();
                WaitFor(() => !contextMenu.IsOpen);
                var opened = OpenContextMenu(root);
                FeedbackFamilyEffectTests.SettleFor(TimeSpan.FromMilliseconds(100));
                return opened.TranslatePoint(new Point(), target).ShouldNotBeNull();
            }
        });
    }

    private static NavScenario MenuScenario(string token)
    {
        switch (token)
        {
            // ---- menu bar: hovered and opened top-level items, sizes ---------------------------------------------
            case "TopLevelItemHoverBg" or "TopLevelItemHoverColor" or "ColorBgTextHover" or "ColorTextSecondary":
                return MenuBar(static menu => Pseudo(TopLevelOf(menu, "File"), ":pointerover"));
            case "TopLevelItemBorderRadius" or "BorderRadius":
                // The hovered item's background shows its corners; perturb them to a clearly rounder item.
                return MenuBar(static menu => Pseudo(TopLevelOf(menu, "File"), ":pointerover"),
                    region: static menu => TopLevelOf((Menu)menu, "File"),
                    perturb: static _ => new CornerRadius(14), bordered: token == "BorderRadius");
            case "TopLevelItemBorderRadiusSM" or "BorderRadiusSM":
                return MenuBar(static menu => Pseudo(TopLevelOf(menu, "File"), ":pointerover"),
                    region: static menu => TopLevelOf((Menu)menu, "File"),
                    perturb: static _ => new CornerRadius(14), size: CustomizableSizeType.Small, bordered: token == "BorderRadiusSM");
            case "TopLevelItemBorderRadiusLG":
                return MenuBar(static menu => Pseudo(TopLevelOf(menu, "File"), ":pointerover"),
                    region: static menu => TopLevelOf((Menu)menu, "File"),
                    perturb: static _ => new CornerRadius(18), size: CustomizableSizeType.Large);
            case "TopLevelItemPaddingSM" or "TopLevelItemFontSizeSM" or "TopLevelItemLineHeightSM" or "ControlHeightSM":
                return MenuBar(null, size: CustomizableSizeType.Small, bordered: token == "ControlHeightSM");
            case "TopLevelItemPaddingLG" or "TopLevelItemFontSizeLG" or "TopLevelItemLineHeightLG" or "ControlHeightLG" or
                "FontSizeLG" or "PaddingContentHorizontalSM":
                return MenuBar(null, size: CustomizableSizeType.Large, bordered: token == "ControlHeightLG");
            // The menu's own frame only shows with a border.
            case "ColorBorder" or "ControlHeight":
                return MenuBar(null, bordered: true);

            // ---- the menu bar's submenu popup --------------------------------------------------------------------
            case "TopLevelItemSelectedBg" or "TopLevelItemSelectedColor":
                return MenuPopup(static menu => TopLevelOf((Menu)menu, "File"));
            case "TopLevelItemPopupMarginToAnchor":
                // The popup gap: the opened popup moves inside the layer.
                return MenuPopup(static menu => LayerPanelOf(menu));
            case "MenuPopupBgColor" or "ColorBgElevated" or "ItemBg" or "MenuPopupContentPadding" or "BorderRadiusLG" or
                "UniformlyPaddingXXS" or "MenuPopupMinWidth" or "ItemColor" or "ColorText" or "ItemHeight" or
                "ItemPaddingInline" or "UniformlyPadding" or "ItemIconSize" or "IconSize" or "ItemIconMarginInlineEnd" or
                "ItemMargin" or "KeyGestureColor" or "ColorTextQuaternary" or "SeparatorItemHeight" or "LineWidth" or
                "GroupTitlePadding" or "UniformlyPaddingXS" or "DangerItemColor" or "ColorError" or "ItemDisabledColor" or
                "ColorTextDisabled":
                return MenuPopup(PopupFrameOfMenu);
            case "MenuPopupMaxWidth":
                // Only an item wider than the maximum is clamped: shrink the maximum below the long item.
                return MenuPopup(PopupFrameOfMenu, longItem: true, perturb: static _ => 140d);
            case "ItemHoverBg" or "ItemHoverColor":
                return MenuPopup(PopupFrameOfMenu, static menu => Pseudo(PopupItemOf(menu, "Open"), ":pointerover"));
            case "ItemBorderRadius":
                return MenuPopup(static menu => PopupItemOf((Menu)menu, "Open"),
                    static menu => Pseudo(PopupItemOf(menu, "Open"), ":pointerover"), perturb: static _ => new CornerRadius(14));
            case "DangerItemHoverColor":
                return MenuPopup(PopupFrameOfMenu, static menu => Pseudo(PopupItemOf(menu, "Delete"), ":pointerover"));

            // ---- menu bar default: top-level text, padding, icon -----------------------------------------------
            default:
                return MenuBar(null);
        }
    }

    private static Control PopupFrameOfMenu(Control menu) => MenuPopupFrameOf((Menu)menu);

    private static NavScenario MenuBar(
        Action<Menu>? arrange,
        Func<Control, Control>? region = null,
        Func<object, object>? perturb = null,
        CustomizableSizeType size = CustomizableSizeType.Middle,
        bool bordered = false) =>
        new(() =>
            {
                var menu = CreateMenu();
                menu.SizeType = size;
                if (bordered)
                {
                    // A user-supplied border: the menu's border colour, radius and minimum height show through it.
                    menu.BorderThickness = new Thickness(1);
                }
                return menu;
            },
            arrange is null ? null : control => arrange((Menu)control),
            region is null && perturb is null ? null : new EffectOptions { Region = region, Perturb = perturb });

    /// <summary>A menu bar whose "File" submenu is pinned open through the overlay popup layer.</summary>
    private static NavScenario MenuPopup(
        Func<Control, Control> region,
        Action<Menu>? arrange = null,
        bool longItem = false,
        Func<object, object>? perturb = null) =>
        new(() => OverlayLayerHost(CreateMenu(longItem)),
            control =>
            {
                var menu = (Menu)control;
                menu.IsPopupPinnedOpen = true;
                MenuPopupFrameOf(menu);
                arrange?.Invoke(menu);
            },
            new EffectOptions
            {
                Target  = static root => root.GetVisualDescendants().OfType<Menu>().First(),
                Region  = region,
                Perturb = perturb
            });

    /// <summary>
    /// A menu bar: "File" (icon) opens a submenu with an icon item with a key gesture, a plain item, a separator, a titled
    /// group, a danger item, a disabled item and a nested submenu; "Edit" is a second top-level item.
    /// </summary>
    internal static Menu CreateMenu(bool longItem = false)
    {
        var file = new MenuItem { Header = "File", Icon = new SearchOutlined() };
        file.Items.Add(new MenuItem
        {
            Header       = "New",
            Icon         = new SettingOutlined(),
            InputGesture = new KeyGesture(Key.N, KeyModifiers.Control)
        });
        file.Items.Add(new MenuItem
            { Header = longItem ? string.Join(' ', Enumerable.Repeat("A long menu item", 6)) : "Open" });
        file.Items.Add(new MenuSeparator());
        file.Items.Add(new MenuItemGroup { Header = "Recent", Items = { new MenuItem { Header = "Recent file" } } });
        file.Items.Add(new MenuItem { Header = "Delete", IsDanger = true });
        file.Items.Add(new MenuItem { Header = "Disabled", IsEnabled = false });
        var more = new MenuItem { Header = "More" };
        more.Items.Add(new MenuItem { Header = "Nested" });
        file.Items.Add(more);
        var menu = new Menu { Width = 360 };
        menu.Items.Add(file);
        menu.Items.Add(new MenuItem { Header = "Edit" });
        return menu;
    }

    internal static MenuItem TopLevelOf(Menu menu, string header) =>
        menu.GetVisualDescendants().OfType<MenuItem>().First(item => Equals(item.Header, header));

    /// <summary>A realized item of an open menu popup, searched through the whole window (popups paint in the layer).</summary>
    internal static MenuItem PopupItemOf(Control anyInWindow, string header) =>
        WaitForVisual(() => TopLevel.GetTopLevel(anyInWindow)!.GetVisualDescendants().OfType<MenuItem>()
                                    .FirstOrDefault(item => Equals(item.Header, header) && !item.IsTopLevel),
            $"The menu popup item '{header}'");

    internal static AvaloniaBorder MenuPopupFrameOf(Control anyInWindow) =>
        WaitForVisual(() => TopLevel.GetTopLevel(anyInWindow)!.GetVisualDescendants().OfType<AvaloniaBorder>()
                                    .FirstOrDefault(static border => border.Name == "PopupFrame" && border.IsEffectivelyVisible),
            "The menu popup");

    // ---- MenuItem / MenuItemGroup / MenuSeparator -------------------------------------------------------------------

    public static IEnumerable<object[]> MenuItemRows() => MapRows("MenuItem");

    [Theory]
    [MemberData(nameof(MenuItemRows))]
    public void MenuItem_Token_Has_Effect(string token, bool isOwn)
    {
        _ = isOwn;
        if (token == "IconSizeXS")
        {
            // The submenu arrow of a popup item that has its own submenu.
            Run(static () => OverlayLayerHost(CreateMenu()),
                null, MenuItemTokens.Identity, token,
                new EffectOptions
                {
                    Target = static root =>
                    {
                        root.GetVisualDescendants().OfType<Menu>().First().IsPopupPinnedOpen = true;
                        return PopupItemOf(root, "More");
                    }
                });
            return;
        }

        // A top-level item's minimum height per size: its hover background shows the height.
        var size = token switch
        {
            "ControlHeightSM" => CustomizableSizeType.Small,
            "ControlHeightLG" => CustomizableSizeType.Large,
            _                 => CustomizableSizeType.Middle
        };
        Run(() =>
            {
                var menu = CreateMenu();
                menu.SizeType = size;
                return menu;
            },
            static item => Pseudo(item, ":pointerover"), MenuItemTokens.Identity, token,
            new EffectOptions { Target = static root => TopLevelOf((Menu)root, "File") });
    }

    public static IEnumerable<object[]> MenuItemGroupRows() => MapRows("MenuItemGroup");

    [Theory]
    [MemberData(nameof(MenuItemGroupRows))]
    public void MenuItemGroup_Token_Has_Effect(string token, bool isOwn)
    {
        _ = isOwn;
        Run(static () => OverlayLayerHost(CreateMenu()), null, MenuItemGroupTokens.Identity, token,
            new EffectOptions { Target = static root => OpenedPopupPart<MenuItemGroup>(root) });
    }

    public static IEnumerable<object[]> MenuSeparatorRows() => MapRows("MenuSeparator");

    [Theory]
    [MemberData(nameof(MenuSeparatorRows))]
    public void MenuSeparator_Token_Has_Effect(string token, bool isOwn)
    {
        _ = isOwn;
        Run(static () => OverlayLayerHost(CreateMenu()), null, MenuSeparatorTokens.Identity, token,
            new EffectOptions { Target = static root => OpenedPopupPart<MenuSeparator>(root) });
    }

    private static T OpenedPopupPart<T>(Control root)
        where T : Control
    {
        root.GetVisualDescendants().OfType<Menu>().First().IsPopupPinnedOpen = true;
        return WaitForVisual(() => TopLevel.GetTopLevel(root)!.GetVisualDescendants().OfType<T>().FirstOrDefault(),
            typeof(T).Name);
    }

    // ---- ContextMenu ----------------------------------------------------------------------------------------------

    public static IEnumerable<object[]> ContextMenuRows() => MapRows("ContextMenu");

    [Theory]
    [MemberData(nameof(ContextMenuRows))]
    public void ContextMenu_Token_Has_Effect(string token, bool isOwn)
    {
        _ = isOwn;
        // EnableMotion / MotionDurationMid read back; ContextMenu_Motion_Follows_Its_Motion_Tokens shows the consumers.
        Run(static () => ContextMenuHost(withMenuBar: false), null, ContextMenuTokens.Identity, token,
            new EffectOptions { Target = static root => OpenContextMenu(root) });
    }

    [Fact]
    public void ContextMenu_Motion_Follows_Its_Motion_Tokens()
    {
        var root = ContextMenuHost(withMenuBar: false);
        WithProvider(root, provider =>
        {
            var contextMenu = OpenContextMenu(root);
            var duration    = TimeSpan.FromMilliseconds(654);
            contextMenu.MotionDuration.ShouldNotBe(duration);
            provider.Config = WithControlToken(ContextMenuTokens.Identity, "MotionDurationMid", duration.ToString("c"));
            WaitFor(() => contextMenu.MotionDuration == duration);
            contextMenu.MotionDuration.ShouldBe(duration);

            contextMenu.IsMotionEnabled.ShouldBeTrue();
            provider.Config = WithControlToken(ContextMenuTokens.Identity, "EnableMotion", "false");
            WaitFor(() => !contextMenu.IsMotionEnabled);
            contextMenu.IsMotionEnabled.ShouldBeFalse();
        });
    }

    /// <summary>A host with a context menu (the menu items of <see cref="CreateMenu"/>'s File submenu), optionally beside a menu bar.</summary>
    private static Control ContextMenuHost(bool withMenuBar)
    {
        var contextMenu = new ContextMenu();
        contextMenu.Items.Add(new MenuItem { Header = "Copy", Icon = new SearchOutlined() });
        contextMenu.Items.Add(new MenuItem { Header = "Paste" });
        var target = new AvaloniaBorder
        {
            Width       = 160,
            Height      = 48,
            Margin      = new Thickness(40, 80, 0, 0),
            Background  = Avalonia.Media.Brushes.LightGray,
            ContextMenu = contextMenu
        };
        var stack = new StackPanel { Orientation = Orientation.Vertical };
        if (withMenuBar)
        {
            stack.Children.Add(CreateMenu());
        }
        stack.Children.Add(target);
        return OverlayLayerHost(stack);
    }

    private static ContextMenu OpenContextMenu(Control anyInWindow)
    {
        var contextMenu = TopLevel.GetTopLevel(anyInWindow)!.GetVisualDescendants().OfType<AvaloniaBorder>()
                                  .Select(static border => border.ContextMenu).OfType<ContextMenu>().First();
        contextMenu.IsPopupPinnedOpen = true;
        return WaitForVisual(() => contextMenu.IsEffectivelyVisible ? contextMenu : null, "The context menu");
    }

    // ---- MenuFlyoutPresenter --------------------------------------------------------------------------------------

    public static IEnumerable<object[]> MenuFlyoutPresenterRows() => MapRows("MenuFlyoutPresenter");

    [Theory]
    [MemberData(nameof(MenuFlyoutPresenterRows))]
    public void MenuFlyoutPresenter_Token_Has_Effect(string token, bool isOwn)
    {
        _ = isOwn;
        Run(static () =>
            {
                var flyout = new MenuFlyout();
                flyout.Items.Add(new MenuItem { Header = "First item" });
                flyout.Items.Add(new MenuItem { Header = "Second item" });
                return OverlayLayerHost(new DropdownButton { Content = "Dropdown", DropdownFlyout = flyout });
            },
            null, MenuFlyoutPresenterTokens.Identity, token,
            new EffectOptions
            {
                Target = static root =>
                {
                    root.GetVisualDescendants().OfType<DropdownButton>().First().IsPopupPinnedOpen = true;
                    return WaitForVisual(() => TopLevel.GetTopLevel(root)!.GetVisualDescendants().OfType<MenuFlyoutPresenter>()
                                                       .FirstOrDefault(static p => p.IsEffectivelyVisible),
                        "The drop-down flyout");
                }
            });
    }

    // ---- E3: named top-level item theme ----------------------------------------------------------------------------

    [Fact]
    public void Menu_Top_Level_Item_Theme_Comes_From_The_Nearest_Resource_Scope()
    {
        // E3: the Menu looked the named "TopLevelMenuItemTheme" up on Application.Current, so a theme supplied in a
        // local resource scope was ignored. It now resolves from the Menu itself.
        var local = new Avalonia.Styling.ControlTheme(typeof(MenuItem));
        var menu  = CreateMenu();
        var scope = new AvaloniaBorder { Child = menu };
        scope.Resources[Menu.TopLevelMenuItemThemeKey] = local;
        WithProvider(scope, _ =>
        {
            menu.ItemContainerTheme.ShouldBeSameAs(local);
            TopLevelOf(menu, "File").Theme.ShouldBeSameAs(local);
        });
    }
}
