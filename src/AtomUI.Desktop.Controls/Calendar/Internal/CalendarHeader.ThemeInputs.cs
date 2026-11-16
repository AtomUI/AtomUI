using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls.Internal.Calendar;

// Concrete inheritable style inputs. Each public terminal supplies its own values in AXAML.
internal partial class CalendarHeader
{
    #region 公共属性定义

    public static readonly AttachedProperty<IBrush?> ThemeFullBgProperty =
        AvaloniaProperty.RegisterAttached<CalendarHeader, StyledElement, IBrush?>("ThemeFullBg", inherits: true);

    public static IBrush? GetThemeFullBg(StyledElement element) => element.GetValue(ThemeFullBgProperty);
    public static void SetThemeFullBg(StyledElement element, IBrush? value) => element.SetValue(ThemeFullBgProperty, value);

    public static readonly AttachedProperty<IBrush?> ThemeFullPanelBgProperty =
        AvaloniaProperty.RegisterAttached<CalendarHeader, StyledElement, IBrush?>("ThemeFullPanelBg", inherits: true);

    public static IBrush? GetThemeFullPanelBg(StyledElement element) => element.GetValue(ThemeFullPanelBgProperty);
    public static void SetThemeFullPanelBg(StyledElement element, IBrush? value) => element.SetValue(ThemeFullPanelBgProperty, value);

    public static readonly AttachedProperty<IBrush?> ThemeItemActiveColorProperty =
        AvaloniaProperty.RegisterAttached<CalendarHeader, StyledElement, IBrush?>("ThemeItemActiveColor", inherits: true);

    public static IBrush? GetThemeItemActiveColor(StyledElement element) => element.GetValue(ThemeItemActiveColorProperty);
    public static void SetThemeItemActiveColor(StyledElement element, IBrush? value) => element.SetValue(ThemeItemActiveColorProperty, value);

    public static readonly AttachedProperty<IBrush?> ThemeItemActiveBgProperty =
        AvaloniaProperty.RegisterAttached<CalendarHeader, StyledElement, IBrush?>("ThemeItemActiveBg", inherits: true);

    public static IBrush? GetThemeItemActiveBg(StyledElement element) => element.GetValue(ThemeItemActiveBgProperty);
    public static void SetThemeItemActiveBg(StyledElement element, IBrush? value) => element.SetValue(ThemeItemActiveBgProperty, value);

    public static readonly AttachedProperty<double> ThemeYearControlWidthProperty =
        AvaloniaProperty.RegisterAttached<CalendarHeader, StyledElement, double>("ThemeYearControlWidth", inherits: true);

    public static double GetThemeYearControlWidth(StyledElement element) => element.GetValue(ThemeYearControlWidthProperty);
    public static void SetThemeYearControlWidth(StyledElement element, double value) => element.SetValue(ThemeYearControlWidthProperty, value);

    public static readonly AttachedProperty<double> ThemeMonthControlWidthProperty =
        AvaloniaProperty.RegisterAttached<CalendarHeader, StyledElement, double>("ThemeMonthControlWidth", inherits: true);

    public static double GetThemeMonthControlWidth(StyledElement element) => element.GetValue(ThemeMonthControlWidthProperty);
    public static void SetThemeMonthControlWidth(StyledElement element, double value) => element.SetValue(ThemeMonthControlWidthProperty, value);

    public static readonly AttachedProperty<double> ThemeYearMonthCellWidthProperty =
        AvaloniaProperty.RegisterAttached<CalendarHeader, StyledElement, double>("ThemeYearMonthCellWidth", inherits: true);

    public static double GetThemeYearMonthCellWidth(StyledElement element) => element.GetValue(ThemeYearMonthCellWidthProperty);
    public static void SetThemeYearMonthCellWidth(StyledElement element, double value) => element.SetValue(ThemeYearMonthCellWidthProperty, value);

    public static readonly AttachedProperty<double> ThemeWeekHeaderHeightProperty =
        AvaloniaProperty.RegisterAttached<CalendarHeader, StyledElement, double>("ThemeWeekHeaderHeight", inherits: true);

