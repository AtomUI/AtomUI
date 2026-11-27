using System.ComponentModel;
using AtomUI.Controls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Media;
using Avalonia.Metadata;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Image previewer that shows a single cover while closed and opens the full preview of
/// <see cref="AbstractImagePreviewer.ItemsSource" />.
/// The cover is the item at <see cref="AbstractImagePreviewer.CoverIndex" />, clamped into the range of the items.
/// The theme sizes the cover with <see cref="AbstractImagePreviewer.CoverWidth" />
/// and <see cref="AbstractImagePreviewer.CoverHeight" />.
/// </summary>
public partial class ImagePreviewer : AbstractImagePreviewer
{
    #region 公共属性定义

    /// <summary>
    /// Identifies the <see cref="CoverIndicatorContent" /> property.
    /// </summary>
    public static readonly StyledProperty<object?> CoverIndicatorContentProperty =
        AvaloniaProperty.Register<ImagePreviewer, object?>(nameof(CoverIndicatorContent));

    /// <summary>
    /// Identifies the <see cref="CoverIndicatorContentTemplate" /> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> CoverIndicatorContentTemplateProperty =
        AvaloniaProperty.Register<ImagePreviewer, IDataTemplate?>(nameof(CoverIndicatorContentTemplate));

    /// <summary>
    /// Identifies the <see cref="IsShowCoverMask" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsShowCoverMaskProperty =
        AvaloniaProperty.Register<ImagePreviewer, bool>(nameof(IsShowCoverMask), true);

    /// <summary>
    /// Identifies the <see cref="CoverLoadState" /> property.
    /// </summary>
    public static readonly DirectProperty<ImagePreviewer, ImageLoadState> CoverLoadStateProperty =
        AvaloniaProperty.RegisterDirect<ImagePreviewer, ImageLoadState>(
            nameof(CoverLoadState),
            control => control.CoverLoadState);

    /// <summary>
    /// Identifies the <see cref="CoverLoadError" /> property.
    /// </summary>
    public static readonly DirectProperty<ImagePreviewer, ImageLoadError?> CoverLoadErrorProperty =
        AvaloniaProperty.RegisterDirect<ImagePreviewer, ImageLoadError?>(
            nameof(CoverLoadError),
            control => control.CoverLoadError);

    /// <summary>
    /// Identifies the <see cref="CoverLoadProgress" /> property.
    /// </summary>
    public static readonly DirectProperty<ImagePreviewer, ImageLoadProgress?> CoverLoadProgressProperty =
        AvaloniaProperty.RegisterDirect<ImagePreviewer, ImageLoadProgress?>(
            nameof(CoverLoadProgress),
            control => control.CoverLoadProgress);

    /// <summary>
    /// Identifies the <see cref="IsCoverLoading" /> property.
    /// </summary>
    public static readonly DirectProperty<ImagePreviewer, bool> IsCoverLoadingProperty =
        AvaloniaProperty.RegisterDirect<ImagePreviewer, bool>(
            nameof(IsCoverLoading),
            control => control.IsCoverLoading);

    /// <summary>
    /// Identifies the <see cref="IsCoverLoaded" /> property.
    /// </summary>
    public static readonly DirectProperty<ImagePreviewer, bool> IsCoverLoadedProperty =
        AvaloniaProperty.RegisterDirect<ImagePreviewer, bool>(
            nameof(IsCoverLoaded),
            control => control.IsCoverLoaded);

    /// <summary>
    /// Identifies the <see cref="IsCoverFailed" /> property.
    /// </summary>
    public static readonly DirectProperty<ImagePreviewer, bool> IsCoverFailedProperty =
        AvaloniaProperty.RegisterDirect<ImagePreviewer, bool>(
            nameof(IsCoverFailed),
            control => control.IsCoverFailed);

    /// <summary>
    /// Data that <see cref="CoverIndicatorContentTemplate" /> builds the cover indicator from.
    /// The indicator is drawn on the cover mask, which appears on pointer over and only while
    /// <see cref="IsShowCoverMask" /> is <see langword="true" />.
    /// The default is <see langword="null" />. The theme's default template does not use this value.
    /// </summary>
    [DependsOn(nameof(CoverIndicatorContentTemplate))]
    public object? CoverIndicatorContent
    {
        get => GetValue(CoverIndicatorContentProperty);
        set => SetValue(CoverIndicatorContentProperty, value);
    }

