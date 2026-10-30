using System.Runtime.CompilerServices;
using AtomUI.Theme;
using Avalonia.Controls;
using Shouldly;
using Xunit;

namespace AtomUI.Core.Tests.Theme.Lifecycle;

[Collection(ThemeConfigProviderTestCollection.Name)]
public class ThemeContextLeaseLifecycleTests
{
    [Fact]
    public void Dispose_Releases_The_Lease_And_Bridge_While_The_Owner_Context_Remains_Alive()
    {
        ThemeContext? ownerContext = null;
        CollectibleLeaseReferences? references = null;
        HeadlessTestApp.Run(() =>
        {
            ownerContext = CreateContext();
            references = AttachAndDispose(ownerContext);
        });

        Collect();

        references.ShouldNotBeNull();
        references.Lease.IsAlive.ShouldBeFalse();
        references.Bridge.IsAlive.ShouldBeFalse();
        GC.KeepAlive(ownerContext);
    }

    [Fact]
    public void Owner_Replacement_Releases_The_Previous_Lease_And_Bridge()
    {
        HeadlessTestApp.Run(() =>
        {
            var firstContext = CreateContext();
            var replacement = ReplaceOwner(firstContext, CreateContext("#00b96b", 1));

            Collect();

            replacement.PreviousLease.IsAlive.ShouldBeFalse();
            replacement.PreviousBridge.IsAlive.ShouldBeFalse();
            replacement.ActiveLease.Dispose();
            replacement.Host.Close();
            GC.KeepAlive(firstContext);
            GC.KeepAlive(replacement.Host);
        });
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static CollectibleLeaseReferences AttachAndDispose(ThemeContext context)
    {
        var host = new Window();
        var lease = ThemeContextLease.Attach(host, context);
        var bridge = host.Resources.MergedDictionaries
                         .OfType<ThemeContextResourceBridge>()
                         .ShouldHaveSingleItem();

        lease.Dispose();
        host.Close();

        return new CollectibleLeaseReferences(
            new WeakReference(lease),
            new WeakReference(bridge));
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static ActiveReplacement ReplaceOwner(
        ThemeContext first,
        ThemeContext second)
    {
        var host = new Window();
        var previousLease = ThemeContextLease.Attach(host, first);
        var previousBridge = host.Resources.MergedDictionaries
                                 .OfType<ThemeContextResourceBridge>()
                                 .ShouldHaveSingleItem();
        var activeLease = ThemeContextLease.Attach(host, second);
        return new ActiveReplacement(
            host,
            activeLease,
            new WeakReference(previousLease),
            new WeakReference(previousBridge));
    }

    private static ThemeContext CreateContext(
        string? globalPrimary = null,
        long registrationId = 0)
    {
        return new ThemeContext(
            new ThemeManager(static () => true),
            ThemeTokenResourceProviderTests.Compile(globalPrimary),
            registrationId);
    }

    private static void Collect()
    {
        for (var attempt = 0; attempt < 3; attempt++)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }
    }

    private sealed record CollectibleLeaseReferences(
        WeakReference Lease,
        WeakReference Bridge);

    private sealed record ActiveReplacement(
        Window Host,
        ThemeContextLease ActiveLease,
        WeakReference PreviousLease,
        WeakReference PreviousBridge);
}
