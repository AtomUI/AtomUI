using System.Globalization;
using AtomUI.Media;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Controls.Utils;

internal static class DateTimeUtils
{
    public static string FormatTimeSpan(TimeSpan? time, bool is12HourClock = false, string? amText = null, string? pmText = null)
    {
        if (time is null)
        {
            return string.Empty;
        }
        var dateTime = DateTime.Today.Add(time.Value);
        if (is12HourClock)
        {
            var formatInfo = new DateTimeFormatInfo();
            formatInfo.AMDesignator = amText ?? "AM";
            formatInfo.PMDesignator = pmText ?? "PM";
            return dateTime.ToString("hh:mm:ss tt", formatInfo);
        }

        return dateTime.ToString("HH:mm:ss");
    }

    /// <summary>
    /// 基于格式和字体计算出任意日期/时间字符串能达到的最大渲染宽度，
    /// 用于给输入框设置稳定的 PreferredInputWidth，避免因数字/月份/星期名不同而抖动。
    /// </summary>
    public static Size CalculateWidestFormattedDateTimeSize(
        string format,
        double fontSize,
        FontFamily fontFamily,
        FontStyle fontStyle,
        FontWeight fontWeight,
        DateTimeFormatInfo? formatInfo = null)
    {
        var widestDigit = FindWidestDigit(fontSize, fontFamily, fontStyle, fontWeight);

        var needMonthSweep   = ContainsToken(format, 'M');
        var needWeekdaySweep = ContainsToken(format, 'd', minRepeat: 3);
        var needAmPmSweep    = ContainsToken(format, 't');
        // Formatting all candidate dates preserves culture/calendar behavior. After replacing
        // digits, identical strings have identical text metrics and need shaping only once.
        var measuredTexts = needMonthSweep || needWeekdaySweep
            ? new HashSet<string>(StringComparer.Ordinal)
            : null;

        var maxWidth  = 0d;
        var maxHeight = 0d;
        // 2028-01-02 是星期天,加 offset 可以扫过一周;日 28 保证两位数字,时分秒取宽数字概率更高.
        var monthCount = needMonthSweep ? 12 : 1;
        for (var month = 1; month <= monthCount; month++)
        {
            var weekdayCount = needWeekdaySweep ? 7 : 1;
            for (var offset = 0; offset < weekdayCount; offset++)
            {
                DateTime date;
                try
                {
                    date = new DateTime(2028, month, 28).AddDays(offset);
                }
                catch
                {
                    continue;
                }

                var hourCount = needAmPmSweep ? 2 : 1;
                for (var hourIndex = 0; hourIndex < hourCount; hourIndex++)
                {
                    var hour   = needAmPmSweep && hourIndex == 0 ? 9 : 22;
                    var sample = new DateTime(date.Year, date.Month, date.Day, hour, 58, 58);
                    var text   = formatInfo != null ? sample.ToString(format, formatInfo) : sample.ToString(format);
                    text = ReplaceDigits(text, widestDigit);
                    if (measuredTexts is not null && !measuredTexts.Add(text))
                    {
                        continue;
                    }
                    var size = TextUtils.CalculateTextSize(text, fontSize, fontFamily, fontStyle, fontWeight);
                    if (size.Width > maxWidth)
                    {
                        maxWidth = size.Width;
                    }
                    if (size.Height > maxHeight)
                    {
                        maxHeight = size.Height;
                    }
                }
            }
        }

        return new Size(maxWidth, maxHeight);
    }

    /// <summary>
    /// 基于时钟类型和字体计算 TimePicker 输入框的最大渲染宽度。
    /// </summary>
    public static Size CalculateWidestFormattedTimeSpanSize(
        bool is12HourClock,
        string? amText,
        string? pmText,
        double fontSize,
        FontFamily fontFamily,
        FontStyle fontStyle,
        FontWeight fontWeight)
    {
        if (!is12HourClock)
        {
            return CalculateWidestFormattedDateTimeSize("HH:mm:ss", fontSize, fontFamily, fontStyle, fontWeight);
        }

        var formatInfo = new DateTimeFormatInfo
        {
            AMDesignator = amText ?? "AM",
            PMDesignator = pmText ?? "PM"
        };
        return CalculateWidestFormattedDateTimeSize("hh:mm:ss tt", fontSize, fontFamily, fontStyle, fontWeight, formatInfo);
    }

    private static char FindWidestDigit(double fontSize, FontFamily fontFamily, FontStyle fontStyle, FontWeight fontWeight)
    {
        var widest      = '0';
        var widestWidth = 0d;
        for (var d = '0'; d <= '9'; d++)
        {
            var w = TextUtils.CalculateTextSize(d.ToString(), fontSize, fontFamily, fontStyle, fontWeight).Width;
            if (w > widestWidth)
            {
                widestWidth = w;
                widest      = d;
            }
        }
        return widest;
    }

    private static string ReplaceDigits(string text, char widestDigit)
    {
        var buffer = new char[text.Length];
        for (var i = 0; i < text.Length; i++)
        {
            buffer[i] = char.IsDigit(text[i]) ? widestDigit : text[i];
        }
        return new string(buffer);
    }

    /// <summary>
    /// 检查格式字符串中(忽略 'literal' 引号段和转义字符)是否出现指定字符至少 minRepeat 次连续。
    /// </summary>
    private static bool ContainsToken(string format, char token, int minRepeat = 1)
    {
        var inQuote    = false;
        var quoteChar  = '\0';
        var i          = 0;
        while (i < format.Length)
        {
            var c = format[i];
            if (inQuote)
            {
                if (c == quoteChar)
                {
                    inQuote = false;
                }
                i++;
                continue;
            }

            if (c == '\'' || c == '"')
            {
                inQuote   = true;
                quoteChar = c;
                i++;
                continue;
            }

            if (c == '\\' && i + 1 < format.Length)
            {
                i += 2;
                continue;
            }

            if (c == token)
            {
                var run = 1;
                while (i + run < format.Length && format[i + run] == token)
                {
                    run++;
                }
                if (run >= minRepeat)
                {
                    return true;
                }
                i += run;
                continue;
            }

            i++;
        }

        return false;
    }
}
