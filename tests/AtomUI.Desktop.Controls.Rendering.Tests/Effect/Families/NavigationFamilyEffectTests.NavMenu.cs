using AtomUI.Controls;
using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Icons.AntDesign;
using AtomUI.Theme;
using AtomUI.Theme.Resources;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Styling;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;

namespace AtomUI.Desktop.Controls.Rendering.Tests.Effect.Families;

/// <summary>
/// NavMenu. Its items, headers, group titles, dividers and submenu popups have no Token identity of their own (the
/// horizontal and inline headers are registered but Token-owner transparent), so every NavMenu row runs on a real menu in
/// the state that shows the Token: inline (default, hovered, selected, pressed, keyboard-active, opened submenu, collapsed),
/// vertical with its submenu popup pinned open, horizontal (top-level selection bar, top-level popup), dark style, and
/// danger items.
/// </summary>
public partial class NavigationFamilyEffectTests
{
    public static IEnumerable<object[]> NavMenuRows() => MapRows("NavMenu");

    [Theory]
    [MemberData(nameof(NavMenuRows))]
    public void NavMenu_Token_Has_Effect(string token, bool isOwn)
    {
        _ = isOwn;
        var scenario = NavMenuScenario(token);
        Run(scenario.Create, scenario.Arrange, NavMenuTokens.Identity, token, scenario.Options);
    }

    [Fact]
    public void NavMenu_Light_Horizontal_Item_Radius_Shapes_A_Styled_Hover_Background()
    {
        // HorizontalItemBorderRadius applies to light horizontal top-level items too (antd horizontalItemBorderRadius). Their
        // backgrounds are transparent in every state by default (hover, open, selected, keyboard-active and pointer-hold), so
        // the radius shows on a background an application gives them: here an application style fills the hovered item
        // (the map row covers the dark selected item).
        Run(static () =>
            {
                var menu = CreateNavMenu(NavMenuMode.Horizontal);
                menu.Styles.Add(new Style(static x => x.OfType<HorizontalNavMenuItemHeader>().Class(":pointerover"))
                {
                    Setters = { new Setter(TemplatedControl.BackgroundProperty, Avalonia.Media.Brushes.LightBlue) }
                });
                return menu;
            },
            static menu => Pseudo(HeaderOf(ItemOf((NavMenu)menu, "Mail")), ":pointerover"),
            NavMenuTokens.Identity, "HorizontalItemBorderRadius",
            new EffectOptions
            {
                Region  = static menu => HeaderOf(ItemOf((NavMenu)menu, "Mail")),
                Perturb = static _ => new CornerRadius(16)
            });
    }

    private sealed record NavScenario(Func<Control> Create, Action<Control>? Arrange, EffectOptions? Options = null);

