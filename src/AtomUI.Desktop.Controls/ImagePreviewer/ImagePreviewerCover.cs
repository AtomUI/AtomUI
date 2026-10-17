using AtomUI.Animations;
using Avalonia.Threading;
using AtomUI.Controls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Interactivity;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

internal class ImagePreviewerCover : ContentControl, IMotionAwareControl
{
    #region 公共属性定义

    public static readonly StyledProperty<IImage?> ImageSourceProperty =
        AvaloniaProperty.Register<ImagePreviewerCover, IImage?>(nameof(ImageSource));

    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<ImagePreviewerCover>();

    public static readonly StyledProperty<bool> IsShowCoverMaskProperty =
        ImagePreviewer.IsShowCoverMaskProperty.AddOwner<ImagePreviewerCover>();

    public static readonly StyledProperty<bool> IsLoadingProperty =
        AvaloniaProperty.Register<ImagePreviewerCover, bool>(nameof(IsLoading));

    public static readonly StyledProperty<bool> IsFailedProperty =
        AvaloniaProperty.Register<ImagePreviewerCover, bool>(nameof(IsFailed));

    public static readonly StyledProperty<object?> LoadingContentProperty =
        AvaloniaProperty.Register<ImagePreviewerCover, object?>(nameof(LoadingContent));

    public static readonly StyledProperty<IDataTemplate?> LoadingContentTemplateProperty =
        AvaloniaProperty.Register<ImagePreviewerCover, IDataTemplate?>(nameof(LoadingContentTemplate));

    public static readonly StyledProperty<object?> ErrorContentProperty =
        AvaloniaProperty.Register<ImagePreviewerCover, object?>(nameof(ErrorContent));

    public static readonly StyledProperty<IDataTemplate?> ErrorContentTemplateProperty =
        AvaloniaProperty.Register<ImagePreviewerCover, IDataTemplate?>(nameof(ErrorContentTemplate));

