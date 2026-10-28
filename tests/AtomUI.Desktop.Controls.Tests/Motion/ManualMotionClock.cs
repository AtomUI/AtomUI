using System.Reflection;
using Avalonia;
using Avalonia.Animation;

namespace AtomUI.Desktop.Controls.Tests.Motion;

/// <summary>Controls Avalonia's global clock for non-visual motion drivers; reflection stays in tests.</summary>
internal sealed class ManualMotionClock : IDisposable
{
    internal object Instance { get; }

    private static readonly Type MediaContextType = typeof(Animatable).Assembly.GetType("Avalonia.Media.MediaContext", true)!;
    private static readonly Type ClockType = MediaContextType.GetNestedType("MediaContextClock", BindingFlags.NonPublic)!;
    private static readonly Type GlobalClockType = typeof(Animatable).Assembly.GetType("Avalonia.Animation.IGlobalClock", true)!;
    private static readonly MethodInfo Pulse = ClockType.GetMethod("Pulse")!;
    private static readonly MethodInfo PulseNewSubscriptions = ClockType.GetMethod("PulseNewSubscriptions")!;
    private readonly IDisposable _scope;

    internal ManualMotionClock()
    {
        var mediaContext = MediaContextType.GetProperty("Instance", BindingFlags.Static | BindingFlags.Public)!.GetValue(null)!;
        Instance = Activator.CreateInstance(ClockType, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
            null, [mediaContext], null)!;
        _scope = AvaloniaLocator.EnterScope();
        try
        {
            var registration = typeof(AvaloniaLocator).GetMethod("Bind")!.MakeGenericMethod(GlobalClockType)
                .Invoke(AvaloniaLocator.CurrentMutable, null)!;
            registration.GetType().GetMethod("ToConstant")!.MakeGenericMethod(ClockType).Invoke(registration, [Instance]);
        }
        catch
        {
            _scope.Dispose();
            throw;
        }
    }

    internal void Step(int milliseconds)
    {
        // MediaContext seeds subscriptions created after the preceding layout pass
        // at that pass's timestamp before advancing the next frame.
        PulseNewSubscriptions.Invoke(Instance, null);
        Pulse.Invoke(Instance, [TimeSpan.FromMilliseconds(milliseconds)]);
    }

    public void Dispose()
    {
        _scope.Dispose();
    }
}
