using Avalonia.Headless;
using AtomUI.Controls;
using AtomUI.Controls.DesignTokens;
using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Icons.AntDesign;
using AtomUI.Theme;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.VisualTree;
using Avalonia.Controls.Primitives;
using Shouldly;
using Xunit;

namespace AtomUI.Desktop.Controls.Rendering.Tests.Effect.Families;

public partial class PrimitivesFamilyEffectTests
{
    private static readonly string[] SplashGlobals = ["ColorBgContainer", "BorderRadiusLG", "BoxShadowsSecondary", "PaddingLG",
        "FontSizeHeading4", "ColorText", "FontSize", "ColorTextSecondary", "ColorPrimary", "ColorFillSecondary", "ColorIcon", "ColorError", "LineWidth", "SpacingSM", "IconSizeSM", "Margin"];

    [Fact]
    public void Primitive_Owner_Read_Inventory_Does_Not_Drift()
    {
        ThemeOwnerReads.Read("src/AtomUI.Controls/Splash/Themes/SplashDefaultTheme.axaml").Order()
            .ShouldBe(SplashGlobals.Order());
        ThemeOwnerReads.Read("src/AtomUI.Controls/Icon/Themes/PathIconTheme.axaml").Order()
            .ShouldBe(new[] { "ColorIcon", "IconSize" }.Order());
        ThemeOwnerReads.Read("src/AtomUI.Controls/TextSelectionHandle/Themes/TextSelectionHandleTheme.axaml")
            .ShouldBe(new HashSet<string> { "SelectionBackground" });
        ThemeOwnerReads.Read("src/AtomUI.Controls/Embedding/Themes/EmbeddableControlRootTheme.axaml").Order()
            .ShouldBe(new[] { "ColorBgContainer", "ColorText", "FontFamily", "FontSize" }.Order());
        ThemeOwnerReads.Read("src/AtomUI.Desktop.Controls/Primitives/Themes/WaveSpiritDecoratorTheme.axaml")
            .Union(ThemeOwnerReads.ReadCode("src/AtomUI.Controls/Primitives/WaveSpiritDecorator.cs")).Order()
            .ShouldBe(new[] { "ColorPrimary", "WaveStartOpacity", "MotionDurationSlow", "WaveAnimationRange" }.Order());
    }

    public static IEnumerable<object[]> SplashRows() => SplashGlobals.Select(static token => new object[] { token });

    [Theory, MemberData(nameof(SplashRows))]
    public void Splash_Owner_Token_Has_Effect(string token)
    {
        TokenEffectHarness.AssertHasEffect(() => new ArrowDecoratedBox
        {
            Background = Brushes.Gray,
            Content = new Splash
            {
                Theme = (ControlTheme)new SplashDefaultTheme()[typeof(Splash)]!,
                Title = "Starting application", Message = "Preparing resources", Progress = .5, IsIndeterminate = false,
                Status = token is "ColorError" or "ColorIcon" or "LineWidth" or "IconSizeSM" or "Margin" ? SplashStatus.Error : SplashStatus.Loading
            }
        }, ArrowDecoratedBoxTokens.Identity, token, options: new EffectOptions
        {
            Region = c => token is "ColorIcon" or "LineWidth" or "IconSizeSM" or "Margin" ? c.GetVisualDescendants().OfType<Avalonia.Controls.Shapes.Path>().First(p => p.Name == "PART_CloseButton") : c.GetVisualDescendants().OfType<Splash>().Single(),
            Perturb = token == "BorderRadiusLG" ? _ => new CornerRadius(40) : null,
            RegionInflation = token == "BoxShadowsSecondary" ? 50 : 0
        });
    }