    /// <summary>
    /// Template that builds the cover indicator from <see cref="CoverIndicatorContent" />.
    /// With no local value, including after the value is cleared, the theme supplies its default template,
    /// which shows an eye icon and the preview label.
    /// </summary>
    public IDataTemplate? CoverIndicatorContentTemplate
    {
        get => GetValue(CoverIndicatorContentTemplateProperty);
        set => SetValue(CoverIndicatorContentTemplateProperty, value);
    }

    /// <summary>
    /// Whether the cover has its mask, the layer that carries the cover indicator.
    /// The mask is transparent at rest and appears while the pointer is over the cover, with a hand cursor.
    /// The default is <see langword="true" />. The mask is independent of the loading, image and failure states.
    /// </summary>
    public bool IsShowCoverMask
    {
        get => GetValue(IsShowCoverMaskProperty);
        set => SetValue(IsShowCoverMaskProperty, value);
    }

    private ImageLoadState _coverLoadState;

    /// <summary>
    /// Load state of the thumbnail of the current cover. It is <see cref="ImageLoadState.Idle" /> when there is no
    /// cover entry.
    /// </summary>
    public ImageLoadState CoverLoadState => _coverLoadState;

    private ImageLoadError? _coverLoadError;

    /// <summary>
    /// Error of the thumbnail load of the current cover, or <see langword="null" /> when the cover has no
    /// load error.
    /// </summary>
    public ImageLoadError? CoverLoadError => _coverLoadError;

    private ImageLoadProgress? _coverLoadProgress;

    /// <summary>
    /// Progress of the thumbnail load of the current cover, or <see langword="null" /> when the cover reports no
    /// progress.
    /// </summary>
    public ImageLoadProgress? CoverLoadProgress => _coverLoadProgress;

    private bool _isCoverLoading;

    /// <summary>
    /// Whether the thumbnail of the cover is loading, or is still missing while its load has not failed.
    /// It is <see langword="false" /> when there is no cover entry. While it is <see langword="true" /> and no
    /// image is available, the cover shows its loading placeholder.
    /// </summary>
    public bool IsCoverLoading => _isCoverLoading;

    private bool _isCoverLoaded;

    /// <summary>
    /// Whether the thumbnail of the current cover has been loaded.
    /// </summary>
    public bool IsCoverLoaded => _isCoverLoaded;

    private bool _isCoverFailed;

    /// <summary>
    /// Whether the thumbnail load of the current cover has failed.
    /// </summary>
    public bool IsCoverFailed => _isCoverFailed;

    #endregion

    #region 内部属性定义

    internal static readonly DirectProperty<ImagePreviewer, IImage?> EffectiveCoverImageProperty =
        AvaloniaProperty.RegisterDirect<ImagePreviewer, IImage?>(
            nameof(EffectiveCoverImage),
            control => control.EffectiveCoverImage);

    private IImage? _effectiveCoverImage;

    internal IImage? EffectiveCoverImage => _effectiveCoverImage;

    #endregion

    private ImagePreviewEntry? _coverEntry;

    private ImagePreviewEntry? _retainedCoverEntry;

    /// <summary>
    /// Requests the thumbnail of the current cover entry again at high priority, with the decode size that follows
    /// from the current cover size. The request forces a new load even when one is already running for that size.
    /// Does nothing while there is no cover entry.
    /// </summary>
    public void ReloadCover()
    {
        if (_coverEntry is null)
        {
            return;
        }
        var (width, height) = GetCoverDecodeSize();
        RequestThumbnailLoad(
            _coverEntry,
            width,
            height,
            ImageRequestPriority.High,
            reload: true);
    }

