using System;
using Avalonia.Markup.Xaml;

namespace AtomUI.Desktop.Controls;

public class ColInfoExtension : MarkupExtension
{
    public int? Span { get; set; }
    public int? Offset { get; set; }
    public int? Order { get; set; }
    public int? Push { get; set; }
    public int? Pull { get; set; }
    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        ValidateColumnValue(Span, "Span");
        ValidateColumnValue(Offset, "Offset");
        ValidateColumnValue(Push, "Push");
        ValidateColumnValue(Pull, "Pull");

        return new GridColSize
        {
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
