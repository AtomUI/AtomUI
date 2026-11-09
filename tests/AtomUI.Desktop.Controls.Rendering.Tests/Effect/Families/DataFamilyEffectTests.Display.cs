using AtomUI.Controls;
using AtomUI.Controls.DesignTokens;
using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Icons.AntDesign;
using AtomUI.Theme.Configuration;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AvaloniaBorder = Avalonia.Controls.Border;
using AvaloniaTextBlock = Avalonia.Controls.TextBlock;

namespace AtomUI.Desktop.Controls.Rendering.Tests.Effect.Families;

/// <summary>
/// Avatar (its Own Tokens live in AtomUI.Controls) and AvatarGroup, Timeline (TimelineItem is Token-owner transparent; the
/// identity-less indicator reads the Timeline TokenResource), Statistic / TimerStatistic (their shared base theme reads the
/// owner; AbstractStatistic is never instantiated) and StatisticCountUp, and Carousel (CarouselPage is Token-owner transparent;
/// CarouselNavButton keeps its identity).
/// </summary>
public partial class DataFamilyEffectTests
{
    // ---- Avatar ---------------------------------------------------------------------------------------------------------

    private static Avatar TextAvatar(CustomizableSizeType size = CustomizableSizeType.Middle, AvatarShape shape = AvatarShape.Circle) =>
        new() { Text = "U", SizeType = size, Shape = shape };

    /// <summary>
    /// An icon avatar: AtomUI's TextFontSize* is the icon size of an icon avatar (its defaults are antd 6.7's iconFontSize*);
    /// text avatars scale their text to fit the container.
    /// </summary>
    private static Avatar IconAvatar(CustomizableSizeType size = CustomizableSizeType.Middle) =>
        new() { Icon = new UserOutlined(), SizeType = size };

    private static AvatarGroup CreateAvatarGroup(int? maxDisplayCount = null)
    {
        var group = new AvatarGroup { MaxDisplayCount = maxDisplayCount };
        group.Children.Add(new Avatar { Text = "A" });
        group.Children.Add(new Avatar { Text = "B" });
        group.Children.Add(new Avatar { Icon = new UserOutlined() });
        group.Children.Add(new Avatar { Text = "D" });
        return group;
    }

    public static IEnumerable<object[]> AvatarRows() => MapRows("Avatar");

    [Theory]
    [MemberData(nameof(AvatarRows))]
    public void Avatar_Token_Has_Effect(string token, bool isOwn)
    {
        _ = isOwn;
        var scenario = AvatarScenario(token);
        Run(scenario.Create, scenario.Arrange, AvatarTokens.Identity, token, scenario.Options);
    }

    private static Scenario AvatarScenario(string token)
    {
        switch (token)
        {
            case "ContainerSizeLG" or "ControlHeightLG":
                return new Scenario(static () => TextAvatar(CustomizableSizeType.Large));
            case "ContainerSizeSM" or "ControlHeightSM":
                return new Scenario(static () => TextAvatar(CustomizableSizeType.Small));
            // The icon size of an icon avatar per size (TextFontSize = round((FontSizeLG + FontSizeXL) / 2)).
            case "TextFontSizeLG" or "FontSizeHeading3":
                return new Scenario(static () => IconAvatar(CustomizableSizeType.Large));
            case "TextFontSizeSM" or "FontSize":
                return new Scenario(static () => IconAvatar(CustomizableSizeType.Small));
            case "TextFontSize" or "FontSizeLG" or "FontSizeXL":
                return new Scenario(static () => IconAvatar());
            // Square avatars use the size's corner radius.
            case "BorderRadiusLG":
                return new Scenario(static () => TextAvatar(CustomizableSizeType.Large, AvatarShape.Square));
            case "BorderRadiusSM":
                return new Scenario(static () => TextAvatar(CustomizableSizeType.Small, AvatarShape.Square));
            case "BorderRadius":
                return new Scenario(static () => TextAvatar(shape: AvatarShape.Square));
            // The ring between grouped avatars (the group gives its avatars a border).
            case "GroupBorderColor" or "ColorBorderBg":
                return new Scenario(static () => new AvaloniaBorder
                    {
                        Background = Brushes.LightGray,
                        Padding    = new Thickness(8),
                        Child      = CreateAvatarGroup()
                    }, null,
                    new EffectOptions
                    {
                        Target = static host => host.GetVisualDescendants().OfType<Avatar>().ElementAt(1),
                        Region = static avatar => avatar.FindAncestorOfType<AvatarGroup>()!
                    });
            default:
                // AvatarBg / ColorTextPlaceholder, AvatarColor / ColorTextLightSolid, ContainerSize / TextFontSize /
                // ControlHeight / FontSizeLG / FontSizeXL, EnableMotion.
                return new Scenario(static () => TextAvatar());
        }
    }

