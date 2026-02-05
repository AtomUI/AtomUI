using System.Globalization;
using AtomUI.Controls;

namespace AtomUI.Desktop.Controls;

public record GridGutterInfo
{
    public double ExtraSmall { get; init; }
    public double Small { get; init; }
    public double Medium { get; init; }
    public double Large { get; init; }
    public double ExtraLarge { get; init; }
    public double ExtraExtraLarge { get; init; }

    public GridGutterInfo()
        : this(0)
    {
    }

    public GridGutterInfo(double value)
    {
        ExtraSmall      = value;
        Small           = value;
        Medium          = value;
        Large           = value;
        ExtraLarge      = value;
        ExtraExtraLarge = value;
    }

    public GridGutterInfo(double extraSmall, double small, double medium, double large, double extraLarge, double extraExtraLarge)
    {
        ExtraSmall      = extraSmall;
        Small           = small;
        Medium          = medium;
        Large           = large;
        ExtraLarge      = extraLarge;
        ExtraExtraLarge = extraExtraLarge;
    }

    public static GridGutterInfo Parse(string input)
    {
        var trimmed = input.Trim();
        if (double.TryParse(trimmed, NumberStyles.Float, CultureInfo.InvariantCulture, out var single))
        {
            ValidateGutterValue(single);
            return new GridGutterInfo(single);
        }

        return ParseKeyValueFormat(input);
    }

    internal double GetValue(MediaBreakPoint breakPoint)
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

    private static GridGutterInfo ParseKeyValueFormat(string input)
    {
        var result       = new GridGutterInfo();
        var span         = input.AsSpan();
        int segmentIndex = 0;

        while (!span.IsEmpty)
        {
            segmentIndex++;
            var commaIndex = span.IndexOf(',');
            var segment = commaIndex >= 0 ? span[..commaIndex] : span;

            ProcessSegment(segment, segmentIndex, ref result);

            span = commaIndex >= 0 ? span[(commaIndex + 1)..] : ReadOnlySpan<char>.Empty;
        }

        return result;
    }

    private static void ProcessSegment(ReadOnlySpan<char> segment, int segmentIndex, ref GridGutterInfo result)
    {
        var colonIndex = segment.IndexOf(':');
        if (colonIndex < 0)
        {
            throw new FormatException($"Segment {segmentIndex}: Missing colon separator '{segment.ToString()}'");
        }

        var breakpoint = segment[..colonIndex].Trim();
        var valueSpan  = segment[(colonIndex + 1)..].Trim();

        if (breakpoint.IsEmpty)
        {
            throw new FormatException($"Segment {segmentIndex}: Breakpoint name is empty.");
        }

        if (valueSpan.IsEmpty)
        {
            throw new FormatException($"The breakpoint '{breakpoint.ToString()}' at segment {segmentIndex} is null.");
        }

        if (!double.TryParse(valueSpan, NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
        {
            throw new FormatException($"The value of breakpoint '{breakpoint.ToString()}' is not a valid number.");
        }

        ValidateGutterValue(value);

        if (breakpoint.Equals("xs", StringComparison.OrdinalIgnoreCase))
        {
            result = result with { ExtraSmall = value };
        }
        else if (breakpoint.Equals("sm", StringComparison.OrdinalIgnoreCase))
        {
            result = result with { Small = value };
        }
        else if (breakpoint.Equals("md", StringComparison.OrdinalIgnoreCase))
        {
            result = result with { Medium = value };
        }
        else if (breakpoint.Equals("lg", StringComparison.OrdinalIgnoreCase))
        {
            result = result with { Large = value };
        }
        else if (breakpoint.Equals("xl", StringComparison.OrdinalIgnoreCase))
        {
            result = result with { ExtraLarge = value };
        }
        else if (breakpoint.Equals("xxl", StringComparison.OrdinalIgnoreCase))
        {
            result = result with { ExtraExtraLarge = value };
        }
        else
        {
            throw new FormatException(
                $"`{segmentIndex}`: Unknown breakpoint '{breakpoint.ToString()}', supported: xs, sm, md, lg, xl, xxl");
        }
    }

    private static void ValidateGutterValue(double value)
    {
        if (double.IsNaN(value) || double.IsInfinity(value) || value < 0)
        {
            throw new FormatException($"Gutter value must be >= 0, got {value}.");
        }
    }
}
