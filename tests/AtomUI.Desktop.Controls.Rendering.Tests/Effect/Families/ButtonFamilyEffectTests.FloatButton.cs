using System.Diagnostics;
using AtomUI.Controls;
using AtomUI.Controls.Commons;
using AtomUI.Controls.Primitives;
using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Icons.AntDesign;
using AtomUI.Theme;
using AtomUI.Theme.Resources;
using AtomUI.Theme.Schema;
using Avalonia.Controls;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Rendering.Tests.Effect.Families;

public partial class ButtonFamilyEffectTests
{
    /// <summary>
    /// Global Tokens that the shared <c>AbstractFloatButtonTheme</c> reads through <c>OwnerTokenResource</c>. The
    /// consumption map attributes them to the primitive, so they are listed here and checked through each owner.
    /// </summary>
    private static readonly string[] s_floatButtonOwnerReads =
    [
        "BorderRadiusLG", "BorderRadiusSM", "ColorError",
        "ColorPrimary", "ColorPrimaryActive", "ColorPrimaryHover", "EnableMotion", "FontSizeSM",
        "MarginXXS", "PaddingXXS", "ZIndexPopupBase"
    ];

    /// <summary>Global Tokens the internal FloatButtonItemsControl theme reads from its FloatButtonGroup owner.</summary>
    private static readonly string[] s_floatButtonGroupOwnerReads =
        ["BorderRadiusLG", "ColorSplit", "Spacing"];

    [Fact]
    public void Owner_Read_Lists_Match_The_Primitive_Themes()
    {
        // Drift guard: a new OwnerTokenResource read in a primitive theme must get an effect row through its owners.
        ThemeOwnerReads.Read("src/AtomUI.Desktop.Controls/FloatButton/Themes/AbstractFloatButtonTheme.axaml")
                       .Order(StringComparer.Ordinal)
                       .ShouldBe(s_floatButtonOwnerReads.Order(StringComparer.Ordinal));
        ThemeOwnerReads.Read("src/AtomUI.Desktop.Controls/FloatButton/Themes/FloatButtonItemsControlTheme.axaml")
                       .Order(StringComparer.Ordinal)
                       .ShouldBe(s_floatButtonGroupOwnerReads.Order(StringComparer.Ordinal));
    }

    public static IEnumerable<object[]> FloatButtonRows() =>
        WithOwnerReads(EffectTestData.For("AtomUI", "FloatButton"), s_floatButtonOwnerReads);

    [Theory]
    [MemberData(nameof(FloatButtonRows))]
    public async Task FloatButton_Token_Has_Effect(string token, bool isOwn)
    {
        _ = isOwn;
        if (token is "ColorBorder" or "BorderThickness" or "BorderRadius" or "BorderRadiusMD" or "IconSize" or "FontSize" or "SpacingXS" or "PaddingLG" or "ControlHeight")
        {
            await ApplyShadcnRecipeTheme();
        }
        if (token is "FloatOffsetX" or "FloatOffsetY" or "UniformlyMargin")
        {
            // The host places the overlay button at its offsets; the button moves inside the overlay layer.
            Run(new EffectSetup(
                    static () => OverlayHost(new FloatButtonHost { Icon = new SearchOutlined() }),
                    Options: new EffectOptions
                    {
                        Target = static root => OverlayButton<FloatButton>(root),
                        Region = static button => button.GetVisualAncestors().OfType<ScopeAwareOverlayLayerPanel>().First()
                    }),
                FloatButtonTokens.Identity, token);
            return;
        }

        if (token == "ZIndexPopupBase")
        {
            AssertZIndexFollowsToken(static () => new FloatButton { Icon = new SearchOutlined() }, FloatButtonTokens.Identity);
            return;
        }

        Run(FloatButtonSetup(token, static () => new FloatButton { Icon = new SearchOutlined() }), FloatButtonTokens.Identity, token);
    }

    public static IEnumerable<object[]> BackTopFloatButtonRows() =>
        WithOwnerReads(EffectTestData.For("AtomUI", "BackTopFloatButton"), s_floatButtonOwnerReads);

