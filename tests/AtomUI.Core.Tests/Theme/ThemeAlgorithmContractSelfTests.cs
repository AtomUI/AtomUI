using AtomUI.Theme;
using AtomUI.Theme.Algorithms;
using AtomUI.Theme.ContractTests;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Schema;
using Avalonia;
using Avalonia.Media;
using Shouldly;
using Xunit;

namespace AtomUI.Core.Tests.Theme;

/// <summary>
/// Proves that every check of <see cref="ThemeAlgorithmContract"/> can fail: each test registers an algorithm that
/// breaks exactly one rule of spec 4.6 and expects the verifier to report it.
/// </summary>
public class ThemeAlgorithmContractSelfTests
{
    private static readonly ThemeAlgorithm s_test = new("Test", "Contract");
    private static int s_counter;

    [Fact]
    public void MapComplete_Reports_A_Map_Token_The_Algorithm_Did_Not_Write()
    {
        var violations = Verify(Map((seed, previous, next) =>
        {
            var kept = next.ColorPrimaryBg;
            new DefaultThemeVariantCalculator().Evaluate(seed, previous, next);
            next.ColorPrimaryBg = kept;
        }));

        violations.ShouldContain(violation => violation.Check == ThemeAlgorithmContract.MapComplete &&
                                              violation.Algorithm == s_test &&
                                              violation.TokenName == nameof(DesignToken.ColorPrimaryBg));
    }

    [Fact]
    public void Deterministic_Reports_An_Algorithm_That_Reads_A_Static_Counter()
    {
        var violations = Verify(Map((seed, previous, next) =>
        {
            new DefaultThemeVariantCalculator().Evaluate(seed, previous, next);
            next.BorderRadiusLG = new CornerRadius(Interlocked.Increment(ref s_counter) % 1000);
        }));

        violations.ShouldContain(violation => violation.Check == ThemeAlgorithmContract.Deterministic &&
                                              violation.TokenName == nameof(DesignToken.BorderRadiusLG));
    }

    [Fact]
    public void InputsUnchanged_Reports_An_Algorithm_That_Mutates_The_Seed()
    {
        var violations = Verify(Map((seed, previous, next) =>
        {
            new DefaultThemeVariantCalculator().Evaluate(seed, previous, next);
            seed.ColorPrimary = Colors.Red;
        }));

        violations.ShouldContain(violation => violation.Check == ThemeAlgorithmContract.InputsUnchanged &&
                                              violation.TokenName == nameof(DesignToken.ColorPrimary));
    }

    [Fact]
    public void SeedResponsive_Reports_A_Hard_Coded_Primary()
    {
        var violations = Verify(Map((seed, previous, next) =>
        {
            new DefaultThemeVariantCalculator().Evaluate(seed, previous, next);
            next.ColorPrimary = Color.Parse("#1677FF");
        }));

        violations.ShouldContain(violation => violation.Check == ThemeAlgorithmContract.SeedResponsive &&
                                              violation.TokenName == nameof(DesignToken.ColorPrimary));
        violations.ShouldNotContain(violation => violation.Check == ThemeAlgorithmContract.SeedResponsive &&
                                                 violation.TokenName == nameof(DesignToken.ColorPrimaryHover));
    }

    [Fact]
    public void StatesDistinct_Reports_Identical_Hover_And_Active()
    {
        var violations = Verify(Map((seed, previous, next) =>
        {
            new DefaultThemeVariantCalculator().Evaluate(seed, previous, next);
            next.ColorPrimaryActive = next.ColorPrimaryHover;
        }));

        violations.ShouldContain(violation => violation.Check == ThemeAlgorithmContract.StatesDistinct &&
                                              violation.TokenName == nameof(DesignToken.ColorPrimaryActive));
    }

