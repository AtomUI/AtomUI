using Avalonia.Input;
using Avalonia.Interactivity;

namespace AtomUI.Desktop.Controls;

internal static class DialogInputCaptureTracker
{
    private static bool s_initialized;
    private static IPointer? s_latestPrimaryMousePointer;

    public static void Initialize()
    {
        if (s_initialized)
        {
            return;
        }

        s_initialized = true;
        InputElement.PointerPressedEvent.AddClassHandler<InputElement>(
            (_, args) => TrackPointer(args.Pointer),
            RoutingStrategies.Tunnel,
            handledEventsToo: true);
        InputElement.PointerReleasedEvent.AddClassHandler<InputElement>(
            (_, args) => TrackPointer(args.Pointer),
            RoutingStrategies.Tunnel,
            handledEventsToo: true);
    }

    public static void ReleaseCurrentMouseCapture()
    {
        var pointer = s_latestPrimaryMousePointer;
        if (pointer?.Captured is not null)
        {
            pointer.Capture(null);
        }
    }

    private static void TrackPointer(IPointer pointer)
    {
        if (pointer is { Type: PointerType.Mouse, IsPrimary: true })
        {
            s_latestPrimaryMousePointer = pointer;
        }
    }
}
