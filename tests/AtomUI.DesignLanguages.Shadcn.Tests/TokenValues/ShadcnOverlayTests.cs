using AtomUI.Theme;
using AtomUI.Theme.Compilation;
using AtomUI.Theme.Languages;
using AtomUI.Theme.Schema;
using Avalonia;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Shouldly;
using Xunit;

namespace AtomUI.DesignLanguages.Shadcn.Tests.TokenValues;

/// <summary>
/// The Nova control stages of the floating surfaces: menus and arrow flyouts, the dialog ring, the drawer shadow,
/// the mask and the inverted tooltip — against the appendix values and the design 5.6.1 shadow table.
/// </summary>
public class ShadcnOverlayTests
{
    [AvaloniaTheory]
    [InlineData("neutral", false)]
    [InlineData("neutral", true)]
    [InlineData("zinc", false)]
    [InlineData("zinc", true)]
    public async Task Menu_Popup_Shadow_Is_Ring_Then_Md(string family, bool dark)
    {
        var snapshot = await Apply(family, dark);
        var table = ShadcnAppendix.BaseColors();
        var ring = Ring(table, family, dark);

        var shadows = Own<BoxShadows>(snapshot, "Popup", "PopupRootShadow");
        shadows.Count.ShouldBe(3);
        shadows[0].OffsetX.ShouldBe(0);
        shadows[0].OffsetY.ShouldBe(0);
        shadows[0].Blur.ShouldBe(0);
        shadows[0].Spread.ShouldBe(1);
        shadows[0].Color.ShouldBe(ring);
        shadows.ShouldBe(BoxShadows.Parse("0 0 0 1 " + ArgbHex(ring) + ", 0 4 6 -1 #1A000000, 0 2 4 -2 #1A000000"));
        Own<BoxShadows>(snapshot, "Popup", "OverlayHostShadow").ShouldBe(shadows);
        Own<CornerRadius>(snapshot, "Popup", "PopupCornerRadius").ShouldBe(new CornerRadius(10));
    }

    [AvaloniaTheory]
    [InlineData("neutral", false)]
    [InlineData("neutral", true)]
    [InlineData("zinc", false)]
    [InlineData("zinc", true)]
    public async Task Arrow_Flyout_Has_Integrated_Border_And_Plain_Shadow(string family, bool dark)
    {
        var snapshot = await Apply(family, dark);
        var table = ShadcnAppendix.BaseColors();
        var ring = Ring(table, family, dark);

        // The flyout shadow has no ring: the arrow box draws the integrated one-line border.
        Own<BoxShadows>(snapshot, "FlyoutHost", "PopupRootShadow")
            .ShouldBe(BoxShadows.Parse("0 4 6 -1 #1A000000, 0 2 4 -2 #1A000000"));
        Own<BoxShadows>(snapshot, "FlyoutHost", "OverlayHostShadow")
            .ShouldBe(BoxShadows.Parse("0 4 6 -1 #1A000000, 0 2 4 -2 #1A000000"));
        Own<Color>(snapshot, "ArrowDecoratedBox", "ContentBorderColor").ShouldBe(ring);
        Own<Thickness>(snapshot, "ArrowDecoratedBox", "ContentBorderThickness").ShouldBe(new Thickness(1));
    }

    [AvaloniaTheory]
    [InlineData("neutral", false)]
    [InlineData("neutral", true)]
    public async Task Dialog_Has_Ring_Only(string family, bool dark)
    {
        var snapshot = await Apply(family, dark);
        var table = ShadcnAppendix.BaseColors();
        var ring = Ring(table, family, dark);

        // The dialog surface reads the global BoxShadows (DialogSurfaceTheme); Nova's dialog is the outline ring
        // alone, no shadow.
        Global<BoxShadows>(snapshot, "BoxShadows").Count.ShouldBe(1);
        Global<BoxShadows>(snapshot, "BoxShadows")[0].Spread.ShouldBe(1);
        Global<BoxShadows>(snapshot, "BoxShadows")[0].Color.ShouldBe(ring);
    }

    [AvaloniaTheory]
    [InlineData("neutral", false)]
    [InlineData("neutral", true)]
    public async Task Drawer_Shadow_Is_Lg(string family, bool dark)
    {
        var snapshot = await Apply(family, dark);
        var lg = BoxShadows.Parse("0 10 15 -3 #1A000000, 0 4 6 -4 #1A000000");

        Own<BoxShadows>(snapshot, "Drawer", "BoxShadowDrawerLeft").ShouldBe(lg);
        Own<BoxShadows>(snapshot, "Drawer", "BoxShadowDrawerRight").ShouldBe(lg);
        Own<BoxShadows>(snapshot, "Drawer", "BoxShadowDrawerUp").ShouldBe(lg);
        Own<BoxShadows>(snapshot, "Drawer", "BoxShadowDrawerDown").ShouldBe(lg);
    }

