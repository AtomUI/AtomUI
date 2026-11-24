using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Avalonia;

namespace AtomUI.Reflection;

internal static class GlobalClockReflection
{
    #region 反射信息定义

    private const string MediaContextClockName = "Avalonia.Media.MediaContext+MediaContextClock";

    // IGlobalClock is internal; Animation instances resolve it from the locator the same way.
    // The constant assembly-qualified name keeps the lookup analyzable by the trimmer.
    [DynamicDependency(DynamicallyAccessedMemberTypes.Interfaces, "Avalonia.Animation.IGlobalClock", "Avalonia.Base")]
    private static readonly Lazy<Type> GlobalClockType = new(() =>
        Type.GetType("Avalonia.Animation.IGlobalClock, Avalonia.Base")
        ?? throw new TypeLoadException("Avalonia.Animation.IGlobalClock was not found."));

    // Resolve the known type and look up the named field in the same factory so the trimmer can track the type.
    // Cache each FieldInfo separately to keep lookup deferred until the field is used.
    [DynamicDependency("_observers", "Avalonia.Media.MediaContext.MediaContextClock", "Avalonia.Base")]
    private static readonly Lazy<FieldInfo> ObserversFieldInfo = new(static () =>
    {
        var type = Type.GetType(MediaContextClockName + ", Avalonia.Base")
                   ?? throw new TypeLoadException($"{MediaContextClockName} was not found.");
        return FixedMemberReflection.RequireField(
            type.GetField("_observers", BindingFlags.Instance | BindingFlags.NonPublic), type, "_observers");
    });

    [DynamicDependency("_currentAnimationTimestamp", "Avalonia.Media.MediaContext.MediaContextClock", "Avalonia.Base")]
    private static readonly Lazy<FieldInfo> CurrentTimestampFieldInfo = new(static () =>
    {
        var type = Type.GetType(MediaContextClockName + ", Avalonia.Base")
                   ?? throw new TypeLoadException($"{MediaContextClockName} was not found.");
        return FixedMemberReflection.RequireField(
            type.GetField("_currentAnimationTimestamp", BindingFlags.Instance | BindingFlags.NonPublic),
            type, "_currentAnimationTimestamp");
    });

    #endregion

    /// <summary>Resolves the clock that drives Avalonia animations in the current locator scope.</summary>
    internal static IObservable<TimeSpan> GetGlobalClock() =>
        AvaloniaLocator.Current.GetService(GlobalClockType.Value) as IObservable<TimeSpan>
        ?? throw new InvalidOperationException("The Avalonia global animation clock is not registered.");

    /// <summary>The global clock, or null when no platform registered one (for example a bare dispatcher).</summary>
    internal static IObservable<TimeSpan>? TryGetGlobalClock() =>
        AvaloniaLocator.Current.GetService(GlobalClockType.Value) as IObservable<TimeSpan>;

    /// <summary>The live subscriptions of the render-loop clock; read on the UI thread only, never kept.</summary>
    internal static List<IObserver<TimeSpan>> GetObservers(IObservable<TimeSpan> clock) =>
        (List<IObserver<TimeSpan>>)ObserversFieldInfo.Value.GetValue(clock)!;

    /// <summary>The time of the frame the render-loop clock last pulsed.</summary>
    internal static TimeSpan GetCurrentTimestamp(IObservable<TimeSpan> clock) =>
        (TimeSpan)CurrentTimestampFieldInfo.Value.GetValue(clock)!;
}
