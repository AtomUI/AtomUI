using System.Globalization;
using Avalonia;
using Avalonia.Media.Transformation;
using Avalonia.Utilities;

namespace AtomUI.Media;

public static class TransformParser
{
    private static readonly (string, TransformFunction)[] s_functionMapping =
    {
        ("translate", TransformFunction.Translate),
        ("translateX", TransformFunction.TranslateX),
        ("translateY", TransformFunction.TranslateY),
        ("scale", TransformFunction.Scale),
        ("scaleX", TransformFunction.ScaleX),
        ("scaleY", TransformFunction.ScaleY),
        ("skew", TransformFunction.Skew),
        ("skewX", TransformFunction.SkewX),
        ("skewY", TransformFunction.SkewY),
        ("rotate", TransformFunction.Rotate),
        ("matrix", TransformFunction.Matrix)
    };

    private static readonly (string, Unit)[] s_unitMapping =
    {
        ("deg", Unit.Degree),
        ("grad", Unit.Gradian),
        ("rad", Unit.Radian),
        ("turn", Unit.Turn), 
        ("px", Unit.Pixel)
    };

    public static TransformOperations Parse(string s)
    {
        void ThrowInvalidFormat()
        {
            throw new FormatException($"Invalid transform string: '{s}'.");
        }

        if (string.IsNullOrEmpty(s))
        {
            throw new ArgumentException(nameof(s));
        }

        var span = s.AsSpan().Trim();

        if (span.Equals("none".AsSpan(), StringComparison.OrdinalIgnoreCase))
        {
            return TransformOperations.Identity;
        }

        var builder = TransformOperations.CreateBuilder(0);

        while (true)
        {
            var beginIndex = span.IndexOf('(');
            var endIndex   = span.IndexOf(')');

            if (beginIndex == -1 || endIndex == -1)
            {
                ThrowInvalidFormat();
            }

            var namePart = span.Slice(0, beginIndex).Trim();

            var function = ParseTransformFunction(in namePart);

            if (function == TransformFunction.Invalid)
            {
                ThrowInvalidFormat();
            }

            var valuePart = span.Slice(beginIndex + 1, endIndex - beginIndex - 1).Trim();

            ParseFunction(in valuePart, function, in builder);

            span = span.Slice(endIndex + 1);

            if (span.IsWhiteSpace())
            {
                break;
            }
        }

        return builder.Build();
    }

