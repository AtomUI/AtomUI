using AtomUI.Theme;
using AtomUI.Theme.Algorithms;
using AtomUI.Theme.Compilation;
using AtomUI.Theme.Configuration;
using Avalonia;
using Avalonia.Media;
using Shouldly;
using Xunit;

namespace AtomUI.Core.Tests.Theme;

/// <summary>
/// <c>ColorNeutral</c> is a nullable seed token: null by default, set only by a caller that chose a neutral color.
/// Setting it must not move any other global token of the Ant Design calculators (light, dark and compact chains).
/// </summary>
public class ColorNeutralSeedTests
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public async Task ColorNeutral_Is_A_Nullable_Seed_That_Changes_No_AntDesign_Token(bool dark, bool compact)
    {
        await HeadlessTestApp.RunAsync(async () =>
        {
            var builder = new ThemeManagerBuilder(Application.Current!.GetType());
            var manager = builder.Build();
            manager.InitializeApplication(Application.Current!);

            var chain = new List<ThemeAlgorithm> { ThemeAlgorithm.Default };
            if (compact)
            {
                chain.Add(ThemeAlgorithm.Compact);
            }
            if (dark)
            {
                chain.Add(ThemeAlgorithm.Dark);
            }

            var plain = await Apply(manager, new ThemeConfigBuilder().WithAlgorithms(chain.ToArray()).Build());
            var seeded = await Apply(manager, new ThemeConfigBuilder().WithAlgorithms(chain.ToArray())
                .WithToken("ColorNeutral", "#71717B").Build());

            plain.Registry.TryGetGlobalToken("ColorNeutral", out var neutral).ShouldBeTrue();
            plain.GlobalTokenValues.GetValue(neutral!.Slot).ShouldBeNull();
            seeded.GlobalTokenValues.GetValue(neutral.Slot).ShouldBe(Color.Parse("#71717B"));

            foreach (var token in plain.Registry.GlobalTokens)
            {
                if (token.Name == "ColorNeutral")
                {
                    continue;
                }

                seeded.GlobalTokenValues.GetValue(token.Slot)
                    .ShouldBe(plain.GlobalTokenValues.GetValue(token.Slot), token.Name);
            }
        });
    }

    private static async Task<ThemeSnapshot> Apply(ThemeManager manager, ThemeConfig config)
    {
        var result = await manager.ApplyThemeAsync(
            new ThemeRequest(IThemeManager.DEFAULT_THEME_ID, config, ThemeTransitionReason.UserRequest));
        result.Status.ShouldBeOneOf(ThemeTransitionStatus.Committed, ThemeTransitionStatus.NoOp);
        return manager.CurrentSnapshot!;
    }
}
