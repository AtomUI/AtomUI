using AtomUI.Generated.AtomUICore;
using AtomUI.Registration;
using AtomUI.Theme;
using AtomUI.Theme.Algorithms;
using AtomUI.Theme.Compilation;
using AtomUI.Theme.Configuration;
using AtomUI.Theme.ContractTests;
using AtomUI.Theme.Definitions;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Resources;
using AtomUI.Theme.Schema;
using Avalonia.Controls;
using Avalonia.Media;
using Shouldly;
using Xunit;

namespace AtomUI.Core.Tests.Theme;

/// <summary>
/// Effect and contract tests for the control token stage of a theme algorithm as the compiler runs it (spec 4.3):
/// <c>EvaluateGlobal</c> on the control's own global copy, then the control-scoped global overrides, then the Own
/// tokens through the staged evaluation.
/// </summary>
public class ControlTokenAlgorithmCompilationTests
{
    private static readonly ThemeAlgorithm s_x = new("Test", "X");
    private static readonly ThemeAlgorithm s_y = new("Test", "Y");

    [Fact]
    public void EvaluateOwn_Value_Overrides_The_Control_Default()
    {
        var snapshot = CompileSuccess([Stage<AccentRed>(s_x)]);

        snapshot.Own<Color>(nameof(StageProbeToken.Accent)).ShouldBe(Colors.Red);
        snapshot.Global<Color>(nameof(DesignToken.ColorPrimary)).ShouldNotBe(Colors.Red);
    }

    [Fact]
    public void EvaluateOwn_Write_To_A_Guarded_Token_Drives_Derived_Own_Tokens()
    {
        var snapshot = CompileSuccess([Stage<BaseSize50>(s_x)]);

        snapshot.Own<double>(nameof(StageProbeToken.BaseSize)).ShouldBe(50);
        snapshot.Own<double>(nameof(StageProbeToken.DerivedHeight)).ShouldBe(60);
    }

    [Fact]
    public void Explicit_Control_Token_Wins_Over_EvaluateOwn()
    {
        var snapshot = CompileSuccess(
            [Stage<BaseSize50>(s_x)],
            ownTokens: [(nameof(StageProbeToken.BaseSize), "7")]);

        snapshot.Own<double>(nameof(StageProbeToken.BaseSize)).ShouldBe(7);
        snapshot.Own<double>(nameof(StageProbeToken.DerivedHeight)).ShouldBe(17);
    }

    [Fact]
    public void EvaluateGlobal_Changes_The_Global_Value_For_That_Control_Only()
    {
        var snapshot = CompileSuccess([Stage<GlobalPrimaryRed>(s_x)]);
        var button = ThemeCompilerTests.CreateCompilerButtonDescriptor().Identity;

        snapshot.EffectiveGlobal<Color>(StageProbeToken.ControlIdentity, nameof(DesignToken.ColorPrimary))
                .ShouldBe(Colors.Red);
        // The write takes effect directly: Map and Alias are not rerun, so the derived states keep the scope value.
        snapshot.EffectiveGlobal<Color>(StageProbeToken.ControlIdentity, nameof(DesignToken.ColorPrimaryHover))
                .ShouldBe(snapshot.Global<Color>(nameof(DesignToken.ColorPrimaryHover)));
        // The control's default calculation reads the written value.
        snapshot.Own<Color>(nameof(StageProbeToken.Accent)).ShouldBe(Colors.Red);
        // Another control in the same scope, and the scope itself, still see the scope value.
        snapshot.Global<Color>(nameof(DesignToken.ColorPrimary)).ShouldNotBe(Colors.Red);
        snapshot.EffectiveGlobal<Color>(button, nameof(DesignToken.ColorPrimary))
                .ShouldBe(snapshot.Global<Color>(nameof(DesignToken.ColorPrimary)));
    }

