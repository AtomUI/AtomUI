using System.Globalization;
using Avalonia;
using Avalonia.Animation.Easings;
using Avalonia.Media.Immutable;

namespace AtomUI.Theme.Schema;

/// <summary>
/// Formats Token values as the literal text the theme system uses for them, always in the invariant culture.
/// </summary>
public static class ThemeTokenValueFormatter
{
    /// <summary>
    /// Writes a Token value as its literal text in the invariant culture.
    /// </summary>
    /// <typeparam name="T">
    /// The declared type of the value; it does not affect the result, the runtime type selects the format.
    /// </typeparam>
    /// <param name="value">
    /// The value to write. <see langword="null" /> yields the text <c>null</c>.
    /// </param>
    /// <returns>
    /// The literal text. A string is returned unchanged, a number is written with round-trip precision, an easing as
    /// <c>Spline(x1,y1,x2,y2)</c> or <c>Spring(mass,stiffness,damping,velocity)</c>, a transform in the form
    /// <c>Matrix.Parse</c> reads and a <see cref="TimeSpan" /> in its <c>c</c> format. Other values use
    /// <c>ToString</c>, for an <see cref="IFormattable" /> without a format and in the invariant culture.
    /// </returns>
    public static string Format<T>(T value)
    {
        return value switch
        {
            null => "null",
            SplineEasing easing => string.Create(
                CultureInfo.InvariantCulture,
                $"Spline({easing.X1:R},{easing.Y1:R},{easing.X2:R},{easing.Y2:R})"),
            SpringEasing easing => string.Create(
                CultureInfo.InvariantCulture,
                $"Spring({easing.Mass:R},{easing.Stiffness:R},{easing.Damping:R},{easing.InitialVelocity:R})"),
            string text => text,
            bool boolean => boolean ? "true" : "false",
            double number => number.ToString("R", CultureInfo.InvariantCulture),
            float number => number.ToString("R", CultureInfo.InvariantCulture),
            TimeSpan timeSpan => timeSpan.ToString("c", CultureInfo.InvariantCulture),
            // The matrix in Matrix.Parse form, so transform Tokens round-trip and distinct transforms stay distinct.
            ImmutableTransform transform => FormatMatrix(transform.Value),
            Enum enumValue => enumValue.ToString(),
            IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture) ?? string.Empty,
            _ => value.ToString() ?? string.Empty
        };
    }

    private static string FormatMatrix(Matrix matrix)
    {
        return matrix.M13 == 0 && matrix.M23 == 0 && matrix.M33 == 1
            ? string.Create(CultureInfo.InvariantCulture,
                $"{matrix.M11:R},{matrix.M12:R},{matrix.M21:R},{matrix.M22:R},{matrix.M31:R},{matrix.M32:R}")
            : string.Create(CultureInfo.InvariantCulture,
                $"{matrix.M11:R},{matrix.M12:R},{matrix.M13:R},{matrix.M21:R},{matrix.M22:R},{matrix.M23:R},{matrix.M31:R},{matrix.M32:R},{matrix.M33:R}");
    }
}
