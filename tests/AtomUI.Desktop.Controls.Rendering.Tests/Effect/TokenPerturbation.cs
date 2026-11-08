using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls.Rendering.Tests.Effect;

/// <summary>
/// Produces a clearly different value for a Token, following the perturbation table in
/// docs/engineering/development/theme-effect-tests.md §3.
/// </summary>
public static class TokenPerturbation
{
    /// <summary>Channels within this distance of 128 count as "near mid grey", where inversion barely changes the color.</summary>
    private const int MidGreyRadius = 32;

    /// <summary>Alpha below this keeps any inverse nearly invisible, so such colors perturb to an opaque pure color.</summary>
    private const byte VisibleAlpha = 0x80;

    private static readonly Color PureColor = Color.FromRgb(0xFF, 0x00, 0x00);

    /// <summary>
    /// Returns the perturbed value. Types outside the table throw <see cref="NotSupportedException"/>:
    /// effect tests never skip a Token, so a new type needs an explicit case here.
    /// </summary>
    public static object Perturb(object value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return value switch
        {
            Color color => PerturbColor(color),
            ISolidColorBrush brush => new SolidColorBrush(PerturbColor(brush.Color), brush.Opacity),
            double number => PerturbDouble(number),
            Thickness thickness => new Thickness(
                thickness.Left + 4,
                thickness.Top + 4,
                thickness.Right + 4,
                thickness.Bottom + 4),
            CornerRadius radius => new CornerRadius(
                radius.TopLeft + 4,
                radius.TopRight + 4,
                radius.BottomRight + 4,
                radius.BottomLeft + 4),
            BoxShadows shadows => PerturbShadows(shadows),
            BoxShadow shadow => PerturbShadow(shadow),
            TimeSpan duration => duration == TimeSpan.Zero ? TimeSpan.FromMilliseconds(200) : duration * 2,
            bool flag => !flag,
            _ => throw new NotSupportedException(
                $"No Token perturbation is defined for value type '{value.GetType().FullName}'. " +
                "Add an explicit case to TokenPerturbation; effect tests do not skip Tokens.")
        };
    }

    private static Color PerturbColor(Color color)
    {
        if (color.A < VisibleAlpha)
        {
            // Mostly transparent: its inverse would stay invisible (or blend into the background), so go opaque pure.
            return PureColor;
        }

        return IsNearMidGrey(color)
            ? Color.FromArgb(color.A, PureColor.R, PureColor.G, PureColor.B)
            : Invert(color);
    }

    private static Color Invert(Color color) =>
        Color.FromArgb(color.A, (byte)(255 - color.R), (byte)(255 - color.G), (byte)(255 - color.B));

    private static bool IsNearMidGrey(Color color) =>
        Math.Abs(color.R - 128) <= MidGreyRadius &&
        Math.Abs(color.G - 128) <= MidGreyRadius &&
        Math.Abs(color.B - 128) <= MidGreyRadius;

    private static double PerturbDouble(double number)
    {
        if (!double.IsFinite(number))
        {
            throw new NotSupportedException(
                $"Double Token value '{number}' cannot be perturbed by doubling; add an explicit case.");
        }

        return number == 0 ? 8 : number * 2;
    }

    private static BoxShadows PerturbShadows(BoxShadows shadows)
    {
        if (shadows.Count == 0)
        {
            // No shadow yet: perturbing nothing by +8 must still be visible.
            return new BoxShadows(new BoxShadow
            {
                OffsetX = 8,
                OffsetY = 8,
                Blur    = 8,
                Color   = Color.FromArgb(0x60, 0, 0, 0)
            });
        }

        var rest = new BoxShadow[shadows.Count - 1];
        for (var i = 1; i < shadows.Count; i++)
        {
            rest[i - 1] = PerturbShadow(shadows[i]);
        }

        return new BoxShadows(PerturbShadow(shadows[0]), rest);
    }

    private static BoxShadow PerturbShadow(BoxShadow shadow)
    {
        return new BoxShadow
        {
            OffsetX = shadow.OffsetX + 8,
            OffsetY = shadow.OffsetY + 8,
            Blur    = shadow.Blur + 8,
            Spread  = shadow.Spread,
            Color   = shadow.Color,
            IsInset = shadow.IsInset
        };
    }
}
