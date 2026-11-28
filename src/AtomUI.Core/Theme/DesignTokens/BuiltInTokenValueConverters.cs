using System.Globalization;
using Avalonia;
using Avalonia.Animation.Easings;
using Avalonia.Media;
using Avalonia.Media.Immutable;

namespace AtomUI.Theme.DesignTokens;

[TokenValueConverter]
internal class StringTokenValueConverter : ITokenValueConverter
{
    public Type TargetType()
    {
        return typeof(string);
    }

    public object Convert(string value)
    {
        return value;
    }
}

[TokenValueConverter]
internal class IntegerTokenValueConverter : ITokenValueConverter
{
    public Type TargetType()
    {
        return typeof(int);
    }

    public object Convert(string value)
    {
        try
        {
            return int.Parse(value, NumberStyles.Integer, CultureInfo.InvariantCulture);
        }
        catch (Exception exception)
        {
            throw new InvalidOperationException($"Convert {value} to int failed.", exception);
        }
    }
}

[TokenValueConverter]
internal class DoubleTokenValueConverter : ITokenValueConverter
{
    public Type TargetType()
    {
        return typeof(double);
    }
    
    public object Convert(string value)
    {
        try
        {
            return double.Parse(value, NumberStyles.Float, CultureInfo.InvariantCulture);
        }
        catch (Exception exception)
        {
            throw new InvalidOperationException($"Convert {value} to double failed.", exception);
        }
    }
}

[TokenValueConverter]
internal class FloatTokenValueConverter : ITokenValueConverter
{
    public Type TargetType()
    {
        return typeof(float);
    }
    
    public object Convert(string value)
    {
        try
        {
            return float.Parse(value, NumberStyles.Float, CultureInfo.InvariantCulture);
        }
        catch (Exception exception)
        {
            throw new InvalidOperationException($"Convert {value} to float failed.", exception);
        }
    }
}

[TokenValueConverter]
internal class BoolTokenValueConverter : ITokenValueConverter
{
    public Type TargetType()
    {
        return typeof(bool);
    }

    public object Convert(string value)
    {
        var isTrue  = string.Compare(value, "true", StringComparison.InvariantCultureIgnoreCase) == 0;
        var isFalse = string.Compare(value, "false", StringComparison.InvariantCultureIgnoreCase) == 0;
        if (!isTrue && !isFalse)
        {
            throw new InvalidOperationException($"Convert {value} to bool failed.");
        }

        if (isTrue)
        {
            return true;
        }

        return false;
    }
}

[TokenValueConverter]
internal class ColorTokenValueConverter : ITokenValueConverter
{
    public Type TargetType()
    {
        return typeof(Color);
    }

    public object Convert(string value)
    {
        try
        {
            return Color.Parse(value);
        }
        catch (Exception exception)
        {
            throw new InvalidOperationException($"Convert {value} to Color failed.", exception);
        }
    }
}

[TokenValueConverter]
internal class FontFamilyTokenValueConverter : ITokenValueConverter
{
    public Type TargetType()
    {
        return typeof(FontFamily);
    }

    public object Convert(string value)
    {
        try
        {
            return FontFamily.Parse(value);
        }
        catch (Exception exception)
        {
            throw new InvalidOperationException($"Convert {value} to FontFamily failed.", exception);
        }
    }
}

[TokenValueConverter]
internal class BoxShadowsTokenValueConverter : ITokenValueConverter
{
    public Type TargetType()
    {
        return typeof(BoxShadows);
    }

    public object Convert(string value)
    {
        try
        {
            return BoxShadows.Parse(value);
        }
        catch (Exception exception)
        {
            throw new InvalidOperationException($"Convert {value} to BoxShadows failed.", exception);
        }
    }
}

[TokenValueConverter]
internal class TextDecorationTokenValueConverter : ITokenValueConverter
{
    public Type TargetType()
    {
        return typeof(TextDecorationInfo);
    }