    [Fact]
    public void Explicit_Control_Scoped_Global_Override_Wins_Over_EvaluateGlobal()
    {
        var snapshot = CompileSuccess(
            [Stage<GlobalPrimaryRed>(s_x)],
            globalTokens: [(nameof(DesignToken.ColorPrimary), "#0000FF")]);

        snapshot.EffectiveGlobal<Color>(StageProbeToken.ControlIdentity, nameof(DesignToken.ColorPrimary))
                .ShouldBe(Colors.Blue);
        snapshot.Own<Color>(nameof(StageProbeToken.Accent)).ShouldBe(Colors.Blue);
    }

    [Fact]
    public void EvaluateOwn_Sees_The_Final_Effective_Global_Including_EvaluateGlobal_Writes()
    {
        // EvaluateGlobal writes ControlHeight; the control-scoped override writes ColorPrimary. EvaluateOwn copies
        // both into own tokens, so it must run after both.
        var snapshot = CompileSuccess(
            [Stage<HeightThenCopy>(s_x)],
            globalTokens: [(nameof(DesignToken.ColorPrimary), "#0000FF")]);

        snapshot.Own<double>(nameof(StageProbeToken.BaseSize)).ShouldBe(78);
        snapshot.Own<double>(nameof(StageProbeToken.DerivedHeight)).ShouldBe(88);
        snapshot.Own<Color>(nameof(StageProbeToken.Accent)).ShouldBe(Colors.Blue);
    }

    [Fact]
    public void EvaluateOwn_Mutating_EffectiveGlobal_Is_Reported()
    {
        var snapshot = CompileSuccess([Stage<MutatesEffectiveGlobal>(s_x)]);

        var exception = Should.Throw<ThemeAlgorithmContractException>(() => snapshot.ProbeSnapshot());

        exception.Algorithm.ShouldBe(s_x);
        exception.TokenName.ShouldBe(nameof(DesignToken.ColorPrimary));
        exception.Violation.ShouldBe(ThemeAlgorithmContractViolationKind.StageBoundary);
        exception.Message.ShouldBe("Algorithm 'Test:X' wrote token 'ColorPrimary' outside the Control stage.");
    }

    [Theory]
    [InlineData(ControlAlgorithmMode.Unspecified)]
    [InlineData(ControlAlgorithmMode.Disabled)]
    [InlineData(ControlAlgorithmMode.Global)]
    [InlineData(ControlAlgorithmMode.Custom)]
    public void Control_Stage_Runs_With_The_Right_Chain_In_Every_Mode(ControlAlgorithmMode mode)
    {
        // The scope chain is [Default, X]; the control's own chain is [Default, Compact, Y]. Each stage writes its
        // marker colour and 100 × the length of the chain it was given.
        var snapshot = CompileSuccess(
            [Stage<ChainProbeX>(s_x), Stage<ChainProbeY>(s_y)],
            mode: mode);

        var custom = mode == ControlAlgorithmMode.Custom;
        snapshot.Own<Color>(nameof(StageProbeToken.Accent)).ShouldBe(custom ? Colors.Green : Colors.Red);
        snapshot.Own<double>(nameof(StageProbeToken.BaseSize)).ShouldBe(custom ? 300 : 200);
        snapshot.Own<double>(nameof(StageProbeToken.DerivedHeight)).ShouldBe(custom ? 310 : 210);
        snapshot.EffectiveGlobal<double>(StageProbeToken.ControlIdentity, nameof(DesignToken.LineWidth))
                .ShouldBe(custom ? 3 : 2);
    }

