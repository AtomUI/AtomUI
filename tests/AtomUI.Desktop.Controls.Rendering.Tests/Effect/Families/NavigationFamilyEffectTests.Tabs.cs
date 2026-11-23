using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Icons.AntDesign;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AvaloniaTextBlock = Avalonia.Controls.TextBlock;

namespace AtomUI.Desktop.Controls.Rendering.Tests.Effect.Families;

/// <summary>
/// The tab controls: TabControl, CardTabControl, TabStrip and CardTabStrip, with their TabItem / TabStripItem items. The
/// Own Tokens are TabControl's and every tab theme reads them through <c>TabControlTokenResource</c>, so TabControl rows that
/// style card tabs or strips put a TabControl (the identity under test) beside the member that draws the Token and compare
/// that member. The identity-less parts (the BaseTabControl / BaseTabStrip base themes, the tab scroll viewer and the
/// overflow menu) read their host's globals through <c>OwnerTokenResource</c>; each host runs those owner reads as rows.
/// </summary>
public partial class NavigationFamilyEffectTests
{
    private const string TabThemes = "src/AtomUI.Desktop.Controls/TabControl/Themes/";

    internal static readonly FeedbackFamilyEffectTests.OwnerReadList s_baseTabControl =
        new(TabThemes + "BaseTabControlTheme.axaml", ["BorderThickness", "ColorBorderSecondary", "EnableMotion"],
            "src/AtomUI.Desktop.Controls/TabControl/BaseTabControl.cs");

    internal static readonly FeedbackFamilyEffectTests.OwnerReadList s_baseTabStrip =
        new(TabThemes + "TabStrip/BaseTabStripTheme.axaml", ["BorderThickness", "ColorBorderSecondary", "EnableMotion"]);

    internal static readonly FeedbackFamilyEffectTests.OwnerReadList s_tabScrollViewer =
        new(TabThemes + "TabScrollViewerTheme.axaml", ["ColorTextSecondary", "IconSize"]);

    internal static readonly FeedbackFamilyEffectTests.OwnerReadList s_tabOverflowMenu =
        new(TabThemes + "TabOverflowMenuTheme.axaml", ["ColorBgElevated", "ColorPrimary", "EnableMotion", "IconSizeSM"]);

    [Fact]
    public void Tab_Owner_Read_Lists_Match_The_Part_Themes()
    {
        foreach (var list in new[] { s_baseTabControl, s_baseTabStrip, s_tabScrollViewer, s_tabOverflowMenu })
        {
            ThemeOwnerReads.Read(list.Theme).Union(ThemeOwnerReads.ReadCode(list.Code)).Order(StringComparer.Ordinal)
                           .ShouldBe(list.Tokens.Order(StringComparer.Ordinal), list.Theme);
        }
    }

    /// <summary>Consumption-map rows of a tab host plus the owner reads of the identity-less parts it carries.</summary>
    private static IEnumerable<object[]> TabHostRows(string controlId, params FeedbackFamilyEffectTests.OwnerReadList[] parts)
    {
        var rows = MapRows(controlId).ToList();
        var seen = new HashSet<string>(rows.Select(static row => (string)row[0]), StringComparer.Ordinal);
        foreach (var token in parts.SelectMany(static list => list.Tokens).Distinct(StringComparer.Ordinal)
                                   .Order(StringComparer.Ordinal))
        {
            if (seen.Add(token))
            {
                rows.Add([token, false]);
            }
        }

        return rows;
    }

    public enum TabKind
    {
        Line,
        Card,
        Strip,
        CardStrip
    }

