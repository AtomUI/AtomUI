using System.Globalization;
using AtomUI.Controls.Utils;
using AtomUI.Desktop.Controls.Localization;
using AtomUI.Media;
using AtomUI.Desktop.Controls.Internal.DateViewer;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

internal static class DatePickerFormattingHelper
{
    internal static CultureInfo GetFormattingCulture() => Application.Current is { } application
        ? global::AtomUI.ApplicationExtensions.GetLanguageManager(application)?.Current.FormattingCulture ?? CultureInfo.CurrentCulture
        : CultureInfo.CurrentCulture;
    private const string AntDesignDefaultDatePickerInputWidthReferenceText = "Select quarter";

    internal static string GetEffectiveFormat(string? format, bool isShowTime, ClockIdentifierType clockIdentifier)
    {
        return GetEffectiveFormat(format, DatePickerMode.Date, isShowTime, clockIdentifier);
    }

    internal static string GetEffectiveFormat(
        string? format,
        DatePickerMode pickerMode,
        bool isShowTime,
        ClockIdentifierType clockIdentifier)
    {
        if (format is not null)
        {
            return format;
        }

        if (pickerMode != DatePickerMode.Date)
        {
            return pickerMode switch
            {
                DatePickerMode.Week    => "yyyy-ww周",
                DatePickerMode.Month   => "yyyy-MM",
                DatePickerMode.Quarter => "yyyy-'Q'q",
                DatePickerMode.Year    => "yyyy",
                _                      => "yyyy-MM-dd"
            };
        }

        var effectiveFormat = "yyyy-MM-dd";
        if (!isShowTime)
        {
            return effectiveFormat;
        }

        return clockIdentifier == ClockIdentifierType.HourClock12
            ? $"{effectiveFormat} hh:mm:ss tt"
            : $"{effectiveFormat} HH:mm:ss";
    }

    internal static DateTimeFormatInfo? CreateFormatInfo(
        ClockIdentifierType clockIdentifier,
        string? amText,
        string? pmText)
    {
        if (clockIdentifier != ClockIdentifierType.HourClock12)
        {
            return null;
        }

        var localizer = Application.Current is { } application
            ? global::AtomUI.ApplicationExtensions.GetLocalizer(application)
            : null;
        var amDesignator = amText ?? localizer?.Get(TimePickerLangResourceKind.AMText);
        var pmDesignator = pmText ?? localizer?.Get(TimePickerLangResourceKind.PMText);
        if (amDesignator is null || pmDesignator is null)
        {
            return null;
        }

        return new DateTimeFormatInfo
        {
            AMDesignator = amDesignator,
            PMDesignator = pmDesignator
        };
    }

    internal static string FormatDateTime(DateTime dateTime, string format, DateTimeFormatInfo? formatInfo)
    {
        return formatInfo is null
            ? dateTime.ToString(format)
            : dateTime.ToString(format, formatInfo);
    }

    internal static string FormatDateTime(
        DateTime dateTime,
        string? format,
        DatePickerMode pickerMode,
        bool isShowTime,
        ClockIdentifierType clockIdentifier,
        DateTimeFormatInfo? formatInfo)
    {
        if (format is not null || pickerMode == DatePickerMode.Date)
        {
            return FormatDateTime(dateTime, GetEffectiveFormat(format, pickerMode, isShowTime, clockIdentifier), formatInfo);
        }

        return pickerMode switch
        {
            DatePickerMode.Week    => FormatWeek(dateTime),
            DatePickerMode.Month   => dateTime.ToString("yyyy-MM", CultureInfo.InvariantCulture),
            DatePickerMode.Quarter => FormatQuarter(dateTime),
            DatePickerMode.Year    => dateTime.ToString("yyyy", CultureInfo.InvariantCulture),
            _                      => FormatDateTime(dateTime, "yyyy-MM-dd", formatInfo)
        };
    }

    internal static bool IsFormattedTextAffectingProperty(
        AvaloniaProperty property,
        AvaloniaProperty isShowTimeProperty,
        AvaloniaProperty formatProperty,
        AvaloniaProperty? pickerModeProperty,
        AvaloniaProperty clockIdentifierProperty,
        AvaloniaProperty amTextProperty,
        AvaloniaProperty pmTextProperty)
    {
        return property == isShowTimeProperty ||
               property == formatProperty ||
               property == pickerModeProperty ||
               property == clockIdentifierProperty ||
               property == amTextProperty ||
               property == pmTextProperty;
    }

