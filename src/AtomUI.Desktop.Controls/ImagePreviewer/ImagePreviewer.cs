using System.ComponentModel;
using AtomUI.Controls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Media;
using Avalonia.Metadata;

namespace AtomUI.Desktop.Controls;

public partial class ImagePreviewer : AbstractImagePreviewer
{
    #region 公共属性定义

    public static readonly StyledProperty<object?> CoverIndicatorContentProperty =
        AvaloniaProperty.Register<ImagePreviewer, object?>(nameof(CoverIndicatorContent));

    public static readonly StyledProperty<IDataTemplate?> CoverIndicatorContentTemplateProperty =
        AvaloniaProperty.Register<ImagePreviewer, IDataTemplate?>(nameof(CoverIndicatorContentTemplate));

    public static readonly StyledProperty<bool> IsShowCoverMaskProperty =
        AvaloniaProperty.Register<ImagePreviewer, bool>(nameof(IsShowCoverMask), true);

    public static readonly DirectProperty<ImagePreviewer, ImageLoadState> CoverLoadStateProperty =
        AvaloniaProperty.RegisterDirect<ImagePreviewer, ImageLoadState>(
            nameof(CoverLoadState),
            control => control.CoverLoadState);

    public static readonly DirectProperty<ImagePreviewer, ImageLoadError?> CoverLoadErrorProperty =
        AvaloniaProperty.RegisterDirect<ImagePreviewer, ImageLoadError?>(
            nameof(CoverLoadError),
            control => control.CoverLoadError);

    public static readonly DirectProperty<ImagePreviewer, ImageLoadProgress?> CoverLoadProgressProperty =
        AvaloniaProperty.RegisterDirect<ImagePreviewer, ImageLoadProgress?>(
            nameof(CoverLoadProgress),
            control => control.CoverLoadProgress);

    public static readonly DirectProperty<ImagePreviewer, bool> IsCoverLoadingProperty =
        AvaloniaProperty.RegisterDirect<ImagePreviewer, bool>(
            nameof(IsCoverLoading),
            control => control.IsCoverLoading);

    public static readonly DirectProperty<ImagePreviewer, bool> IsCoverLoadedProperty =
        AvaloniaProperty.RegisterDirect<ImagePreviewer, bool>(
            nameof(IsCoverLoaded),
            control => control.IsCoverLoaded);

    public static readonly DirectProperty<ImagePreviewer, bool> IsCoverFailedProperty =
        AvaloniaProperty.RegisterDirect<ImagePreviewer, bool>(
            nameof(IsCoverFailed),
            control => control.IsCoverFailed);

    [DependsOn(nameof(CoverIndicatorContentTemplate))]
    public object? CoverIndicatorContent
    {
        get => GetValue(CoverIndicatorContentProperty);
        set => SetValue(CoverIndicatorContentProperty, value);
    }

    public IDataTemplate? CoverIndicatorContentTemplate
    {
        get => GetValue(CoverIndicatorContentTemplateProperty);
        set => SetValue(CoverIndicatorContentTemplateProperty, value);
    }

    public bool IsShowCoverMask
    {
        get => GetValue(IsShowCoverMaskProperty);
        set => SetValue(IsShowCoverMaskProperty, value);
    }

    private ImageLoadState _coverLoadState;

    public ImageLoadState CoverLoadState => _coverLoadState;

    private ImageLoadError? _coverLoadError;

    public ImageLoadError? CoverLoadError => _coverLoadError;

    private ImageLoadProgress? _coverLoadProgress;

    public ImageLoadProgress? CoverLoadProgress => _coverLoadProgress;

    private bool _isCoverLoading;

    public bool IsCoverLoading => _isCoverLoading;

    private bool _isCoverLoaded;

    public bool IsCoverLoaded => _isCoverLoaded;

    private bool _isCoverFailed;

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

    private protected override void OnEffectiveItemsChanged()
    {
        ConfigureCoverEntry();
    }

    private protected override void RequestClosedStateLoads()
    {
        ConfigureCoverEntry();
        RequestCoverLoadIfNeeded();
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var arranged = base.ArrangeOverride(finalSize);
        RequestCoverLoadIfNeeded();
        return arranged;
    }

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
        // 订阅不变量：entry 被订阅 ⇔ 它是 cover 或 retained cover
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

    // 封面保留帧引用的唯一变更通道：替换时对新旧源（与封面项不同的那个）成对退订/订阅
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
            // 无封面项或保留帧源已卸载/释放/提交失败时丢弃（经配对通道同步退订）
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
            // WaitForLoaded 在目标尚无图期间保留上一张封面。
            displayImage = _retainedCoverEntry?.ThumbnailImage;
        }
        else
        {
            // Immediate 从切换瞬间的 Idle 开始即清空旧图并进入加载占位。
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
