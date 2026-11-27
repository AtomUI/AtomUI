namespace AtomUI.Controls;

/// <summary>
/// Pseudo-class selectors that mark which kind of color a tag resolved its
/// <see cref="AtomUI.Controls.Commons.AbstractTag.TagColor" /> to. At most one of them is set on a tag, and none
/// is set while <see cref="AtomUI.Controls.Commons.AbstractTag.TagColor" /> selects no color.
/// </summary>
public static class TagPseudoClass
{
    /// <summary>
    /// The pseudo-class of a tag whose <see cref="AtomUI.Controls.Commons.AbstractTag.TagColor" /> is a color
    /// string for a custom color, not a preset color name or a status name.
    /// </summary>
    public const string CustomColor = ":custom-color";
    /// <summary>
    /// The pseudo-class of a tag whose <see cref="AtomUI.Controls.Commons.AbstractTag.TagColor" /> is a preset
    /// color name such as <c>red</c>.
    /// </summary>
    public const string PresetColor = ":preset-color";
    /// <summary>
    /// The pseudo-class of a tag whose <see cref="AtomUI.Controls.Commons.AbstractTag.TagColor" /> is a status
    /// name such as <c>success</c>.
    /// </summary>
    public const string StatusColor = ":status-color";
}