    [AvaloniaTheory]
    [InlineData("neutral", false)]
    [InlineData("neutral", true)]
    public async Task Mask_Is_Black_10(string family, bool dark)
    {
        var snapshot = await Apply(family, dark);

        Global<Color>(snapshot, "ColorBgMask").ShouldBe(Color.FromArgb(0x1A, 0, 0, 0));
    }

    [AvaloniaTheory]
    [InlineData("neutral", false)]
    [InlineData("neutral", true)]
    public async Task Tooltip_Is_Inverted(string family, bool dark)
    {
        var snapshot = await Apply(family, dark);
        var table = ShadcnAppendix.BaseColors();

        Own<Color>(snapshot, "ToolTip", "ToolTipBackground").ShouldBe(table[(family, dark, "foreground")]);
        Own<Color>(snapshot, "ToolTip", "ToolTipColor").ShouldBe(table[(family, dark, "background")]);
        Own<CornerRadius>(snapshot, "ToolTip", "ToolTipCornerRadius").ShouldBe(new CornerRadius(8));
        Own<Thickness>(snapshot, "ToolTip", "ContentPadding").ShouldBe(new Thickness(12, 6, 12, 6));
        Own<BoxShadows>(snapshot, "ToolTip", "ToolTipShadows").Count.ShouldBe(0);
    }

    [AvaloniaTheory]
    [InlineData("Vega")]
    [InlineData("Maia")]
    [InlineData("Lyra")]
    [InlineData("Mira")]
    [InlineData("Luma")]
    [InlineData("Sera")]
    [InlineData("Rhea")]
    public async Task Every_Style_Overlay_Matches_The_Design_Table(string style)
    {
        var row = OverlayTable.Single(item => item.Style == style);
        foreach (var dark in new[] { false, true })
        {
            var snapshot = await Apply(style, "neutral", dark);
            var table = ShadcnAppendix.BaseColors();
            var foreground = table[("neutral", dark, "foreground")];
            var ring = Color.FromArgb(Alpha(dark ? row.RingDark : row.RingLight), foreground.R, foreground.G, foreground.B);

            // Menu popup: the ring at the style's alpha ahead of the style's menu shadow, the style's radius.
            var expectedMenu = ShadcnRecipes.RingThenShadow(ring, Shadow(row.MenuShadow));
            Own<BoxShadows>(snapshot, "Popup", "PopupRootShadow").ShouldBe(expectedMenu, $"{style} menu");
            Own<CornerRadius>(snapshot, "Popup", "PopupCornerRadius").ShouldBe(new CornerRadius(row.MenuRadius));

            // Arrow flyouts: the plain popover shadow; the integrated border is the same ring colour.
            Own<BoxShadows>(snapshot, "FlyoutHost", "PopupRootShadow").ShouldBe(Shadow(row.PopoverShadow));

            // Drawer, mask, tooltip.
            var drawer = Shadow(row.DrawerShadow);
            Own<BoxShadows>(snapshot, "Drawer", "BoxShadowDrawerLeft").ShouldBe(drawer);
            Global<Color>(snapshot, "ColorBgMask").ShouldBe(Color.FromArgb(Alpha(row.Mask), 0, 0, 0));
            Own<CornerRadius>(snapshot, "ToolTip", "ToolTipCornerRadius").ShouldBe(new CornerRadius(row.TooltipRadius));
        }
    }

    [AvaloniaFact]
    public async Task Maia_Menu_Shadow_Is_2xl_With_Ring_5()
    {
        var snapshot = await Apply("Maia", "neutral", false);
        var foreground = ShadcnAppendix.BaseColors()[("neutral", false, "foreground")];
        var ring = Color.FromArgb(Alpha(5), foreground.R, foreground.G, foreground.B);

        Own<BoxShadows>(snapshot, "Popup", "PopupRootShadow")
            .ShouldBe(ShadcnRecipes.RingThenShadow(ring, Shadow("2xl")));
    }

    [AvaloniaFact]
    public async Task Luma_Dialog_Has_Xl_Shadow()
    {
        var snapshot = await Apply("Luma", "neutral", false);
        var foreground = ShadcnAppendix.BaseColors()[("neutral", false, "foreground")];
        var ring = Color.FromArgb(Alpha(5), foreground.R, foreground.G, foreground.B);

        // The dialog surface reads the dialog's effective global BoxShadows (control stage override).
        ControlGlobal<BoxShadows>(snapshot, "Dialog", "BoxShadows")
            .ShouldBe(ShadcnRecipes.RingThenShadow(ring, Shadow("xl")));
    }