    [Fact]
    public void Indicator_Scrollbar_Tracks_The_Actual_Extent_And_Viewport()
    {
        var viewer = new AtomUI.Desktop.Controls.Primitives.IndicatorScrollViewer
        {
            Width = 240, Height = 160,
            Content = new Border { Height = 650, Width = 400, Background = Brushes.AliceBlue }
        };
        FeedbackFamilyEffectTests.WithProvider(viewer, _ =>
        {
            var bar = viewer.GetVisualDescendants().OfType<AtomUI.Desktop.Controls.Primitives.IndicatorScrollBar>().Single();
            bar.Maximum.ShouldBe(viewer.Extent.Height - viewer.Viewport.Height, 0.01);
            bar.ViewportSize.ShouldBe(viewer.Viewport.Height, 0.01);
        });
    }

    public static IEnumerable<object[]> AddOnRows() => EffectTestData.For("AtomUI", "WindowTitleBarButton");
    public static IEnumerable<object[]> ToggleAddOnRows() => EffectTestData.For("AtomUI", "WindowTitleBarToggleButton");

    [Theory, MemberData(nameof(AddOnRows))]
    public void TitleBar_AddOn_Token_Has_Effect(string token, bool own)
    {
        _ = own;
        TokenEffectHarness.AssertHasEffect(() => new WindowTitleBarButton { Icon = new CheckOutlined(), IsEnabled = token != "ColorTextDisabled", IsWindowActive = !token.StartsWith("Inactive") && token is not "ColorTextQuaternary" and not "ColorFillQuaternary", HostOsType = token is "CaptionButtonCornerRadius" or "CaptionButtonBackgroundInset" ? OsType.Linux : OsType.Unknown },
            WindowTitleBarButtonTokens.Identity, token, control =>
            {
                if (token.Contains("Pressed") || token == "ColorFill")
                {
                    ((Avalonia.Controls.IPseudoClasses)control.Classes).Add(":pressed");
                }
                else if (token.Contains("Hover") || token == "ColorFillSecondary")
                {
                    var topLevel = TopLevel.GetTopLevel(control)!;
                    topLevel.MouseMove(control.TranslatePoint(new Point(control.Bounds.Width / 2, control.Bounds.Height / 2), topLevel)!.Value);
                    control.IsPointerOver.ShouldBeTrue();
                }
            }, options: new EffectOptions
            { Perturb = token == "BorderRadiusSM" ? _ => new CornerRadius(24) : token == "CaptionButtonCornerRadius" ? _ => new CornerRadius(0) : null });
    }

    [Theory, MemberData(nameof(ToggleAddOnRows))]
    public void TitleBar_Toggle_AddOn_Token_Has_Effect(string token, bool own)
    {
        _ = own;
        TokenEffectHarness.AssertHasEffect(() => new WindowTitleBarToggleButton { UnCheckedIcon = new CheckOutlined(), IsEnabled = token != "ColorTextDisabled", IsWindowActive = !token.StartsWith("Inactive") && token is not "ColorTextQuaternary" and not "ColorFillQuaternary", HostOsType = token is "CaptionButtonCornerRadius" or "CaptionButtonBackgroundInset" ? OsType.Linux : OsType.Unknown },
            WindowTitleBarToggleButtonTokens.Identity, token, control =>
            {
                if (token.Contains("Pressed") || token == "ColorFill")
                {
                    ((Avalonia.Controls.IPseudoClasses)control.Classes).Add(":pressed");
                }
                else if (token.Contains("Hover") || token == "ColorFillSecondary")
                {
                    var topLevel = TopLevel.GetTopLevel(control)!;
                    topLevel.MouseMove(control.TranslatePoint(new Point(control.Bounds.Width / 2, control.Bounds.Height / 2), topLevel)!.Value);
                    control.IsPointerOver.ShouldBeTrue();
                }
            }, options: new EffectOptions
            { Perturb = token == "BorderRadiusSM" ? _ => new CornerRadius(24) : token == "CaptionButtonCornerRadius" ? _ => new CornerRadius(0) : null });
    }

