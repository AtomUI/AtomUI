using AtomUI.Desktop.Controls.DesignTokens;
using Avalonia;
using Avalonia.Controls;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AvaloniaButton = Avalonia.Controls.Button;

namespace AtomUI.Desktop.Controls.Rendering.Tests.Effect.Families;

/// <summary>
/// MessageCard, NotificationCard, their window managers and PopupConfirm. Card Tokens render on a standalone card; the
/// manager-level Tokens (the stack inset and the message backplates, which the MessageCard Tokens style because they echo
/// the cards) render on a manager placed in the test window with a stack of cards. PopupConfirm rows open its confirm
/// flyout through the overlay popup layer and compare the confirm container.
/// </summary>
public partial class FeedbackFamilyEffectTests
{
    // ---- MessageCard / WindowMessageManager ---------------------------------------------------

    public static IEnumerable<object[]> MessageCardRows() => MapRows("MessageCard");

    public static IEnumerable<object[]> WindowMessageManagerRows() => MapRows("WindowMessageManager");

    /// <summary>MessageCard Tokens drawn by the manager: its stack inset and the backplates behind a collapsed stack.</summary>
    private static readonly HashSet<string> s_messageManagerTokens = new(StringComparer.Ordinal)
    {
        "MessageTopMargin", "BoxShadowsTertiary"
    };

    [Theory]
    [MemberData(nameof(MessageCardRows))]
    public void MessageCard_Token_Has_Effect(string token, bool isOwn)
    {
        _ = isOwn;
        if (s_messageManagerTokens.Contains(token))
        {
            Run(static () => CreateMessageManager(), null, MessageCardTokens.Identity, token,
                new EffectOptions
                {
                    Target = static root => LatestMessage(root),
                    Region = static card => card.GetVisualAncestors().OfType<WindowMessageManager>().First()
                });
            return;
        }

        var type = token switch
        {
            "ColorError" or "ErrorShadows"       => MessageType.Error,
            "ColorSuccess" or "SuccessShadows"   => MessageType.Success,
            "ColorWarning" or "WarningShadows"   => MessageType.Warning,
            "LoadingShadows" or "LoadingIconColor" => MessageType.Loading,
            // The severity surface tokens (SuccessBg, ErrorColor, ...) paint the message of their severity.
            _ when token.StartsWith("Error", StringComparison.Ordinal)   => MessageType.Error,
            _ when token.StartsWith("Success", StringComparison.Ordinal) => MessageType.Success,
            _ when token.StartsWith("Warning", StringComparison.Ordinal) => MessageType.Warning,
            _                                    => MessageType.Information
        };
        Action<Control>? arrange = token is "SelectionBackground" or "SelectionForeground"
            ? static card => card.GetVisualDescendants().OfType<Avalonia.Controls.SelectableTextBlock>().First().SelectAll()
            : null;
        Func<object, object>? perturb = token is "BorderRadiusLG" ? static _ => new CornerRadius(20) : null;
        // The loading icon is a thin ring: compare its own presenter, not the whole card.
        Func<Control, Control>? region = token is "LoadingIconColor"
            ? static card => card.GetVisualDescendants().OfType<AtomUI.Controls.IconPresenter>().First()
            : null;
        Run(() => new MessageCard { Message = "This is a normal message", MessageType = type, IsMotionEnabled = false },
            arrange, MessageCardTokens.Identity, token, new EffectOptions { Perturb = perturb, Region = region });
    }

    [Theory]
    [MemberData(nameof(WindowMessageManagerRows))]
    public void WindowMessageManager_Token_Has_Effect(string token, bool isOwn)
    {
        _ = isOwn;
        Run(static () => CreateMessageManager(), null, WindowMessageManagerTokens.Identity, token,
            new EffectOptions
            {
                Target = static root =>
                {
                    LatestMessage(root);
                    return (Control)root;
                }
            });
    }

    private static WindowMessageManager CreateMessageManager() =>
        new()
        {
            Width          = 520,
            Height         = 240,
            IsStackEnabled = true,
            Position       = NotificationPosition.TopCenter
        };

    private static MessageCard LatestMessage(Control root)
    {
        var manager = (WindowMessageManager)root;
        if (manager.Cards.Count == 0)
        {
            for (var index = 0; index < 4; index++)
            {
                manager.Show(new Message($"Message {index}", expiration: TimeSpan.Zero));
            }
        }
        return WaitForVisual(() => manager.Cards.Count == 4 ? manager.Cards[^1] : null, "The message stack");
    }

