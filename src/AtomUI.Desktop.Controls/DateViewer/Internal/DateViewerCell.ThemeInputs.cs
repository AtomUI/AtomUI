using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls.Internal.DateViewer;

// Concrete inheritable style inputs. Each public terminal supplies its own values in AXAML.
internal partial class DateViewerCell
{
    #region 公共属性定义

    public static readonly AttachedProperty<IBrush?> ThemePanelBgProperty =
        AvaloniaProperty.RegisterAttached<DateViewerCell, StyledElement, IBrush?>("ThemePanelBg", inherits: true);

    public static IBrush? GetThemePanelBg(StyledElement element) => element.GetValue(ThemePanelBgProperty);
    public static void SetThemePanelBg(StyledElement element, IBrush? value) => element.SetValue(ThemePanelBgProperty, value);

    public static readonly AttachedProperty<IBrush?> ThemeCellHoverBgProperty =
        AvaloniaProperty.RegisterAttached<DateViewerCell, StyledElement, IBrush?>("ThemeCellHoverBg", inherits: true);

    public static IBrush? GetThemeCellHoverBg(StyledElement element) => element.GetValue(ThemeCellHoverBgProperty);
    public static void SetThemeCellHoverBg(StyledElement element, IBrush? value) => element.SetValue(ThemeCellHoverBgProperty, value);

    public static readonly AttachedProperty<IBrush?> ThemeCellActiveColorProperty =
        AvaloniaProperty.RegisterAttached<DateViewerCell, StyledElement, IBrush?>("ThemeCellActiveColor", inherits: true);

    public static IBrush? GetThemeCellActiveColor(StyledElement element) => element.GetValue(ThemeCellActiveColorProperty);
    public static void SetThemeCellActiveColor(StyledElement element, IBrush? value) => element.SetValue(ThemeCellActiveColorProperty, value);

    public static readonly AttachedProperty<IBrush?> ThemeCellRangeBgProperty =
        AvaloniaProperty.RegisterAttached<DateViewerCell, StyledElement, IBrush?>("ThemeCellRangeBg", inherits: true);

    public static IBrush? GetThemeCellRangeBg(StyledElement element) => element.GetValue(ThemeCellRangeBgProperty);
    public static void SetThemeCellRangeBg(StyledElement element, IBrush? value) => element.SetValue(ThemeCellRangeBgProperty, value);

    public static readonly AttachedProperty<double> ThemePanelWidthProperty =
        AvaloniaProperty.RegisterAttached<DateViewerCell, StyledElement, double>("ThemePanelWidth", inherits: true);

    public static double GetThemePanelWidth(StyledElement element) => element.GetValue(ThemePanelWidthProperty);
    public static void SetThemePanelWidth(StyledElement element, double value) => element.SetValue(ThemePanelWidthProperty, value);

    public static readonly AttachedProperty<double> ThemeCellHeightProperty =
        AvaloniaProperty.RegisterAttached<DateViewerCell, StyledElement, double>("ThemeCellHeight", inherits: true);

    public static double GetThemeCellHeight(StyledElement element) => element.GetValue(ThemeCellHeightProperty);
    public static void SetThemeCellHeight(StyledElement element, double value) => element.SetValue(ThemeCellHeightProperty, value);

    public static readonly AttachedProperty<double> ThemeCellWidthProperty =
        AvaloniaProperty.RegisterAttached<DateViewerCell, StyledElement, double>("ThemeCellWidth", inherits: true);

    public static double GetThemeCellWidth(StyledElement element) => element.GetValue(ThemeCellWidthProperty);
    public static void SetThemeCellWidth(StyledElement element, double value) => element.SetValue(ThemeCellWidthProperty, value);

    public static readonly AttachedProperty<double> ThemeCellRowHeightProperty =
        AvaloniaProperty.RegisterAttached<DateViewerCell, StyledElement, double>("ThemeCellRowHeight", inherits: true);

    public static double GetThemeCellRowHeight(StyledElement element) => element.GetValue(ThemeCellRowHeightProperty);
    public static void SetThemeCellRowHeight(StyledElement element, double value) => element.SetValue(ThemeCellRowHeightProperty, value);

    public static readonly AttachedProperty<double> ThemePeriodCellWidthProperty =
        AvaloniaProperty.RegisterAttached<DateViewerCell, StyledElement, double>("ThemePeriodCellWidth", inherits: true);

    public static double GetThemePeriodCellWidth(StyledElement element) => element.GetValue(ThemePeriodCellWidthProperty);
    public static void SetThemePeriodCellWidth(StyledElement element, double value) => element.SetValue(ThemePeriodCellWidthProperty, value);

    public static readonly AttachedProperty<Thickness> ThemePeriodCellPaddingProperty =
        AvaloniaProperty.RegisterAttached<DateViewerCell, StyledElement, Thickness>("ThemePeriodCellPadding", inherits: true);

    public static Thickness GetThemePeriodCellPadding(StyledElement element) => element.GetValue(ThemePeriodCellPaddingProperty);
    public static void SetThemePeriodCellPadding(StyledElement element, Thickness value) => element.SetValue(ThemePeriodCellPaddingProperty, value);

    public static readonly AttachedProperty<double> ThemeContentCellMinHeightProperty =
        AvaloniaProperty.RegisterAttached<DateViewerCell, StyledElement, double>("ThemeContentCellMinHeight", inherits: true);