    [Fact]
    public void Readable_Reports_Low_Contrast_Text()
    {
        var violations = Verify(Map((seed, previous, next) =>
        {
            new DefaultThemeVariantCalculator().Evaluate(seed, previous, next);
            next.ColorText = Color.Parse("#EEEEEE");
        }));

        violations.ShouldContain(violation => violation.Check == ThemeAlgorithmContract.Readable &&
                                              violation.TokenName == nameof(DesignToken.ColorText));
    }

    [Fact]
    public void AppearanceConsistent_Reports_A_Light_Result_From_A_Dark_Algorithm()
    {
        var violations = Verify(Map(
            static (seed, previous, next) => new DefaultThemeVariantCalculator().Evaluate(seed, previous, next),
            ThemeAppearanceEffect.Dark));

        violations.ShouldContain(violation => violation.Check == ThemeAlgorithmContract.AppearanceConsistent &&
                                              violation.TokenName == nameof(DesignToken.ColorBgBase));
    }

    [Fact]
    public void StageBoundary_Reports_A_Map_Write_From_The_Alias_Stage()
    {
        var violations = Verify(Alias(static (in _, token) => token.ColorPrimary = Colors.Red));

        violations.ShouldContain(violation => violation.Check == ThemeAlgorithmContract.StageBoundary &&
                                              violation.Algorithm == s_test &&
                                              violation.TokenName == nameof(DesignToken.ColorPrimary));
    }

    [Fact]
    public void ContrastRatio_Matches_Known_Pairs()
    {
        ContrastRatio.Of(Colors.Black, Colors.White).ShouldBe(21, 1e-9);
        ContrastRatio.Of(Colors.White, Colors.Black).ShouldBe(21, 1e-9);
        ContrastRatio.Of(Color.Parse("#777777"), Colors.White).ShouldBe(4.48, 0.005);
        ContrastRatio.Of(Colors.White, Colors.White).ShouldBe(1, 1e-9);
        // A translucent foreground is composited over the background first: black at alpha 128 on white blends to
        // 255 × (1 − 128/255) = 127 per channel, i.e. #7F7F7F.
        ContrastRatio.Of(Color.FromArgb(128, 0, 0, 0), Colors.White)
                     .ShouldBe(ContrastRatio.Of(Color.FromRgb(127, 127, 127), Colors.White), 0.01);
    }

    [Fact]
    public void Identity_Reports_An_Identity_Registered_Twice()
    {
        var first = Map(static (seed, previous, next) => new DefaultThemeVariantCalculator().Evaluate(seed, previous, next));
        var second = Map(static (seed, previous, next) => new DefaultThemeVariantCalculator().Evaluate(seed, previous, next));
        var input = ThemeAlgorithmContractInput.Capture(builder => builder.AddThemeAlgorithms([first, second]));

        var violations = ThemeAlgorithmContract.Verify(input, s_test);

        violations.ShouldContain(violation => violation.Check == ThemeAlgorithmContract.Identity &&
                                              violation.Algorithm == s_test);
    }

    [Fact]
    public void Stable_Reports_A_Self_Referential_Alias_Write()
    {
        var violations = Verify(Alias(static (in _, token) =>
            token.ControlOutlineWidth = token.ControlOutlineWidth + 1));

        violations.ShouldContain(violation => violation.Check == ThemeAlgorithmContract.Stable &&
                                              violation.TokenName == nameof(DesignToken.ControlOutlineWidth));
    }

    [Fact]
    public void ComposableWithCompact_Reports_A_Color_That_Follows_The_Control_Height()
    {
        var violations = Verify(Alias(static (in _, token) =>
            token.ColorTextPlaceholder = token.ControlHeight < 30 ? Colors.Red : Colors.Blue));

        violations.ShouldContain(violation => violation.Check == ThemeAlgorithmContract.ComposableWithCompact &&
                                              violation.TokenName == nameof(DesignToken.ColorTextPlaceholder));
    }

