using AtomUI.Controls;
using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Theme;
using AtomUI.Theme.Resources;
using Avalonia.Controls;
using Avalonia.Animation;
using Avalonia.Media;
using Shouldly;
using Xunit;

namespace AtomUI.Desktop.Controls.Rendering.Tests.Effect.Families;

public partial class DisplayFamilyEffectTests
{
    private const string IsolationColor = "#ff00b96b";

    [Theory]
    [InlineData("Result", "ColorTextHeading")]
    [InlineData("Empty", "ColorTextDescription")]
    [InlineData("QRCode", "ColorText")]
    [InlineData("Spin", "ColorPrimary")]
    [InlineData("ListTransfer", "ColorBorder")]
    [InlineData("TreeTransfer", "ColorBorder")]
    [InlineData("HyperLinkTextBlock", "ColorLink")]
    [InlineData("SelectableTextBlock", "SelectionBackground")]
    [InlineData("BorderBeam", "ColorPrimary")]
    public void Control_Override_Is_Isolated_From_Unrelated_Sibling(string member, string token)
    {
        var (scenario, identity) = member switch
        {
            "Result" => (ResultScenario(token), ResultTokens.Identity),
            "Empty" => (EmptyScenario(token), EmptyTokens.Identity),
            "QRCode" => (QRCodeScenario(token), QRCodeTokens.Identity),
            "Spin" => (SpinScenario(token), SpinTokens.Identity),
            "ListTransfer" => (TransferScenario(token), ListTransferTokens.Identity),
            "TreeTransfer" => (TransferScenario(token, true), TreeTransferTokens.Identity),
            "HyperLinkTextBlock" => (new Scenario(static () => new HyperLinkTextBlock { Text = "Read more" }), HyperLinkTextBlockTokens.Identity),
            "SelectableTextBlock" => (new Scenario(static () => new SelectableTextBlock { Text = "Select words", SelectionStart = 0, SelectionEnd = 6 }), SelectableTextBlockTokens.Identity),
            "BorderBeam" => (new Scenario(static () => new BorderBeam { Width = 160, Height = 60, BorderThickness = new Avalonia.Thickness(3), Duration = TimeSpan.FromDays(1), Content = "Beam" }), BorderBeamTokens.Identity),
            _ => throw new ArgumentOutOfRangeException(nameof(member))
        };
        var subject = scenario.Create();
        var sibling = new Button { Content = "Unrelated", Width = 100 };
        FeedbackFamilyEffectTests.WithIsolationWindow(new StackPanel { Children = { subject, sibling } }, (provider, capture) =>
        {
            scenario.Arrange?.Invoke(subject);
            using var before = capture();
            provider.Config = FeedbackFamilyEffectTests.IsolationConfig(control: (identity, token, IsolationColor));
            using var after = capture();
            FeedbackFamilyEffectTests.ChangedRatioOf(before, after, subject).ShouldBeGreaterThan(0d);
            FeedbackFamilyEffectTests.ChangedRatioOf(before, after, sibling).ShouldBe(0d);
            provider.Config = FeedbackFamilyEffectTests.IsolationConfig(root: (token, IsolationColor));
            using var root = capture();
            FeedbackFamilyEffectTests.ChangedRatioOf(before, root, subject).ShouldBeGreaterThan(0d);
        });
    }

    private static void AssertDuration(Control control, TimeSpan duration)
    {
        FeedbackFamilyEffectTests.WaitFor(() => control.Transitions?.OfType<TransitionBase>().Any() == true &&
            control.Transitions.OfType<TransitionBase>().All(transition => transition.Duration == duration));
        control.Transitions!.OfType<TransitionBase>().ShouldAllBe(transition => transition.Duration == duration);
    }