    /// <summary>
    /// A tab host with an icon tab (selected), a plain tab, a closable tab and a disabled tab; <paramref name="count"/>
    /// tabs in a narrow host overflow and show the overflow-menu button and edge shadows.
    /// </summary>
    internal static Control CreateTabs(
        TabKind kind,
        Dock placement = Dock.Top,
        SizeType size = SizeType.Middle,
        int count = 4,
        double width = 520,
        bool addButton = false,
        bool extraContent = false)
    {
        Control host = kind switch
        {
            TabKind.Line      => new TabControl(),
            TabKind.Card      => new CardTabControl { IsShowAddTabButton = addButton },
            TabKind.Strip     => new TabStrip(),
            _                 => new CardTabStrip { IsShowAddTabButton = addButton }
        };
        var items = (ItemsControl)host;
        host.Width               = width;
        host.Height              = placement is Dock.Left or Dock.Right ? 260 : double.NaN;
        host.HorizontalAlignment = HorizontalAlignment.Left;
        host.VerticalAlignment   = VerticalAlignment.Top;
        for (var index = 0; index < count; index++)
        {
            var header = index switch
            {
                0 => "Tab 1",
                2 => "Closable",
                3 => "Disabled",
                _ => $"Tab {index + 1}"
            };
            if (kind is TabKind.Line or TabKind.Card)
            {
                var item = new TabItem { Header = header, Content = $"Content of {header}" };
                if (index == 0)
                {
                    item.Icon = new SearchOutlined();
                }
                item.IsClosable = index == 2;
                item.IsAutoHideCloseButton = false;
                item.IsEnabled = index != 3;
                items.Items.Add(item);
            }
            else
            {
                var item = new TabStripItem { Content = header };
                if (index == 0)
                {
                    item.Icon = new SearchOutlined();
                }
                item.IsClosable = index == 2;
                item.IsAutoHideCloseButton = false;
                item.IsEnabled = index != 3;
                items.Items.Add(item);
            }
        }

        switch (host)
        {
            case BaseTabControl tabControl:
                tabControl.TabStripPlacement = placement;
                tabControl.SizeType          = size;
                tabControl.SelectedIndex     = 0;
                if (extraContent)
                {
                    tabControl.HeaderStartExtraContent = new AvaloniaTextBlock { Text = "Start" };
                    tabControl.HeaderEndExtraContent   = new AvaloniaTextBlock { Text = "End" };
                }
                break;
            case BaseTabStrip tabStrip:
                tabStrip.TabStripPlacement = placement;
                tabStrip.SizeType          = size;
                tabStrip.SelectedIndex     = 0;
                if (extraContent)
                {
                    tabStrip.HeaderStartExtraContent = new AvaloniaTextBlock { Text = "Start" };
                    tabStrip.HeaderEndExtraContent   = new AvaloniaTextBlock { Text = "End" };
                }
                break;
        }

        return host;
    }

    /// <summary>The tab (TabItem or TabStripItem) whose header or content is <paramref name="header"/>.</summary>
    internal static Control TabOf(Control host, string header) =>
        host.GetVisualDescendants().OfType<Control>()
            .First(control => control switch
            {
                TabItem item      => Equals(item.Header, header),
                TabStripItem item => Equals(item.Content, header),
                _                 => false
            });

    private static void ScrollToMiddle(Control host)
    {
        var viewer = host.GetVisualDescendants().OfType<Avalonia.Controls.ScrollViewer>()
                         .First(static control => control.GetType().Name == "TabScrollViewer");
        TopLevel.GetTopLevel(host)!.UpdateLayout();
        viewer.Offset = new Vector((viewer.Extent.Width - viewer.Viewport.Width) / 2,
            (viewer.Extent.Height - viewer.Viewport.Height) / 2);
        TopLevel.GetTopLevel(host)!.UpdateLayout();
    }

