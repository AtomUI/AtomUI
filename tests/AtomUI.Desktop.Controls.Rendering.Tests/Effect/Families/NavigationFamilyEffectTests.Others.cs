using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Icons.AntDesign;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AvaloniaBorder = Avalonia.Controls.Border;

namespace AtomUI.Desktop.Controls.Rendering.Tests.Effect.Families;

/// <summary>
/// Breadcrumb (with BreadcrumbItem), Pagination and SimplePagination (their page items are identity-less and read the
/// hosting pagination's globals through <c>OwnerTokenResource</c>), and Tour (TourStep, DefaultTourIndicator; the steps
/// view is a single-host part reading the Tour's Tokens). The Tour opens through the overlay popup layer.
/// </summary>
public partial class NavigationFamilyEffectTests
{
    // ---- Breadcrumb ---------------------------------------------------------------------------

    /// <summary>A navigable icon item, a plain item, a navigable item and the (plain) last item.</summary>
    internal static Breadcrumb CreateBreadcrumb()
    {
        var breadcrumb = new Breadcrumb();
        breadcrumb.Items.Add(new BreadcrumbItem { Content = "Home", Icon = new SearchOutlined(), NavigateContext = "home" });
        breadcrumb.Items.Add(new BreadcrumbItem { Content = "Plain" });
        breadcrumb.Items.Add(new BreadcrumbItem { Content = "Application", NavigateContext = "app" });
        breadcrumb.Items.Add(new BreadcrumbItem { Content = "Current" });
        return breadcrumb;
    }

    private static BreadcrumbItem CrumbOf(Control breadcrumb, string content) =>
        breadcrumb.GetVisualDescendants().OfType<BreadcrumbItem>().First(item => Equals(item.Content, content));

    public static IEnumerable<object[]> BreadcrumbRows() => MapRows("Breadcrumb");

    [Theory]
    [MemberData(nameof(BreadcrumbRows))]
    public void Breadcrumb_Token_Has_Effect(string token, bool isOwn)
    {
        _ = isOwn;
        Action<Control>? arrange = null;
        Func<Control, Control>? region = token switch
        {
            "ItemColor"                                     => static b => CrumbOf(b, "Plain"),
            "LastItemColor" or "ColorText"                  => static b => CrumbOf(b, "Current"),
            "LinkColor"                                     => static b => CrumbOf(b, "Application"),
            "LinkHoverColor" or "LinkHoverBgColor" or "ColorBgTextHover" => static b => CrumbOf(b, "Application"),
            _                                               => null
        };
        if (token is "LinkHoverColor" or "LinkHoverBgColor" or "ColorBgTextHover")
        {
            arrange = static b => Pseudo(CrumbOf(b, "Application"), ":pointerover");
        }
        Run(CreateBreadcrumb, arrange, BreadcrumbTokens.Identity, token,
            region is null ? null : new EffectOptions { Region = region });
    }

    public static IEnumerable<object[]> BreadcrumbItemRows() => MapRows("BreadcrumbItem");

    [Theory]
    [MemberData(nameof(BreadcrumbItemRows))]
    public void BreadcrumbItem_Token_Has_Effect(string token, bool isOwn)
    {
        _ = isOwn;
        // The icon item shows the icon size and the icon-to-text spacing; its hover background shows the radius.
        Run(CreateBreadcrumb,
            token == "BorderRadius" ? static item => Pseudo(item, ":pointerover") : null,
            BreadcrumbItemTokens.Identity, token,
            new EffectOptions
            {
                Target  = static root => CrumbOf(root, "Home"),
                Perturb = token == "BorderRadius" ? static _ => new CornerRadius(12) : null
            });
    }

    // ---- Pagination / SimplePagination ------------------------------------------------------------------------------

    private const string PaginationThemes = "src/AtomUI.Desktop.Controls/Pagination/Themes/";