    public object Convert(string value)
    {
        try
        {
            if (value.IndexOf("none", StringComparison.InvariantCultureIgnoreCase) != -1)
            {
                return new TextDecorationInfo
                {
                    LineType  = TextDecorationLine.None,
                    Thickness = 0
                };
            }

            var textDecoration = new TextDecorationInfo();
            if (ContainStr(value, "underline"))
            {
                textDecoration.LineType = TextDecorationLine.Underline;
            }
            else if (ContainStr(value, "overline"))
            {
                textDecoration.LineType = TextDecorationLine.Overline;
            }
            else if (ContainStr(value, "line-through"))
            {
                textDecoration.LineType = TextDecorationLine.LineThrough;
            }
            else
            {
                throw new InvalidOperationException($"Unsupported line type in value expression {value}.");
            }

            if (ContainStr(value, "solid"))
            {
                textDecoration.LineStyle = LineStyle.Solid;
            }
            else if (ContainStr(value, "double"))
            {
                textDecoration.LineStyle = LineStyle.Double;
            }
            else if (ContainStr(value, "dotted"))
            {
                textDecoration.LineStyle = LineStyle.Dotted;
            }
            else if (ContainStr(value, "dashed"))
            {
                textDecoration.LineStyle = LineStyle.Dashed;
            }
            else if (ContainStr(value, "Wavy"))
            {
                textDecoration.LineStyle = LineStyle.Wavy;
            }
            else
            {
                throw new InvalidOperationException($"Unsupported line style in value expression {value}.");
            }

            var colorRange = FindColorRange(value);
            if (colorRange.Length > 0)
            {
                textDecoration.Color = Color.Parse(value.AsSpan(colorRange.Start, colorRange.Length));
            }

            if (TryReadThickness(value, colorRange.Start, colorRange.Length, out var thickness))
            {
                textDecoration.Thickness = thickness;
            }

            return textDecoration;
        }
        catch (Exception exception)
        {
            throw new InvalidOperationException($"Convert {value} to TextDecorationInfo failed.", exception);
        }
    }

    private static (int Start, int Length) FindColorRange(string valueExpr)
    {
        var count = valueExpr.Length;
        var pos   = valueExpr.IndexOf('#');
        if (pos != -1)
        {
            var endPos = pos;
            while (endPos < count && !char.IsWhiteSpace(valueExpr[endPos]))
            {
                endPos++;
            }

            return (pos, endPos - pos);
        }

        if (ContainStr(valueExpr, "rgb"))
        {
            pos = valueExpr.IndexOf("rgb", StringComparison.InvariantCultureIgnoreCase);
            var endPos = pos;
            while (endPos < count && valueExpr[endPos] != ')')
            {
                endPos++;
            }

            var length = endPos < count
                ? endPos - pos + 1
                : count - pos;
            return (pos, length);
        }

        return (-1, 0);
    }

    private static bool TryReadThickness(string value, int skipStart, int skipLength, out int thickness)
    {
        thickness = 0;
        var alreadySeeNum = false;
        var skipEnd       = skipStart + skipLength;

        for (var i = 0; i < value.Length; ++i)
        {
            if (skipLength > 0 && i >= skipStart && i < skipEnd)
            {
                continue;
            }

            var cur = value[i];
            if (alreadySeeNum && !char.IsDigit(cur))
            {
                break;
            }

            if (char.IsDigit(cur))
            {
                alreadySeeNum = true;
                checked
                {
                    thickness = thickness * 10 + cur - '0';
                }
            }
        }

        return alreadySeeNum;
    }

    private static bool ContainStr(string expr, string searched)
    {
        return expr.IndexOf(searched, StringComparison.InvariantCultureIgnoreCase) != -1;
    }
}

[TokenValueConverter]
internal class LineStyleTokenValueConverter : ITokenValueConverter
{
    public Type TargetType()
    {
        return typeof(LineStyle);
    }

    public object Convert(string value)
    {
        return value switch
        {
            nameof(LineStyle.Solid) => LineStyle.Solid,
            nameof(LineStyle.Double) => LineStyle.Double,
            nameof(LineStyle.Dotted) => LineStyle.Dotted,
            nameof(LineStyle.Dashed) => LineStyle.Dashed,
            nameof(LineStyle.Wavy) => LineStyle.Wavy,
            _ => throw new InvalidOperationException($"Unsupported line style in value expression {value}.")
        };
    }
}

