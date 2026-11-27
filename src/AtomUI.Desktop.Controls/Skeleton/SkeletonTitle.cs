namespace AtomUI.Desktop.Controls;

/// <summary>
/// The title placeholder of a composite skeleton: a single line that is 50% wide by default and is styled as a
/// <see cref="SkeletonLine" />.
/// </summary>
public class SkeletonTitle : SkeletonLine
{
    /// <summary>
    /// Returns <see cref="SkeletonLine" />: the title has no theme of its own and is styled as a line, which the line
    /// theme tells apart by the internal <c>IsTitle</c> flag.
    /// </summary>
    protected override Type StyleKeyOverride { get; } = typeof(SkeletonLine);
    
    static SkeletonTitle()
    {
        LineWidthProperty.OverrideDefaultValue<SkeletonTitle>(new Dimension(50, DimensionUnitType.Percentage));
        IsTitleProperty.OverrideDefaultValue<SkeletonTitle>(true);
    }
    
    public SkeletonTitle()
    {
    }
}