    /// <summary>Owner reads of the identity-less page items, shared by Pagination and SimplePagination.</summary>
    internal static readonly FeedbackFamilyEffectTests.OwnerReadList s_paginationNavItem =
        new(PaginationThemes + "PaginationNavItemTheme.axaml",
        [
            "BorderThickness", "ColorBgTextActive", "ColorPrimary", "ColorText", "ColorTextDisabled", "ColorTransparent",
            "FontSize", "IconSize", "IconSizeLG", "IconSizeSM"
        ]);

    [Fact]
    public void Pagination_Owner_Read_List_Matches_The_Item_Theme()
    {
        ThemeOwnerReads.Read(s_paginationNavItem.Theme).Order(StringComparer.Ordinal)
                       .ShouldBe(s_paginationNavItem.Tokens.Order(StringComparer.Ordinal), s_paginationNavItem.Theme);
    }

    internal static Pagination CreatePagination(CustomizableSizeType size = CustomizableSizeType.Middle, bool enabled = true) =>
        new()
        {
            Total             = 500,
            PageSize          = 10,
            CurrentPage       = 6,
            IsShowQuickJumper = true,
            IsShowTotalInfo   = true,
            SizeType          = size,
            IsEnabled         = enabled
        };

    private static PaginationNavItem PageItem(Control host, Func<PaginationNavItem, bool> predicate) =>
        host.GetVisualDescendants().OfType<PaginationNavItem>().First(predicate);

    private static PaginationNavItem SelectedPage(Control host) =>
        PageItem(host, static item => item.PaginationItemType == PaginationItemType.PageIndicator && item.IsSelected);

    private static PaginationNavItem UnselectedPage(Control host) =>
        PageItem(host, static item => item.PaginationItemType == PaginationItemType.PageIndicator && !item.IsSelected);

    private static PaginationNavItem JumpNext(Control host) =>
        PageItem(host, static item => item.PaginationItemType == PaginationItemType.JumpNext);

    private static PaginationNavItem NextItem(Control host) =>
        PageItem(host, static item => item.PaginationItemType == PaginationItemType.Next);

    public static IEnumerable<object[]> PaginationRows() =>
        TabHostRows("Pagination", s_paginationNavItem);

    [Theory]
    [MemberData(nameof(PaginationRows))]
    public void Pagination_Token_Has_Effect(string token, bool isOwn)
    {
        _ = isOwn;
        var scenario = PaginationScenario(token);
        Run(scenario.Create, scenario.Arrange, PaginationTokens.Identity, token, scenario.Options);
    }

