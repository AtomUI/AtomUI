using System.Reactive.Disposables;
using Avalonia;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace AtomUI.Controls.Commons;

/// <summary>
/// Decides whether a continuous animation owned by a control should run: motion is enabled, the owner is attached to
/// a visual tree, every visual ancestor is visible, and the effective <see cref="MotionActivity"/> source is active.
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

    /// <param name="owner">The control whose animation is gated.</param>
    /// <param name="shouldRunChanged">Called with the new <see cref="ShouldRun"/> each time it changes, after the
    /// dispatcher job that changed it; not called for the initial value, which the owner reads from
    /// <see cref="ShouldRun"/>.</param>
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

    /// <summary>Usually bound to the <c>EnableMotion</c> token through the owner's own motion property.</summary>
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

    public bool ShouldRun => _shouldRun;

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
    }

    private void Attach()
    {
        _attachment?.Dispose();
        _attachment = new CompositeDisposable();
        _isAttached = true;
        ResolveSource();
        // Reports the current value synchronously, then every ancestor visibility change.
        _owner.TrackEffectiveVisibility(isVisible =>
        {
            _isVisible = isVisible;
            Update();
        }, _attachment);
    }

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
