using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Avalonia.Controls.Platform;
using Avalonia.Threading;

namespace AtomUI.Reflection;

internal static class DispatcherReflectionExtensions
{
    private static readonly Version SupportedVersion = new(12, 1, 3, 0);

    #region 反射信息定义

    [DynamicDependency("_impl", typeof(Dispatcher))]
    private static readonly Lazy<FieldInfo> PlatformImplFieldInfo = new(() =>
        FixedMemberReflection.RequireField(
            typeof(Dispatcher).GetField("_impl", BindingFlags.Instance | BindingFlags.NonPublic),
            typeof(Dispatcher),
            "_impl"));

    [DynamicDependency("_timeProvider", typeof(Dispatcher))]
    private static readonly Lazy<FieldInfo> TimeProviderFieldInfo = new(() =>
        FixedMemberReflection.RequireField(
            typeof(Dispatcher).GetField("_timeProvider", BindingFlags.Instance | BindingFlags.NonPublic),
            typeof(Dispatcher),
            "_timeProvider"));

    // The owner is compiler generated; both its name and this exact field need revalidation on upgrade.
    // DynamicDependency uses the documentation type name, with '.' for the nested type.
    [DynamicDependency("st", "Avalonia.Threading.Dispatcher.<>c__DisplayClass10_1", "Avalonia.Base")]
    private static readonly Lazy<FieldInfo> CapturedStopwatchFieldInfo = new(() =>
    {
        var ownerType = typeof(Dispatcher).GetNestedType("<>c__DisplayClass10_1", BindingFlags.NonPublic)
                        ?? throw new TypeLoadException("The Dispatcher Stopwatch closure was not found.");
        return FixedMemberReflection.RequireField(
            ownerType.GetField("st", BindingFlags.Instance | BindingFlags.Public), ownerType, "st");
    });

    [DynamicDependency("_clock", typeof(ManagedDispatcherImpl))]
    private static readonly Lazy<FieldInfo> ClockFieldInfo = new(() =>
        FixedMemberReflection.RequireField(
            typeof(ManagedDispatcherImpl).GetField("_clock", BindingFlags.Instance | BindingFlags.NonPublic),
            typeof(ManagedDispatcherImpl),
            "_clock"));

    #endregion

    // Call only on the UI thread after platform initialization, before entering its run loop.
    internal static bool TryShareManagedClock(this Dispatcher dispatcher)
    {
        dispatcher.VerifyAccess();

        if (typeof(Dispatcher).Assembly.GetName().Version != SupportedVersion)
        {
            return false;
        }

        try
        {
            var backend = PlatformImplFieldInfo.Value.GetValue(dispatcher);
            if (backend?.GetType() != typeof(ManagedDispatcherImpl))
            {
                return false;
            }

            var provider = TimeProviderFieldInfo.Value.GetValue(dispatcher) as Func<long>;
            var capturedStopwatchField = CapturedStopwatchFieldInfo.Value;
            if (provider?.Target is not { } clockOwner || clockOwner.GetType() != capturedStopwatchField.DeclaringType)
            {
                return false;
            }

            var sharedClock = capturedStopwatchField.GetValue(clockOwner) as Stopwatch;
            var backendClockField = ClockFieldInfo.Value;
            if (sharedClock is not { IsRunning: true } || backendClockField.FieldType != typeof(Stopwatch))
            {
                return false;
            }

            if (ReferenceEquals(backendClockField.GetValue(backend), sharedClock))
            {
                return true;
            }

            // Preserve Dispatcher time and existing absolute deadlines. Both backend Now and its run loop
            // read this same Stopwatch; no clock reset, offset, or additional wakeup is needed.
            backendClockField.SetValue(backend, sharedClock);
            return ReferenceEquals(backendClockField.GetValue(backend), sharedClock);
        }
        catch (MemberAccessException)
        {
            return false;
        }
        catch (NotSupportedException)
        {
            return false;
        }
        catch (TypeLoadException)
        {
            return false;
        }
    }
}
