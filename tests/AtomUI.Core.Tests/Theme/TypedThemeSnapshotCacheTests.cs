using AtomUI.Generated.AtomUICore;
using AtomUI.Theme;
using AtomUI.Theme.Algorithms;
using AtomUI.Theme.Compilation;
using AtomUI.Theme.Configuration;
using AtomUI.Theme.Definitions;
using AtomUI.Theme.Schema;
using AtomUI.Theme.DesignTokens;
using Shouldly;
using Xunit;

namespace AtomUI.Core.Tests.Theme;

public class TypedThemeSnapshotCacheTests
{
    [Fact]
    public async Task Equivalent_Typed_Inputs_Reuse_One_Snapshot_And_Compile_Once()
    {
        var evaluations = 0;
        var algorithm = new ThemeAlgorithmDescriptor(
            ThemeAlgorithm.Default,
            1,
            ThemeAppearanceEffect.Light,
            () => new CountingAlgorithm(() => Interlocked.Increment(ref evaluations)));
        var registry = CreateRegistry(Array.Empty<ControlTokenDescriptor>(), [algorithm]);
        var cache = new ThemeSnapshotCache();
        var compiler = new ThemeCompiler();

        var first = await CompileAsync(cache, CreateInput(registry, [algorithm]), compiler);
        var second = await CompileAsync(cache, CreateInput(registry, [algorithm]), compiler);

        first.Success.ShouldBeTrue();
        second.Snapshot.ShouldBeSameAs(first.Snapshot);
        evaluations.ShouldBe(1);
        cache.Count.ShouldBe(1);
    }

    [Fact]
    public void Cache_Accepts_A_Precomputed_Snapshot_Key()
    {
        var registry = CreateRegistry();
        var input = CreateInput(registry);
        var key = ThemeSnapshotCacheKey.Create(input);
        var cache = new ThemeSnapshotCache();

        var first = cache.GetOrCompile(key, input, new ThemeCompiler());
        var second = cache.GetOrCompile(key, input, new ThemeCompiler());

        first.Success.ShouldBeTrue();
        second.Snapshot.ShouldBeSameAs(first.Snapshot);
        cache.Count.ShouldBe(1);
    }

    [Fact]
    public async Task Reusable_Parent_Publication_Identity_Does_Not_Change_The_Snapshot_Key()
    {
        var registry = CreateRegistry();
        var compiler = new ThemeCompiler();
        var firstParent = compiler.Compile(CreateInput(registry)).Snapshot!;
        var secondParent = compiler.Compile(CreateInput(registry)).Snapshot!;
        var cache = new ThemeSnapshotCache();

        var first = await CompileAsync(cache,
            CreateInput(registry, reusableParent: firstParent),
            compiler);
        var second = await CompileAsync(cache,
            CreateInput(registry, reusableParent: secondParent),
            compiler);

        secondParent.ShouldNotBeSameAs(firstParent);
        second.Snapshot.ShouldBeSameAs(first.Snapshot);
    }

    [Fact]
    public async Task Failed_And_Oversized_Snapshots_Are_Not_Retained()
    {
        var registry = CreateRegistry();
        var compiler = new ThemeCompiler();
        var cache = new ThemeSnapshotCache(new ThemeCacheOptions(
            SnapshotEntryLimit: 4,
            SnapshotRetainedBytesLimit: 1));
        var invalidInput = CreateInput(registry, Array.Empty<ThemeAlgorithmDescriptor>());

        var firstFailure = await CompileAsync(cache, invalidInput, compiler);
        var secondFailure = await CompileAsync(cache, invalidInput, compiler);

        firstFailure.Success.ShouldBeFalse();
        secondFailure.ShouldNotBeSameAs(firstFailure);
        cache.Count.ShouldBe(0);

        var first = await CompileAsync(cache, CreateInput(registry), compiler);
        var second = await CompileAsync(cache, CreateInput(registry), compiler);

        first.Success.ShouldBeTrue();
        first.Snapshot!.EstimatedRetainedBytes.ShouldBeGreaterThan(1);
        second.Snapshot.ShouldNotBeSameAs(first.Snapshot);
        cache.Count.ShouldBe(0);
        cache.RetainedBytes.ShouldBe(0);
    }

    internal static ThemeSchemaRegistry CreateRegistry(
        IReadOnlyList<ControlTokenDescriptor>? controls = null,
        IReadOnlyList<ThemeAlgorithmDescriptor>? algorithms = null,
        IReadOnlyList<ControlThemeAssetDescriptor>? themeAssets = null)
    {
        return new ThemeSchemaRegistry(
            GeneratedThemeSchema.GetGlobalTokens(),
            controls ?? Array.Empty<ControlTokenDescriptor>(),
            algorithms ?? GeneratedThemeSchema.GetAlgorithms(),
            themeAssets ?? Array.Empty<ControlThemeAssetDescriptor>());
    }

    internal static ThemeCompileInput CreateInput(
        ThemeSchemaRegistry registry,
        IReadOnlyList<ThemeAlgorithmDescriptor>? algorithms = null,
        IReadOnlyList<NormalizedTokenValue>? globalTokens = null,
        IReadOnlyList<NormalizedControlThemeConfig>? controls = null,
        ThemeSnapshot? reusableParent = null)
    {
        var effectiveAlgorithms = algorithms ??
            [registry.Algorithms.Single(static algorithm => algorithm.Algorithm == ThemeAlgorithm.Default)];
        var location = new ThemeSourceLocation("test", 1, 1, "/Theme");
        var definition = new PreparedThemeDefinition(
            "TestTheme",
            "Test Theme",
            ThemeAppearance.Light,
            ThemeAppearance.Light,
            true,
            effectiveAlgorithms,
            Array.Empty<BoundTokenValue>(),
            Array.Empty<ControlThemeDefinition>(),
            location);
        var config = new NormalizedThemeConfig(
            false,
            true,
            effectiveAlgorithms,
            globalTokens ?? Array.Empty<NormalizedTokenValue>(),
            controls ?? Array.Empty<NormalizedControlThemeConfig>());
        return new ThemeCompileInput(
            definition,
            new ThemeDefinitionRevision("test", "1", "A1"),
            config,
            registry,
            reusableParent);
    }

    internal static ValueTask<ThemeCompileResult> CompileAsync(
        ThemeSnapshotCache cache,
        ThemeCompileInput input,
        ThemeCompiler compiler)
    {
        return cache.GetOrCompileAsync(
            input,
            compiler,
            TestContext.Current.CancellationToken);
    }

    private sealed class CountingAlgorithm(Action onEvaluate) : IThemeAlgorithm
    {
        public void Evaluate(DesignToken effectiveSeed, DesignToken? previousMap, DesignToken nextMap)
        {
            onEvaluate();
        }
    }
}