[TokenValueConverter]
internal class ThicknessTokenValueConverter : ITokenValueConverter
{
    public Type TargetType()
    {
        return typeof(Thickness);
    }

    public object Convert(string value)
    {
        try
        {
            return Thickness.Parse(value);
        }
        catch (Exception exception)
        {
            throw new InvalidOperationException($"Convert {value} to Thickness failed.", exception);
        }
    }
}

[TokenValueConverter]
internal class CornerRadiusTokenValueConverter : ITokenValueConverter
{
    public Type TargetType()
    {
        return typeof(CornerRadius);
    }

    public object Convert(string value)
    {
        try
        {
            return CornerRadius.Parse(value);
        }
        catch (Exception exception)
        {
            throw new InvalidOperationException($"Convert {value} to CornerRadius failed.", exception);
        }
    }
}

[TokenValueConverter]
internal sealed class DimensionTokenValueConverter : ITokenValueConverter
{
    public Type TargetType() => typeof(Dimension);

    public object Convert(string value)
    {
        return TokenValueParserHelper.Parse(value, Dimension.Parse, nameof(Dimension));
    }
}

[TokenValueConverter]
internal sealed class NullableColorTokenValueConverter : ITokenValueConverter
{
    public Type TargetType() => typeof(Color?);

    public object Convert(string value)
    {
        return TokenValueParserHelper.Parse(value, Color.Parse, "Color?");
    }
}

[TokenValueConverter]
internal sealed class NullableTextDecorationTokenValueConverter : ITokenValueConverter
{
    private static readonly TextDecorationTokenValueConverter s_converter = new();

    public Type TargetType() => typeof(TextDecorationInfo?);

    public object Convert(string value) => s_converter.Convert(value);
}

[TokenValueConverter]
internal sealed class NullableCornerRadiusTokenValueConverter : ITokenValueConverter
{
    public Type TargetType() => typeof(CornerRadius?);

    public object Convert(string value)
    {
        return TokenValueParserHelper.Parse(value, CornerRadius.Parse, "CornerRadius?");
    }
}

[TokenValueConverter]
internal sealed class EasingTokenValueConverter : ITokenValueConverter
{
    public Type TargetType() => typeof(Easing);

    public object Convert(string value)
    {
        return TokenValueParserHelper.Parse(value, ParseEasing, nameof(Easing));
    }

    private static Easing ParseEasing(string value)
    {
        var text = value.AsSpan().Trim();
        var spring = text.StartsWith("Spring(", StringComparison.OrdinalIgnoreCase);
        if (!spring && !text.StartsWith("Spline(", StringComparison.OrdinalIgnoreCase))
        {
            return Easing.Parse(value);
        }

        Span<Range> arguments = stackalloc Range[5];
        if (text[^1] != ')' || text[7..^1].Split(arguments, ',', StringSplitOptions.TrimEntries) != 4)
        {
            throw new FormatException("Expected four easing parameters.");
        }

        var body = text[7..^1];
        Span<double> parameters = stackalloc double[4];
        for (var index = 0; index < parameters.Length; index++)
        {
            if (!double.TryParse(body[arguments[index]], NumberStyles.Float, CultureInfo.InvariantCulture, out var number) ||
                !double.IsFinite(number))
            {
                throw new FormatException("Easing parameters must be finite numbers.");
            }
            parameters[index] = number;
        }

        if (!spring)
        {
            return new SplineEasing(parameters[0], parameters[1], parameters[2], parameters[3]);
        }

        if (parameters[0] <= 0 || parameters[1] <= 0 || parameters[2] < 0)
        {
            throw new FormatException("Spring mass and stiffness must be positive and damping must be nonnegative.");
        }

        return new SpringEasing(parameters[0], parameters[1], parameters[2], parameters[3]);
    }
}

[TokenValueConverter]
internal sealed class BoxShadowTokenValueConverter : ITokenValueConverter
{
    public Type TargetType() => typeof(BoxShadow);

    public object Convert(string value)
    {
        return TokenValueParserHelper.Parse(value, BoxShadow.Parse, nameof(BoxShadow));
    }
}