    public static double GetThemeWeekHeaderHeight(StyledElement element) => element.GetValue(ThemeWeekHeaderHeightProperty);
    public static void SetThemeWeekHeaderHeight(StyledElement element, double value) => element.SetValue(ThemeWeekHeaderHeightProperty, value);

    public static readonly AttachedProperty<Thickness> ThemeWeekHeaderPaddingProperty =
        AvaloniaProperty.RegisterAttached<CalendarHeader, StyledElement, Thickness>("ThemeWeekHeaderPadding", inherits: true);

    public static Thickness GetThemeWeekHeaderPadding(StyledElement element) => element.GetValue(ThemeWeekHeaderPaddingProperty);
    public static void SetThemeWeekHeaderPadding(StyledElement element, Thickness value) => element.SetValue(ThemeWeekHeaderPaddingProperty, value);

    public static readonly AttachedProperty<Thickness> ThemeFullCellMarginProperty =
        AvaloniaProperty.RegisterAttached<CalendarHeader, StyledElement, Thickness>("ThemeFullCellMargin", inherits: true);

    public static Thickness GetThemeFullCellMargin(StyledElement element) => element.GetValue(ThemeFullCellMarginProperty);
    public static void SetThemeFullCellMargin(StyledElement element, Thickness value) => element.SetValue(ThemeFullCellMarginProperty, value);

    public static readonly AttachedProperty<Thickness> ThemeFullCellPaddingProperty =
        AvaloniaProperty.RegisterAttached<CalendarHeader, StyledElement, Thickness>("ThemeFullCellPadding", inherits: true);

    public static Thickness GetThemeFullCellPadding(StyledElement element) => element.GetValue(ThemeFullCellPaddingProperty);
    public static void SetThemeFullCellPadding(StyledElement element, Thickness value) => element.SetValue(ThemeFullCellPaddingProperty, value);

    public static readonly AttachedProperty<Thickness> ThemeFullCellBorderThicknessProperty =
        AvaloniaProperty.RegisterAttached<CalendarHeader, StyledElement, Thickness>("ThemeFullCellBorderThickness", inherits: true);

    public static Thickness GetThemeFullCellBorderThickness(StyledElement element) => element.GetValue(ThemeFullCellBorderThicknessProperty);
    public static void SetThemeFullCellBorderThickness(StyledElement element, Thickness value) => element.SetValue(ThemeFullCellBorderThicknessProperty, value);

    public static readonly AttachedProperty<double> ThemeRangeBarHeightProperty =
        AvaloniaProperty.RegisterAttached<CalendarHeader, StyledElement, double>("ThemeRangeBarHeight", inherits: true);

    public static double GetThemeRangeBarHeight(StyledElement element) => element.GetValue(ThemeRangeBarHeightProperty);
    public static void SetThemeRangeBarHeight(StyledElement element, double value) => element.SetValue(ThemeRangeBarHeightProperty, value);

    public static readonly AttachedProperty<double> ThemeRangeBarGapProperty =
        AvaloniaProperty.RegisterAttached<CalendarHeader, StyledElement, double>("ThemeRangeBarGap", inherits: true);

    public static double GetThemeRangeBarGap(StyledElement element) => element.GetValue(ThemeRangeBarGapProperty);
    public static void SetThemeRangeBarGap(StyledElement element, double value) => element.SetValue(ThemeRangeBarGapProperty, value);

    public static readonly AttachedProperty<Thickness> ThemeRangeBarLabelPaddingProperty =
        AvaloniaProperty.RegisterAttached<CalendarHeader, StyledElement, Thickness>("ThemeRangeBarLabelPadding", inherits: true);

    public static Thickness GetThemeRangeBarLabelPadding(StyledElement element) => element.GetValue(ThemeRangeBarLabelPaddingProperty);
    public static void SetThemeRangeBarLabelPadding(StyledElement element, Thickness value) => element.SetValue(ThemeRangeBarLabelPaddingProperty, value);

    #endregion
}
