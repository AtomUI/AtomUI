using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Theme;
using AtomUI.Theme.Schema;
using Avalonia.Controls;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;

namespace AtomUI.Desktop.Controls.Rendering.Tests.Effect.Families;

/// <summary>
/// Scope isolation of Breadcrumb, Pagination / SimplePagination (their shared page items follow the hosting pagination)
/// and Tour (its footer buttons keep the Button identity).
/// </summary>
public partial class NavigationFamilyEffectTests
{
    [Fact]
    public void Controls_Breadcrumb_And_Paginations_Change_Only_Their_Member()
    {
        var breadcrumb = CreateBreadcrumb();
        var pagination = CreatePagination();
        var simple     = new SimplePagination { Total = 500, PageSize = 10, CurrentPage = 3 };
        var root = new StackPanel { Spacing = 24, Children = { breadcrumb, pagination, simple } };
        WithIsolationWindow(root, (provider, capture) =>
        {
            var members = new (ControlTokenIdentity Identity, string Token, Control Control)[]
            {
                (BreadcrumbTokens.Identity, "ColorTextDescription", breadcrumb),
                // The page items are shared parts: each pagination's items read their own host's ColorText.
                (PaginationTokens.Identity, "ColorText", pagination),
                (SimplePaginationTokens.Identity, "ColorText", simple)
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
                        ratio.ShouldBeGreaterThan(TokenEffectHarness.ThinLineThresholdRatio,
                            $"Controls[{identity}].{token} did not reach {control.GetType().Name}.");
                    }
                    else
                    {
                        ratio.ShouldBe(0d, $"Controls[{identity}].{token} leaked into {control.GetType().Name}.");
                    }
                }

                provider.Config = IsolationConfig(root: (token, IsolationColor));
                using var byRoot = capture();
                ChangedRatioOf(before, byRoot, target).ShouldBeGreaterThan(TokenEffectHarness.ThinLineThresholdRatio,
                    $"Root {token} did not reach {target.GetType().Name}.");
            }
        });
    }

    [Fact]
    public void Tour_Frame_Follows_The_Tour_And_Its_Buttons_Keep_The_Button_Identity()
    {
        var root = CreateTourHost();
        WithIsolationWindow(root, (provider, capture) =>
        {
            OpenTour(root);
            var frame = TourFrameOf(root);
            var next  = Descendant<Button>(frame, "NextButton");
            ThemeScope.GetTokenOwner(next).ShouldBe(ButtonTokens.Identity);
            // The steps view sits in the registered ArrowDecoratedBox (its owner), so it reads the Tour's Tokens explicitly
            // through TourTokenResource; the frame values the Tour sets on the box follow Controls[Tour].
            ThemeScope.GetTokenOwner(frame).ShouldBe(ArrowDecoratedBoxTokens.Identity);
            ThemeScope.GetTokenOwner(frame.GetVisualDescendants().OfType<TourStepsView>().First()).ShouldBe(ArrowDecoratedBoxTokens.Identity);
            using var before = capture();

            provider.Config = IsolationConfig(control: (TourTokens.Identity, "ColorBgElevated", IsolationColor));
            using var byTour = capture();
            ChangedRatioOf(before, byTour, frame).ShouldBeGreaterThan(TokenEffectHarness.DefaultThresholdRatio,
                "Controls[Tour].ColorBgElevated did not reach the tour frame.");

            provider.Config = IsolationConfig(control: (TourTokens.Identity, "ColorPrimary", IsolationColor));
            using var byTourPrimary = capture();
            ChangedRatioOf(before, byTourPrimary, next).ShouldBe(0d, "Controls[Tour].ColorPrimary reached the footer Button.");

            provider.Config = IsolationConfig(control: (ButtonTokens.Identity, "ColorPrimary", IsolationColor));
            using var byButton = capture();
            ChangedRatioOf(before, byButton, next).ShouldBeGreaterThan(TokenEffectHarness.DefaultThresholdRatio,
                "Controls[Button].ColorPrimary did not reach the footer Button.");
        });
    }
}
