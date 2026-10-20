using Avalonia.Animation;
using Avalonia.Data;
using Avalonia.Threading;

namespace AtomUI.Animations;

/// <summary>
/// Temporarily hides the effective transition collection without replacing its source.
/// </summary>
internal sealed class TransitionSuppressionScope : IDisposable
{
    private IDisposable? _value;

    internal TransitionSuppressionScope(Animatable target)
    {
        _value = target.SetValue(Animatable.TransitionsProperty, null, BindingPriority.Animation);
    }

    public void Dispose()
    {
        Dispatcher.UIThread.VerifyAccess();
        var value = _value;
        _value = null;
        value?.Dispose();
    }
}
