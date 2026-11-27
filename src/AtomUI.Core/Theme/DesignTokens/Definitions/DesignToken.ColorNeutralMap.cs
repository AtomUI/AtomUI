using Avalonia.Media;

namespace AtomUI.Theme.DesignTokens;

[GlobalDesignToken]
public partial class DesignToken
{
     // ----------   Text   ---------- //
    /// <summary>
    /// The darkest text color and the darkest neutral color. It is the default text color, chosen to meet the
    /// W3C standard.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Map)]
    public partial Color ColorText { get; set; }

    /// <summary>
    /// Second-level text color, for text that needs less emphasis, such as label text or the selected state of
    /// menu text.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Map)]
    public partial Color ColorTextSecondary { get; set; }

    /// <summary>
    /// Third-level text color, for descriptive text such as the supplementary text of a form field or the
    /// description of a list item.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Map)]
    public partial Color ColorTextTertiary { get; set; }

    /// <summary>
    /// Fourth-level text color, the lightest one, for text such as a form placeholder or disabled text.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Map)]
    public partial Color ColorTextQuaternary { get; set; }

    // ----------   Border   ---------- //
    /// <summary>
    /// Default border color, used to separate elements, for example the divider of a form or of a card.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Map)]
    public partial Color ColorBorder { get; set; }

    /// <summary>
    /// Border color one step lighter than <see cref="ColorBorder" />. It is a solid color, not a transparent one,
    /// and it has the same value as <see cref="ColorSplit" />.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Map)]
    public partial Color ColorBorderSecondary { get; set; }

    // ----------   Fill   ---------- //

    /// <summary>
    /// The darkest fill level, clearly stronger than the secondary and tertiary fills.
    /// The Segmented, Skeleton, Splitter and title bar action token rules read it, among others.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Map)]
    public partial Color ColorFill { get; set; }

    /// <summary>
    /// Second-level fill, strong enough to outline the shape of an element, as in Rate and Skeleton.
    /// It also serves as the hover state of the tertiary fill, as in Table.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Map)]
    public partial Color ColorFillSecondary { get; set; }

    /// <summary>
    /// Third-level fill, used to outline the shape of an element, as in Slider and Segmented.
    /// When no emphasis is needed, it is the recommended default fill.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Map)]
    public partial Color ColorFillTertiary { get; set; }

    /// <summary>
    /// Fourth-level fill, the weakest one, for color blocks that should not attract attention,
    /// such as zebra stripes or blocks that mark a boundary.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Map)]
    public partial Color ColorFillQuaternary { get; set; }

    // ----------   Surface   ---------- //
    /// <summary>
    /// Background of the whole page layout. Use it only for regions that must sit on the page layout level of
    /// the visual hierarchy; any other use is wrong.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Map)]
    public partial Color ColorBgLayout { get; set; }

    /// <summary>
    /// Background of a component container, such as a default button or an input field.
    /// Do not confuse it with <see cref="ColorBgElevated" />, the background of floating layers.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Map)]
    public partial Color ColorBgContainer { get; set; }

    /// <summary>
    /// Background of floating layers, such as modal dialogs, popups and menus.
    /// In the dark theme it is lighter than <see cref="ColorBgContainer" />.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Map)]
    public partial Color ColorBgElevated { get; set; }

    /// <summary>
    /// Background that draws strong attention. Currently only the Tooltip background uses it.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Map)]
    public partial Color ColorBgSpotlight { get; set; }

    /// <summary>
    /// Background of frosted-glass containers. It is usually a transparent color.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Map)]
    public partial Color ColorBgBlur { get; set; }
    
    // ----------   Solid   ---------- //
    /// <summary>
    /// Solid background color of the default solid button. Tag also uses it as the background of solid tags.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Map)]
    public partial Color ColorBgSolid { get; set; }
    
    /// <summary>
    /// Active state of the solid background color, used for the pressed effect of the default solid button.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Map)]
    public partial Color ColorBgSolidActive { get; set; }
    
    /// <summary>
    /// Hover state of the solid background color, used for the hover effect of the default solid button.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Map)]
    public partial Color ColorBgSolidHover { get; set; }
}