    [AvaloniaFact]
    public async Task Lyra_Radius_Is_Zero_Everywhere()
    {
        var snapshot = await Apply("Lyra", "neutral", false);

        Global<CornerRadius>(snapshot, "BorderRadius").ShouldBe(new CornerRadius(0));
        Own<CornerRadius>(snapshot, "Popup", "PopupCornerRadius").ShouldBe(new CornerRadius(0));
        Own<CornerRadius>(snapshot, "ToolTip", "ToolTipCornerRadius").ShouldBe(new CornerRadius(0));
    }

    [AvaloniaFact]
    public async Task Mira_Focus_Ring_Is_2px_30()
    {
        var snapshot = await Apply("Mira", "neutral", false);
        var ring = ShadcnAppendix.BaseColors()[("neutral", false, "ring")];

        Global<double>(snapshot, "ControlOutlineWidth").ShouldBe(2);
        Global<Color>(snapshot, "ColorControlOutline").ShouldBe(Color.FromArgb(77, ring.R, ring.G, ring.B));
    }

    internal static T ControlGlobal<T>(ThemeSnapshot snapshot, string controlId, string globalName)
    {
        snapshot.Registry.TryGetControl(new ControlTokenIdentity("AtomUI", controlId), out var control)
                 .ShouldBeTrue(controlId);
        snapshot.Registry.TryGetGlobalToken(globalName, out var token).ShouldBeTrue(globalName);
        return snapshot.Controls[control!.Slot]
                       .GetEffectiveGlobalValue<T>(snapshot.GlobalTokenValues, token!.Slot);
    }

    private static byte Alpha(int percent) =>
        (byte)Math.Clamp(Math.Round(percent * 255 / 100d, MidpointRounding.AwayFromZero), 0, 255);

    private static Color Ring(IReadOnlyDictionary<(string, bool, string), Color> table, string family, bool dark)
    {
        // The outline ring of the floating surfaces: the foreground at 10% in both Nova appearances.
        var foreground = table[(family, dark, "foreground")];
        return Color.FromArgb(0x1A, foreground.R, foreground.G, foreground.B);
    }

    internal static Task<ThemeSnapshot> Apply(string family, bool dark) => Apply("Nova", family, dark);

    /// <summary>Applies the style in the appearance with the base colour of <paramref name="family"/>.</summary>
    internal static async Task<ThemeSnapshot> Apply(string style, string family, bool dark)
    {
        var manager = (ThemeManager)Application.Current!.GetThemeManager()!;
        var selection = ThemeSelection.Of("Shadcn", new DesignStyle(style))
                                      .WithAppearance(dark ? ThemeAppearance.Dark : ThemeAppearance.Light)
                                      .WithLanguageSeed(nameof(ShadcnToken.BaseColor), Hex(Seed(family)));
        var request = ThemeSelectionResolver.Resolve(manager.Languages, manager.Themes, selection).Request;
        var result = await manager.ApplyThemeAsync(request);
        result.Status.ShouldBeOneOf(ThemeTransitionStatus.Committed, ThemeTransitionStatus.NoOp);
        return manager.CurrentSnapshot!;
    }

    /// <summary>The official Tailwind v4 shadow levels (design 5.6.1).</summary>
    internal static BoxShadows Shadow(string level) => level switch
    {
        "none" => default,
        "xs"   => BoxShadows.Parse("0 1 2 0 #0D000000"),
        "sm"   => BoxShadows.Parse("0 1 3 0 #1A000000, 0 1 2 -1 #1A000000"),
        "md"   => BoxShadows.Parse("0 4 6 -1 #1A000000, 0 2 4 -2 #1A000000"),
        "lg"   => BoxShadows.Parse("0 10 15 -3 #1A000000, 0 4 6 -4 #1A000000"),
        "xl"   => BoxShadows.Parse("0 20 25 -5 #1A000000, 0 8 10 -6 #1A000000"),
        "2xl"  => BoxShadows.Parse("0 25 50 -12 #40000000"),
        _      => throw new ArgumentOutOfRangeException(nameof(level), level, null)
    };

    /// <summary>The overlay table of design 5.6.1: style → (menu shadow, menu radius, popover shadow, popover
    /// radius, ring alpha light, ring alpha dark, tooltip radius, dialog shadow, drawer shadow, mask %).</summary>
    internal static readonly (string Style, string MenuShadow, double MenuRadius, string PopoverShadow,
        double PopoverRadius, int RingLight, int RingDark, double TooltipRadius, string DialogShadow,
        string DrawerShadow, int Mask)[] OverlayTable =
    [
        ("Vega", "md", 8, "md", 8, 10, 10, 8, "none", "lg", 10),
        ("Nova", "md", 10, "md", 10, 10, 10, 8, "none", "lg", 10),
        ("Maia", "2xl", 18, "2xl", 18, 5, 10, 18, "none", "lg", 80),
        ("Lyra", "md", 0, "md", 0, 10, 10, 0, "none", "lg", 10),
        ("Mira", "md", 10, "md", 10, 10, 10, 8, "none", "lg", 80),
        ("Luma", "lg", 22, "lg", 22, 5, 10, 14, "xl", "xl", 30),
        ("Sera", "md", 0, "md", 0, 10, 10, 0, "md", "md", 20),
        ("Rhea", "lg", 18, "lg", 22, 5, 10, 14, "xl", "xl", 30)
    ];