    private static NavScenario PaginationScenario(string token)
    {
        Func<Control> middle = static () => CreatePagination();
        switch (token)
        {
            case "PaginationItemPaddingInline" or "UniformlyMarginXXS":
                return new NavScenario(static () =>
                {
                    var pagination = CreatePagination();
                    pagination.Total       = 500_000;
                    pagination.CurrentPage = 2_000;
                    return pagination;
                }, null);
            case "ItemSizeSM" or "ControlHeightSM" or "ControlOutlineWidth" or "PaginationLayoutMiniSpacing" or
                "PaginationMiniQuickJumperInputWidth" or "IconSizeSM":
                return new NavScenario(static () => CreatePagination(CustomizableSizeType.Small), null);
            case "IconSizeLG":
                return new NavScenario(static () => CreatePagination(CustomizableSizeType.Large), null);
            case "ItemActiveBgDisabled" or "ItemActiveColorDisabled" or "ColorTextDisabled" or "ControlItemBgActiveDisabled":
                return new NavScenario(static () => CreatePagination(enabled: false), null);
            // The selected page: active background, primary text and border, strong weight, radius.
            case "ItemActiveBg" or "ColorPrimary" or "FontWeightStrong" or "BorderThickness" or "BorderRadius" or
                "ItemActiveColor" or "ItemActiveBorderColor" or "ItemActiveFontWeight" or "ItemBorderRadius":
                return new NavScenario(middle, null, new EffectOptions
                {
                    Region  = SelectedPageRegion,
                    Perturb = token is "BorderRadius" or "ItemBorderRadius" ? static _ => new CornerRadius(14) : null
                });
            case "ColorPrimaryHover" or "ItemActiveHoverColor" or "ItemActiveHoverBorderColor":
                return new NavScenario(middle, static host => Pseudo(SelectedPage(host), ":pointerover"),
                    new EffectOptions { Region = SelectedPageRegion });
            case "ColorBgTextHover" or "ItemHoverBg" or "ItemHoverColor":
                return new NavScenario(middle, static host => Pseudo(UnselectedPage(host), ":pointerover"),
                    new EffectOptions { Region = static host => UnselectedPage(host) });
            case "ColorBgTextActive":
                return new NavScenario(middle, static host => Pseudo(UnselectedPage(host), ":pressed"),
                    new EffectOptions { Region = static host => UnselectedPage(host) });
            // Unselected page backgrounds and the transparent border of the page items.
            case "ItemBg" or "ColorBgContainer" or "ColorTransparent":
                return new NavScenario(middle, null, new EffectOptions { Region = static host => UnselectedPage(host) });
            // The simple pager input of a SimplePagination beside the Pagination (identity under test).
            case "ItemInputBg":
                return new NavScenario(
                    static () => new StackPanel
                    {
                        Spacing  = 16,
                        Children = { CreatePagination(), new SimplePagination { Total = 500, PageSize = 10, CurrentPage = 3, IsReadOnly = false } }
                    },
                    null,
                    new EffectOptions
                    {
                        Target = static root => root.GetVisualDescendants().OfType<Pagination>().First(),
                        Region = static pagination => Descendant<LineEdit>(((StackPanel)pagination.Parent!).Children[1], "PART_QuickJumper")
                    });
            default:
                return new NavScenario(middle, null);
        }

        static Control SelectedPageRegion(Control host) => SelectedPage(host);
    }

    /// <summary>
    /// SimplePagination draws only previous / next page items, so its owner rows are the reads those items render; the
    /// page-number and jump-item reads (font, selected and hover primary colours, strong weight, ellipsis colour) and the
    /// border thickness of their transparent border run on Pagination.
    /// </summary>
    public static IEnumerable<object[]> SimplePaginationRows() =>
        TabHostRows("SimplePagination",
            new FeedbackFamilyEffectTests.OwnerReadList(s_paginationNavItem.Theme,
                ["BorderRadius", "ColorBgTextActive", "ColorBgTextHover", "ColorText", "ColorTransparent", "IconSize", "IconSizeLG",
                 "IconSizeSM"]));

    [Theory]
    [MemberData(nameof(SimplePaginationRows))]
    public void SimplePagination_Token_Has_Effect(string token, bool isOwn)
    {
        _ = isOwn;
        var size = token switch
        {
            "IconSizeLG" => CustomizableSizeType.Large,
            "IconSizeSM" or "ItemSizeSM" or "ControlHeightSM" or "ControlOutlineWidth" or "InputPaddingSM" or
                "PaginationLayoutMiniSpacing" or "PaginationMiniQuickJumperInputWidth" or "UniformlyMarginXXS" => CustomizableSizeType.Small,
            _            => CustomizableSizeType.Middle
        };
        Action<Control>? arrange = token switch
        {
            "ColorBgTextHover" or "BorderRadius" or "ItemHoverBg" or "ItemHoverColor" or "ItemBorderRadius" =>
                static host => Pseudo(NextItem(host), ":pointerover"),
            "ColorBgTextActive"                  => static host => Pseudo(NextItem(host), ":pressed"),
            _                                    => null
        };
        Run(() => new SimplePagination { Total = 500, PageSize = 10, CurrentPage = 3, SizeType = size, IsReadOnly = false, IsEnabled = token is not ("ItemActiveColorDisabled" or "ColorTextDisabled") }, arrange,
            SimplePaginationTokens.Identity, token,
            new EffectOptions
            {
                Region = token is "ColorBgTextHover" or "ColorBgTextActive" or "BorderRadius" or "IconSize" or "IconSizeLG" or "IconSizeSM" or
                    "ItemHoverBg" or "ItemHoverColor" or "ItemBorderRadius"
                    ? static host => NextItem(host) : null,
                Perturb = token is "BorderRadius" or "ItemBorderRadius" ? static _ => new CornerRadius(14) : null
            });
    }

