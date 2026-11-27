using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Design tokens of the <see cref="Alert" />: status colors, icon and close icon sizes, and spacing.
/// Padding, the status icon size and its margin have separate values for alerts with and without a description.
/// </summary>
[ControlDesignToken]
public sealed partial class AlertToken : AbstractControlDesignToken
{
    /// <summary>
    /// Fill color of the status icon in a success alert.
    /// </summary>
    public partial Color SuccessIconColor { get; set; }

    /// <summary>
    /// Fill color of the status icon in an info alert.
    /// </summary>
    public partial Color InfoIconColor { get; set; }

    /// <summary>
    /// Fill color of the status icon in a warning alert.
    /// </summary>
    public partial Color WarningIconColor { get; set; }

    /// <summary>
    /// Fill color of the status icon in an error alert.
    /// </summary>
    public partial Color ErrorIconColor { get; set; }


    /// <summary>
    /// Inset of the content in an alert with a title only.
    /// </summary>
    public partial Thickness DefaultPadding { get; set; }

    /// <summary>
    /// Inset of the content in an alert that includes a description.
    /// </summary>
    public partial Thickness WithDescriptionPadding { get; set; }

    /// <summary>
    /// Space around the status icon in a title-only alert.
    /// </summary>
    public partial Thickness IconDefaultMargin { get; set; }

    /// <summary>
    /// Space around the status icon when the alert includes a description.
    /// </summary>
    public partial Thickness IconWithDescriptionMargin { get; set; }

    /// <summary>
    /// Width and height of the status icon in a title-only alert.
    /// </summary>
    public partial double DefaultIconSize { get; set; }

    /// <summary>
    /// Width and height of the status icon when an alert has a description.
    /// </summary>
    public partial double WithDescriptionIconSize { get; set; }

    /// <summary>
    /// Width and height of the close button icon.
    /// </summary>
    public partial double CloseIconSize { get; set; }

    /// <summary>
    /// Outer margin of the close button.
    /// </summary>
    public partial Thickness ExtraElementMargin { get; set; }
    
    /// <summary>
    /// Space around the description label under the alert title.
    /// </summary>
    public partial Thickness DescriptionLabelMargin { get; set; }

    /// <summary>
    /// Text color of the message and the description in a success alert.
    /// </summary>
    public partial Color SuccessMessageColor { get; set; }

    /// <summary>
    /// Text color of the message and the description in an info alert.
    /// </summary>
    public partial Color InfoMessageColor { get; set; }

    /// <summary>
    /// Text color of the message and the description in a warning alert.
    /// </summary>
    public partial Color WarningMessageColor { get; set; }

    /// <summary>
    /// Text color of the message and the description in an error alert.
    /// </summary>
    public partial Color ErrorMessageColor { get; set; }

    /// <summary>
    /// Font weight set on the alert. The message, the description and the action content inherit it.
    /// </summary>
    public partial FontWeight TextFontWeight { get; set; }
    
}