    public static double GetThemeContentCellMinHeight(StyledElement element) => element.GetValue(ThemeContentCellMinHeightProperty);
    public static void SetThemeContentCellMinHeight(StyledElement element, double value) => element.SetValue(ThemeContentCellMinHeightProperty, value);

    public static readonly AttachedProperty<Thickness> ThemeContentCellSurfaceMarginProperty =
        AvaloniaProperty.RegisterAttached<DateViewerCell, StyledElement, Thickness>("ThemeContentCellSurfaceMargin", inherits: true);

    public static Thickness GetThemeContentCellSurfaceMargin(StyledElement element) => element.GetValue(ThemeContentCellSurfaceMarginProperty);
    public static void SetThemeContentCellSurfaceMargin(StyledElement element, Thickness value) => element.SetValue(ThemeContentCellSurfaceMarginProperty, value);

    public static readonly AttachedProperty<Thickness> ThemeContentCellTopBorderThicknessProperty =
        AvaloniaProperty.RegisterAttached<DateViewerCell, StyledElement, Thickness>("ThemeContentCellTopBorderThickness", inherits: true);

    public static Thickness GetThemeContentCellTopBorderThickness(StyledElement element) => element.GetValue(ThemeContentCellTopBorderThicknessProperty);
    public static void SetThemeContentCellTopBorderThickness(StyledElement element, Thickness value) => element.SetValue(ThemeContentCellTopBorderThicknessProperty, value);

    public static readonly AttachedProperty<Thickness> ThemeContentCellValueMarginProperty =
        AvaloniaProperty.RegisterAttached<DateViewerCell, StyledElement, Thickness>("ThemeContentCellValueMargin", inherits: true);

    public static Thickness GetThemeContentCellValueMargin(StyledElement element) => element.GetValue(ThemeContentCellValueMarginProperty);
    public static void SetThemeContentCellValueMargin(StyledElement element, Thickness value) => element.SetValue(ThemeContentCellValueMarginProperty, value);

    public static readonly AttachedProperty<Thickness> ThemeContentCellContentMarginProperty =
        AvaloniaProperty.RegisterAttached<DateViewerCell, StyledElement, Thickness>("ThemeContentCellContentMargin", inherits: true);

    public static Thickness GetThemeContentCellContentMargin(StyledElement element) => element.GetValue(ThemeContentCellContentMarginProperty);
    public static void SetThemeContentCellContentMargin(StyledElement element, Thickness value) => element.SetValue(ThemeContentCellContentMarginProperty, value);

    public static readonly AttachedProperty<Thickness> ThemePanelPaddingProperty =
        AvaloniaProperty.RegisterAttached<DateViewerCell, StyledElement, Thickness>("ThemePanelPadding", inherits: true);

    public static Thickness GetThemePanelPadding(StyledElement element) => element.GetValue(ThemePanelPaddingProperty);
    public static void SetThemePanelPadding(StyledElement element, Thickness value) => element.SetValue(ThemePanelPaddingProperty, value);

    public static readonly AttachedProperty<Thickness> ThemeHeaderHorizontalPaddingProperty =
        AvaloniaProperty.RegisterAttached<DateViewerCell, StyledElement, Thickness>("ThemeHeaderHorizontalPadding", inherits: true);

    public static Thickness GetThemeHeaderHorizontalPadding(StyledElement element) => element.GetValue(ThemeHeaderHorizontalPaddingProperty);
    public static void SetThemeHeaderHorizontalPadding(StyledElement element, Thickness value) => element.SetValue(ThemeHeaderHorizontalPaddingProperty, value);

    public static readonly AttachedProperty<Thickness> ThemeDateBodyPaddingProperty =
        AvaloniaProperty.RegisterAttached<DateViewerCell, StyledElement, Thickness>("ThemeDateBodyPadding", inherits: true);

    public static Thickness GetThemeDateBodyPadding(StyledElement element) => element.GetValue(ThemeDateBodyPaddingProperty);
    public static void SetThemeDateBodyPadding(StyledElement element, Thickness value) => element.SetValue(ThemeDateBodyPaddingProperty, value);

    public static readonly AttachedProperty<Thickness> ThemeHeaderBorderThicknessProperty =
        AvaloniaProperty.RegisterAttached<DateViewerCell, StyledElement, Thickness>("ThemeHeaderBorderThickness", inherits: true);

    public static Thickness GetThemeHeaderBorderThickness(StyledElement element) => element.GetValue(ThemeHeaderBorderThicknessProperty);
    public static void SetThemeHeaderBorderThickness(StyledElement element, Thickness value) => element.SetValue(ThemeHeaderBorderThicknessProperty, value);

    public static readonly AttachedProperty<double> ThemeWeekNumberOpacityProperty =
        AvaloniaProperty.RegisterAttached<DateViewerCell, StyledElement, double>("ThemeWeekNumberOpacity", inherits: true);

    public static double GetThemeWeekNumberOpacity(StyledElement element) => element.GetValue(ThemeWeekNumberOpacityProperty);
    public static void SetThemeWeekNumberOpacity(StyledElement element, double value) => element.SetValue(ThemeWeekNumberOpacityProperty, value);

    #endregion
}
