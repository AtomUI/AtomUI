namespace AtomUI.Controls;

/// <summary>
/// An entry of the <c>Options</c> of a checkable tag group that keeps the value the tag is checked by apart from
/// the content it displays.
/// </summary>
public interface ICheckableTagOption
{
    /// <summary>
    /// Identifies the entry: the tag is checked when the checked values of the group contain this value. It must not
    /// be <see langword="null" />: the group throws an <see cref="System.ArgumentException" /> when it builds its
    /// tags from an entry with a <see langword="null" /> value.
    /// </summary>
    object Value { get; }

    /// <summary>
    /// What the tag displays when the group sets no <c>ItemTemplate</c>; <see langword="null" /> leaves the tag
    /// without content.
    /// </summary>
    object? Content { get; }
}

/// <summary>
/// A ready-made entry for the <c>Options</c> of a checkable tag group that keeps the checked value and the
/// displayed content apart.
/// </summary>
public class CheckableTagOption : ICheckableTagOption
{
    /// <summary>
    /// Identifies the entry: the tag is checked when the checked values of the group contain this value. It has no
    /// default and must not be <see langword="null" />.
    /// </summary>
    public object Value { get; set; } = null!;

    /// <summary>
    /// What the tag displays when the group sets no <c>ItemTemplate</c>; <see langword="null" /> leaves the tag
    /// without content.
    /// </summary>
    public object? Content { get; set; }
}
