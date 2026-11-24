using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls.Internal.DateViewer;

// Concrete inheritable style inputs. Each public terminal supplies its own values in AXAML.
internal partial class DateViewerCell
{
    #region 公共属性定义

    public static readonly AttachedProperty<IBrush?> PanelBackgroundProperty =
        AvaloniaProperty.RegisterAttached<DateViewerCell, StyledElement, IBrush?>("PanelBackground", inherits: true);

    public static IBrush? GetPanelBackground(StyledElement element) => element.GetValue(PanelBackgroundProperty);
    public static void SetPanelBackground(StyledElement element, IBrush? value) => element.SetValue(PanelBackgroundProperty, value);

    public static readonly AttachedProperty<IBrush?> CellHoverBackgroundProperty =
        AvaloniaProperty.RegisterAttached<DateViewerCell, StyledElement, IBrush?>("CellHoverBackground", inherits: true);

    public static IBrush? GetCellHoverBackground(StyledElement element) => element.GetValue(CellHoverBackgroundProperty);
    public static void SetCellHoverBackground(StyledElement element, IBrush? value) => element.SetValue(CellHoverBackgroundProperty, value);

    public static readonly AttachedProperty<IBrush?> ActiveCellForegroundProperty =
        AvaloniaProperty.RegisterAttached<DateViewerCell, StyledElement, IBrush?>("ActiveCellForeground", inherits: true);

    public static IBrush? GetActiveCellForeground(StyledElement element) => element.GetValue(ActiveCellForegroundProperty);
    public static void SetActiveCellForeground(StyledElement element, IBrush? value) => element.SetValue(ActiveCellForegroundProperty, value);

    public static readonly AttachedProperty<IBrush?> RangeCellBackgroundProperty =
        AvaloniaProperty.RegisterAttached<DateViewerCell, StyledElement, IBrush?>("RangeCellBackground", inherits: true);

    public static IBrush? GetRangeCellBackground(StyledElement element) => element.GetValue(RangeCellBackgroundProperty);
    public static void SetRangeCellBackground(StyledElement element, IBrush? value) => element.SetValue(RangeCellBackgroundProperty, value);

    public static readonly AttachedProperty<double> PanelWidthProperty =
        AvaloniaProperty.RegisterAttached<DateViewerCell, StyledElement, double>("PanelWidth", inherits: true);

    public static double GetPanelWidth(StyledElement element) => element.GetValue(PanelWidthProperty);
    public static void SetPanelWidth(StyledElement element, double value) => element.SetValue(PanelWidthProperty, value);

    public static readonly AttachedProperty<double> CellHeightProperty =
        AvaloniaProperty.RegisterAttached<DateViewerCell, StyledElement, double>("CellHeight", inherits: true);

    public static double GetCellHeight(StyledElement element) => element.GetValue(CellHeightProperty);
    public static void SetCellHeight(StyledElement element, double value) => element.SetValue(CellHeightProperty, value);

    public static readonly AttachedProperty<double> CellWidthProperty =
        AvaloniaProperty.RegisterAttached<DateViewerCell, StyledElement, double>("CellWidth", inherits: true);

    public static double GetCellWidth(StyledElement element) => element.GetValue(CellWidthProperty);
    public static void SetCellWidth(StyledElement element, double value) => element.SetValue(CellWidthProperty, value);

    public static readonly AttachedProperty<double> CellRowHeightProperty =
        AvaloniaProperty.RegisterAttached<DateViewerCell, StyledElement, double>("CellRowHeight", inherits: true);

    public static double GetCellRowHeight(StyledElement element) => element.GetValue(CellRowHeightProperty);
    public static void SetCellRowHeight(StyledElement element, double value) => element.SetValue(CellRowHeightProperty, value);

    public static readonly AttachedProperty<double> PeriodCellWidthProperty =
        AvaloniaProperty.RegisterAttached<DateViewerCell, StyledElement, double>("PeriodCellWidth", inherits: true);

    public static double GetPeriodCellWidth(StyledElement element) => element.GetValue(PeriodCellWidthProperty);
    public static void SetPeriodCellWidth(StyledElement element, double value) => element.SetValue(PeriodCellWidthProperty, value);

    public static readonly AttachedProperty<Thickness> PeriodCellPaddingProperty =
        AvaloniaProperty.RegisterAttached<DateViewerCell, StyledElement, Thickness>("PeriodCellPadding", inherits: true);

    public static Thickness GetPeriodCellPadding(StyledElement element) => element.GetValue(PeriodCellPaddingProperty);
    public static void SetPeriodCellPadding(StyledElement element, Thickness value) => element.SetValue(PeriodCellPaddingProperty, value);

    public static readonly AttachedProperty<double> ContentCellMinHeightProperty =
        AvaloniaProperty.RegisterAttached<DateViewerCell, StyledElement, double>("ContentCellMinHeight", inherits: true);