    [Fact]
    public void Later_Algorithm_In_The_Chain_Overrides_An_Earlier_Control_Stage()
    {
        var snapshot = CompileSuccess(
            [Stage<EarlierStage>(s_x), Stage<LaterStage>(s_y)],
            chain: [s_x, s_y]);

        snapshot.Own<Color>(nameof(StageProbeToken.Accent)).ShouldBe(Colors.Green);
        snapshot.Own<double>(nameof(StageProbeToken.BaseSize)).ShouldBe(50);
        snapshot.EffectiveGlobal<double>(StageProbeToken.ControlIdentity, nameof(DesignToken.ControlHeight))
                .ShouldBe(88);
        snapshot.EffectiveGlobal<double>(StageProbeToken.ControlIdentity, nameof(DesignToken.LineWidth))
                .ShouldBe(5);
    }

    [Fact]
    public void Changing_A_Control_Algorithm_Revision_Invalidates_That_Controls_Cache_Entry()
    {
        var revision1 = Stage<AccentRed>(s_x, revision: 1);
        var revision2 = Stage<AccentRed>(s_x, revision: 2);
        var registry = CreateRegistry([revision1]);
        var input = CreateInput(registry, [Default(registry), Follower(registry, s_x)], null);
        var globalKey = GlobalCompilationCacheKey.Create(input);
        registry.TryGetControl(StageProbeToken.ControlIdentity, out var probe).ShouldBeTrue();

        var first = ControlCompilationCacheKey.Create(globalKey, probe!, null, ThemeAppearance.Light, [revision1]);
        var same = ControlCompilationCacheKey.Create(globalKey, probe!, null, ThemeAppearance.Light, [revision1]);
        var changed = ControlCompilationCacheKey.Create(globalKey, probe!, null, ThemeAppearance.Light, [revision2]);
        var none = ControlCompilationCacheKey.Create(globalKey, probe!, null, ThemeAppearance.Light);

        same.ShouldBe(first);
        same.GetHashCode().ShouldBe(first.GetHashCode());
        changed.ShouldNotBe(first);
        none.ShouldNotBe(first);

        // Through the cache: the entry compiled under revision 1 is not served for revision 2.
        using var cache = new ControlCompilationCache(new ThemeCacheOptions());
        var compiled = 0;
        ControlThemeSnapshot Factory()
        {
            compiled++;
            return new ControlThemeSnapshot(probe!.Slot, ThemeAppearance.Light,
                new Dictionary<int, object?>(), new Dictionary<object, object?>(), TokenValueTable.Empty,
                new Dictionary<object, object?>());
        }

        cache.GetOrCompile(globalKey, probe!, null, ThemeAppearance.Light, Factory, controlStages: [revision1]);
        cache.GetOrCompile(globalKey, probe!, null, ThemeAppearance.Light, Factory, controlStages: [revision1]);
        compiled.ShouldBe(1);
        cache.GetOrCompile(globalKey, probe!, null, ThemeAppearance.Light, Factory, controlStages: [revision2]);
        compiled.ShouldBe(2);
    }