    // ---- Tour -------------------------------------------------------------------------------------------------------

    /// <summary>A Tour of three steps on a target; <paramref name="step"/> selects the open step (1 shows previous and next).</summary>
    internal static Control CreateTourHost(TourStyleType style = TourStyleType.Default, int step = 1, string? description = null)
    {
        var target = new AvaloniaBorder
        {
            Width      = 96,
            Height     = 32,
            Background = Brushes.LightGray,
            Margin     = new Thickness(40, 40, 0, 0)
        };
        var tour = new Tour { StyleType = style };
        for (var index = 0; index < 3; index++)
        {
            tour.Steps.Add(new TourStep
            {
                Target      = target,
                Placement   = TourPlacementMode.Bottom,
                Title       = $"Step {index + 1}",
                Description = description ?? "Tour description"
            });
        }
        tour.Tag = step;
        var panel = new Panel { Children = { target, tour } };
        return OverlayLayerHost(panel, 640, 480);
    }

    private static Tour TourOf(Control root) => root.GetVisualDescendants().OfType<Tour>().First();

    private static Control OpenTour(Control root)
    {
        var tour = TourOf(root);
        tour.IsPopupPinnedOpen = true;
        WaitFor(() => tour.IsOpen);
        // The step to show (1: previous and next buttons, both indicator states).
        tour.SetCurrentValue(Tour.CurrentIndexProperty, tour.Tag is int step ? step : 0);
        return WaitForVisual(() => TopLevel.GetTopLevel(root)!.GetVisualDescendants().OfType<TourStep>()
                                           .FirstOrDefault(static step => step.IsEffectivelyVisible),
            "The open tour step");
    }

    private static Control TourFrameOf(Control anyInWindow) =>
        WaitForVisual(() => TopLevel.GetTopLevel(anyInWindow)!.GetVisualDescendants().OfType<ArrowDecoratedBox>()
                                    .FirstOrDefault(static box => box.Name == "PART_ArrowDecorator" && box.IsEffectivelyVisible),
            "The tour frame");

    public static IEnumerable<object[]> TourRows() => MapRows("Tour");

    [Theory]
    [MemberData(nameof(TourRows))]
    public void Tour_Token_Has_Effect(string token, bool isOwn)
    {
        _ = isOwn;
        var primary = token is "PrimaryPrevBtnBg" or "PrimaryNextBtnHoverBg" or "ColorPrimary" or "ColorWhite" or
            "ColorTextLightSolid" or "ColorBgTextHover";
        Func<Control, Control> region = token switch
        {
            // The gap moves the popup; the mask covers the layer.
            "PopupMarginToAnchor" or "SpacingXXS" or "ColorBgMask" => static tour => LayerPanelOf(tour),
            // The primary previous button: its border, hover fill and text are the light solid colour family.
            "PrimaryPrevBtnBg" or "ColorTextLightSolid" => static tour => Descendant<Button>(TourFrameOf(tour), "PreviousButton"),
            "SpacingXS" => static tour => Descendant<StackPanel>(TourFrameOf(tour), "ActionsLayout"),
            "PrimaryNextBtnHoverBg" or "ColorBgTextHover" or "ColorWhite" =>
                static tour => Descendant<Button>(TourFrameOf(tour), "NextButton"),
            _ => static tour => TourFrameOf(tour)
        };
        Action<Control>? arrange = token is "PrimaryNextBtnHoverBg" or "ColorBgTextHover"
            ? static tour => Pseudo(Descendant<Button>(TourFrameOf(tour), "NextButton"), ":pointerover")
            : null;
        Run(() => CreateTourHost(primary ? TourStyleType.Primary : TourStyleType.Default,
                description: token is "TourViewMinWidth" or "TourViewMinHeight" ? "A" : null),
            arrange, TourTokens.Identity, token,
            new EffectOptions
            {
                Target  = static root =>
                {
                    OpenTour(root);
                    return TourOf(root);
                },
                Region  = region,
                Perturb = token is "TourBorderRadius" or "BorderRadiusLG" ? static _ => new CornerRadius(24) : null
            });
    }

