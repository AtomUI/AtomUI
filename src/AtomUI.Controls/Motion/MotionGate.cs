using System.Reactive.Disposables;
using Avalonia;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace AtomUI.Controls.Commons;

/// <summary>
/// Decides whether a continuous animation owned by a control should run: motion is enabled, the owner is attached to
/// a visual tree, the owner and every visual ancestor of it are visible, and the effective
/// <see cref="MotionActivity" /> source is active.
/// The gate holds no timer; it tells the owner when the answer changes and the owner starts or stops its animation.
/// Changes within one dispatcher job are coalesced: a visibility or activity flicker that ends where it started does
/// not stop and restart the animation.
/// </summary>
public sealed class MotionGate : IDisposable
{
    private readonly Visual _owner;
    private readonly Action<bool> _shouldRunChanged;
    private CompositeDisposable? _attachment;
    private IMotionActivitySource? _source;
    private bool _isMotionEnabled = true;
    private bool _isAttached;
    private bool _isVisible;
    private bool _shouldRun;
    private bool _isInitialized;
    private bool _isFlushPosted;
    private bool _isDisposed;

    /// <summary>
    /// Creates the gate with <see cref="ShouldRun" /> evaluated from the current state of <paramref name="owner" />.
    /// </summary>
    /// <param name="owner">
    /// The control whose animation is gated.
    /// </param>
    /// <param name="shouldRunChanged">
    /// Called with the new <see cref="ShouldRun" /> each time it changes, after the dispatcher job that changed it; not
    /// called for the initial value, which the owner reads from <see cref="ShouldRun" />.
    /// </param>
    public MotionGate(Visual owner, Action<bool> shouldRunChanged)
    {
        _owner            = owner;
        _shouldRunChanged = shouldRunChanged;
        _owner.AttachedToVisualTree   += HandleAttachedToVisualTree;
        _owner.DetachedFromVisualTree += HandleDetachedFromVisualTree;
        _owner.PropertyChanged        += HandleOwnerPropertyChanged;
        if (_owner.IsAttachedToVisualTree())
        {
            Attach();
        }
        _shouldRun     = Evaluate();
        _isInitialized = true;
    }

    /// <summary>
    /// When it is <see langword="false" />, <see cref="ShouldRun" /> is <see langword="false" />. The default is
    /// <see langword="true" />. Usually bound to the <c>EnableMotion</c> token through the owner's own motion property.
    /// </summary>
    public bool IsMotionEnabled
    {
        get => _isMotionEnabled;
        set
        {
            if (_isMotionEnabled == value)
            {
                return;
            }
            _isMotionEnabled = value;
            Update();
        }
    }

    /// <summary>
    /// Whether the owner's animation should run now; it is <see langword="false" /> after <see cref="Dispose" />. The
    /// value changes in the same step as the <c>shouldRunChanged</c> callback, after the dispatcher job that changed an
    /// input, so it can lag such a change within that job.
    /// </summary>
    public bool ShouldRun => _shouldRun;

    /// <summary>
    /// Detaches the gate from its owner: it unsubscribes from the owner's events, drops the ancestor visibility
    /// tracking and the activity source, and sets <see cref="ShouldRun" /> to <see langword="false" /> without raising
    /// the <c>shouldRunChanged</c> callback. Calling it again has no effect.
    /// </summary>
    public void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }
        _isDisposed = true;
        _owner.AttachedToVisualTree   -= HandleAttachedToVisualTree;
        _owner.DetachedFromVisualTree -= HandleDetachedFromVisualTree;
        _owner.PropertyChanged        -= HandleOwnerPropertyChanged;
        Detach();
        _shouldRun = false;
    }

    private void HandleAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        // A gate created inside the owner's OnAttachedToVisualTree already attached in its constructor; the event
        // Avalonia raises right after that override must not rebuild the ancestor subscriptions.
        if (_isAttached)
        {
            return;
        }
        Attach();
        Update();
    }

    private void HandleDetachedFromVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        Detach();
        Update();
    }

    private void HandleOwnerPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == MotionActivity.SourceProperty && _isAttached)
        {
            ResolveSource();
            Update();
        }
        else if (e.Property == Visual.IsVisibleProperty && _isAttached)
        {
            ConfigureVisibilityTracking();
            Update();
        }
    }

    private void Attach()
    {
        _isAttached = true;
        ResolveSource();
        ConfigureVisibilityTracking();
    }

    private void ConfigureVisibilityTracking()
    {
        _attachment?.Dispose();
        _attachment = null;
        // A hidden owner is never effectively visible whatever its ancestors are, so the ancestor chain is watched
        // only while the owner itself is visible. Many owners start hidden (placeholders, optional parts), and each
        // tracked owner subscribes to every ancestor. The owner's own IsVisible changes re-run this method.
        if (!_owner.IsVisible)
        {
            _isVisible = false;
            return;
        }

        _attachment = new CompositeDisposable();
        // Reports the current value synchronously, then every ancestor visibility change.
        _owner.TrackEffectiveVisibility(isVisible =>
        {
            _isVisible = isVisible;
            Update();
        }, _attachment);
    }

    /// <summary>
    /// Gets whether the gate currently subscribes to the owner's visual ancestors.
    /// </summary>
    internal bool IsTrackingAncestorVisibility => _attachment is not null;

    private void Detach()
    {
        _attachment?.Dispose();
        _attachment = null;
        _isAttached = false;
        _isVisible  = false;
        SetSource(null);
    }

    private void ResolveSource() => SetSource(MotionActivity.GetSource(_owner) ?? MotionActivity.Application);

    private void SetSource(IMotionActivitySource? source)
    {
        if (ReferenceEquals(_source, source))
        {
            return;
        }
        if (_source is not null)
        {
            _source.IsActiveChanged -= HandleSourceActiveChanged;
        }
        _source = source;
        if (_source is not null)
        {
            _source.IsActiveChanged += HandleSourceActiveChanged;
        }
    }

    private void HandleSourceActiveChanged(object? sender, EventArgs e) => Update();

    private bool Evaluate() =>
        !_isDisposed && _isMotionEnabled && _isAttached && _isVisible && (_source?.IsActive ?? false);

    private void Update()
    {
        if (!_isInitialized || _isFlushPosted || Evaluate() == _shouldRun)
        {
            return;
        }
        _isFlushPosted = true;
        Dispatcher.UIThread.Post(Flush);
    }

    private void Flush()
    {
        _isFlushPosted = false;
        var next = Evaluate();
        if (next == _shouldRun)
        {
            return;
        }
        _shouldRun = next;
        _shouldRunChanged(next);
    }
}
