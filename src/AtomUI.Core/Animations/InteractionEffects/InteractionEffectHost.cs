using System.Runtime.CompilerServices;
using AtomUI.Controls;
using AtomUI.Theme;
using AtomUI.Theme.Resources;
using AtomUI.Theme.Compilation;
using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Logging;
using Avalonia.Media;
using Avalonia.VisualTree;

namespace AtomUI.Animations;

internal interface IReusableInteractionVisual
{
    void ReturnToPool();
}

/// <summary>
/// Owns transient feedback from the first callback through the last presentation lease.
/// </summary>
internal sealed class InteractionEffectHost : IInteractionEffectPlayback
{
    private const int MaxConcurrentOperations = 8;
    private static long s_nextOperation;
    private static readonly ConditionalWeakTable<Control, List<InteractionEffectHost>> s_active = new();

    private readonly Control _surface;
    private readonly InteractionEffect _effect;
    private readonly InteractionEffectInputSource _input;
    private readonly Point? _origin;
    private readonly CornerRadius _radius;
    private readonly IBrush? _brush;
    private readonly ThemeContext? _themeContext;
    private readonly object? _selection;
    private readonly TopLevel? _topLevel;
    private readonly long _id = ++s_nextOperation;
    private readonly List<Lease> _leases = [];
    private IInteractionEffectSession? _session;
    private bool _revoked;
    private bool _finished;
    private bool _inCallback;
    private bool _startingPress;
    private bool _hadPress;
    private bool _accepted;
    private bool _activated;

    private InteractionEffectHost(Control surface, InteractionEffect effect, InteractionEffectInputSource input,
                                  Point? origin, CornerRadius radius, IBrush? brush, ThemeContext? context)
    {
        _surface = surface;
        _effect = effect;
        _input = input;
        _origin = origin;
        _radius = radius;
        _brush = brush;
        _themeContext = context;
        _selection = context?.Snapshot;
        _topLevel = TopLevel.GetTopLevel(surface);
        var operations = s_active.GetValue(surface, static _ => []);
        if (operations.Count == MaxConcurrentOperations)
        {
            operations[0].Cancel();
        }
        // Cancelling the final old request may have removed its list from the table.
        s_active.GetValue(surface, static _ => []).Add(this);
        surface.DetachedFromVisualTree += OnDetached;
        surface.ResourcesChanged += OnResourcesChanged;
        surface.PropertyChanged += OnPropertyChanged;
    }

    internal static InteractionEffectHost? Begin(Control surface, InteractionEffect fallback,
        InteractionEffectInputSource input, Point? origin, CornerRadius radius, IBrush? brush)
    {
        var host = Create(surface, fallback, input, origin, radius, brush);
        if (host is null)
        {
            return null;
        }
        host._hadPress = true;
        host._startingPress = true;
        host.Invoke(() =>
        {
            var session = host._effect.OnPressStarted(new InteractionEffectStartContext(host, host._id,
                input, origin, surface.Bounds.Size, radius, host._topLevel?.RenderScaling ?? 1, brush));
            if (host._revoked)
            {
                EndSession(session, cancel: true, activated: false);
            }
            else
            {
                host._session = session;
            }
        });
        host._startingPress = false;
        return host._revoked ? null : host;
    }

    internal static void Activate(Control surface, InteractionEffect fallback, InteractionEffectInputSource input,
                                  Point? origin, CornerRadius radius, IBrush? brush)
    {
        Create(surface, fallback, input, origin, radius, brush)?.Complete(activated: true);
    }

    internal static void CancelFor(Control surface)
    {
        if (s_active.TryGetValue(surface, out var operations))
        {
            foreach (var operation in operations.ToArray())
            {
                operation.Cancel();
            }
        }
    }

    private static InteractionEffectHost? Create(Control surface, InteractionEffect fallback,
        InteractionEffectInputSource input, Point? origin, CornerRadius radius, IBrush? brush)
    {
        if (!CanPlay(surface))
        {
            return null;
        }
        var context = ThemeScope.ResolveContext(surface);
        var snapshot = context?.Snapshot;
        var effect = InteractionEffect.GetEffect(surface) ?? ResolveLanguageEffect(surface, snapshot) ?? fallback;
        return ReferenceEquals(effect, InteractionEffect.None) ? null :
            new InteractionEffectHost(surface, effect, input, origin, radius, brush, context);
    }

    internal static InteractionEffect? ResolveLanguageEffect(Control surface, ThemeSnapshot? snapshot)
    {
        if (snapshot is null)
        {
            return null;
        }
        var owner = ThemeScope.GetTokenOwner(surface);
        var variant = snapshot.Variant;
        if (snapshot.Language.Language.GetInteractionEffect(owner, variant) is { } selected)
        {
            return selected;
        }
        for (var type = snapshot.Language.LanguageTokenType.BaseType;
             type is not null && type != typeof(LanguageDesignToken); type = type.BaseType)
        {
            foreach (var candidate in snapshot.Registry.Languages)
            {
                if (candidate.LanguageTokenType == type &&
                    candidate.Language.GetInteractionEffect(owner, variant) is { } parentEffect)
                {
                    return parentEffect;
                }
            }
        }
        return null;
    }

