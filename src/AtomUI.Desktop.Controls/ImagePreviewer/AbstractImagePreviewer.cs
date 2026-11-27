using System.ComponentModel;
using System.Collections.Specialized;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;
using System.Reactive.Linq;
using AtomUI.Controls;
using AtomUI.Data;
using AtomUI.Theme;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Metadata;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Base class of controls that show the images of <see cref="ItemsSource" /> and open a full preview of them.
/// The preview opens in a <see cref="ImagePreviewerDialog" /> window, or in an overlay of the current window
/// when the platform has no native windows. <see cref="ImagePreviewer" /> shows a single cover while closed,
/// <see cref="ImageGroupPreviewer" /> shows every image as a cover.
/// </summary>
public abstract class AbstractImagePreviewer : TemplatedControl,
                                               IMotionAwareControl,
                                               ISemanticPartCrossRootProvider
{
    #region 公共属性定义

    /// <summary>
    /// Identifies the <see cref="ItemsSource" /> property.
    /// </summary>
    public static readonly StyledProperty<IEnumerable<ImagePreviewItem>?> ItemsSourceProperty =
        AvaloniaProperty.Register<AbstractImagePreviewer, IEnumerable<ImagePreviewItem>?>(nameof(ItemsSource));

    /// <summary>
    /// Identifies the <see cref="PreviewTitle" /> property.
    /// </summary>
    public static readonly StyledProperty<string?> PreviewTitleProperty =
        AvaloniaProperty.Register<AbstractImagePreviewer, string?>(nameof(PreviewTitle));

    /// <summary>
    /// Identifies the <see cref="PreviewTitleIcon" /> property.
    /// </summary>
    public static readonly StyledProperty<PathIcon?> PreviewTitleIconProperty =
        AvaloniaProperty.Register<AbstractImagePreviewer, PathIcon?>(nameof(PreviewTitleIcon));

    /// <summary>
    /// Identifies the <see cref="PreviewTitleResolver" /> property.
    /// </summary>
    public static readonly StyledProperty<IImagePreviewTitleResolver?> PreviewTitleResolverProperty =
        AvaloniaProperty.Register<AbstractImagePreviewer, IImagePreviewTitleResolver?>(
            nameof(PreviewTitleResolver),
            DefaultImagePreviewTitleResolver.Instance);

    /// <summary>
    /// Identifies the <see cref="LoadingContent" /> property.
    /// </summary>
    public static readonly StyledProperty<object?> LoadingContentProperty =
        AvaloniaProperty.Register<AbstractImagePreviewer, object?>(nameof(LoadingContent));

    /// <summary>
    /// Identifies the <see cref="LoadingContentTemplate" /> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> LoadingContentTemplateProperty =
        AvaloniaProperty.Register<AbstractImagePreviewer, IDataTemplate?>(nameof(LoadingContentTemplate));

    /// <summary>
    /// Identifies the <see cref="ErrorContent" /> property.
    /// </summary>
    public static readonly StyledProperty<object?> ErrorContentProperty =
        AvaloniaProperty.Register<AbstractImagePreviewer, object?>(nameof(ErrorContent));

    /// <summary>
    /// Identifies the <see cref="ErrorContentTemplate" /> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> ErrorContentTemplateProperty =
        AvaloniaProperty.Register<AbstractImagePreviewer, IDataTemplate?>(nameof(ErrorContentTemplate));

    /// <summary>
    /// Identifies the <see cref="IsOpen" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsOpenProperty =
        AvaloniaProperty.Register<AbstractImagePreviewer, bool>(
            nameof(IsOpen),
            defaultBindingMode: BindingMode.TwoWay);

    /// <summary>
    /// Identifies the <see cref="IsMotionEnabled" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<AbstractImagePreviewer>();

    /// <summary>
    /// Identifies the <see cref="CoverWidth" /> property.
    /// </summary>
    public static readonly StyledProperty<double> CoverWidthProperty =
        AvaloniaProperty.Register<AbstractImagePreviewer, double>(nameof(CoverWidth), double.NaN);

    /// <summary>
    /// Identifies the <see cref="CoverHeight" /> property.
    /// </summary>
    public static readonly StyledProperty<double> CoverHeightProperty =
        AvaloniaProperty.Register<AbstractImagePreviewer, double>(nameof(CoverHeight), double.NaN);

    /// <summary>
    /// Identifies the <see cref="CurrentIndex" /> property.
    /// </summary>
    public static readonly StyledProperty<int> CurrentIndexProperty =
        AvaloniaProperty.Register<AbstractImagePreviewer, int>(
            nameof(CurrentIndex),
            0,
            defaultBindingMode: BindingMode.TwoWay);

    /// <summary>
    /// Identifies the <see cref="CoverIndex" /> property.
    /// </summary>
    public static readonly StyledProperty<int> CoverIndexProperty =
        AvaloniaProperty.Register<AbstractImagePreviewer, int>(
            nameof(CoverIndex),
            0,
            coerce: CoerceNonNegativeValue);

    /// <summary>
    /// Identifies the <see cref="PreloadCount" /> property.
    /// </summary>
    public static readonly StyledProperty<int> PreloadCountProperty =
        AvaloniaProperty.Register<AbstractImagePreviewer, int>(
            nameof(PreloadCount),
            1,
            coerce: CoerceNonNegativeValue);

    /// <summary>
    /// Identifies the <see cref="ImageSwitchMode" /> property.
    /// </summary>
    public static readonly StyledProperty<ImageSwitchMode> ImageSwitchModeProperty =
        AvaloniaProperty.Register<AbstractImagePreviewer, ImageSwitchMode>(
            nameof(ImageSwitchMode),
            ImageSwitchMode.Immediate);

    /// <summary>
    /// Identifies the <see cref="CurrentItem" /> property.
    /// </summary>
    public static readonly DirectProperty<AbstractImagePreviewer, ImagePreviewItem?> CurrentItemProperty =
        AvaloniaProperty.RegisterDirect<AbstractImagePreviewer, ImagePreviewItem?>(
            nameof(CurrentItem),
            control => control.CurrentItem);

    /// <summary>
    /// Identifies the <see cref="CurrentLoadState" /> property.
    /// </summary>
    public static readonly DirectProperty<AbstractImagePreviewer, ImageLoadState> CurrentLoadStateProperty =
        AvaloniaProperty.RegisterDirect<AbstractImagePreviewer, ImageLoadState>(
            nameof(CurrentLoadState),
            control => control.CurrentLoadState);

    /// <summary>
    /// Identifies the <see cref="CurrentLoadError" /> property.
    /// </summary>
    public static readonly DirectProperty<AbstractImagePreviewer, ImageLoadError?> CurrentLoadErrorProperty =
        AvaloniaProperty.RegisterDirect<AbstractImagePreviewer, ImageLoadError?>(
            nameof(CurrentLoadError),
            control => control.CurrentLoadError);

    /// <summary>
    /// Identifies the <see cref="CurrentLoadProgress" /> property.
    /// </summary>
    public static readonly DirectProperty<AbstractImagePreviewer, ImageLoadProgress?> CurrentLoadProgressProperty =
        AvaloniaProperty.RegisterDirect<AbstractImagePreviewer, ImageLoadProgress?>(
            nameof(CurrentLoadProgress),
            control => control.CurrentLoadProgress);

    /// <summary>
    /// Identifies the <see cref="IsCurrentLoading" /> property.
    /// </summary>
    public static readonly DirectProperty<AbstractImagePreviewer, bool> IsCurrentLoadingProperty =
        AvaloniaProperty.RegisterDirect<AbstractImagePreviewer, bool>(
            nameof(IsCurrentLoading),
            control => control.IsCurrentLoading);

    /// <summary>
    /// Identifies the <see cref="IsCurrentLoaded" /> property.
    /// </summary>
    public static readonly DirectProperty<AbstractImagePreviewer, bool> IsCurrentLoadedProperty =
        AvaloniaProperty.RegisterDirect<AbstractImagePreviewer, bool>(
            nameof(IsCurrentLoaded),
            control => control.IsCurrentLoaded);

    /// <summary>
    /// Identifies the <see cref="IsCurrentFailed" /> property.
    /// </summary>
    public static readonly DirectProperty<AbstractImagePreviewer, bool> IsCurrentFailedProperty =
        AvaloniaProperty.RegisterDirect<AbstractImagePreviewer, bool>(
            nameof(IsCurrentFailed),
            control => control.IsCurrentFailed);

    /// <summary>
    /// Identifies the <see cref="IsImageMovable" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsImageMovableProperty =
        AvaloniaProperty.Register<AbstractImagePreviewer, bool>(nameof(IsImageMovable), true);

    /// <summary>
    /// Identifies the <see cref="ImageScaleStep" /> property.
    /// </summary>
    public static readonly StyledProperty<double> ImageScaleStepProperty =
        AvaloniaProperty.Register<AbstractImagePreviewer, double>(nameof(ImageScaleStep), 0.5);

    /// <summary>
    /// Identifies the <see cref="ImageMinScale" /> property.
    /// </summary>
    public static readonly StyledProperty<double> ImageMinScaleProperty =
        AvaloniaProperty.Register<AbstractImagePreviewer, double>(nameof(ImageMinScale), 1.0);

    /// <summary>
    /// Identifies the <see cref="ImageMaxScale" /> property.
    /// </summary>
    public static readonly StyledProperty<double> ImageMaxScaleProperty =
        AvaloniaProperty.Register<AbstractImagePreviewer, double>(nameof(ImageMaxScale), 50.0);

    /// <summary>
    /// Identifies the <see cref="IsWheelZoomEnabled" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsWheelZoomEnabledProperty =
        AvaloniaProperty.Register<AbstractImagePreviewer, bool>(nameof(IsWheelZoomEnabled), true);

    /// <summary>
    /// Identifies the <see cref="IsDialogModal" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsDialogModalProperty =
        AvaloniaProperty.Register<AbstractImagePreviewer, bool>(nameof (IsDialogModal), false);

    /// <summary>
    /// Identifies the <see cref="IsDialogTopmost" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsDialogTopmostProperty =
        AvaloniaProperty.Register<AbstractImagePreviewer, bool>(nameof(IsDialogTopmost));

    /// <summary>
    /// Gets or sets the images to preview. Null entries are skipped. When the control is attached, a source that
    /// implements <see cref="INotifyCollectionChanged" /> is followed: add, remove, replace and single-item move
    /// notifications update the covers, and the loaded images of items that stay are kept. <see langword="null" />
    /// or an empty collection leaves nothing to preview, and the preview does not open.
    /// </summary>
    public IEnumerable<ImagePreviewItem>? ItemsSource
    {
        get => GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    /// <summary>
    /// Gets or sets the fixed title of the native preview window. A non-blank value wins over the title of the
    /// current item and over <see cref="PreviewTitleResolver" />. When it is <see langword="null" /> or blank, the
    /// title comes from the current item. The overlay host has no title and ignores this value.
    /// </summary>
    public string? PreviewTitle
    {
        get => GetValue(PreviewTitleProperty);
        set => SetValue(PreviewTitleProperty, value);
    }

    /// <summary>
    /// Gets or sets the icon of the native preview window's title, relayed to
    /// <see cref="ImagePreviewerDialog.TitleIcon" />. The overlay host has no title and ignores this value.
    /// </summary>
    public PathIcon? PreviewTitleIcon
    {
        get => GetValue(PreviewTitleIconProperty);
        set => SetValue(PreviewTitleIconProperty, value);
    }

    /// <summary>
    /// Gets or sets the object that derives the title of the native preview window from the current item. It is
    /// consulted when <see cref="PreviewTitle" /> is blank and the item has no title of its own. The default is
    /// <see cref="DefaultImagePreviewTitleResolver.Instance" />; <see langword="null" /> turns the derivation off.
    /// </summary>
    public IImagePreviewTitleResolver? PreviewTitleResolver
    {
        get => GetValue(PreviewTitleResolverProperty);
        set => SetValue(PreviewTitleResolverProperty, value);
    }

    /// <summary>
    /// Gets or sets the content shown in place of the image while the image is loading, on the cover and in the open
    /// preview. <see langword="null" /> shows the built-in loading indicator.
    /// </summary>
    [DependsOn(nameof(LoadingContentTemplate))]
    public object? LoadingContent
    {
        get => GetValue(LoadingContentProperty);
        set => SetValue(LoadingContentProperty, value);
    }

    /// <summary>
    /// Gets or sets the template that presents <see cref="LoadingContent" />. When <see langword="null" />, the
    /// content is presented by the default DataTemplate lookup.
    /// </summary>
    public IDataTemplate? LoadingContentTemplate
    {
        get => GetValue(LoadingContentTemplateProperty);
        set => SetValue(LoadingContentTemplateProperty, value);
    }

    /// <summary>
    /// Gets or sets the content shown in place of the image when the image failed to load, on the cover and in the
    /// open preview. <see langword="null" /> shows the built-in error indicator.
    /// </summary>
    [DependsOn(nameof(ErrorContentTemplate))]
    public object? ErrorContent
    {
        get => GetValue(ErrorContentProperty);
        set => SetValue(ErrorContentProperty, value);
    }

    /// <summary>
    /// Gets or sets the template that presents <see cref="ErrorContent" />. When <see langword="null" />, the
    /// content is presented by the default DataTemplate lookup.
    /// </summary>
    public IDataTemplate? ErrorContentTemplate
    {
        get => GetValue(ErrorContentTemplateProperty);
        set => SetValue(ErrorContentTemplateProperty, value);
    }

    /// <summary>
    /// Gets or sets whether the preview is open. Setting <see langword="true" /> opens it and setting
    /// <see langword="false" /> closes it. The control updates the value itself while opening and closing; the
    /// default binding mode is two way, so a binding follows the actual state. It is <see langword="true" /> once
    /// the preview is shown, and stays so when <see cref="DialogClosing" /> cancels the close.
    /// </summary>
    public bool IsOpen
    {
        get => GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    /// <summary>
    /// Gets or sets whether the open preview animates. The value is relayed to the preview host, which uses it for
    /// the transitions of the image viewer and the preview title bar. Without a local value the theme decides:
    /// <see cref="ImageGroupPreviewer" /> takes it from its <c>EnableMotion</c> token, <see cref="ImagePreviewer" />
    /// from <c>ImageViewer.DefaultMotionEnabled</c>.
    /// </summary>
    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }

    /// <summary>
    /// Gets or sets the width of the cover shown while the preview is closed, in logical pixels. It also determines
    /// the width the cover bitmap is decoded at. <see cref="double.NaN" /> (the default) lets the cover take its
    /// size from the control. <see cref="ImageGroupPreviewer" /> takes the value from
    /// <c>ImageViewer.CoverImageSize</c> when it is not set.
    /// </summary>
    public double CoverWidth
    {
        get => GetValue(CoverWidthProperty);
        set => SetValue(CoverWidthProperty, value);
    }

    /// <summary>
    /// Gets or sets the height of the cover shown while the preview is closed, in logical pixels. It also determines
    /// the height the cover bitmap is decoded at. <see cref="double.NaN" /> (the default) lets the cover take its
    /// size from the control.
    /// </summary>
    public double CoverHeight
    {
        get => GetValue(CoverHeightProperty);
        set => SetValue(CoverHeightProperty, value);
    }

    /// <summary>
    /// Gets or sets the zero-based index in <see cref="ItemsSource" /> of the item shown in the preview, counting
    /// only non-null entries. It is bound both ways to the open preview, so navigating inside the preview updates
    /// it. Values outside the range are clamped where the control uses the index; the stored value is not changed.
    /// </summary>
    public int CurrentIndex
    {
        get => GetValue(CurrentIndexProperty);
        set => SetValue(CurrentIndexProperty, value);
    }

    /// <summary>
    /// Gets or sets the zero-based index in <see cref="ItemsSource" /> of the item shown on the cover while the
    /// preview is closed, counting only non-null entries. Negative values are treated as 0 and values beyond the
    /// last item are clamped when the cover item is determined. Only <see cref="ImagePreviewer" /> reads it;
    /// <see cref="ImageGroupPreviewer" /> shows every item.
    /// </summary>
    public int CoverIndex
    {
        get => GetValue(CoverIndexProperty);
        set => SetValue(CoverIndexProperty, value);
    }

    /// <summary>
    /// Gets or sets how many items on each side of <see cref="CurrentIndex" /> are loaded ahead while the preview is
    /// open. 0 loads only the current item, and negative values are treated as 0. Pending full-image loads of the
    /// other items are canceled.
    /// </summary>
    public int PreloadCount
    {
        get => GetValue(PreloadCountProperty);
        set => SetValue(PreloadCountProperty, value);
    }

    /// <summary>
    /// Gets or sets how the display changes when the current item changes. <c>Immediate</c> clears the old image at
    /// once and shows the loading placeholder. <c>WaitForLoaded</c> keeps the previous image until the image of the
    /// new item is available. It applies to the cover and to the open preview, and a change takes effect at once.
    /// </summary>
    public ImageSwitchMode ImageSwitchMode
    {
        get => GetValue(ImageSwitchModeProperty);
        set => SetValue(ImageSwitchModeProperty, value);
    }

    private ImagePreviewItem? _currentItem;

    /// <summary>
    /// Gets the item at <see cref="CurrentIndex" />, or <see langword="null" /> when there is no current item.
    /// </summary>
    public ImagePreviewItem? CurrentItem => _currentItem;

    private ImageLoadState _currentLoadState;

    /// <summary>
    /// Gets the load state of the full image of <see cref="CurrentItem" />. It is <see cref="ImageLoadState.Idle" />
    /// when there is no current item.
    /// </summary>
    public ImageLoadState CurrentLoadState => _currentLoadState;

    private ImageLoadError? _currentLoadError;

    /// <summary>
    /// Gets the error of the full-image load of <see cref="CurrentItem" /> when that load failed, or
    /// <see langword="null" /> when it did not fail or there is no current item.
    /// </summary>
    public ImageLoadError? CurrentLoadError => _currentLoadError;

    private ImageLoadProgress? _currentLoadProgress;

    /// <summary>
    /// Gets the progress reported by the full-image load of <see cref="CurrentItem" />, or <see langword="null" />
    /// when none was reported or there is no current item.
    /// </summary>
    public ImageLoadProgress? CurrentLoadProgress => _currentLoadProgress;

    private bool _isCurrentLoading;

    /// <summary>
    /// Gets a value indicating whether the full image of <see cref="CurrentItem" /> is loading.
    /// </summary>
    public bool IsCurrentLoading => _isCurrentLoading;

    private bool _isCurrentLoaded;

    /// <summary>
    /// Gets a value indicating whether the full image of <see cref="CurrentItem" /> is loaded.
    /// </summary>
    public bool IsCurrentLoaded => _isCurrentLoaded;

    private bool _isCurrentFailed;

    /// <summary>
    /// Gets a value indicating whether the full image of <see cref="CurrentItem" /> failed to load.
    /// </summary>
    public bool IsCurrentFailed => _isCurrentFailed;

    /// <summary>
    /// Gets or sets whether the user can drag the image in the open preview with the left mouse button to move it.
    /// Zooming is not affected.
    /// </summary>
    public bool IsImageMovable
    {
        get => GetValue(IsImageMovableProperty);
        set => SetValue(IsImageMovableProperty, value);
    }

    /// <summary>
    /// Gets or sets the amount by which one zoom step changes the scale of the image in the open preview. The
    /// default is 0.5.
    /// </summary>
    public double ImageScaleStep
    {
        get => GetValue(ImageScaleStepProperty);
        set => SetValue(ImageScaleStepProperty, value);
    }

    /// <summary>
    /// Gets or sets the smallest scale the image in the open preview can have. The default is 1.0.
    /// </summary>
    public double ImageMinScale
    {
        get => GetValue(ImageMinScaleProperty);
        set => SetValue(ImageMinScaleProperty, value);
    }

    /// <summary>
    /// Gets or sets the largest scale the image in the open preview can have. The default is 50.0.
    /// </summary>
    public double ImageMaxScale
    {
        get => GetValue(ImageMaxScaleProperty);
        set => SetValue(ImageMaxScaleProperty, value);
    }

    /// <summary>
    /// Gets or sets whether rotating the mouse wheel over the image zooms it. The default is <see langword="true" />.
    /// </summary>
    public bool IsWheelZoomEnabled
    {
        get => GetValue(IsWheelZoomEnabledProperty);
        set => SetValue(IsWheelZoomEnabledProperty, value);
    }

    /// <summary>
    /// Gets or sets whether the preview opens as a modal dialog that blocks its owner window. The default is
    /// <see langword="false" />, which opens a modeless window. The value is read when the preview opens, and it is
    /// passed on to the overlay host on platforms without native windows.
    /// </summary>
    public bool IsDialogModal
    {
        get => GetValue(IsDialogModalProperty);
        set => SetValue(IsDialogModalProperty, value);
    }

    /// <summary>
    /// Gets or sets whether the preview window stays above other windows. It is read when the preview opens, so a
    /// later change applies to the next opening. It has no effect on the overlay host, which has no window.
    /// </summary>
    public bool IsDialogTopmost
    {
        get => GetValue(IsDialogTopmostProperty);
        set => SetValue(IsDialogTopmostProperty, value);
    }

    #endregion

    #region 公共事件定义

    /// <summary>
    /// Occurs after the preview host has closed and the bindings and handlers of the preview session are released.
    /// It is not raised when <see cref="CloseDialog" /> finds no preview open.
    /// </summary>
    public event EventHandler? DialogClosed;

    /// <summary>
    /// Occurs after the preview is shown and <see cref="IsOpen" /> became <see langword="true" />, right before
    /// <see cref="CrossRootsChanged" />.
    /// </summary>
    public event EventHandler? DialogOpened;

    /// <summary>
    /// Occurs when a close is requested, before anything is closed and while the preview is still open. It is also
    /// raised when <see cref="CloseDialog" /> finds no preview open, so handlers must not expect an open preview.
    /// Setting <see cref="CancelEventArgs.Cancel" /> to <see langword="true" /> aborts the close.
    /// </summary>
    public event EventHandler<CancelEventArgs>? DialogClosing;

    /// <summary>
    /// Occurs when the full image of <see cref="CurrentItem" /> finishes loading. It is raised after
    /// <see cref="CurrentLoadState" /> is updated, and again for an image that is already loaded when its item
    /// becomes the current item.
    /// </summary>
    public event EventHandler<ImagePreviewOpenedEventArgs>? ImageOpened;

    /// <summary>
    /// Occurs when the full image of <see cref="CurrentItem" /> fails to load. It is raised after
    /// <see cref="CurrentLoadState" /> is updated. The arguments carry the item, its index and the error.
    /// </summary>
    public event EventHandler<ImagePreviewFailedEventArgs>? ImageFailed;

    /// <inheritdoc />
    public event EventHandler? CrossRootsChanged;

    #endregion

    #region 内部属性定义

    internal static readonly DirectProperty<AbstractImagePreviewer, IList<ImagePreviewEntry>?> EffectiveItemsProperty =
        AvaloniaProperty.RegisterDirect<AbstractImagePreviewer, IList<ImagePreviewEntry>?>(
            nameof(EffectiveItems),
            o => o.EffectiveItems,
            (o, v) => o.EffectiveItems = v);

    private IList<ImagePreviewEntry>? _effectiveItems;

    internal IList<ImagePreviewEntry>? EffectiveItems
    {
        get => _effectiveItems;
        set => SetAndRaise(EffectiveItemsProperty, ref _effectiveItems, value);
    }

    /// <summary>
    /// The most recent entry whose full image finished loading and is still usable, or <see langword="null" />.
    /// In <c>WaitForLoaded</c> mode it seeds the retained frame. When the user switches faster than images load,
    /// the current entry can be replaced before it completes, so the retained frame must be replenished from
    /// whichever entry completes; otherwise the display falls back to the placeholder and flickers with each
    /// completion.
    /// </summary>
    internal ImagePreviewEntry? LatestLoadedFullEntry =>
        _latestLoadedFullEntry is { IsFullLoaded: true, FullImage: not null }
            ? _latestLoadedFullEntry
            : null;

    #endregion

    #region 内部协作 API

    internal void RequestPreviewLoads()
    {
        if (EffectiveItems is not { Count: > 0 } entries)
        {
            return;
        }
        var currentIndex = ClampIndex(CurrentIndex, entries.Count);
        var active = new HashSet<int> { currentIndex };
        for (var offset = 1; offset <= PreloadCount; offset++)
        {
            if (currentIndex - offset >= 0)
            {
                active.Add(currentIndex - offset);
            }
            if (currentIndex + offset < entries.Count)
            {
                active.Add(currentIndex + offset);
            }
        }
        for (var index = 0; index < entries.Count; index++)
        {
            if (!active.Contains(index))
            {
                entries[index].CancelFullLoad();
            }
        }

        var (width, height) = GetFullDecodeSize();
        var currentSize = _previewDecodeSize ?? new PixelSize(width, height);
        RequestFullLoad(entries[currentIndex], currentSize.Width, currentSize.Height, ImageRequestPriority.Critical);
        foreach (var index in active.Where(index => index != currentIndex).OrderBy(index => Math.Abs(index - currentIndex)))
        {
            RequestFullLoad(entries[index], width, height, ImageRequestPriority.Preload);
        }
    }

    internal void NotifyPreviewDecodeSize(ImagePreviewEntry? entry, Size imageSize, PixelSize size)
    {
        // A reset or retained frame can still describe the previous item while a switch is in flight.
        // Accept geometry only for the current entry and the source dimensions used to calculate it.
        if (!ReferenceEquals(entry, _currentEntry) || entry?.FullImage is null ||
            imageSize != entry.FullImageSize ||
            size.Width <= 0 || size.Height <= 0 || _previewDecodeSize == size)
        {
            return;
        }

        _previewDecodeSize = size;
        if (IsOpen && _isAttachedToVisualTree)
        {
            // Only the current item uses the zoom target; neighbours keep the baseline preload size.
            RequestFullLoad(entry, size.Width, size.Height, ImageRequestPriority.Critical);
        }
    }

    internal void NotifyDialogHostCloseRequest()
    {
        CloseDialog();
    }

    #endregion

    private bool _ignoreIsOpenChanged;

    private IImagePreviewerOpenState? _openState;

    private IDisposable? _modalSubscription;

    private bool _dialogOpening;

    private bool _dialogClosing;

    private INotifyCollectionChanged? _observableItemsSource;

    private bool _isItemsSourceSubscribed;

    private bool _isAttachedToVisualTree;

    private ImagePreviewEntry? _currentEntry;

    private ImagePreviewEntry? _latestLoadedFullEntry;

    private ImageLoadState _lastNotifiedCurrentState;

    private TopLevel? _decodeSizeTopLevel;

    private PixelSize? _previewDecodeSize;

    static AbstractImagePreviewer()
    {
        FocusableProperty.OverrideDefaultValue<AbstractImagePreviewer>(true);
        IsOpenProperty.Changed.AddClassHandler<AbstractImagePreviewer>((x, e) => x.HandleIsOpenChanged(e));
    }

    protected AbstractImagePreviewer()
    {
    }

    /// <summary>
    /// Returns the visual root of the open preview host: the <see cref="ImagePreviewerDialog" /> window, or the
    /// overlay host when the preview is shown inside the same window. The host lies outside the visual tree of this
    /// control, so semantic preview finds it through this contract and refreshes on open, on close and once the
    /// dialog template is applied. The list is empty while no preview is open.
    /// </summary>
    public IReadOnlyList<Visual> GetCrossRoots()
    {
        return _openState?.RootVisual is { } rootVisual
            ? new Visual[] { rootVisual }
            : Array.Empty<Visual>();
    }

    /// <summary>
    /// Called when a styled property of this control changes. It calls the base implementation first, then reacts
    /// to the properties of this class. A change of <see cref="ItemsSource" /> rebuilds the entries. A change of
    /// <see cref="CurrentIndex" /> or <see cref="PreloadCount" /> re-selects the current entry and re-requests the
    /// loads while the preview is open. While the preview is closed, a change of <see cref="CoverWidth" /> or
    /// <see cref="CoverHeight" /> re-requests the loads. A new <see cref="ImageSwitchMode" /> is pushed to the
    /// open host. Overrides must call the base implementation, otherwise these reactions are lost.
    /// </summary>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == ItemsSourceProperty)
        {
            HandleItemsSourceChanged(change.GetOldValue<IEnumerable<ImagePreviewItem>?>());
        }
        else if (change.Property == CurrentIndexProperty ||
                 change.Property == PreloadCountProperty)
        {
            ConfigureCurrentEntry();
            if (IsOpen)
            {
                RequestPreviewLoads();
            }
        }
        else if ((change.Property == CoverWidthProperty ||
                  change.Property == CoverHeightProperty) &&
                 !IsOpen)
        {
            RequestClosedStateLoads();
        }
        else if (change.Property == ImageSwitchModeProperty)
        {
            _openState?.RefreshImageSwitchMode();
        }
    }

    private protected void MaterializeEffectiveItems()
    {
        var sourceItems = ItemsSource?.Where(item => item is not null).ToList() ?? [];
        var reusable = new Dictionary<ImagePreviewItem, Queue<ImagePreviewEntry>>();
        if (EffectiveItems is not null)
        {
            foreach (var entry in EffectiveItems)
            {
                if (!reusable.TryGetValue(entry.Item, out var queue))
                {
                    queue = [];
                    reusable.Add(entry.Item, queue);
                }
                queue.Enqueue(entry);
            }
        }

        var entries = new ObservableCollection<ImagePreviewEntry>();
        var preserved = new HashSet<ImagePreviewEntry>();
        foreach (var item in sourceItems)
        {
            ImagePreviewEntry entry;
            if (reusable.TryGetValue(item, out var queue) && queue.Count > 0)
            {
                entry = queue.Dequeue();
                preserved.Add(entry);
            }
            else
            {
                entry = new ImagePreviewEntry(item);
            }
            entries.Add(entry);
        }
        SetEffectiveItems(entries, preserved);
    }

    /// <summary>
    /// Called after the entries were built from <see cref="ItemsSource" /> or updated by a collection notification,
    /// and before the loads of the current state are requested. The base implementation does nothing.
    /// </summary>
    private protected virtual void OnEffectiveItemsChanged()
    {
    }

    private protected void RequestFullLoad(
        ImagePreviewEntry entry,
        int width,
        int height,
        ImageRequestPriority priority,
        bool reload = false)
    {
        entry.LoadFull(width, height, priority, reload);
    }

    private protected void RequestThumbnailLoad(
        ImagePreviewEntry entry,
        int width,
        int height,
        ImageRequestPriority priority,
        bool reload = false)
    {
        entry.LoadThumbnail(width, height, priority, reload);
    }

    /// <summary>
    /// Requests the image loads of the closed state, in which the covers are shown. The control calls it when it is
    /// loaded, after the preview closed, when the entries or the decode size change while the preview is closed, and
    /// when <see cref="CoverWidth" /> or <see cref="CoverHeight" /> changes while the preview is closed. The base
    /// implementation does nothing; a derived class overrides it to load what its covers need.
    /// </summary>
    private protected virtual void RequestClosedStateLoads()
    {
    }

    /// <summary>
    /// Requests the full image of <see cref="CurrentItem" /> to be loaded again. It is decoded at the size the open
    /// preview needs, or at a size derived from the top level window when the preview has not calculated one. The
    /// call does nothing when there is no current item.
    /// </summary>
    public void ReloadCurrent()
    {
        if (_currentEntry is null)
        {
            return;
        }
        var baseline = GetFullDecodeSize();
        var size = _previewDecodeSize ?? new PixelSize(baseline.Width, baseline.Height);
        RequestFullLoad(_currentEntry, size.Width, size.Height, ImageRequestPriority.Critical, reload: true);
    }

    /// <summary>
    /// Requests the full image of the item at <paramref name="index" /> to be loaded again. The current item is
    /// decoded at the size the open preview needs; any other item at a size derived from the top level window.
    /// </summary>
    /// <param name="index">
    /// Zero-based position in <see cref="ItemsSource" />, counting only non-null entries.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="index" /> is negative or not below the number of items.
    /// </exception>
    public void ReloadItem(int index)
    {
        if (EffectiveItems is null || index < 0 || index >= EffectiveItems.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }
        var baseline = GetFullDecodeSize();
        var isCurrent = index == ClampIndex(CurrentIndex, EffectiveItems.Count);
        var size = isCurrent && _previewDecodeSize is { } previewSize
            ? previewSize
            : new PixelSize(baseline.Width, baseline.Height);
        RequestFullLoad(
            EffectiveItems[index],
            size.Width,
            size.Height,
            isCurrent ? ImageRequestPriority.Critical : ImageRequestPriority.Preload,
            reload: true);
    }

    private protected static int ClampIndex(int index, int count)
    {
        if (count <= 0)
        {
            return 0;
        }
        return Math.Clamp(index, 0, count - 1);
    }

    /// <summary>
    /// Called after the control is loaded. It calls the base implementation, starts following the size and scaling
    /// of the top level, builds the entries when needed, selects the current entry and requests the loads of the
    /// closed state. Overrides must call the base implementation.
    /// </summary>
    protected override void OnLoaded(RoutedEventArgs args)
    {
        base.OnLoaded(args);
        SubscribeDecodeSizeTopLevel();
        if (EffectiveItems is null)
        {
            MaterializeEffectiveItems();
        }
        ConfigureCurrentEntry();
        RequestClosedStateLoads();
    }

    /// <summary>
    /// Called when the control is attached to the visual tree. It calls the base implementation, then starts
    /// following the size and scaling of the top level and the changes of <see cref="ItemsSource" />, and builds the
    /// entries. Overrides must call the base implementation, otherwise no image is ever loaded.
    /// </summary>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _isAttachedToVisualTree = true;
        SubscribeDecodeSizeTopLevel();
        SubscribeItemsSource();
        MaterializeEffectiveItems();
    }

    /// <summary>
    /// Called when the control is detached from the visual tree. It stops the subscriptions, cancels the pending
    /// loads and releases the decoded images, so they are loaded again on the next attach, then calls the base
    /// implementation. Overrides must call the base implementation to keep that release.
    /// </summary>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _isAttachedToVisualTree = false;
        UnsubscribeDecodeSizeTopLevel();
        UnsubscribeItemsSource();
        CancelImageLoads(releaseLeases: true);
        base.OnDetachedFromVisualTree(e);
    }

    /// <summary>
    /// Called when a pointer is released over the control. It calls the base implementation and then opens the
    /// preview, whatever the button.
    /// </summary>
    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        OpenDialog();
    }

    /// <summary>
    /// Opens the preview of <see cref="ItemsSource" />, starting at <see cref="CurrentIndex" />, and raises
    /// <see cref="DialogOpened" />. The preview is a <see cref="ImagePreviewerDialog" /> window when the platform
    /// supports native windows, and an overlay in the current window otherwise. The call does nothing while the
    /// preview is open or opening, and when there is no item to preview.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// The control is not attached to a top level.
    /// </exception>
    public void OpenDialog()
    {
        if (_openState != null || _dialogOpening)
        {
            return;
        }

        PrepareDialogOpen();
        if (EffectiveItems is not { Count: > 0 })
        {
            return;
        }
        RequestPreviewLoads();

        _dialogOpening = true;
        var placementTarget = this;
        Debug.Assert(placementTarget != null);
        var topLevel = TopLevel.GetTopLevel(placementTarget);
        if (!RuntimePlatform.Features.SupportsNativeWindow)
        {
            OpenOverlayDialog(placementTarget, topLevel);
            return;
        }

        if (topLevel is null)
        {
            _dialogOpening = false;
            throw new InvalidOperationException("Unable to resolve TopLevel for ImagePreviewer.");
        }

        CompositeDisposable relayBindingDisposables = new CompositeDisposable();

        var previewDialog = new ImagePreviewerDialog(topLevel, this);
        RelayDialogHostBindings(relayBindingDisposables, previewDialog);

        Debug.Assert(previewDialog != null);
        var handlerCleanup = new CompositeDisposable(8);
        previewDialog.Topmost = IsDialogTopmost;
        ((ISetLogicalParent)previewDialog).SetParent(this);

        previewDialog
            .Bind(
                ThemeVariantScope.ActualThemeVariantProperty,
                this.GetBindingObservable(ThemeVariantScope.ActualThemeVariantProperty))
            .DisposeWith(handlerCleanup);

        SubscribeToEventHandler<ImagePreviewerDialog, EventHandler<TemplateAppliedEventArgs>>(previewDialog, RootTemplateApplied,
            (x, handler) => x.TemplateApplied += handler,
            (x, handler) => x.TemplateApplied -= handler).DisposeWith(handlerCleanup);

        SubscribeToEventHandler<Control, EventHandler<VisualTreeAttachmentEventArgs>>(placementTarget, TargetDetached,
            (x, handler) => x.DetachedFromVisualTree += handler,
            (x, handler) => x.DetachedFromVisualTree -= handler).DisposeWith(handlerCleanup);
        if (topLevel is Window window)
        {
            SubscribeToEventHandler<Window, EventHandler>(window, ParentClosed,
                (x, handler) => x.Closed += handler,
                (x, handler) => x.Closed -= handler).DisposeWith(handlerCleanup);
        }

        var cleanupPopup = Disposable.Create((previewDialog, handlerCleanup), state =>
        {
            previewDialog.Close(() =>
            {
                state.handlerCleanup.Dispose();
                ((ISetLogicalParent)state.previewDialog).SetParent(null);
                relayBindingDisposables.Dispose();
                _dialogClosing             = false;
                if (DataContext is IDialogAwareDataContext dialogAwareDataContext)
                {
                    dialogAwareDataContext.NotifyClosed();
                }
                DialogClosed?.Invoke(this, EventArgs.Empty);
                RaiseCrossRootsChanged();
            });
        });

        _openState = new DialogOpenState(topLevel, previewDialog, cleanupPopup);

        previewDialog.Focus();
        if (IsDialogModal)
        {
            if (topLevel is Window windowTopLevel)
            {
                previewDialog.ShowDialog(windowTopLevel);
            }
        }
        else
        {
            previewDialog.Show();
        }

        if (IsDialogModal)
        {
            var tcs = new TaskCompletionSource<object?>();
            var disposables = new CompositeDisposable(
            [
                Observable.FromEventPattern(
                              x => DialogClosed += x,
                              x => DialogClosed -= x)
                          .Take(1)
                          .Subscribe(_ =>
                          {
                              _modalSubscription?.Dispose();
                          }),
                Disposable.Create(() =>
                {
                    _modalSubscription = null;
                    tcs.SetResult(null);
                })
            ]);

            _modalSubscription = disposables;
        }

        using (BeginIgnoringIsOpen())
        {
            SetCurrentValue(IsOpenProperty, true);
        }
        DialogOpened?.Invoke(this, EventArgs.Empty);
        RaiseCrossRootsChanged();
        _dialogOpening = false;
    }

    /// <summary>
    /// Called at the start of <see cref="OpenDialog" />, before it checks whether there is an item to preview. The
    /// base implementation builds the entries when they do not exist yet. Overrides must call the base member,
    /// otherwise <see cref="OpenDialog" /> does nothing while the entries are missing.
    /// </summary>
    private protected virtual void PrepareDialogOpen()
    {
        if (EffectiveItems is null)
        {
            MaterializeEffectiveItems();
        }
    }

    /// <summary>
    /// Closes the open preview. It raises <see cref="DialogClosing" /> first and stops when the event is canceled.
    /// Otherwise it disposes the open state, which closes the host and raises <see cref="DialogClosed" /> when
    /// the host has closed. It then releases the full images, requests the loads of the closed state and sets
    /// <see cref="IsOpen" /> to <see langword="false" />. When no preview is open, only <see cref="IsOpen" /> is
    /// reset.
    /// </summary>
    protected virtual void CloseDialog()
    {
        if (_dialogClosing)
        {
            return;
        }

        var closingArgs = new CancelEventArgs();
        DialogClosing?.Invoke(this, closingArgs);
        if (closingArgs.Cancel)
        {
            return;
        }

        if (_openState is null)
        {
            using (BeginIgnoringIsOpen())
            {
                SetCurrentValue(IsOpenProperty, false);
            }

            return;
        }

        _openState.Dispose();
        _openState = null;
        _previewDecodeSize = null;

        _modalSubscription?.Dispose();
        _modalSubscription = null;
        ReleaseFullImageLoads();
        RequestClosedStateLoads();
        using (BeginIgnoringIsOpen())
        {
            SetCurrentValue(IsOpenProperty, false);
        }
    }

    /// <summary>
    /// Relays the properties that the native preview window follows to <paramref name="dialogHost" />: the items and
    /// <see cref="CurrentIndex" /> in both directions, the image interaction settings, the loading and error content,
    /// the title settings, <see cref="IsMotionEnabled" /> and <see cref="IsDialogModal" />. Each relay binding goes
    /// into <paramref name="disposables" />. It is called while the preview opens, before the window is shown.
    /// Overrides must call the base implementation, otherwise the window shows none of these values.
    /// </summary>
    private protected virtual void RelayDialogHostBindings(CompositeDisposable disposables, ImagePreviewerDialog dialogHost)
    {
        // The dialog is created in a separate visual root, outside this control's template.
        // ControlTheme cannot reach it; the dialog session owns and disposes these relay bindings.
        disposables.Add(BindUtils.RelayBind(this, IsImageMovableProperty, dialogHost, ImagePreviewerDialog.IsImageMovableProperty));
        disposables.Add(BindUtils.RelayBind(this, ImageScaleStepProperty, dialogHost, ImagePreviewerDialog.ScaleStepProperty));
        disposables.Add(BindUtils.RelayBind(this, ImageMinScaleProperty, dialogHost, ImagePreviewerDialog.MinScaleProperty));
        disposables.Add(BindUtils.RelayBind(this, ImageMaxScaleProperty, dialogHost, ImagePreviewerDialog.MaxScaleProperty));
        disposables.Add(BindUtils.RelayBind(this, IsWheelZoomEnabledProperty, dialogHost, ImagePreviewerDialog.IsWheelZoomEnabledProperty));
        disposables.Add(BindUtils.RelayBind(this, EffectiveItemsProperty, dialogHost, ImagePreviewerDialog.ItemsSourceProperty));
        disposables.Add(BindUtils.RelayBind(this, LoadingContentProperty, dialogHost, ImagePreviewerDialog.LoadingContentProperty));
        disposables.Add(BindUtils.RelayBind(this, LoadingContentTemplateProperty, dialogHost, ImagePreviewerDialog.LoadingContentTemplateProperty));
        disposables.Add(BindUtils.RelayBind(this, ErrorContentProperty, dialogHost, ImagePreviewerDialog.ErrorContentProperty));
        disposables.Add(BindUtils.RelayBind(this, ErrorContentTemplateProperty, dialogHost, ImagePreviewerDialog.ErrorContentTemplateProperty));
        disposables.Add(BindUtils.RelayBind(this, IsMotionEnabledProperty, dialogHost, ImagePreviewerDialog.IsMotionEnabledProperty));
        disposables.Add(BindUtils.RelayBind(this, IsDialogModalProperty, dialogHost, ImagePreviewerDialog.IsModalProperty));
        disposables.Add(BindUtils.RelayBind(this,
            CurrentIndexProperty,
            dialogHost,
            ImagePreviewerDialog.CurrentIndexProperty,
            BindingMode.TwoWay));
        disposables.Add(BindUtils.RelayBind(this, PreviewTitleProperty, dialogHost, Window.TitleProperty));
        disposables.Add(BindUtils.RelayBind(this, PreviewTitleIconProperty, dialogHost, ImagePreviewerDialog.TitleIconProperty));
        disposables.Add(BindUtils.RelayBind(this, PreviewTitleResolverProperty, dialogHost, ImagePreviewerDialog.PreviewTitleResolverProperty));
    }

    private void RaiseCrossRootsChanged()
    {
        CrossRootsChanged?.Invoke(this, EventArgs.Empty);
    }

    private static int CoerceNonNegativeValue(AvaloniaObject sender, int value)
    {
        return Math.Max(0, value);
    }

    private void HandleItemsSourceChanged(IEnumerable<ImagePreviewItem>? oldValue)
    {
        UnsubscribeItemsSource(oldValue as INotifyCollectionChanged);
        _observableItemsSource = ItemsSource as INotifyCollectionChanged;
        SubscribeItemsSource();
        MaterializeEffectiveItems();
    }

    private void SubscribeItemsSource()
    {
        if (!_isAttachedToVisualTree || _isItemsSourceSubscribed || _observableItemsSource is null)
        {
            return;
        }
        _observableItemsSource.CollectionChanged += HandleItemsCollectionChanged;
        _isItemsSourceSubscribed = true;
    }

    private void UnsubscribeItemsSource(INotifyCollectionChanged? source = null)
    {
        if (!_isItemsSourceSubscribed)
        {
            return;
        }
        (source ?? _observableItemsSource)?.CollectionChanged -= HandleItemsCollectionChanged;
        _isItemsSourceSubscribed = false;
    }

    private void SetEffectiveItems(
        ObservableCollection<ImagePreviewEntry> entries,
        IReadOnlySet<ImagePreviewEntry>? preserved = null)
    {
        var oldEntries = EffectiveItems;
        UnsubscribeEntries(oldEntries);
        SetCurrentValue(EffectiveItemsProperty, entries);
        SubscribeEntries(entries);
        if (oldEntries is not null)
        {
            foreach (var entry in oldEntries)
            {
                if (preserved?.Contains(entry) != true)
                {
                    entry.Dispose();
                }
            }
        }
        ConfigureCurrentEntry();
        OnEffectiveItemsChanged();
        RequestLoadsForCurrentState();
    }

    private void HandleItemsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs args)
    {
        if (EffectiveItems is not ObservableCollection<ImagePreviewEntry> entries)
        {
            MaterializeEffectiveItems();
            return;
        }
        switch (args.Action)
        {
            case NotifyCollectionChangedAction.Add:
                if (args.NewStartingIndex < 0)
                {
                    MaterializeEffectiveItems();
                    return;
                }
                var addIndex = args.NewStartingIndex >= 0 ? args.NewStartingIndex : entries.Count;
                foreach (ImagePreviewItem item in args.NewItems ?? Array.Empty<object>())
                {
                    var entry = new ImagePreviewEntry(item);
                    entry.PropertyChanged += HandleEffectiveEntryPropertyChanged;
                    entries.Insert(addIndex++, entry);
                }
                break;
            case NotifyCollectionChangedAction.Remove:
                if (args.OldStartingIndex < 0)
                {
                    MaterializeEffectiveItems();
                    return;
                }
                RemoveEntries(entries, args.OldStartingIndex, args.OldItems?.Count ?? 0);
                break;
            case NotifyCollectionChangedAction.Replace:
                if (args.OldStartingIndex < 0 || args.NewStartingIndex < 0)
                {
                    MaterializeEffectiveItems();
                    return;
                }
                RemoveEntries(entries, args.OldStartingIndex, args.OldItems?.Count ?? 0);
                var replaceIndex = args.NewStartingIndex;
                foreach (ImagePreviewItem item in args.NewItems ?? Array.Empty<object>())
                {
                    var entry = new ImagePreviewEntry(item);
                    entry.PropertyChanged += HandleEffectiveEntryPropertyChanged;
                    entries.Insert(replaceIndex++, entry);
                }
                break;
            case NotifyCollectionChangedAction.Move:
                if (args.OldItems?.Count == 1)
                {
                    entries.Move(args.OldStartingIndex, args.NewStartingIndex);
                }
                else
                {
                    MaterializeEffectiveItems();
                    return;
                }
                break;
            default:
                MaterializeEffectiveItems();
                return;
        }
        ConfigureCurrentEntry();
        OnEffectiveItemsChanged();
        RequestLoadsForCurrentState();
    }

    private void RemoveEntries(
        ObservableCollection<ImagePreviewEntry> entries,
        int startIndex,
        int count)
    {
        for (var index = 0; index < count && startIndex >= 0 && startIndex < entries.Count; index++)
        {
            var entry = entries[startIndex];
            entry.PropertyChanged -= HandleEffectiveEntryPropertyChanged;
            entries.RemoveAt(startIndex);
            entry.Dispose();
        }
    }

    private void RequestLoadsForCurrentState()
    {
        if (!_isAttachedToVisualTree)
        {
            return;
        }
        if (IsOpen)
        {
            RequestPreviewLoads();
        }
        else
        {
            RequestClosedStateLoads();
        }
    }

    private void CancelImageLoads(bool releaseLeases = false)
    {
        if (EffectiveItems is null)
        {
            return;
        }
        foreach (var entry in EffectiveItems)
        {
            if (releaseLeases)
            {
                entry.Unload();
            }
            else
            {
                entry.CancelFullLoad();
                entry.CancelThumbnailLoad();
            }
        }
    }

    private void ReleaseFullImageLoads()
    {
        if (EffectiveItems is null)
        {
            return;
        }
        foreach (var entry in EffectiveItems)
        {
            entry.UnloadFull();
        }
    }

    private void SubscribeEntries(IEnumerable<ImagePreviewEntry>? entries)
    {
        if (entries is null)
        {
            return;
        }
        foreach (var entry in entries)
        {
            entry.PropertyChanged += HandleEffectiveEntryPropertyChanged;
        }
    }

    private void UnsubscribeEntries(IEnumerable<ImagePreviewEntry>? entries)
    {
        if (entries is null)
        {
            return;
        }
        foreach (var entry in entries)
        {
            entry.PropertyChanged -= HandleEffectiveEntryPropertyChanged;
        }
    }

    private void HandleEffectiveEntryPropertyChanged(object? sender, PropertyChangedEventArgs args)
    {
        if (args.PropertyName is nameof(ImagePreviewEntry.IsFullLoaded) or
            nameof(ImagePreviewEntry.FullImage))
        {
            // Retained-frame seed: any entry that finishes its full-image load updates it; it is cleared when
            // that image becomes unusable (unloaded or released).
            if (sender is ImagePreviewEntry { IsFullLoaded: true, FullImage: not null } loaded)
            {
                _latestLoadedFullEntry = loaded;
            }
            else if (ReferenceEquals(sender, _latestLoadedFullEntry))
            {
                _latestLoadedFullEntry = null;
            }
        }
        if (ReferenceEquals(sender, _currentEntry))
        {
            UpdateCurrentState();
        }
    }

    private void ConfigureCurrentEntry()
    {
        var next = EffectiveItems is { Count: > 0 } entries
            ? entries[ClampIndex(CurrentIndex, entries.Count)]
            : null;
        if (!ReferenceEquals(_currentEntry, next))
        {
            _currentEntry = next;
            _previewDecodeSize = null;
            _lastNotifiedCurrentState = ImageLoadState.Idle;
        }
        UpdateCurrentState();
    }

    private void UpdateCurrentState()
    {
        var item = _currentEntry?.Item;
        var state = _currentEntry?.FullState ?? ImageLoadState.Idle;
        SetAndRaise(CurrentItemProperty, ref _currentItem, item);
        SetAndRaise(CurrentLoadStateProperty, ref _currentLoadState, state);
        SetAndRaise(CurrentLoadErrorProperty, ref _currentLoadError, _currentEntry?.FullError);
        SetAndRaise(CurrentLoadProgressProperty, ref _currentLoadProgress, _currentEntry?.FullProgress);
        SetAndRaise(IsCurrentLoadingProperty, ref _isCurrentLoading, state == ImageLoadState.Loading);
        SetAndRaise(IsCurrentLoadedProperty, ref _isCurrentLoaded, state == ImageLoadState.Loaded);
        SetAndRaise(IsCurrentFailedProperty, ref _isCurrentFailed, state == ImageLoadState.Failed);

        if (_currentEntry is not null && state != _lastNotifiedCurrentState)
        {
            var index = EffectiveItems?.IndexOf(_currentEntry) ?? -1;
            if (state == ImageLoadState.Loaded)
            {
                ImageLoadEventDispatcher.Dispatch(
                    ImageOpened,
                    this,
                    new ImagePreviewOpenedEventArgs(
                        _currentEntry.Item,
                        index,
                        _currentEntry.FullOrigin));
            }
            else if (state == ImageLoadState.Failed && _currentEntry.FullError is { } error)
            {
                ImageLoadEventDispatcher.Dispatch(
                    ImageFailed,
                    this,
                    new ImagePreviewFailedEventArgs(_currentEntry.Item, index, error));
            }
        }
        _lastNotifiedCurrentState = state;
    }

    private (int Width, int Height) GetFullDecodeSize()
    {
        var topLevel = TopLevel.GetTopLevel(this);
        var scaling = topLevel?.RenderScaling ?? 1;
        var size = topLevel?.ClientSize ?? Bounds.Size;
        return (Quantize(size.Width * scaling), Quantize(size.Height * scaling));
    }

    private static int Quantize(double value)
    {
        if (!double.IsFinite(value) || value <= 0)
        {
            return 0;
        }
        return checked((int)(Math.Ceiling(value / 16) * 16));
    }

    private void SubscribeDecodeSizeTopLevel()
    {
        var topLevel = TopLevel.GetTopLevel(this);
        if (ReferenceEquals(_decodeSizeTopLevel, topLevel))
        {
            return;
        }
        UnsubscribeDecodeSizeTopLevel();
        _decodeSizeTopLevel = topLevel;
        if (_decodeSizeTopLevel is not null)
        {
            _decodeSizeTopLevel.ScalingChanged += HandleDecodeSizeChanged;
            _decodeSizeTopLevel.SizeChanged += HandleDecodeSizeChanged;
        }
    }

    private void UnsubscribeDecodeSizeTopLevel()
    {
        if (_decodeSizeTopLevel is null)
        {
            return;
        }
        _decodeSizeTopLevel.ScalingChanged -= HandleDecodeSizeChanged;
        _decodeSizeTopLevel.SizeChanged -= HandleDecodeSizeChanged;
        _decodeSizeTopLevel = null;
    }

    private void HandleDecodeSizeChanged(object? sender, EventArgs args)
    {
        RequestLoadsForCurrentState();
    }

    private void HandleIsOpenChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (!_ignoreIsOpenChanged)
        {
            if (e.GetNewValue<bool>())
            {
                OpenDialog();
            }
            else
            {
                CloseDialog();
            }
        }
    }

    private void OpenOverlayDialog(Control placementTarget, TopLevel? topLevel)
    {
        try
        {
            var overlayLayer = OverlayLayerResolver.ResolveOverlayLayer(placementTarget, topLevel, nameof(ImagePreviewer));
            topLevel ??= TopLevel.GetTopLevel(overlayLayer);
            if (topLevel is null)
            {
                _dialogOpening = false;
                throw new InvalidOperationException("Unable to resolve TopLevel for ImagePreviewer.");
            }

            CompositeDisposable relayBindingDisposables = new CompositeDisposable();
            var previewHost = new ImagePreviewerOverlayHost(topLevel, this);
            RelayOverlayHostBindings(relayBindingDisposables, previewHost);

            var handlerCleanup = new CompositeDisposable(8);
            ((ISetLogicalParent)previewHost).SetParent(this);

            // This runtime overlay lives outside the owner's template and needs its actual theme scope.
            // handlerCleanup releases the cross-root binding when this overlay session closes.
            previewHost
                .Bind(
                    ThemeVariantScope.ActualThemeVariantProperty,
                    this.GetBindingObservable(ThemeVariantScope.ActualThemeVariantProperty))
                .DisposeWith(handlerCleanup);

            SubscribeToEventHandler<Control, EventHandler<VisualTreeAttachmentEventArgs>>(placementTarget, TargetDetached,
                (x, handler) => x.DetachedFromVisualTree += handler,
                (x, handler) => x.DetachedFromVisualTree -= handler).DisposeWith(handlerCleanup);

            ConfigureOverlayHostBounds(previewHost, overlayLayer, topLevel);
            void HandleOverlayLayerSizeChanged(object? sender, SizeChangedEventArgs args)
            {
                ConfigureOverlayHostBounds(previewHost, overlayLayer, topLevel);
            }

            overlayLayer.SizeChanged += HandleOverlayLayerSizeChanged;
            overlayLayer.Children.Add(previewHost);

            var cleanupPopup = Disposable.Create((previewHost, overlayLayer, handlerCleanup), state =>
            {
                previewHost.Close(() =>
                {
                    overlayLayer.SizeChanged -= HandleOverlayLayerSizeChanged;
                    state.overlayLayer.Children.Remove(state.previewHost);
                    state.handlerCleanup.Dispose();
                    ((ISetLogicalParent)state.previewHost).SetParent(null);
                    relayBindingDisposables.Dispose();
                    _dialogClosing             = false;
                    if (DataContext is IDialogAwareDataContext dialogAwareDataContext)
                    {
                        dialogAwareDataContext.NotifyClosed();
                    }
                    DialogClosed?.Invoke(this, EventArgs.Empty);
                    RaiseCrossRootsChanged();
                });
            });

            _openState = new OverlayOpenState(topLevel, previewHost, cleanupPopup);
            previewHost.Focus();

            using (BeginIgnoringIsOpen())
            {
                SetCurrentValue(IsOpenProperty, true);
            }
            DialogOpened?.Invoke(this, EventArgs.Empty);
            RaiseCrossRootsChanged();
            _dialogOpening = false;
        }
        catch
        {
            _dialogOpening = false;
            throw;
        }
    }

    private static void ConfigureOverlayHostBounds(Control previewHost, Control overlayLayer, TopLevel topLevel)
    {
        var size = overlayLayer.Bounds.Size;
        if (size.Width <= 0 || size.Height <= 0)
        {
            size = topLevel.ClientSize;
        }

        previewHost.Width  = size.Width;
        previewHost.Height = size.Height;
        Canvas.SetLeft(previewHost, 0);
        Canvas.SetTop(previewHost, 0);
    }

    private void RelayOverlayHostBindings(CompositeDisposable disposables, ImagePreviewerOverlayHost overlayHost)
    {
        // The overlay host is inserted into the runtime OverlayLayer, not this control's template.
        // ControlTheme cannot reach it; the open-state disposables own these relay bindings.
        disposables.Add(BindUtils.RelayBind(this, IsImageMovableProperty, overlayHost, ImagePreviewerOverlayHost.IsImageMovableProperty));
        disposables.Add(BindUtils.RelayBind(this, ImageScaleStepProperty, overlayHost, ImagePreviewerOverlayHost.ScaleStepProperty));
        disposables.Add(BindUtils.RelayBind(this, ImageMinScaleProperty, overlayHost, ImagePreviewerOverlayHost.MinScaleProperty));
        disposables.Add(BindUtils.RelayBind(this, ImageMaxScaleProperty, overlayHost, ImagePreviewerOverlayHost.MaxScaleProperty));
        disposables.Add(BindUtils.RelayBind(this, IsWheelZoomEnabledProperty, overlayHost, ImagePreviewerOverlayHost.IsWheelZoomEnabledProperty));
        disposables.Add(BindUtils.RelayBind(this, EffectiveItemsProperty, overlayHost, ImagePreviewerOverlayHost.ItemsSourceProperty));
        disposables.Add(BindUtils.RelayBind(this, LoadingContentProperty, overlayHost, ImagePreviewerOverlayHost.LoadingContentProperty));
        disposables.Add(BindUtils.RelayBind(this, LoadingContentTemplateProperty, overlayHost, ImagePreviewerOverlayHost.LoadingContentTemplateProperty));
        disposables.Add(BindUtils.RelayBind(this, ErrorContentProperty, overlayHost, ImagePreviewerOverlayHost.ErrorContentProperty));
        disposables.Add(BindUtils.RelayBind(this, ErrorContentTemplateProperty, overlayHost, ImagePreviewerOverlayHost.ErrorContentTemplateProperty));
        disposables.Add(BindUtils.RelayBind(this, IsMotionEnabledProperty, overlayHost, ImagePreviewerOverlayHost.IsMotionEnabledProperty));
        disposables.Add(BindUtils.RelayBind(this, IsDialogModalProperty, overlayHost, ImagePreviewerOverlayHost.IsModalProperty));
        disposables.Add(BindUtils.RelayBind(this,
            CurrentIndexProperty,
            overlayHost,
            ImagePreviewerOverlayHost.CurrentIndexProperty,
            BindingMode.TwoWay));
    }

    private void RootTemplateApplied(object? sender, TemplateAppliedEventArgs e)
    {
        if (_openState is not DialogOpenState dialogOpenState)
        {
            return;
        }

        var popupHost = dialogOpenState.DialogHost;
        popupHost.TemplateApplied -= RootTemplateApplied;
        dialogOpenState.SetPresenterSubscription(null);

        // If the Popup appears in a control template, then the child controls
        // that appear in the popup host need to have their TemplatedParent
        // properties set.
        if (TemplatedParent != null && popupHost.Presenter is Control presenter)
        {
            presenter.ApplyTemplate();

            var presenterSubscription = presenter.GetObservable(ContentPresenter.ChildProperty)
                                                 .Subscribe(SetTemplatedParentAndApplyChildTemplates);

            dialogOpenState.SetPresenterSubscription(presenterSubscription);
        }

        // The dialog surface's template is applied only after the synchronous Show path, so the cross-root scan of the
        // semantic preview is keyed to this moment.
        RaiseCrossRootsChanged();
    }

    private void SetTemplatedParentAndApplyChildTemplates(Control? control)
    {
        if (control != null)
        {
            TemplatedControlUtils.ApplyTemplatedParent(control, TemplatedParent);
        }
    }

    private void TargetDetached(object? sender, VisualTreeAttachmentEventArgs e)
    {
        CloseDialog();
    }

    private static IDisposable SubscribeToEventHandler<T, TEventHandler>(T target, TEventHandler handler, Action<T, TEventHandler> subscribe, Action<T, TEventHandler> unsubscribe)
    {
        subscribe(target, handler);

        return Disposable.Create((unsubscribe, target, handler), state => state.unsubscribe(state.target, state.handler));
    }

    private void ParentClosed(object? sender, EventArgs e)
    {
        CloseDialog();
    }

    private IgnoreIsOpenScope BeginIgnoringIsOpen()
    {
        return new IgnoreIsOpenScope(this);
    }

    private readonly struct IgnoreIsOpenScope : IDisposable
    {
        private readonly AbstractImagePreviewer _owner;

        public IgnoreIsOpenScope(AbstractImagePreviewer owner)
        {
            _owner                      = owner;
            _owner._ignoreIsOpenChanged = true;
        }

        public void Dispose()
        {
            _owner._ignoreIsOpenChanged = false;
        }
    }

    private interface IImagePreviewerOpenState : IDisposable
    {
        TopLevel TopLevel { get; }

        /// <summary>
        /// Visual root of the live host (the native dialog window or the overlay host), reported to the cross-root
        /// contract.
        /// </summary>
        Visual? RootVisual { get; }

        void RefreshImageSwitchMode();
    }

    private class DialogOpenState : IImagePreviewerOpenState
    {
        private readonly IDisposable _cleanup;
        private IDisposable? _presenterCleanup;

        public DialogOpenState(TopLevel topLevel, ImagePreviewerDialog previewDialog, IDisposable cleanup)
        {
            TopLevel   = topLevel;
            DialogHost = previewDialog;
            _cleanup   = cleanup;
        }

        public ImagePreviewerDialog DialogHost { get; }
        public TopLevel TopLevel { get; }

        public Visual? RootVisual => DialogHost;

        public void SetPresenterSubscription(IDisposable? presenterCleanup)
        {
            _presenterCleanup?.Dispose();
            _presenterCleanup = presenterCleanup;
        }

        public void RefreshImageSwitchMode()
        {
            DialogHost.RefreshImageSwitchMode();
        }

        public void Dispose()
        {
            _presenterCleanup?.Dispose();
            _cleanup.Dispose();
        }
    }

    private class OverlayOpenState : IImagePreviewerOpenState
    {
        private readonly IDisposable _cleanup;

        public OverlayOpenState(TopLevel topLevel, ImagePreviewerOverlayHost previewHost, IDisposable cleanup)
        {
            TopLevel    = topLevel;
            PreviewHost = previewHost;
            _cleanup    = cleanup;
        }

        public ImagePreviewerOverlayHost PreviewHost { get; }
        public TopLevel TopLevel { get; }

        public Visual? RootVisual => PreviewHost;

        public void RefreshImageSwitchMode()
        {
            PreviewHost.RefreshImageSwitchMode();
        }

        public void Dispose()
        {
            _cleanup.Dispose();
        }
    }
}
