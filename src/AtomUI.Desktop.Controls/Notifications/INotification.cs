using Avalonia.Controls;
using Avalonia.Controls.Templates;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Data of one notification to display through an <see cref="INotificationManager" />: text, severity, icon,
/// actions, lifetime and callbacks.
/// </summary>
public interface INotification
{
   /// <summary>
   /// Gets the Title of the notification.
   /// </summary>
   string Title { get; }

   /// <summary>
   /// Gets the notification message.
   /// </summary>
   object? Content { get; }

   /// <summary>
   /// Icon shown in front of the text. When <see langword="null" />, the card shows the status icon of
   /// <see cref="Type" />, if that type has one.
   /// </summary>
   PathIcon? Icon { get; }

   /// <summary>
   /// Content of the action area below the description. <see langword="null" />, the default, hides the area.
   /// </summary>
   object? Actions => null;

   /// <summary>
   /// Data template used to present <see cref="Actions" />. <see langword="null" /> by default.
   /// </summary>
   IDataTemplate? ActionsTemplate => null;

   /// <summary>
   /// Gets the <see cref="NotificationType" /> of the notification.
   /// </summary>
   NotificationType Type { get; }

   /// <summary>
   /// Gets the expiration time of the notification after which it will automatically close.
   /// If the value is <see cref="TimeSpan.Zero" /> then the notification will remain open until the user closes it.
   /// </summary>
   TimeSpan Expiration { get; }

   /// <summary>
   /// Shows a remaining-time bar along the bottom edge of the card. It needs an <see cref="Expiration" /> other
   /// than <see cref="TimeSpan.Zero" />; a notification that stays open shows no bar.
   /// </summary>
   bool ShowProgress { get; }

   /// <summary>
   /// Gets an Action to be run when the notification is clicked.
   /// </summary>
   Action? OnClick { get; }

   /// <summary>
   /// Gets an Action to be run when the notification is closed.
   /// </summary>
   Action? OnClose { get; }
}