using Avalonia.Media;
using ColorValue = Avalonia.Media.Color;

namespace AtomUI.Theme.Algorithms;

public enum PresetColorType
{
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
}

public class PresetPrimaryColor : IEquatable<PresetPrimaryColor>
{
    public static readonly PresetPrimaryColor Red = new(PresetColorType.Red);
    public static readonly PresetPrimaryColor Volcano = new(PresetColorType.Volcano);
    public static readonly PresetPrimaryColor Orange = new(PresetColorType.Orange);
    public static readonly PresetPrimaryColor Gold = new(PresetColorType.Gold);
    public static readonly PresetPrimaryColor Yellow = new(PresetColorType.Yellow);
    public static readonly PresetPrimaryColor Lime = new(PresetColorType.Lime);
    public static readonly PresetPrimaryColor Green = new(PresetColorType.Green);
    public static readonly PresetPrimaryColor Cyan = new(PresetColorType.Cyan);
    public static readonly PresetPrimaryColor Blue = new(PresetColorType.Blue);
    public static readonly PresetPrimaryColor GeekBlue = new(PresetColorType.GeekBlue);
    public static readonly PresetPrimaryColor Purple = new(PresetColorType.Purple);
    public static readonly PresetPrimaryColor Pink = new(PresetColorType.Pink);
    public static readonly PresetPrimaryColor Magenta = new(PresetColorType.Magenta);
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

    public PresetColorType Type { get; }
    private readonly Color _color;

    private PresetPrimaryColor(PresetColorType colorType)
    {
        Type   = colorType;
        _color = ColorValue.FromUInt32(RgbValue());
    }

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

    public Color Color()
    {
        return _color;
    }

    public static IEnumerable<PresetPrimaryColor> AllColorTypes()
    {
        return AllColors;
    }

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

    public bool Equals(PresetPrimaryColor? other)
    {
        return other is not null && Type == other.Type;
    }

    public override bool Equals(object? obj)
    {
        return obj is PresetPrimaryColor other && Equals(other);
    }

    public override int GetHashCode()
    {
        return (int)Type;
    }

    public static bool operator ==(PresetPrimaryColor left, PresetPrimaryColor right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(PresetPrimaryColor left, PresetPrimaryColor right)
    {
        return !left.Equals(right);
    }
}