    internal void Complete(bool activated)
    {
        if (_finished || _revoked)
        {
            return;
        }
        activated |= _accepted || _activated;
        _finished = true;
        var session = _session;
        _session = null;
        Invoke(() =>
        {
            EndSession(session, cancel: false, activated);
            if (activated)
            {
                PublishActivation();
            }
        });
        foreach (var lease in _leases.ToArray())
        {
            lease.ReleaseHold();
        }
        ReleaseIfIdle();
    }

    internal void Accept() => _accepted = true;

    private void PublishActivation()
    {
        if (_activated || _revoked)
        {
            return;
        }
        _activated = true;
        _effect.OnActivated(new InteractionEffectActivateContext(this, _id, _input, _origin,
            _surface.Bounds.Size, _radius, _topLevel?.RenderScaling ?? 1, _brush, _hadPress));
    }

    internal void Cancel()
    {
        if (_revoked)
        {
            return;
        }
        _revoked = true;
        var session = _session;
        _session = null;
        foreach (var lease in _leases.ToArray())
        {
            lease.Revoke();
        }
        try
        {
            EndSession(session, cancel: true, activated: false);
        }
        catch (Exception error)
        {
            Report(error);
        }
        ReleaseIfIdle();
    }

    IDecorationEffectLease IInteractionEffectPlayback.PlayDecoration(Func<Control> createVisual,
        AbstractMotion motion, InteractionEffectClip clip)
    {
        ArgumentNullException.ThrowIfNull(createVisual);
        ArgumentNullException.ThrowIfNull(motion);
        if (!Enum.IsDefined(clip))
        {
            throw new ArgumentOutOfRangeException(nameof(clip));
        }
        if (!CanStartLease() || AdornerLayer.GetAdornerLayer(_surface) is not { } layer)
        {
            return Lease.Empty;
        }
        var lease = new Lease(this, _startingPress, decoration: true);
        _leases.Add(lease);
        try
        {
            var visual = createVisual();
            if (_revoked)
            {
                (visual as IReusableInteractionVisual)?.ReturnToPool();
                return Lease.Empty;
            }
            ArgumentNullException.ThrowIfNull(visual);
            if (visual.GetVisualParent() is not null || visual.Parent is not null)
            {
                throw new InvalidOperationException("An interaction decoration requires an unparented control.");
            }
            lease.Attach(layer, visual, clip, motion);
            return lease;
        }
        catch
        {
            lease.Revoke();
            throw;
        }
    }

    IDisposable IInteractionEffectPlayback.PlayOnSurface(AbstractMotion motion)
    {
        ArgumentNullException.ThrowIfNull(motion);
        if (!CanStartLease())
        {
            return Lease.Empty;
        }
        foreach (var existing in _leases)
        {
            if (existing.IsSurface)
            {
                return existing.TryBeginExit(motion) ? existing : Lease.Empty;
            }
        }
        if (MotionPlayback.HasPresentation(_surface))
        {
            return Lease.Empty;
        }
        var lease = new Lease(this, _startingPress, decoration: false);
        _leases.Add(lease);
        try
        {
            lease.Play(_surface, motion);
            return lease;
        }
        catch
        {
            lease.Revoke();
            throw;
        }
    }

    private bool CanStartLease() => _inCallback && !_revoked && _leases.Count < MaxConcurrentOperations;

    private static bool CanPlay(Control surface) => surface.IsAttachedToVisualTree() && surface.IsEffectivelyVisible &&
        (surface is not IMotionAwareControl control || !surface.IsSet(MotionAwareControlProperty.IsMotionEnabledProperty) ||
         control.IsMotionEnabled) && Motion.IsAnimationEnabled(surface);

    internal T GetMetric<T>(SharedTokenKind key, T fallback) =>
        MotionPreparation.ReadToken(_surface, key) is T value ? value : fallback;

    private void Invoke(Action callback)
    {
        _inCallback = true;
        try
        {
            callback();
        }
        catch (Exception error)
        {
            Report(error);
            Cancel();
        }
        finally
        {
            _inCallback = false;
            ReleaseIfIdle();
        }
    }

    private static void EndSession(IInteractionEffectSession? session, bool cancel, bool activated)
    {
        if (session is null)
        {
            return;
        }
        try
        {
            if (cancel)
            {
                session.Cancel();
            }
            else
            {
                session.Complete(activated);
            }
        }
        finally
        {
            session.Dispose();
        }
    }

    private void OnDetached(object? sender, VisualTreeAttachmentEventArgs args) => Cancel();

    private void OnResourcesChanged(object? sender, ResourcesChangedEventArgs args)
    {
        var current = ThemeScope.ResolveContext(_surface);
        if (!ReferenceEquals(current, _themeContext) || !ReferenceEquals(current?.Snapshot, _selection) || !CanPlay(_surface))
        {
            Cancel();
        }
    }

