namespace AtomUI.Theme.DesignTokens;

[GlobalDesignToken]
public partial class DesignToken
{
    /// <summary>
    /// The height of controls at the extra small size. By default it is half of <see cref="ControlHeight" />.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Map)]
    public partial double ControlHeightXS { get; set; }

    /// <summary>
    /// The height of controls at the small size. By default it is three quarters of <see cref="ControlHeight" />.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Map)]
    public partial double ControlHeightSM { get; set; }

    /// <summary>
    /// The height of controls at the large size. By default it is one and a quarter times
    /// <see cref="ControlHeight" />.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Map)]
    public partial double ControlHeightLG { get; set; }
}