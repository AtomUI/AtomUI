using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
public abstract partial class AbstractDialogToken : AbstractControlDesignToken
{

    /// <summary>
    /// 顶部背景色
    /// Background color of header
    /// </summary>
    public partial Color HeaderBg { get; set; }

    /// <summary>Space below the dialog header.</summary>
    public partial Thickness HeaderMarginBottom { get; set; }

    /// <summary>
    /// overlay dialog 的 logo 大小
    /// </summary>
    public partial double LogoSize { get; set; }

    /// <summary>
    /// 标题字体大小
    /// Font size of title
    /// </summary>
    public partial double HeaderFontSize { get; set; }

    /// <summary>
    /// 标题字体颜色
    /// Font color of title
    /// </summary>
    public partial Color HeaderColor { get; set; }

    /// <summary>
    /// 内容区域背景色
    /// Background color of content
    /// </summary>
    public partial Color ContentBg { get; set; }

    /// <summary>Padding inside the dialog's title area.</summary>
    public partial Thickness HeaderPadding { get; set; }

    /// <summary>Padding inside the dialog content area.</summary>
    public partial Thickness ContentPadding { get; set; }

    /// <summary>Padding inside the content area of a dialog hosted in a native window.</summary>
    public partial Thickness WindowContentPadding { get; set; }

    /// <summary>
    /// 底部区域背景色
    /// Background color of footer
    /// </summary>
    public partial Color FooterBg { get; set; }

    /// <summary>Padding inside the dialog footer.</summary>
    public partial Thickness FooterPadding { get; set; }

    /// <summary>Space above the dialog footer.</summary>
    public partial Thickness FooterMarginTop { get; set; }

    /// <summary>Size of the dialog's close button.</summary>
    public partial double CloseBtnSize { get; set; }

    /// <summary>Default minimum height of the dialog.</summary>
    public partial double MinHeight { get; set; }

    /// <summary>Default minimum width of the dialog.</summary>
    public partial double MinWidth { get; set; }

    /// <summary>Horizontal spacing between buttons in the dialog footer.</summary>
    public partial double ButtonGroupSpacing { get; set; }

    /// <summary>Margin around the dialog's loading indicator.</summary>
    public partial Thickness LoadingIndicatorMargin { get; set; }
}
