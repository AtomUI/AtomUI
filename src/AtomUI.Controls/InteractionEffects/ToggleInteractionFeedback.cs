using System.Runtime.CompilerServices;
using AtomUI.Animations;
using AtomUI.Controls.Primitives;
using AtomUI.Theme;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;

namespace AtomUI.Controls;

/// <summary>
/// Reports native toggle gestures and committed clicks without changing their routing.
/// </summary>
internal static class ToggleInteractionFeedback
{
    private static readonly ConditionalWeakTable<Button, InteractionEffectHost> s_pressed = new();

    internal static void Configure<T>(
        Func<T, Control> surface,
        Func<T, bool> playsDefaultOnClick,
        bool quick = false,
        Func<T, bool>? canStart = null)
        where T : Button
    {
        var defaultWave = quick ? AntDesignWaveEffect.Quick : AntDesignWaveEffect.Default;
        InteractionEffect SelectFallback(T target, Control control, bool checkActivation) =>
            !checkActivation || playsDefaultOnClick(target) || HasExplicitOrLanguageEffect(control)
                ? defaultWave
                : InteractionEffect.None;

        InputElement.PointerPressedEvent.AddClassHandler<T>((target, args) =>
        {
            if (args.Handled || !args.GetCurrentPoint(target).Properties.IsLeftButtonPressed || canStart?.Invoke(target) == false)
            {
                return;
            }
            var control = surface(target);
            Start(target, control, SelectFallback(target, control, checkActivation: false),
                InteractionEffectInputSource.Pointer, args.GetPosition(control));
        }, RoutingStrategies.Tunnel);

        InputElement.KeyDownEvent.AddClassHandler<T>((target, args) =>
        {
            if (!args.Handled && target.IsFocused && args.Key == Key.Space && !target.IsPressed && canStart?.Invoke(target) != false)
            {
                var control = surface(target);
                Start(target, control, SelectFallback(target, control, checkActivation: false),
                    InteractionEffectInputSource.Keyboard, null);
            }
        }, RoutingStrategies.Tunnel);

        Button.ClickEvent.AddClassHandler<T>((target, _) =>
        {
            if (!IsEnabled(target))
            {
                return;
            }
            var control = surface(target);
            var allowActivation = playsDefaultOnClick(target) || HasExplicitOrLanguageEffect(control);
            if (s_pressed.TryGetValue(target, out var press))
            {
                if (allowActivation)
                {
                    press.Accept();
                }
            }
            else if (allowActivation)
            {
                InteractionEffectHost.Activate(control, defaultWave, InteractionEffectInputSource.Automation,
                    null, GetRadius(control), GetBrush(control));
            }
        }, handledEventsToo: true);

        InputElement.PointerReleasedEvent.AddClassHandler<T>(static (target, args) =>
        {
            if (args.InitialPressMouseButton == MouseButton.Left)
            {
                End(target);
            }
        }, handledEventsToo: true);

        InputElement.KeyUpEvent.AddClassHandler<T>(static (target, args) =>
        {
            if (args.Key == Key.Space)
            {
                End(target);
            }
        }, handledEventsToo: true);

        InputElement.PointerCaptureLostEvent.AddClassHandler<T>(static (target, _) => Cancel(target), handledEventsToo: true);
        InputElement.LostFocusEvent.AddClassHandler<T>(static (target, _) => Cancel(target), handledEventsToo: true);
        InteractionEffectAwareControlProperty.IsInteractionEffectEnabledProperty.Changed.AddClassHandler<T>((target, args) =>
        {
            if (!args.GetNewValue<bool>())
            {
                Cancel(target);
                InteractionEffectHost.CancelFor(surface(target));
            }
        });
    }

    internal static void Cancel(Button target)
    {
        if (s_pressed.TryGetValue(target, out var press) && s_pressed.Remove(target))
        {
            press.Cancel();
        }
    }

    private static void End(Button target)
    {
        if (s_pressed.TryGetValue(target, out var press) && s_pressed.Remove(target))
        {
            press.Complete(activated: false);
        }
    }

    private static void Start(
        Button owner,
        Control surface,
        InteractionEffect fallback,
        InteractionEffectInputSource input,
        Point? position)
    {
        Cancel(owner);
        if (IsEnabled(owner) && InteractionEffectHost.Begin(surface, fallback, input, position,
                GetRadius(surface), GetBrush(surface)) is { } press)
        {
            s_pressed.AddOrUpdate(owner, press);
        }
    }

    private static bool HasExplicitOrLanguageEffect(Control surface) =>
        InteractionEffect.GetEffect(surface) is not null ||
        InteractionEffectHost.ResolveLanguageEffect(surface, ThemeScope.ResolveContext(surface)?.Snapshot) is not null;

    private static bool IsEnabled(Button target) => target.IsEffectivelyEnabled &&
        InteractionEffectAwareControlProperty.IsEnabled(target) &&
        (!target.IsSet(MotionAwareControlProperty.IsMotionEnabledProperty) ||
         target.GetValue(MotionAwareControlProperty.IsMotionEnabledProperty)) && Motion.IsAnimationEnabled(target);

    private static CornerRadius GetRadius(Control control) => control is IFocusOutlineShapeProvider shape
        ? shape.FocusOutlineCornerRadius
        : control is TemplatedControl templated ? templated.CornerRadius : default;

    private static IBrush? GetBrush(Control control) => control is TemplatedControl templated
        ? AntDesignWaveEffect.ResolveBrush(templated.BorderBrush, templated.Background) : null;
}