    public static IEnumerable<object[]> AvatarGroupRows() => MapRows("AvatarGroup");

    [Theory]
    [MemberData(nameof(AvatarGroupRows))]
    public void AvatarGroup_Token_Has_Effect(string token, bool isOwn)
    {
        _ = isOwn;
        switch (token)
        {
            // The spacing between avatars in the folded avatars popover: open it (pinned) in the window's overlay layer.
            case "GroupSpace" or "UniformlyMarginXXS":
                Run(static () => FeedbackFamilyEffectTests.OverlayLayerHost(CreateAvatarGroup(2), 320, 200),
                    static group => ((AvatarGroup)group).IsPopupPinnedOpen = true,
                    AvatarGroupTokens.Identity, token,
                    new EffectOptions
                    {
                        Target = static host => Descendant<AvatarGroup>(host),
                        Region = static group => FeedbackFamilyEffectTests.WaitForVisual(
                            () => TopLevel.GetTopLevel(group)!.GetVisualDescendants().OfType<StackPanel>()
                                          .FirstOrDefault(panel => panel.Children.OfType<Avatar>().Count() == 2 &&
                                                                   panel.FindAncestorOfType<AvatarGroup>() is null),
                            "The folded avatars popover")
                    });
                break;
            default:
                // GroupOverlapping / UniformlyMarginXS (the overlap of adjacent avatars), BorderThickness (their ring).
                Run(static () => new AvaloniaBorder { Background = Brushes.LightGray, Padding = new Thickness(8), Child = CreateAvatarGroup() },
                    null, AvatarGroupTokens.Identity, token,
                    new EffectOptions { Target = static host => Descendant<AvatarGroup>(host) });
                break;
        }
    }

    // ---- Timeline -------------------------------------------------------------------------------------------------------

    internal static Timeline CreateTimeline(
        TimelineMode mode = TimelineMode.Start,
        bool pending = false,
        bool icon = false,
        bool border = false,
        bool horizontal = false)
    {
        var timeline = new Timeline
        {
            Width       = horizontal ? 480 : 320,
            Mode        = mode,
            Orientation = horizontal ? Orientation.Horizontal : Orientation.Vertical
        };
        if (pending)
        {
            timeline.Pending = "Recording...";
        }
        if (border)
        {
            timeline.BorderThickness = new Thickness(1);
        }

        timeline.Items.Add(new TimelineItem { Content = "Create a services site 2015-09-01" });
        timeline.Items.Add(new TimelineItem
        {
            Content       = "Solve initial network problems 2015-09-01",
            IndicatorIcon = icon ? new ClockCircleOutlined() : null
        });
        timeline.Items.Add(new TimelineItem { Content = "Technical testing 2015-09-01" });
        return timeline;
    }

    private static Control FirstIndicatorOf(Control timeline) =>
        timeline.GetVisualDescendants().OfType<AtomUI.Controls.Commons.TimelineIndicator>().First();

    /// <summary>A timeline on a grey surface, so its container background shows.</summary>
    private static Scenario TimelineOnSurface(Func<Timeline> create) =>
        new(() => new AvaloniaBorder { Background = Brushes.LightGray, Padding = new Thickness(8), Child = create() }, null,
            new EffectOptions { Target = static host => Descendant<Timeline>(host) });

    public static IEnumerable<object[]> TimelineRows() => MapRows("Timeline");

    [Theory]
    [MemberData(nameof(TimelineRows))]
    public void Timeline_Token_Has_Effect(string token, bool isOwn)
    {
        _ = isOwn;
        var scenario = TimelineScenario(token);
        Run(scenario.Create, scenario.Arrange, TimelineTokens.Identity, token, scenario.Options);
    }

