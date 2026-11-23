using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
public sealed partial class MessageCardToken : AbstractControlDesignToken
{

    /// <summary>Default card fill behind the message text and status icon.</summary>
    public partial Color ContentBg { get; set; }

    /// <summary>Inset of the icon and text from the card edge.</summary>
    public partial Thickness ContentPadding { get; set; }

    /// <summary>Width and height of the leading message icon.</summary>
    public partial double MessageIconSize { get; set; }

    /// <summary>Space between the leading message icon and the message content.</summary>
    public partial Thickness MessageIconMargin { get; set; }
    /// <summary>Space above the card in the message stack.</summary>
    public partial Thickness MessageTopMargin { get; set; }

    /// <summary>Shadow below a card with the information type.</summary>
    public partial BoxShadows InformationShadows { get; set; }

    /// <summary>Shadow below a card with the success type.</summary>
    public partial BoxShadows SuccessShadows { get; set; }

    /// <summary>Shadow below a card with the warning type.</summary>
    public partial BoxShadows WarningShadows { get; set; }

    /// <summary>Shadow below a card with the error type.</summary>
    public partial BoxShadows ErrorShadows { get; set; }

    /// <summary>Shadow below a card with the loading type.</summary>
    public partial BoxShadows LoadingShadows { get; set; }

    /// <summary>
    /// Background of an information message.
    /// </summary>
    public partial Color InformationBg { get; set; }

    /// <summary>
    /// Icon colour of an information message.
    /// </summary>
    public partial Color InformationIconColor { get; set; }

    /// <summary>
    /// 加载中消息的图标颜色
    /// Icon colour of a loading message.
    /// </summary>
    public partial Color LoadingIconColor { get; set; }

    /// <summary>
    /// Text colour of an information message.
    /// </summary>
    public partial Color InformationColor { get; set; }

    /// <summary>
    /// Background of an success message.
    /// </summary>
    public partial Color SuccessBg { get; set; }

    /// <summary>
    /// Icon colour of an success message.
    /// </summary>
    public partial Color SuccessIconColor { get; set; }

    /// <summary>
    /// Text colour of an success message.
    /// </summary>
    public partial Color SuccessColor { get; set; }

    /// <summary>
    /// Background of an warning message.
    /// </summary>
    public partial Color WarningBg { get; set; }

    /// <summary>
    /// Icon colour of an warning message.
    /// </summary>
    public partial Color WarningIconColor { get; set; }

    /// <summary>
    /// Text colour of an warning message.
    /// </summary>
    public partial Color WarningColor { get; set; }

    /// <summary>
    /// Background of an error message.
    /// </summary>
    public partial Color ErrorBg { get; set; }

    /// <summary>
    /// Icon colour of an error message.
    /// </summary>
    public partial Color ErrorIconColor { get; set; }

    /// <summary>
    /// Text colour of an error message.
    /// </summary>
    public partial Color ErrorColor { get; set; }
    
}
