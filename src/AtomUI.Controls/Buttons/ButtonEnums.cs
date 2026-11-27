// Keep the CLR namespace so Desktop can forward the existing public enum types.
namespace AtomUI.Desktop.Controls;

/// <summary>
/// Frame shape of a button, chosen with the <c>Shape</c> property of a button that derives from
/// <c>AbstractButton</c>. Each value fixes the aspect ratio of the frame, the source of its corner radius, or both.
/// </summary>
public enum ButtonShape
{
    /// <summary>
    /// Keeps the size measured from the content. The corner radius is the one of the theme, which a hosting control
    /// or a compact space may adjust.
    /// </summary>
    Default,
    /// <summary>
    /// Measures square and rounds the frame into a circle whose radius follows the measured height. The padding is
    /// the circle padding of the theme instead of the content padding.
    /// </summary>
    Circle,
    /// <summary>
    /// Measures at least one and a half times as wide as high and rounds both ends with a radius that follows the
    /// measured height, which gives the frame the shape of a pill.
    /// </summary>
    Round,
    /// <summary>
    /// Keeps the size measured from the content and draws square corners, ignoring the corner radius of the theme.
    /// </summary>
    Rectangular
}

/// <summary>
/// Side of the content that the icon of a button is placed on, chosen with the <c>IconPlacement</c> property of a
/// button that derives from <c>AbstractButton</c>.
/// </summary>
public enum ButtonIconPlacement
{
    /// <summary>
    /// Places the icon to the left of the content. This is the default of <c>IconPlacement</c>.
    /// </summary>
    Start,
    /// <summary>
    /// Places the icon to the right of the content.
    /// </summary>
    End
}

/// <summary>
/// Color scheme a button is painted with, chosen with the <c>Color</c> property of a button that derives from
/// <c>AbstractButton</c>. <see cref="ButtonColor.Default" />, <see cref="ButtonColor.Primary" />,
/// <see cref="ButtonColor.Danger" />, <see cref="ButtonColor.Success" /> and <see cref="ButtonColor.Warning" /> use
/// Tokens of the theme; every other value uses the palette of a preset primary color.
/// </summary>
public enum ButtonColor
{
    /// <summary>
    /// The neutral scheme, painted with the default Tokens of the button.
    /// </summary>
    Default,
    /// <summary>
    /// Paints the button in the primary color of the theme, with the hover, active, background and border colors of
    /// the primary palette.
    /// </summary>
    Primary,
    /// <summary>
    /// The error color of the theme, for a destructive action. It is the color a button falls back to while
    /// <c>IsDanger</c> is set.
    /// </summary>
    Danger,
    /// <summary>
    /// Paints the button with the palette of the <c>Red</c> preset primary color.
    /// </summary>
    Red,
    /// <summary>
    /// Paints the button with the palette of the <c>Volcano</c> preset primary color.
    /// </summary>
    Volcano,
    /// <summary>
    /// Paints the button with the palette of the <c>Orange</c> preset primary color.
    /// </summary>
    Orange,
    /// <summary>
    /// Paints the button with the palette of the <c>Gold</c> preset primary color.
    /// </summary>
    Gold,
    /// <summary>
    /// Paints the button with the palette of the <c>Yellow</c> preset primary color.
    /// </summary>
    Yellow,
    /// <summary>
    /// Paints the button with the palette of the <c>Lime</c> preset primary color.
    /// </summary>
    Lime,
    /// <summary>
    /// Paints the button with the palette of the <c>Green</c> preset primary color.
    /// </summary>
    Green,
    /// <summary>
    /// Paints the button with the palette of the <c>Cyan</c> preset primary color.
    /// </summary>
    Cyan,
    /// <summary>
    /// Paints the button with the palette of the <c>Blue</c> preset primary color.
    /// </summary>
    Blue,
    /// <summary>
    /// Paints the button with the palette of the <c>GeekBlue</c> preset primary color.
    /// </summary>
    GeekBlue,
    /// <summary>
    /// Paints the button with the palette of the <c>Purple</c> preset primary color.
    /// </summary>
    Purple,
    /// <summary>
    /// Paints the button with the palette of the <c>Pink</c> preset primary color.
    /// </summary>
    Pink,
    /// <summary>
    /// Paints the button with the palette of the <c>Magenta</c> preset primary color. Its color is the same as that of
    /// <see cref="ButtonColor.Pink" />, but it is a different preset.
    /// </summary>
    Magenta,
    /// <summary>
    /// Paints the button with the palette of the <c>Grey</c> preset primary color.
    /// </summary>
    Grey,
    /// <summary>
    /// The success color of the theme and the hover, active, background and border Tokens that belong to it.
    /// </summary>
    Success,
    /// <summary>
    /// The warning color of the theme and the hover, active, background and border Tokens that belong to it.
    /// </summary>
    Warning
}

/// <summary>
/// Painting style of a button, chosen with the <c>Variant</c> property of a button that derives from
/// <c>AbstractButton</c>. Each value sets how the background, the border and the text are drawn in the color
/// scheme of the button.
/// </summary>
public enum ButtonVariant
{
    /// <summary>
    /// Draws the text and a solid border in the scheme color, on the default background of the button.
    /// </summary>
    Outlined,
    /// <summary>
    /// Draws the same colors as <see cref="ButtonVariant.Outlined" />, with a dashed border.
    /// </summary>
    Dashed,
    /// <summary>
    /// Fills the background with the base color of the scheme and draws the text in the solid text color.
    /// </summary>
    Solid,
    /// <summary>
    /// Draws the text in the scheme color on the light background color of the scheme, without a border.
    /// </summary>
    Filled,
    /// <summary>
    /// Draws the text in the scheme color without a border. The background appears only while the button is hovered
    /// or pressed.
    /// </summary>
    Text,
    /// <summary>
    /// Draws the text without a border and with a transparent background. In the neutral scheme the text takes the
    /// link color of the theme, and the design language decides whether the text is underlined at rest or on hover.
    /// </summary>
    Link
}
