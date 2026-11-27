using AtomUI.Controls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
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

    /// <summary>
    /// Padding of the PixelAlignedBorder in the owner template. It is not used for layout; it only feeds
    /// the negative margin that offsets it.
    /// </summary>
    internal Thickness OwnerPadding
    {
        get => GetValue(OwnerPaddingProperty);
        set => SetValue(OwnerPaddingProperty, value);
    }

    /// <summary>
    /// Border thickness of the PixelAlignedBorder in the owner template. It is not used for layout; it
    /// only feeds the negative margin that offsets it.
    /// </summary>
    internal Thickness OwnerBorderThickness
    {
        get => GetValue(OwnerBorderThicknessProperty);
        set => SetValue(OwnerBorderThicknessProperty, value);
    }

    /// <summary>
    /// Corner radius of the PixelAlignedBorder in the owner template. It is not used for layout: the
    /// mask and the template presenters take their corner radius from it.
    /// </summary>
    /// <remarks>
    /// The antd cover overlay (inset: 0) is clipped to rounded corners by the root's overflow: hidden
    /// and border-radius. The AtomUI mask extends past the owner padding through a negative margin, so
    /// the owner's rounded clip cannot cover it. The value is therefore applied directly to the
    /// CornerRadius of the mask and of the border, loading and error presenters of the template.
    /// </remarks>
    internal CornerRadius OwnerCornerRadius
    {
        get => GetValue(OwnerCornerRadiusProperty);
        set => SetValue(OwnerCornerRadiusProperty, value);
    }

    private Thickness _ownerMaskMargin;

    /// <summary>
    /// Negative margin of the mask layer. It cancels the Padding plus BorderThickness of the owner
    /// template's PixelAlignedBorder (in the ImagePreviewer and ImageGroupPreviewer templates), so the
    /// mask covers the whole root like the antd genImageCoverStyle cover (position: absolute; inset: 0).
    /// </summary>
    /// <remarks>
    /// This is run-time geometry: the owner padding is a user property, so a ControlTheme cannot express
    /// it statically.
    /// </remarks>
    internal Thickness OwnerMaskMargin
    {
        get => _ownerMaskMargin;
        private set => SetAndRaise(OwnerMaskMarginProperty, ref _ownerMaskMargin, value);
    }

    #endregion

    // The previewer terminals (ImagePreviewer, ImageGroupPreviewer) declare these ImageViewer inputs; a cover maps them
    // itself so a cover created alone follows the ImagePreviewer identity instead of the attached-property defaults.
    private static Primitives.PartTokenMapping[]? s_ownerInputs;

    // Built on first use: the input properties live in another partial file, so a static initializer cannot see them as assigned.
    private static Primitives.PartTokenMapping[] OwnerInputs => s_ownerInputs ??=
    [
        new(ImageViewer.LayoutBackgroundProperty, "ColorBgLayout"),
        new(ImageViewer.OverlayMaskBackgroundProperty, "ColorBgMask"),
        new(ImageViewer.ActiveTextBackgroundProperty, "ColorBgTextActive"),
        new(ImageViewer.HoverTextBackgroundProperty, "ColorBgTextHover"),
        new(ImageViewer.TertiaryFillProperty, "ColorFillTertiary"),
        new(ImageViewer.InverseTextForegroundProperty, "ColorTextLightSolid"),
        new(ImageViewer.TertiaryTextForegroundProperty, "ColorTextTertiary"),
        new(ImageViewer.SmallToolbarButtonSizeProperty, "ControlHeightSM"),
        new(ImageViewer.CoverImageSizeProperty, "CoverImageWidth"),
        new(ImageViewer.DialogMinHeightProperty, "DialogMinHeight"),
        new(ImageViewer.DialogMinWidthProperty, "DialogMinWidth"),
        new(ImageViewer.DefaultMotionEnabledProperty, "EnableMotion"),
        new(ImageViewer.ErrorIconSizeProperty, "ErrorIconSize"),
        new(ImageViewer.FloatToolbarIndicatorPaddingProperty, "FloatToolbarIndicatorPadding"),
        new(ImageViewer.FloatToolbarPaddingProperty, "FloatToolbarPadding"),
        new(ImageViewer.IconSizeProperty, "IconSize"),
        new(ImageViewer.LargeIconSizeProperty, "IconSizeLG"),
        new(ImageViewer.MarginSMProperty, "MarginSM"),
        new(ImageViewer.MarginXLProperty, "MarginXL"),
        new(ImageViewer.MaskBackgroundProperty, "MaskBgColor"),
        new(ImageViewer.NavButtonBackgroundProperty, "NavButtonBgColor"),
        new(ImageViewer.NavButtonHoverBackgroundProperty, "NavButtonBgHoverColor"),
        new(ImageViewer.PaddingSMProperty, "PaddingSM"),
        new(ImageViewer.PaddingXSProperty, "PaddingXS"),
        new(ImageViewer.PreviewOperationForegroundProperty, "PreviewOperationColor"),
        new(ImageViewer.DisabledPreviewOperationForegroundProperty, "PreviewOperationColorDisabled"),
        new(ImageViewer.HoverPreviewOperationForegroundProperty, "PreviewOperationHoverColor"),
        new(ImageViewer.PreviewOperationSizeProperty, "PreviewOperationSize"),
        new(ImageViewer.SpacingProperty, "Spacing"),
        new(ImageViewer.SpacingXXSProperty, "SpacingXXS"),
        new(ImageViewer.TitleBarBackgroundProperty, "TitleBarBackgroundColor")
    ];

    // Lives as long as the cover; it only subscribes to the cover itself and to its theme context while attached.
    private readonly IDisposable _ownerInputs;

    public ImagePreviewerCover()
    {
        _ownerInputs = Primitives.PartTokenBinder.Bind(this, null, typeof(ImagePreviewer), OwnerInputs,
            typeof(ImageGroupPreviewer));
    }

    static ImagePreviewerCover()
    {
        // Avalonia's TemplatedControl defaults to ClipToBounds=true (composition-layer clipping). That
        // would clip the mask, which extends beyond the cover bounds through its negative margin (the antd
        // genImageCoverStyle absolute inset: 0), back to the inner area, so the root's padding ring would
        // never be dimmed. Clipping belongs to the owner root's PixelAlignedBorder (the antd root's
        // overflow: hidden and border-radius).
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

    }

    private void UpdateVisualState()
    {
        PseudoClasses.Set(":has-image", ImageSource is not null);
        PseudoClasses.Set(":loading", IsLoading);
        PseudoClasses.Set(":failed", IsFailed);
        PseudoClasses.Set(":loading-skeleton", IsLoading && ImageSource is null && LoadingContent is null);
        SetAndRaise(HasErrorProperty, ref _hasError, IsFailed && ImageSource is null);
        // The mask is the hover operation layer (clicking opens the preview in any loading state). It is
        // decided by IsShowCoverMask alone and stays independent of the loading, image and failure states.
        // Its opacity is left entirely to the theme (0 at rest, 1 on hover): forcing a value in C# would
        // fight the hover style and its transitions and cause flicker.
        SetCurrentValue(IsCoverMaskVisibleProperty, IsShowCoverMask);
    }

    private void UpdateOwnerMaskGeometry()
    {
        // The mask must cover the whole owner root, including the padding ring, but the cover itself is
        // inset by the Padding plus BorderThickness of the owner template's PixelAlignedBorder. The negative
        // margin cancels that inset.
        var padding = OwnerPadding + OwnerBorderThickness;
        OwnerMaskMargin = new Thickness(-padding.Left, -padding.Top, -padding.Right, -padding.Bottom);
    }
}
