using AtomUI.Controls;
using System.Globalization;
using AtomUI.Controls.Utils;
using AtomUI.Desktop.Controls.Localization;
using AtomUI.Media;
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

    /// <summary>
    /// Parses text typed into a picker input. As in Ant Design, the text must match the display format in full;
    /// a week may omit its localized suffix. The result is normalized to the picker unit, keeping the clock only
    /// when the format shows it.
    /// </summary>
    /// <returns>
    /// <see langword="true" /> with <paramref name="value" /> set to the parsed date and time; or
    /// <see langword="false" /> with <paramref name="value" /> set to <see langword="default" /> when
    /// <paramref name="text" /> is null or white space, or does not match the format in full.
    /// </returns>
    internal static bool TryParseDateTime(
        string? text,
        string? format,
        DatePickerMode pickerMode,
        bool isShowTime,
        ClockIdentifierType clockIdentifier,
        DateTimeFormatInfo? formatInfo,
        out DateTime value)
    {
        value = default;
        if (string.IsNullOrWhiteSpace(text))
        {
            return false;
        }

        var trimmed = text.Trim();
        if (format is null && pickerMode != DatePickerMode.Date)
        {
            var parsed = pickerMode switch
            {
                DatePickerMode.Week    => TryParseWeek(trimmed, out value),
                DatePickerMode.Quarter => TryParseQuarter(trimmed, out value),
                DatePickerMode.Month => DateTime.TryParseExact(trimmed, "yyyy-MM", CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out value),
                _ => DateTime.TryParseExact(trimmed, "yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None,
                    out value)
            };
            if (parsed)
            {
                value = NormalizeDateTime(value, pickerMode);
            }
            return parsed;
        }

        var effectiveFormat = GetEffectiveFormat(format, pickerMode, isShowTime, clockIdentifier);
        IFormatProvider provider = formatInfo ?? (IFormatProvider)CultureInfo.CurrentCulture;
        if (!DateTime.TryParseExact(trimmed, effectiveFormat, provider, DateTimeStyles.None, out value))
        {
            return false;
        }

        if (pickerMode != DatePickerMode.Date)
        {
            value = NormalizeDateTime(value, pickerMode);
        }
        return true;
    }

    private static bool TryParseQuarter(string text, out DateTime value)
    {
        value = default;
        if (text.Length != 7 || text[4] != '-' || char.ToUpperInvariant(text[5]) != 'Q' ||
            !int.TryParse(text.AsSpan(0, 4), NumberStyles.None, CultureInfo.InvariantCulture, out var year) ||
            year < 1 || text[6] is < '1' or > '4')
        {
            return false;
        }

        value = new DateTime(year, (text[6] - '1') * 3 + 1, 1);
        return true;
    }

    private static bool TryParseWeek(string text, out DateTime value)
    {
        value = default;
        var digitsEnd = 5;
        while (digitsEnd < text.Length && char.IsAsciiDigit(text[digitsEnd]))
        {
            digitsEnd++;
        }

        if (text.Length < 6 || text[4] != '-' || digitsEnd == 5 || digitsEnd - 5 > 2 ||
            !int.TryParse(text.AsSpan(0, 4), NumberStyles.None, CultureInfo.InvariantCulture, out var year) ||
            !int.TryParse(text.AsSpan(5, digitsEnd - 5), NumberStyles.None, CultureInfo.InvariantCulture, out var week) ||
            year is < 2 or > 9998)
        {
            return false;
        }

        var culture  = GetFormattingCulture();
        var firstDay = DateUnitMath.GetWeekFirstDay(culture);
        var start    = DateUnitMath.GetWeekStart(new DateTime(year, 1, 1).AddDays(-7), firstDay);
        for (var index = 0; index < 55; index++)
        {
            var candidate = start.AddDays(index * 7);
            if (DateUnitMath.GetWeekIdentity(candidate, culture, firstDay) != (year, week))
            {
                continue;
            }

            if (digitsEnd < text.Length && !string.Equals(FormatWeek(candidate), text, StringComparison.Ordinal))
            {
                return false;
            }

            value = candidate;
            return true;
        }

        return false;
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
                TextUtils.CalculateTextSizeCached(
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
        var unit = GetFormattingUnit(pickerMode);
        var firstDay = ResolveWeekStart(unit, firstDayOfWeek);
        return DateUnitMath.Normalize(dateTime, unit, firstDay);
    }

    internal static bool IsSamePickerUnit(DateTime first, DateTime second, DatePickerMode pickerMode)
    {
        var unit = GetFormattingUnit(pickerMode);
        return DateUnitMath.IsSameUnit(first, second, unit, ResolveWeekStart(unit, null));
    }

    private static DayOfWeek ResolveWeekStart(DateViewerSelectionUnit unit, DayOfWeek? firstDayOfWeek) =>
        unit == DateViewerSelectionUnit.Week ? firstDayOfWeek ?? DateUnitMath.GetWeekFirstDay(GetFormattingCulture()) : default;

    private static DateViewerSelectionUnit GetFormattingUnit(DatePickerMode pickerMode) =>
        pickerMode is >= DatePickerMode.Date and <= DatePickerMode.Year
            ? DatePickerPresenter.ToSelectionUnit(pickerMode)
            : DateViewerSelectionUnit.Date;

    private static string FormatWeek(DateTime dateTime)
    {
        var culture = GetFormattingCulture();
        var (year, week) = DateUnitMath.GetWeekIdentity(dateTime, culture, DateUnitMath.GetWeekFirstDay(culture));
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
        var quarter = DateUnitMath.GetQuarter(dateTime);
        return string.Create(CultureInfo.InvariantCulture, $"{dateTime.Year:D4}-Q{quarter}");
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
