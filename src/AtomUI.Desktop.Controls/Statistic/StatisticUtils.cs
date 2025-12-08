using System.Globalization;

namespace AtomUI.Desktop.Controls;

internal static class StatisticUtils
{
    public static string? FormatNumber(object value,  string groupSeparator, string decimalSeparator, int precision)
    {
        var strValue     = value.ToString();
        var customFormat = new CultureInfo("en-US");
        customFormat.NumberFormat.NumberGroupSeparator   = groupSeparator;
        customFormat.NumberFormat.NumberDecimalSeparator = decimalSeparator;
        customFormat.NumberFormat.NumberDecimalDigits    = precision;
        string? effectiveValue = null;
        if (int.TryParse(strValue,  out var intValue))
        {
            effectiveValue = intValue.ToString("N", customFormat);
        }
        else if (uint.TryParse(strValue, out var uintValue))
        {
            effectiveValue = uintValue.ToString("N", customFormat);
        }
        else if (long.TryParse(strValue, out var longValue))
        {
            effectiveValue = longValue.ToString("N", customFormat);
        }
        else if (long.TryParse(strValue, out var ulongValue))
        {
            effectiveValue = ulongValue.ToString("N", customFormat);
        }
        else if (float.TryParse(strValue, out var floatValue))
        {
            effectiveValue = floatValue.ToString("N", customFormat);
        }
        else if (double.TryParse(strValue, out var doubleValue))
        {
            effectiveValue = doubleValue.ToString("N", customFormat);
        }
        else if (decimal.TryParse(strValue, out var decimalValue))
        {
            effectiveValue = decimalValue.ToString("N", customFormat);
        }
        else
        {
            effectiveValue = strValue;
        }

        return effectiveValue;
    }
}