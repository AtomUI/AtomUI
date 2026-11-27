using Avalonia.Media;

namespace AtomUI;

/// <summary>
/// Names the position of the line that a text decoration draws, relative to the text.
/// </summary>
public enum TextDecorationLine

{
    /// <summary>
    /// No line is drawn.
    /// </summary>
    None,
    /// <summary>
    /// A line below the text.
    /// </summary>
    Underline,
    /// <summary>
    /// A line above the text.
    /// </summary>
    Overline,
    /// <summary>
    /// A line across the middle of the text.
    /// </summary>
    LineThrough
}

/// <summary>
/// The stroke pattern of a line. The <c>LineType</c> seed token takes a value of this type, and so does
/// <see cref="TextDecorationInfo.LineStyle" />.
/// </summary>
public enum LineStyle
{
    /// <summary>
    /// One unbroken line.
    /// </summary>
    Solid,
    /// <summary>
    /// Two parallel lines.
    /// </summary>
    Double,
    /// <summary>
    /// A line of dots.
    /// </summary>
    Dotted,
    /// <summary>
    /// A line of dashes.
    /// </summary>
    Dashed,
    /// <summary>
    /// A wavy line.
    /// </summary>
    Wavy
}

/// <summary>
/// Names one side of a rectangular control. A motion uses it for the side where layout space is added or removed:
/// <see cref="Top" /> and <see cref="Bottom" /> change the height, <see cref="Left" /> and <see cref="Right" />
/// the width, and the content stays attached to the opposite edge.
/// </summary>
public enum Direction
{
    /// <summary>
    /// The left side.
    /// </summary>
    Left,
    /// <summary>
    /// The top side.
    /// </summary>
    Top,
    /// <summary>
    /// The right side.
    /// </summary>
    Right,
    /// <summary>
    /// The bottom side. It is the default direction of the layout motions.
    /// </summary>
    Bottom
}

/// <summary>
/// Names one corner of a rectangle, or a set of corners. Each corner occupies one bit, so values combine with a
/// bitwise OR; <see cref="All" /> is the combination of the four corners.
/// </summary>
public enum Corner
{
    /// <summary>
    /// No corner.
    /// </summary>
    None = 0x00,
    /// <summary>
    /// The top-left corner.
    /// </summary>
    TopLeft = 0x01,
    /// <summary>
    /// The top-right corner.
    /// </summary>
    TopRight = 0x02,
    /// <summary>
    /// The bottom-left corner.
    /// </summary>
    BottomLeft = 0x04,
    /// <summary>
    /// The bottom-right corner.
    /// </summary>
    BottomRight = 0x08,
    /// <summary>
    /// All four corners.
    /// </summary>
    All = TopLeft | TopRight | BottomLeft | BottomRight
}

/// <summary>
/// Selects the size preset of a control. A preset sets the values that scale with the size of the control, such
/// as its height, padding and icon size.
/// </summary>
public enum SizeType
{
    /// <summary>
    /// The large preset.
    /// </summary>
    Large,
    /// <summary>
    /// The middle preset, between the large and the small one.
    /// </summary>
    Middle,
    /// <summary>
    /// The small preset.
    /// </summary>
    Small
}

/// <summary>
/// Selects the size preset of a control like <see cref="SizeType" />, with the additional choice
/// <see cref="Custom" /> for sizes the application sets itself.
/// </summary>
public enum CustomizableSizeType
{
    /// <summary>
    /// The large preset.
    /// </summary>
    Large,
    /// <summary>
    /// The middle preset.
    /// </summary>
    Middle,
    /// <summary>
    /// The small preset.
    /// </summary>
    Small,
    /// <summary>
    /// No preset. The control uses the sizes the application sets on it. A size that the application does not set
    /// falls back to the middle preset or stays unchanged, depending on the control.
    /// </summary>
    Custom
}

/// <summary>
/// Describes the line of a text decoration, in the sense of the CSS <c>text-decoration</c> property.
/// The <c>LinkDecoration</c>, <c>LinkHoverDecoration</c> and <c>LinkFocusDecoration</c> tokens hold a value
/// of this type, or <see langword="null" /> when no decoration is defined.
/// </summary>
public record struct TextDecorationInfo
{
    public TextDecorationInfo()
    {}
    
    /// <summary>
    /// The color of the line. The default is <c>Gray</c>.
    /// </summary>
    public Color Color { get; set; } = Colors.Gray;
    /// <summary>
    /// The position of the line relative to the text. The default is
    /// <see cref="TextDecorationLine.None" />, which draws no line.
    /// </summary>
    public TextDecorationLine LineType { get; set; } = TextDecorationLine.None;
    /// <summary>
    /// The stroke pattern of the line. The default is <c>Solid</c>.
    /// </summary>
    public LineStyle LineStyle { get; set; } = LineStyle.Solid;
    /// <summary>
    /// The thickness of the line. The default is 1.
    /// </summary>
    public int Thickness { get; set; } = 1;
}

/// <summary>
/// Selects the hexadecimal notation that the <c>HexName</c> extension method of <see cref="Color" /> returns.
/// </summary>
public enum ColorNameFormat
{
    /// <summary>
    /// Six hexadecimal digits, <c>#rrggbb</c>, without the alpha channel. This is the default of <c>HexName</c>.
    /// </summary>
    HexRgb,
    /// <summary>
    /// Eight hexadecimal digits, <c>#aarrggbb</c>, with the alpha channel in front.
    /// </summary>
    HexArgb
}

internal static class AtomUILogArea
{
    public const string Theme = "Theme";
    public const string Control = "Control";
    public const string System = "System";
}