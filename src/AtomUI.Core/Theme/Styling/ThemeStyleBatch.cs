namespace AtomUI.Theme.Styling;

internal sealed class ThemeStyleBatch : IDisposable
{
    private readonly List<ThemeStyleUpdate> _updates = [];
    private (ThemeContext Context, ThemeStyleInputs Previous, ThemeStyleInputs Next,
        ThemeStyleContext? Styles, ThemeStyleContextStamp? Stamp)[] _targets;
    private bool _committed;
    private bool _completed;
    private bool _prepared;
    private bool _activated;

    internal ThemeStyleBatch(IReadOnlyList<(ThemeContext Context, ThemeStyleInputs Inputs)> targets)
    {
        if (targets.Select(static item => item.Context).Distinct().Count() != targets.Count)
        {
            throw new ArgumentException("Each context may occur only once in a presentation batch.", nameof(targets));
        }
        _targets = targets.Select(static item => (item.Context, item.Context.StyleInputs, item.Inputs,
            item.Context.InitializedStyles, item.Context.InitializedStyles?.CaptureStamp())).ToArray();
        HasChanges = _targets.Any(static target => !target.Previous.Equals(target.Next));
    }

    internal bool HasChanges { get; }
    internal bool IsCommitted => _committed;
    internal IEnumerable<ThemeContext> ChangedContexts => _targets
        .Where(static target => !target.Previous.Equals(target.Next)).Select(static target => target.Context);

    internal void Prepare()
    {
        if (_prepared || _completed)
        {
            throw new InvalidOperationException("A presentation batch can only prepare once.");
        }
        _prepared = true;
        foreach (var target in _targets)
        {
            ValidateCapture(target.Context, target.Previous, target.Styles, target.Stamp);
            if (!target.Previous.AssetsEqual(target.Next))
            {
                _updates.Add(target.Context.Styles.Prepare(target.Next.Assets));
            }
        }
    }

    internal void WireAndActivate()
    {
        if (!_prepared || _activated || _completed)
        {
            throw new InvalidOperationException("A presentation batch must be prepared before its single activation.");
        }
        _activated = true;
        foreach (var update in _updates)
        {
            update.Wire();
        }
        foreach (var update in _updates)
        {
            update.Activate();
        }
        Validate();
    }

    internal void Validate()
    {
        if (_committed || _completed || HasChanges && !_activated)
        {
            throw new InvalidOperationException("Only a pending, fully activated presentation batch can be validated.");
        }
        foreach (var target in _targets)
        {
            if (!ReferenceEquals(target.Context.StyleInputs, target.Previous))
            {
                throw new InvalidOperationException("Effective style inputs changed during preparation.");
            }
            if (target.Previous.AssetsEqual(target.Next))
            {
                ValidateCapture(target.Context, target.Previous, target.Styles, target.Stamp);
            }
        }
        foreach (var update in _updates)
        {
            update.Validate();
        }
    }

    private static void ValidateCapture(ThemeContext context, ThemeStyleInputs inputs,
        ThemeStyleContext? styles, ThemeStyleContextStamp? stamp)
    {
        if (!ReferenceEquals(context.StyleInputs, inputs) ||
            !ReferenceEquals(context.InitializedStyles, styles) || stamp?.IsCurrent == false)
        {
            throw new InvalidOperationException("A captured presentation context or host changed during preparation.");
        }
    }

    // Must follow validation of all participating updates and manager/scope stamps, with no callback in between.
    internal void CommitPrepared()
    {
        if (!_activated || _committed || _completed)
        {
            throw new InvalidOperationException("Only a fully activated batch can enter the reference commit phase.");
        }
        foreach (var update in _updates)
        {
            update.CommitPrepared();
        }
        foreach (var target in _targets)
        {
            target.Context.CommitStyleInputs(target.Next);
        }
        _committed = true;
    }

    public void Dispose()
    {
        if (_completed)
        {
            return;
        }
        var failures = new List<Exception>();
        foreach (var update in _updates.AsEnumerable().Reverse())
        {
            try
            {
                if (_committed)
                {
                    update.Retire();
                }
                else
                {
                    update.Rollback();
                }
            }
            catch (Exception exception)
            {
                failures.Add(exception);
            }
        }
        if (failures.Count != 0)
        {
            throw new AggregateException("Presentation cleanup is incomplete.", failures);
        }
        _completed = true;
        _updates.Clear();
        _targets = [];
    }
}
