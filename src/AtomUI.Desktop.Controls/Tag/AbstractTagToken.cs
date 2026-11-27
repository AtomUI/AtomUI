using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Own Tokens shared by <see cref="Tag" /> and <see cref="CheckableTag" /> (antd Tag <c>tagFontSize</c>,
/// <c>tagLineHeight</c>, <c>tagIconSize</c> and the tag paddings). Each control has its own terminal Token, so
/// <c>Controls[Tag]</c> and <c>Controls[CheckableTag]</c> compute and override these values independently.
/// </summary>
[ControlDesignToken]
public abstract partial class AbstractTagToken : AbstractControlDesignToken
{
    /// <summary>
    /// Font size of the tag text.
    /// </summary>
    public partial double TagFontSize { get; set; }

    /// <summary>
    /// Font weight of the tag text.
    /// </summary>
    public partial FontWeight TagFontWeight { get; set; }

    /// <summary>
    /// Line height of the tag text.
    /// </summary>
    public partial double TagLineHeight { get; set; }

    /// <summary>
    /// Width and height of the icon shown beside the tag text.
    /// </summary>
    public partial double TagIconSize { get; set; }

    /// <summary>
    /// Padding of the tag: the space between its border and its content.
    /// </summary>
    public partial Thickness TagPadding { get; set; }

    /// <summary>
    /// Inline padding of the tag text label; it keeps the text clear of the icon and of the close button.
    /// </summary>
    public partial Thickness TagTextPaddingInline { get; set; }
}
