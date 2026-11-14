using AtomUI.Generated.AtomUICore;
using AtomUI.Theme;
using AtomUI.Theme.Algorithms;
using AtomUI.Theme.Compilation;
using AtomUI.Theme.Configuration;
using AtomUI.Theme.Definitions;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Schema;
using Avalonia.Media;
using Shouldly;
using Xunit;

namespace AtomUI.Core.Tests.Theme;

/// <summary>
/// Contract and effect tests for the optional Alias stage of a theme algorithm and the compiler's staged
/// evaluation (spec 4.2–4.4).
/// </summary>
public class ThemeAliasAlgorithmTests
{
    private static readonly ThemeAlgorithm s_testX = new("Test", "X");

    [Fact]
    public void Alias_Stage_Value_Overrides_The_Ant_Default()
    {
        var snapshot = CompileSuccess([Alias(s_testX, static (in _, token) =>
            token.ColorTextPlaceholder = token.ColorTextSecondary)]);

        snapshot.Global<Color>(nameof(DesignToken.ColorTextPlaceholder))
                .ShouldBe(snapshot.Global<Color>(nameof(DesignToken.ColorTextSecondary)));
        snapshot.Global<Color>(nameof(DesignToken.ColorTextPlaceholder))
                .ShouldNotBe(snapshot.Global<Color>(nameof(DesignToken.ColorTextQuaternary)));
    }

    [Fact]
    public void Alias_Stage_Write_Drives_Derived_Defaults()
    {
        var snapshot = CompileSuccess([Alias(s_testX, static (in _, token) => token.ControlOutlineWidth = 0)]);

        snapshot.Global<double>(nameof(DesignToken.ControlOutlineWidth)).ShouldBe(0);
        snapshot.Global<BoxShadows>(nameof(DesignToken.InputActiveShadow))[0].Spread.ShouldBe(0);
    }

    [Fact]
    public void Explicit_Theme_Value_Wins_Over_The_Alias_Stage()
    {
        var snapshot = CompileSuccess(
            [Alias(s_testX, static (in _, token) => token.ControlOutlineWidth = 0)],
            [(nameof(DesignToken.ControlOutlineWidth), "3")]);

        snapshot.Global<double>(nameof(DesignToken.ControlOutlineWidth)).ShouldBe(3);
        snapshot.Global<BoxShadows>(nameof(DesignToken.InputActiveShadow))[0].Spread.ShouldBe(3);
    }

    [Fact]
    public void Writing_A_Map_Token_With_Its_Current_Value_Is_Still_A_Stage_Violation()
    {
        // A write is a setter call, not a value change.
        var exception = CompileFailure([Alias(s_testX, static (in _, token) =>
            token.ColorPrimary = token.ColorPrimary)]);

        exception.Algorithm.ShouldBe(s_testX);
        exception.TokenName.ShouldBe(nameof(DesignToken.ColorPrimary));
    }

    [Fact]
    public void Writing_Zero_Or_Transparent_Counts_As_Written()
    {
        var snapshot = CompileSuccess([Alias(s_testX, static (in _, token) =>
        {
            token.ControlOutlineWidth = 0;
            token.ColorControlOutline = Colors.Transparent;
        })]);

        var shadow = snapshot.Global<BoxShadows>(nameof(DesignToken.InputActiveShadow))[0];
        shadow.Spread.ShouldBe(0);
        shadow.Color.ShouldBe(Colors.Transparent);
    }

    [Fact]
    public void Later_Algorithm_In_The_Chain_Overrides_An_Earlier_One()
    {
        var snapshot = CompileSuccess([
            Alias(new ThemeAlgorithm("Test", "A"), static (in _, token) =>
            {
                token.ColorTextPlaceholder = token.ColorTextSecondary;
                token.ControlOutlineWidth  = 5;
            }),
            Alias(new ThemeAlgorithm("Test", "B"), static (in _, token) =>
                token.ColorTextPlaceholder = token.ColorTextTertiary)
        ]);

        snapshot.Global<Color>(nameof(DesignToken.ColorTextPlaceholder))
                .ShouldBe(snapshot.Global<Color>(nameof(DesignToken.ColorTextTertiary)));
        snapshot.Global<double>(nameof(DesignToken.ControlOutlineWidth)).ShouldBe(5);
    }

    [Fact]
    public void Context_Reports_The_Resolved_Appearance_And_The_Full_Chain()
    {
        var light = new ThemeAlgorithm("Test", "Light");
        var contexts = new List<ThemeAlgorithmContext>();
        var registryAlgorithms = GeneratedThemeSchema.GetAlgorithms();
        var dark = registryAlgorithms.Single(static algorithm => algorithm.Algorithm == ThemeAlgorithm.Dark);

        CompileSuccess(
            [Alias(light, (in context, _) => contexts.Add(context), ThemeAppearanceEffect.Light), dark]);

        contexts.ShouldNotBeEmpty();
        contexts[0].Appearance.ShouldBe(ThemeAppearance.Dark);
        contexts[0].Algorithms.ShouldBe([light, ThemeAlgorithm.Dark]);
    }