    /// <summary>
    /// An overflowing host whose tabs are all closable and whose selected tab (Tab 6) is hidden, so the opened overflow menu
    /// lists it as selected with close buttons; <paramref name="region"/> picks the compared part of the menu.
    /// </summary>
    private static NavScenario Overflow(TabKind kind, Func<Control, Control> region) =>
        new(() =>
            {
                var host = CreateTabs(kind, count: 20, width: 360);
                foreach (var item in ((ItemsControl)host).Items.OfType<Control>())
                {
                    switch (item)
                    {
                        case TabItem tab:
                            tab.IsClosable = true;
                            break;
                        case TabStripItem tab:
                            tab.IsClosable = true;
                            break;
                    }
                }
                switch (host)
                {
                    case BaseTabControl tabControl:
                        tabControl.SelectedIndex = 5;
                        break;
                    case BaseTabStrip tabStrip:
                        tabStrip.SelectedIndex = 5;
                        break;
                }
                return OverlayLayerHost(host);
            },
            static host => OpenOverflow(host),
            new EffectOptions
            {
                Target = static root => root.GetVisualDescendants().OfType<Control>().First(static c => c is BaseTabControl or BaseTabStrip),
                Region = region
            });

    private static Control OverflowMenuPart(Control host, string token) =>
        token == "ColorPrimary"
            ? WaitForVisual(() => OverflowMenuOf(host).GetVisualDescendants().OfType<TabOverflowMenuItem>()
                                                      .FirstOrDefault(static item => item.IsSelected), "The selected overflow menu item")
            : OverflowMenuOf(host);

    private static Control ScrollViewerOf(Control host) =>
        host.GetVisualDescendants().OfType<Avalonia.Controls.ScrollViewer>()
            .First(static control => control.GetType().Name == "TabScrollViewer");

    private static Control MenuIndicatorOf(Control host) => Descendant<IconButton>(host, "PART_ScrollMenuIndicator");

    private static void OpenOverflow(Control host)
    {
        switch (host)
        {
            case BaseTabControl tabControl:
                tabControl.IsPopupPinnedOpen = true;
                break;
            case BaseTabStrip tabStrip:
                tabStrip.IsPopupPinnedOpen = true;
                break;
        }
        OverflowMenuOf(host);
    }

    private static Control OverflowMenuOf(Control anyInWindow) =>
        WaitForVisual(() => TopLevel.GetTopLevel(anyInWindow)!.GetVisualDescendants().OfType<TabOverflowMenu>()
                                    .FirstOrDefault(static menu => menu.IsEffectivelyVisible),
            "The tab overflow menu");

    /// <summary>
    /// The shared arrange of a host's global and owner-read rows: line / card tab hosts and strips draw the same parts.
    /// </summary>
    private static NavScenario TabHostScenario(TabKind kind, string token)
    {
        switch (token)
        {
            // The tab scroll viewer: the overflow-menu button of an overflowing host.
            case "ColorTextSecondary" or "IconSize" when kind is TabKind.Line or TabKind.Strip:
                return Tabs(kind, null, count: 20, width: 360, options: new EffectOptions { Region = MenuIndicatorOf });
            case "ColorTextSecondary":
                return Tabs(kind, null, count: 20, width: 360, options: new EffectOptions { Region = MenuIndicatorOf });
            // The overflow menu opened in its popup.
            case "ColorBgElevated" or "ColorPrimary" or "IconSizeSM" when kind is not TabKind.Line:
                return Overflow(kind, host => OverflowMenuPart(host, token));
            case "SpacingXS":
                return Tabs(kind, null, extraContent: true);
            case "BorderRadius" or "BorderRadiusMD" or "BorderRadiusLG" or "BorderRadiusSM":
                // The card corners: a clearly rounder radius on the selected card.
                var size = token switch
                {
                    "BorderRadiusLG" => SizeType.Large,
                    "BorderRadiusSM" => SizeType.Small,
                    _                => SizeType.Middle
                };
                return new NavScenario(() => CreateTabs(kind, size: size), null,
                    new EffectOptions
                    {
                        Region  = static host => TabOf(host, "Tab 1"),
                        Perturb = static _ => new CornerRadius(16)
                    });
            case "IconSize":
                // The card add-tab button icon.
                return Tabs(kind, null, addButton: true,
                    options: new EffectOptions { Region = static host => Descendant<IconButton>(host, "PART_AddTabButton") });
            default:
                return Tabs(kind, null, addButton: kind is TabKind.Card or TabKind.CardStrip);
        }
    }

