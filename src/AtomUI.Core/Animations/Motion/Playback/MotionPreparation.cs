using AtomUI.Theme;
using AtomUI.Theme.Resources;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Styling;

namespace AtomUI.Animations;

internal sealed class MotionPreparation
{
    private readonly HashSet<AbstractMotion> _active = new(ReferenceEqualityComparer.Instance);
    private Control? _target;
    private Dictionary<AvaloniaProperty, object?>? _values;

    internal MotionPreparation(Control target)
    {
        _target = target;
    }

    internal object? ReadToken(SharedTokenKind? kind)
    {
        return _target is { } target && kind is { } declared ? ReadToken(target, declared) : null;
    }

    internal static object? ReadToken(Control target, SharedTokenKind declared)
    {
        var owner = target.GetValue(ThemeScope.TokenOwnerProperty);
        var variant = target.ActualThemeVariant;
        if (owner is { } identity && target.TryFindResource(OwnerTokenBinding.GetResourceKey(identity, declared),
                                                           variant, out var value))
        {
            return value;
        }
        return target.TryFindResource(declared, variant, out value) ? value : null;
    }

    internal T ReadProperty<T>(StyledProperty<T> property)
    {
        if (_target is null)
        {
            throw new InvalidOperationException("The motion preparation is no longer active.");
        }
        if (!property.IsAttached && !AvaloniaPropertyRegistry.Instance.IsRegistered(_target, property))
        {
            throw new InvalidOperationException("The motion property does not apply to this target.");
        }
        _values ??= new Dictionary<AvaloniaProperty, object?>();
        if (_values.TryGetValue(property, out var cached))
        {
            return (T)cached!;
        }
        var value = _target.GetValue(property);
        _values.Add(property, value);
        return value;
    }

    internal void Enter(AbstractMotion motion)
    {
        if (_active.Count >= 64 || !_active.Add(motion))
        {
            throw new InvalidOperationException("A motion definition contains a cycle or exceeds the supported nesting depth.");
        }
    }

    internal void Leave(AbstractMotion motion)
    {
        _active.Remove(motion);
        if (_active.Count == 0)
        {
            _target = null;
        }
    }
}