    private static NavScenario NavMenuScenario(string token)
    {
        switch (token)
        {
            // ---- inline: hovered, selected, pressed, keyboard-active items -------------------------------------------
            // Tokens of a single item compare that item: its text alone is too small a share of the whole menu.
            case "ItemHoverColor" or "ItemHoverBg" or "ColorBgTextHover":
                return Inline(static menu => Pseudo(HeaderOf(ItemOf(menu, "Mail")), ":pointerover"), At("Mail"));
            case "ItemSelectedColor" or "ItemSelectedBg" or "ControlItemBgActive" or "ColorPrimary":
                return Inline(static menu => menu.SelectedItem = NodeOf(menu, "Mail"), At("Mail"));
            case "ItemBorderRadius" or "BorderRadiusLG":
                // The selected background shows the corners; perturb them to a clearly rounder item.
                return Inline(static menu => menu.SelectedItem = NodeOf(menu, "Mail"),
                    new EffectOptions
                    {
                        Region  = static menu => HeaderOf(ItemOf((NavMenu)menu, "Mail")),
                        Perturb = static _ => new CornerRadius(18)
                    });
            case "ItemActiveBg" or "ColorFillContent":
                return Inline(static menu => ItemOf(menu, "Mail").IsKeyboardActive = true);

            // ---- inline: opened submenu ------------------------------------------------------------------------------
            case "SubMenuItemBg" or "ColorFillAlter" or "VerticalChildItemsMargin" or "InlineItemIndentUnit":
                return Inline(static menu => OpenInline(menu, "Navigation"));

            // ---- inline: danger items --------------------------------------------------------------------------------
            case "DangerItemColor" or "ColorError":
                return Inline(null, At("Delete"));
            case "DangerItemHoverColor":
                return Inline(static menu => Pseudo(HeaderOf(ItemOf(menu, "Delete")), ":pointerover"), At("Delete"));
            case "DangerItemSelectedColor" or "DangerItemSelectedBg" or "ColorErrorBg":
                return Inline(static menu => menu.SelectedItem = NodeOf(menu, "Delete"), At("Delete"));
            case "DangerItemActiveBg":
                return Inline(static menu => ItemOf(menu, "Delete").IsPointerHold = true, At("Delete"));

            // ---- inline: disabled item, submenu arrow, divider -------------------------------------------------------
            case "ItemDisabledColor" or "ColorTextDisabled":
                return Inline(null, At("Disabled"));
            case "MenuArrowSize":
                return Inline(null, At("Navigation"));
            case "ColorSplit":
                return Inline(null, new EffectOptions { Region = static menu => Descendant<NavMenuDividerItem>(menu) });

            // ---- inline collapsed -----------------------------------------------------------------------------------
            case "InlineCollapsedWidth" or "CollapsedIconSize" or "IconSizeLG":
                return Inline(static menu => menu.IsInlineCollapsed = true);

            // ---- dark style ------------------------------------------------------------------------------------------
            case "DarkMenuBg" or "DarkItemColor" or "ColorTextLightSolid" or "DarkItemBg" or "DarkGroupTitleColor":
                return Inline(null, dark: true);
            case "DarkItemDisabledColor":
                return Inline(null, At("Disabled"), dark: true);
            case "DarkDangerItemColor":
                return Inline(null, At("Delete"), dark: true);
            case "DarkItemHoverColor" or "DarkItemHoverBg":
                return Inline(static menu => Pseudo(HeaderOf(ItemOf(menu, "Mail")), ":pointerover"), At("Mail"), dark: true);
            case "DarkItemSelectedColor" or "DarkItemSelectedBg":
                return Inline(static menu => menu.SelectedItem = NodeOf(menu, "Mail"), At("Mail"), dark: true);
            case "DarkSubMenuItemBg":
                return Inline(static menu => OpenInline(menu, "Navigation"), dark: true);
            case "DarkDangerItemHoverColor" or "ColorErrorHover":
                return Inline(static menu => Pseudo(HeaderOf(ItemOf(menu, "Delete")), ":pointerover"), At("Delete"), dark: true);
            case "DarkDangerItemSelectedColor" or "DarkDangerItemSelectedBg":
                return Inline(static menu => menu.SelectedItem = NodeOf(menu, "Delete"), At("Delete"), dark: true);
            case "DarkDangerItemActiveBg":
                return Inline(static menu => ItemOf(menu, "Delete").IsPointerHold = true, At("Delete"), dark: true);

            // ---- vertical: the submenu popup -------------------------------------------------------------------------
            case "MenuPopupBg" or "ColorBgElevated" or "MenuPopupContentPadding" or "MenuPopupMinWidth" or
                "MenuPopupShadows" or "BoxShadowsSecondary":
                return VerticalPopup(static popup => popup);
            case "DarkMenuPopupBg":
                return VerticalPopup(static popup => popup, dark: true);
            case "MenuPopupMaxWidth":
                // Only a header wider than the maximum is clamped: shrink the maximum below the long header.
                return VerticalPopup(static popup => popup, longChild: string.Join(' ', Enumerable.Repeat("A long option", 6)),
                    perturb: static _ => 120d);
            case "MenuPopupMaxHeight":
                // Only a list taller than the maximum is clamped: shrink the maximum below the eight-item list.
                return VerticalPopup(static popup => popup, childCount: 8, perturb: static _ => 120d);
            case "SubMenuItemBorderRadius" or "BorderRadiusSM":
                // A selected popup item shows the corners; perturb them to a clearly rounder item.
                return VerticalPopup(static popup => popup, selectChild: true, perturb: static _ => new CornerRadius(16));
            case "SpacingXS" or "SpacingXXS":
                // The popup gap: the opened popup moves inside the layer.
                return VerticalPopup(static popup => LayerPanelOf(popup));

            // ---- horizontal ------------------------------------------------------------------------------------------
            case "MenuHorizontalHeight" or "ColorBorderSecondary" or "HorizontalItemMargin" or "IconMargin" or
                "ControlHeightSM" or "HorizontalLineHeight":
                return Horizontal(null);
            case "HorizontalItemSelectedColor" or "HorizontalItemSelectedBg" or "ActiveBarHeight" or "LineWidthBold":
                return Horizontal(static menu => menu.SelectedItem = NodeOf(menu, "Mail"), At("Mail"));
            case "ActiveBarScaleX":
                // The bar spans the full item at 1 (its maximum): shrink it to half the item.
                return Horizontal(static menu => menu.SelectedItem = NodeOf(menu, "Mail"),
                    At("Mail") with { Perturb = static _ => 0.5d });
            case "HorizontalItemHoverBg":
                return Horizontal(static menu => Pseudo(HeaderOf(ItemOf(menu, "Mail")), ":pointerover"), At("Mail"));
            case "HorizontalItemBorderRadius":
                // The dark selected top-level item has a filled background that shows its corners.
                return Horizontal(static menu => menu.SelectedItem = NodeOf(menu, "Mail"),
                    new EffectOptions
                    {
                        Region  = static menu => HeaderOf(ItemOf((NavMenu)menu, "Mail")),
                        Perturb = static _ => new CornerRadius(16)
                    }, dark: true);
            case "TopLevelItemPopupMarginToAnchor":
                // The top-level popup gap: the opened popup moves inside the layer.
                return new NavScenario(
                    static () => OverlayLayerHost(CreateNavMenu(NavMenuMode.Horizontal)),
                    static menu => OpenPinned((NavMenu)menu),
                    new EffectOptions
                    {
                        Target = static root => root.GetVisualDescendants().OfType<NavMenu>().First(),
                        Region = static menu => LayerPanelOf(menu)
                    });

            // ---- inline default: base layout, colours, typography, group title, divider, disabled --------------------
            default:
                return Inline(null);
        }
    }

