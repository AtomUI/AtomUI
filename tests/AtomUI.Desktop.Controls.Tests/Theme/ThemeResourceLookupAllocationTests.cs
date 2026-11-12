using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Theme;
using AtomUI.Theme.Resources;
using Avalonia;
using Shouldly;
using Xunit;

namespace AtomUI.Desktop.Controls.Tests.Theme;

// The test application enables the official packages, so lookups run through the real startup
// package index rather than the frozen schema maps that unit snapshots use.
public class ThemeResourceLookupAllocationTests
{
    private static int s_lookupHits;

    [Fact]
    public void Generated_Control_Token_Lookup_Does_Not_Allocate_After_Warmup()
    {
        var manager = Application.Current!.GetThemeManager().ShouldBeOfType<ThemeManager>();
        var provider = manager.RootContext.ResourceProvider;
        var key = ControlTokenResourceKey.Global(ButtonTokens.Identity, SharedTokenKind.ColorPrimary);
        provider.TryGetResource(key, null, out _).ShouldBeTrue();

        MeasureAllocations(() => provider.TryGetResource(key, null, out _))
            .ShouldBe(0, "a generated Control Token key must resolve its owner without building a string");
    }

    [Fact]
    public void String_Key_Miss_In_Startup_Resources_Does_Not_Allocate_After_Warmup()
    {
        var manager = Application.Current!.GetThemeManager().ShouldBeOfType<ThemeManager>();
        var provider = manager.Resources.MergedDictionaries.OfType<StartupResourceProvider>().ShouldHaveSingleItem();
        const string key = "AtomUI.Tests.UnregisteredStringResource";
        provider.TryGetResource(key, null, out _).ShouldBeFalse();

        MeasureAllocations(() => provider.TryGetResource(key, null, out _))
            .ShouldBe(0, "a string resource key must be matched without building a prefixed key");
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