    internal static bool IsPreferredWidthAffectingProperty(
        AvaloniaProperty property,
        AvaloniaProperty fontSizeProperty,
        AvaloniaProperty fontFamilyProperty,
        AvaloniaProperty fontStyleProperty,
        AvaloniaProperty fontWeightProperty,
        AvaloniaProperty sizeTypeProperty,
        AvaloniaProperty minWidthProperty,
        AvaloniaProperty widthProperty,
        AvaloniaProperty maxWidthProperty,
        AvaloniaProperty horizontalAlignmentProperty)
    {
        return property == fontSizeProperty ||
               property == fontFamilyProperty ||
               property == fontStyleProperty ||
               property == fontWeightProperty ||
               property == sizeTypeProperty ||
               property == minWidthProperty ||
               property == widthProperty ||
               property == maxWidthProperty ||
               property == horizontalAlignmentProperty;
    }

    internal static double CalculatePreferredInputWidth(
        string format,
        double fontSize,
        FontFamily fontFamily,
        FontStyle fontStyle,
        FontWeight fontWeight,
        DateTimeFormatInfo? formatInfo)
    {
        return DateTimeUtils.CalculateWidestFormattedDateTimeSize(
            format, fontSize, fontFamily, fontStyle, fontWeight, formatInfo).Width;
    }

    internal static double CalculatePreferredInputWidth(
        string? format,
        DatePickerMode pickerMode,
        bool isShowTime,
        ClockIdentifierType clockIdentifier,
        double fontSize,
        FontFamily fontFamily,
        FontStyle fontStyle,
        FontWeight fontWeight,
        DateTimeFormatInfo? formatInfo)
    {
        var widthReserveFormat = format is null && pickerMode != DatePickerMode.Date
            ? GetEffectiveFormat(null, DatePickerMode.Date, false, clockIdentifier)
            : GetEffectiveFormat(format, pickerMode, isShowTime, clockIdentifier);
        var preferredWidth = DateTimeUtils.CalculateWidestFormattedDateTimeSize(
            widthReserveFormat,
            fontSize,
            fontFamily,
            fontStyle,
            fontWeight,
            formatInfo).Width;
        if (format is null)
        {
            preferredWidth = Math.Max(
                preferredWidth,
                CalculateAntDesignDefaultDatePickerInputBaselineWidth(fontSize, fontFamily, fontStyle, fontWeight));
        }

        return preferredWidth;
    }

    private static double CalculateAntDesignDefaultDatePickerInputBaselineWidth(
        double fontSize,
        FontFamily fontFamily,
        FontStyle fontStyle,
        FontWeight fontWeight)
    {
        return CalculateAntDesignInputBaselineWidth(
            fontSize,
            fontFamily,
            fontStyle,
            fontWeight,
            AntDesignDefaultDatePickerInputWidthReferenceText);
    }

    internal static double CalculateAntDesignInputBaselineWidth(
        double fontSize,
        FontFamily fontFamily,
        FontStyle fontStyle,
        FontWeight fontWeight,
        params string[] referenceTexts)
    {
        var preferredWidth = 0d;
        foreach (var referenceText in referenceTexts)
        {
            preferredWidth = Math.Max(preferredWidth,
                TextUtils.CalculateTextSize(
                    referenceText,
                    fontSize,
                    fontFamily,
                    fontStyle,
                    fontWeight).Width);
        }

        return preferredWidth;
    }

    internal static double CalculateBoundedPreferredInputWidth(
        string? format,
        DatePickerMode pickerMode,
        bool isShowTime,
        ClockIdentifierType clockIdentifier,
        double fontSize,
        FontFamily fontFamily,
        FontStyle fontStyle,
        FontWeight fontWeight,
        double minWidth,
        double maxWidth,
        DateTimeFormatInfo? formatInfo)
    {
        var preferredWidth = CalculatePreferredInputWidth(
            format,
            pickerMode,
            isShowTime,
            clockIdentifier,
            fontSize,
            fontFamily,
            fontStyle,
            fontWeight,
            formatInfo);

        return ApplyWidthBounds(preferredWidth, minWidth, maxWidth);
    }