    [Fact]
    public void HyperLink_Root_Background_Reaches_Existing_Text_Visual()
    {
        var link = new HyperLinkTextBlock { Text = "Read more", Background = Brushes.Lime };
        FeedbackFamilyEffectTests.WithProvider(link, provider =>
        {
            var text = Part<AtomUI.Desktop.Controls.TextBlock>(link, "PART_Text");
            text.Background.ShouldBe(Brushes.Lime);
            link.Background = Brushes.Red;
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            text.Background.ShouldBe(Brushes.Red);
            provider.Config = FeedbackFamilyEffectTests.WithControlToken(HyperLinkTextBlockTokens.Identity, "MotionDurationMid", "00:00:00.4000000");
            AssertDuration(link, TimeSpan.FromMilliseconds(400));
            EffectTestData.For("AtomUI", "HyperLinkTextBlock")
                          .Any(row => (string)row[0] == "MotionDurationMid" && !(bool)row[1]).ShouldBeTrue();
        });
    }

    [Fact]
    public void Root_Global_Reaches_Two_Independent_Display_Members()
    {
        var result = CreateResult();
        var empty = CreateEmpty();
        FeedbackFamilyEffectTests.WithIsolationWindow(new StackPanel { Children = { result, empty } }, (provider, capture) =>
        {
            using var before = capture();
            provider.Config = FeedbackFamilyEffectTests.IsolationConfig(control: (ResultTokens.Identity, "ColorTextDescription", IsolationColor));
            using var scoped = capture();
            FeedbackFamilyEffectTests.ChangedRatioOf(before, scoped, result).ShouldBeGreaterThan(0d);
            FeedbackFamilyEffectTests.ChangedRatioOf(before, scoped, empty).ShouldBe(0d);
            provider.Config = FeedbackFamilyEffectTests.IsolationConfig(root: ("ColorTextDescription", IsolationColor));
            using var root = capture();
            FeedbackFamilyEffectTests.ChangedRatioOf(before, root, result).ShouldBeGreaterThan(0d);
            FeedbackFamilyEffectTests.ChangedRatioOf(before, root, empty).ShouldBeGreaterThan(0d);
        });
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Transfer_Items_Follow_View_And_Editors_Keep_Their_Identity(bool tree)
    {
        var transfer = CreateTransfer(tree);
        FeedbackFamilyEffectTests.WithIsolationWindow(transfer, (provider, capture) =>
        {
            var input = Part<LineEdit>(transfer, "FilterInput");
            var view = tree ? (Control)Part<TransferTreeView>(transfer) : Part<TransferListView>(transfer);
            var item = tree ? (Control)Part<TransferTreeViewItem>(view) : Part<TransferListItem>(view);
            var hostIdentity = tree ? TreeTransferTokens.Identity : ListTransferTokens.Identity;
            var viewIdentity = tree ? TransferTreeViewTokens.Identity : TransferListViewTokens.Identity;
            ThemeScope.GetTokenOwner(item).ShouldBe(viewIdentity);
            ThemeScope.GetTokenOwner(input).ShouldBe(LineEditTokens.Identity);
            if (tree)
            {
                ((TransferTreeViewItem)item).IsMasked = true;
            }
            using var before = capture();
            provider.Config = FeedbackFamilyEffectTests.IsolationConfig(control: (hostIdentity, "ColorBorder", IsolationColor));
            using var byHost = capture();
            FeedbackFamilyEffectTests.ChangedRatioOf(before, byHost, input).ShouldBe(0d);
            provider.Config = FeedbackFamilyEffectTests.IsolationConfig(control: (LineEditTokens.Identity, "ColorBorder", IsolationColor));
            using var byInput = capture();
            FeedbackFamilyEffectTests.ChangedRatioOf(before, byInput, input).ShouldBeGreaterThan(0d);
            provider.Config = FeedbackFamilyEffectTests.IsolationConfig(control: (viewIdentity,
                tree ? "ColorTextDisabled" : "ControlHeight", tree ? IsolationColor : "60"));
            using var byView = capture();
            FeedbackFamilyEffectTests.ChangedRatioOf(before, byView, item).ShouldBeGreaterThan(0d);
            provider.Config = FeedbackFamilyEffectTests.WithControlToken(viewIdentity, "MotionDurationMid", "00:00:00.4000000");
            if (tree)
            {
                AssertDuration(Part<TransferTreeViewItemHeader>(item), TimeSpan.FromMilliseconds(400));
                AssertDuration(Part<NodeSwitcherButton>(item), TimeSpan.FromMilliseconds(400));
            }
            else
            {
                AssertDuration(item, TimeSpan.FromMilliseconds(400));
            }
        });
    }

    [Fact]
    public void Spin_Indicator_Is_Transparent_And_Nested_Button_Is_Independent()
    {
        var spin = CreateSpin();
        var button = new Button { ButtonType = ButtonType.Primary, Content = "Nested" };
        spin.Content = button;
        FeedbackFamilyEffectTests.WithIsolationWindow(spin, (provider, capture) =>
        {
            ThemeScope.GetTokenOwner(Part<SpinIndicator>(spin)).ShouldBe(SpinTokens.Identity);
            ThemeScope.GetTokenOwner(button).ShouldBe(ButtonTokens.Identity);
            using var before = capture();
            provider.Config = FeedbackFamilyEffectTests.IsolationConfig(control: (SpinTokens.Identity, "ColorPrimary", IsolationColor));
            using var after = capture();
            FeedbackFamilyEffectTests.ChangedRatioOf(before, after, Part<SpinIndicator>(spin)).ShouldBeGreaterThan(0d);
            // Compare the actual button fill, away from the independently overlaid indicator.
            ((Spin)spin).IsSpinning = false;
            provider.Config = FeedbackFamilyEffectTests.IsolationConfig();
            using var closed = capture();
            provider.Config = FeedbackFamilyEffectTests.IsolationConfig(control: (SpinTokens.Identity, "ColorPrimary", IsolationColor));
            using var bySpin = capture();
            FeedbackFamilyEffectTests.ChangedRatioOf(closed, bySpin, button).ShouldBe(0d);
            provider.Config = FeedbackFamilyEffectTests.IsolationConfig(control: (ButtonTokens.Identity, "ColorPrimary", IsolationColor));
            using var byButton = capture();
            FeedbackFamilyEffectTests.ChangedRatioOf(closed, byButton, button).ShouldBeGreaterThan(0d);
        });
    }

    [Fact]
    public void Spin_And_Indicator_Receive_Own_And_Derived_Duration_Updates()
    {
        var spin = CreateSpin();
        spin.ClearValue(Spin.MotionDurationProperty);
        FeedbackFamilyEffectTests.WithProvider(spin, provider =>
        {
            var indicator = Part<SpinIndicator>(spin);
            provider.Config = FeedbackFamilyEffectTests.WithControlToken(SpinTokens.Identity, "IndicatorDuration", "00:00:00.4000000");
            FeedbackFamilyEffectTests.WaitFor(() => spin.MotionDuration == TimeSpan.FromMilliseconds(400));
            indicator.MotionDuration.ShouldBe(TimeSpan.FromMilliseconds(400));
            provider.Config = FeedbackFamilyEffectTests.WithControlToken(SpinTokens.Identity, "MotionDurationSlow", "00:00:00.2000000");
            FeedbackFamilyEffectTests.WaitFor(() => spin.MotionDuration == TimeSpan.FromMilliseconds(800));
            indicator.MotionDuration.ShouldBe(TimeSpan.FromMilliseconds(800));
            provider.Config = FeedbackFamilyEffectTests.WithControlToken(SpinTokens.Identity, "MotionDurationMid", "00:00:00.4000000");
            AssertDuration(spin, TimeSpan.FromMilliseconds(400));
        });
    }

    [Fact]
    public void QRCode_Null_Fallback_Reattaches_And_Explicit_Color_Does_Not_Rebuild_On_Publish()
    {
        var code = new QRCode { Color = null, Value = "https://atomui.net" };
        var host = new Panel { Children = { code } };
        FeedbackFamilyEffectTests.WithProvider(host, provider =>
        {
            code.Bitmap.ShouldNotBeNull();
            host.Children.Remove(code);
            var detachedBitmap = code.Bitmap;
            provider.Config = FeedbackFamilyEffectTests.IsolationConfig(control: (QRCodeTokens.Identity, "ColorText", "#ff00ff00"));
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            code.Bitmap.ShouldBeSameAs(detachedBitmap);
            host.Children.Add(code);
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            code.Bitmap.ShouldNotBeSameAs(detachedBitmap);
            ControlTokenAccessor.Capture(code).GetEffectiveGlobal<Color>(SharedTokenKind.ColorText).ShouldBe(Colors.Lime);
            code.Color = Brushes.Blue;
            var explicitBitmap = code.Bitmap;
            provider.Config = FeedbackFamilyEffectTests.IsolationConfig(control: (QRCodeTokens.Identity, "ColorText", "#ffff0000"));
            FeedbackFamilyEffectTests.WaitFor(() => ControlTokenAccessor.Capture(code).GetEffectiveGlobal<Color>(SharedTokenKind.ColorText) == Colors.Red);
            code.Bitmap.ShouldBeSameAs(explicitBitmap);
            code.Color.ShouldBe(Brushes.Blue);
        });
    }

    [Fact]
    public void Drawer_Bridge_Tracks_Local_Publish_And_Releases_On_Close_And_Reopen()
    {
        var nested = CreateEmpty();
        var drawer = new Drawer { Width = 1, Height = 1, Title = "Drawer", Content = nested, IsMotionEnabled = false, IsShowMask = false };
        var sibling = new Empty { Description = "Sibling", PresetImage = PresetEmptyImage.Default };
        FeedbackFamilyEffectTests.WithIsolationWindow(new StackPanel { Children = { drawer, sibling } }, (provider, capture) =>
        {
            drawer.IsOpen = true;
            using var before = capture();
            var container = (DrawerContainer)DrawerFrame(drawer);
            var surface = Part<DrawerInfoContainer>(container);
            var frame = Part<Border>(surface, "Frame");
            ThemeScope.GetTokenOwner(nested).ShouldBe(EmptyTokens.Identity);
            provider.Config = FeedbackFamilyEffectTests.IsolationConfig(control: (DrawerTokens.Identity, "ColorBgElevated", IsolationColor));
            using var after = capture();
            ((ISolidColorBrush)frame.Background!).Color.ShouldBe(Color.Parse(IsolationColor));
            FeedbackFamilyEffectTests.ChangedRatioOf(before, after, surface).ShouldBeGreaterThan(0d);
            FeedbackFamilyEffectTests.ChangedRatioOf(before, after, sibling).ShouldBe(0d);
            // The nested Empty reads its preset palette from its own Tokens (BgColor stays an unset override), so the
            // Drawer's ColorBgElevated does not reach it.
            nested.BgColor.ShouldBeNull();
            ControlTokenAccessor.Capture(nested).GetEffectiveGlobal<Color>(SharedTokenKind.ColorBgElevated)
                                .ShouldNotBe(Color.Parse(IsolationColor));
            drawer.IsOpen = false;
            capture().Dispose();
            container.Resources.MergedDictionaries.OfType<ThemeContextResourceBridge>().ShouldBeEmpty();
            provider.Config = FeedbackFamilyEffectTests.IsolationConfig(control: (DrawerTokens.Identity, "ColorBgElevated", "#ffff0000"));
            drawer.IsOpen = true;
            capture().Dispose();
            frame = Part<Border>(Part<DrawerInfoContainer>(DrawerFrame(drawer)), "Frame");
            ((ISolidColorBrush)frame.Background!).Color.ShouldBe(Colors.Red);
            drawer.ClearValue(Drawer.IsMotionEnabledProperty);
            provider.Config = FeedbackFamilyEffectTests.WithControlToken(DrawerTokens.Identity, "MotionDurationMid", "00:00:00.4000000");
            AssertDuration(Part<DrawerInfoContainer>(DrawerFrame(drawer)), TimeSpan.FromMilliseconds(400));
            // Close without motion so disposal is observed synchronously.
            drawer.IsMotionEnabled = false;
            ((DrawerContainer)DrawerFrame(drawer)).Resources.MergedDictionaries.OfType<ThemeContextResourceBridge>().Count().ShouldBe(1);
            drawer.IsOpen = false;
            capture().Dispose();
            ((DrawerContainer)container).Resources.MergedDictionaries.OfType<ThemeContextResourceBridge>().ShouldBeEmpty();
        });
    }
}
