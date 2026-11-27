using System.Globalization;

namespace AtomUI.Controls;

/// <summary>
/// The width of one part of a <see cref="GridGutter" />, held as one value per breakpoint in device-independent
/// pixels. Values that come from <see cref="Parse(string)" /> are zero or positive and finite; a value that is
/// set in code is not checked.
/// </summary>
public record GridGutterInfo
{
    /// <summary>
    /// The gutter width for the <see cref="MediaBreakPoint.ExtraSmall" /> breakpoint (<c>xs</c>).
    /// </summary>
    public double ExtraSmall { get; init; }
    /// <summary>
    /// The gutter width for the <see cref="MediaBreakPoint.Small" /> breakpoint (<c>sm</c>).
    /// </summary>
    public double Small { get; init; }
    /// <summary>
    /// The gutter width for the <see cref="MediaBreakPoint.Medium" /> breakpoint (<c>md</c>).
    /// </summary>
    public double Medium { get; init; }
    /// <summary>
    /// The gutter width for the <see cref="MediaBreakPoint.Large" /> breakpoint (<c>lg</c>).
    /// </summary>
    public double Large { get; init; }
    /// <summary>
    /// The gutter width for the <see cref="MediaBreakPoint.ExtraLarge" /> breakpoint (<c>xl</c>).
    /// </summary>
    public double ExtraLarge { get; init; }
    /// <summary>
    /// The gutter width for the <see cref="MediaBreakPoint.ExtraExtraLarge" /> breakpoint (<c>xxl</c>).
    /// </summary>
    public double ExtraExtraLarge { get; init; }
    /// <summary>
    /// The gutter width for the <see cref="MediaBreakPoint.ExtraExtraExtraLarge" /> breakpoint (<c>xxxl</c>).
    /// </summary>
    public double ExtraExtraExtraLarge { get; init; }

    public GridGutterInfo()
        : this(0)
    {
    }

    /// <summary>
    /// Initializes a gutter part that uses <paramref name="value" /> at every breakpoint.
    /// </summary>
    public GridGutterInfo(double value)
    {
        ExtraSmall      = value;
        Small           = value;
        Medium          = value;
        Large           = value;
        ExtraLarge      = value;
        ExtraExtraLarge = value;
        ExtraExtraExtraLarge = value;
    }

    /// <summary>
    /// Initializes a gutter part with one width per breakpoint. The width of the <c>xxl</c> breakpoint is also used
    /// for the <c>xxxl</c> breakpoint.
    /// </summary>
    public GridGutterInfo(double extraSmall, double small, double medium, double large, double extraLarge, double extraExtraLarge)
        : this(extraSmall, small, medium, large, extraLarge, extraExtraLarge, extraExtraLarge)
    {
    }

    /// <summary>
    /// Initializes a gutter part with one width per breakpoint.
    /// </summary>
    public GridGutterInfo(double extraSmall,
                          double small,
                          double medium,
                          double large,
                          double extraLarge,
                          double extraExtraLarge,
                          double extraExtraExtraLarge)
    {
        ExtraSmall      = extraSmall;
        Small           = small;
        Medium          = medium;
        Large           = large;
        ExtraLarge      = extraLarge;
        ExtraExtraLarge = extraExtraLarge;
        ExtraExtraExtraLarge = extraExtraExtraLarge;
    }

    /// <summary>
    /// Parses a width that applies to every breakpoint, or a width for each breakpoint that the input names.
    /// </summary>
    /// <param name="input">
    /// Either a number such as <c>16</c>, or comma-separated segments of the form <c>breakpoint:value</c> such as
    /// <c>xs: 8, md: 16</c>. The breakpoints are <c>xs</c>, <c>sm</c>, <c>md</c>, <c>lg</c>, <c>xl</c>,
    /// <c>xxl</c> and <c>xxxl</c>, ignoring case. Numbers use the invariant culture. White space around a value
    /// or separator is ignored.
    /// </param>
    /// <returns>
    /// A gutter part with the number at every breakpoint. For a breakpoint list, each breakpoint takes the value
    /// of the largest named breakpoint that is not larger than it, and the breakpoints below the smallest named
    /// one are left at zero.
    /// </returns>
    /// <exception cref="FormatException">
    /// The input is empty, is neither a number nor a readable breakpoint list, a breakpoint is named twice or
    /// unknown, or a number is negative, NaN or infinite.
    /// </exception>
    public static GridGutterInfo Parse(string input)
    {
        return Parse(input.AsSpan());
    }

    internal static GridGutterInfo Parse(ReadOnlySpan<char> input)
    {
        var trimmed = input.Trim();
        if (double.TryParse(trimmed, NumberStyles.Float, CultureInfo.InvariantCulture, out var single))
        {
            ValidateGutterValue(single);
            return new GridGutterInfo(single);
        }

        var responsive = ResponsiveDouble.Parse(trimmed.ToString());
        return new GridGutterInfo(
            responsive.Resolve(MediaBreakPoint.ExtraSmall, 0),
            responsive.Resolve(MediaBreakPoint.Small, 0),
            responsive.Resolve(MediaBreakPoint.Medium, 0),
            responsive.Resolve(MediaBreakPoint.Large, 0),
            responsive.Resolve(MediaBreakPoint.ExtraLarge, 0),
            responsive.Resolve(MediaBreakPoint.ExtraExtraLarge, 0),
            responsive.Resolve(MediaBreakPoint.ExtraExtraExtraLarge, 0));
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
            MediaBreakPoint.ExtraExtraLarge => ExtraExtraLarge,
            _ => ExtraExtraExtraLarge
        };
    }

    private static void ValidateGutterValue(double value)
    {
        if (double.IsNaN(value) || double.IsInfinity(value) || value < 0)
        {
            throw new FormatException($"Gutter value must be >= 0, got {value}.");
        }
    }
}
