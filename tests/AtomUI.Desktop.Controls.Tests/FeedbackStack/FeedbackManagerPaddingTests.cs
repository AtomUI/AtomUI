using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless;
using Avalonia.Layout;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.FeedbackStack;

public class FeedbackManagerPaddingTests
{
    static FeedbackManagerPaddingTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Theory]
    [InlineData(typeof(WindowMessageManager), 8)]
    [InlineData(typeof(WindowNotificationManager), 24)]
    public void Root_Padding_Controls_Stack_Geometry_And_Tracks_Position(Type managerType, double defaultInset)
    {
        var manager = CreateManager(managerType);
        using var lifetime = (IDisposable)manager;
        var customPadding = new Thickness(31, 43, 57, 69);
        var rootStyle = new Style(selector => selector.OfType(managerType));
        rootStyle.Setters.Add(new Setter(TemplatedControl.PaddingProperty, customPadding));

        ShowInWindow(manager, window =>
        {
            ShowCards(manager);
            UpdateLayout(window);
            var presenter = manager.GetVisualDescendants().OfType<FeedbackStackPresenter>().Single();
            AssertInsets(manager, presenter, new Thickness(defaultInset),
                HorizontalAlignment.Right, VerticalAlignment.Top);

            // Root styles and later local values must both reach the actual list geometry.
            manager.Styles.Add(rootStyle);
            foreach (var padding in new[] { customPadding, new Thickness(13, 17, 29, 37) })
            {
                if (padding != customPadding)
                {
                    manager.Padding = padding;
                }

                foreach (var (position, horizontal, vertical) in new[]
                         {
                             (NotificationPosition.TopLeft, HorizontalAlignment.Left, VerticalAlignment.Top),
                             (NotificationPosition.TopCenter, HorizontalAlignment.Center, VerticalAlignment.Top),
                             (NotificationPosition.TopRight, HorizontalAlignment.Right, VerticalAlignment.Top),
                             (NotificationPosition.BottomLeft, HorizontalAlignment.Left, VerticalAlignment.Bottom),
                             (NotificationPosition.BottomCenter, HorizontalAlignment.Center, VerticalAlignment.Bottom),
                             (NotificationPosition.BottomRight, HorizontalAlignment.Right, VerticalAlignment.Bottom)
                         })
                {
                    manager.SetValue(manager is WindowMessageManager
                        ? WindowMessageManager.PositionProperty
                        : WindowNotificationManager.PositionProperty, position);
                    UpdateLayout(window);
                    AssertInsets(manager, presenter, padding, horizontal, vertical);
                }
            }
        });
    }

    [Theory]
    [InlineData(typeof(WindowMessageManager))]
    [InlineData(typeof(WindowNotificationManager))]
    public void Custom_Padding_Preserves_Card_Hover_And_Excludes_Empty_Inset(Type managerType)
    {
        var manager = CreateManager(managerType);
        using var lifetime = (IDisposable)manager;
        manager.Padding = new Thickness(60);

        ShowInWindow(manager, window =>
        {
            ShowCards(manager);
            UpdateLayout(window);
            var presenter = manager.GetVisualDescendants().OfType<FeedbackStackPresenter>().Single();
            var latest = manager is WindowMessageManager message
                ? (Control)message.Cards[^1]
                : ((WindowNotificationManager)manager).Cards[^1];

            foreach (var fraction in new[] { 0.1, 0.5, 0.9 })
            {
                presenter.IsCollapsed.ShouldBeTrue();
                var cardPoint = latest.TranslatePoint(
                    new Point(latest.Bounds.Width * fraction, latest.Bounds.Height / 2), window).ShouldNotBeNull();
                window.MouseMove(cardPoint);
                UpdateLayout(window);
                presenter.IsCollapsed.ShouldBeFalse();

                // This point is inside the manager but outside the visible card stack.
                window.MouseMove(manager.TranslatePoint(
                    new Point(manager.Bounds.Width - 5, 5), window).ShouldNotBeNull());
                UpdateLayout(window);
                presenter.IsCollapsed.ShouldBeTrue();
            }
        });
    }

    private static TemplatedControl CreateManager(Type managerType)
    {
        return managerType == typeof(WindowMessageManager)
            ? new WindowMessageManager
            {
                IsMotionEnabled = false, IsStackEnabled = true, Position = NotificationPosition.TopRight
            }
            : new WindowNotificationManager
            {
                IsMotionEnabled = false, IsStackEnabled = true, Position = NotificationPosition.TopRight
            };
    }

    private static void ShowCards(TemplatedControl manager)
    {
        for (var index = 0; index < 4; index++)
        {
            if (manager is WindowMessageManager message)
            {
                message.Show(new Message($"Message {index}", expiration: TimeSpan.Zero));
            }
            else
            {
                ((WindowNotificationManager)manager).Show(
                    new Notification($"Notification {index}", "Body", expiration: TimeSpan.Zero));
            }
        }
    }

    private static void AssertInsets(TemplatedControl manager, Control stack, Thickness padding,
                                     HorizontalAlignment horizontal, VerticalAlignment vertical)
    {
        var origin = stack.TranslatePoint(new Point(0, 0), manager).ShouldNotBeNull();
        var expectedX = horizontal switch
        {
            HorizontalAlignment.Left => padding.Left,
            HorizontalAlignment.Right => manager.Bounds.Width - padding.Right - stack.Bounds.Width,
            _ => padding.Left + (manager.Bounds.Width - padding.Left - padding.Right - stack.Bounds.Width) / 2
        };
        var expectedY = vertical == VerticalAlignment.Top
            ? padding.Top
            : manager.Bounds.Height - padding.Bottom - stack.Bounds.Height;
        origin.X.ShouldBe(expectedX, 0.5);
        origin.Y.ShouldBe(expectedY, 0.5);
        stack.Bounds.Width.ShouldBeGreaterThan(0);
        stack.Bounds.Height.ShouldBeGreaterThan(0);
    }

    private static void UpdateLayout(AvaloniaWindow window)
    {
        Dispatcher.UIThread.RunJobs();
        window.UpdateLayout();
    }

    private static void ShowInWindow(Control content, Action<AvaloniaWindow> assertion)
    {
        var window = new AvaloniaWindow { Width = 800, Height = 600, Content = content };
        try
        {
            window.Show();
            UpdateLayout(window);
            assertion(window);
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }
}