    [Fact]
    public void Scroll_Motion_Uses_Its_Actual_Host_Through_Transparent_Bars_And_Thumbs()
    {
        var viewer = new ScrollViewer { Width = 260, Height = 160, AllowAutoHide = false, Content = new Border { Width = 600, Height = 650 } };
        FeedbackFamilyEffectTests.WithProvider(viewer, provider =>
        {
            var bars = viewer.GetVisualDescendants().OfType<ScrollBar>().ToArray();
            var thumbs = viewer.GetVisualDescendants().OfType<ScrollBarThumb>().ToArray();
            bars.ShouldNotBeEmpty();
            thumbs.ShouldNotBeEmpty();
            foreach (var part in bars.Cast<Control>().Concat(thumbs)) { ThemeScope.GetTokenOwner(part).ShouldBe(ScrollViewerTokens.Identity); }
            provider.Config = FeedbackFamilyEffectTests.WithControlToken(ScrollViewerTokens.Identity, "MotionDurationMid", "00:00:00.7000000");
            FeedbackFamilyEffectTests.WaitFor(() => thumbs.All(t => t.Transitions is { Count: > 0 } && t.Transitions.OfType<Avalonia.Animation.TransitionBase>().All(tr => tr.Duration == TimeSpan.FromMilliseconds(700))));
            foreach (var thumb in thumbs)
            {
                thumb.Transitions.ShouldNotBeNull().OfType<Avalonia.Animation.TransitionBase>().ShouldAllBe(t => t.Duration == TimeSpan.FromMilliseconds(700));
            }
            provider.Config = FeedbackFamilyEffectTests.WithControlToken(ScrollViewerTokens.Identity, "EnableMotion", "false");
            FeedbackFamilyEffectTests.WaitFor(() => bars.All(b => !b.IsMotionEnabled) && thumbs.All(t => !t.IsMotionEnabled));
            bars.ShouldAllBe(b => !b.IsMotionEnabled);
            thumbs.ShouldAllBe(t => !t.IsMotionEnabled);
        });
    }

    [Fact]
    public void Known_Zero_Map_Parts_Remain_Explicit_In_The_Family_Inventory()
    {
        // These parts render caller-provided values and have no Own/global Token promise rows.
        // New consumption must be deliberately added to the effect matrix rather than silently ignored.
        foreach (var id in new[] { "AsyncImage", "ItemsControl", "TransitioningContentControl", "FeedbackStackPresenter", "ScrollBar", "ScrollBarThumb" })
        {
            Should.Throw<ArgumentException>(() => EffectTestData.For("AtomUI", id).ToArray());
        }
    }

    [Fact]
    public void Text_Selection_Handle_Preserves_Visible_Size_And_Required_Transparent_Indicator()
    {
        var handle = new TextSelectionHandle();
        FeedbackFamilyEffectTests.WithProvider(new ArrowDecoratedBox { Content = handle }, _ =>
        {
            var icon = handle.GetVisualDescendants().OfType<PathIcon>().Single();
            icon.Height.ShouldBe(32);
            var indicator = handle.GetVisualDescendants().OfType<Border>().Single(b => b.Name == "PART_Indicator");
            indicator.Bounds.Width.ShouldBe(1);
            indicator.Bounds.Height.ShouldBe(5);
            ((ISolidColorBrush)indicator.Background!).Color.A.ShouldBe((byte)0);
        });
    }

    [Fact]
    public void Fallback_Token_Refresh_Preserves_Normal_And_Explicit_User_Icon_Colors()
    {
        var normal = new CheckOutlined();
        var fallback = new FallbackIcon();
        var explicitIcon = new FallbackIcon { FallbackBrush = Brushes.Cyan };
        FeedbackFamilyEffectTests.WithIsolationWindow(new StackPanel { Children = { normal, fallback, explicitIcon } }, (provider, capture) =>
        {
            using var before = capture();
            provider.Config = FeedbackFamilyEffectTests.WithControlToken(IconTokens.Identity, "FallbackColor", "#ff00b96b");
            using var after = capture();
            FeedbackFamilyEffectTests.ChangedRatioOf(before, after, fallback).ShouldBeGreaterThan(0d);
            FeedbackFamilyEffectTests.ChangedRatioOf(before, after, normal).ShouldBe(0d);
            FeedbackFamilyEffectTests.ChangedRatioOf(before, after, explicitIcon).ShouldBe(0d);
            explicitIcon.FallbackBrush.ShouldBe(Brushes.Cyan);
        });
    }

