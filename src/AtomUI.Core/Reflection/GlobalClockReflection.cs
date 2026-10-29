using System.Diagnostics.CodeAnalysis;
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

    #endregion

    /// <summary>Resolves the clock that drives Avalonia animations in the current locator scope.</summary>
    internal static IObservable<TimeSpan> GetGlobalClock() =>
        AvaloniaLocator.Current.GetService(GlobalClockType.Value) as IObservable<TimeSpan>
        ?? throw new InvalidOperationException("The Avalonia global animation clock is not registered.");
}
