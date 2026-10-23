using Avalonia;
using Avalonia.Styling;
using Avalonia.Threading;

namespace AtomUI.Theme.Styling;

internal sealed class ThemeStyleUpdate : IDisposable
{
    private ThemeStyleContext _context;
    private ThemeStyleGeneration _previous;
    private ThemeStyleGeneration _candidate;
    private readonly long _hostRevision;
    private (ThemeStyleHostLease Host, Styles Previous, Styles Candidate)[] _hosts;
    private ThemeStyleHostStamp[] _stamps;
    private int _wired;
    private UpdatePhase _phase;

    internal ThemeStyleUpdate(ThemeStyleContext context, ThemeStyleGeneration candidate, ThemeStyleHostLease[] hosts)
    {
        _context = context;
        _previous = context.Current;
        _candidate = candidate;
        _hostRevision = context.HostRevision;
        _hosts = hosts.Select(host => (host, host.Current, (Styles)null!)).ToArray();
        _stamps = hosts.Select(static host => host.CaptureStamp()).ToArray();
    }

    internal void PrepareHosts()
    {
        VerifyPending();
        if (_phase != UpdatePhase.Created)
        {
            throw new InvalidOperationException("A style update can only prepare its hosts once.");
        }
        _phase = UpdatePhase.Preparing;
        for (var index = 0; index < _hosts.Length; index++)
        {
            var host = _hosts[index];
            _hosts[index] = (host.Host, host.Previous, host.Host.Prepare(_candidate));
        }
        _phase = UpdatePhase.Prepared;
    }

    internal void Wire()
    {
        VerifyPending();
        EnsureCurrent();
        if (_phase != UpdatePhase.Prepared)
        {
            throw new InvalidOperationException("A style update can only be wired once.");
        }
        _phase = UpdatePhase.Wiring;
        while (_wired < _hosts.Length)
        {
            EnsureCurrent();
            var host = _hosts[_wired];
            _stamps[_wired] = _stamps[_wired].WithCandidate(host.Previous, host.Candidate);
            _wired++;
            host.Host.Mount(host.Candidate);
        }
        EnsureCurrent();
        _phase = UpdatePhase.Wired;
    }

    internal void Activate()
    {
        VerifyPending();
        EnsureCurrent();
        if (_phase != UpdatePhase.Wired)
        {
            throw new InvalidOperationException("A style update must be fully wired before its single activation.");
        }
        _phase = UpdatePhase.Activating;
        _context.SetValue(ThemeStyleContext.ActivationProperty, _candidate.Activation);
        EnsureCurrent();
        _phase = UpdatePhase.Active;
    }

    internal void Commit()
    {
        Validate();
        CommitPrepared();
    }

    internal void Validate()
    {
        VerifyPending();
        EnsureCurrent();
        if (_phase != UpdatePhase.Active ||
            !ReferenceEquals(_context.GetValue(ThemeStyleContext.ActivationProperty), _candidate.Activation))
        {
            throw new InvalidOperationException("Only the active style generation can be committed.");
        }

    }

    // The batch validates every update first, then calls this callback-free reference phase.
    internal void CommitPrepared()
    {
        if (_phase != UpdatePhase.Active)
        {
            throw new InvalidOperationException("Only an active update can enter the reference commit phase.");
        }
        _context.Commit(_candidate);
        foreach (var host in _hosts)
        {
            host.Host.Commit(host.Candidate);
        }
        _phase = UpdatePhase.Committed;
    }

    internal void Rollback()
    {
        Dispatcher.UIThread.VerifyAccess();
        if (_phase == UpdatePhase.Completed)
        {
            return;
        }
        if (_phase == UpdatePhase.Committed)
        {
            throw new InvalidOperationException("Committed styles cannot be rolled back.");
        }
        var errors = new List<Exception>();
        if (_phase >= UpdatePhase.Activating)
        {
            TryCleanup(() => _context.SetValue(ThemeStyleContext.ActivationProperty, _previous.Activation), errors);
        }
        for (var index = _wired - 1; index >= 0; index--)
        {
            var host = _hosts[index];
            TryCleanup(() => host.Host.Remove(host.Candidate), errors);
        }
        Complete(errors);
    }

    internal void Retire()
    {
        Dispatcher.UIThread.VerifyAccess();
        if (_phase == UpdatePhase.Completed)
        {
            return;
        }
        if (_phase != UpdatePhase.Committed)
        {
            throw new InvalidOperationException("Only committed styles can be retired.");
        }
        var errors = new List<Exception>();
        foreach (var host in _hosts)
        {
            TryCleanup(() => host.Host.Remove(host.Previous), errors);
        }
        Complete(errors);
    }

    public void Dispose()
    {
        if (_phase == UpdatePhase.Committed)
        {
            Retire();
        }
        else
        {
            Rollback();
        }
    }

    private void VerifyPending()
    {
        Dispatcher.UIThread.VerifyAccess();
        if (_phase >= UpdatePhase.Committed)
        {
            throw new InvalidOperationException("The style update is no longer pending.");
        }
    }

    private void EnsureCurrent()
    {
        if (_hostRevision != _context.HostRevision || !ReferenceEquals(_context.Current, _previous) ||
            _hosts.Where((host, index) => !host.Host.Matches(_stamps[index])).Any())
        {
            throw new InvalidOperationException("The style host topology or generation changed during preparation.");
        }
    }

    private void Complete(List<Exception> errors)
    {
        if (errors.Count != 0)
        {
            // Keep one pending update as the bounded cleanup responsibility. A retry can finish it.
            throw new AggregateException("Managed style restoration or retirement was incomplete.", errors);
        }
        _phase = UpdatePhase.Completed;
        _context.Complete(this);
        // A retained, completed handle must not retain hosts, old graphs or asset factories.
        _hosts = [];
        _stamps = [];
        _context = null!;
        _previous = null!;
        _candidate = null!;
    }

    private static void TryCleanup(Action action, List<Exception> errors)
    {
        try
        {
            action();
        }
        catch (Exception exception)
        {
            errors.Add(exception);
        }
    }

    private enum UpdatePhase
    {
        Created,
        Preparing,
        Prepared,
        Wiring,
        Wired,
        Activating,
        Active,
        Committed,
        Completed
    }
}
