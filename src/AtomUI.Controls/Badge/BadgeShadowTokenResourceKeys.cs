using System.Globalization;
using AtomUI.Theme.Resources;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Controls.Commons;

/// <summary>
/// The resource keys of the ring of a badge indicator, its color and its spread; <c>CountBadgeAdorner</c> and
/// <c>DotBadgeAdorner</c> in AtomUI.Desktop.Controls expose their keys through it.
/// </summary>
/// <remarks>
/// <c>Observe</c> watches both keys on an element and reports each change. <c>TryRead</c> turns them into a single
/// <c>BoxShadow</c> and returns <see langword="true" /> when a color was resolved: from a <c>Color</c>, from a brush
/// cast to <c>ISolidColorBrush</c>, or from a string that parses as a color. It returns <see langword="false" />
/// with an empty shadow when the value is null or of another type. A brush or spread that has a value on the
/// element takes precedence over the observed value.
/// </remarks>
internal sealed record BadgeShadowTokenResourceKeys(object ColorKey, object SpreadKey)
{
    internal OwnerTokenSet Observe(StyledElement element, Action changed)
    {
        var tokens = new OwnerTokenSet(element, 2, changed);
        tokens.SetResourceKey(0, ColorKey);
        tokens.SetResourceKey(1, SpreadKey);
        return tokens;
    }

    internal static bool TryRead(
        StyledElement element,
        OwnerTokenSet? tokens,
        StyledProperty<IBrush?> brushProperty,
        StyledProperty<double> spreadProperty,
        out BoxShadows shadow)
    {
        var value = tokens is null || element.IsSet(brushProperty) ? element.GetValue(brushProperty) : tokens.GetValue(0);
        var color = value switch
        {
            Color direct => direct,
            IBrush brush => ((ISolidColorBrush)brush).Color,
            string text when Color.TryParse(text, out var parsed) => parsed,
            _ => (Color?)null
        };
        var spread = tokens is null || element.IsSet(spreadProperty)
            ? element.GetValue(spreadProperty)
            : ReadSpread(tokens.GetValue(1));
        shadow = color is { } tint
            ? new BoxShadows(new BoxShadow { Spread = spread, Color = tint })
            : default;
        return color.HasValue;
    }

    private static double ReadSpread(object? value)
    {
        if (value is double spread)
        {
            return spread;
        }

        try
        {
            return value is IConvertible convertible ? convertible.ToDouble(CultureInfo.CurrentCulture) : 0;
        }
        catch (Exception error) when (error is FormatException or InvalidCastException or OverflowException)
        {
            return 0;
        }
    }
}