    [Theory]
    [MemberData(nameof(BackTopFloatButtonRows))]
    public void BackTopFloatButton_Token_Has_Effect(string token, bool isOwn)
    {
        _ = isOwn;
        if (token == "ZIndexPopupBase")
        {
            AssertZIndexFollowsToken(static () => new BackTopFloatButton(), BackTopFloatButtonTokens.Identity);
            return;
        }

        if (token is "ColorBorderSecondary" or "LineWidthBold" or "ColorPrimary")
        {
            // The progress ring shows the track (ColorBorderSecondary) and the scrolled part (ColorPrimary).
            Run(new EffectSetup(static () =>
                {
                    var button = new BackTopFloatButton { IsShowProgress = true };
                    button.SetCurrentValue(AbstractBackTopFloatButton.ScrollProgressProperty, 0.5);
                    return button;
                }),
                BackTopFloatButtonTokens.Identity, token);
            return;
        }

        Run(FloatButtonSetup(token, static () => new BackTopFloatButton()), BackTopFloatButtonTokens.Identity, token);
    }

    public static IEnumerable<object[]> FloatButtonGroupRows() =>
        WithOwnerReads(EffectTestData.For("AtomUI", "FloatButtonGroup"), s_floatButtonGroupOwnerReads);

    [Theory]
    [MemberData(nameof(FloatButtonGroupRows))]
    public void FloatButtonGroup_Token_Has_Effect(string token, bool isOwn)
    {
        _ = isOwn;
        var shape = token == "Spacing" ? FloatButtonShape.Circle : FloatButtonShape.Square;
        Run(new EffectSetup(() =>
            {
                var group = new FloatButtonGroup { Shape = shape };
                group.Children.Add(new FloatButton { Icon = new SearchOutlined() });
                group.Children.Add(new FloatButton { Icon = new SearchOutlined() });
                return group;
            }),
            FloatButtonGroupTokens.Identity, token);
    }

    public static IEnumerable<object[]> FloatButtonHostRows() => EffectTestData.For("AtomUI", "FloatButtonHost");

    [Theory]
    [MemberData(nameof(FloatButtonHostRows))]
    public void FloatButtonHost_Token_Has_Effect(string token, bool isOwn)
    {
        _ = isOwn;
        Run(HostSetup(static () => new FloatButtonHost { Icon = new SearchOutlined() }), FloatButtonHostTokens.Identity, token);
    }

    public static IEnumerable<object[]> FloatButtonGroupHostRows() => EffectTestData.For("AtomUI", "FloatButtonGroupHost");

    [Theory]
    [MemberData(nameof(FloatButtonGroupHostRows))]
    public void FloatButtonGroupHost_Token_Has_Effect(string token, bool isOwn)
    {
        _ = isOwn;
        Run(HostSetup(static () =>
            {
                var host = new FloatButtonGroupHost { Shape = FloatButtonShape.Square };
                host.Children.Add(new FloatButton { Icon = new SearchOutlined() });
                host.Children.Add(new FloatButton { Icon = new SearchOutlined() });
                return host;
            }, static root => root.GetVisualDescendants().OfType<FloatButtonGroup>().First()),
            FloatButtonGroupHostTokens.Identity, token);
    }

    public static IEnumerable<object[]> BackTopFloatButtonHostRows() => EffectTestData.For("AtomUI", "BackTopFloatButtonHost");

    [Theory]
    [MemberData(nameof(BackTopFloatButtonHostRows))]
    public void BackTopFloatButtonHost_Token_Has_Effect(string token, bool isOwn)
    {
        _ = isOwn;
        Run(HostSetup(static () => new BackTopFloatButtonHost()), BackTopFloatButtonHostTokens.Identity, token);
    }

    // ---- FloatButton helpers ------------------------------------------------------------------

    private static IEnumerable<object[]> WithOwnerReads(IEnumerable<object[]> rows, IEnumerable<string> ownerReads)
    {
        var list = rows.ToList();
        var seen = list.Select(static row => (string)row[0]).ToHashSet(StringComparer.Ordinal);
        list.AddRange(ownerReads.Where(seen.Add).Select(static token => new object[] { token, false }));
        return list;
    }

