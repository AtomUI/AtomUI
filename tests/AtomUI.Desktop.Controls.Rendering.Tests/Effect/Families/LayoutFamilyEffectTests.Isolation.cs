using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Theme;
using AtomUI.Theme.Configuration;
using AtomUI.Theme.Schema;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using SkiaSharp;
using Xunit;

namespace AtomUI.Desktop.Controls.Rendering.Tests.Effect.Families;

/// <summary>
/// Scope isolation of the layout family (theme-effect-tests.md, family gate): a <c>Controls[X]</c> override reaches X only,
/// the root Token reaches X, the Token-owner transparent parts (CollapseItem, CardGridItem, CardTabsContent) follow their
/// host, and registered controls nested in a member (the tab control of a tabs card, card action buttons, the expand
/// buttons) keep their own identity.
/// </summary>
public partial class LayoutFamilyEffectTests
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

    [Fact]
    public void Controls_X_Border_Colours_Change_Only_X()
    {
        var card      = CreateCard();
        var collapse  = CreateCollapse();
        var expander  = CreateExpander();
        var groupBox  = CreateGroupBox();
        var separator = CreateSeparator();
        var splitView = CreateSplitView();
        splitView.Width  = 360;
        splitView.Height = 120;
        splitView.OpenPaneLength = 120;
        var left = new StackPanel { Spacing = 12, Children = { card, groupBox, separator } };
        var right = new StackPanel { Spacing = 12, Children = { collapse, expander, splitView } };
        var root = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 16, Children = { left, right } };
        WithIsolationWindow(root, (provider, capture) =>
        {
            var members = new (ControlTokenIdentity Identity, string Token, Control Control)[]
            {
                (CardTokens.Identity, "ColorBorderSecondary", card),
                (CollapseTokens.Identity, "ColorBorder", collapse),
                (ExpanderTokens.Identity, "ColorBorder", expander),
                (GroupBoxTokens.Identity, "ColorBorder", groupBox),
                (SeparatorTokens.Identity, "ColorSplit", separator),
                (SplitViewTokens.Identity, "ColorBorder", splitView)
            };
            foreach (var (identity, token, target) in members)
            {
                provider.Config = IsolationConfig();
                using var before = capture();
                provider.Config = IsolationConfig(control: (identity, token, IsolationColor));
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

                provider.Config = IsolationConfig(root: (token, IsolationColor));
                using var byRoot = capture();
                ChangedRatioOf(before, byRoot, target).ShouldBeGreaterThan(0d, $"Root {token} did not reach {target.GetType().Name}.");
            }
        });
    }

    [Fact]
    public void Collapse_Items_Follow_The_Collapse_And_Their_Expand_Buttons_Keep_Their_Identity()
    {
        var collapse = CreateCollapse();
        WithIsolationWindow(collapse, (provider, capture) =>
        {
            var item = collapse.GetVisualDescendants().OfType<CollapseItem>().First();
            ThemeScope.GetTokenOwner(item).ShouldBe(CollapseTokens.Identity);
            var expandButton = Descendant<IconButton>(item, "PART_ExpandButton");
            ThemeScope.GetTokenOwner(expandButton).ShouldBe(IconButtonTokens.Identity);
            using var before = capture();

            // CollapseItem has an identity but is Token-owner transparent: Controls[CollapseItem] reaches nothing.
            provider.Config = IsolationConfig(control: (CollapseItemTokens.Identity, "ColorTextHeading", IsolationColor));
            using var byItem = capture();
            ChangedRatioOf(before, byItem, item).ShouldBe(0d, "Controls[CollapseItem] reached a transparent collapse item.");

            provider.Config = IsolationConfig(control: (CollapseTokens.Identity, "ColorTextHeading", IsolationColor));
            using var byCollapse = capture();
            ChangedRatioOf(before, byCollapse, item).ShouldBeGreaterThan(0d, "Controls[Collapse] did not reach the item header.");

            provider.Config = IsolationConfig(control: (CollapseTokens.Identity, "ColorIcon", IsolationColor));
            using var byCollapseIcon = capture();
            ChangedRatioOf(before, byCollapseIcon, expandButton).ShouldBe(0d, "Controls[Collapse].ColorIcon reached the expand IconButton.");

            provider.Config = IsolationConfig(control: (IconButtonTokens.Identity, "ColorIcon", IsolationColor));
            using var byIconButton = capture();
            ((Avalonia.Media.ISolidColorBrush)expandButton.IconBrush!).Color
                .ShouldBe(Avalonia.Media.Color.Parse(IsolationColor), "Controls[IconButton].ColorIcon did not reach the expand button.");
        });
    }

    [Fact]
    public void Card_Parts_Follow_The_Card_And_Its_Nested_Controls_Keep_Their_Identity()
    {
        var tabsCard = CreateCard(content: CreateTabs());
        var gridCard = CreateCard(content: CreateGrid());
        var root = new StackPanel { Spacing = 16, Children = { tabsCard, gridCard } };
        WithIsolationWindow(root, (provider, capture) =>
        {
            var tabs       = Descendant<CardTabsContent>(tabsCard);
            var tabControl = Descendant<TabControl>(tabs);
            var gridItem   = Descendant<CardGridItem>(gridCard);
            var action     = Descendant<CardActionButton>(tabsCard);
            // The card body sits in the Skeleton, which is Token-owner transparent: the transparent parts inherit the Card.
            ThemeScope.GetTokenOwner(tabs).ShouldBe(CardTokens.Identity);
            ThemeScope.GetTokenOwner(gridItem).ShouldBe(CardTokens.Identity);
            ThemeScope.GetTokenOwner(tabControl).ShouldBe(TabControlTokens.Identity);
            ThemeScope.GetTokenOwner(action).ShouldBe(CardActionButtonTokens.Identity);
            using var before = capture();

            // The tab bar edge padding is the Card's (CardTabsContent reads the Card), the ink bar the TabControl's.
            provider.Config = IsolationConfig(control: (CardTokens.Identity, "UniformlyPaddingSM", "40"));
            using var byCardPadding = capture();
            ChangedRatioOf(before, byCardPadding, tabControl).ShouldBeGreaterThan(0d,
                "Controls[Card].UniformlyPaddingSM did not reach the tab bar edge padding.");

            provider.Config = IsolationConfig(control: (CardTokens.Identity, "ColorPrimary", IsolationColor));
            using var byCardPrimary = capture();
            ChangedRatioOf(before, byCardPrimary, tabControl).ShouldBe(0d, "Controls[Card].ColorPrimary reached the TabControl.");
            ChangedRatioOf(before, byCardPrimary, action).ShouldBe(0d, "Controls[Card].ColorPrimary reached a CardActionButton.");

            provider.Config = IsolationConfig(control: (TabControlTokens.Identity, "ColorPrimary", IsolationColor));
            using var byTabControl = capture();
            ChangedRatioOf(before, byTabControl, tabControl).ShouldBeGreaterThan(0d,
                "Controls[TabControl].ColorPrimary did not reach the card tabs.");

            // The grid item edges are the Card's grid shadows (LineWidth / ColorBorderSecondary of the Card).
            provider.Config = IsolationConfig(control: (CardTokens.Identity, "ColorBorderSecondary", IsolationColor));
            using var byCardBorder = capture();
            ChangedRatioOf(before, byCardBorder, gridItem).ShouldBeGreaterThan(0d,
                "Controls[Card].ColorBorderSecondary did not reach the grid item edges.");

            provider.Config = IsolationConfig(control: (CardActionButtonTokens.Identity, "ColorIcon", IsolationColor));
            using var byAction = capture();
            ChangedRatioOf(before, byAction, action).ShouldBeGreaterThan(0d,
                "Controls[CardActionButton].ColorIcon did not reach the card action.");
        });
    }

    [Fact]
    public void Space_Add_On_Tokens_And_Add_On_Globals_Reach_Only_The_Add_On()
    {
        var root = CreateSpaceHost();
        WithIsolationWindow(root, (provider, capture) =>
        {
            var addOn    = AddOnOf(root);
            var lineEdit = Descendant<LineEdit>(root);
            using var before = capture();

            // The add-on fill is a CompactSpaceAddOn Own Token (moved from Space, which no longer declares AddonBg);
            // the add-on border a CompactSpaceAddOn global.
            provider.Config = IsolationConfig(control: (CompactSpaceAddOnTokens.Identity, "AddonBg", IsolationColor));
            using var byAddOnBg = capture();
            ChangedRatioOf(before, byAddOnBg, addOn).ShouldBeGreaterThan(0d, "Controls[CompactSpaceAddOn].AddonBg did not reach the add-on.");
            ChangedRatioOf(before, byAddOnBg, lineEdit).ShouldBe(0d, "Controls[CompactSpaceAddOn].AddonBg leaked into the LineEdit.");

            provider.Config = IsolationConfig(control: (CompactSpaceAddOnTokens.Identity, "ColorBorder", IsolationColor));
            using var byAddOn = capture();
            ChangedRatioOf(before, byAddOn, addOn).ShouldBeGreaterThan(0d,
                "Controls[CompactSpaceAddOn].ColorBorder did not reach the add-on.");
            ChangedRatioOf(before, byAddOn, lineEdit).ShouldBe(0d, "Controls[CompactSpaceAddOn].ColorBorder leaked into the LineEdit.");
        });
    }

    [Fact]
    public void Motion_Properties_Follow_The_Member_Tokens()
    {
        // TimeSpan / bool rows assert read-backs; these check the members actually consume them.
        var card     = CreateCard();
        var collapse = CreateCollapse();
        var expander = CreateExpander();
        var masonry  = new Masonry { Width = 200 };
        var root = new StackPanel { Spacing = 8, Children = { card, collapse, expander, masonry } };
        FeedbackFamilyEffectTests.WithProvider(root, provider =>
        {
            var item = collapse.GetVisualDescendants().OfType<CollapseItem>().First();
            card.IsMotionEnabled.ShouldBeTrue();
            collapse.IsMotionEnabled.ShouldBeTrue();
            expander.IsMotionEnabled.ShouldBeTrue();

            provider.Config = new ThemeConfigBuilder()
                              .WithControl(CardTokens.Identity, new ControlThemeConfigBuilder().WithToken("EnableMotion", "false").Build())
                              .WithControl(CollapseTokens.Identity, new ControlThemeConfigBuilder()
                                                                     .WithToken("MotionDurationSlow", "00:00:00.7000000").Build())
                              .WithControl(ExpanderTokens.Identity, new ControlThemeConfigBuilder()
                                                                     .WithToken("MotionDurationSlow", "00:00:00.6000000").Build())
                              .WithControl(MasonryTokens.Identity, new ControlThemeConfigBuilder()
                                                                    .WithToken("MotionDurationSlow", "00:00:00.5000000")
                                                                    .WithToken("MotionDurationFast", "00:00:00.0500000").Build())
                              .Build();
            WaitFor(() => !card.IsMotionEnabled && masonry.LeaveMotionDuration == TimeSpan.FromMilliseconds(50));
            card.IsMotionEnabled.ShouldBeFalse();
            collapse.IsMotionEnabled.ShouldBeTrue();
            item.MotionDuration.ShouldBe(TimeSpan.FromMilliseconds(700));
            expander.MotionDuration.ShouldBe(TimeSpan.FromMilliseconds(600));
            expander.IsMotionEnabled.ShouldBeTrue();
            masonry.MotionDuration.ShouldBe(TimeSpan.FromMilliseconds(500));
            masonry.LeaveMotionDuration.ShouldBe(TimeSpan.FromMilliseconds(50));

            // EnableMotion off for the Collapse alone: its items stop animating (motion off zeroes the durations).
            provider.Config = new ThemeConfigBuilder()
                              .WithControl(CollapseTokens.Identity, new ControlThemeConfigBuilder().WithToken("EnableMotion", "false").Build())
                              .Build();
            WaitFor(() => !collapse.IsMotionEnabled);
            collapse.IsMotionEnabled.ShouldBeFalse();
            item.IsMotionEnabled.ShouldBeFalse();
            item.MotionDuration.ShouldBe(TimeSpan.Zero);
            expander.IsMotionEnabled.ShouldBeTrue();
            Dispatcher.UIThread.RunJobs();
        });
    }
}
