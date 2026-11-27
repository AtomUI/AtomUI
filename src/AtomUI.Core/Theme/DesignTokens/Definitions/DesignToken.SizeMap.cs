namespace AtomUI.Theme.DesignTokens;

[GlobalDesignToken]
public partial class DesignToken
{
    /// <summary>
    /// The largest step of the size gradient. The XXL margin and spacing tokens take this value.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Map)]
    public partial double SizeXXL { get; set; } = 48;

    /// <summary>
    /// XL
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Map)]
    public partial double SizeXL { get; set; } = 32;

    /// <summary>
    /// LG
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Map)]
    public partial double SizeLG { get; set; } = 24;

    /// <summary>
    /// MD
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Map)]
    public partial double SizeMD { get; set; } = 20;

    /// <summary>
    /// Same as size by default, but could be larger in compact mode
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Map)]
    public partial double SizeMS { get; set; }

    /// <summary>
    /// The default size of the size gradient, equal to <see cref="SizeMS" /> outside a compact layout.
    /// It is derived from <see cref="SizeUnit" /> and <see cref="SizeStep" />.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Map)]
    public partial double Size { get; set; } = 16;

    /// <summary>
    /// SM
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Map)]
    public partial double SizeSM { get; set; } = 12;

    /// <summary>
    /// XS
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Map)]
    public partial double SizeXS { get; set; } = 8;

    /// <summary>
    /// XXS
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Map)]
    public partial double SizeXXS { get; set; } = 4;
}