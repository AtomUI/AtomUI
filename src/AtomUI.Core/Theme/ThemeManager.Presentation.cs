using AtomUI.Theme.Styling;

namespace AtomUI.Theme;

internal partial class ThemeManager
{
    private readonly Dictionary<IDisposable, string> _presentationCleanup = new(ReferenceEqualityComparer.Instance);
    private bool _retryingPresentationCleanup;
    private bool _reportingPresentation;

    public ThemePresentationResult Presentation { get; private set; } = ThemePresentationResult.Unchanged;
    public event EventHandler<ThemePresentationChangedEventArgs>? ThemePresentationChanged;

    public ThemePresentationResult RetryPresentationCleanup()
    {
        if (!_checkTransitionAccess())
        {
            throw new InvalidOperationException("Presentation cleanup must run on the UI thread.");
        }
        if (_retryingPresentationCleanup)
        {
            throw new InvalidOperationException("Presentation cleanup cannot be reentered.");
        }
        if (_presentationCleanup.Count == 0)
        {
            return ThemePresentationResult.Unchanged;
        }
        _retryingPresentationCleanup = true;
        try
        {
            var failures = new List<Exception>();
            foreach (var item in _presentationCleanup.ToArray())
            {
                try
                {
                    item.Key.Dispose();
                    _presentationCleanup.Remove(item.Key);
                }
                catch (Exception exception)
                {
                    failures.Add(exception);
                }
            }
            return ReportPresentation(failures.Count == 0
                ? new ThemePresentationResult(ThemePresentationStatus.Restored, [])
                : PresentationFailure("CleanupRetry", new AggregateException(failures)));
        }
        finally
        {
            _retryingPresentationCleanup = false;
        }
    }

    internal ThemePresentationResult CleanupPresentation(IDisposable? responsibility, string path)
    {
        if (responsibility is null)
        {
            return ThemePresentationResult.Unchanged;
        }
        try
        {
            responsibility.Dispose();
            _presentationCleanup.Remove(responsibility);
            return ThemePresentationResult.Unchanged;
        }
        catch (Exception exception)
        {
            // Entries correspond to already enrolled hosts/contexts. New mutations are gated until
            // these exact responsibilities are released; failed retries never append another entry.
            return RetainPresentationCleanup(responsibility, path, exception);
        }
    }

    internal ThemePresentationResult RetainPresentationCleanup(IDisposable responsibility, string path, Exception exception)
    {
        _presentationCleanup.TryAdd(responsibility, path);
        return ReportPresentation(PresentationFailure(path, exception));
    }

    private ThemePresentationResult FinishPresentation(ThemeStyleBatch? batch)
    {
        if (batch is null)
        {
            return ThemePresentationResult.Unchanged;
        }
        var changedContexts = batch.ChangedContexts.ToArray();
        var result = CleanupPresentation(batch, "ThemeStyleBatch");
        if (_presentationCleanup.Count != 0)
        {
            result = Presentation;
        }
        else if (result.Status != ThemePresentationStatus.Degraded && batch.HasChanges)
        {
            result = ReportPresentation(new ThemePresentationResult(batch.IsCommitted
                ? ThemePresentationStatus.Applied : ThemePresentationStatus.Restored, []));
        }
        foreach (var context in changedContexts)
        {
            if (_scopeGraph.TryGetNode(context.RegistrationId, out var node) &&
                ReferenceEquals(node!.Context, context))
            {
                node.Provider.PublishPresentation(context, result);
            }
        }
        return result;
    }

    private ThemePresentationResult ReportPresentation(ThemePresentationResult result)
    {
        Presentation = result;
        var observerDiagnostics = new List<ThemeDiagnostic>();
        var wasReporting = _reportingPresentation;
        _reportingPresentation = true;
        try
        {
            ThemeEventDispatcher.Dispatch(ThemePresentationChanged, this,
                new ThemePresentationChangedEventArgs(result), observerDiagnostics, nameof(ThemePresentationChanged));
        }
        finally
        {
            _reportingPresentation = wasReporting;
        }
        return result;
    }

    private static ThemePresentationResult PresentationFailure(string path, Exception exception) => new(
        ThemePresentationStatus.Degraded,
        [new ThemeDiagnostic("ATMTHM7101", ThemeDiagnosticSeverity.Error, nameof(ThemeManager), path,
            $"Presentation cleanup remains pending: {exception.GetBaseException().Message}")], exception);

    private void EnsurePresentationReady()
    {
        if (_presentationCleanup.Count != 0 && _reportingPresentation)
        {
            throw new InvalidOperationException("A failed presentation cleanup notification cannot reenter theme mutation.");
        }
        if (_presentationCleanup.Count != 0 &&
            RetryPresentationCleanup().Status == ThemePresentationStatus.Degraded)
        {
            throw new InvalidOperationException("Presentation cleanup must complete before another theme mutation.");
        }
    }

    internal void EnsurePresentationHostReady()
    {
        if (_presentationCleanup.Count != 0)
        {
            throw new InvalidOperationException("Managed style hosts cannot attach while presentation cleanup is pending.");
        }
    }

    private ThemeStyleBatch CaptureCommittedPresentation(ThemeScopeCapture capture)
    {
        var targets = capture.Nodes.Select(static node => (node.Context, node.Context.StyleInputs)).ToList();
        if (_rootContext is not null)
        {
            targets.Insert(0, (_rootContext, _rootContext.StyleInputs));
        }
        return new ThemeStyleBatch(targets);
    }

    private ThemeStyleBatch PrepareRootPresentation(ThemeTransaction transaction)
    {
        var rootInputs = ThemeStyleInputs.Resolve(
            ThemeStyleInputs.Resolve(ThemeStyleInputs.Empty, _initialRequest.Config), transaction.Request.Config);
        var targets = new List<(ThemeContext Context, ThemeStyleInputs Inputs)>
        {
            (transaction.PreparedRootContext!, rootInputs)
        };
        targets.AddRange(CreateScopeStyleTargets(rootInputs, transaction.ScopeCapture!, useLastValidConfig: true));
        return new ThemeStyleBatch(targets);
    }

    private IReadOnlyList<(ThemeContext Context, ThemeStyleInputs Inputs)> CreateScopeStyleTargets(
        ThemeStyleInputs rootInputs,
        ThemeScopeCapture capture,
        bool useLastValidConfig)
    {
        var targets = new List<(ThemeContext Context, ThemeStyleInputs Inputs)>();
        var inputsByScope = new Dictionary<long, ThemeStyleInputs>();
        foreach (var node in capture.Nodes)
        {
            ThemeStyleInputs parent;
            if (node.Stamp.ParentRegistrationId == 0)
            {
                parent = rootInputs;
            }
            else if (!inputsByScope.TryGetValue(node.Stamp.ParentRegistrationId, out parent!))
            {
                parent = ResolveParentContext(node.Stamp.ParentRegistrationId).StyleInputs;
            }
            var inputs = ThemeStyleInputs.Resolve(parent, useLastValidConfig ? node.LastValidConfig : node.Config);
            targets.Add((node.Context, inputs));
            inputsByScope.Add(node.Stamp.RegistrationId, inputs);
        }
        return targets;
    }

    private ThemeContext ResolveParentContext(long registrationId)
    {
        if (registrationId == 0)
        {
            return RootContext;
        }
        return _scopeGraph.TryGetNode(registrationId, out var node)
            ? node!.Context
            : throw new InvalidOperationException("The parent theme context is no longer registered.");
    }
}
