using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Avalonia;

namespace AtomUI.Reflection;

internal static class GlobalClockReflection
{
    #region 反射信息定义

    // IGlobalClock is internal; Animation instances resolve it from the locator the same way.
    // The constant assembly-qualified name keeps the lookup analyzable by the trimmer.
    [DynamicDependency(DynamicallyAccessedMemberTypes.Interfaces, "Avalonia.Animation.IGlobalClock", "Avalonia.Base")]
    private static readonly Lazy<Type> GlobalClockType = new(() =>
        Type.GetType("Avalonia.Animation.IGlobalClock, Avalonia.Base")
        ?? throw new TypeLoadException("Avalonia.Animation.IGlobalClock was not found."));

    // MediaContext's clock: every transition, keyframe animation and motion frame driver is one of its observers,
    // and it records the time of the frame it last pulsed.
    [DynamicDependency("_observers", "Avalonia.Media.MediaContext.MediaContextClock", "Avalonia.Base")]
    [DynamicDependency("_currentAnimationTimestamp", "Avalonia.Media.MediaContext.MediaContextClock", "Avalonia.Base")]
    private static readonly Lazy<Type> MediaContextClockType = new(() =>
        Type.GetType("Avalonia.Media.MediaContext+MediaContextClock, Avalonia.Base")
        ?? throw new TypeLoadException("Avalonia.Media.MediaContext+MediaContextClock was not found."));

    private static readonly Lazy<FieldInfo> ObserversFieldInfo = new(() =>
        FixedMemberReflection.RequireField(
            MediaContextClockType.Value.GetField("_observers", BindingFlags.Instance | BindingFlags.NonPublic),
            MediaContextClockType.Value,
            "_observers"));

    private static readonly Lazy<FieldInfo> CurrentTimestampFieldInfo = new(() =>
        FixedMemberReflection.RequireField(
            MediaContextClockType.Value.GetField("_currentAnimationTimestamp", BindingFlags.Instance | BindingFlags.NonPublic),
            MediaContextClockType.Value,
            "_currentAnimationTimestamp"));

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
