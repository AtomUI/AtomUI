using AtomUI.Controls;
using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Theme;
using AtomUI.Theme.Configuration;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;

namespace AtomUI.Desktop.Controls.Rendering.Tests.Effect.Families;

public partial class PrimitivesFamilyEffectTests
{
    public static IEnumerable<object[]> FlyoutRows() => EffectTestData.For("AtomUI", "FlyoutHost");
    public static IEnumerable<object[]> TreeFlyoutRows() => EffectTestData.For("AtomUI", "TreeViewFlyoutPresenter");

    [Theory, MemberData(nameof(FlyoutRows))]
    public void Flyout_Token_Has_Effect(string token, bool own)
    {
        _ = own;
        if (token is "HorizontalOffset" or "VerticalOffset" or "UniformlyMarginXS")
        {
            AssertPointerOffset(token);
            return;
        }
        if (token == "PopupRootShadow")
        {
            AssertFlyoutRootShadow();
            return;
        }
        TokenEffectHarness.AssertHasEffect(() =>
        {
            var flyout = new Flyout { Content = "Real overlay flyout content" };
            return FeedbackFamilyEffectTests.OverlayLayerHost(new FlyoutHost
            { Width = 180, Height = 40, Content = new Button { Content = "Open flyout" }, Flyout = flyout, IsPopupPinnedOpen = true });
        }, FlyoutHostTokens.Identity, token, options: new EffectOptions
        {
            Target = root =>
            {
                var host = root.GetVisualDescendants().OfType<FlyoutHost>().Single();
                host.ShowFlyout(true);
                FeedbackFamilyEffectTests.WaitFor(() => PopupFrame(host) is not null);
                return host;
            },
            Region = host => PopupFrame(host)!,
            RegionInflation = token is "MarginToAnchor" or "UniformlyMarginXXS" ? 32 : 0
        });
    }

    [Theory, MemberData(nameof(TreeFlyoutRows))]
    public void Tree_Flyout_Token_Has_Effect(string token, bool own)
    {
        _ = own;
        TokenEffectHarness.AssertHasEffect(() =>
        {
            var flyout = new TreeViewFlyout();
            var count = token is "PopupMaxHeight" ? 60 : 2;
            for (var i = 0; i < count; ++i)
            {
                flyout.Items.Add(new TreeViewItem { Header = token == "PopupMaxWidth" ? new string('W', 100) : $"Tree item {i}" });
            }
            return FeedbackFamilyEffectTests.OverlayLayerHost(new FlyoutHost
            { Width = 180, Height = 40, Content = new Button { Content = "Open tree" }, Flyout = flyout, IsPopupPinnedOpen = true });
        }, TreeViewFlyoutPresenterTokens.Identity, token, options: new EffectOptions
        {
            Target = root =>
            {
                var host = root.GetVisualDescendants().OfType<FlyoutHost>().Single();
                host.ShowFlyout(true);
                FeedbackFamilyEffectTests.WaitFor(() => TopLevel.GetTopLevel(host)!.GetVisualDescendants().OfType<TreeViewFlyoutPresenter>().Any());
                return TopLevel.GetTopLevel(host)!.GetVisualDescendants().OfType<TreeViewFlyoutPresenter>().Single();
            },
            Perturb = token is "PopupMaxHeight" or "PopupMaxWidth" ? _ => 240d : null
        });
    }

    private static Border? PopupFrame(Control control) => TopLevel.GetTopLevel(control)?.GetVisualDescendants()
        .OfType<Border>().FirstOrDefault(static b => b.Name == "PopupFrame" && b.IsEffectivelyVisible && b.Bounds.Height > 0);

    private static void AssertPointerOffset(string token)
    {
        var target = new Button { Content = "Pointer menu" };
        var flyout = new MenuFlyout { RequestedPlacement = PlacementMode.Pointer, IsMotionEnabled = false };
        flyout.Items.Add(new MenuItem { Header = "First" });
        FeedbackFamilyEffectTests.WithProvider(FeedbackFamilyEffectTests.OverlayLayerHost(target), provider =>
        {
            flyout.ShowAt(target);
            try
            {
                var beforeX = flyout.HorizontalOffset;
                var beforeY = flyout.VerticalOffset;
                provider.Config = FeedbackFamilyEffectTests.WithControlToken(FlyoutHostTokens.Identity, token, "32");
                FeedbackFamilyEffectTests.WaitFor(() => token == "HorizontalOffset" ? flyout.HorizontalOffset == 32
                    : token == "VerticalOffset" ? flyout.VerticalOffset == 32 : flyout.HorizontalOffset == 32 && flyout.VerticalOffset == 32);
                if (token != "VerticalOffset") { flyout.HorizontalOffset.ShouldBe(32); }
                if (token != "HorizontalOffset") { flyout.VerticalOffset.ShouldBe(32); }
                provider.Config = new ThemeConfigBuilder().Build();
                FeedbackFamilyEffectTests.WaitFor(() => flyout.HorizontalOffset == beforeX && flyout.VerticalOffset == beforeY);
                flyout.HorizontalOffset.ShouldBe(beforeX);
                flyout.VerticalOffset.ShouldBe(beforeY);
            }
            finally { flyout.Hide(); }
        });
    }

    private static void AssertFlyoutRootShadow()
    {
        var target = new Button { Content = "Popup" };
        var flyout = new Flyout { Content = "Popup content", IsMotionEnabled = false };
        FeedbackFamilyEffectTests.WithProvider(FeedbackFamilyEffectTests.OverlayLayerHost(target), provider =>
        {
            flyout.ShowAt(target);
            try
            {
                var before = flyout.PopupRootShadow;
                var value = Avalonia.Media.BoxShadows.Parse("0 12 32 4 #9900b96b");
                provider.Config = FeedbackFamilyEffectTests.WithControlToken(FlyoutHostTokens.Identity, "PopupRootShadow", value.ToString());
                FeedbackFamilyEffectTests.WaitFor(() => flyout.PopupRootShadow == value);
                flyout.PopupRootShadow.ShouldBe(value);
                provider.Config = new ThemeConfigBuilder().Build();
                FeedbackFamilyEffectTests.WaitFor(() => flyout.PopupRootShadow == before);
                flyout.PopupRootShadow.ShouldBe(before);
            }
            finally { flyout.Hide(); }
        });
    }
}