    [Fact]
    public void Controls_Without_A_Control_Stage_Compile_Exactly_As_Before()
    {
        // The probe control has a stage under X; the button has none. The button compiles as it would without any
        // control stage registered: same values, no global delta, root shared resources, the same cache key, and
        // not one extra byte allocated.
        var withStages = CreateRegistry([Stage<GlobalPrimaryRed>(s_x)]);
        var withoutStages = CreateRegistry([]);
        var buttonIdentity = ThemeCompilerTests.CreateCompilerButtonDescriptor().Identity;

        var staged = Compile(withStages, [Default(withStages), Follower(withStages, s_x)], null).Snapshot!;
        var plain = Compile(withoutStages, [Default(withoutStages), Follower(withoutStages, s_x)], null).Snapshot!;
        withStages.TryGetControl(buttonIdentity, out var stagedButton).ShouldBeTrue();
        withoutStages.TryGetControl(buttonIdentity, out var plainButton).ShouldBeTrue();

        var stagedCell = staged.Controls.GetCell(stagedButton!.Slot);
        stagedCell.UsesRootSharedResources.ShouldBeTrue();
        var stagedValue = stagedCell.GetValue();
        var plainValue = plain.Controls[plainButton!.Slot];
        stagedValue.EffectiveGlobalTokenDelta.ShouldBeEmpty();
        stagedValue.ControlTokenValues.GetValue(0).ShouldBe(plainValue.ControlTokenValues.GetValue(0));
        // The probe control itself is not on the root path any more.
        withStages.TryGetControl(StageProbeToken.ControlIdentity, out var stagedProbe).ShouldBeTrue();
        staged.Controls.GetCell(stagedProbe!.Slot).UsesRootSharedResources.ShouldBeFalse();

        IReadOnlyList<ThemeAlgorithmDescriptor> chain = [Default(withStages), Follower(withStages, s_x)];
        ThemeCompiler.ResolveControlStages(withStages, stagedButton, null, chain).ShouldBeNull();
        ThemeCompiler.ResolveControlStages(withoutStages, plainButton, null, chain).ShouldBeNull();
        ThemeCompiler.ResolveControlStages(withStages, stagedProbe, null, chain).ShouldNotBeNull();

        var globalKey = GlobalCompilationCacheKey.Create(CreateInput(withStages, chain, null));
        ControlCompilationCacheKey.Create(globalKey, stagedButton, null, ThemeAppearance.Light, null)
                                  .ShouldBe(ControlCompilationCacheKey.Create(
                                      globalKey, stagedButton, null, ThemeAppearance.Light));

        var builder = ThemeCompiler.CreateBuilder(staged.GlobalTokenValues, staged.PresetColorPalettes,
            withStages.GlobalTokens);
        long stagedBytes = long.MaxValue;
        long plainBytes = long.MaxValue;
        for (var sample = 0; sample < 40; sample++)
        {
            var before = GC.GetAllocatedBytesForCurrentThread();
            ThemeCompiler.CompileControl(stagedButton, null, chain, ThemeAppearance.Light, builder,
                staged.GlobalTokenValues, staged.GlobalResources, withStages);
            var middle = GC.GetAllocatedBytesForCurrentThread();
            ThemeCompiler.CompileControl(plainButton, null, chain, ThemeAppearance.Light, builder,
                staged.GlobalTokenValues, staged.GlobalResources, withoutStages);
            var after = GC.GetAllocatedBytesForCurrentThread();
            stagedBytes = Math.Min(stagedBytes, middle - before);
            plainBytes = Math.Min(plainBytes, after - middle);
        }

        plainBytes.ShouldBeGreaterThan(0);
        stagedBytes.ShouldBe(plainBytes);
    }

    [Fact]
    public void Startup_Index_Path_Serves_The_Stage_Written_Global_Value_For_That_Control_Only()
    {
        // Real applications compile controls through the startup index (CompileIndexedControls). There, an
        // unconfigured control normally reads the root's shared resources directly; a control whose scope chain runs
        // a control stage must not, because its EvaluateGlobal may have written its global copy.
        var manual = CreateRegistry([]);
        var index = StartupPackageIndex.Create([], null, manual);
        var registry = new ThemeSchemaRegistry(GeneratedThemeSchema.GetGlobalTokens(), index,
            manual.Algorithms, [Stage<GlobalPrimaryRed>(s_x)]);
        registry.UsesStartupIndex.ShouldBeTrue();
        var result = Compile(registry, [Default(registry), Follower(registry, s_x)], null);
        result.Success.ShouldBeTrue();
        var provider = new ThemeTokenResourceProvider(result.Snapshot!);

        provider.TryGetResource(SharedTokenKind.ColorPrimary, null, out var root).ShouldBeTrue();
        provider.TryGetResource(ControlTokenResourceKey.Global(StageProbeToken.ControlIdentity,
            SharedTokenKind.ColorPrimary), null, out var probe).ShouldBeTrue();
        provider.TryGetResource(ControlTokenResourceKey.Global(ThemeCompilerTests.CreateCompilerButtonDescriptor().Identity,
            SharedTokenKind.ColorPrimary), null, out var button).ShouldBeTrue();

        // The shared resource of a colour token is its projected resource value; compare it through its text.
        root!.ToString().ShouldNotBe(Colors.Red.ToString());
        probe!.ToString().ShouldBe(Colors.Red.ToString());
        probe.ShouldNotBe(root);
        button.ShouldBe(root);
    }