    private static void ParseFunction(
        in ReadOnlySpan<char> functionPart,
        TransformFunction function,
        in TransformOperations.Builder builder)
    {
        static UnitValue ParseValue(ReadOnlySpan<char> part, int index, TransformFunction function)
        {
            int unitIndex = -1;

            for (int i = 0; i < part.Length; i++)
            {
                char c = part[i];

                if (char.IsDigit(c) || c == '-' || c == '.' || c == 'e' || c == 'E')
                {
                    continue;
                }

                unitIndex = i;
                break;
            }

            Unit unit = Unit.None;

            if (unitIndex != -1)
            {
                var unitPart = part.Slice(unitIndex, part.Length - unitIndex);

                unit = ParseUnit(unitPart);

                part = part.Slice(0, unitIndex);
            }

            var value = double.Parse(part.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture);
            
            // 应用默认单位
            if (function == TransformFunction.Translate ||
                function == TransformFunction.TranslateX ||
                function == TransformFunction.TranslateY)
            {
                if (unit == Unit.None)
                {
                    unit = Unit.Pixel;
                }
            }
            else if (function == TransformFunction.Rotate ||
                     function == TransformFunction.Skew ||
                     function == TransformFunction.SkewX ||
                     function == TransformFunction.SkewY)
            {
                if (unit == Unit.None)
                {
                    if (function == TransformFunction.Rotate)
                    {
                        if (index == 0)
                        {
                            unit = Unit.Degree;
                        }
                        else
                        {
                            unit = Unit.Pixel;
                        }
                    }
                    else
                    {
                        unit = Unit.Degree;
                    }
                }
            }
            return new UnitValue(unit, value);
        }

        static int ParseValuePair(
            in ReadOnlySpan<char> part,
            TransformFunction function,
            ref UnitValue leftValue,
            ref UnitValue rightValue)
        {
            var separatorIndex = part.IndexOf(',');
            if (separatorIndex == -1)
            {
                separatorIndex = part.IndexOf(' ');
            }
            if (separatorIndex != -1)
            {
                var leftPart  = part.Slice(0, separatorIndex).Trim();
                var rightPart = part.Slice(separatorIndex + 1, part.Length - separatorIndex - 1).Trim();

                leftValue  = ParseValue(leftPart, 0, function);
                rightValue = ParseValue(rightPart, 1, function);

                return 2;
            }

            leftValue = ParseValue(part, 0, function);

            return 1;
        }

        static int ParseCommaDelimitedValues(ReadOnlySpan<char> part, in Span<UnitValue> outValues, TransformFunction function)
        {
            int valueIndex = 0;
            int paramPos = 0;
            while (true)
            {
                if (valueIndex >= outValues.Length)
                {
                    throw new FormatException("Too many provided values.");
                }

                part = part.Trim();
                var separatorIndex = part.IndexOf(',');
                if (separatorIndex == -1)
                {
                    separatorIndex = part.IndexOf(' ');
                }

                if (separatorIndex == -1)
                {
                    if (!part.IsWhiteSpace())
                    {
                        outValues[valueIndex++] = ParseValue(part, paramPos++, function);
                    }

                    break;
                }

                var valuePart = part.Slice(0, separatorIndex).Trim();

                outValues[valueIndex++] = ParseValue(valuePart, paramPos++, function);

                part = part.Slice(separatorIndex + 1, part.Length - separatorIndex - 1);
            }

            return valueIndex;
        }

        switch (function)
        {
            case TransformFunction.Scale:
            case TransformFunction.ScaleX:
            case TransformFunction.ScaleY:
            {
                var scaleX = UnitValue.One;
                var scaleY = UnitValue.One;

                int count = ParseValuePair(functionPart, function, ref scaleX, ref scaleY);

                if (count != 1 && (function == TransformFunction.ScaleX || function == TransformFunction.ScaleY))
                {
                    ThrowFormatInvalidValueCount(function, 1);
                }

                VerifyZeroOrUnit(function, in scaleX, Unit.None);
                VerifyZeroOrUnit(function, in scaleY, Unit.None);

                if (function == TransformFunction.ScaleY)
                {
                    scaleY = scaleX;
                    scaleX = UnitValue.One;
                }
                else if (function == TransformFunction.Scale && count == 1)
                {
                    scaleY = scaleX;
                }

                builder.AppendScale(scaleX.Value, scaleY.Value);

                break;
            }
            case TransformFunction.Skew:
            case TransformFunction.SkewX:
            case TransformFunction.SkewY:
            {
                var skewX = UnitValue.Zero;
                var skewY = UnitValue.Zero;

                int count = ParseValuePair(functionPart, function, ref skewX, ref skewY);

                if (count != 1 && (function == TransformFunction.SkewX || function == TransformFunction.SkewY))
                {
                    ThrowFormatInvalidValueCount(function, 1);
                }

                VerifyZeroOrAngle(function, in skewX);
                VerifyZeroOrAngle(function, in skewY);

                if (function == TransformFunction.SkewY)
                {
                    skewY = skewX;
                    skewX = UnitValue.Zero;
                }

                builder.AppendSkew(ToRadians(in skewX), ToRadians(in skewY));

                break;
            }
            case TransformFunction.Rotate:
            {
                Span<UnitValue> values = stackalloc UnitValue[3];
                int count = ParseCommaDelimitedValues(functionPart, in values, function);

                if (count != 1 && count != 3)
                {
                    ThrowFormatInvalidValueCount(function, 1, 3);
                }
                
                VerifyZeroOrAngle(function, in values[0]);

                if (count == 1)
                {
                    builder.AppendRotate(ToRadians(in values[0]));
                }
                else if (count == 3)
                {
                    var angle = values[0];
                    var centerX = values[1];
                    var centerY = values[2];
                    
                    // 验证中心坐标是否有单位
                    if (centerX.Unit != Unit.None && centerX.Unit != Unit.Pixel)
                    {
                        ThrowFormatInvalidValue(function, in centerX);
                    }
                    if (centerY.Unit != Unit.None && centerY.Unit != Unit.Pixel)
                    {
                        ThrowFormatInvalidValue(function, in centerY);
                    }
                    
                    double angleRad = ToRadians(in angle);
                    
                    builder.AppendMatrix(Matrix.CreateRotation(angleRad, new Point(centerX.Value, centerY.Value)));
                }
                break;
            }
            case TransformFunction.Translate:
            case TransformFunction.TranslateX:
            case TransformFunction.TranslateY:
            {
                var translateX = UnitValue.Zero;
                var translateY = UnitValue.Zero;

                int count = ParseValuePair(functionPart, function, ref translateX, ref translateY);

                if (count != 1 && (function == TransformFunction.TranslateX || function == TransformFunction.TranslateY))
                {
                    ThrowFormatInvalidValueCount(function, 1);
                }

                VerifyZeroOrUnit(function, in translateX, Unit.Pixel);
                VerifyZeroOrUnit(function, in translateY, Unit.Pixel);

                if (function == TransformFunction.TranslateY)
                {
                    translateY = translateX;
                    translateX = UnitValue.Zero;
                }

                builder.AppendTranslate(translateX.Value, translateY.Value);

                break;
            }
            case TransformFunction.Matrix:
            {
                Span<UnitValue> values = stackalloc UnitValue[6];

                int count = ParseCommaDelimitedValues(functionPart, in values, function);

                if (count != 6)
                {
                    ThrowFormatInvalidValueCount(function, 6);
                }

                foreach (UnitValue value in values)
                {
                    VerifyZeroOrUnit(function, value, Unit.None);
                }

                var matrix = new Matrix(
                    values[0].Value,
                    values[1].Value,
                    values[2].Value,
                    values[3].Value, 
                    values[4].Value,
                    values[5].Value);

                builder.AppendMatrix(matrix);

                break;
            }
        }
    }

