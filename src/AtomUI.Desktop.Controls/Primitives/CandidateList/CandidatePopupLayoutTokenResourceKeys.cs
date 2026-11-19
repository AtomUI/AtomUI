using System.Globalization;
using AtomUI.Theme.Resources;
using Avalonia;
using Avalonia.Data;

namespace AtomUI.Desktop.Controls;

internal sealed record CandidatePopupLayoutTokenResourceKeys(object RowHeight, object? ContentPadding = null, object? DefaultMaxHeight = null)
{
    internal OwnerTokenSet Observe(StyledElement owner, Action changed)
    {
        var tokens = new OwnerTokenSet(owner, DefaultMaxHeight is null ? 2 : 3, changed);
        tokens.SetResourceKey(0, RowHeight);
        tokens.SetResourceKey(1, ContentPadding);
        if (DefaultMaxHeight is not null)
        {
            tokens.SetResourceKey(2, DefaultMaxHeight);
        }
        return tokens;
    }

    internal static double? ReadLength(object? value, double? previous)
    {
        if (value is double number)
        {
            return number;
        }
        if (ReferenceEquals(value, BindingOperations.DoNothing))
        {
            return previous;
        }
        try
        {
            return value is IConvertible convertible ? convertible.ToDouble(CultureInfo.CurrentCulture) : null;
        }
        catch (Exception error) when (error is FormatException or InvalidCastException or OverflowException)
        {
            return null;
        }
    }

    internal static Thickness ReadPadding(object? value, Thickness previous)
    {
        if (value is Thickness padding)
        {
            return padding;
        }
        if (ReferenceEquals(value, BindingOperations.DoNothing))
        {
            return previous;
        }
        if (value is string text)
        {
            try
            {
                return Thickness.Parse(text);
            }
            catch (FormatException)
            {
                return default;
            }
        }
        return default;
    }
}