    [Fact]
    public void Contract_StageBoundary_Reports_A_Control_Stage_That_Mutates_EffectiveGlobal()
    {
        var violations = VerifyContract(Stage<MutatesEffectiveGlobal>(s_x));

        violations.ShouldContain(violation => violation.Check == ThemeAlgorithmContract.StageBoundary &&
                                              violation.Algorithm == s_x &&
                                              violation.TokenName == nameof(DesignToken.ColorPrimary));
        violations.ShouldContain(violation => violation.Check == ThemeAlgorithmContract.InputsUnchanged &&
                                              violation.Algorithm == s_x &&
                                              violation.TokenName == nameof(DesignToken.ColorPrimary));
        violations.ShouldNotContain(violation => violation.Check == ThemeAlgorithmContract.Stable);
    }

    [Fact]
    public void Contract_Stable_Reports_A_Self_Referential_Control_Stage_Write()
    {
        var violations = VerifyContract(Stage<SelfReferential>(s_x));

        violations.ShouldContain(violation => violation.Check == ThemeAlgorithmContract.Stable &&
                                              violation.Algorithm == s_x &&
                                              violation.TokenName == nameof(StageProbeToken.BaseSize));
        violations.ShouldNotContain(violation => violation.Check == ThemeAlgorithmContract.StageBoundary);
    }

    // ---------------------------------------------------------------- Helpers

    private static IReadOnlyList<ThemeAlgorithmContractViolation> VerifyContract(ControlTokenAlgorithmDescriptor stage)
    {
        var x = new ThemeAlgorithmDescriptor(s_x, 1, ThemeAppearanceEffect.Preserve, static () => new FollowerMap());
        var input = ThemeAlgorithmContractInput.Capture(builder => builder.AddThemeAlgorithms([x])
                                                                          .AddControlTokenAlgorithms([stage]));
        input.ControlAlgorithms.ShouldBe([stage]);
        input = input with { Controls = [CreateProbeDescriptor()] };
        return ThemeAlgorithmContract.Verify(input, s_x, standalone: false);
    }

    private static ControlTokenAlgorithmDescriptor Stage<TBody>(ThemeAlgorithm algorithm, int revision = 1)
        where TBody : IStageBody =>
        ControlTokenAlgorithmDescriptor.Create<StageProbeToken, StageOf<TBody>>(algorithm, revision);

    private static ThemeSnapshot CompileSuccess(
        IReadOnlyList<ControlTokenAlgorithmDescriptor> stages,
        (string Name, string Value)[]? globalTokens = null,
        (string Name, string Value)[]? ownTokens = null,
        ControlAlgorithmMode? mode = null,
        ThemeAlgorithm[]? chain = null)
    {
        var registry = CreateRegistry(stages);
        ThemeAlgorithmDescriptor[] algorithms =
            [Default(registry), .. (chain ?? [s_x]).Select(identity => Follower(registry, identity))];
        NormalizedControlThemeConfig? config = null;
        if (globalTokens is not null || ownTokens is not null || mode is not null)
        {
            registry.TryGetControl(StageProbeToken.ControlIdentity, out var probe).ShouldBeTrue();
            registry.TryGetAlgorithm(ThemeAlgorithm.Compact, out var compact).ShouldBeTrue();
            var controlMode = mode ?? ControlAlgorithmMode.Disabled;
            config = new NormalizedControlThemeConfig(
                StageProbeToken.ControlIdentity,
                controlMode,
                controlMode == ControlAlgorithmMode.Custom
                    ? [Default(registry), compact!, Follower(registry, s_y)]
                    : Array.Empty<ThemeAlgorithmDescriptor>(),
                (globalTokens ?? []).Select(token => Value(registry.GlobalTokens, token)),
                (ownTokens ?? []).Select(token => Value(probe!.OwnTokens, token)));
        }

        var result = Compile(registry, algorithms, config);
        result.Exception.ShouldBeNull();
        result.Success.ShouldBeTrue();
        return result.Snapshot!;
    }

