using Avalonia.Media;
using ColorValue = Avalonia.Media.Color;

namespace AtomUI.Theme.Derivation;

/// <summary>
/// The types of the fourteen preset primary colors. Each type has one instance in
/// <see cref="PresetPrimaryColor" />.
/// </summary>
public enum PresetColorType
{
    /// <summary>
    /// The red preset.
    /// </summary>
    Red,
    /// <summary>
    /// The volcano preset.
    /// </summary>
    Volcano,
    /// <summary>
    /// The orange preset.
    /// </summary>
    Orange,
    /// <summary>
    /// The gold preset.
    /// </summary>
    Gold,
    /// <summary>
    /// The yellow preset.
    /// </summary>
    Yellow,
    /// <summary>
    /// The lime preset.
    /// </summary>
    Lime,
    /// <summary>
    /// The green preset.
    /// </summary>
    Green,
    /// <summary>
    /// The cyan preset.
    /// </summary>
    Cyan,
    /// <summary>
    /// The blue preset.
    /// </summary>
    Blue,
    /// <summary>
    /// The geek blue preset.
    /// </summary>
    GeekBlue,
    /// <summary>
    /// The purple preset.
    /// </summary>
    Purple,
    /// <summary>
    /// The pink preset. It has the same color as <see cref="Magenta" /> but is a different type.
    /// </summary>
    Pink,
    /// <summary>
    /// The magenta preset. It has the same color as <see cref="Pink" /> but is a different type.
    /// </summary>
    Magenta,
    /// <summary>
    /// The gray preset.
    /// </summary>
    Grey
}

/// <summary>
/// A preset primary color with its name and its color, the base of a preset palette. Instances exist only as the
/// static members of this class; two of them are equal when their <see cref="Type" /> is equal.
/// </summary>
public class PresetPrimaryColor : IEquatable<PresetPrimaryColor>
{
    /// <summary>
    /// The red preset primary color, of type <see cref="PresetColorType.Red" />.
    /// </summary>
    public static readonly PresetPrimaryColor Red = new(PresetColorType.Red);
    /// <summary>
    /// The volcano preset primary color, of type <see cref="PresetColorType.Volcano" />.
    /// </summary>
    public static readonly PresetPrimaryColor Volcano = new(PresetColorType.Volcano);
    /// <summary>
    /// The orange preset primary color, of type <see cref="PresetColorType.Orange" />.
    /// </summary>
    public static readonly PresetPrimaryColor Orange = new(PresetColorType.Orange);
    /// <summary>
    /// The gold preset primary color, of type <see cref="PresetColorType.Gold" />.
    /// </summary>
    public static readonly PresetPrimaryColor Gold = new(PresetColorType.Gold);
    /// <summary>
    /// The yellow preset primary color, of type <see cref="PresetColorType.Yellow" />.
    /// </summary>
    public static readonly PresetPrimaryColor Yellow = new(PresetColorType.Yellow);
    /// <summary>
    /// The lime preset primary color, of type <see cref="PresetColorType.Lime" />.
    /// </summary>
    public static readonly PresetPrimaryColor Lime = new(PresetColorType.Lime);
    /// <summary>
    /// The green preset primary color, of type <see cref="PresetColorType.Green" />.
    /// </summary>
    public static readonly PresetPrimaryColor Green = new(PresetColorType.Green);
    /// <summary>
    /// The cyan preset primary color, of type <see cref="PresetColorType.Cyan" />.
    /// </summary>
    public static readonly PresetPrimaryColor Cyan = new(PresetColorType.Cyan);
    /// <summary>
    /// The blue preset primary color, of type <see cref="PresetColorType.Blue" />.
    /// </summary>
    public static readonly PresetPrimaryColor Blue = new(PresetColorType.Blue);
    /// <summary>
    /// The geek blue preset primary color, of type <see cref="PresetColorType.GeekBlue" />.
    /// </summary>
    public static readonly PresetPrimaryColor GeekBlue = new(PresetColorType.GeekBlue);
    /// <summary>
    /// The purple preset primary color, of type <see cref="PresetColorType.Purple" />.
    /// </summary>
    public static readonly PresetPrimaryColor Purple = new(PresetColorType.Purple);
    /// <summary>
    /// The pink preset primary color, of type <see cref="PresetColorType.Pink" />.
    /// </summary>
    public static readonly PresetPrimaryColor Pink = new(PresetColorType.Pink);
    /// <summary>
    /// The magenta preset primary color, of type <see cref="PresetColorType.Magenta" />.
    /// </summary>
    public static readonly PresetPrimaryColor Magenta = new(PresetColorType.Magenta);
    /// <summary>
    /// The gray preset primary color, of type <see cref="PresetColorType.Grey" />.
    /// </summary>
    public static readonly PresetPrimaryColor Grey = new(PresetColorType.Grey);

