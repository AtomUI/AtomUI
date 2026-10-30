using Avalonia.Controls;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace AtomUI.Animations;

/// <summary>
/// Owns the initialization boundary of a state-animation target. The control commits
/// its complete baseline through its existing lifecycle/layout entry points.
/// </summary>
internal sealed class TransitionInitializationSession : IDisposable
{
    internal bool IsReady => _phase == Phase.Ready;

    private readonly Control _target;
    private TransitionSuppressionScope? _suppression;
    private TransitionInitializationSession? _parent;
    private TransitionInitializationSession? _firstTarget;
    private List<TransitionInitializationSession>? _additionalTargets;
    private Phase _phase;

    internal TransitionInitializationSession(Control target)
    {
        _target = target;
        _suppression = new TransitionSuppressionScope(target);
    }

    internal void Reset()
    {
        if (_phase != Phase.Ready)
        {
            return;
        }

        // Setting the effective collection raises synchronous property notifications.
        // Completion must not run until the newly acquired handle is owned here.
        _phase = Phase.Resetting;
        var suppression = new TransitionSuppressionScope(_target);
        if (_phase == Phase.Released)
        {
            suppression.Dispose();
            return;
        }
        _suppression = suppression;
        _phase = Phase.Initializing;
        VisitTargets(static target => target.Reset());
    }

    // Synchronous owner callback, after all current baseline writes. Loaded alone is
    // insufficient: the owner also verifies its geometry and template identity.
    internal void CompleteInitialization()
    {
        if (_phase != Phase.Initializing || _parent is { IsReady: false } ||
            !_target.IsLoaded || !_target.IsAttachedToVisualTree())
        {
            return;
        }

        _phase = Phase.Ready;
        var suppression = _suppression;
        _suppression = null;
        suppression?.Dispose();
        VisitTargets(static target => target.CompleteInitialization());
    }

    internal void RegisterTarget(TransitionInitializationSession target)
    {
        if (_phase == Phase.Released || target._phase == Phase.Released || ReferenceEquals(target._parent, this))
        {
            return;
        }

        target._parent?.UnregisterTarget(target);
        target.Reset();
        if (_phase == Phase.Released || target._phase == Phase.Released || target._parent is not null)
        {
            return;
        }
        target._parent = this;
        if (_firstTarget is null)
        {
            _firstTarget = target;
        }
        else
        {
            (_additionalTargets ??= []).Add(target);
        }
        if (IsReady)
        {
            target.CompleteInitialization();
        }
    }

    internal void UnregisterTarget(TransitionInitializationSession target)
    {
        if (ReferenceEquals(target._parent, this))
        {
            if (ReferenceEquals(_firstTarget, target))
            {
                _firstTarget = _additionalTargets is { Count: > 0 } ? _additionalTargets[0] : null;
                if (_firstTarget is not null)
                {
                    _additionalTargets!.RemoveAt(0);
                }
            }
            else
            {
                _additionalTargets?.Remove(target);
            }
            target._parent = null;
        }
    }

    internal void UnregisterFromOwner()
    {
        _parent?.UnregisterTarget(this);
    }

    public void Dispose()
    {
        Dispatcher.UIThread.VerifyAccess();
        _phase = Phase.Released;
        _parent?.UnregisterTarget(this);
        var suppression = _suppression;
        _suppression = null;
        try
        {
            VisitTargets(static target => target.Dispose());
        }
        finally
        {
            _firstTarget = null;
            _additionalTargets = null;
            suppression?.Dispose();
        }
    }

    private void VisitTargets(Action<TransitionInitializationSession> action)
    {
        if (_firstTarget is not { } first)
        {
            return;
        }

        // One template target is the common case: no list, backing array or snapshot.
        // Multiple targets snapshot only at lifecycle boundaries to allow retemplating.
        var additional = _additionalTargets is { Count: > 0 } ? _additionalTargets.ToArray() : null;
        List<Exception>? failures = null;
        Invoke(first);
        if (additional is not null)
        {
            foreach (var target in additional)
            {
                Invoke(target);
            }
        }

        void Invoke(TransitionInitializationSession target)
        {
            if (!ReferenceEquals(target._parent, this))
            {
                return;
            }
            try
            {
                action(target);
            }
            catch (Exception exception)
            {
                (failures ??= []).Add(exception);
            }
        }
        if (failures is not null)
        {
            throw new AggregateException(failures);
        }
    }

    private enum Phase : byte
    {
        Initializing,
        Resetting,
        Ready,
        Released
    }
}

internal interface ITransitionInitializationOwner
{
    TransitionInitializationSession TransitionInitialization { get; }
}