    public IImage? ImageSource
    {
        get => GetValue(ImageSourceProperty);
        set => SetValue(ImageSourceProperty, value);
    }

    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }

    public bool IsShowCoverMask
    {
        get => GetValue(IsShowCoverMaskProperty);
        set => SetValue(IsShowCoverMaskProperty, value);
    }

    public bool IsLoading
    {
        get => GetValue(IsLoadingProperty);
        set => SetValue(IsLoadingProperty, value);
    }

    public bool IsFailed
    {
        get => GetValue(IsFailedProperty);
        set => SetValue(IsFailedProperty, value);
    }

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

    #endregion

    #region 内部属性定义

    internal static readonly DirectProperty<ImagePreviewerCover, bool> HasErrorProperty =
        AvaloniaProperty.RegisterDirect<ImagePreviewerCover, bool>(
            nameof(HasError),
            control => control.HasError);

    internal static readonly StyledProperty<double> MaskOpacityProperty =
        AvaloniaProperty.Register<ImagePreviewerCover, double>(nameof(MaskOpacity), 0.0);

    internal static readonly DirectProperty<ImagePreviewerCover, bool> IsCoverMaskVisibleProperty =
        AvaloniaProperty.RegisterDirect<ImagePreviewerCover, bool>(
            nameof(IsCoverMaskVisible),
            o => o.IsCoverMaskVisible,
            (o, v) => o.IsCoverMaskVisible = v);

    internal static readonly StyledProperty<Thickness> OwnerPaddingProperty =
        AvaloniaProperty.Register<ImagePreviewerCover, Thickness>(nameof(OwnerPadding));

    internal static readonly StyledProperty<Thickness> OwnerBorderThicknessProperty =
        AvaloniaProperty.Register<ImagePreviewerCover, Thickness>(nameof(OwnerBorderThickness));

    internal static readonly StyledProperty<CornerRadius> OwnerCornerRadiusProperty =
        AvaloniaProperty.Register<ImagePreviewerCover, CornerRadius>(nameof(OwnerCornerRadius));

    internal static readonly DirectProperty<ImagePreviewerCover, Thickness> OwnerMaskMarginProperty =
        AvaloniaProperty.RegisterDirect<ImagePreviewerCover, Thickness>(
            nameof(OwnerMaskMargin),
            o => o.OwnerMaskMargin);

    private bool _hasError;

    internal bool HasError => _hasError;

    internal double MaskOpacity
    {
        get => GetValue(MaskOpacityProperty);
        set => SetValue(MaskOpacityProperty, value);
    }

    private bool _isCoverMaskVisible = true;

    internal bool IsCoverMaskVisible
    {
        get => _isCoverMaskVisible;
        set => SetAndRaise(IsCoverMaskVisibleProperty, ref _isCoverMaskVisible, value);
    }

    /// <summary>owner 模板 PixelAlignedBorder 的 Padding（用于负 Margin 抵消），非布局用途。</summary>
    internal Thickness OwnerPadding
    {
        get => GetValue(OwnerPaddingProperty);
        set => SetValue(OwnerPaddingProperty, value);
    }

    /// <summary>owner 模板 PixelAlignedBorder 的 BorderThickness（用于负 Margin 抵消），非布局用途。</summary>
    internal Thickness OwnerBorderThickness
    {
        get => GetValue(OwnerBorderThicknessProperty);
        set => SetValue(OwnerBorderThicknessProperty, value);
    }

    /// <summary>
    /// owner 模板 PixelAlignedBorder 的 CornerRadius（遮罩与模板 presenter 圆角跟随），非布局用途。
    /// 上游 cover 的 inset:0 覆盖层在 root 的 overflow:hidden + border-radius 下被裁成圆角；
    /// AtomUI 的遮罩以负 Margin 越过 owner padding，无法被 owner 的圆角裁剪覆盖，
    /// 因此直接把该值涂到 Mask 与模板 border/loading/error presenter 的 CornerRadius 上。
    /// </summary>
    internal CornerRadius OwnerCornerRadius
    {
        get => GetValue(OwnerCornerRadiusProperty);
        set => SetValue(OwnerCornerRadiusProperty, value);
    }

    private Thickness _ownerMaskMargin;

    /// <summary>
    /// 遮罩层的负外边距：抵消 owner（ImagePreviewer/ImageGroupPreviewer 模板的
    /// PixelAlignedBorder）的 Padding + BorderThickness，使遮罩铺满整个 root，
    /// 对齐 ant `genImageCoverStyle` 的 `position:absolute; inset:0` cover。
    /// 这是运行时几何（宿主 padding 是用户属性），ControlTheme 无法静态表达。
    /// </summary>
    internal Thickness OwnerMaskMargin
    {
        get => _ownerMaskMargin;
        private set => SetAndRaise(OwnerMaskMarginProperty, ref _ownerMaskMargin, value);
    }

    #endregion

    static ImagePreviewerCover()
    {
        // Avalonia TemplatedControl 默认 ClipToBounds=true（合成层裁剪），会把负 Margin
        // 铺出 cover 边界的遮罩（对齐 ant genImageCoverStyle 的 absolute inset:0）裁回
        // 内区，root 的 padding 环永远压不暗。裁剪职责归 owner 根的 PixelAlignedBorder
        // （对齐上游 root 的 overflow:hidden + border-radius）。
        ClipToBoundsProperty.OverrideDefaultValue<ImagePreviewerCover>(false);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == OwnerPaddingProperty ||
            change.Property == OwnerBorderThicknessProperty)
        {
            UpdateOwnerMaskGeometry();
        }
        if (change.Property == ImageSourceProperty ||
            change.Property == IsShowCoverMaskProperty ||
            change.Property == IsLoadingProperty ||
            change.Property == IsFailedProperty ||
            change.Property == LoadingContentProperty ||
            change.Property == LoadingContentTemplateProperty)
        {
            UpdateVisualState();
        }
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();
        UpdateOwnerMaskGeometry();
        UpdateVisualState();
        this.DisableTransitions();
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        Dispatcher.Post(this.EnableTransitions);
    }

    private void UpdateVisualState()
    {
        PseudoClasses.Set(":has-image", ImageSource is not null);
        PseudoClasses.Set(":loading", IsLoading);
        PseudoClasses.Set(":failed", IsFailed);
        PseudoClasses.Set(":loading-skeleton", IsLoading && ImageSource is null && LoadingContent is null);
        SetAndRaise(HasErrorProperty, ref _hasError, IsFailed && ImageSource is null);
        // mask 是悬停操作层（点击打开预览在任何加载态都有效），只由 IsShowCoverMask
        // 决定，与加载/图片/失败状态解耦：透明度完全交给主题（基础 0，悬停 1），
        // 避免 C# 强制值与悬停样式/过渡互相打断造成闪烁
        SetCurrentValue(IsCoverMaskVisibleProperty, IsShowCoverMask);
    }

    private void UpdateOwnerMaskGeometry()
    {
        // 遮罩需要铺满 owner root（含 padding 环），但 cover 本体被 owner 模板的
        // PixelAlignedBorder 缩进了 Padding + BorderThickness；负 Margin 抵消该缩进。
        var padding = OwnerPadding + OwnerBorderThickness;
        OwnerMaskMargin = new Thickness(-padding.Left, -padding.Top, -padding.Right, -padding.Bottom);
    }
}