    private static void VerifyZeroOrUnit(TransformFunction function, in UnitValue value, Unit unit)
    {
        bool isZero = value.Unit == Unit.None && value.Value == 0d;

        if (!isZero && value.Unit != unit)
        {
            ThrowFormatInvalidValue(function, in value);
        }
    }

    private static void VerifyZeroOrAngle(TransformFunction function, in UnitValue value)
    {
        if (value.Value != 0d && !IsAngleUnit(value.Unit))
        {
            ThrowFormatInvalidValue(function, in value);
        }
    }

    private static bool IsAngleUnit(Unit unit)
    {
        switch (unit)
        {
            case Unit.Radian:
            case Unit.Gradian:
            case Unit.Degree:
            case Unit.Turn:
            {
                return true;
            }
        }

        return false;
    }

    private static void ThrowFormatInvalidValue(TransformFunction function, in UnitValue value)
    {
        var unitString = value.Unit == Unit.None ? string.Empty : value.Unit.ToString();

        throw new FormatException($"Invalid value {value.Value} {unitString} for {function}");
    }

    private static void ThrowFormatInvalidValueCount(TransformFunction function, int expectedCount)
    {
        throw new FormatException($"Invalid format. {function} expects {expectedCount} value(s).");
    }
    
    private static void ThrowFormatInvalidValueCount(TransformFunction function, int minCount, int maxCount)
    {
        throw new FormatException($"Invalid format. {function} expects between {minCount} and {maxCount} values.");
    }

    private static Unit ParseUnit(in ReadOnlySpan<char> part)
    {
        foreach (var (name, unit) in s_unitMapping)
        {
            if (part.Equals(name.AsSpan(), StringComparison.OrdinalIgnoreCase))
            {
                return unit;
            }
        }

        throw new FormatException($"Invalid unit: {part.ToString()}");
    }

    private static TransformFunction ParseTransformFunction(in ReadOnlySpan<char> part)
    {
        foreach (var (name, transformFunction) in s_functionMapping)
        {
            if (part.Equals(name.AsSpan(), StringComparison.OrdinalIgnoreCase))
            {
                return transformFunction;
            }
        }

        return TransformFunction.Invalid;
    }

    private static double ToRadians(in UnitValue value)
    {
        return value.Unit switch
        {
            Unit.Radian => value.Value,
            Unit.Gradian => MathUtilities.Grad2Rad(value.Value),
            Unit.Degree => MathUtilities.Deg2Rad(value.Value),
            Unit.Turn => MathUtilities.Turn2Rad(value.Value),
            _ => value.Value
        };
    }

    private enum Unit
    {
        None,
        Pixel,
        Radian,
        Gradian,
        Degree,
        Turn
    }

    private readonly struct UnitValue
    {
        public readonly Unit Unit;
        public readonly double Value;

        public UnitValue(Unit unit, double value)
        {
            Unit  = unit;
            Value = value;
        }

        public static UnitValue Zero => new UnitValue(Unit.None, 0);

        public static UnitValue One => new UnitValue(Unit.None, 1);
    }

    private enum TransformFunction
    {
        Invalid,
        Translate,
        TranslateX,
        TranslateY,
        Scale,
        ScaleX,
        ScaleY,
        Skew,
        SkewX,
        SkewY,
        Rotate,
        Matrix
    }
}