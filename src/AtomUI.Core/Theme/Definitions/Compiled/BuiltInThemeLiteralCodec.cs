using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Schema;
using Avalonia;
using Avalonia.Animation.Easings;
using Avalonia.Media;
using Avalonia.Media.Immutable;

namespace AtomUI.Theme.Definitions.Compiled;

// The worker source-links this exact codec and its runtime converters. No parser is
// maintained in the generator, and application/custom converter code is never loaded.
internal static class BuiltInThemeLiteralCodec
{
    internal const string Identity = "atomui-core-literal-v2";
    private static readonly ITokenValueConverter[] Converters =
    [
        new StringTokenValueConverter(),
        new IntegerTokenValueConverter(),
        new DoubleTokenValueConverter(),
        new FloatTokenValueConverter(),
        new BoolTokenValueConverter(),
        new ColorTokenValueConverter(),
        new FontFamilyTokenValueConverter(),
        new BoxShadowsTokenValueConverter(),
        new TextDecorationTokenValueConverter(),
        new LineStyleTokenValueConverter(),
        new ThicknessTokenValueConverter(),
        new CornerRadiusTokenValueConverter(),
        new DimensionTokenValueConverter(),
        new NullableColorTokenValueConverter(),
        new NullableTextDecorationTokenValueConverter(),
        new EasingTokenValueConverter(),
        new BoxShadowTokenValueConverter(),
        new FontWeightTokenValueConverter(),
        new BrushTokenValueConverter(),
        new ImmutableBrushTokenValueConverter(),
        new ImmutableTransformTokenValueConverter(),
        new PenLineCapTokenValueConverter(),
        new PenLineJoinTokenValueConverter(),
        new SolidColorBrushTokenValueConverter(),
        new PointTokenValueConverter(),
        new SizeTokenValueConverter(),
        new TimeSpanTokenValueConverter(),
    ];

    internal static IReadOnlyList<BuiltInThemeLiteralValue> Compile(string text)
    {
        ArgumentNullException.ThrowIfNull(text);
        var result = new List<BuiltInThemeLiteralValue>(Converters.Length);
        foreach (var converter in Converters)
        {
            var type = Nullable.GetUnderlyingType(converter.TargetType()) ?? converter.TargetType();
            if (result.Any(candidate => candidate.Type == type.FullName)) { continue; }
            object parsed;
            try { parsed = converter.Convert(text); }
            catch (Exception error) when (error is FormatException or InvalidOperationException or ArgumentException or OverflowException)
            { continue; }
            var value = Pack(parsed);
            // FontFamily.ToString is not a constructor round-trip for composite families.
            // Font construction is the explicit runtime parsing exception in the compiled ABI.
            if (parsed is FontFamily) { value.Text = text; }
            value.Type = type.FullName!;
            value.Canonical = ThemeTokenValueFormatter.Format(parsed);
            result.Add(value);
        }
        return result;
    }

    private static BuiltInThemeLiteralValue Pack(object parsed)
    {
        var value = new BuiltInThemeLiteralValue();
        switch (parsed)
        {
            case string text: value.Kind = "string"; value.Text = text; break;
            case int number: value.Kind = "int"; value.Integer = number; break;
            case bool flag: value.Kind = "bool"; value.Integer = flag ? 1 : 0; break;
            case double number: value.Kind = "double"; value.Numbers = [number]; break;
            case float number: value.Kind = "float"; value.Numbers = [number]; break;
            case TimeSpan duration: value.Kind = "duration"; value.Integer = duration.Ticks; break;
            case Color color: value.Kind = "color"; value.Integer = color.ToUInt32(); break;
            case ISolidColorBrush brush: value.Kind = "color"; value.Integer = brush.Color.ToUInt32(); break;
            case FontFamily font: value.Kind = "font"; value.Text = font.ToString(); break;
            case Thickness thickness: value.Kind = "numbers"; value.Numbers = [thickness.Left, thickness.Top, thickness.Right, thickness.Bottom]; break;
            case CornerRadius radius: value.Kind = "numbers"; value.Numbers = [radius.TopLeft, radius.TopRight, radius.BottomRight, radius.BottomLeft]; break;
            case Point point: value.Kind = "numbers"; value.Numbers = [point.X, point.Y]; break;
            case Size size: value.Kind = "numbers"; value.Numbers = [size.Width, size.Height]; break;
            case Dimension dimension: value.Kind = "numbers"; value.Numbers = [dimension.Value, (int)dimension.UnitType]; break;
            case ImmutableTransform transform:
                var matrix = transform.Value;
                value.Kind = "numbers"; value.Numbers = [matrix.M11, matrix.M12, matrix.M21, matrix.M22, matrix.M31, matrix.M32]; break;
            case TextDecorationInfo decoration:
                value.Kind = "decoration"; value.Integer = decoration.Color.ToUInt32();
                value.Numbers = [(int)decoration.LineType, (int)decoration.LineStyle, decoration.Thickness]; break;
            case BoxShadow shadow:
                value.Kind = "shadow"; value.Integer = shadow.Color.ToUInt32();
                value.Numbers = [shadow.OffsetX, shadow.OffsetY, shadow.Blur, shadow.Spread, shadow.IsInset ? 1 : 0]; break;
            case BoxShadows shadows:
                value.Kind = "shadows"; value.Items = new BuiltInThemeLiteralValue[shadows.Count];
                for (var index = 0; index < shadows.Count; index++) { value.Items[index] = Pack(shadows[index]); }
                break;
            case SplineEasing easing:
                value.Kind = "spline"; value.Numbers = [easing.X1, easing.Y1, easing.X2, easing.Y2]; break;
            case SpringEasing easing:
                value.Kind = "spring"; value.Numbers = [easing.Mass, easing.Stiffness, easing.Damping, easing.InitialVelocity]; break;
            case Easing easing:
                value.Kind = "easing"; value.Text = easing.GetType().FullName!; break;
            case Enum enumValue:
                value.Kind = "enum"; value.Integer = System.Convert.ToInt64(enumValue); break;
            default: throw new NotSupportedException($"No compiled value representation for '{parsed.GetType()}'.");
        }
        return value;
    }
}
