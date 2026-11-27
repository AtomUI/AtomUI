using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Design tokens of the panel of a <see cref="Collapse" />: the background, text colors and font weight of its
/// header, the corner radius of its frame, the paddings of its header and content for each size, and the margins
/// of its expand button.
/// </summary>
[ControlDesignToken]
public sealed partial class CollapseToken : AbstractControlDesignToken
{

    /// <summary>
    /// Padding inside a panel header at the standard size.
    /// </summary>
    public partial Thickness HeaderPadding { get; set; }

    /// <summary>
    /// Background color of a collapsible panel header.
    /// </summary>
    public partial Color HeaderBg { get; set; }

    /// <summary>
    /// Text color of a collapsed panel's header.
    /// </summary>
    public partial Color HeaderColor { get; set; }

    /// <summary>
    /// Text color of an expanded panel's header.
    /// </summary>
    public partial Color HeaderActiveColor { get; set; }

    /// <summary>
    /// Font weight of the panel header text.
    /// </summary>
    public partial FontWeight HeaderFontWeight { get; set; }

    /// <summary>
    /// Padding inside the expanded panel content at the standard size.
    /// </summary>
    public partial Thickness ContentPadding { get; set; }

    /// <summary>
    /// Background color of the expanded panel content.
    /// </summary>
    public partial Color ContentBg { get; set; }

    #region 内部 Token 定义

    /// <summary>
    /// Padding inside a small panel header.
    /// </summary>
    public partial Thickness CollapseHeaderPaddingSM { get; set; }
    /// <summary>
    /// Padding inside a large panel header.
    /// </summary>
    public partial Thickness CollapseHeaderPaddingLG { get; set; }
    /// <summary>
    /// Padding inside the expanded content of a small panel.
    /// </summary>
    public partial Thickness CollapseContentPaddingSM { get; set; }
    /// <summary>
    /// Padding inside the expanded content of a large panel.
    /// </summary>
    public partial Thickness CollapseContentPaddingLG { get; set; }
    /// <summary>
    /// Corner radius of the collapsible panel frame.
    /// </summary>
    public partial CornerRadius CollapsePanelBorderRadius { get; set; }
    /// <summary>
    /// Margin around a left-aligned expand button in a small panel header.
    /// </summary>
    public partial Thickness LeftExpandButtonMarginSM { get; set; }
    /// <summary>
    /// Margin around a left-aligned expand button in a standard panel header.
    /// </summary>
    public partial Thickness LeftExpandButtonMargin { get; set; }
    /// <summary>
    /// Margin around a left-aligned expand button in a large panel header.
    /// </summary>
    public partial Thickness LeftExpandButtonMarginLG { get; set; }
    /// <summary>
    /// Margin around a right-aligned expand button in a small panel header.
    /// </summary>
    public partial Thickness RightExpandButtonMarginSM { get; set; }
    /// <summary>
    /// Margin around a right-aligned expand button in a standard panel header.
    /// </summary>
    public partial Thickness RightExpandButtonMargin { get; set; }
    /// <summary>
    /// Margin around a right-aligned expand button in a large panel header.
    /// </summary>
    public partial Thickness RightExpandButtonMarginLG { get; set; }
    
    #endregion
}