    internal static double CalculateBoundedRangePreferredInputWidth(
        string? format,
        DatePickerMode pickerMode,
        bool isShowTime,
        ClockIdentifierType clockIdentifier,
        double fontSize,
        FontFamily fontFamily,
        FontStyle fontStyle,
        FontWeight fontWeight,
        double minWidth,
        double maxWidth,
        DateTimeFormatInfo? formatInfo)
    {
        var preferredWidth = CalculatePreferredInputWidth(
            format,
            pickerMode,
            isShowTime,
            clockIdentifier,
            fontSize,
            fontFamily,
            fontStyle,
            fontWeight,
            formatInfo);

        return ApplyWidthBounds(preferredWidth, minWidth, maxWidth);
    }

    internal static DateTime NormalizeDateTime(DateTime dateTime, DatePickerMode pickerMode, DayOfWeek? firstDayOfWeek = null)
    {
        var date = dateTime.Date;
        return pickerMode switch
        {
            DatePickerMode.Week    => GetWeekStart(date, firstDayOfWeek ?? DatePanelAlgorithms.GetWeekFirstDay(GetFormattingCulture())),
            DatePickerMode.Month   => new DateTime(date.Year, date.Month, 1),
            DatePickerMode.Quarter => new DateTime(date.Year, ((date.Month - 1) / 3 * 3) + 1, 1),
            DatePickerMode.Year    => new DateTime(date.Year, 1, 1),
            _                      => date
        };
    }

    internal static bool IsSamePickerUnit(DateTime first, DateTime second, DatePickerMode pickerMode)
    {
        return pickerMode switch
        {
            DatePickerMode.Week    => NormalizeDateTime(first, pickerMode) == NormalizeDateTime(second, pickerMode),
            DatePickerMode.Month   => first.Year == second.Year && first.Month == second.Month,
            DatePickerMode.Quarter => first.Year == second.Year && ((first.Month - 1) / 3) == ((second.Month - 1) / 3),
            DatePickerMode.Year    => first.Year == second.Year,
            _                      => first.Date == second.Date
        };
    }

    private static string FormatWeek(DateTime dateTime)
    {
        var culture = GetFormattingCulture();
        var (year, week) = DatePanelAlgorithms.GetWeekIdentity(dateTime, culture, DatePanelAlgorithms.GetWeekFirstDay(culture));
        var suffix = culture.TwoLetterISOLanguageName switch
        {
            "zh" when culture.Name.EndsWith("-TW", StringComparison.OrdinalIgnoreCase) || culture.Name.Equals("zh-Hant", StringComparison.OrdinalIgnoreCase) => "週",
            "zh" => "周",
            "en" when week % 100 is >= 11 and <= 13 => "th",
            "en" => (week % 10) switch { 1 => "st", 2 => "nd", 3 => "rd", _ => "th" },
            "pt" => "º",
            _ => string.Empty
        };
        return string.Create(CultureInfo.InvariantCulture, $"{year:D4}-{week}{suffix}");
    }

    private static string FormatQuarter(DateTime dateTime)
    {
        var quarter = ((dateTime.Month - 1) / 3) + 1;
        return string.Create(CultureInfo.InvariantCulture, $"{dateTime.Year:D4}-Q{quarter}");
    }

    private static DateTime GetWeekStart(DateTime date, DayOfWeek firstDayOfWeek)
    {
        var offset = ((int)date.DayOfWeek - (int)firstDayOfWeek + 7) % 7;
        return new DateTime(Math.Max(DateTime.MinValue.Ticks, date.Ticks - offset * TimeSpan.TicksPerDay));
    }

    private static double ApplyWidthBounds(double preferredWidth, double minWidth, double maxWidth)
    {
        if (!double.IsNaN(minWidth))
        {
            preferredWidth = Math.Max(minWidth, preferredWidth);
        }

        if (!double.IsNaN(maxWidth))
        {
            preferredWidth = Math.Min(maxWidth, preferredWidth);
        }

        return preferredWidth;
    }
}
