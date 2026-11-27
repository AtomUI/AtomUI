using AtomUI.Generated.AtomUICore;
using AtomUI.Theme;
using AtomUI.Theme.Languages;
using AtomUI.Theme.Compilation;
using AtomUI.Theme.Configuration;
using AtomUI.Theme.Definitions;
using AtomUI.Theme.Schema;
using Shouldly;
using Xunit;

namespace AtomUI.Core.Tests.Theme;

public class TypedThemeSnapshotCacheTests
{
    [Fact]
    public void Snapshot_Key_Is_Identified_By_The_Language_Descriptor_Instance()
    {
        var language = TestDesignLanguages.Probe();
        var sameShape = TestDesignLanguages.Probe();
        var registry = CreateRegistry(languages: [language]);
        var first = ThemeSnapshotCacheKey.Create(CreateInput(registry));
        var second = ThemeSnapshotCacheKey.Create(CreateInput(registry));
        var other = ThemeSnapshotCacheKey.Create(CreateInput(CreateRegistry(languages: [sameShape])));

        first.ShouldBe(second);
        first.GetHashCode().ShouldBe(second.GetHashCode());
        first.ShouldNotBe(other, "a different descriptor instance may carry a different language");
    }

    [Fact]
    public async Task Equivalent_Typed_Inputs_Reuse_One_Snapshot_And_Compile_Once()
    {
        var language = new ProbeLanguage();
        var registry = CreateRegistry(languages: [TestDesignLanguages.Probe(language)]);
        var cache = new ThemeSnapshotCache();
        var compiler = new ThemeCompiler();

        var first = await CompileAsync(cache, CreateInput(registry), compiler);
        var second = await CompileAsync(cache, CreateInput(registry), compiler);

        first.Success.ShouldBeTrue();
        second.Snapshot.ShouldBeSameAs(first.Snapshot);
        language.DeriveGlobalCalls.ShouldBe(1);
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
        var invalidInput = CreateInput(CreateRegistry(languages: [TestDesignLanguages.Probe(new ProbeLanguage { FailGlobal = true })]));

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

    /// <summary>
    /// A registry with the generated global tokens, the controls and (by default) Ant Design with the test rules
    /// of <see cref="TestControlTokenRules" />.
    /// </summary>
    internal static ThemeSchemaRegistry CreateRegistry(
        IReadOnlyList<ControlTokenDescriptor>? controls = null,
        IReadOnlyList<DesignLanguageDescriptor>? languages = null,
        IReadOnlyList<ControlThemeAssetDescriptor>? themeAssets = null,
        IReadOnlyList<ControlTokenRuleDescriptor>? rules = null)
    {
        return new ThemeSchemaRegistry(
            GeneratedThemeSchema.GetGlobalTokens(),
            controls ?? Array.Empty<ControlTokenDescriptor>(),
            languages ?? [TestDesignLanguages.AntDesign],
            themeAssets ?? Array.Empty<ControlThemeAssetDescriptor>(),
            rules ?? TestControlTokenRules.All);
    }

    /// <summary>
    /// Builds the input of a scope that shows the language default of the registry's first language with the given
    /// values on top. The axes of the scope follow the language default, except for
    /// <paramref name="appearance" /> and <paramref name="density" /> when they are given.
    /// </summary>
    internal static ThemeCompileInput CreateInput(
        ThemeSchemaRegistry registry,
        IReadOnlyList<NormalizedTokenValue>? globalTokens = null,
        IReadOnlyList<NormalizedControlThemeConfig>? controls = null,
        ThemeSnapshot? reusableParent = null,
        ThemeAppearance? appearance = null,
        DesignDensity? density = null)
    {
        var definition = ThemeTestSnapshotFactory.CreateLanguageDefinition(registry.Languages[0]);
        var axes = ThemeCompiler.CreateDefinitionDefaults(definition, registry).Axes;
        var config = new NormalizedThemeConfig(
            false,
            axes with { Appearance = appearance ?? axes.Appearance, Density = density ?? axes.Density },
            globalTokens ?? Array.Empty<NormalizedTokenValue>(),
            Array.Empty<LanguageTokenValue>(),
            controls ?? Array.Empty<NormalizedControlThemeConfig>(),
            definition);
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
}
