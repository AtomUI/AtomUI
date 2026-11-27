using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Design tokens of a <see cref="MessageCard" />: the fill and the padding of the card, the size and the margin
/// of the status icon, and the background, the icon color, the text color and the shadow that depend on the
/// message type.
/// </summary>
[ControlDesignToken]
public sealed partial class MessageCardToken : AbstractControlDesignToken
{

    /// <summary>
    /// Default card fill behind the message text and status icon.
    /// </summary>
    public partial Color ContentBg { get; set; }

    /// <summary>
    /// Inset of the icon and text from the card edge.
    /// </summary>
    public partial Thickness ContentPadding { get; set; }

    /// <summary>
    /// Width and height of the leading message icon.
    /// </summary>
    public partial double MessageIconSize { get; set; }

    /// <summary>
    /// Space between the leading message icon and the message content.
    /// </summary>
    public partial Thickness MessageIconMargin { get; set; }
    /// <summary>
    /// Space above the card in the message stack.
    /// </summary>
    public partial Thickness MessageTopMargin { get; set; }

    /// <summary>
    /// Shadow below a card with the information type.
    /// </summary>
    public partial BoxShadows InformationShadows { get; set; }

    /// <summary>
    /// Shadow below a card with the success type.
    /// </summary>
    public partial BoxShadows SuccessShadows { get; set; }

    /// <summary>
    /// Shadow below a card with the warning type.
    /// </summary>
    public partial BoxShadows WarningShadows { get; set; }

    /// <summary>
    /// Shadow below a card with the error type.
    /// </summary>
    public partial BoxShadows ErrorShadows { get; set; }

    /// <summary>
    /// Shadow below a card with the loading type.
    /// </summary>
    public partial BoxShadows LoadingShadows { get; set; }

    /// <summary>
    /// Background of an information message.
    /// </summary>
    public partial Color InformationBg { get; set; }

    /// <summary>
    /// Icon color of an information message.
    /// </summary>
    public partial Color InformationIconColor { get; set; }

    /// <summary>
    /// Icon color of a loading message.
    /// </summary>
    public partial Color LoadingIconColor { get; set; }

    /// <summary>
    /// Text color of an information message.
    /// </summary>
    public partial Color InformationColor { get; set; }

    /// <summary>
    /// Background of a success message.
    /// </summary>
    public partial Color SuccessBg { get; set; }

    /// <summary>
    /// Icon color of a success message.
    /// </summary>
    public partial Color SuccessIconColor { get; set; }

    /// <summary>
    /// Text color of a success message.
    /// </summary>
    public partial Color SuccessColor { get; set; }

    /// <summary>
    /// Background of a warning message.
    /// </summary>
    public partial Color WarningBg { get; set; }

    /// <summary>
    /// Icon color of a warning message.
    /// </summary>
    public partial Color WarningIconColor { get; set; }

    /// <summary>
    /// Text color of a warning message.
    /// </summary>
    public partial Color WarningColor { get; set; }

    /// <summary>
    /// Background of an error message.
    /// </summary>
    public partial Color ErrorBg { get; set; }

    /// <summary>
    /// Icon color of an error message.
    /// </summary>
    public partial Color ErrorIconColor { get; set; }

    /// <summary>
    /// Text color of an error message.
    /// </summary>
    public partial Color ErrorColor { get; set; }
    
}