    [Fact]
    public void Writing_A_Map_Token_In_The_Alias_Stage_Fails_With_Algorithm_And_Token()
    {
        var exception = CompileFailure([Alias(s_testX, static (in _, token) =>
            token.ColorPrimary = Colors.Red)]);

        exception.Algorithm.ShouldBe(s_testX);
        exception.TokenName.ShouldBe(nameof(DesignToken.ColorPrimary));
        exception.Violation.ShouldBe(ThemeAlgorithmContractViolationKind.StageBoundary);
        exception.Message.ShouldBe("Algorithm 'Test:X' wrote token 'ColorPrimary' outside the Alias stage.");
    }

    [Fact]
    public void A_Self_Referential_Write_Is_Reported_As_Unstable()
    {
        var exception = CompileFailure([Alias(s_testX, static (in _, token) =>
            token.ControlOutlineWidth = token.ControlOutlineWidth + 1)]);

        exception.Algorithm.ShouldBe(s_testX);
        exception.TokenName.ShouldBe(nameof(DesignToken.ControlOutlineWidth));
        exception.Violation.ShouldBe(ThemeAlgorithmContractViolationKind.Unstable);
        exception.Message.ShouldBe(
            "Algorithm 'Test:X' did not stabilize after 3 rounds; unstable token 'ControlOutlineWidth'.");
    }

    [Fact]
    public void An_Exception_In_The_Alias_Stage_Names_The_Algorithm_And_Leaves_Recording_Off()
    {
        DesignToken? received = null;
        var failure = new InvalidOperationException("boom");

        var exception = CompileFailure([Alias(s_testX, (in _, token) =>
        {
            received = token;
            throw failure;
        })]);

        exception.Algorithm.ShouldBe(s_testX);
        exception.TokenName.ShouldBeNull();
        exception.Violation.ShouldBe(ThemeAlgorithmContractViolationKind.Failed);
        exception.Message.ShouldBe("Algorithm 'Test:X' failed in the Alias stage.");
        exception.InnerException.ShouldBeSameAs(failure);
        received.ShouldNotBeNull();
        received.IsRecordingWrites.ShouldBeFalse();
    }

    [Fact]
    public void BuiltIn_Chains_Create_No_Extra_Objects_And_Run_The_Default_Once()
    {
        var registry = CreateRegistry();
        var empty = Array.Empty<NormalizedTokenValue>();
        foreach (var identity in new[] { ThemeAlgorithm.Default, ThemeAlgorithm.Dark, ThemeAlgorithm.Compact })
        {
            registry.TryGetAlgorithm(identity, out var builtIn).ShouldBeTrue();
            ThemeAlgorithmDescriptor[] chain = [builtIn!];

            // Structural: the Map stage of a built-in chain collects no Alias-stage algorithm, so the Alias stage
            // takes the path that existed before the stage was introduced.
            var map = ThemeCompiler.EvaluateAlgorithms(
                new DesignToken(), chain, registry.GlobalTokens, out var aliasAlgorithms);
            aliasAlgorithms.ShouldBeNull();

            // Allocation: that path allocates exactly what one run of the Ant Alias formulas allocates. An extra
            // object (a context, a list, a closure, a write log) or a second run of the formulas changes the count.
            long direct = long.MaxValue;
            long staged = long.MaxValue;
            for (var sample = 0; sample < 40; sample++)
            {
                var before = GC.GetAllocatedBytesForCurrentThread();
                map.CalculateAliasTokenValues();
                var middle = GC.GetAllocatedBytesForCurrentThread();
                ThemeCompiler.CalculateAlias(map, empty, null, ThemeAppearance.Light, chain, registry.GlobalTokens);
                var after = GC.GetAllocatedBytesForCurrentThread();
                direct = Math.Min(direct, middle - before);
                staged = Math.Min(staged, after - middle);
            }

            direct.ShouldBeGreaterThan(0, "the formulas must allocate for a second run to be visible");
            staged.ShouldBe(direct, identity.ToString());
            map.IsRecordingWrites.ShouldBeFalse();
        }
    }

