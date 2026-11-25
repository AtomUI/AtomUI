using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

// Concrete inheritable style inputs. Each public terminal supplies its own values in AXAML.
internal partial class ImageViewer
{
    #region 公共属性定义

    public static readonly AttachedProperty<IBrush?> LayoutBackgroundProperty =
        AvaloniaProperty.RegisterAttached<ImageViewer, StyledElement, IBrush?>("LayoutBackground", inherits: true);

    public static IBrush? GetLayoutBackground(StyledElement element) => element.GetValue(LayoutBackgroundProperty);
    public static void SetLayoutBackground(StyledElement element, IBrush? value) => element.SetValue(LayoutBackgroundProperty, value);

    public static readonly AttachedProperty<IBrush?> OverlayMaskBackgroundProperty =
        AvaloniaProperty.RegisterAttached<ImageViewer, StyledElement, IBrush?>("OverlayMaskBackground", inherits: true);

    public static IBrush? GetOverlayMaskBackground(StyledElement element) => element.GetValue(OverlayMaskBackgroundProperty);
    public static void SetOverlayMaskBackground(StyledElement element, IBrush? value) => element.SetValue(OverlayMaskBackgroundProperty, value);

    public static readonly AttachedProperty<IBrush?> ActiveTextBackgroundProperty =
        AvaloniaProperty.RegisterAttached<ImageViewer, StyledElement, IBrush?>("ActiveTextBackground", inherits: true);

    public static IBrush? GetActiveTextBackground(StyledElement element) => element.GetValue(ActiveTextBackgroundProperty);
    public static void SetActiveTextBackground(StyledElement element, IBrush? value) => element.SetValue(ActiveTextBackgroundProperty, value);

    public static readonly AttachedProperty<IBrush?> HoverTextBackgroundProperty =
        AvaloniaProperty.RegisterAttached<ImageViewer, StyledElement, IBrush?>("HoverTextBackground", inherits: true);

    public static IBrush? GetHoverTextBackground(StyledElement element) => element.GetValue(HoverTextBackgroundProperty);
    public static void SetHoverTextBackground(StyledElement element, IBrush? value) => element.SetValue(HoverTextBackgroundProperty, value);

    public static readonly AttachedProperty<IBrush?> TertiaryFillProperty =
        AvaloniaProperty.RegisterAttached<ImageViewer, StyledElement, IBrush?>("TertiaryFill", inherits: true);

    public static IBrush? GetTertiaryFill(StyledElement element) => element.GetValue(TertiaryFillProperty);
    public static void SetTertiaryFill(StyledElement element, IBrush? value) => element.SetValue(TertiaryFillProperty, value);

    public static readonly AttachedProperty<IBrush?> InverseTextForegroundProperty =
        AvaloniaProperty.RegisterAttached<ImageViewer, StyledElement, IBrush?>("InverseTextForeground", inherits: true);

    public static IBrush? GetInverseTextForeground(StyledElement element) => element.GetValue(InverseTextForegroundProperty);
    public static void SetInverseTextForeground(StyledElement element, IBrush? value) => element.SetValue(InverseTextForegroundProperty, value);

    public static readonly AttachedProperty<IBrush?> TertiaryTextForegroundProperty =
        AvaloniaProperty.RegisterAttached<ImageViewer, StyledElement, IBrush?>("TertiaryTextForeground", inherits: true);

    public static IBrush? GetTertiaryTextForeground(StyledElement element) => element.GetValue(TertiaryTextForegroundProperty);
    public static void SetTertiaryTextForeground(StyledElement element, IBrush? value) => element.SetValue(TertiaryTextForegroundProperty, value);

    public static readonly AttachedProperty<double> SmallToolbarButtonSizeProperty =
        AvaloniaProperty.RegisterAttached<ImageViewer, StyledElement, double>("SmallToolbarButtonSize", inherits: true);

    public static double GetSmallToolbarButtonSize(StyledElement element) => element.GetValue(SmallToolbarButtonSizeProperty);
    public static void SetSmallToolbarButtonSize(StyledElement element, double value) => element.SetValue(SmallToolbarButtonSizeProperty, value);

    public static readonly AttachedProperty<double> CoverImageSizeProperty =
        AvaloniaProperty.RegisterAttached<ImageViewer, StyledElement, double>("CoverImageSize", inherits: true);

    public static double GetCoverImageSize(StyledElement element) => element.GetValue(CoverImageSizeProperty);
    public static void SetCoverImageSize(StyledElement element, double value) => element.SetValue(CoverImageSizeProperty, value);

    public static readonly AttachedProperty<double> DialogMinHeightProperty =
        AvaloniaProperty.RegisterAttached<ImageViewer, StyledElement, double>("DialogMinHeight", inherits: true);

    public static double GetDialogMinHeight(StyledElement element) => element.GetValue(DialogMinHeightProperty);
    public static void SetDialogMinHeight(StyledElement element, double value) => element.SetValue(DialogMinHeightProperty, value);