    [Fact]
    public void Text_Selection_Handle_Consumes_Owner_Selection_Color() => TokenEffectHarness.AssertHasEffect(
        () => new ArrowDecoratedBox { Content = new TextSelectionHandle() }, ArrowDecoratedBoxTokens.Identity, "SelectionBackground");

    [Fact]
    public void IconPresenter_Consumes_Its_Registered_Global() => TokenEffectHarness.AssertHasEffect(
        () => new IconPresenter { Width = 32, Height = 32, Icon = new CheckOutlined() }, IconPresenterTokens.Identity, "ColorIconHover");

    [Theory]
    [InlineData("ColorIcon")]
    [InlineData("IconSize")]
    public void PathIcon_Uses_Its_Actual_Owner(string token) => TokenEffectHarness.AssertHasEffect(
        () => new ArrowDecoratedBox { Content = new PathIcon { Data = StreamGeometry.Parse("M0,0 L16,0 L16,16 L0,16 Z") } },
        ArrowDecoratedBoxTokens.Identity, token);

    [Theory]
    [InlineData("ArrowDecoratedBox", "ColorBgElevated")]
    [InlineData("WindowTitleBar", "ColorBgContainer")]
    [InlineData("Icon", "ColorIconHover")]
    public void Local_Root_And_Nested_Registered_Controls_Keep_Isolation(string member, string token)
    {
        var identity = member == "Icon" ? IconTokens.Identity : member == "WindowTitleBar" ? WindowTitleBarTokens.Identity : ArrowDecoratedBoxTokens.Identity;
        Control subject = member == "Icon" ? new CheckOutlined() : member == "WindowTitleBar"
            ? new WindowTitleBar { Title = "Scoped title", Width = 360 } : new ArrowDecoratedBox { Content = "Scoped arrow" };
        Control sibling;
        if (member == "Icon") { sibling = new IconPresenter { Width = 32, Height = 32, Icon = new CheckOutlined() }; }
        else if (member == "WindowTitleBar") { sibling = new Card { Content = "Unrelated card", Width = 180 }; }
        else
        {
            var presenter = new TreeViewFlyoutPresenter();
            presenter.Items.Add(new TreeViewItem { Header = "Unrelated tree" });
            sibling = presenter;
        }
        var nested = new Button { Content = "Nested registered action" };
        if (subject is ArrowDecoratedBox arrow) { arrow.Content = nested; }
        FeedbackFamilyEffectTests.WithIsolationWindow(new StackPanel { Children = { subject, sibling } }, (provider, capture) =>
        {
            using var before = capture();
            provider.Config = FeedbackFamilyEffectTests.IsolationConfig(control: (identity, token, "#ff00b96b"));
            using var scoped = capture();
            FeedbackFamilyEffectTests.ChangedRatioOf(before, scoped, subject).ShouldBeGreaterThan(0d);
            FeedbackFamilyEffectTests.ChangedRatioOf(before, scoped, sibling).ShouldBe(0d);
            if (subject is ArrowDecoratedBox) { ThemeScope.GetTokenOwner(nested).ShouldBe(ButtonTokens.Identity); }
            provider.Config = FeedbackFamilyEffectTests.IsolationConfig(root: (token, "#ff00b96b"));
            using var root = capture();
            FeedbackFamilyEffectTests.ChangedRatioOf(before, root, subject).ShouldBeGreaterThan(0d);
            FeedbackFamilyEffectTests.ChangedRatioOf(before, root, sibling).ShouldBeGreaterThan(0d);
        });
    }
}