    public static double GetContentCellMinHeight(StyledElement element) => element.GetValue(ContentCellMinHeightProperty);
    public static void SetContentCellMinHeight(StyledElement element, double value) => element.SetValue(ContentCellMinHeightProperty, value);

    public static readonly AttachedProperty<Thickness> ContentCellSurfaceMarginProperty =
        AvaloniaProperty.RegisterAttached<DateViewerCell, StyledElement, Thickness>("ContentCellSurfaceMargin", inherits: true);

    public static Thickness GetContentCellSurfaceMargin(StyledElement element) => element.GetValue(ContentCellSurfaceMarginProperty);
    public static void SetContentCellSurfaceMargin(StyledElement element, Thickness value) => element.SetValue(ContentCellSurfaceMarginProperty, value);

    public static readonly AttachedProperty<Thickness> ContentCellTopBorderThicknessProperty =
        AvaloniaProperty.RegisterAttached<DateViewerCell, StyledElement, Thickness>("ContentCellTopBorderThickness", inherits: true);

    public static Thickness GetContentCellTopBorderThickness(StyledElement element) => element.GetValue(ContentCellTopBorderThicknessProperty);
    public static void SetContentCellTopBorderThickness(StyledElement element, Thickness value) => element.SetValue(ContentCellTopBorderThicknessProperty, value);

    public static readonly AttachedProperty<Thickness> ContentCellValueMarginProperty =
        AvaloniaProperty.RegisterAttached<DateViewerCell, StyledElement, Thickness>("ContentCellValueMargin", inherits: true);

    public static Thickness GetContentCellValueMargin(StyledElement element) => element.GetValue(ContentCellValueMarginProperty);
    public static void SetContentCellValueMargin(StyledElement element, Thickness value) => element.SetValue(ContentCellValueMarginProperty, value);

    public static readonly AttachedProperty<Thickness> ContentCellContentMarginProperty =
        AvaloniaProperty.RegisterAttached<DateViewerCell, StyledElement, Thickness>("ContentCellContentMargin", inherits: true);

    public static Thickness GetContentCellContentMargin(StyledElement element) => element.GetValue(ContentCellContentMarginProperty);
    public static void SetContentCellContentMargin(StyledElement element, Thickness value) => element.SetValue(ContentCellContentMarginProperty, value);

    public static readonly AttachedProperty<Thickness> PanelPaddingProperty =
        AvaloniaProperty.RegisterAttached<DateViewerCell, StyledElement, Thickness>("PanelPadding", inherits: true);

    public static Thickness GetPanelPadding(StyledElement element) => element.GetValue(PanelPaddingProperty);
    public static void SetPanelPadding(StyledElement element, Thickness value) => element.SetValue(PanelPaddingProperty, value);

    public static readonly AttachedProperty<Thickness> HeaderHorizontalPaddingProperty =
        AvaloniaProperty.RegisterAttached<DateViewerCell, StyledElement, Thickness>("HeaderHorizontalPadding", inherits: true);

    public static Thickness GetHeaderHorizontalPadding(StyledElement element) => element.GetValue(HeaderHorizontalPaddingProperty);
    public static void SetHeaderHorizontalPadding(StyledElement element, Thickness value) => element.SetValue(HeaderHorizontalPaddingProperty, value);

    public static readonly AttachedProperty<Thickness> DateBodyPaddingProperty =
        AvaloniaProperty.RegisterAttached<DateViewerCell, StyledElement, Thickness>("DateBodyPadding", inherits: true);

    public static Thickness GetDateBodyPadding(StyledElement element) => element.GetValue(DateBodyPaddingProperty);
    public static void SetDateBodyPadding(StyledElement element, Thickness value) => element.SetValue(DateBodyPaddingProperty, value);

    public static readonly AttachedProperty<Thickness> HeaderBorderThicknessProperty =
        AvaloniaProperty.RegisterAttached<DateViewerCell, StyledElement, Thickness>("HeaderBorderThickness", inherits: true);

    public static Thickness GetHeaderBorderThickness(StyledElement element) => element.GetValue(HeaderBorderThicknessProperty);
    public static void SetHeaderBorderThickness(StyledElement element, Thickness value) => element.SetValue(HeaderBorderThicknessProperty, value);

    public static readonly AttachedProperty<double> WeekNumberOpacityProperty =
        AvaloniaProperty.RegisterAttached<DateViewerCell, StyledElement, double>("WeekNumberOpacity", inherits: true);

    public static double GetWeekNumberOpacity(StyledElement element) => element.GetValue(WeekNumberOpacityProperty);
    public static void SetWeekNumberOpacity(StyledElement element, double value) => element.SetValue(WeekNumberOpacityProperty, value);

    #endregion
}
