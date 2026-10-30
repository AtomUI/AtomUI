using System.Reflection;
using System.Runtime.CompilerServices;
using Avalonia.Controls;
using Avalonia.Input;
using AvaloniaPopup = Avalonia.Controls.Primitives.Popup;

namespace AtomUI.Desktop.Controls;

// Avalonia snapshots the dismiss registration in Open(). This compatibility boundary
// takes ownership of that exact lease, not the shared layer, so configuration changes
// do not recreate the Popup host or hide another Popup's dismiss surface.
internal sealed class PopupLightDismissRegistration : IDisposable
{
    private static readonly FieldInfo OpenStateField = typeof(AvaloniaPopup)
        .GetField("_openState", BindingFlags.NonPublic | BindingFlags.Instance)!;
    private static readonly FieldInfo CleanupField = Type.GetType(
        "Avalonia.Controls.Primitives.Popup+PopupOpenState, Avalonia.Controls", true)!
        .GetField("_cleanup", BindingFlags.NonPublic | BindingFlags.Instance)!;
    private static readonly FieldInfo CleanupStateField = Type.GetType(
        "Avalonia.Reactive.Disposable+AnonymousDisposable`1[[System.ValueTuple`2[[Avalonia.Controls.Primitives.IPopupHost, Avalonia.Controls],[Avalonia.Reactive.CompositeDisposable, Avalonia.Base]], System.Private.CoreLib]], Avalonia.Base", true)!
        .GetField("_state", BindingFlags.NonPublic | BindingFlags.Instance)!;
    private static readonly Type RegistrationType = Type.GetType(
        "Avalonia.Controls.Primitives.LightDismissOverlayLayer+Registration, Avalonia.Controls", true)!;
    private static readonly MethodInfo GetLayerMethod = Type.GetType(
        "Avalonia.Controls.Primitives.LightDismissOverlayLayer, Avalonia.Controls", true)!
        .GetMethod("GetLightDismissOverlayLayer", BindingFlags.Public | BindingFlags.Static)!;
    private static readonly MethodInfo RegisterMethod = Type.GetType(
        "Avalonia.Controls.Primitives.LightDismissOverlayLayer, Avalonia.Controls", true)!
        .GetMethod("Register", BindingFlags.Public | BindingFlags.Instance)!;
    private static readonly MethodInfo PointerPressedMethod = typeof(AvaloniaPopup)
        .GetMethod("PointerPressedDismissOverlay", BindingFlags.NonPublic | BindingFlags.Instance)!;

    private readonly Control _layer;
    private readonly EventHandler<PointerPressedEventArgs> _pointerPressed;
    private IDisposable? _registration;
    private IInputElement? _passThrough;

    private PopupLightDismissRegistration(AvaloniaPopup popup, Control layer, IDisposable? registration)
    {
        _layer = layer;
        _registration = registration;
        _passThrough = popup.OverlayInputPassThroughElement;
        _pointerPressed = PointerPressedMethod.CreateDelegate<EventHandler<PointerPressedEventArgs>>(popup);
        // There may already be a native handler if Open() enabled light-dismiss.
        // Keep one identical handler for this session, including when first opened pinned.
        layer.PointerPressed -= _pointerPressed;
        layer.PointerPressed += _pointerPressed;
    }

    public static PopupLightDismissRegistration? Acquire(AvaloniaPopup popup, Control target)
    {
        if (GetLayerMethod.Invoke(null, [target]) is not Control layer)
        {
            return null;
        }

        var openState = OpenStateField.GetValue(popup);
        if (openState is null)
        {
            return null;
        }

        var cleanup = CleanupField.GetValue(openState)!;
        var state = (ITuple)CleanupStateField.GetValue(cleanup)!;
        var subscriptions = (IEnumerable<IDisposable>)state[1]!;
        var nativeRegistration = subscriptions.SingleOrDefault(RegistrationType.IsInstanceOfType);
        return new PopupLightDismissRegistration(popup, layer, nativeRegistration);
    }

    public void Update(AvaloniaPopup popup)
    {
        if (!popup.IsLightDismissEnabled || !ReferenceEquals(_passThrough, popup.OverlayInputPassThroughElement))
        {
            _registration?.Dispose();
            _registration = null;
        }

        _passThrough = popup.OverlayInputPassThroughElement;
        if (popup.IsLightDismissEnabled && _registration is null)
        {
            _registration = (IDisposable)RegisterMethod.Invoke(_layer, [_passThrough])!;
        }
    }

    public void Dispose()
    {
        _registration?.Dispose();
        _registration = null;
        _layer.PointerPressed -= _pointerPressed;
    }
}