    public static IEnumerable<object[]> TourStepRows() => MapRows("TourStep");

    [Theory]
    [MemberData(nameof(TourStepRows))]
    public void TourStep_Token_Has_Effect(string token, bool isOwn)
    {
        _ = isOwn;
        // ColorIconHover is the hovered close icon of a primary step when the step foreground does not contrast with the
        // hovered button: give the step its frame's colour so the readable fallback shows.
        Action<Control>? arrange = token == "ColorIconHover"
            ? static step =>
            {
                ((TourStep)step).Foreground = ((ArrowDecoratedBox)TourFrameOf(step)).Background;
                Pseudo(Descendant<Control>(step, "CloseButton"), ":pointerover");
            }
            : null;
        var primary = token is "ColorTextLightSolid" or "ColorIconHover";
        Run(() => CreateTourHost(primary ? TourStyleType.Primary : TourStyleType.Default), arrange,
            TourStepTokens.Identity, token,
            new EffectOptions
            {
                Target = static root => OpenTour(root),
                Region = token == "ColorIconHover" ? static step => Descendant<Control>(step, "CloseButton") : null
            });
    }

    public static IEnumerable<object[]> DefaultTourIndicatorRows() => MapRows("DefaultTourIndicator");

    [Theory]
    [MemberData(nameof(DefaultTourIndicatorRows))]
    public void DefaultTourIndicator_Token_Has_Effect(string token, bool isOwn)
    {
        _ = isOwn;
        Run(() => CreateTourHost(token == "ColorTextLightSolid" ? TourStyleType.Primary : TourStyleType.Default), null,
            DefaultTourIndicatorTokens.Identity, token,
            new EffectOptions
            {
                Target = static root =>
                {
                    OpenTour(root);
                    return WaitForVisual(() => TopLevel.GetTopLevel(root)!.GetVisualDescendants().OfType<DefaultTourIndicator>()
                                                       .FirstOrDefault(static indicator => indicator.IsEffectivelyVisible),
                        "The tour indicator");
                }
            });
    }

    // ---- Tour borrowed fix 9c01e0cc7: the popup gap follows the Token while open -------------------------------------

    [Fact]
    public void Tour_Popup_Gap_Follows_Its_Token_And_Gap_While_Open_Only()
    {
        // 9c01e0cc7: the open popup's margin to the target is the step gap offset plus TourToken.PopupMarginToAnchor. The
        // Token was read once when the step opened, so a theme or Controls[Tour] change did not reach the open popup; a gap
        // change reconfigured the popup even while the Tour was closed.
        var root = CreateTourHost();
        WithProvider(root, provider =>
        {
            var tour  = TourOf(root);
            var popup = tour.GetVisualDescendants().OfType<Popup>().First();
            OpenTour(root);
            var token = tour.PopupMarginToAnchor;
            popup.MarginToAnchor.ShouldBe(tour.GapOffsetY + token);

            provider.Config = WithControlToken(TourTokens.Identity, "PopupMarginToAnchor", "20");
            WaitFor(() => Math.Abs(popup.MarginToAnchor - (tour.GapOffsetY + 20)) < 0.01);
            popup.MarginToAnchor.ShouldBe(tour.GapOffsetY + 20);

            tour.GapOffsetY = 12;
            popup.MarginToAnchor.ShouldBe(12 + 20);

            tour.IsPopupPinnedOpen = false;
            tour.HideTour();
            WaitFor(() => !tour.IsOpen);
            var closedMargin = popup.MarginToAnchor;
            tour.GapOffsetY = 30;
            popup.MarginToAnchor.ShouldBe(closedMargin, "A closed Tour reconfigured its popup margin.");

            OpenTour(root);
            popup.MarginToAnchor.ShouldBe(30 + 20);
        });
    }
}