    private static EffectSetup FloatButtonSetup(string token, Func<AbstractFloatButton> factory)
    {
        Func<Control> With(Action<AbstractFloatButton> init) => () =>
        {
            var button = factory();
            init(button);
            return button;
        };

        switch (token)
        {
            case "BorderRadius" or "BorderRadiusMD":
                return new(With(static b => b.Shape = FloatButtonShape.Square));
            case "IconSize" or "FontSize" or "SpacingXS" or "PaddingLG" or "ControlHeight":
                return new(With(static b =>
                {
                    b.Shape = FloatButtonShape.Square;
                    b.Content = "Help info";
                }));
            case "DescriptionLineHeight" or "FontSizeSM" or "PaddingXXS" or "BorderRadiusLG" or "DescriptionWeight":
                return new(With(static b =>
                {
                    b.Shape   = FloatButtonShape.Square;
                    b.Content = "Help";
                }));
            case "OutlineDescriptionLineHeight":
                // The outline line height starts at 0 (unused while the outline is off); a clear value makes it show.
                return new(With(static b =>
                {
                    b.Shape   = FloatButtonShape.Square;
                    b.Content = "Help";
                }), Options: new EffectOptions { Perturb = static _ => 24.0 });
            case "BorderRadiusSM" or "MarginXXS":
                // Embedded square buttons (inside a group) use the small radius and an inner margin.
                return new(With(static b =>
                {
                    b.Shape       = FloatButtonShape.Square;
                    b.IsEmbedMode = true;
                    b.Background  = Avalonia.Media.Brushes.SteelBlue;
                }));
            case "ColorBgTextHover":
                return new(With(static _ => { }), Pseudo(":pointerover"));
            case "DefaultHoverBg":
                return new(With(static _ => { }), Pseudo(":pointerover"));
            case "DefaultActiveBg":
                return new(With(static _ => { }), Pseudo(":pressed"));
            case "PrimaryColor" or "ColorTextLightSolid" or "ColorPrimary":
                return new(With(static b => b.ButtonType = FloatButtonType.Primary));
            case "ColorPrimaryHover":
                return new(With(static b => b.ButtonType = FloatButtonType.Primary), Pseudo(":pointerover"));
            case "ColorPrimaryActive":
                return new(With(static b => b.ButtonType = FloatButtonType.Primary), Pseudo(":pressed"));
            case "ColorError":
                return new(With(static b =>
                {
                    b.IsBadgeEnabled = true;
                    b.BadgeCount     = 5;
                }), Options: s_shadowMargin);
            default:
                return new(With(static _ => { }));
        }
    }

    private static Control OverlayHost(Control host)
    {
        var panel = new ScopeAwareOverlayLayerPanel { Width = 360, Height = 280 };
        panel.Children.Add(host);
        return panel;
    }

    private static TButton OverlayButton<TButton>(Control root) where TButton : Control =>
        ScopeAwareOverlayLayer.FindLayer(root).ShouldNotBeNull().GetVisualDescendants().OfType<TButton>().First();

    /// <summary>
    /// Hosts are placeholders in the layout; what they show lives in the overlay layer, so the comparison region is the
    /// overlay visual (<paramref name="overlayVisual"/>, or the overlay layer when omitted).
    /// </summary>
    private static EffectSetup HostSetup(Func<Control> createHost, Func<Control, Control>? overlayVisual = null)
    {
        return new EffectSetup(
            () => OverlayHost(createHost()),
            Options: new EffectOptions
            {
                Target = static root => ((Panel)root).Children[0],
                Region = control =>
                {
                    var panel = control.GetVisualAncestors().OfType<ScopeAwareOverlayLayerPanel>().First();
                    return overlayVisual?.Invoke(ScopeAwareOverlayLayer.FindLayer(panel).ShouldNotBeNull()) ?? panel;
                }
            });
    }

    /// <summary>ZIndexPopupBase is an integer stacking order: it has no pixels of its own, the ZIndex must follow it.</summary>
    private static void AssertZIndexFollowsToken(Func<Control> create, ControlTokenIdentity identity)
    {
        var control  = create();
        var provider = new ThemeConfigProvider { Config = BaselineConfig(null), Child = control };
        var window   = new AvaloniaWindow { Width = 300, Height = 200, Content = provider };
        try
        {
            window.Show();
            SettleWindow(window);
            var before = control.ZIndex;
            ControlTokenAccessor.Capture(control).GetEffectiveGlobal<int>(SharedTokenKind.ZIndexPopupBase).ShouldBe(before);
            provider.Config = BaselineConfig((identity, "ZIndexPopupBase", (before + 100).ToString()));
            SettleWindow(window);
            control.ZIndex.ShouldBe(before + 100);
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    private static void SettleWindow(AvaloniaWindow window)
    {
        var stopwatch = Stopwatch.StartNew();
        while (stopwatch.Elapsed < TimeSpan.FromMilliseconds(200))
        {
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            Thread.Sleep(5);
        }
    }
}
