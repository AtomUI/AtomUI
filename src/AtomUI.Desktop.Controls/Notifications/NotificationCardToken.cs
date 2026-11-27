using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Design tokens for <see cref="NotificationCard" />. The members without a severity prefix paint the frame of
/// a default notification and the layout shared by every notification; the Information, Success, Warning and
/// Error members paint the frame of a notification of <see cref="NotificationType" /> with that severity.
/// </summary>
[ControlDesignToken]
public sealed partial class NotificationCardToken : AbstractControlDesignToken
{

    /// <summary>
    /// Fill of the default notification card frame behind the title, description and close button.
    /// </summary>
    public partial Color NotificationBg { get; set; }

    /// <summary>
    /// Padding inside the content area around the notification's text and actions.
    /// </summary>
    public partial Thickness NotificationPadding { get; set; }

    /// <summary>
    /// Width and height of the status icon preceding the notification text.
    /// </summary>
    public partial double NotificationIconSize { get; set; }

    /// <summary>
    /// Margin around the status icon preceding the notification text.
    /// </summary>
    public partial Thickness NotificationIconMargin { get; set; }

    /// <summary>
    /// Width and height of the close button at the upper-right corner of the card.
    /// </summary>
    public partial double NotificationCloseButtonSize { get; set; }
    
    /// <summary>
    /// Vertical gap between the notification title and description.
    /// </summary>
    public partial double NotificationSectionSpacing { get; set; }

    /// <summary>
    /// Inset at the right edge of the title, leaving room for the overlaid close button.
    /// </summary>
    public partial Thickness NotificationTitlePadding { get; set; }

    /// <summary>
    /// Margin above the row of notification action buttons.
    /// </summary>
    public partial Thickness NotificationActionsMargin { get; set; }

    /// <summary>
    /// Margin positioning the overlaid close button at the upper-right edge of the card.
    /// </summary>
    public partial Thickness NotificationCloseButtonMargin { get; set; }

    /// <summary>
    /// Fill of the remaining-time segment along the bottom edge of the notification. Drawn over
    /// <see cref="NotificationProgressTrackBg" />.
    /// </summary>
    public partial IImmutableBrush? NotificationProgressBg { get; set; }

    /// <summary>
    /// Fill of the full-width remaining-time track behind <see cref="NotificationProgressBg" />.
    /// </summary>
    public partial Color NotificationProgressTrackBg { get; set; }

    /// <summary>
    /// Height of the remaining-time bar at the bottom edge of the notification.
    /// </summary>
    public partial double NotificationProgressHeight { get; set; }

    /// <summary>
    /// Margin around the remaining-time bar at the bottom edge of the notification.
    /// </summary>
    public partial Thickness NotificationProgressMargin { get; set; }

    /// <summary>
    /// Width of the notification card.
    /// </summary>
    public partial double NotificationWidth { get; set; }


    /// <summary>
    /// Padding around the notification stack when the manager places it at the top of the window.
    /// </summary>
    public partial Thickness NotificationTopMargin { get; set; }

    /// <summary>
    /// Padding around the notification stack when the manager places it at the bottom of the window.
    /// </summary>
    public partial Thickness NotificationBottomMargin { get; set; }

    /// <summary>
    /// Shadows cast by the frame of a default notification.
    /// </summary>
    public partial BoxShadows RestingShadows { get; set; }

    /// <summary>
    /// Shadows cast by the frame of an information notification.
    /// </summary>
    public partial BoxShadows InformationShadows { get; set; }

    /// <summary>
    /// Shadows cast by the frame of a success notification.
    /// </summary>
    public partial BoxShadows SuccessShadows { get; set; }

    /// <summary>
    /// Shadows cast by the frame of a warning notification.
    /// </summary>
    public partial BoxShadows WarningShadows { get; set; }

    /// <summary>
    /// Shadows cast by the frame of an error notification.
    /// </summary>
    public partial BoxShadows ErrorShadows { get; set; }

    /// <summary>
    /// Thickness of each edge of the notification card frame. The leading edge can be thicker to form a status stripe.
    /// </summary>
    public partial Thickness NotificationBorderThickness { get; set; }

    /// <summary>
    /// Color of the frame outline of a default notification, paired with <see cref="NotificationBg" />.
    /// </summary>
    public partial Color NotificationBorderColor { get; set; }

    /// <summary>
    /// Fill of the information card frame behind the status icon, title, description and close button. Pairs with
    /// <see cref="InformationIconColor" />, <see cref="InformationTitleColor" />,
    /// <see cref="InformationDescriptionColor" /> and <see cref="InformationCloseIconColor" />.
    /// </summary>
    public partial Color InformationBg { get; set; }

    /// <summary>
    /// Foreground of the information status icon next to the text. Pairs with <see cref="InformationBg" />.
    /// </summary>
    public partial Color InformationIconColor { get; set; }

    /// <summary>
    /// Color of the information card frame outline. Pairs with <see cref="InformationBg" />.
    /// </summary>
    public partial Color InformationBorderColor { get; set; }

    /// <summary>
    /// Foreground of the information notification title. Pairs with <see cref="InformationBg" />.
    /// </summary>
    public partial Color InformationTitleColor { get; set; }