    // ---- NotificationCard / WindowNotificationManager -----------------------------------------

    public static IEnumerable<object[]> NotificationCardRows() => MapRows("NotificationCard");

    public static IEnumerable<object[]> WindowNotificationManagerRows() => MapRows("WindowNotificationManager");

    private static readonly HashSet<string> s_notificationManagerTokens = new(StringComparer.Ordinal)
    {
        "NotificationTopMargin", "NotificationBottomMargin", "UniformlyMarginLG"
    };

    [Theory]
    [MemberData(nameof(NotificationCardRows))]
    public void NotificationCard_Token_Has_Effect(string token, bool isOwn)
    {
        _ = isOwn;
        if (s_notificationManagerTokens.Contains(token))
        {
            var position = token == "NotificationBottomMargin" ? NotificationPosition.BottomRight : NotificationPosition.TopRight;
            Run(() => CreateNotificationManager(position), null, NotificationCardTokens.Identity, token,
                new EffectOptions
                {
                    Target = static root => LatestNotification(root),
                    Region = static card => card.GetVisualAncestors().OfType<WindowNotificationManager>().First()
                });
            return;
        }

        var type = token switch
        {
            "ColorError" or "ErrorShadows"       => NotificationType.Error,
            "ColorSuccess" or "SuccessShadows"   => NotificationType.Success,
            "ColorWarning" or "WarningShadows"   => NotificationType.Warning,
            // The resting shadow and the plain border only paint a notification with no severity.
            "RestingShadows" or "NotificationBorderColor" => NotificationType.Default,
            // The severity surface tokens (SuccessBg, ErrorTitleColor, ...) paint the card of their severity.
            _ when token.StartsWith("Error", StringComparison.Ordinal)   => NotificationType.Error,
            _ when token.StartsWith("Success", StringComparison.Ordinal) => NotificationType.Success,
            _ when token.StartsWith("Warning", StringComparison.Ordinal) => NotificationType.Warning,
            _                                    => NotificationType.Information
        };
        // The default frame border is 0: a border colour shows on a card given a start bar.
        var bordered = token.EndsWith("BorderColor", StringComparison.Ordinal);
        var closeIconHover = token.EndsWith("CloseIconHoverColor", StringComparison.Ordinal);
        var progress = token is "NotificationProgressBg" or "NotificationProgressHeight" or "NotificationProgressMargin"
            or "NotificationProgressTrackBg" or "ColorFillQuaternary" or "ColorPrimaryBorderHover" or "BorderRadiusLG";
        var actions = token is "NotificationActionsMargin";
        // The close button is transparent at rest: its size, margin, radius and padding show on its hover background.
        var closeHover = token is "BorderRadiusSM" or "NotificationCloseButtonSize" or "ControlHeightLG" or
            "NotificationCloseButtonMargin";
        Action<Control>? arrange = token switch
        {
            "ColorBgTextHover" or "ColorIconHover" => static card => Pseudo(CloseButtonOf(card), ":pointerover"),
            _ when closeHover => static card => Pseudo(CloseButtonOf(card), ":pointerover"),
            "ColorBgTextActive" => static card => Pseudo(CloseButtonOf(card), ":pressed"),
            _ when closeIconHover => static card => Pseudo(CloseButtonOf(card), ":pointerover"),
            _ when bordered => static card => ((NotificationCard)card).BorderThickness = new Thickness(6, 0, 0, 0),
            // Half of the time left: the track shows next to the remaining-time indicator.
            _ when progress => static card => ((NotificationCard)card).Expiration = TimeSpan.FromSeconds(5),
            _ => null
        };
        Func<Control, Control>? region = token switch
        {
            "ColorBgTextHover" or "ColorIconHover" or "ColorBgTextActive" or "ColorIcon" or "BorderRadiusSM" or
                "IconSizeSM" => static card => CloseButtonOf(card),
            _ when token.Contains("CloseIcon", StringComparison.Ordinal) => static card => CloseButtonOf(card),
            _ => null
        };
        Func<object, object>? perturb = token switch
        {
            "BorderRadiusLG" => static _ => new CornerRadius(24),
            // A gradient has no table perturbation: replace it with a solid colour.
            "NotificationProgressBg" => static _ => new Avalonia.Media.Immutable.ImmutableSolidColorBrush(Avalonia.Media.Colors.Red),
            _ => null
        };
        Run(() => new NotificationCard
            {
                Title           = "Notification Title",
                Content         = "This is the content of the notification.",
                NotificationType = type,
                IsMotionEnabled = false,
                IsShowProgress  = progress,
                Expiration      = progress ? TimeSpan.FromSeconds(10) : null,
                Actions         = actions ? new AvaloniaButton { Content = "Confirm" } : null
            },
            arrange, NotificationCardTokens.Identity, token, new EffectOptions { Region = region, Perturb = perturb });
    }

