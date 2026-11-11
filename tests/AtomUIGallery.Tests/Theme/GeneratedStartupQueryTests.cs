using AtomUI.Desktop.Controls;
using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Theme;
using AtomUI.Theme.Resources;
using Avalonia;
using Shouldly;
using Xunit;

namespace AtomUIGallery.Tests.Theme;

/// <summary>
/// Zero-allocation contract for the generated startup registry: the manual-registry contracts in
/// AtomUI.Core.Tests never exercise <c>StartupPackageIndex</c>, so the real application's warmed
/// identity, validation and type routes are proven here against the generated startup path.
/// </summary>
public class GeneratedStartupQueryTests
{
    [Theory]
    [InlineData("identity-resource")]
    [InlineData("identity-validation")]
    [InlineData("type-resource")]
    public void Generated_Startup_Hot_Query_Allocates_Zero(string route)
    {
        AvaloniaTestApp.EnsureInitialized();
        var manager = (ThemeManager)Application.Current!.GetThemeManager()!;
        var registry = manager.RootContext.Snapshot.Registry;
        registry.UsesStartupIndex.ShouldBeTrue("this contract must exercise generated startup, not a manual registry fixture");
        var provider = manager.RootContext.ResourceProvider;
        var identity = ButtonTokens.Identity;
        var identityKey = ControlTokenResourceKey.Global(identity, SharedTokenKind.ColorPrimary);
        var typeKey = ControlTokenResourceKey.Global(typeof(Button), SharedTokenKind.ColorPrimary);
        Func<bool> query = route switch
        {
            "identity-resource" => () => provider.TryGetResource(identityKey, null, out _),
            "identity-validation" => () => registry.TryGetControl(identity, out _),
            _ => () => provider.TryGetResource(typeKey, null, out _)
        };
        for (var index = 0; index < 200; index++)
        {
            query().ShouldBeTrue();
        }
        var start = GC.GetAllocatedBytesForCurrentThread();
        for (var index = 0; index < 10000; index++)
        {
            if (!query())
            {
                throw new InvalidOperationException("warmed registered query missed");
            }
        }
        (GC.GetAllocatedBytesForCurrentThread() - start).ShouldBe(0, route);
    }
}