    [Fact]
    public void Control_Custom_Algorithm_Mode_Runs_The_Alias_Stage_For_The_Control_Global()
    {
        var algorithm = Alias(s_testX, static (in _, token) =>
            token.ColorTextPlaceholder = token.ColorTextSecondary);
        var control = ThemeCompilerTests.CreateCompilerButtonDescriptor();
        var registry = CreateRegistry([control], [algorithm]);
        registry.TryGetControl(control.Identity, out control).ShouldBeTrue();
        registry.TryGetAlgorithm(ThemeAlgorithm.Default, out var defaultAlgorithm).ShouldBeTrue();
        var config = new NormalizedControlThemeConfig(
            control!.Identity,
            ControlAlgorithmMode.Custom,
            [algorithm],
            Array.Empty<NormalizedTokenValue>(),
            Array.Empty<NormalizedTokenValue>());

        var result = new ThemeCompiler().Compile(
            CreateInput(registry, [defaultAlgorithm!], Array.Empty<NormalizedTokenValue>(), [config]));

        result.Success.ShouldBeTrue();
        var snapshot = result.Snapshot!;
        snapshot.EffectiveGlobal<Color>(control.Identity, nameof(DesignToken.ColorTextPlaceholder))
                .ShouldBe(snapshot.EffectiveGlobal<Color>(control.Identity, nameof(DesignToken.ColorTextSecondary)));
        snapshot.Global<Color>(nameof(DesignToken.ColorTextPlaceholder))
                .ShouldBe(snapshot.Global<Color>(nameof(DesignToken.ColorTextQuaternary)));
    }

    private delegate void AliasBody(in ThemeAlgorithmContext context, DesignToken token);

    private static ThemeAlgorithmDescriptor Alias(
        ThemeAlgorithm identity,
        AliasBody body,
        ThemeAppearanceEffect appearanceEffect = ThemeAppearanceEffect.Preserve)
    {
        return new ThemeAlgorithmDescriptor(identity, 1, appearanceEffect, () => new TestAliasAlgorithm(body));
    }

    private static ThemeSnapshot CompileSuccess(
        IReadOnlyList<ThemeAlgorithmDescriptor> chain,
        params (string Name, string Value)[] tokens)
    {
        var result = Compile(chain, tokens);
        result.Exception.ShouldBeNull();
        result.Success.ShouldBeTrue();
        return result.Snapshot!;
    }

    private static ThemeAlgorithmContractException CompileFailure(IReadOnlyList<ThemeAlgorithmDescriptor> chain)
    {
        var result = Compile(chain, []);
        result.Success.ShouldBeFalse();
        return result.Exception.ShouldBeOfType<ThemeAlgorithmContractException>();
    }

    private static ThemeCompileResult Compile(
        IReadOnlyList<ThemeAlgorithmDescriptor> chain,
        IReadOnlyList<(string Name, string Value)> tokens)
    {
        var custom = chain.Where(static algorithm => algorithm.Algorithm.Catalog != "AtomUI").ToArray();
        var registry = CreateRegistry(Array.Empty<ControlTokenDescriptor>(), custom);
        var values = tokens.Select(token =>
        {
            registry.TryGetGlobalToken(token.Name, out var descriptor).ShouldBeTrue();
            var parsed = descriptor!.Parse(token.Value);
            return new NormalizedTokenValue(descriptor, parsed, descriptor.Format(parsed));
        }).ToArray();
        return new ThemeCompiler().Compile(
            CreateInput(registry, chain, values, Array.Empty<NormalizedControlThemeConfig>()));
    }

    private static ThemeSchemaRegistry CreateRegistry(
        IReadOnlyList<ControlTokenDescriptor>? controls = null,
        IReadOnlyList<ThemeAlgorithmDescriptor>? custom = null)
    {
        return new ThemeSchemaRegistry(
            GeneratedThemeSchema.GetGlobalTokens(),
            controls ?? Array.Empty<ControlTokenDescriptor>(),
            GeneratedThemeSchema.GetAlgorithms().Concat(custom ?? []).ToArray());
    }

    private static ThemeCompileInput CreateInput(
        ThemeSchemaRegistry registry,
        IReadOnlyList<ThemeAlgorithmDescriptor> algorithms,
        IReadOnlyList<NormalizedTokenValue> globalTokens,
        IReadOnlyList<NormalizedControlThemeConfig> controls)
    {
        var definition = new PreparedThemeDefinition(
            "TestTheme",
            "Test Theme",
            ThemeAppearance.Light,
            ThemeAppearance.Light,
            true,
            algorithms,
            Array.Empty<BoundTokenValue>(),
            Array.Empty<ControlThemeDefinition>(),
            new ThemeSourceLocation("test", 1, 1, "/Theme"));
        var config = new NormalizedThemeConfig(false, true, algorithms, globalTokens, controls);
        return new ThemeCompileInput(
            definition,
            new ThemeDefinitionRevision("test", "1", "A1"),
            config,
            registry);
    }

    private sealed class TestAliasAlgorithm(AliasBody body) : IThemeAlgorithm, IThemeAliasAlgorithm
    {
        private readonly DefaultThemeVariantCalculator _map = new();

        public void Evaluate(DesignToken effectiveSeed, DesignToken? previousMap, DesignToken nextMap)
        {
            _map.Evaluate(effectiveSeed, previousMap, nextMap);
        }

        public void EvaluateAlias(in ThemeAlgorithmContext context, DesignToken token)
        {
            body(in context, token);
        }
    }
}