    private static NavScenario Tabs(
        TabKind kind,
        Action<Control>? arrange,
        Dock placement = Dock.Top,
        SizeType size = SizeType.Middle,
        int count = 4,
        double width = 520,
        bool addButton = false,
        bool extraContent = false,
        EffectOptions? options = null) =>
        new(() => CreateTabs(kind, placement, size, count, width, addButton, extraContent), arrange, options);

    // ---- TabControl ---------------------------------------------------------------------------

    public static IEnumerable<object[]> TabControlRows() =>
        TabHostRows("TabControl", s_baseTabControl, s_tabScrollViewer, s_tabOverflowMenu);

    [Theory]
    [MemberData(nameof(TabControlRows))]
    public void TabControl_Token_Has_Effect(string token, bool isOwn)
    {
        _ = isOwn;
        var scenario = TabControlScenario(token);
        Run(scenario.Create, scenario.Arrange, TabControlTokens.Identity, token, scenario.Options);
    }

    private static NavScenario TabControlScenario(string token, TabKind kind = TabKind.Line)
    {
        if (kind is TabKind.Card or TabKind.CardStrip && token == "UniformlyPaddingXS")
        {
            return Tabs(kind, null, placement: Dock.Left);
        }
        switch (token)
        {
            case "BorderRadius" or "BorderRadiusMD" or "BorderRadiusLG" or "BorderRadiusSM":
                return TabHostScenario(kind, token);
            // ---- card tabs and card strips draw the card Own Tokens: compare the card host -------------------------
            case "CardBg" or "ColorFillAlter" or "CardSize" or "ControlHeightLG" or "CardPadding" or "RelativeLineHeight" or
                "LineWidth" or "CardGutter":
                return Tabs(kind, null, addButton: true);
            case "AddTabButtonMarginHorizontal":
                return Tabs(kind, null, addButton: true, options: new EffectOptions { Region = AddButtonOf });
            case "CardPaddingSM" or "UniformlyPaddingXXS":
                return Tabs(kind, null, size: SizeType.Small);
            case "CardPaddingLG":
                return Tabs(kind, null, size: SizeType.Large);
            case "VerticalItemPadding":
                return Tabs(kind, null, placement: Dock.Left);
            case "AddTabButtonMarginVertical":
                return Tabs(kind, null, placement: Dock.Left, addButton: true, options: new EffectOptions { Region = AddButtonOf });

            // ---- line tabs --------------------------------------------------------------------------------------
            case "ItemHoverColor" or "ColorPrimaryHover":
                return Tabs(kind, static host => Pseudo(TabOf(host, "Tab 2"), ":pointerover"),
                    options: new EffectOptions { Region = static host => TabOf(host, "Tab 2") });
            case "ItemSelectedColor" or "ColorPrimary":
                return Tabs(kind, null, options: new EffectOptions { Region = static host => TabOf(host, "Tab 1") });
            case "InkBarColor" or "InkBarThickness" or "LineWidthBold":
                return Tabs(kind, null,
                    options: new EffectOptions { Region = static host => Descendant<Avalonia.Controls.Border>(host, "PART_SelectedItemIndicator") });
            case "TitleFontSizeLG" or "FontSizeLG" or "HorizontalItemPaddingLG" or "UniformlyPadding":
                return Tabs(kind, null, size: SizeType.Large);
            case "TitleFontSizeSM" or "HorizontalItemPaddingSM" or "UniformlyPaddingXS":
                return Tabs(kind, null, size: SizeType.Small);
            case "VerticalItemGutter" or "SpacingXXS":
                return Tabs(kind, null, placement: Dock.Left);
            case "CloseIconMargin" or "UniformlyMarginXXS":
                return Tabs(kind, null, options: new EffectOptions { Region = static host => TabOf(host, "Closable") });
            case "ItemIconMargin" or "UniformlyMarginSM":
                return Tabs(kind, null, options: new EffectOptions { Region = static host => TabOf(host, "Tab 1") });

            // ---- overflow: menu button, edge shadows ----------------------------------------------------------------
            // Each shadow is compared around its caster; negative offsets perturb outward (away from the caster) so the
            // shadow reaches further into the tabs.
            case "BoxShadowTabsOverflowLeft":
                return EdgeShadow(Dock.Top, start: true, "18 0 16 -8 #29000000", kind);
            case "BoxShadowTabsOverflowRight":
                return EdgeShadow(Dock.Top, start: false, "-18 0 16 -8 #29000000", kind);
            case "BoxShadowTabsOverflowTop":
                return EdgeShadow(Dock.Left, start: true, "0 18 16 -8 #29000000", kind);
            case "BoxShadowTabsOverflowBottom":
                return EdgeShadow(Dock.Left, start: false, "0 -18 16 -8 #29000000", kind);
            case "MenuIndicatorPaddingHorizontal":
                return Tabs(kind, null, count: 20, width: 360, options: new EffectOptions { Region = MenuIndicatorOf });
            case "MenuIndicatorPaddingVertical":
                return Tabs(kind, null, placement: Dock.Left, count: 20, options: new EffectOptions { Region = MenuIndicatorOf });
            case "ColorBgElevated" or "IconSizeSM":
                return Overflow(kind, host => OverflowMenuPart(host, token));

            // ---- the line host's separator, header extra content, scroll viewer -----------------------------------
            case "BorderThickness" or "ColorBorderSecondary" or "SpacingXS" or "ColorTextSecondary" or "IconSize":
                return TabHostScenario(kind, token);

            default:
                return Tabs(kind, null);
        }
    }

