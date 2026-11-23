using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
public sealed partial class ExpanderToken : AbstractControlDesignToken
{

    /// <summary>Padding around the title and affordance in a regular expander header.</summary>
    public partial Thickness HeaderPadding { get; set; }

    /// <summary>Padding around the title and affordance in a small expander header.</summary>
    public partial Thickness HeaderPaddingSM { get; set; }

    /// <summary>Padding around the title and affordance in a large expander header.</summary>
    public partial Thickness HeaderPaddingLG { get; set; }

    /// <summary>Fill of the header area containing the title and toggle.</summary>
    public partial Color HeaderBg { get; set; }

    /// <summary>
    /// 折叠面板头部文本颜色
    /// Text color of the header
    /// </summary>
    public partial Color HeaderColor { get; set; }

    /// <summary>
    /// 头部字重
    /// Font weight of the header text
    /// </summary>
    public partial FontWeight HeaderFontWeight { get; set; }

    /// <summary>Padding around the expanded body at regular size.</summary>
    public partial Thickness ContentPadding { get; set; }

    /// <summary>Padding around the expanded body at small size.</summary>
    public partial Thickness ContentPaddingSM { get; set; }

    /// <summary>Padding around the expanded body at large size.</summary>
    public partial Thickness ContentPaddingLG { get; set; }

    /// <summary>Fill of the expanded body behind its content.</summary>
    public partial Color ContentBg { get; set; }

    #region 内部 Token 定义

    /// <summary>Corner radius of the expander frame.</summary>
    public partial CornerRadius ExpanderBorderRadius { get; set; }
    /// <summary>Horizontal spacing around the toggle when it is placed on the left.</summary>
    public partial Thickness LeftExpandButtonHMargin { get; set; }
    /// <summary>Horizontal spacing around the toggle when it is placed on the right.</summary>
    public partial Thickness RightExpandButtonHMargin { get; set; }
    
    #endregion
}
