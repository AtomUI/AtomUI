using Avalonia;

namespace AtomUI.Theme.DesignTokens;

[GlobalDesignToken]
public partial class DesignToken
{
    /// <summary>
    /// A line width one step thicker than <see cref="LineWidth" />, for emphasized lines such as the active bar
    /// of a navigation menu, the ink bar of line tabs and the tail of a timeline item.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Map)]
    public partial double LineWidthBold { get; set; } = 1;

    /// <summary>
    /// The border width of controls: <see cref="LineWidth" /> on all four sides.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Map)]
    public partial Thickness BorderThickness { get; set; }

    /// <summary>
    /// The extra small corner radius, for small inner corners in component styles such as Segmented and Arrow.
    /// It is derived from the top-left corner of the seed <see cref="BorderRadius" />.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Map)]
    public partial CornerRadius BorderRadiusXS { get; set; } = new CornerRadius(2);

    /// <summary>
    /// The small corner radius, used by controls at the small size such as Button, Input and Select.
    /// It is derived from the top-left corner of the seed <see cref="BorderRadius" />.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Map)]
    public partial CornerRadius BorderRadiusSM { get; set; } = new CornerRadius(4);

    /// <summary>
    /// Medium corner radius: the radius of a control at its default size, equal to the seed
    /// <see cref="BorderRadius" />. Controls read this Map token rather than the seed, so a design language
    /// can override it for one control.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Map)]
    public partial CornerRadius BorderRadiusMD { get; set; } = new CornerRadius(6);

    /// <summary>
    /// The large corner radius, for components with a large radius such as Card and Modal.
    /// It is derived from the top-left corner of the seed <see cref="BorderRadius" />.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Map)]
    public partial CornerRadius BorderRadiusLG { get; set; } = new CornerRadius(8);

    /// <summary>
    /// The corner radius of the outer tier, derived from the top-left corner of the seed
    /// <see cref="BorderRadius" />. It is the base radius of the arrow of popups such as tooltips.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Map)]
    public partial CornerRadius BorderRadiusOuter { get; set; } = new CornerRadius(4);

    // Motion
    /// <summary>
    /// The fast motion duration, for animations of small elements. It equals <see cref="MotionBase" />
    /// plus <see cref="MotionUnit" />.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Map)]
    public partial TimeSpan MotionDurationFast { get; set; }

    /// <summary>
    /// The medium motion duration, for animations of medium-sized elements. It equals <see cref="MotionBase" />
    /// plus 2 times <see cref="MotionUnit" />.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Map)]
    public partial TimeSpan MotionDurationMid { get; set; }

    /// <summary>
    /// The slow motion duration, for animations of large elements such as panels. It equals
    /// <see cref="MotionBase" /> plus 3 times <see cref="MotionUnit" />.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Map)]
    public partial TimeSpan MotionDurationSlow { get; set; }

    /// <summary>
    /// The slowest motion duration, for animations of large elements such as panels. It equals
    /// <see cref="MotionBase" /> plus 8 times <see cref="MotionUnit" />.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Map)]
    public partial TimeSpan MotionDurationVerySlow { get; set; }
}