    /// <summary>A TabControl (the identity under test) beside a card or strip host that draws the Token; the latter is compared.</summary>
    private static Control AddButtonOf(Control host) => Descendant<IconButton>(host, "PART_AddTabButton");

    /// <summary>A scrolled-to-middle overflowing line TabControl; the start or end edge shadow caster is compared.</summary>
    private static NavScenario EdgeShadow(Dock placement, bool start, string perturbed, TabKind kind = TabKind.Line) =>
        Tabs(kind, static host => ScrollToMiddle(host), placement: placement, count: 20,
            width: placement is Dock.Top or Dock.Bottom ? 360 : 520,
            options: new EffectOptions
            {
                Region  = host => Descendant<TabOverflowEdgeIndicator>(host,
                    start ? "PART_ScrollStartEdgeIndicator" : "PART_ScrollEndEdgeIndicator"),
                Perturb = _ => Avalonia.Media.BoxShadows.Parse(perturbed)
            });

    private static NavScenario Beside(
        TabKind kind,
        Dock placement = Dock.Top,
        SizeType size = SizeType.Middle,
        bool addButton = false,
        Func<Control, Control>? region = null) =>
        new(() => new StackPanel
            {
                Spacing  = 16,
                Children = { CreateTabs(TabKind.Line, width: 200, count: 2), CreateTabs(kind, placement, size, addButton: addButton) }
            },
            null,
            new EffectOptions
            {
                Target = static root => root.GetVisualDescendants().OfType<TabControl>().First(),
                Region = tabControl => region is null
                    ? ((StackPanel)tabControl.Parent!).Children[1]
                    : region(((StackPanel)tabControl.Parent!).Children[1])
            });

    // ---- CardTabControl / TabStrip / CardTabStrip ---------------------------------------------------------------

    public static IEnumerable<object[]> CardTabControlRows() =>
        TabHostRows("CardTabControl", s_baseTabControl, s_tabScrollViewer, s_tabOverflowMenu);

