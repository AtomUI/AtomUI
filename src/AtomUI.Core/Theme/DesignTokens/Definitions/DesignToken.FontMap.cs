namespace AtomUI.Theme.DesignTokens;

[GlobalDesignToken]
public partial class DesignToken
{
    // Font Size
    /// <summary>
    /// The small step of the font size gradient, derived from the seed <see cref="FontSize" />.
    /// <see cref="FontHeightSM" /> is its line height as a length.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Map)]
    public partial double FontSizeSM { get; set; }

    /// <summary>
    /// The large step of the font size gradient, derived from the seed <see cref="FontSize" />.
    /// <see cref="FontHeightLG" /> is its line height as a length.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Map)]
    public partial double FontSizeLG { get; set; }

    /// <summary>
    /// The extra large step of the font size gradient, derived from the seed <see cref="FontSize" />.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Map)]
    public partial double FontSizeXL { get; set; }

    /// <summary>
    /// The font size of a first-level heading (H1), derived from the seed <see cref="FontSize" />.
    /// <see cref="RelativeLineHeightHeading1" /> is its line height.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Map)]
    public partial double FontSizeHeading1 { get; set; } = 38;

    /// <summary>
    /// The font size of a second-level heading (H2), derived from the seed <see cref="FontSize" />.
    /// <see cref="RelativeLineHeightHeading2" /> is its line height.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Map)]
    public partial double FontSizeHeading2 { get; set; } = 30;

    /// <summary>
    /// The font size of a third-level heading (H3), derived from the seed <see cref="FontSize" />.
    /// <see cref="RelativeLineHeightHeading3" /> is its line height.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Map)]
    public partial double FontSizeHeading3 { get; set; } = 24;

    /// <summary>
    /// The font size of a fourth-level heading (H4), derived from the seed <see cref="FontSize" />.
    /// <see cref="RelativeLineHeightHeading4" /> is its line height.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Map)]
    public partial double FontSizeHeading4 { get; set; } = 20;

    /// <summary>
    /// The font size of a fifth-level heading (H5), derived from the seed <see cref="FontSize" />.
    /// <see cref="RelativeLineHeightHeading5" /> is its line height.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Map)]
    public partial double FontSizeHeading5 { get; set; } = 16;

    // LineHeight
    /// <summary>
    /// The line height of body text as a multiple of <see cref="FontSize" />, not as a length.
    /// <see cref="FontHeight" /> is the resulting line height as a length.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Map)]
    public partial double RelativeLineHeight { get; set; }

    /// <summary>
    /// The line height of large text as a multiple of <see cref="FontSizeLG" />, not as a length.
    /// <see cref="FontHeightLG" /> is the resulting line height as a length.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Map)]
    public partial double RelativeLineHeightLG { get; set; }

    /// <summary>
    /// The line height of small text as a multiple of <see cref="FontSizeSM" />, not as a length.
    /// <see cref="FontHeightSM" /> is the resulting line height as a length.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Map)]
    public partial double RelativeLineHeightSM { get; set; }

    // TextHeight
    /// <summary>
    /// Round of fontSize * lineHeight
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Map)]
    public partial double FontHeight { get; set; }

    /// <summary>
    /// Round of fontSizeSM * lineHeightSM
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Map)]
    public partial double FontHeightSM { get; set; }

    /// <summary>
    /// Round of fontSizeLG * lineHeightLG
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Map)]
    public partial double FontHeightLG { get; set; }

    /// <summary>
    /// The line height of a first-level heading (H1) as a multiple of <see cref="FontSizeHeading1" />,
    /// not as a length.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Map)]
    public partial double RelativeLineHeightHeading1 { get; set; }

    /// <summary>
    /// The line height of a second-level heading (H2) as a multiple of <see cref="FontSizeHeading2" />,
    /// not as a length.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Map)]
    public partial double RelativeLineHeightHeading2 { get; set; }

    /// <summary>
    /// The line height of a third-level heading (H3) as a multiple of <see cref="FontSizeHeading3" />,
    /// not as a length.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Map)]
    public partial double RelativeLineHeightHeading3 { get; set; }

    /// <summary>
    /// The line height of a fourth-level heading (H4) as a multiple of <see cref="FontSizeHeading4" />,
    /// not as a length.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Map)]
    public partial double RelativeLineHeightHeading4 { get; set; }

    /// <summary>
    /// The line height of a fifth-level heading (H5) as a multiple of <see cref="FontSizeHeading5" />,
    /// not as a length.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Map)]
    public partial double RelativeLineHeightHeading5 { get; set; }
}