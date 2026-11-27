using System.ComponentModel;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Display state machine of the preview window. In the <see cref="ImageSwitchMode.Immediate" /> mode the
/// target shows the loading placeholder as soon as it has no image. In the
/// <see cref="ImageSwitchMode.WaitForLoaded" /> mode the entry that last held a usable full image stays as the
/// retained frame until the target has loaded. <c>ImagePreviewerDialog</c> and <c>ImagePreviewerOverlayHost</c>
/// share it.
/// </summary>
/// <remarks>
/// <see cref="ImageSwitchMode.WaitForLoaded" /> holds at most one retained frame entry besides the current
/// entry. The subscriptions of the current entry and of the retained frame change only through
/// <see cref="SetCurrentItem" /> and <see cref="SetRetained" />, each subscription paired with its
/// unsubscription. <see cref="Recompute" />, the hot path, does not allocate.
/// </remarks>
internal sealed class ImagePreviewDisplayTracker : IDisposable
{
    private static long s_nextTrackerId;

    private readonly Func<ImageSwitchMode> _modeAccessor;
    private readonly Func<ImagePreviewEntry?>? _retainedSeedAccessor;
    private readonly Action _stateChanged;
    private readonly long _trackerId = Interlocked.Increment(ref s_nextTrackerId);
    private ImagePreviewEntry? _currentItem;
    private ImagePreviewEntry? _retainedItem;
    private long _nextTargetRevision;

    internal ImagePreviewDisplayTracker(
        Func<ImageSwitchMode> modeAccessor,
        Action stateChanged,
        Func<ImagePreviewEntry?>? retainedSeedAccessor = null)
    {
        _modeAccessor          = modeAccessor ?? throw new ArgumentNullException(nameof(modeAccessor));
        _stateChanged          = stateChanged ?? throw new ArgumentNullException(nameof(stateChanged));
        _retainedSeedAccessor  = retainedSeedAccessor;
    }

    internal ImagePreviewEntry? CurrentItem => _currentItem;

    internal IImage? EffectiveImage { get; private set; }

    internal Size EffectiveImageSize { get; private set; }

    internal bool IsCurrentLoading { get; private set; }

    internal bool IsCurrentFailed { get; private set; }

    internal void SetCurrentItem(ImagePreviewEntry? item)
    {
        if (!ReferenceEquals(_currentItem, item))
        {
            // Subscription invariant: an entry is subscribed while it is the current or the retained one.
            // The previous current entry is often also the retained frame (the usual state after a load completed).
            // Unsubscribing it would cut the retained frame off from its notifications, so its subscription stays.
            var previous = _currentItem;
            _currentItem = item;
            if (_currentItem is not null)
            {
                // The revision only orders display targets by age within this tracker. The owner id keeps a session of
                // the host that is already closed apart. Both are plain values: they do not hold the entry or extend
                // its lifetime.
                _currentItem.DisplayTargetOwnerId = _trackerId;
                _currentItem.DisplayTargetRevision = ++_nextTargetRevision;
            }
            if (previous is not null && !ReferenceEquals(previous, _retainedItem))
            {
                previous.PropertyChanged -= HandleEntryPropertyChanged;
            }
            if (_currentItem is not null && !ReferenceEquals(_currentItem, _retainedItem))
            {
                _currentItem.PropertyChanged += HandleEntryPropertyChanged;
            }
        }
        Recompute();
    }

    internal void Clear()
    {
        if (_currentItem is not null)
        {
            _currentItem.PropertyChanged -= HandleEntryPropertyChanged;
            _currentItem = null;
        }
        SetRetained(null);
        Recompute();
    }

    internal void Refresh()
    {
        Recompute();
    }

