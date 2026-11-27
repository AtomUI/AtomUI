namespace AtomUI.Controls;

/// <summary>
/// The statuses that a tag takes when <see cref="AtomUI.Controls.Commons.AbstractTag.TagColor" /> is a status
/// name. A status tag is painted with the matching status tokens. The status is passed to
/// <c>StatusColorTokenKey</c>, so that a derived tag can name its own token for the main color instead. The
/// status name <c>primary</c> has no member.
/// </summary>
public enum TagStatus
{
    /// <summary>
    /// The status that the name <c>success</c> selects in
    /// <see cref="AtomUI.Controls.Commons.AbstractTag.TagColor" />.
    /// </summary>
    Success,
    /// <summary>
    /// The status that the names <c>info</c> and <c>processing</c> select in
    /// <see cref="AtomUI.Controls.Commons.AbstractTag.TagColor" />.
    /// </summary>
    Info,
    /// <summary>
    /// The status that the name <c>error</c> selects in <see cref="AtomUI.Controls.Commons.AbstractTag.TagColor" />;
    /// <c>danger</c> gives the same colors.
    /// </summary>
    Error,
    /// <summary>
    /// The status that the name <c>warning</c> selects in
    /// <see cref="AtomUI.Controls.Commons.AbstractTag.TagColor" />.
    /// </summary>
    Warning
}

/// <summary>
/// The visual styles a tag can take, which differ in how the background, the text and the border use the color
/// of <see cref="AtomUI.Controls.Commons.AbstractTag.TagColor" />. Each member describes a tag with a color;
/// without one the theme defines a neutral look for each style.
/// </summary>
public enum TagVariant
{
    /// <summary>
    /// A light background tinted with the color, text in the main color and no border. This is the default style.
    /// For a status color the theme can make it paint like <see cref="Solid" /> instead.
    /// </summary>
    Filled,
    /// <summary>
    /// The color as a solid background, with light text on it.
    /// </summary>
    Solid,
    /// <summary>
    /// A light background tinted with the color, text in the main color and a border derived from the color.
    /// </summary>
    Outlined,
    /// <summary>
    /// A transparent background, with text and a border in the main color.
    /// </summary>
    Outline
}
