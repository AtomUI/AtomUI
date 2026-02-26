using Avalonia.Controls;

namespace AtomUI.Controls;

public record MediaBreakGridLength
{
    public GridLength ExtraSmall { get; init; }
    public GridLength Small { get; init; }
    public GridLength Medium { get; init; }
    public GridLength Large { get; init; }
    public GridLength ExtraLarge { get; init; }
    public GridLength ExtraExtraLarge { get; init; }

    public MediaBreakGridLength()
        : this(GridLength.Auto)
    {
    }
    
    public MediaBreakGridLength(GridLength value)
    {
        ExtraSmall      = value;
        Small           = value;
        Medium          = value;
        Large           = value;
        ExtraLarge      = value;
        ExtraExtraLarge = value;
    }

    public MediaBreakGridLength(GridLength extraSmall,
                                GridLength small, 
                                GridLength medium,
                                GridLength large,
                                GridLength extraLarge, 
                                GridLength extraExtraLarge)
    {
        ExtraSmall      = extraSmall;
        Small           = small;
        Medium          = medium;
        Large           = large;
        ExtraLarge      = extraLarge;
        ExtraExtraLarge = extraExtraLarge;
    }
    
    public static MediaBreakGridLength Parse(string input)
    {
    
        var span         = input.AsSpan();
        int segmentIndex = 0;
        
        // 先按照一个值进行解析
        try
        {
            var value = GridLength.Parse(input);
            return new MediaBreakGridLength(value);
        }
        catch (Exception)
        {
        }

        var result = new MediaBreakGridLength();
        while (!span.IsEmpty)
        {
            segmentIndex++;
            int                commaIndex = span.IndexOf(',');
            ReadOnlySpan<char> segment    = commaIndex >= 0 ? span[..commaIndex] : span;
            
            ProcessSegmentWithSwitch(segment, segmentIndex, ref result);
            
            span = commaIndex >= 0 ? span[(commaIndex + 1)..] : ReadOnlySpan<char>.Empty;
        }

        return result;
    }

    private static void ProcessSegmentWithSwitch(ReadOnlySpan<char> segment, int segmentIndex, ref MediaBreakGridLength result)
    {
        int colonIndex = segment.IndexOf(':');
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

        var value = GridLength.Parse(valueSpan.ToString());

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
            throw new FormatException($"`{segmentIndex}`: An unknown breakpoint name '{breakpoint.ToString()}', supporting breakpoints are: xs, sm, md, lg, xl, xxl");
        }
    }
}