using System.Collections.Specialized;
using System.Runtime.CompilerServices;
using AtomUI.Controls.Primitives;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.FeedbackStack;

public class WindowFeedbackLayerActivationTests
{
    static WindowFeedbackLayerActivationTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Notification_Managers_Activate_In_Last_Successful_Show_Order_Without_Detach()
    {
        var window = CreateWindow();

        try
        {
            using var first = new WindowNotificationManager(window) { IsMotionEnabled = false };
            using var second = new WindowNotificationManager(window) { IsMotionEnabled = false };
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();

            var layer = WindowFeedbackLayer.GetLayer(window).ShouldBeOfType<WindowFeedbackLayer>();
            var moveCount = 0;
            var firstDetachCount = 0;
            var secondDetachCount = 0;
            layer.Children.CollectionChanged += (_, args) =>
            {
                if (args.Action == NotifyCollectionChangedAction.Move)
                {
                    moveCount++;
                }
            };
            first.DetachedFromVisualTree += (_, _) => firstDetachCount++;
            second.DetachedFromVisualTree += (_, _) => secondDetachCount++;

            first.Show(new Notification("First", "content", expiration: TimeSpan.Zero));

            layer.Children[^1].ShouldBeSameAs(first);
            moveCount.ShouldBe(1);

            first.Show(new Notification("First again", "content", expiration: TimeSpan.Zero));

            layer.Children[^1].ShouldBeSameAs(first);
            moveCount.ShouldBe(1);

            second.Show(new Notification("Second", "content", expiration: TimeSpan.Zero));

            layer.Children[^1].ShouldBeSameAs(second);
            moveCount.ShouldBe(2);

            first.Show(new Notification("First last", "content", expiration: TimeSpan.Zero));

            layer.Children[^1].ShouldBeSameAs(first);
            moveCount.ShouldBe(3);
            firstDetachCount.ShouldBe(0);
            secondDetachCount.ShouldBe(0);
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    [Fact]
    public void Message_And_Notification_Managers_Share_The_Last_Show_Order()
    {
        var window = CreateWindow();

        try
        {
            using var message = new WindowMessageManager(window) { IsMotionEnabled = false };
            using var notification = new WindowNotificationManager(window) { IsMotionEnabled = false };
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();

            var layer = WindowFeedbackLayer.GetLayer(window).ShouldBeOfType<WindowFeedbackLayer>();

            message.Show(new Message("Message", expiration: TimeSpan.Zero));
            layer.Children[^1].ShouldBeSameAs(message);

            notification.Show(new Notification("Notification", "content", expiration: TimeSpan.Zero));
            layer.Children[^1].ShouldBeSameAs(notification);

            message.Show(new Message("Message last", expiration: TimeSpan.Zero));
            layer.Children[^1].ShouldBeSameAs(message);
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    [Fact]
    public void Reentrant_Show_From_MaxItems_Close_Callback_Remains_Topmost()
    {
        var window = CreateWindow();

        try
        {
            using var callbackManager = new WindowNotificationManager(window) { IsMotionEnabled = false };
            using var outerManager = new WindowNotificationManager(window)
            {
                IsMotionEnabled = false,
                MaxItems = 1
            };
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();

            var layer = WindowFeedbackLayer.GetLayer(window).ShouldBeOfType<WindowFeedbackLayer>();
            outerManager.Show(new Notification(
                "Old",
                "content",
                expiration: TimeSpan.Zero,
                onClose: () => callbackManager.Show(new Notification(
                    "Callback",
                    "content",
                    expiration: TimeSpan.Zero))));
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();

            outerManager.Show(new Notification("Replacement", "content", expiration: TimeSpan.Zero));
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();

            callbackManager.Cards.Count.ShouldBe(1);
            layer.Children[^1].ShouldBeSameAs(callbackManager);
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    [Fact]
    public void Disposed_Activated_Managers_Do_Not_Retain_Feedback_Graphs()
    {
        var graph = CreateDisposedActivatedFeedbackGraphs();

        try
        {
            ForceFullCollection();

            graph.References.ShouldAllBe(reference => !reference.Reference.IsAlive);
        }
        finally
        {
            graph.Window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static RetainedFeedbackGraph CreateDisposedActivatedFeedbackGraphs()
    {
        var messageCallbackOwner = new object();
        var notificationCallbackOwner = new object();
        var window = CreateWindow();
        var message = new WindowMessageManager(window) { IsMotionEnabled = false };
        var notification = new WindowNotificationManager(window) { IsMotionEnabled = false };

        message.Show(new Message(
            "Message",
            expiration: TimeSpan.FromMinutes(1),
            onClose: () => GC.KeepAlive(messageCallbackOwner)));
        notification.Show(new Notification(
            "Notification",
            "content",
            expiration: TimeSpan.FromMinutes(1),
            onClose: () => GC.KeepAlive(notificationCallbackOwner)));
        message.Show(new Message("Message last", expiration: TimeSpan.Zero));
        Dispatcher.UIThread.RunJobs();
        window.UpdateLayout();

        var messagePresenter = message.GetVisualDescendants().OfType<FeedbackStackPresenter>().Single();
        var notificationPresenter = notification.GetVisualDescendants().OfType<FeedbackStackPresenter>().Single();
        var notificationCard = notification.Cards[0];
        var notificationActor = notificationCard.GetVisualDescendants().OfType<Border>().Single(control => control.Name == "Frame");
        var references = new[]
        {
            new NamedReference("Message manager", new WeakReference(message)),
            new NamedReference("Notification manager", new WeakReference(notification)),
            new NamedReference("Message presenter", new WeakReference(messagePresenter)),
            new NamedReference("Notification presenter", new WeakReference(notificationPresenter)),
            new NamedReference("Message card", new WeakReference(message.Cards[0])),
            new NamedReference("Notification card", new WeakReference(notificationCard)),
            new NamedReference("Notification actor", new WeakReference(notificationActor)),
            new NamedReference("Message callback owner", new WeakReference(messageCallbackOwner)),
            new NamedReference("Notification callback owner", new WeakReference(notificationCallbackOwner))
        };

        var layer = WindowFeedbackLayer.GetLayer(window).ShouldBeOfType<WindowFeedbackLayer>();
        message.Dispose();
        notification.Dispose();
        layer.Children.ShouldNotContain(message);
        layer.Children.ShouldNotContain(notification);
        AvaloniaHeadlessPlatform.ForceRenderTimerTick();
        Dispatcher.UIThread.RunJobs();
        return new RetainedFeedbackGraph(window, references);
    }

    private static AvaloniaWindow CreateWindow()
    {
        var root = new VisualLayerManager
        {
            EnableAdornerLayer = true,
            Child = new Border()
        };
        var window = new AvaloniaWindow
        {
            Width = 700,
            Height = 500,
            Content = root
        };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        window.UpdateLayout();
        return window;
    }

    private static void ForceFullCollection()
    {
        for (var i = 0; i < 3; i++)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }
    }

    private sealed record NamedReference(string Name, WeakReference Reference)
    {
        public override string ToString() => Name;
    }

    private sealed record RetainedFeedbackGraph(AvaloniaWindow Window, NamedReference[] References);
}
