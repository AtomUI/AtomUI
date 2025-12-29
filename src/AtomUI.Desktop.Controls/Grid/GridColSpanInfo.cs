using System;
using System.ComponentModel;
using System.Globalization;
using AtomUI.Controls;

namespace AtomUI.Desktop.Controls;

[TypeConverter(typeof(GridColSpanInfoConverter))]
public readonly record struct GridColSpanInfo
{
    public int ExtraSmall { get; init; }
    public int Small { get; init; }
    public int Medium { get; init; }
    public int Large { get; init; }
    public int ExtraLarge { get; init; }
    public int ExtraExtraLarge { get; init; }

    public GridColSpanInfo(int span)
    {
        ValidateSpan(span);
        ExtraSmall = span;
        Small = span;
        Medium = span;
        Large = span;
        ExtraLarge = span;
        ExtraExtraLarge = span;
    }

    public GridColSpanInfo(int extraSmall, int small, int medium, int large, int extraLarge, int extraExtraLarge)
    {
        ValidateSpan(extraSmall);
        ValidateSpan(small);
        ValidateSpan(medium);
        ValidateSpan(large);
        ValidateSpan(extraLarge);
        ValidateSpan(extraExtraLarge);

        ExtraSmall = extraSmall;
        Small = small;
        Medium = medium;
        Large = large;
        ExtraLarge = extraLarge;
        ExtraExtraLarge = extraExtraLarge;
    }

    public static GridColSpanInfo Parse(string input)
    {
        if (int.TryParse(input.Trim(), out var singleSpan))
        {
            ValidateSpan(singleSpan);
            return new GridColSpanInfo(singleSpan);
        }

        return ParseKeyValueFormat(input);
    }

    public int GetValue(MediaBreakPoint breakPoint)
    {
        return breakPoint switch
        {
            MediaBreakPoint.ExtraSmall => ExtraSmall,
            MediaBreakPoint.Small => Small,
            MediaBreakPoint.Medium => Medium,
            MediaBreakPoint.Large => Large,
            MediaBreakPoint.ExtraLarge => ExtraLarge,
            _ => ExtraExtraLarge
        };
    }

    private static GridColSpanInfo ParseKeyValueFormat(string input)
    {
        var result = new GridColSpanInfo(0);
        var span = input.AsSpan();
        int segmentIndex = 0;

        while (!span.IsEmpty)
        {
            segmentIndex++;
            var commaIndex = span.IndexOf(',');
            var segment = commaIndex >= 0 ? span[..commaIndex] : span;

            result = ProcessSegment(segment, segmentIndex, result);

            span = commaIndex >= 0 ? span[(commaIndex + 1)..] : ReadOnlySpan<char>.Empty;
        }

        return result;
    }

    private static GridColSpanInfo ProcessSegment(ReadOnlySpan<char> segment, int segmentIndex, GridColSpanInfo result)
    {
        var colonIndex = segment.IndexOf(':');
        if (colonIndex < 0)
        {
            throw new FormatException($"Segment {segmentIndex}: Missing colon separator '{segment.ToString()}'");
        }

        var breakpoint = segment[..colonIndex].Trim();
        var valueSpan = segment[(colonIndex + 1)..].Trim();

        if (breakpoint.IsEmpty)
        {
            throw new FormatException($"Segment {segmentIndex}: Breakpoint name is empty.");
        }

        if (valueSpan.IsEmpty)
        {
            throw new FormatException($"The breakpoint '{breakpoint.ToString()}' at segment {segmentIndex} is null.");
        }

        if (!int.TryParse(valueSpan, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value))
        {
            throw new FormatException($"The value of breakpoint '{breakpoint.ToString()}' is not a valid integer.");
        }

        ValidateSpan(value);

        if (breakpoint.Equals("xs", StringComparison.OrdinalIgnoreCase))
        {
            return result with { ExtraSmall = value };
        }
        if (breakpoint.Equals("sm", StringComparison.OrdinalIgnoreCase))
        {
            return result with { Small = value };
        }
        if (breakpoint.Equals("md", StringComparison.OrdinalIgnoreCase))
        {
            return result with { Medium = value };
        }
        if (breakpoint.Equals("lg", StringComparison.OrdinalIgnoreCase))
        {
            return result with { Large = value };
        }
        if (breakpoint.Equals("xl", StringComparison.OrdinalIgnoreCase))
        {
            return result with { ExtraLarge = value };
        }
        if (breakpoint.Equals("xxl", StringComparison.OrdinalIgnoreCase))
        {
            return result with { ExtraExtraLarge = value };
        }

        throw new FormatException(
            $"`{segmentIndex}`: Unknown breakpoint '{breakpoint.ToString()}', supported: xs, sm, md, lg, xl, xxl");
    }

    private static void ValidateSpan(int span)
    {
        if (span < 0 || span > 24)
        {
            throw new FormatException($"Span must be between 0 and 24, got {span}.");
        }
    }

    public static implicit operator GridColSpanInfo(int span) => new(span);
}

public class GridColSpanInfoConverter : TypeConverter
{
    public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
    {
        var typeCode = Type.GetTypeCode(sourceType);
        return typeCode switch
        {
            TypeCode.String => true,
            TypeCode.Decimal => true,
            TypeCode.Single => true,
            TypeCode.Double => true,
            TypeCode.Int16 => true,
            TypeCode.Int32 => true,
            TypeCode.Int64 => true,
            TypeCode.UInt16 => true,
            TypeCode.UInt32 => true,
            TypeCode.UInt64 => true,
            _ => false
        };
    }

    public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType)
    {
        return destinationType == typeof(string);
    }

    public override object ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
    {
        if (value == null)
        {
            throw new NotSupportedException("Cannot convert null to GridColSpanInfo.");
        }

        if (value is GridColSpanInfo spanInfo)
        {
            return spanInfo;
        }

        if (value is string str)
        {
            return GridColSpanInfo.Parse(str);
        }

        if (value is IConvertible convertible)
        {
            var number = convertible.ToInt32(culture ?? CultureInfo.InvariantCulture);
            return new GridColSpanInfo(number);
        }

        throw new NotSupportedException($"Cannot convert value '{value}' to GridColSpanInfo.");
    }

    public override object ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
    {
        if (destinationType != typeof(string))
        {
            throw new NotSupportedException($"Cannot convert GridColSpanInfo to {destinationType}.");
        }

        if (value is GridColSpanInfo spanInfo)
        {
            return spanInfo.ExtraSmall.ToString(CultureInfo.InvariantCulture);
        }

        return string.Empty;
    }
}