    private static NormalizedTokenValue Value(IReadOnlyList<TokenDescriptor> descriptors, (string Name, string Value) token)
    {
        var descriptor = descriptors.Single(candidate => candidate.Name == token.Name);
        var parsed = descriptor.Parse(token.Value);
        return new NormalizedTokenValue(descriptor, parsed, descriptor.Format(parsed));
    }

    private static ThemeCompileResult Compile(
        ThemeSchemaRegistry registry,
        IReadOnlyList<ThemeAlgorithmDescriptor> algorithms,
        NormalizedControlThemeConfig? control)
    {
        return new ThemeCompiler().Compile(CreateInput(registry, algorithms, control));
    }

    private static ThemeCompileInput CreateInput(
        ThemeSchemaRegistry registry,
        IReadOnlyList<ThemeAlgorithmDescriptor> algorithms,
        NormalizedControlThemeConfig? control)
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
        var config = new NormalizedThemeConfig(false, true, algorithms, Array.Empty<NormalizedTokenValue>(),
            control is null ? Array.Empty<NormalizedControlThemeConfig>() : [control]);
        return new ThemeCompileInput(definition, new ThemeDefinitionRevision("test", "1", "A1"), config, registry);
    }

    private static ThemeSchemaRegistry CreateRegistry(IReadOnlyList<ControlTokenAlgorithmDescriptor> stages)
    {
        ThemeAlgorithmDescriptor[] followers =
        [
            new(s_x, 1, ThemeAppearanceEffect.Preserve, static () => new FollowerMap()),
            new(s_y, 1, ThemeAppearanceEffect.Preserve, static () => new FollowerMap())
        ];
        return new ThemeSchemaRegistry(
            GeneratedThemeSchema.GetGlobalTokens(),
            [ThemeCompilerTests.CreateCompilerButtonDescriptor(), CreateProbeDescriptor()],
            GeneratedThemeSchema.GetAlgorithms().Concat(followers).ToArray(),
            null,
            stages);
    }

    private static ThemeAlgorithmDescriptor Default(ThemeSchemaRegistry registry) =>
        registry.TryGetAlgorithm(ThemeAlgorithm.Default, out var descriptor) ? descriptor! : throw new InvalidOperationException();

    private static ThemeAlgorithmDescriptor Follower(ThemeSchemaRegistry registry, ThemeAlgorithm identity) =>
        registry.TryGetAlgorithm(identity, out var descriptor) ? descriptor! : throw new InvalidOperationException();

    internal static ControlTokenDescriptor CreateProbeDescriptor()
    {
        return new ControlTokenDescriptor(
            typeof(StageProbeControl),
            StageProbeToken.ControlIdentity,
            [
                new TokenDescriptor(nameof(StageProbeToken.Accent), 0, TokenStage.Control, typeof(Color),
                    StageProbeTokenKind.Accent,
                    static value => ThemeTokenValueParser.Parse<Color>(value),
                    static value => ThemeTokenValueFormatter.Format((Color)value!),
                    static token => ((StageProbeToken)token).Accent,
                    static (token, value) => ((StageProbeToken)token).Accent = (Color)value!,
                    static token => ((StageProbeToken)token).Accent),
                new TokenDescriptor(nameof(StageProbeToken.BaseSize), 1, TokenStage.Control, typeof(double),
                    StageProbeTokenKind.BaseSize,
                    static value => ThemeTokenValueParser.Parse<double>(value),
                    static value => ThemeTokenValueFormatter.Format((double)value!),
                    static token => ((StageProbeToken)token).BaseSize,
                    static (token, value) => ((StageProbeToken)token).BaseSize = (double)value!,
                    static token => ((StageProbeToken)token).BaseSize),
                new TokenDescriptor(nameof(StageProbeToken.DerivedHeight), 2, TokenStage.Control, typeof(double),
                    StageProbeTokenKind.DerivedHeight,
                    static value => ThemeTokenValueParser.Parse<double>(value),
                    static value => ThemeTokenValueFormatter.Format((double)value!),
                    static token => ((StageProbeToken)token).DerivedHeight,
                    static (token, value) => ((StageProbeToken)token).DerivedHeight = (double)value!,
                    static token => ((StageProbeToken)token).DerivedHeight)
            ],
            static () => new StageProbeToken(),
            static (token, appearance) =>
                ((StageProbeToken)token).RunDefaultCalculation(appearance == ThemeAppearance.Dark));
    }

    /// <summary>A Map stage that keeps the previous algorithm's result; used only as a follower.</summary>
    private sealed class FollowerMap : IThemeAlgorithm
    {
        public void Evaluate(DesignToken effectiveSeed, DesignToken? previousMap, DesignToken nextMap)
        {
        }
    }

    private sealed class StageProbeControl : Control
    {
    }

    // ---------------------------------------------------------------- Stage bodies

    public interface IStageBody
    {
        static virtual void Global(in ThemeAlgorithmContext context, DesignToken controlGlobal)
        {
        }

        static virtual void Own(in ThemeAlgorithmContext context, DesignToken effectiveGlobal, StageProbeToken token)
        {
        }
    }

    public sealed class StageOf<TBody> : IControlTokenAlgorithm<StageProbeToken> where TBody : IStageBody
    {
        public void EvaluateGlobal(in ThemeAlgorithmContext context, DesignToken controlGlobal) =>
            TBody.Global(in context, controlGlobal);

        public void EvaluateOwn(in ThemeAlgorithmContext context, DesignToken effectiveGlobal, StageProbeToken token) =>
            TBody.Own(in context, effectiveGlobal, token);
    }

    public sealed class AccentRed : IStageBody
    {
        static void IStageBody.Own(in ThemeAlgorithmContext context, DesignToken effectiveGlobal, StageProbeToken token) =>
            token.Accent = Colors.Red;
    }

    public sealed class BaseSize50 : IStageBody
    {
        static void IStageBody.Own(in ThemeAlgorithmContext context, DesignToken effectiveGlobal, StageProbeToken token) =>
            token.BaseSize = 50;
    }

    public sealed class GlobalPrimaryRed : IStageBody
    {
        static void IStageBody.Global(in ThemeAlgorithmContext context, DesignToken controlGlobal) =>
            controlGlobal.ColorPrimary = Colors.Red;
    }

    public sealed class HeightThenCopy : IStageBody
    {
        static void IStageBody.Global(in ThemeAlgorithmContext context, DesignToken controlGlobal) =>
            controlGlobal.ControlHeight = 77;

        static void IStageBody.Own(in ThemeAlgorithmContext context, DesignToken effectiveGlobal, StageProbeToken token)
        {
            token.BaseSize = effectiveGlobal.ControlHeight + 1;
            token.Accent   = effectiveGlobal.ColorPrimary;
        }
    }

    public sealed class MutatesEffectiveGlobal : IStageBody
    {
        static void IStageBody.Own(in ThemeAlgorithmContext context, DesignToken effectiveGlobal, StageProbeToken token) =>
            effectiveGlobal.ColorPrimary = Colors.Red;
    }

    public sealed class SelfReferential : IStageBody
    {
        static void IStageBody.Own(in ThemeAlgorithmContext context, DesignToken effectiveGlobal, StageProbeToken token) =>
            token.BaseSize = token.BaseSize + 1;
    }

    public sealed class ChainProbeX : IStageBody
    {
        static void IStageBody.Global(in ThemeAlgorithmContext context, DesignToken controlGlobal) =>
            controlGlobal.LineWidth = context.Algorithms.Count;

        static void IStageBody.Own(in ThemeAlgorithmContext context, DesignToken effectiveGlobal, StageProbeToken token)
        {
            token.Accent   = Colors.Red;
            token.BaseSize = 100 * context.Algorithms.Count;
        }
    }

    public sealed class ChainProbeY : IStageBody
    {
        static void IStageBody.Global(in ThemeAlgorithmContext context, DesignToken controlGlobal) =>
            controlGlobal.LineWidth = context.Algorithms.Count;

        static void IStageBody.Own(in ThemeAlgorithmContext context, DesignToken effectiveGlobal, StageProbeToken token)
        {
            token.Accent   = Colors.Green;
            token.BaseSize = 100 * context.Algorithms.Count;
        }
    }

    public sealed class EarlierStage : IStageBody
    {
        static void IStageBody.Global(in ThemeAlgorithmContext context, DesignToken controlGlobal)
        {
            controlGlobal.ControlHeight = 77;
            controlGlobal.LineWidth     = 5;
        }

        static void IStageBody.Own(in ThemeAlgorithmContext context, DesignToken effectiveGlobal, StageProbeToken token)
        {
            token.Accent   = Colors.Red;
            token.BaseSize = 50;
        }
    }

    public sealed class LaterStage : IStageBody
    {
        static void IStageBody.Global(in ThemeAlgorithmContext context, DesignToken controlGlobal) =>
            controlGlobal.ControlHeight = 88;

        static void IStageBody.Own(in ThemeAlgorithmContext context, DesignToken effectiveGlobal, StageProbeToken token) =>
            token.Accent = Colors.Green;
    }
}

