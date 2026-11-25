using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
public sealed partial class AlertToken : AbstractControlDesignToken
{
    /// <summary>
    /// 成功提示的图标颜色
    /// Icon colour of a success alert.
    /// </summary>
    public partial Color SuccessIconColor { get; set; }

    /// <summary>
    /// 信息提示的图标颜色
    /// Icon colour of an info alert.
    /// </summary>
    public partial Color InfoIconColor { get; set; }

    /// <summary>
    /// 警告提示的图标颜色
    /// Icon colour of a warning alert.
    /// </summary>
    public partial Color WarningIconColor { get; set; }

    /// <summary>
    /// 错误提示的图标颜色
    /// Icon colour of an error alert.
    /// </summary>
    public partial Color ErrorIconColor { get; set; }


    /// <summary>Inset of the content in an alert with a title only.</summary>
    public partial Thickness DefaultPadding { get; set; }

    /// <summary>Inset of the content in an alert that includes a description.</summary>
    public partial Thickness WithDescriptionPadding { get; set; }

    /// <summary>Space around the status icon in a title-only alert.</summary>
    public partial Thickness IconDefaultMargin { get; set; }

    /// <summary>Space around the status icon when the alert includes a description.</summary>
    public partial Thickness IconWithDescriptionMargin { get; set; }

    /// <summary>Width and height of the status icon in a title-only alert.</summary>
    public partial double DefaultIconSize { get; set; }

    /// <summary>Width and height of the status icon when an alert has a description.</summary>
    public partial double WithDescriptionIconSize { get; set; }

    /// <summary>
    /// 关闭按钮图标的大小（antd Alert close-icon 的 fontSizeIcon）
    /// </summary>
    public partial double CloseIconSize { get; set; }

    /// <summary>Space around the optional extra element in the alert.</summary>
    public partial Thickness ExtraElementMargin { get; set; }
    
    /// <summary>Space around the description label under the alert title.</summary>
    public partial Thickness DescriptionLabelMargin { get; set; }

    /// <summary>
    /// 成功提示文字颜色
    /// Text colour of a success alert
    /// </summary>
    public partial Color SuccessMessageColor { get; set; }

    /// <summary>
    /// 信息提示文字颜色
    /// Text colour of an info alert
    /// </summary>
    public partial Color InfoMessageColor { get; set; }

    /// <summary>
    /// 警告提示文字颜色
    /// Text colour of a warning alert
    /// </summary>
    public partial Color WarningMessageColor { get; set; }

    /// <summary>
    /// 错误提示文字颜色
    /// Text colour of an error alert
    /// </summary>
    public partial Color ErrorMessageColor { get; set; }

    /// <summary>
    /// 提示文字字重
    /// Font weight of the alert text (message and description)
    /// </summary>
    public partial FontWeight TextFontWeight { get; set; }
    
}
