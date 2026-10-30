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
/// 预览窗口切换图片时的显示策略。
/// </summary>
public enum ImageSwitchMode
{
    /// <summary>切换后立即清空显示并进入加载态（默认）。</summary>
    Immediate = 0,

    /// <summary>目标图片未加载完成前保持显示上一张已加载图片，加载完成后一次性切换。</summary>
    WaitForLoaded = 1,
}

public abstract class AbstractImagePreviewer : TemplatedControl,
                                               IMotionAwareControl,
                                               ISemanticPartCrossRootProvider
{
    #region 公共属性定义

    public static readonly StyledProperty<IEnumerable<ImagePreviewItem>?> ItemsSourceProperty =
        AvaloniaProperty.Register<AbstractImagePreviewer, IEnumerable<ImagePreviewItem>?>(nameof(ItemsSource));

    public static readonly StyledProperty<string?> PreviewTitleProperty =
        AvaloniaProperty.Register<AbstractImagePreviewer, string?>(nameof(PreviewTitle));

    public static readonly StyledProperty<PathIcon?> PreviewTitleIconProperty =
        AvaloniaProperty.Register<AbstractImagePreviewer, PathIcon?>(nameof(PreviewTitleIcon));

    public static readonly StyledProperty<IImagePreviewTitleResolver?> PreviewTitleResolverProperty =
        AvaloniaProperty.Register<AbstractImagePreviewer, IImagePreviewTitleResolver?>(
            nameof(PreviewTitleResolver),
            DefaultImagePreviewTitleResolver.Instance);

    public static readonly StyledProperty<object?> LoadingContentProperty =
        AvaloniaProperty.Register<AbstractImagePreviewer, object?>(nameof(LoadingContent));

    public static readonly StyledProperty<IDataTemplate?> LoadingContentTemplateProperty =
        AvaloniaProperty.Register<AbstractImagePreviewer, IDataTemplate?>(nameof(LoadingContentTemplate));

    public static readonly StyledProperty<object?> ErrorContentProperty =
        AvaloniaProperty.Register<AbstractImagePreviewer, object?>(nameof(ErrorContent));

    public static readonly StyledProperty<IDataTemplate?> ErrorContentTemplateProperty =
        AvaloniaProperty.Register<AbstractImagePreviewer, IDataTemplate?>(nameof(ErrorContentTemplate));

    public static readonly StyledProperty<bool> IsOpenProperty =
        AvaloniaProperty.Register<AbstractImagePreviewer, bool>(
            nameof(IsOpen),
            defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<AbstractImagePreviewer>();

    public static readonly StyledProperty<double> CoverWidthProperty =
        AvaloniaProperty.Register<AbstractImagePreviewer, double>(nameof(CoverWidth), double.NaN);

    public static readonly StyledProperty<double> CoverHeightProperty =
        AvaloniaProperty.Register<AbstractImagePreviewer, double>(nameof(CoverHeight), double.NaN);

    public static readonly StyledProperty<int> CurrentIndexProperty =
        AvaloniaProperty.Register<AbstractImagePreviewer, int>(
            nameof(CurrentIndex),
            0,
            defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<int> CoverIndexProperty =
        AvaloniaProperty.Register<AbstractImagePreviewer, int>(
            nameof(CoverIndex),
            0,
            coerce: CoerceNonNegativeValue);

    public static readonly StyledProperty<int> PreloadCountProperty =
        AvaloniaProperty.Register<AbstractImagePreviewer, int>(
            nameof(PreloadCount),
            1,
            coerce: CoerceNonNegativeValue);

    public static readonly StyledProperty<ImageSwitchMode> ImageSwitchModeProperty =
        AvaloniaProperty.Register<AbstractImagePreviewer, ImageSwitchMode>(
            nameof(ImageSwitchMode),
            ImageSwitchMode.Immediate);

    public static readonly DirectProperty<AbstractImagePreviewer, ImagePreviewItem?> CurrentItemProperty =
        AvaloniaProperty.RegisterDirect<AbstractImagePreviewer, ImagePreviewItem?>(
            nameof(CurrentItem),
            control => control.CurrentItem);

    public static readonly DirectProperty<AbstractImagePreviewer, ImageLoadState> CurrentLoadStateProperty =
        AvaloniaProperty.RegisterDirect<AbstractImagePreviewer, ImageLoadState>(
            nameof(CurrentLoadState),
            control => control.CurrentLoadState);

    public static readonly DirectProperty<AbstractImagePreviewer, ImageLoadError?> CurrentLoadErrorProperty =
        AvaloniaProperty.RegisterDirect<AbstractImagePreviewer, ImageLoadError?>(
            nameof(CurrentLoadError),
            control => control.CurrentLoadError);

    public static readonly DirectProperty<AbstractImagePreviewer, ImageLoadProgress?> CurrentLoadProgressProperty =
        AvaloniaProperty.RegisterDirect<AbstractImagePreviewer, ImageLoadProgress?>(
            nameof(CurrentLoadProgress),
            control => control.CurrentLoadProgress);

    public static readonly DirectProperty<AbstractImagePreviewer, bool> IsCurrentLoadingProperty =
        AvaloniaProperty.RegisterDirect<AbstractImagePreviewer, bool>(
            nameof(IsCurrentLoading),
            control => control.IsCurrentLoading);

    public static readonly DirectProperty<AbstractImagePreviewer, bool> IsCurrentLoadedProperty =
        AvaloniaProperty.RegisterDirect<AbstractImagePreviewer, bool>(
            nameof(IsCurrentLoaded),
            control => control.IsCurrentLoaded);

    public static readonly DirectProperty<AbstractImagePreviewer, bool> IsCurrentFailedProperty =
        AvaloniaProperty.RegisterDirect<AbstractImagePreviewer, bool>(
            nameof(IsCurrentFailed),
            control => control.IsCurrentFailed);

    public static readonly StyledProperty<bool> IsImageMovableProperty =
        AvaloniaProperty.Register<AbstractImagePreviewer, bool>(nameof(IsImageMovable), true);

    public static readonly StyledProperty<double> ImageScaleStepProperty =
        AvaloniaProperty.Register<AbstractImagePreviewer, double>(nameof(ImageScaleStep), 0.5);

    public static readonly StyledProperty<double> ImageMinScaleProperty =
        AvaloniaProperty.Register<AbstractImagePreviewer, double>(nameof(ImageMinScale), 1.0);

    public static readonly StyledProperty<double> ImageMaxScaleProperty =
        AvaloniaProperty.Register<AbstractImagePreviewer, double>(nameof(ImageMaxScale), 50.0);

    public static readonly StyledProperty<bool> IsWheelZoomEnabledProperty =
        AvaloniaProperty.Register<AbstractImagePreviewer, bool>(nameof(IsWheelZoomEnabled), true);

    public static readonly StyledProperty<bool> IsDialogModalProperty =
        AvaloniaProperty.Register<AbstractImagePreviewer, bool>(nameof (IsDialogModal), false);

    public static readonly StyledProperty<bool> IsDialogTopmostProperty =
        AvaloniaProperty.Register<AbstractImagePreviewer, bool>(nameof(IsDialogTopmost));

    public IEnumerable<ImagePreviewItem>? ItemsSource
    {
        get => GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    public string? PreviewTitle
    {
        get => GetValue(PreviewTitleProperty);
        set => SetValue(PreviewTitleProperty, value);
    }

    public PathIcon? PreviewTitleIcon
    {
        get => GetValue(PreviewTitleIconProperty);
        set => SetValue(PreviewTitleIconProperty, value);
    }

    public IImagePreviewTitleResolver? PreviewTitleResolver
    {
        get => GetValue(PreviewTitleResolverProperty);
        set => SetValue(PreviewTitleResolverProperty, value);
    }

    [DependsOn(nameof(LoadingContentTemplate))]
    public object? LoadingContent
    {
        get => GetValue(LoadingContentProperty);
        set => SetValue(LoadingContentProperty, value);
    }

    public IDataTemplate? LoadingContentTemplate
    {
        get => GetValue(LoadingContentTemplateProperty);
        set => SetValue(LoadingContentTemplateProperty, value);
    }

    [DependsOn(nameof(ErrorContentTemplate))]
    public object? ErrorContent
    {
        get => GetValue(ErrorContentProperty);
        set => SetValue(ErrorContentProperty, value);
    }

    public IDataTemplate? ErrorContentTemplate
    {
        get => GetValue(ErrorContentTemplateProperty);
        set => SetValue(ErrorContentTemplateProperty, value);
    }

    public bool IsOpen
    {
        get => GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }

    public double CoverWidth
    {
        get => GetValue(CoverWidthProperty);
        set => SetValue(CoverWidthProperty, value);
    }

    public double CoverHeight
    {
        get => GetValue(CoverHeightProperty);
        set => SetValue(CoverHeightProperty, value);
    }

    public int CurrentIndex
    {
        get => GetValue(CurrentIndexProperty);
        set => SetValue(CurrentIndexProperty, value);
    }

    public int CoverIndex
    {
        get => GetValue(CoverIndexProperty);
        set => SetValue(CoverIndexProperty, value);
    }

    public int PreloadCount
    {
        get => GetValue(PreloadCountProperty);
        set => SetValue(PreloadCountProperty, value);
    }

    public ImageSwitchMode ImageSwitchMode
    {
        get => GetValue(ImageSwitchModeProperty);
        set => SetValue(ImageSwitchModeProperty, value);
    }

    private ImagePreviewItem? _currentItem;

    public ImagePreviewItem? CurrentItem => _currentItem;

    private ImageLoadState _currentLoadState;

    public ImageLoadState CurrentLoadState => _currentLoadState;

    private ImageLoadError? _currentLoadError;

    public ImageLoadError? CurrentLoadError => _currentLoadError;

    private ImageLoadProgress? _currentLoadProgress;

    public ImageLoadProgress? CurrentLoadProgress => _currentLoadProgress;

    private bool _isCurrentLoading;

    public bool IsCurrentLoading => _isCurrentLoading;

    private bool _isCurrentLoaded;

    public bool IsCurrentLoaded => _isCurrentLoaded;

    private bool _isCurrentFailed;

    public bool IsCurrentFailed => _isCurrentFailed;

    public bool IsImageMovable
    {
        get => GetValue(IsImageMovableProperty);
        set => SetValue(IsImageMovableProperty, value);
    }

    public double ImageScaleStep
    {
        get => GetValue(ImageScaleStepProperty);
        set => SetValue(ImageScaleStepProperty, value);
    }

    public double ImageMinScale
    {
        get => GetValue(ImageMinScaleProperty);
        set => SetValue(ImageMinScaleProperty, value);
    }

    public double ImageMaxScale
    {
        get => GetValue(ImageMaxScaleProperty);
        set => SetValue(ImageMaxScaleProperty, value);
    }

    public bool IsWheelZoomEnabled
    {
        get => GetValue(IsWheelZoomEnabledProperty);
        set => SetValue(IsWheelZoomEnabledProperty, value);
    }

    public bool IsDialogModal
    {
        get => GetValue(IsDialogModalProperty);
        set => SetValue(IsDialogModalProperty, value);
    }

    public bool IsDialogTopmost
    {
        get => GetValue(IsDialogTopmostProperty);
        set => SetValue(IsDialogTopmostProperty, value);
    }

    #endregion

    #region 公共事件定义

    public event EventHandler? DialogClosed;

    public event EventHandler? DialogOpened;

    public event EventHandler<CancelEventArgs>? DialogClosing;

    public event EventHandler<ImagePreviewOpenedEventArgs>? ImageOpened;

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
    /// 最近一个完成全图加载且图像仍可用的 entry。WaitForLoaded 模式下作为保留帧种子：
    /// 当切换快于加载完成时，当前项可能在完成前被取代，保留帧必须能从任何完成者补充，
    /// 否则显示会回退占位符并随偶发完成振荡（频闪）。
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
    /// 预览面承载在独立 Window（ImagePreviewerDialog）中，属 owner 视觉树之外的跨根宿主；
    /// 语义预览经该契约发现宿主并在打开/关闭/表面就绪时跟随刷新。overlay 宿主
    /// （同窗口内）同样经此上报，两路径统一。
    /// </summary>
    public IReadOnlyList<Visual> GetCrossRoots()
    {
        return _openState?.RootVisual is { } rootVisual
            ? new Visual[] { rootVisual }
            : Array.Empty<Visual>();
    }

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

    private protected virtual void RequestClosedStateLoads()
    {
    }

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

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _isAttachedToVisualTree = true;
        SubscribeDecodeSizeTopLevel();
        SubscribeItemsSource();
        MaterializeEffectiveItems();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _isAttachedToVisualTree = false;
        UnsubscribeDecodeSizeTopLevel();
        UnsubscribeItemsSource();
        CancelImageLoads(releaseLeases: true);
        base.OnDetachedFromVisualTree(e);
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        OpenDialog();
    }

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

    private protected virtual void PrepareDialogOpen()
    {
        if (EffectiveItems is null)
        {
            MaterializeEffectiveItems();
        }
    }

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
            // 保留帧种子：任何 entry 完成全图加载都更新；图像失效（卸载/释放）时清除
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

        // 对话框表面模板就绪晚于 Show 的同步路径；语义预览的跨根扫描以此刻为准。
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

        /// <summary>存活宿主的视觉根（native 对话框窗口 / overlay 宿主），供跨根契约上报。</summary>
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
