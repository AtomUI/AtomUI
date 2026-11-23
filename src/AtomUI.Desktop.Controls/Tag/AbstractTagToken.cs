using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Own Tokens shared by <see cref="Tag"/> and <see cref="CheckableTag"/> (antd Tag <c>tagFontSize</c>,
/// <c>tagLineHeight</c>, <c>tagIconSize</c> and the tag paddings). Each control has its own terminal Token, so
/// <c>Controls[Tag]</c> and <c>Controls[CheckableTag]</c> compute and override these values independently.
/// </summary>
[ControlDesignToken]
public abstract partial class AbstractTagToken : AbstractControlDesignToken
{
    /// <summary>
    /// 标签字号
    /// Font size of the tag text
    /// </summary>
    public partial double TagFontSize { get; set; }

    /// <summary>
    /// 标签字重
    /// Font weight of the tag text
    /// </summary>
    public partial FontWeight TagFontWeight { get; set; }

    /// <summary>
    /// 标签行高
    /// Line height of the tag text
    /// </summary>
    public partial double TagLineHeight { get; set; }

    /// <summary>
    /// 标签图标尺寸
    /// Size of the tag icon
    /// </summary>
    public partial double TagIconSize { get; set; }

    /// <summary>
    /// 标签内间距
    /// Padding of the tag
    /// </summary>
    public partial Thickness TagPadding { get; set; }

    /// <summary>
    /// 标签文字与图标之间的间距
    /// Inline gap between the tag text and its icon
    /// </summary>
    public partial Thickness TagTextPaddingInline { get; set; }
}
