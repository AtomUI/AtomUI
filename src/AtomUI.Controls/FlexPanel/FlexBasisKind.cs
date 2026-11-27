namespace AtomUI.Controls;

/// <summary>
/// How the <see cref="FlexBasis.Value" /> of a <see cref="FlexBasis" /> is interpreted.
/// </summary>
public enum FlexBasisKind
{
    /// <summary>
    /// The child's main-axis size is used: its explicit main size if it has one, otherwise the size it needs for
    /// its content. <see cref="FlexBasis.Value" /> is not used.
    /// </summary>
    Auto,
    /// <summary>
    /// <see cref="FlexBasis.Value" /> is a length in device-independent pixels.
    /// </summary>
    Absolute,
    /// <summary>
    /// <see cref="FlexBasis.Value" /> is a fraction of the panel's main-axis size, so <c>0.5</c> means half of it.
    /// </summary>
    Relative,
}
