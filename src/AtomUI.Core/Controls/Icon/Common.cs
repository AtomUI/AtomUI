namespace AtomUI.Controls;

/// <summary>
/// The looping animation an icon plays as a loading indicator, chosen by <see cref="Icon.LoadingAnimation" />.
/// It runs while the icon is loaded and visible, and one cycle takes <see cref="Icon.LoadingAnimationDuration" />.
/// </summary>
public enum IconAnimation
{
    /// <summary>
    /// No animation. The icon stays still, and a running loading animation stops.
    /// </summary>
    None,
    /// <summary>
    /// One full turn per cycle at a steady speed.
    /// </summary>
    Spin,
    /// <summary>
    /// One full turn per cycle in eight equal steps, shaped by <see cref="PulseEasing" />.
    /// </summary>
    Pulse
}

/// <summary>
/// Names the glyph family an icon belongs to. Each generated icon class sets <see cref="Icon.IconTheme" /> to
/// the family it was generated from; the default is <see cref="Filled" />. The icon theme gives
/// <see cref="Icon.StrokeBrush" /> and <see cref="Icon.FillBrush" /> the <c>ColorIconHover</c> token, except
/// for <see cref="TwoTone" /> and <see cref="MultiColor" />.
/// </summary>
public enum IconThemeType
{
    /// <summary>
    /// A glyph of solid shapes. Every path is filled with <see cref="Icon.FillBrush" />.
    /// </summary>
    Filled,
    /// <summary>
    /// A glyph drawn as outlines. Every path is filled with <see cref="Icon.StrokeBrush" />.
    /// </summary>
    Outlined,
    /// <summary>
    /// A glyph family that no icon in this repository provides. It gets the default brushes of the icon theme.
    /// </summary>
    Rounded,
    /// <summary>
    /// A glyph family that no icon in this repository provides. It gets the default brushes of the icon theme.
    /// </summary>
    Sharp,
    /// <summary>
    /// A glyph in a primary and a secondary layer. Under this theme <see cref="Icon.StrokeBrush" /> takes the
    /// <c>ColorPrimary</c> token and <see cref="Icon.FillBrush" /> the <c>ColorInfoBg</c> token; the primary
    /// layer is filled with the first and the secondary layer with the second.
    /// </summary>
    TwoTone,
    /// <summary>
    /// A glyph whose parts take different colors. Under this theme <see cref="Icon.FillBrush" /> takes the
    /// <c>ColorPrimary</c> token and <see cref="Icon.StrokeBrush" /> keeps the <c>ColorIconHover</c> token.
    /// </summary>
    MultiColor
}

/// <summary>
/// Names one of the brush properties of an <see cref="Icon" />. A <see cref="DrawingInstruction" /> selects
/// one for its pen and one for its fill. The glyphs in this repository select only <see cref="Stroke" /> and
/// <see cref="Fill" />. An icon indexes its brush table with the numeric value, so keep
/// <see cref="None" /> last.
/// </summary>
public enum IconBrushType
{
    /// <summary>
    /// The <see cref="Icon.StrokeBrush" /> of the icon. It fills outlined glyphs and the primary layer of
    /// two-tone glyphs.
    /// </summary>
    Stroke,
    /// <summary>
    /// The <see cref="Icon.FillBrush" /> of the icon. It fills solid glyphs and the secondary layer of two-tone
    /// glyphs.
    /// </summary>
    Fill,
    /// <summary>
    /// The <see cref="Icon.SecondaryStrokeBrush" /> of the icon.
    /// </summary>
    SecondaryStroke,
    /// <summary>
    /// The <see cref="Icon.SecondaryFillBrush" /> of the icon.
    /// </summary>
    SecondaryFill,
    /// <summary>
    /// The <see cref="Icon.FallbackBrush" /> of the icon, the paint for glyph parts that have no resolved color.
    /// </summary>
    Fallback,
    /// <summary>
    /// No brush. <see cref="Icon.FindIconBrush(IconBrushType)" /> returns <see langword="null" /> for it.
    /// </summary>
    None
}