internal static class StageProbeSnapshotExtensions
{
    internal static ControlThemeSnapshot ProbeSnapshot(this ThemeSnapshot snapshot)
    {
        snapshot.Registry.TryGetControl(StageProbeToken.ControlIdentity, out var control).ShouldBeTrue();
        return snapshot.Controls[control!.Slot];
    }

    internal static T Own<T>(this ThemeSnapshot snapshot, string name)
    {
        snapshot.Registry.TryGetControl(StageProbeToken.ControlIdentity, out var control).ShouldBeTrue();
        var slot = control!.OwnTokens.Single(token => token.Name == name).Slot;
        return snapshot.ProbeSnapshot().ControlTokenValues.Get<T>(slot);
    }
}

public enum StageProbeTokenKind
{
    Accent,
    BaseSize,
    DerivedHeight
}

/// <summary>
/// A control token written by hand the way the generator writes one: every setter records the write.
/// </summary>
public sealed class StageProbeToken : AbstractControlDesignToken, IControlToken
{
    public static ControlTokenIdentity ControlIdentity { get; } = new("Test", "StageProbe");

    public Color Accent
    {
        get => field;
        set
        {
            field = value;
            RecordWrite(nameof(Accent));
        }
    }

    public double BaseSize
    {
        get => field;
        set
        {
            field = value;
            RecordWrite(nameof(BaseSize));
        }
    }

    public double DerivedHeight
    {
        get => field;
        set
        {
            field = value;
            RecordWrite(nameof(DerivedHeight));
        }
    }

    internal void RunDefaultCalculation(bool isDarkMode) => CalculateTokenValues(isDarkMode);

    protected override void CalculateTokenValues(bool isDarkMode)
    {
        Accent = EffectiveGlobalToken.ColorPrimary;
        if (!IsExplicit(StageProbeTokenKind.BaseSize))
        {
            BaseSize = EffectiveGlobalToken.ControlHeight;
        }
        DerivedHeight = BaseSize + 10;
    }
}