    private static readonly IReadOnlyList<PresetPrimaryColor> AllColors = Array.AsReadOnly<PresetPrimaryColor>(
    [
        Red,
        Volcano,
        Orange,
        Gold,
        Yellow,
        Lime,
        Green,
        Cyan,
        Blue,
        GeekBlue,
        Purple,
        Pink,
        Magenta,
        Grey
    ]);

    /// <summary>
    /// The preset color type. It is the identity of this color.
    /// </summary>
    public PresetColorType Type { get; }
    private readonly Color _color;

    private PresetPrimaryColor(PresetColorType colorType)
    {
        Type   = colorType;
        _color = ColorValue.FromUInt32(RgbValue());
    }

    /// <summary>
    /// Returns the name of the preset, which is the name of its <see cref="PresetColorType" /> member.
    /// </summary>
    /// <returns>
    /// The preset name, for example <c>Red</c>.
    /// </returns>
    public string Name()
    {
        return Type switch
        {
            PresetColorType.Red => nameof(PresetColorType.Red),
            PresetColorType.Volcano => nameof(PresetColorType.Volcano),
            PresetColorType.Orange => nameof(PresetColorType.Orange),
            PresetColorType.Gold => nameof(PresetColorType.Gold),
            PresetColorType.Yellow => nameof(PresetColorType.Yellow),
            PresetColorType.Lime => nameof(PresetColorType.Lime),
            PresetColorType.Green => nameof(PresetColorType.Green),
            PresetColorType.Cyan => nameof(PresetColorType.Cyan),
            PresetColorType.Blue => nameof(PresetColorType.Blue),
            PresetColorType.GeekBlue => nameof(PresetColorType.GeekBlue),
            PresetColorType.Purple => nameof(PresetColorType.Purple),
            PresetColorType.Pink => nameof(PresetColorType.Pink),
            PresetColorType.Magenta => nameof(PresetColorType.Magenta),
            PresetColorType.Grey => nameof(PresetColorType.Grey),
            _ => nameof(PresetColorType.Grey)
        };
    }

    /// <summary>
    /// Returns the color as an uppercase six-digit hexadecimal string with a leading <c>#</c>.
    /// </summary>
    /// <returns>
    /// The color, for example <c>#1677FF</c>.
    /// </returns>
    public string RgbHex()
    {
        return "#" + (RgbValue() & 0xFFFFFF).ToString("X6", System.Globalization.CultureInfo.InvariantCulture);
    }

    private uint RgbValue()
    {
        return Type switch
        {
            PresetColorType.Red => 0xFFF5222Du,
            PresetColorType.Volcano => 0xFFFA541Cu,
            PresetColorType.Orange => 0xFFFA8C16u,
            PresetColorType.Gold => 0xFFFAAD14u,
            PresetColorType.Yellow => 0xFFFADB14u,
            PresetColorType.Lime => 0xFFA0D911u,
            PresetColorType.Green => 0xFF52C41Au,
            PresetColorType.Cyan => 0xFF13C2C2u,
            PresetColorType.Blue => 0xFF1677FFu,
            PresetColorType.GeekBlue => 0xFF2F54EBu,
            PresetColorType.Purple => 0xFF722ED1u,
            PresetColorType.Magenta => 0xFFEB2F96u,
            PresetColorType.Pink => 0xFFEB2F96u,
            PresetColorType.Grey => 0xFF666666u,
            _ => 0xFF666666u
        };
    }