    private void Recompute()
    {
        var mode = _modeAccessor();
        if (_retainedItem is not null &&
            (_currentItem is null || _retainedItem.FullImage is null))
        {
            // Drop the retained frame when there is no target entry (collection cleared, preview closed), or when the
            // retained entry has unloaded, been disposed or failed to commit. Going through SetRetained unsubscribes
            // it synchronously.
            SetRetained(null);
        }

        var currentImage = _currentItem?.FullImage;
        var needsFallback = false;
        if (currentImage is not null)
        {
            SetRetained(mode == ImageSwitchMode.WaitForLoaded ? _currentItem : null);
        }
        else if (_currentItem is { FullImage: null, IsFullFailed: false } &&
                 mode == ImageSwitchMode.WaitForLoaded)
        {
            needsFallback = true;
        }
        else
        {
            // Immediate keeps no old image: from the Idle state at the moment of the switch, the target goes
            // straight to the loading placeholder.
            SetRetained(null);
        }
        if (needsFallback)
        {
            currentImage = _retainedItem?.FullImage;
            if (_retainedSeedAccessor is not null)
            {
                // With rapid switching the previous target may finish loading only after it is no longer the current
                // entry. Only a target newer than the current retained frame is adopted, so that out-of-order
                // completions never move the display back to an older image.
                var seed = _retainedSeedAccessor.Invoke();
                var retainedRevision = _retainedItem?.DisplayTargetRevision ?? 0;
                if (seed?.FullImage is not null &&
                    seed.DisplayTargetOwnerId == _trackerId &&
                    seed.DisplayTargetRevision > retainedRevision)
                {
                    SetRetained(seed);
                    currentImage = seed.FullImage;
                }
            }
        }

        var imageSize = currentImage is null
            ? default
            : ReferenceEquals(currentImage, _currentItem?.FullImage)
                ? _currentItem.FullImageSize
                : _retainedItem?.FullImageSize ?? currentImage.Size;
        var isLoading = _currentItem?.IsFullLoading == true ||
                        _currentItem is { FullImage: null, IsFullFailed: false };
        var isFailed  = _currentItem?.IsFullFailed == true;
        if (!ReferenceEquals(EffectiveImage, currentImage) ||
            EffectiveImageSize != imageSize ||
            IsCurrentLoading != isLoading ||
            IsCurrentFailed != isFailed)
        {
            // Skip the host callback while nothing changed, to avoid redundant SetCurrentValue calls and capability
            // recalculations.
            EffectiveImage    = currentImage;
            EffectiveImageSize = imageSize;
            IsCurrentLoading = isLoading;
            IsCurrentFailed  = isFailed;
            _stateChanged();
        }
    }

    // The only way to change the retained frame reference. On replacement the old and the new entry are
    // unsubscribed and subscribed as a pair, each one only where it is not also the current entry.
    // Never assign _retainedItem directly in Recompute, as that would break the pairing of the subscriptions.
    private void SetRetained(ImagePreviewEntry? entry)
    {
        if (ReferenceEquals(_retainedItem, entry))
        {
            return;
        }
        if (_retainedItem is not null && !ReferenceEquals(_retainedItem, _currentItem))
        {
            _retainedItem.PropertyChanged -= HandleEntryPropertyChanged;
        }
        _retainedItem = entry;
        if (_retainedItem is not null && !ReferenceEquals(_retainedItem, _currentItem))
        {
            _retainedItem.PropertyChanged += HandleEntryPropertyChanged;
        }
    }

    private void HandleEntryPropertyChanged(object? sender, PropertyChangedEventArgs args)
    {
        if (args.PropertyName == nameof(ImagePreviewEntry.FullImage) ||
            args.PropertyName == nameof(ImagePreviewEntry.FullState) ||
            args.PropertyName == nameof(ImagePreviewEntry.IsFullLoading) ||
            args.PropertyName == nameof(ImagePreviewEntry.IsFullFailed))
        {
            Recompute();
        }
    }

    public void Dispose()
    {
        if (_currentItem is not null)
        {
            _currentItem.PropertyChanged -= HandleEntryPropertyChanged;
            _currentItem = null;
        }
        SetRetained(null);
    }
}