    private static Scenario TimelineScenario(string token)
    {
        switch (token)
        {
            case "IndicatorEndModeMargin":
                return new Scenario(static () => CreateTimeline(TimelineMode.End));
            case "IndicatorMiddleModeMargin":
                return new Scenario(static () => CreateTimeline(TimelineMode.Alternate));
            // The pending item (last, before it the larger padding).
            case "ItemPaddingBottomLG":
                return new Scenario(static () => CreateTimeline(pending: true));
            // The spacing between a horizontal item's indicator and its content.
            case "UniformlyPaddingXS":
                return new Scenario(static () => CreateTimeline(horizontal: true));
            // Thin node and tail visuals: compared on the first item's indicator.
            case "IndicatorTailWidth" or "IndicatorTailColor" or "ColorSplit" or "LineWidthBold" or "ColorPrimary" or
                "IndicatorDotBorderWidth" or "LineWidth" or "IndicatorDotSize":
                return new Scenario(static () => CreateTimeline(), null,
                    new EffectOptions { Region = static timeline => FirstIndicatorOf(timeline) });
            // An icon indicator: the icon size.
            case "IndicatorSize" or "SizeMS":
                return new Scenario(static () => CreateTimeline(icon: true));
            // The container background (on a grey surface) and the dot fill; the border with an application thickness.
            case "ColorBgContainer":
                return TimelineOnSurface(static () => CreateTimeline());
            case "ColorBorder":
                return TimelineOnSurface(static () => CreateTimeline(border: true));
            default:
                // Dot size / ring / colour (IndicatorDotSize, IndicatorDotBorderWidth, LineWidth, ColorPrimary), tail
                // (IndicatorTailColor / ColorSplit, IndicatorTailWidth / LineWidthBold), item padding (ItemPaddingBottom /
                // UniformlyPadding), the start-mode margin (IndicatorStartModeMargin / UniformlyMargin), the label gap
                // (UniformlyPaddingXS) and the indicator line height (RelativeLineHeight).
                return new Scenario(static () => CreateTimeline());
        }
    }

    // ---- Statistic ------------------------------------------------------------------------------------------------------

    private static Statistic CreateStatistic(bool loading = false) => new()
    {
        Header           = "Active Users",
        Content          = "112,893",
        ValuePrefixAddOn = new LikeOutlined(),
        IsLoading        = loading
    };

    // AbstractStatistic is abstract: its theme reads run as rows of Statistic and TimerStatistic.
    public static IEnumerable<object[]> StatisticRows() => HostRows("Statistic", ["AbstractStatistic"], []);

    [Theory]
    [MemberData(nameof(StatisticRows))]
    public void Statistic_Token_Has_Effect(string token, bool isOwn)
    {
        _ = isOwn;
        // The loading placeholder sits further from the title (Spacing); the other rows show the loaded value.
        var loading = token is "Spacing";
        Run(() => CreateStatistic(loading), null, StatisticTokens.Identity, token);
    }

    public static IEnumerable<object[]> TimerStatisticRows() => HostRows("TimerStatistic", ["AbstractStatistic"], []);

    [Theory]
    [MemberData(nameof(TimerStatisticRows))]
    public void TimerStatistic_Token_Has_Effect(string token, bool isOwn)
    {
        _ = isOwn;
        Run(static () => new TimerStatistic { Header = "Countdown", Width = 240, Value = DateTime.Now.AddDays(2) }, null,
            TimerStatisticTokens.Identity, token);
    }

    public static IEnumerable<object[]> StatisticCountUpRows() => MapRows("StatisticCountUp");

    [Theory]
    [MemberData(nameof(StatisticCountUpRows))]
    public void StatisticCountUp_Token_Has_Effect(string token, bool isOwn)
    {
        _ = isOwn;
        Run(static () => new StatisticCountUp { EndValue = 112893 }, null, StatisticCountUpTokens.Identity, token);
    }

    // ---- Carousel -------------------------------------------------------------------------------------------------------