    [Fact]
    public void HybridChainReadable_Reports_An_Alias_Stage_That_Breaks_In_The_Dark_Appearance()
    {
        var violations = Verify(Alias(
            static (in context, token) => token.ColorTextLightSolid =
                context.Appearance == ThemeAppearance.Dark ? token.ColorPrimary : Colors.White,
            ThemeAppearanceEffect.Light));

        violations.ShouldNotContain(violation => violation.Check == ThemeAlgorithmContract.Readable);
        violations.ShouldContain(violation => violation.Check == ThemeAlgorithmContract.HybridChainReadable &&
                                              violation.TokenName == nameof(DesignToken.ColorTextLightSolid));
    }

    [Fact]
    public void HybridChainReadable_Reports_A_Preserve_Algorithm_That_Is_Unreadable_Under_Dark()
    {
        // A dark grey that ignores context.Appearance: contrast about 3.2 on the light primary #1677FF, about 2.5 on
        // the dark primary #1668DC, so it passes on [Default, X] and fails once AtomUI:Dark follows.
        var descriptor = new ThemeAlgorithmDescriptor(s_test, 1, ThemeAppearanceEffect.Preserve,
            static () => new FollowerAliasAlgorithm(static (in _, token) =>
                token.ColorTextLightSolid = Color.FromRgb(0x30, 0x30, 0x30)));

        var violations = Verify(descriptor, standalone: false);

        violations.ShouldNotContain(violation => violation.Check == ThemeAlgorithmContract.Readable);
        violations.ShouldContain(violation => violation.Check == ThemeAlgorithmContract.HybridChainReadable &&
                                              violation.Algorithm == s_test &&
                                              violation.TokenName == nameof(DesignToken.ColorTextLightSolid));
    }

    private static IReadOnlyList<ThemeAlgorithmContractViolation> Verify(
        ThemeAlgorithmDescriptor descriptor,
        bool standalone = true)
    {
        var input = ThemeAlgorithmContractInput.Capture(builder => builder.AddThemeAlgorithms([descriptor]));
        return ThemeAlgorithmContract.Verify(input, descriptor.Algorithm, standalone);
    }

    private delegate void MapBody(DesignToken effectiveSeed, DesignToken? previousMap, DesignToken nextMap);

    private delegate void AliasBody(in ThemeAlgorithmContext context, DesignToken token);

    private static ThemeAlgorithmDescriptor Map(
        MapBody body,
        ThemeAppearanceEffect appearanceEffect = ThemeAppearanceEffect.Light)
    {
        return new ThemeAlgorithmDescriptor(s_test, 1, appearanceEffect, () => new MapAlgorithm(body));
    }

    private static ThemeAlgorithmDescriptor Alias(
        AliasBody body,
        ThemeAppearanceEffect appearanceEffect = ThemeAppearanceEffect.Light)
    {
        return new ThemeAlgorithmDescriptor(s_test, 1, appearanceEffect, () => new AliasAlgorithm(body));
    }

    private sealed class MapAlgorithm(MapBody body) : IThemeAlgorithm
    {
        public void Evaluate(DesignToken effectiveSeed, DesignToken? previousMap, DesignToken nextMap) =>
            body(effectiveSeed, previousMap, nextMap);
    }

    private sealed class AliasAlgorithm(AliasBody body) : IThemeAliasAlgorithm
    {
        public void Evaluate(DesignToken effectiveSeed, DesignToken? previousMap, DesignToken nextMap) =>
            new DefaultThemeVariantCalculator().Evaluate(effectiveSeed, previousMap, nextMap);

        public void EvaluateAlias(in ThemeAlgorithmContext context, DesignToken token) => body(in context, token);
    }

    // A follower whose Map stage keeps the previous map (nextMap starts as its copy) and only adds an Alias stage.
    private sealed class FollowerAliasAlgorithm(AliasBody body) : IThemeAliasAlgorithm
    {
        public void Evaluate(DesignToken effectiveSeed, DesignToken? previousMap, DesignToken nextMap)
        {
        }

        public void EvaluateAlias(in ThemeAlgorithmContext context, DesignToken token) => body(in context, token);
    }
}