[TokenValueConverter]
internal sealed class FontWeightTokenValueConverter : ITokenValueConverter
{
    public Type TargetType() => typeof(FontWeight);

    public object Convert(string value) => TokenValueParserHelper.ParseEnum<FontWeight>(value);
}

[TokenValueConverter]
internal sealed class BrushTokenValueConverter : ITokenValueConverter
{
    public Type TargetType() => typeof(IBrush);

    public object Convert(string value)
    {
        return TokenValueParserHelper.Parse(value, Brush.Parse, nameof(IBrush));
    }
}

[TokenValueConverter]
internal sealed class ImmutableBrushTokenValueConverter : ITokenValueConverter
{
    public Type TargetType() => typeof(IImmutableBrush);

    public object Convert(string value)
    {
        var brush = TokenValueParserHelper.Parse(value, Brush.Parse, nameof(IImmutableBrush));
        return brush switch
        {
            IImmutableBrush immutableBrush => immutableBrush,
            _ => throw new InvalidOperationException(
                $"Convert {value} to {nameof(IImmutableBrush)} returned an unsupported brush type.")
        };
    }
}

[TokenValueConverter]
internal sealed class ImmutableTransformTokenValueConverter : ITokenValueConverter
{
    public Type TargetType() => typeof(ImmutableTransform);

    public object Convert(string value)
    {
        var matrix = TokenValueParserHelper.Parse(value, Matrix.Parse, nameof(Matrix));
        return new ImmutableTransform(matrix);
    }
}

[TokenValueConverter]
internal sealed class PenLineCapTokenValueConverter : ITokenValueConverter
{
    public Type TargetType() => typeof(PenLineCap);

    public object Convert(string value) => TokenValueParserHelper.ParseEnum<PenLineCap>(value);
}

[TokenValueConverter]
internal sealed class PenLineJoinTokenValueConverter : ITokenValueConverter
{
    public Type TargetType() => typeof(PenLineJoin);

    public object Convert(string value) => TokenValueParserHelper.ParseEnum<PenLineJoin>(value);
}

[TokenValueConverter]
internal sealed class SolidColorBrushTokenValueConverter : ITokenValueConverter
{
    public Type TargetType() => typeof(SolidColorBrush);

    public object Convert(string value)
    {
        return TokenValueParserHelper.Parse(value, SolidColorBrush.Parse, nameof(SolidColorBrush));
    }
}

[TokenValueConverter]
internal sealed class PointTokenValueConverter : ITokenValueConverter
{
    public Type TargetType() => typeof(Point);

    public object Convert(string value)
    {
        return TokenValueParserHelper.Parse(value, Point.Parse, nameof(Point));
    }
}

[TokenValueConverter]
internal sealed class SizeTokenValueConverter : ITokenValueConverter
{
    public Type TargetType() => typeof(Size);

    public object Convert(string value)
    {
        return TokenValueParserHelper.Parse(value, Size.Parse, nameof(Size));
    }
}

[TokenValueConverter]
internal sealed class TimeSpanTokenValueConverter : ITokenValueConverter
{
    public Type TargetType() => typeof(TimeSpan);

    public object Convert(string value)
    {
        try
        {
            return TimeSpan.ParseExact(value, "c", CultureInfo.InvariantCulture, TimeSpanStyles.None);
        }
        catch (Exception exception)
        {
            throw new InvalidOperationException($"Convert {value} to {nameof(TimeSpan)} failed.", exception);
        }
    }
}

internal static class TokenValueParserHelper
{
    internal static T Parse<T>(string value, Func<string, T> parser, string targetType)
    {
        try
        {
            return parser(value);
        }
        catch (Exception exception)
        {
            throw new InvalidOperationException($"Convert {value} to {targetType} failed.", exception);
        }
    }

    internal static TEnum ParseEnum<TEnum>(string value)
        where TEnum : struct, Enum
    {
        if (Enum.TryParse<TEnum>(value, false, out var parsed) && Enum.IsDefined(parsed))
        {
            return parsed;
        }

        throw new InvalidOperationException($"Convert {value} to {typeof(TEnum).Name} failed.");
    }
}