    /// <summary>
    /// Returns the color of the preset.
    /// </summary>
    public Color Color()
    {
        return _color;
    }

    /// <summary>
    /// Returns every preset primary color in the order of <see cref="PresetColorType" />, from
    /// <see cref="Red" /> to <see cref="Grey" />.
    /// </summary>
    /// <returns>
    /// The fourteen preset colors.
    /// </returns>
    public static IEnumerable<PresetPrimaryColor> AllColorTypes()
    {
        return AllColors;
    }

    /// <summary>
    /// Returns the preset primary color of a given type.
    /// </summary>
    /// <param name="type">
    /// The preset color type.
    /// </param>
    /// <returns>
    /// The static preset color of that type.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="type" /> is not a value of <see cref="PresetColorType" />.
    /// </exception>
    public static PresetPrimaryColor GetColor(PresetColorType type)
    {
        return type switch
        {
            PresetColorType.Red => Red,
            PresetColorType.Volcano => Volcano,
            PresetColorType.Orange => Orange,
            PresetColorType.Gold => Gold,
            PresetColorType.Yellow => Yellow,
            PresetColorType.Lime => Lime,
            PresetColorType.Green => Green,
            PresetColorType.Cyan => Cyan,
            PresetColorType.Blue => Blue,
            PresetColorType.GeekBlue => GeekBlue,
            PresetColorType.Purple => Purple,
            PresetColorType.Pink => Pink,
            PresetColorType.Magenta => Magenta,
            PresetColorType.Grey => Grey,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Invalid value for PresetColorType")
        };
    }

    /// <summary>
    /// Returns whether this color and <paramref name="other" /> are the same preset, by <see cref="Type" />.
    /// </summary>
    /// <param name="other">
    /// The preset color to compare with, or <see langword="null" />.
    /// </param>
    /// <returns>
    /// <see langword="true" /> when <paramref name="other" /> is not <see langword="null" /> and has the same
    /// <see cref="Type" />.
    /// </returns>
    public bool Equals(PresetPrimaryColor? other)
    {
        return other is not null && Type == other.Type;
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return obj is PresetPrimaryColor other && Equals(other);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return (int)Type;
    }

    /// <summary>
    /// Returns whether two preset colors are the same preset, by <see cref="Type" />.
    /// </summary>
    /// <param name="left">
    /// The first preset color. It must not be <see langword="null" />.
    /// </param>
    /// <param name="right">
    /// The second preset color, or <see langword="null" />.
    /// </param>
    /// <returns>
    /// <see langword="true" /> when <paramref name="right" /> is not <see langword="null" /> and its
    /// <see cref="Type" /> equals that of <paramref name="left" />.
    /// </returns>
    /// <exception cref="NullReferenceException">
    /// <paramref name="left" /> is <see langword="null" />.
    /// </exception>
    public static bool operator ==(PresetPrimaryColor left, PresetPrimaryColor right)
    {
        return left.Equals(right);
    }

    /// <summary>
    /// Returns whether two preset colors are different presets, by <see cref="Type" />.
    /// </summary>
    /// <param name="left">
    /// The first preset color. It must not be <see langword="null" />.
    /// </param>
    /// <param name="right">
    /// The second preset color, or <see langword="null" />.
    /// </param>
    /// <returns>
    /// <see langword="true" /> when <paramref name="right" /> is <see langword="null" /> or has a different
    /// <see cref="Type" />.
    /// </returns>
    /// <exception cref="NullReferenceException">
    /// <paramref name="left" /> is <see langword="null" />.
    /// </exception>
    public static bool operator !=(PresetPrimaryColor left, PresetPrimaryColor right)
    {
        return !left.Equals(right);
    }
}
