using Avalonia.Controls;

namespace AtomUI.Controls;

/// <summary>
/// Holds one <see cref="GridLength" /> for each breakpoint from <see cref="ExtraSmall" /> to
/// <see cref="ExtraExtraLarge" />.
/// </summary>
/// <remarks>
/// The values do not cascade from a smaller breakpoint to a larger one: <c>FormItem</c> reads the value of the
/// exact breakpoint, and it uses <see cref="ExtraExtraLarge" /> for
/// <see cref="MediaBreakPoint.ExtraExtraExtraLarge" /> as well.
/// </remarks>
public record MediaBreakGridLength
{
    /// <summary>
    /// The grid length for the <see cref="MediaBreakPoint.ExtraSmall" /> breakpoint (<c>xs</c>).
    /// </summary>
    public GridLength ExtraSmall { get; init; }
    /// <summary>
    /// The grid length for the <see cref="MediaBreakPoint.Small" /> breakpoint (<c>sm</c>).
    /// </summary>
    public GridLength Small { get; init; }
    /// <summary>
    /// The grid length for the <see cref="MediaBreakPoint.Medium" /> breakpoint (<c>md</c>).
    /// </summary>
    public GridLength Medium { get; init; }
    /// <summary>
    /// The grid length for the <see cref="MediaBreakPoint.Large" /> breakpoint (<c>lg</c>).
    /// </summary>
    public GridLength Large { get; init; }
    /// <summary>
    /// The grid length for the <see cref="MediaBreakPoint.ExtraLarge" /> breakpoint (<c>xl</c>).
    /// </summary>
    public GridLength ExtraLarge { get; init; }
    /// <summary>
    /// The grid length for the <see cref="MediaBreakPoint.ExtraExtraLarge" /> breakpoint (<c>xxl</c>).
    /// </summary>
    public GridLength ExtraExtraLarge { get; init; }

    public MediaBreakGridLength()
        : this(GridLength.Auto)
    {
    }
    
    /// <summary>
    /// Initializes the record with the same grid length at every breakpoint.
    /// </summary>
    /// <param name="value">
    /// The grid length for every breakpoint.
    /// </param>
    public MediaBreakGridLength(GridLength value)
    {
        ExtraSmall      = value;
        Small           = value;
        Medium          = value;
        Large           = value;
        ExtraLarge      = value;
        ExtraExtraLarge = value;
    }

    /// <summary>
    /// Initializes the record with one grid length per breakpoint.
    /// </summary>
    /// <param name="extraSmall">
    /// The grid length for the <see cref="MediaBreakPoint.ExtraSmall" /> breakpoint.
    /// </param>
    /// <param name="small">
    /// The grid length for the <see cref="MediaBreakPoint.Small" /> breakpoint.
    /// </param>
    /// <param name="medium">
    /// The grid length for the <see cref="MediaBreakPoint.Medium" /> breakpoint.
    /// </param>
    /// <param name="large">
    /// The grid length for the <see cref="MediaBreakPoint.Large" /> breakpoint.
    /// </param>
    /// <param name="extraLarge">
    /// The grid length for the <see cref="MediaBreakPoint.ExtraLarge" /> breakpoint.
    /// </param>
    /// <param name="extraExtraLarge">
    /// The grid length for the <see cref="MediaBreakPoint.ExtraExtraLarge" /> breakpoint.
    /// </param>
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
    
    /// <summary>
    /// Parses a grid length that applies to every breakpoint, or a value for each breakpoint that the input names.
    /// </summary>
    /// <param name="input">
    /// Either a text that <see cref="GridLength.Parse(string)" /> accepts, such as <c>Auto</c> or <c>8*</c>, or
    /// comma-separated segments of the form <c>breakpoint:gridLength</c>, such as <c>xs:Auto, md:8*</c>. The
    /// breakpoint names are <c>xs</c>, <c>sm</c>, <c>md</c>, <c>lg</c>, <c>xl</c> and <c>xxl</c>, ignoring case.
    /// </param>
    /// <returns>
    /// A record with the parsed length at every breakpoint, or for the segments a record whose unnamed breakpoints
    /// keep <see cref="GridLength.Auto" />.
    /// </returns>
    /// <exception cref="FormatException">
    /// A segment has no colon, an empty breakpoint name, an empty value or an unknown breakpoint name, or a grid
    /// length is not well formed.
    /// </exception>
    public static MediaBreakGridLength Parse(string input)
    {
    
        var span         = input.AsSpan();
        int segmentIndex = 0;
        
        // Try the input as one grid length for every breakpoint before reading it as per-breakpoint segments.
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