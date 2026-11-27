using Avalonia.Markup.Xaml;

namespace AtomUI.Controls;

/// <summary>
/// XAML markup extension that sets the values of one breakpoint of a <see cref="Col" /> inline, as in
/// <c>{ColInfo Order=2}</c>. It yields a <see cref="GridColSize" /> in which every value that is left out stays
/// unset, so the column keeps its own.
/// </summary>
public class ColInfoExtension : MarkupExtension
{
    /// <summary>
    /// The flex setting of the column at that breakpoint, or <see langword="null" /> to keep the value of the column.
    /// </summary>
    public GridColFlex? Flex { get; set; }
    /// <summary>
    /// The number of columns the column occupies at that breakpoint, or <see langword="null" /> to keep its span.
    /// A value of <c>0</c> hides the column at that breakpoint.
    /// </summary>
    public int? Span { get; set; }
    /// <summary>
    /// The number of empty columns to the left of the column at that breakpoint, or <see langword="null" /> to keep
    /// the value of the column.
    /// </summary>
    public int? Offset { get; set; }
    /// <summary>
    /// The position of the column among the children of the row at that breakpoint, or <see langword="null" /> to
    /// keep the value of the column. Negative values are allowed.
    /// </summary>
    public int? Order { get; set; }
    /// <summary>
    /// The number of columns the column is moved to the right at that breakpoint, or <see langword="null" /> to keep
    /// the value of the column.
    /// </summary>
    public int? Push { get; set; }
    /// <summary>
    /// The number of columns the column is moved to the left at that breakpoint, or <see langword="null" /> to keep
    /// the value of the column.
    /// </summary>
    public int? Pull { get; set; }
    /// <summary>
    /// Checks that <see cref="Span" />, <see cref="Offset" />, <see cref="Push" /> and <see cref="Pull" /> are between
    /// 0 and 24, and returns a new <see cref="GridColSize" /> that carries the values that are set.
    /// </summary>
    /// <returns>
    /// A <see cref="GridColSize" /> whose values that are left out are <see langword="null" />.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <see cref="Span" />, <see cref="Offset" />, <see cref="Push" /> or <see cref="Pull" /> is set outside 0 to 24.
    /// </exception>
    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        ValidateColumnValue(Span, "Span");
        ValidateColumnValue(Offset, "Offset");
        ValidateColumnValue(Push, "Push");
        ValidateColumnValue(Pull, "Pull");

        return new GridColSize
        {
            Flex = Flex,
            Span = Span,
            Offset = Offset,
            Order = Order,
            Push = Push,
            Pull = Pull
        };
    }

    private static void ValidateColumnValue(int? value, string name)
    {
        if (!value.HasValue)
        {
            return;
        }

        if (value < 0 || value > 24)
        {
            throw new ArgumentOutOfRangeException(
                name,
                value,
                $"{name} must be between 0 and 24.");
        }
    }
}