    private void OnPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs args)
    {
        if (args.Property == StyledElement.ThemeProperty || args.Property == TemplatedControl.TemplateProperty ||
            args.Property == InteractionEffect.EffectProperty ||
            args.Property == Motion.IsEnabledProperty || args.Property == MotionAwareControlProperty.IsMotionEnabledProperty ||
            (args.Property == Visual.IsVisibleProperty && !_surface.IsVisible))
        {
            Cancel();
        }
    }

    private void ReleaseIfIdle()
    {
        if ((!_finished && !_revoked) || _inCallback || _leases.Count != 0)
        {
            return;
        }
        _surface.DetachedFromVisualTree -= OnDetached;
        _surface.ResourcesChanged -= OnResourcesChanged;
        _surface.PropertyChanged -= OnPropertyChanged;
        if (s_active.TryGetValue(_surface, out var operations) && operations.Remove(this) && operations.Count == 0)
        {
            s_active.Remove(_surface);
        }
    }

    private void Report(Exception error) => Logger.TryGet(LogEventLevel.Warning, LogArea.Animations)?.Log(_surface,
        "Interaction effect failed for operation {OperationId}: {Error}", _id, error);

    private sealed class Lease : IDecorationEffectLease
    {
        internal static readonly IDecorationEffectLease Empty = new Lease(null, hold: false, decoration: false);
        private readonly InteractionEffectHost? _owner;
        private readonly bool _hold;
        private readonly bool _decoration;
        private Control? _target;
        private InteractionEffectAdorner? _adorner;
        private AdornerLayer? _layer;
        private CancellationTokenSource? _token;
        private MotionResult _result;
        private long _requestId;
        private int _run;
        private bool _exiting;
        private bool _disposed;

        internal bool IsSurface => !_decoration;

        internal Lease(InteractionEffectHost? owner, bool hold, bool decoration)
        {
            _owner = owner;
            _hold = hold;
            _decoration = decoration;
        }

        internal void Attach(AdornerLayer layer, Control target, InteractionEffectClip clip, AbstractMotion motion)
        {
            var surface = _owner!._surface;
            _layer = layer;
            _adorner = new InteractionEffectAdorner(surface, target, clip, _owner._radius);
            AdornerLayer.SetAdornedElement(_adorner, surface);
            AdornerLayer.SetIsClipEnabled(_adorner, false);
            layer.Children.Add(_adorner);
            _adorner.Measure(surface.Bounds.Size);
            _adorner.Arrange(new Rect(surface.Bounds.Size));
            Play(target, motion);
        }

        internal void Play(Control target, AbstractMotion motion)
        {
            _target = target;
            _token ??= new CancellationTokenSource();
            var run = ++_run;
            var playback = motion.PlayAsync(target, _token.Token);
            _requestId = MotionPlayback.CurrentRequestId(target);
            _ = Observe(playback, run);
        }

        public bool TryBeginExit(AbstractMotion exit)
        {
            ArgumentNullException.ThrowIfNull(exit);
            if (_disposed || _exiting || !_hold || _target is not { } target || _owner?._revoked != false)
            {
                return false;
            }
            if (!_decoration && MotionPlayback.CurrentRequestId(target) != _requestId)
            {
                Revoke();
                return false;
            }
            _exiting = true;
            try
            {
                Play(target, exit);
                return true;
            }
            catch (Exception error)
            {
                _owner.Report(error);
                Revoke();
                return false;
            }
        }

        public void Dispose()
        {
            if (!_exiting)
            {
                Revoke();
            }
        }

        internal void ReleaseHold()
        {
            if (_hold)
            {
                Dispose();
            }
        }

        internal void Revoke()
        {
            if (_disposed || _owner is null)
            {
                return;
            }
            _disposed = true;
            _run++;
            if (_adorner is { } adorner)
            {
                adorner.IsVisible = false;
            }
            _token?.Cancel();
            _token?.Dispose();
            if (_target is { } target)
            {
                if (_decoration)
                {
                    MotionPlayback.CancelAndReset(target);
                }
                else if (Motion.IsCurrent(target, _result))
                {
                    Motion.Reset(target);
                }
            }
            if (_adorner is { } attached)
            {
                _layer!.Children.Remove(attached);
                AdornerLayer.SetAdornedElement(attached, null);
                attached.ReleaseVisual();
            }
            if (_decoration && _target is IReusableInteractionVisual reusable)
            {
                reusable.ReturnToPool();
            }
            _layer = null;
            _adorner = null;
            _target = null;
            _owner._leases.Remove(this);
            _owner.ReleaseIfIdle();
        }

        private async Task Observe(Task<MotionResult> task, int run)
        {
            try
            {
                var result = await task;
                if (run != _run)
                {
                    return;
                }
                _result = result;
                if (!_hold || _exiting || !result.IsCompleted)
                {
                    Revoke();
                }
            }
            catch (Exception error)
            {
                if (run == _run)
                {
                    _owner!.Report(error);
                    Revoke();
                }
            }
        }
    }
}
