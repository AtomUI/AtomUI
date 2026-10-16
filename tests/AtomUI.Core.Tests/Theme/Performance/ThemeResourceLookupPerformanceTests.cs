using AtomUI.Registration;
using AtomUI.Theme.Compilation;
using AtomUI.Theme.Configuration;
using AtomUI.Theme.Resources;
using AtomUI.Theme.DesignTokens;
using Shouldly;
using Xunit;

namespace AtomUI.Core.Tests.Theme.Performance;

public class ThemeResourceLookupPerformanceTests
{
    private static int s_lookupHits;

    [Fact]
    public void Global_Control_Type_Identity_And_Miss_Lookups_Allocate_Zero_Bytes_After_Warmup()
    {
        var button = ThemeCompilerTests.CreateCompilerButtonDescriptor();
        var config = new ThemeConfigBuilder()
                     .WithControl(
                         button.Identity,
                         new ControlThemeConfigBuilder()
                             .WithAlgorithm(ControlAlgorithmMode.Disabled)
                             .WithToken(nameof(DesignToken.ColorPrimary), "#ff0000")
                             .WithToken(nameof(CompilerButtonToken.Height), "48")
                             .Build())
                     .Build();
        var snapshot = ThemeTestSnapshotFactory.Compile(config, button);
        var provider = new ThemeTokenResourceProvider(snapshot);
        object globalKey = SharedTokenKind.ColorPrimary;
        var controlSharedKey = ControlTokenResourceKey.Global(
            button.Identity,
            SharedTokenKind.ColorPrimary);
        var controlOwnKey = button.OwnTokens.ShouldHaveSingleItem().ResourceKey;
        var missingKey = new object();

        for (var index = 0; index < 100; index++)
        {
            provider.TryGetResource(globalKey, null, out _).ShouldBeTrue();
            provider.TryGetResource(controlSharedKey, null, out _).ShouldBeTrue();
            provider.TryGetResource(controlOwnKey, null, out _).ShouldBeTrue();
            provider.TryGetResource(missingKey, null, out _).ShouldBeFalse();
        }

        MeasureAllocations(() => provider.TryGetResource(globalKey, null, out _))
            .ShouldBe(0, "global hit must not allocate");
        MeasureAllocations(() => provider.TryGetResource(controlSharedKey, null, out _))
            .ShouldBe(0, "Control shared hit must not allocate");
        MeasureAllocations(() => provider.TryGetResource(controlOwnKey, null, out _))
            .ShouldBe(0, "Control own hit must not allocate");
        MeasureAllocations(() => provider.TryGetResource(missingKey, null, out _))
            .ShouldBe(0, "miss must not allocate");

        var ownerType = button.ControlType;
        var typeKey = ownerType.Assembly.GetName().Name + "|" + ownerType.FullName;
        var resourceKey = "t:" + typeKey;
        StartupPackageIndex.TypeKey(ownerType).ShouldBe(typeKey);
        StartupPackageIndex.ResourceKey(ownerType).ShouldBe(resourceKey);
        MeasureAllocations(() => StartupPackageIndex.TypeKey(ownerType) == typeKey)
            .ShouldBe(0, "warmed Control type identity lookup must not allocate");
        MeasureAllocations(() => StartupPackageIndex.ResourceKey(ownerType) == resourceKey)
            .ShouldBe(0, "warmed Control theme key lookup must not allocate");
    }

    [Fact]
    public void Hot_Compile_Reuses_The_Cold_Snapshot_And_Does_Not_Grow_Either_Cache()
    {
        var button = ThemeCompilerTests.CreateCompilerButtonDescriptor();
        var registry = TypedThemeSnapshotCacheTests.CreateRegistry([button]);
        var input = TypedThemeSnapshotCacheTests.CreateInput(registry);
        var cache = new ThemeSnapshotCache();
        var compiler = new ThemeCompiler();

        var cold = cache.GetOrCompile(input, compiler);
        var snapshotCount = cache.Count;
        var controlCount = cache.ControlCount;
        var retainedBytes = cache.RetainedBytes;
        var controlRetainedBytes = cache.ControlRetainedBytes;
        var hot = cache.GetOrCompile(input, compiler);

        cold.Success.ShouldBeTrue();
        hot.Snapshot.ShouldBeSameAs(cold.Snapshot);
        cache.Count.ShouldBe(snapshotCount);
        cache.ControlCount.ShouldBe(controlCount);
        cache.RetainedBytes.ShouldBe(retainedBytes);
        cache.ControlRetainedBytes.ShouldBe(controlRetainedBytes);
    }

    private static long MeasureAllocations(Func<bool> lookup)
    {
        _ = lookup();
        var before = GC.GetAllocatedBytesForCurrentThread();
        var hits = 0;
        for (var index = 0; index < 10_000; index++)
        {
            hits += lookup() ? 1 : 0;
        }
        var allocated = GC.GetAllocatedBytesForCurrentThread() - before;
        Volatile.Write(ref s_lookupHits, hits);
        return allocated;
    }
}