    /// <summary>Dark pages, so the white arrows and dots show; the second page is selected, so both arrows are visible.</summary>
    internal static Carousel CreateCarousel(CarouselPaginationPosition position = CarouselPaginationPosition.Bottom)
    {
        var carousel = new Carousel
        {
            Width              = 360,
            Height             = 160,
            IsShowNavButtons   = true,
            PaginationPosition = position
        };
        for (var i = 1; i <= 4; i++)
        {
            carousel.Items.Add(new CarouselPage
            {
                Content = new AvaloniaBorder
                {
                    Background = new SolidColorBrush(Color.FromRgb(0x36, 0x4D, 0x79)),
                    Child = new AvaloniaTextBlock
                    {
                        Text                = $"Page {i}",
                        Foreground          = Brushes.White,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment   = VerticalAlignment.Center
                    }
                }
            });
        }
        carousel.SelectedIndex = 1;
        return carousel;
    }

    private static Control PreviousButtonOf(Control carousel) => Descendant<CarouselNavButton>(carousel, "PART_PreviousButton");

    public static IEnumerable<object[]> CarouselRows() => MapRows("Carousel");

    [Theory]
    [MemberData(nameof(CarouselRows))]
    public void Carousel_Token_Has_Effect(string token, bool isOwn)
    {
        _ = isOwn;
        switch (token)
        {
            // The resting arrow is dim; hovering makes it opaque, so the rest opacity only shows on a resting arrow.
            case "ArrowOpacity" or "ArrowSize" or "ArrowOffset" or "SpacingXS":
                Run(static () => CreateCarousel(), null, CarouselTokens.Identity, token,
                    new EffectOptions { Region = static carousel => PreviousButtonOf(carousel) });
                break;
            // The dots (compared on the pagination strip, a small share of the carousel).
            case "IndicatorWidth" or "IndicatorActiveWidth" or "IndicatorHeight" or "IndicatorGap" or "UniformlyMarginXS" or
                "ColorBgContainer":
                Run(static () => CreateCarousel(), null, CarouselTokens.Identity, token,
                    new EffectOptions { Region = static carousel => Descendant<CarouselPagination>(carousel) });
                break;
            default:
                // The dots' offset from the edge (PaginationOffset), EnableMotion, MotionDurationSlow.
                Run(static () => CreateCarousel(), null, CarouselTokens.Identity, token);
                break;
        }
    }

    public static IEnumerable<object[]> CarouselNavButtonRows() => MapRows("CarouselNavButton");

    [Theory]
    [MemberData(nameof(CarouselNavButtonRows))]
    public void CarouselNavButton_Token_Has_Effect(string token, bool isOwn)
    {
        _ = isOwn;
        // The arrow icon brush; hovered so the arrow is opaque.
        Run(static () => CreateCarousel(), static button => Pseudo(button, ":pointerover"), CarouselNavButtonTokens.Identity, token,
            new EffectOptions { Target = static carousel => PreviousButtonOf(carousel) });
    }

    [Fact]
    public void Display_Motion_And_Durations_Follow_The_Member_Tokens()
    {
        // TimeSpan / bool rows are read-backs; these check the members consume them.
        var avatar   = TextAvatar();
        var carousel = CreateCarousel();
        var root = new StackPanel { Spacing = 8, Children = { avatar, carousel } };
        WithProvider(root, provider =>
        {
            avatar.IsMotionEnabled.ShouldBeTrue();
            carousel.IsMotionEnabled.ShouldBeTrue();
            carousel.PageTransitionDuration.ShouldBe(TimeSpan.FromMilliseconds(300));
            provider.Config = new ThemeConfigBuilder()
                              .WithControl(AvatarTokens.Identity, new ControlThemeConfigBuilder().WithToken("EnableMotion", "false").Build())
                              .WithControl(CarouselTokens.Identity, new ControlThemeConfigBuilder()
                                                                     .WithToken("MotionDurationSlow", "00:00:00.6000000").Build())
                              .Build();
            WaitFor(() => !avatar.IsMotionEnabled && carousel.PageTransitionDuration == TimeSpan.FromMilliseconds(600));
            avatar.IsMotionEnabled.ShouldBeFalse();
            carousel.IsMotionEnabled.ShouldBeTrue();
            carousel.PageTransitionDuration.ShouldBe(TimeSpan.FromMilliseconds(600));
        });
    }
}