    /// <summary>Compares the header of the item with <paramref name="header"/>.</summary>
    private static EffectOptions At(string header) =>
        new() { Region = menu => HeaderOf(ItemOf((NavMenu)menu, header)) };

    private static NavScenario Inline(Action<NavMenu>? arrange, EffectOptions? options = null, bool dark = false) =>
        new(() => CreateNavMenu(NavMenuMode.Inline, dark),
            arrange is null ? null : control => arrange((NavMenu)control),
            options);

    private static NavScenario Horizontal(Action<NavMenu>? arrange, EffectOptions? options = null, bool dark = false) =>
        new(() => CreateNavMenu(NavMenuMode.Horizontal, dark),
            arrange is null ? null : control => arrange((NavMenu)control),
            options);

    /// <summary>A vertical NavMenu whose "Navigation" submenu popup is pinned open; the popup frame is compared.</summary>
    private static NavScenario VerticalPopup(
        Func<Control, Control> region,
        bool dark = false,
        int childCount = 2,
        string? longChild = null,
        bool selectChild = false,
        Func<object, object>? perturb = null) =>
        new(() => OverlayLayerHost(CreateNavMenu(NavMenuMode.Vertical, dark, childCount, longChild)),
            control =>
            {
                var menu = (NavMenu)control;
                OpenPinned(menu);
                if (selectChild)
                {
                    menu.SelectedItem = NodeOf(menu, "Option 1");
                }
            },
            new EffectOptions
            {
                Target  = static root => root.GetVisualDescendants().OfType<NavMenu>().First(),
                Region  = menu => region(PopupFrameOf((NavMenu)menu)),
                Perturb = perturb
            });