    [Theory]
    [MemberData(nameof(CardTabControlRows))]
    public void CardTabControl_Token_Has_Effect(string token, bool isOwn)
    {
        _ = isOwn;
        var scenario = TabControlScenario(token, TabKind.Card);
        Run(scenario.Create, scenario.Arrange, CardTabControlTokens.Identity, token, scenario.Options);
    }

    public static IEnumerable<object[]> TabStripRows() =>
        TabHostRows("TabStrip", s_baseTabStrip, s_tabScrollViewer, s_tabOverflowMenu);

    [Theory]
    [MemberData(nameof(TabStripRows))]
    public void TabStrip_Token_Has_Effect(string token, bool isOwn)
    {
        _ = isOwn;
        var scenario = TabControlScenario(token, TabKind.Strip);
        Run(scenario.Create, scenario.Arrange, TabStripTokens.Identity, token, scenario.Options);
    }

    public static IEnumerable<object[]> CardTabStripRows() =>
        TabHostRows("CardTabStrip", s_baseTabStrip, s_tabScrollViewer, s_tabOverflowMenu);

    [Theory]
    [MemberData(nameof(CardTabStripRows))]
    public void CardTabStrip_Token_Has_Effect(string token, bool isOwn)
    {
        _ = isOwn;
        var scenario = TabControlScenario(token, TabKind.CardStrip);
        Run(scenario.Create, scenario.Arrange, CardTabStripTokens.Identity, token, scenario.Options);
    }

    // ---- TabItem / TabStripItem ---------------------------------------------------------------------------------

    public static IEnumerable<object[]> TabItemRows() => MapRows("TabItem");

    [Theory]
    [MemberData(nameof(TabItemRows))]
    public void TabItem_Token_Has_Effect(string token, bool isOwn)
    {
        _ = isOwn;
        var scenario = TabItemScenario(token, TabKind.Line, TabKind.Card);
        Run(scenario.Create, scenario.Arrange, TabItemTokens.Identity, token, scenario.Options);
    }

    public static IEnumerable<object[]> TabStripItemRows() => MapRows("TabStripItem");

    [Theory]
    [MemberData(nameof(TabStripItemRows))]
    public void TabStripItem_Token_Has_Effect(string token, bool isOwn)
    {
        _ = isOwn;
        var scenario = TabItemScenario(token, TabKind.Strip, TabKind.CardStrip);
        Run(scenario.Create, scenario.Arrange, TabStripItemTokens.Identity, token, scenario.Options);
    }

    /// <summary>The tab under test is one item of a line or card host, compared on its own bounds.</summary>
    private static NavScenario TabItemScenario(string token, TabKind line, TabKind card)
    {
        var (kind, header, size, arrange) = token switch
        {
            // Card tabs: the selected card fill, the card border, the disabled card fill, the line mask.
            "ColorBgContainer" or "ColorBorderSecondary" or "LineWidth" => (card, "Tab 1", SizeType.Middle, (Action<Control>?)null),
            "ColorBgContainerDisabled"                                  => (card, "Disabled", SizeType.Middle, null),
            "ColorPrimaryActive"                                        => (line, "Tab 2", SizeType.Middle, static tab => Pseudo(tab, ":pressed")),
            "ColorTextDisabled"                                         => (line, "Disabled", SizeType.Middle, null),
            "IconSizeSM"                                                => (line, "Closable", SizeType.Middle, null),
            _                                                           => (line, "Tab 1", SizeType.Middle, null)
        };
        return new NavScenario(() => CreateTabs(kind, size: size), arrange,
            new EffectOptions
            {
                Target = root => TabOf(root, header),
                // The selected card's line mask (LineWidth high) hides the card's bottom border line; a thicker mask paints
                // over the white card, so the line width shows when the mask drops below the border and the line reappears.
                Perturb = token == "LineWidth" ? static _ => 0d : null
            });
    }
}