    internal static Color Seed(string family) =>
        ShadcnAppendix.NeutralSeeds().Single(item => item.Family == family).Seed;

    internal static string Hex(Color color) => $"#{color.R:X2}{color.G:X2}{color.B:X2}";

    internal static string ArgbHex(Color color) => $"#{color.A:X2}{color.R:X2}{color.G:X2}{color.B:X2}";

    internal static T Global<T>(ThemeSnapshot snapshot, string name)
    {
        snapshot.Registry.TryGetGlobalToken(name, out var token).ShouldBeTrue(name);
        return snapshot.GlobalTokenValues.Get<T>(token!.Slot);
    }

    internal static T Own<T>(ThemeSnapshot snapshot, string controlId, string tokenName)
    {
        snapshot.Registry.TryGetControl(new ControlTokenIdentity("AtomUI", controlId), out var control)
                 .ShouldBeTrue(controlId);
        var token = control!.OwnTokens.Single(item => item.Name == tokenName);
        return snapshot.Controls[control.Slot].ControlTokenValues.Get<T>(token.Slot);
    }

    [AvaloniaTheory]
    [InlineData("Vega", false)]
    [InlineData("Vega", true)]
    public async Task Vega_Drawer_Has_The_Anchored_Side_Edge_Border(string style, bool dark)
    {
        var snapshot = await Apply(style, "neutral", dark);
        var foreground = ShadcnAppendix.BaseColors()[("neutral", dark, "foreground")];
        var ring = Color.FromArgb(0x1A, foreground.R, foreground.G, foreground.B);

        Own<double>(snapshot, "Drawer", "EdgeBorderThickness").ShouldBe(1, style);
        Own<Color>(snapshot, "Drawer", "EdgeBorderColor").ShouldBe(ring, style);
    }

    [AvaloniaFact]
    public async Task Other_Styles_Draw_No_Drawer_Edge_Border()
    {
        foreach (var style in new[] { "Nova", "Maia", "Luma" })
        {
            var snapshot = await Apply(style, "neutral", false);
            Own<double>(snapshot, "Drawer", "EdgeBorderThickness").ShouldBe(0, style);
        }
    }

    /// <summary>
    /// The menu family reads its own token scope (MenuPopupShadows / MenuPopupBorderColor), not Controls[Popup]:
    /// every menu identity gets the outline ring plus the style's menu shadow, and the flyout presenter's
    /// integrated border the ring colour.
    /// </summary>
    [AvaloniaTheory]
    [InlineData("Vega", "md")]
    [InlineData("Nova", "md")]
    [InlineData("Maia", "2xl")]
    [InlineData("Lyra", "md")]
    [InlineData("Mira", "md")]
    [InlineData("Luma", "lg")]
    [InlineData("Sera", "md")]
    [InlineData("Rhea", "lg")]
    public async Task Every_Style_Menu_Family_Carries_Ring_And_Menu_Shadow(string style, string level)
    {
        foreach (var dark in new[] { false, true })
        {
            var snapshot = await Apply(style, "neutral", dark);
            var foreground = ShadcnAppendix.BaseColors()[("neutral", dark, "foreground")];
            var percent = dark ? OverlayTable.Single(r => r.Style == style).RingDark
                               : OverlayTable.Single(r => r.Style == style).RingLight;
            var ring = Color.FromArgb((byte)Math.Round(percent * 2.55), foreground.R, foreground.G, foreground.B);
            var expected = ShadcnRecipes.RingThenShadow(ring, Shadow(level));

            foreach (var id in new[] { "Menu", "ContextMenu", "MenuFlyoutPresenter", "NavMenu" })
            {
                Own<BoxShadows>(snapshot, id, "MenuPopupShadows").ShouldBe(expected, $"{style} {(dark ? "D" : "L")} {id}");
            }
            Own<Color>(snapshot, "MenuFlyoutPresenter", "MenuPopupBorderColor").ShouldBe(ring, $"{style} presenter border");
            Own<Avalonia.Thickness>(snapshot, "MenuFlyoutPresenter", "MenuPopupBorderThickness").ShouldBe(new Avalonia.Thickness(1), $"{style} presenter border width");
        }
    }
}