    /// <summary>
    /// A NavMenu with an icon leaf, an icon submenu, a titled group and a divider (vertical modes), a danger leaf and a
    /// disabled leaf.
    /// </summary>
    internal static NavMenu CreateNavMenu(NavMenuMode mode, bool dark = false, int childCount = 2, string? longChild = null)
    {
        var menu = new NavMenu
        {
            Mode        = mode,
            IsDarkStyle = dark,
            Width       = mode == NavMenuMode.Horizontal ? 560 : 256
        };
        menu.Items.Add(new NavMenuNode { Header = "Mail", Icon = new SearchOutlined() });
        var navigation = new NavMenuNode { Header = "Navigation", Icon = new SettingOutlined() };
        for (var index = 0; index < childCount; index++)
        {
            navigation.Children.Add(new NavMenuNode { Header = index == 0 && longChild is not null ? longChild : $"Option {index + 1}" });
        }
        menu.Items.Add(navigation);
        if (mode != NavMenuMode.Horizontal)
        {
            var group = new NavMenuGroup { Header = "Item Group" };
            group.Entries.Add(new NavMenuNode { Header = "Option 9" });
            menu.Items.Add(group);
            menu.Items.Add(new NavMenuDivider());
        }
        menu.Items.Add(new NavMenuNode { Header = "Delete", IsDanger = true });
        menu.Items.Add(new NavMenuNode { Header = "Disabled", IsEnabled = false });
        return menu;
    }

    internal static INavMenuNode NodeOf(NavMenu menu, string header)
    {
        return Find(menu.Items.OfType<INavMenuEntry>()) ?? throw new InvalidOperationException($"No node '{header}'.");

        INavMenuNode? Find(IEnumerable<INavMenuEntry> entries)
        {
            foreach (var entry in entries)
            {
                switch (entry)
                {
                    case INavMenuNode node when Equals(node.Header, header):
                        return node;
                    case INavMenuNode node when Find(node.Entries) is { } child:
                        return child;
                    case NavMenuGroup group when Find(group.Entries) is { } member:
                        return member;
                }
            }
            return null;
        }
    }

    /// <summary>The realized container of the node with <paramref name="header"/> (top level, in groups and in open submenus).</summary>
    internal static NavMenuItem ItemOf(NavMenu menu, string header)
    {
        var node = NodeOf(menu, header);
        return WaitForVisual(() => TopLevel.GetTopLevel(menu)!.GetVisualDescendants().OfType<NavMenuItem>()
                                           .FirstOrDefault(item => ReferenceEquals(((INavMenuItem)item).Node, node)),
            $"The NavMenu item '{header}'");
    }

    internal static BaseNavMenuItemHeader HeaderOf(NavMenuItem item) =>
        item.GetVisualDescendants().OfType<BaseNavMenuItemHeader>().First(static header => header.Name == "PART_Header");

    private static void OpenInline(NavMenu menu, string header)
    {
        ItemOf(menu, header).SetCurrentValue(NavMenuItem.IsSubMenuOpenProperty, true);
        WaitFor(() => ItemOf(menu, "Option 1").Bounds.Height > 0);
    }

    internal static void OpenPinned(NavMenu menu)
    {
        menu.IsPopupPinnedOpen = true;
        PopupFrameOf(menu);
    }

    internal static NavMenuPopupFrame PopupFrameOf(NavMenu menu) =>
        WaitForVisual(() => TopLevel.GetTopLevel(menu)!.GetVisualDescendants().OfType<NavMenuPopupFrame>()
                                    .FirstOrDefault(static frame => frame.IsEffectivelyVisible),
            "The NavMenu submenu popup");

    // ---- motion ---------------------------------------------------------------------------------------------------