    public static readonly AttachedProperty<double> DialogMinWidthProperty =
        AvaloniaProperty.RegisterAttached<ImageViewer, StyledElement, double>("DialogMinWidth", inherits: true);

    public static double GetDialogMinWidth(StyledElement element) => element.GetValue(DialogMinWidthProperty);
    public static void SetDialogMinWidth(StyledElement element, double value) => element.SetValue(DialogMinWidthProperty, value);

    public static readonly AttachedProperty<bool> DefaultMotionEnabledProperty =
        AvaloniaProperty.RegisterAttached<ImageViewer, StyledElement, bool>("DefaultMotionEnabled", inherits: true);

    public static bool GetDefaultMotionEnabled(StyledElement element) => element.GetValue(DefaultMotionEnabledProperty);
    public static void SetDefaultMotionEnabled(StyledElement element, bool value) => element.SetValue(DefaultMotionEnabledProperty, value);

    public static readonly AttachedProperty<double> ErrorIconSizeProperty =
        AvaloniaProperty.RegisterAttached<ImageViewer, StyledElement, double>("ErrorIconSize", inherits: true);

    public static double GetErrorIconSize(StyledElement element) => element.GetValue(ErrorIconSizeProperty);
    public static void SetErrorIconSize(StyledElement element, double value) => element.SetValue(ErrorIconSizeProperty, value);

    public static readonly AttachedProperty<Thickness> FloatToolbarIndicatorPaddingProperty =
        AvaloniaProperty.RegisterAttached<ImageViewer, StyledElement, Thickness>("FloatToolbarIndicatorPadding", inherits: true);

    public static Thickness GetFloatToolbarIndicatorPadding(StyledElement element) => element.GetValue(FloatToolbarIndicatorPaddingProperty);
    public static void SetFloatToolbarIndicatorPadding(StyledElement element, Thickness value) => element.SetValue(FloatToolbarIndicatorPaddingProperty, value);

    public static readonly AttachedProperty<Thickness> FloatToolbarPaddingProperty =
        AvaloniaProperty.RegisterAttached<ImageViewer, StyledElement, Thickness>("FloatToolbarPadding", inherits: true);

    public static Thickness GetFloatToolbarPadding(StyledElement element) => element.GetValue(FloatToolbarPaddingProperty);
    public static void SetFloatToolbarPadding(StyledElement element, Thickness value) => element.SetValue(FloatToolbarPaddingProperty, value);

    public static readonly AttachedProperty<double> IconSizeProperty =
        AvaloniaProperty.RegisterAttached<ImageViewer, StyledElement, double>("IconSize", inherits: true);

    public static double GetIconSize(StyledElement element) => element.GetValue(IconSizeProperty);
    public static void SetIconSize(StyledElement element, double value) => element.SetValue(IconSizeProperty, value);

    public static readonly AttachedProperty<double> LargeIconSizeProperty =
        AvaloniaProperty.RegisterAttached<ImageViewer, StyledElement, double>("LargeIconSize", inherits: true);

    public static double GetLargeIconSize(StyledElement element) => element.GetValue(LargeIconSizeProperty);
    public static void SetLargeIconSize(StyledElement element, double value) => element.SetValue(LargeIconSizeProperty, value);

    public static readonly AttachedProperty<Thickness> MarginSMProperty =
        AvaloniaProperty.RegisterAttached<ImageViewer, StyledElement, Thickness>("MarginSM", inherits: true);

    public static Thickness GetMarginSM(StyledElement element) => element.GetValue(MarginSMProperty);
    public static void SetMarginSM(StyledElement element, Thickness value) => element.SetValue(MarginSMProperty, value);

    public static readonly AttachedProperty<Thickness> MarginXLProperty =
        AvaloniaProperty.RegisterAttached<ImageViewer, StyledElement, Thickness>("MarginXL", inherits: true);

    public static Thickness GetMarginXL(StyledElement element) => element.GetValue(MarginXLProperty);
    public static void SetMarginXL(StyledElement element, Thickness value) => element.SetValue(MarginXLProperty, value);

    public static readonly AttachedProperty<IBrush?> MaskBackgroundProperty =
        AvaloniaProperty.RegisterAttached<ImageViewer, StyledElement, IBrush?>("MaskBackground", inherits: true);

    public static IBrush? GetMaskBackground(StyledElement element) => element.GetValue(MaskBackgroundProperty);
    public static void SetMaskBackground(StyledElement element, IBrush? value) => element.SetValue(MaskBackgroundProperty, value);

    public static readonly AttachedProperty<IBrush?> NavButtonBackgroundProperty =
        AvaloniaProperty.RegisterAttached<ImageViewer, StyledElement, IBrush?>("NavButtonBackground", inherits: true);

