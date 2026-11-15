using AtomUI.Theme;
using AtomUI.Theme.Algorithms;
using AtomUI.Theme.Compilation;
using AtomUI.Theme.Configuration;
using Avalonia;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Shouldly;
using Xunit;

namespace AtomUI.Themes.Tests;

/// <summary>
/// The AntDesign design language (Core <c>DaybreakBlue</c>, <c>BrandColor="Seed"</c>): a brand seed must move every
/// brand-derived state in both appearances the Gallery reaches through the built-in algorithms.
/// </summary>
public class AntDesignBrandColorTests
{
    [AvaloniaFact]
    public async Task Brand_Seed_Drives_Derived_States_In_Both_Appearances()
    {
        var seed = Color.Parse("#52C41A");
        ThemeAlgorithm[][] chains = [[ThemeAlgorithm.Default], [ThemeAlgorithm.Default, ThemeAlgorithm.Dark]];
        var seededPrimaries = new List<Color>();
        foreach (var chain in chains)
        {
            var defaults = await Apply(new ThemeConfigBuilder().WithAlgorithms(chain).Build());
            var seeded = await Apply(new ThemeConfigBuilder().WithAlgorithms(chain)
                                                             .WithToken("ColorPrimary", "#52C41A").Build());

            foreach (var name in new[] { "ColorPrimary", "ColorPrimaryHover", "ColorPrimaryBg", "ColorControlOutline" })
            {
                Global<Color>(seeded, name).ShouldNotBe(Global<Color>(defaults, name), $"{name} [{string.Join(",", chain)}]");
            }

            Global<Color>(seeded, "ColorPrimaryHover").ShouldNotBe(seed);
            seededPrimaries.Add(Global<Color>(seeded, "ColorPrimary"));
        }

        // The dark algorithm maps the seed, so the dark ColorPrimary is not the light one.
        seededPrimaries[1].ShouldNotBe(seededPrimaries[0]);
        await Apply(null);
    }

    private static async Task<ThemeSnapshot> Apply(ThemeConfig? config)
    {
        var manager = (ThemeManager)Application.Current!.GetThemeManager()!;
        var request = new ThemeRequest(IThemeManager.DEFAULT_THEME_ID, config, ThemeTransitionReason.UserRequest);
        (await manager.ApplyThemeAsync(request, TestContext.Current.CancellationToken))
            .Status.ShouldBe(ThemeTransitionStatus.Committed);
        return manager.CurrentSnapshot!;
    }

    private static T Global<T>(ThemeSnapshot snapshot, string name)
    {
        snapshot.Registry.TryGetGlobalToken(name, out var token).ShouldBeTrue();
        return snapshot.GlobalTokenValues.Get<T>(token!.Slot);
    }
}