    [Fact]
    public void NavMenu_Motion_Follows_Its_Motion_Tokens()
    {
        // MotionDurationMid / EnableMotion read back as values; this fact shows their consumers follow Controls[NavMenu]:
        // the motion switch, the inline submenu expand duration, the inline-collapse width animation (read through the
        // control's effective Tokens, E3) and the header colour transitions (TokenTransitions on the owner-transparent
        // headers resolve to the NavMenu).
        var menu = CreateNavMenu(NavMenuMode.Inline);
        WithProvider(menu, provider =>
        {
            var item   = ItemOf(menu, "Navigation");
            var header = HeaderOf(item);
            menu.IsMotionEnabled.ShouldBeTrue();
            var duration = TimeSpan.FromMilliseconds(777);
            item.OpenCloseMotionDuration.ShouldNotBe(duration);

            provider.Config = WithControlToken(NavMenuTokens.Identity, "MotionDurationMid", duration.ToString("c"));
            WaitFor(() => item.OpenCloseMotionDuration == duration);
            item.OpenCloseMotionDuration.ShouldBe(duration);
            menu.ResolveInlineCollapsedWidthMotionDuration().ShouldBe(duration);
            ForegroundTransitionOf(header).ShouldBe(duration);

            var slow = TimeSpan.FromMilliseconds(999);
            provider.Config = WithControlToken(NavMenuTokens.Identity, "MotionDurationSlow", slow.ToString("c"));
            WaitFor(() => BackgroundTransitionOf(header) == slow);
            BackgroundTransitionOf(header).ShouldBe(slow);

            provider.Config = WithControlToken(NavMenuTokens.Identity, "EnableMotion", "false");
            WaitFor(() => !menu.IsMotionEnabled);
            menu.IsMotionEnabled.ShouldBeFalse();
        });

        static TimeSpan? TransitionOf(Animatable target, AvaloniaProperty property) =>
            target.Transitions?.OfType<TransitionBase>().FirstOrDefault(t => t.Property == property)?.Duration;

        static TimeSpan? ForegroundTransitionOf(BaseNavMenuItemHeader header) =>
            TransitionOf(header, TemplatedControl.ForegroundProperty);

        static TimeSpan? BackgroundTransitionOf(BaseNavMenuItemHeader header) =>
            TransitionOf(header, TemplatedControl.BackgroundProperty);
    }

    [Fact]
    public void NavMenu_Inline_Collapse_Motion_Reads_A_Local_Scope_Duration()
    {
        // E3: the inline-collapse width animation used Application.Current resources with a 200 ms fallback, ignoring a
        // local ThemeConfigProvider. It now reads the NavMenu's effective MotionDurationMid.
        var menu = CreateNavMenu(NavMenuMode.Inline);
        WithProvider(menu, provider =>
        {
            var duration = TimeSpan.FromMilliseconds(345);
            ControlTokenAccessor.Capture(menu).GetEffectiveGlobal<TimeSpan>(SharedTokenKind.MotionDurationMid)
                                .ShouldNotBe(duration);
            provider.Config = new AtomUI.Theme.Configuration.ThemeConfigBuilder()
                              .WithToken(nameof(SharedTokenKind.MotionDurationMid), duration.ToString("c"))
                              .Build();
            WaitFor(() => menu.ResolveInlineCollapsedWidthMotionDuration() == duration);
            menu.ResolveInlineCollapsedWidthMotionDuration().ShouldBe(duration);
        });
    }

    // ---- borrowed fix 473c43b1f: dark group titles ----------------------------------------------------------------

    [Fact]
    public void NavMenu_Dark_Group_Title_Renders_The_Dark_Title_Colour()
    {
        // 473c43b1f: the light title colour was written on the template node and masked the dark-style Setter. Since
        // Task 18 template Token reads are Template priority, so the dark Setter wins; this shows DarkGroupTitleColor on
        // a dark menu and GroupTitleColor not reaching it.
        var menu = CreateNavMenu(NavMenuMode.Inline, dark: true);
        WithProvider(menu, _ =>
        {
            var group = menu.GetVisualDescendants().OfType<NavMenuGroupItem>().First();
            var title = group.GetVisualDescendants().OfType<Avalonia.Controls.Presenters.ContentPresenter>()
                             .First(static presenter => presenter.Name == "PART_HeaderPresenter");
            var tokens = ControlTokenAccessor.Capture(menu);
            var dark   = tokens.GetOwn<Avalonia.Media.Color>(NavMenuTokenKind.DarkGroupTitleColor);
            var light  = tokens.GetOwn<Avalonia.Media.Color>(NavMenuTokenKind.GroupTitleColor);
            dark.ShouldNotBe(light);
            ((Avalonia.Media.ISolidColorBrush)title.Foreground!).Color.ShouldBe(dark);
        });
    }
}