    public static IBrush? GetNavButtonBackground(StyledElement element) => element.GetValue(NavButtonBackgroundProperty);
    public static void SetNavButtonBackground(StyledElement element, IBrush? value) => element.SetValue(NavButtonBackgroundProperty, value);

    public static readonly AttachedProperty<IBrush?> NavButtonHoverBackgroundProperty =
        AvaloniaProperty.RegisterAttached<ImageViewer, StyledElement, IBrush?>("NavButtonHoverBackground", inherits: true);

    public static IBrush? GetNavButtonHoverBackground(StyledElement element) => element.GetValue(NavButtonHoverBackgroundProperty);
    public static void SetNavButtonHoverBackground(StyledElement element, IBrush? value) => element.SetValue(NavButtonHoverBackgroundProperty, value);

    public static readonly AttachedProperty<Thickness> PaddingSMProperty =
        AvaloniaProperty.RegisterAttached<ImageViewer, StyledElement, Thickness>("PaddingSM", inherits: true);

    public static Thickness GetPaddingSM(StyledElement element) => element.GetValue(PaddingSMProperty);
    public static void SetPaddingSM(StyledElement element, Thickness value) => element.SetValue(PaddingSMProperty, value);

    public static readonly AttachedProperty<Thickness> PaddingXSProperty =
        AvaloniaProperty.RegisterAttached<ImageViewer, StyledElement, Thickness>("PaddingXS", inherits: true);

    public static Thickness GetPaddingXS(StyledElement element) => element.GetValue(PaddingXSProperty);
    public static void SetPaddingXS(StyledElement element, Thickness value) => element.SetValue(PaddingXSProperty, value);

    public static readonly AttachedProperty<IBrush?> PreviewOperationForegroundProperty =
        AvaloniaProperty.RegisterAttached<ImageViewer, StyledElement, IBrush?>("PreviewOperationForeground", inherits: true);

    public static IBrush? GetPreviewOperationForeground(StyledElement element) => element.GetValue(PreviewOperationForegroundProperty);
    public static void SetPreviewOperationForeground(StyledElement element, IBrush? value) => element.SetValue(PreviewOperationForegroundProperty, value);

    public static readonly AttachedProperty<IBrush?> DisabledPreviewOperationForegroundProperty =
        AvaloniaProperty.RegisterAttached<ImageViewer, StyledElement, IBrush?>("DisabledPreviewOperationForeground", inherits: true);

    public static IBrush? GetDisabledPreviewOperationForeground(StyledElement element) => element.GetValue(DisabledPreviewOperationForegroundProperty);
    public static void SetDisabledPreviewOperationForeground(StyledElement element, IBrush? value) => element.SetValue(DisabledPreviewOperationForegroundProperty, value);

    public static readonly AttachedProperty<IBrush?> HoverPreviewOperationForegroundProperty =
        AvaloniaProperty.RegisterAttached<ImageViewer, StyledElement, IBrush?>("HoverPreviewOperationForeground", inherits: true);

    public static IBrush? GetHoverPreviewOperationForeground(StyledElement element) => element.GetValue(HoverPreviewOperationForegroundProperty);
    public static void SetHoverPreviewOperationForeground(StyledElement element, IBrush? value) => element.SetValue(HoverPreviewOperationForegroundProperty, value);

    public static readonly AttachedProperty<double> PreviewOperationSizeProperty =
        AvaloniaProperty.RegisterAttached<ImageViewer, StyledElement, double>("PreviewOperationSize", inherits: true);

    public static double GetPreviewOperationSize(StyledElement element) => element.GetValue(PreviewOperationSizeProperty);
    public static void SetPreviewOperationSize(StyledElement element, double value) => element.SetValue(PreviewOperationSizeProperty, value);

    public static readonly AttachedProperty<double> SpacingProperty =
        AvaloniaProperty.RegisterAttached<ImageViewer, StyledElement, double>("Spacing", inherits: true);

    public static double GetSpacing(StyledElement element) => element.GetValue(SpacingProperty);
    public static void SetSpacing(StyledElement element, double value) => element.SetValue(SpacingProperty, value);

    public static readonly AttachedProperty<double> SpacingXXSProperty =
        AvaloniaProperty.RegisterAttached<ImageViewer, StyledElement, double>("SpacingXXS", inherits: true);

    public static double GetSpacingXXS(StyledElement element) => element.GetValue(SpacingXXSProperty);
    public static void SetSpacingXXS(StyledElement element, double value) => element.SetValue(SpacingXXSProperty, value);

    public static readonly AttachedProperty<IBrush?> TitleBarBackgroundProperty =
        AvaloniaProperty.RegisterAttached<ImageViewer, StyledElement, IBrush?>("TitleBarBackground", inherits: true);

    public static IBrush? GetTitleBarBackground(StyledElement element) => element.GetValue(TitleBarBackgroundProperty);
    public static void SetTitleBarBackground(StyledElement element, IBrush? value) => element.SetValue(TitleBarBackgroundProperty, value);

    #endregion
}