    /// <summary>
    /// Foreground of the information notification description. Pairs with <see cref="InformationBg" />.
    /// </summary>
    public partial Color InformationDescriptionColor { get; set; }

    /// <summary>
    /// Foreground of the close icon on an information card at rest. Pairs with <see cref="InformationBg" />.
    /// </summary>
    public partial Color InformationCloseIconColor { get; set; }

    /// <summary>
    /// Foreground of the close icon on an information card while the close button is under the pointer or pressed.
    /// </summary>
    public partial Color InformationCloseIconHoverColor { get; set; }

    /// <summary>
    /// Fill of the success card frame behind the status icon, title, description and close button. Pairs with
    /// <see cref="SuccessIconColor" />, <see cref="SuccessTitleColor" />, <see cref="SuccessDescriptionColor" /> and
    /// <see cref="SuccessCloseIconColor" />.
    /// </summary>
    public partial Color SuccessBg { get; set; }

    /// <summary>
    /// Foreground of the success status icon next to the text. Pairs with <see cref="SuccessBg" />.
    /// </summary>
    public partial Color SuccessIconColor { get; set; }

    /// <summary>
    /// Color of the success card frame outline. Pairs with <see cref="SuccessBg" />.
    /// </summary>
    public partial Color SuccessBorderColor { get; set; }

    /// <summary>
    /// Foreground of the success notification title. Pairs with <see cref="SuccessBg" />.
    /// </summary>
    public partial Color SuccessTitleColor { get; set; }

    /// <summary>
    /// Foreground of the success notification description. Pairs with <see cref="SuccessBg" />.
    /// </summary>
    public partial Color SuccessDescriptionColor { get; set; }

    /// <summary>
    /// Foreground of the close icon on a success card at rest. Pairs with <see cref="SuccessBg" />.
    /// </summary>
    public partial Color SuccessCloseIconColor { get; set; }

    /// <summary>
    /// Foreground of the close icon on a success card while the close button is under the pointer or pressed.
    /// </summary>
    public partial Color SuccessCloseIconHoverColor { get; set; }

    /// <summary>
    /// Fill of the warning card frame behind the status icon, title, description and close button. Pairs with
    /// <see cref="WarningIconColor" />, <see cref="WarningTitleColor" />, <see cref="WarningDescriptionColor" /> and
    /// <see cref="WarningCloseIconColor" />.
    /// </summary>
    public partial Color WarningBg { get; set; }

    /// <summary>
    /// Foreground of the warning status icon next to the text. Pairs with <see cref="WarningBg" />.
    /// </summary>
    public partial Color WarningIconColor { get; set; }

    /// <summary>
    /// Color of the warning card frame outline. Pairs with <see cref="WarningBg" />.
    /// </summary>
    public partial Color WarningBorderColor { get; set; }

    /// <summary>
    /// Foreground of the warning notification title. Pairs with <see cref="WarningBg" />.
    /// </summary>
    public partial Color WarningTitleColor { get; set; }

    /// <summary>
    /// Foreground of the warning notification description. Pairs with <see cref="WarningBg" />.
    /// </summary>
    public partial Color WarningDescriptionColor { get; set; }

    /// <summary>
    /// Foreground of the close icon on a warning card at rest. Pairs with <see cref="WarningBg" />.
    /// </summary>
    public partial Color WarningCloseIconColor { get; set; }

    /// <summary>
    /// Foreground of the close icon on a warning card while the close button is under the pointer or pressed.
    /// </summary>
    public partial Color WarningCloseIconHoverColor { get; set; }

    /// <summary>
    /// Fill of the error card frame behind the status icon, title, description and close button. Pairs with
    /// <see cref="ErrorIconColor" />, <see cref="ErrorTitleColor" />, <see cref="ErrorDescriptionColor" /> and
    /// <see cref="ErrorCloseIconColor" />.
    /// </summary>
    public partial Color ErrorBg { get; set; }

    /// <summary>
    /// Foreground of the error status icon next to the text. Pairs with <see cref="ErrorBg" />.
    /// </summary>
    public partial Color ErrorIconColor { get; set; }

    /// <summary>
    /// Color of the error card frame outline. Pairs with <see cref="ErrorBg" />.
    /// </summary>
    public partial Color ErrorBorderColor { get; set; }

    /// <summary>
    /// Foreground of the error notification title. Pairs with <see cref="ErrorBg" />.
    /// </summary>
    public partial Color ErrorTitleColor { get; set; }

    /// <summary>
    /// Foreground of the error notification description. Pairs with <see cref="ErrorBg" />.
    /// </summary>
    public partial Color ErrorDescriptionColor { get; set; }

    /// <summary>
    /// Foreground of the close icon on an error card at rest. Pairs with <see cref="ErrorBg" />.
    /// </summary>
    public partial Color ErrorCloseIconColor { get; set; }

    /// <summary>
    /// Foreground of the close icon on an error card while the close button is under the pointer or pressed.
    /// </summary>
    public partial Color ErrorCloseIconHoverColor { get; set; }
    
}