    /// <summary>
    /// Called when a property value changes. Resolves the cover entry again when the effective items or
    /// <see cref="AbstractImagePreviewer.CoverIndex" /> change, and recomputes the cover state when
    /// <see cref="AbstractImagePreviewer.ImageSwitchMode" /> changes. Overrides must call the base member.
    /// </summary>
    /// <param name="change">
    /// Describes the property that changed and its old and new value.
    /// </param>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == EffectiveItemsProperty ||
            change.Property == CoverIndexProperty)
        {
            ConfigureCoverEntry();
        }
        else if (change.Property == ImageSwitchModeProperty)
        {
            UpdateCoverState();
        }
    }

    /// <summary>
    /// Resolves the cover entry again: the entry at <see cref="AbstractImagePreviewer.CoverIndex" /> clamped
    /// into the range of the new entries, or no entry when there are none. The cover state is updated and the
    /// thumbnail of the cover is requested when its decode size is not zero on both axes.
    /// </summary>
    private protected override void OnEffectiveItemsChanged()
    {
        ConfigureCoverEntry();
    }

    /// <summary>
    /// Resolves the cover entry again and requests its thumbnail at high priority, with the decode size that
    /// follows from the cover size, the control size or the arranged bounds. It requests nothing when there
    /// is no cover entry or when that size is zero on both axes.
    /// </summary>
    private protected override void RequestClosedStateLoads()
    {
        ConfigureCoverEntry();
        RequestCoverLoadIfNeeded();
    }

    /// <summary>
    /// Arranges the control and then requests the thumbnail of the cover entry. The decode size of that request is
    /// computed from the arranged bounds, so the request runs after the base arrangement.
    /// </summary>
    /// <param name="finalSize">
    /// The size that the parent assigns to the control.
    /// </param>
    /// <returns>
    /// The size the control takes after the arrangement.
    /// </returns>
    protected override Size ArrangeOverride(Size finalSize)
    {
        var arranged = base.ArrangeOverride(finalSize);
        RequestCoverLoadIfNeeded();
        return arranged;
    }

    /// <summary>
    /// Clears the cover entry and the retained cover entry before the control is detached, which removes the property
    /// change subscriptions of both. Overrides must call the base member.
    /// </summary>
    /// <param name="e">
    /// Identifies the visual tree parent that the control is detached from.
    /// </param>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        SetCoverEntry(null);
        SetRetainedCoverEntry(null);
        base.OnDetachedFromVisualTree(e);
    }

    private void ConfigureCoverEntry()
    {
        var entry = EffectiveItems is { Count: > 0 } entries
            ? entries[ClampIndex(CoverIndex, entries.Count)]
            : null;
        SetCoverEntry(entry);
    }

    private void SetCoverEntry(ImagePreviewEntry? entry)
    {
        if (ReferenceEquals(_coverEntry, entry))
        {
            UpdateCoverState();
            RequestCoverLoadIfNeeded();
            return;
        }
        // Subscription invariant: an entry is subscribed to if and only if it is the cover or the retained cover.
        var previous = _coverEntry;
        _coverEntry = entry;
        if (previous is not null && !ReferenceEquals(previous, _retainedCoverEntry))
        {
            previous.PropertyChanged -= HandleCoverEntryPropertyChanged;
        }
        if (_coverEntry is not null && !ReferenceEquals(_coverEntry, _retainedCoverEntry))
        {
            _coverEntry.PropertyChanged += HandleCoverEntryPropertyChanged;
        }
        UpdateCoverState();
        RequestCoverLoadIfNeeded();
    }

    // The only place that changes the retained cover entry. On replacement it unsubscribes the old source and
    // subscribes to the new one, skipping the source that is the cover entry.
    private void SetRetainedCoverEntry(ImagePreviewEntry? entry)
    {
        if (ReferenceEquals(_retainedCoverEntry, entry))
        {
            return;
        }
        if (_retainedCoverEntry is not null && !ReferenceEquals(_retainedCoverEntry, _coverEntry))
        {
            _retainedCoverEntry.PropertyChanged -= HandleCoverEntryPropertyChanged;
        }
        _retainedCoverEntry = entry;
        if (_retainedCoverEntry is not null && !ReferenceEquals(_retainedCoverEntry, _coverEntry))
        {
            _retainedCoverEntry.PropertyChanged += HandleCoverEntryPropertyChanged;
        }
    }

    private void RequestCoverLoadIfNeeded()
    {
        if (_coverEntry is null)
        {
            return;
        }
        var (width, height) = GetCoverDecodeSize();
        if (width == 0 && height == 0)
        {
            return;
        }
        RequestThumbnailLoad(_coverEntry, width, height, ImageRequestPriority.High);
    }

    private void HandleCoverEntryPropertyChanged(object? sender, PropertyChangedEventArgs args)
    {
        if (args.PropertyName is nameof(ImagePreviewEntry.ThumbnailImage) or
            nameof(ImagePreviewEntry.ThumbnailState) or
            nameof(ImagePreviewEntry.ThumbnailError) or
            nameof(ImagePreviewEntry.ThumbnailProgress))
        {
            UpdateCoverState();
        }
    }

    private void UpdateCoverState()
    {
        if (_retainedCoverEntry is not null &&
            (_coverEntry is null || _retainedCoverEntry.ThumbnailImage is null))
        {
            // Drop the retained cover when there is no cover entry, or when the retained source has unloaded, been
            // released or failed to commit. SetRetainedCoverEntry(null) removes the subscription with it.
            SetRetainedCoverEntry(null);
        }

        var state = _coverEntry?.ThumbnailState ?? ImageLoadState.Idle;
        IImage? displayImage = _coverEntry?.ThumbnailImage;
        if (displayImage is not null)
        {
            SetRetainedCoverEntry(
                ImageSwitchMode == ImageSwitchMode.WaitForLoaded ? _coverEntry : null);
        }
        else if (_coverEntry is { ThumbnailImage: null, IsThumbnailFailed: false } &&
                 ImageSwitchMode == ImageSwitchMode.WaitForLoaded)
        {
            // WaitForLoaded keeps the previous cover on display while the target has no image yet.
            displayImage = _retainedCoverEntry?.ThumbnailImage;
        }
        else
        {
            // Immediate clears the old image and enters the loading placeholder from the Idle state at the moment
            // of the switch.
            SetRetainedCoverEntry(null);
        }

        SetAndRaise(EffectiveCoverImageProperty, ref _effectiveCoverImage, displayImage);
        SetAndRaise(CoverLoadStateProperty, ref _coverLoadState, state);
        SetAndRaise(CoverLoadErrorProperty, ref _coverLoadError, _coverEntry?.ThumbnailError);
        SetAndRaise(CoverLoadProgressProperty, ref _coverLoadProgress, _coverEntry?.ThumbnailProgress);
        var isLoading = state == ImageLoadState.Loading ||
                        _coverEntry is { ThumbnailImage: null, IsThumbnailFailed: false };
        SetAndRaise(IsCoverLoadingProperty, ref _isCoverLoading, isLoading);
        SetAndRaise(IsCoverLoadedProperty, ref _isCoverLoaded, state == ImageLoadState.Loaded);
        SetAndRaise(IsCoverFailedProperty, ref _isCoverFailed, state == ImageLoadState.Failed);
    }

    private (int Width, int Height) GetCoverDecodeSize()
    {
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel is null)
        {
            return (0, 0);
        }

        var hasStableWidth = IsFinitePositive(CoverWidth) || IsFinitePositive(Width);
        var hasStableHeight = IsFinitePositive(CoverHeight) || IsFinitePositive(Height);
        var logicalWidth = hasStableWidth ? ResolveStableAxis(CoverWidth, Width, Bounds.Width) : Bounds.Width;
        var logicalHeight = hasStableHeight ? ResolveStableAxis(CoverHeight, Height, Bounds.Height) : Bounds.Height;

        if (!hasStableWidth && !hasStableHeight)
        {
            if (logicalWidth >= logicalHeight)
            {
                logicalHeight = 0;
            }
            else
            {
                logicalWidth = 0;
            }
        }
        else
        {
            if (!hasStableWidth)
            {
                logicalWidth = 0;
            }
            if (!hasStableHeight)
            {
                logicalHeight = 0;
            }
        }

        var scaling = topLevel.RenderScaling;
        return (Quantize(logicalWidth * scaling), Quantize(logicalHeight * scaling));
    }

    private static double ResolveStableAxis(double coverValue, double controlValue, double boundsValue)
    {
        if (IsFinitePositive(coverValue))
        {
            return coverValue;
        }
        if (IsFinitePositive(controlValue))
        {
            return controlValue;
        }
        return boundsValue;
    }

    private static bool IsFinitePositive(double value)
    {
        return double.IsFinite(value) && value > 0;
    }

    private static int Quantize(double value)
    {
        if (!double.IsFinite(value) || value <= 0)
        {
            return 0;
        }
        return checked((int)(Math.Ceiling(value / 16) * 16));
    }
}