    [Theory]
    [MemberData(nameof(WindowNotificationManagerRows))]
    public void WindowNotificationManager_Token_Has_Effect(string token, bool isOwn)
    {
        _ = isOwn;
        Run(static () => CreateNotificationManager(NotificationPosition.TopRight), null,
            WindowNotificationManagerTokens.Identity, token,
            new EffectOptions
            {
                Target = static root =>
                {
                    LatestNotification(root);
                    return (Control)root;
                }
            });
    }

    private static WindowNotificationManager CreateNotificationManager(NotificationPosition position) =>
        new()
        {
            Width          = 640,
            Height         = 420,
            IsStackEnabled = true,
            Position       = position
        };

    private static NotificationCard LatestNotification(Control root)
    {
        var manager = (WindowNotificationManager)root;
        if (manager.Cards.Count == 0)
        {
            manager.Show(new Notification("Notification Title", "Body", expiration: TimeSpan.Zero));
        }
        return WaitForVisual(() => manager.Cards.Count == 1 ? manager.Cards[^1] : null, "The notification card");
    }

    private static Control CloseButtonOf(Control card) =>
        card.GetVisualDescendants().OfType<IconButton>().First(static b => b.Name == "PART_CloseButton");

    // ---- PopupConfirm -------------------------------------------------------------------------

    public static IEnumerable<object[]> PopupConfirmRows() => MapRows("PopupConfirm");

    [Theory]
    [MemberData(nameof(PopupConfirmRows))]
    public void PopupConfirm_Token_Has_Effect(string token, bool isOwn)
    {
        _ = isOwn;
        var status = token switch
        {
            "ColorPrimary" => PopupConfirmStatus.Info,
            "ColorError" => PopupConfirmStatus.Error,
            _ => PopupConfirmStatus.Warning
        };
        Run(() =>
            {
                var confirm = new PopupConfirm
                {
                    Title             = "Delete the task",
                    ConfirmContent    = "Are you sure to delete this task?",
                    ConfirmStatus     = status,
                    Content           = new AvaloniaButton { Content = "Delete" },
                    Margin            = new Thickness(120, 200, 0, 0),
                    IsPopupPinnedOpen = true
                };
                return OverlayLayerHost(confirm, 560, 420);
            },
            null, PopupConfirmTokens.Identity, token,
            new EffectOptions
            {
                Target = static root =>
                {
                    WaitForVisual(() => ConfirmContainerOrNull(root), "The confirm flyout");
                    return root.GetVisualDescendants().OfType<PopupConfirm>().First();
                },
                Region = static confirm => ConfirmContainerOrNull(confirm) ??
                                           throw new InvalidOperationException("The confirm flyout is not open.")
            });
    }

    private static Control? ConfirmContainerOrNull(Control anyInWindow) =>
        TopLevel.GetTopLevel(anyInWindow)?.GetVisualDescendants().OfType<PopupConfirmContainer>()
                .FirstOrDefault(static c => c.IsEffectivelyVisible && c.Bounds.Height > 0);

    [Fact]
    public void Card_Open_Close_Motion_Follows_The_Card_Motion_Token()
    {
        // MessageCard and NotificationCard play their open / close motion over their own MotionDurationMid.
        var message      = new MessageCard { Message = "Message" };
        var notification = new NotificationCard { Title = "Notification", Content = "Body" };
        WithProvider(new StackPanel { Children = { message, notification } }, provider =>
        {
            var duration = TimeSpan.FromMilliseconds(333);
            provider.Config = WithControlToken(MessageCardTokens.Identity, "MotionDurationMid", duration.ToString("c"));
            WaitFor(() => message.OpenCloseMotionDuration == duration);
            message.OpenCloseMotionDuration.ShouldBe(duration);
            notification.OpenCloseMotionDuration.ShouldNotBe(duration, "Controls[MessageCard] reached the NotificationCard.");

            provider.Config = WithControlToken(NotificationCardTokens.Identity, "MotionDurationMid", duration.ToString("c"));
            WaitFor(() => notification.OpenCloseMotionDuration == duration);
            notification.OpenCloseMotionDuration.ShouldBe(duration);
        });
    }
}
