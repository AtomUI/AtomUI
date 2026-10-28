using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless;
using Avalonia.Threading;
using Avalonia.VisualTree;
using AtomUI.Controls.Primitives;
using System.Runtime.CompilerServices;
using Shouldly;
using Xunit;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.FeedbackStack;

public class FeedbackManagerStackTests
{
    static FeedbackManagerStackTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Managers_Expose_Their_Documented_Stack_Defaults()
    {
        using var message = new WindowMessageManager(null);
        using var notification = new WindowNotificationManager();

        message.Position.ShouldBe(NotificationPosition.TopCenter);
        message.MaxItems.ShouldBe(0);
        message.IsStackEnabled.ShouldBeFalse();
        message.StackThreshold.ShouldBe(3);
        message.IsPauseOnHover.ShouldBeTrue();

        notification.Position.ShouldBe(NotificationPosition.TopRight);
        notification.MaxItems.ShouldBe(0);
        notification.IsStackEnabled.ShouldBeFalse();
        notification.StackThreshold.ShouldBe(3);
        notification.IsPauseOnHover.ShouldBeTrue();
        new Message("message").Expiration.ShouldBe(TimeSpan.FromSeconds(3));
        new Notification("title", "content").Expiration.ShouldBe(TimeSpan.FromSeconds(4.5));
    }

    [Fact]
    public void Manager_Position_Changes_Flow_To_Message_And_Notification_Cards()
    {
        using var message = new WindowMessageManager(null)
        {
            Position = NotificationPosition.BottomLeft
        };
        using var notification = new WindowNotificationManager
        {
            Position = NotificationPosition.BottomLeft
        };
        message.Show(new Message("message", expiration: TimeSpan.Zero));
        notification.Show(new Notification("title", "content", expiration: TimeSpan.Zero));

        message.Cards[0].Position.ShouldBe(NotificationPosition.BottomLeft);
        notification.Cards[0].Position.ShouldBe(NotificationPosition.BottomLeft);

        message.Position = NotificationPosition.TopRight;
        notification.Position = NotificationPosition.TopRight;

        message.Cards[0].Position.ShouldBe(NotificationPosition.TopRight);
        notification.Cards[0].Position.ShouldBe(NotificationPosition.TopRight);
    }

    [Fact]
    public void Permanent_Items_Keep_Managers_At_Zero_Idle_Timers()
    {
        using var message = new WindowMessageManager(null);
        using var notification = new WindowNotificationManager();

        message.Show(new Message("message", expiration: TimeSpan.Zero));
        notification.Show(new Notification("title", "content", expiration: TimeSpan.Zero));

        message.HasLifetimeScheduler.ShouldBeFalse();
        message.LifetimeEntryCount.ShouldBe(0);
        notification.HasLifetimeScheduler.ShouldBeFalse();
        notification.LifetimeEntryCount.ShouldBe(0);
    }

    [Fact]
    public void Enabling_Message_Stack_Does_Not_Make_Finite_Items_Permanent()
    {
        using var manager = new WindowMessageManager(null)
        {
            IsMotionEnabled = false,
            IsStackEnabled  = true
        };

        ShowInWindow(manager, _ =>
        {
            manager.Show(new Message("Timed", expiration: TimeSpan.FromMinutes(1)));
            manager.Show(new Message("Permanent", expiration: TimeSpan.Zero));

            manager.HasLifetimeScheduler.ShouldBeTrue();
            manager.LifetimeEntryCount.ShouldBe(1);
            manager.IsLifetimePaused.ShouldBeFalse();
        });
    }

    [Fact]
    public void Timed_Items_Share_One_Lazy_Scheduler_Per_Manager_And_DestroyAll_Drains_It()
    {
        using var manager = new WindowNotificationManager
        {
            IsMotionEnabled = false
        };

        ShowInWindow(manager, window =>
        {
            for (var i = 0; i < 8; i++)
            {
                manager.Show(new Notification($"Notification {i}", "content", expiration: TimeSpan.FromMinutes(1)));
            }

            manager.HasLifetimeScheduler.ShouldBeTrue();
            manager.LifetimeEntryCount.ShouldBe(8);

            manager.DestroyAll();
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();

            manager.LifetimeEntryCount.ShouldBe(0);
        });
    }

    [Fact]
    public void Detached_Lifecycle_Pause_Cannot_Be_Overridden_By_Disabling_Hover_Pause()
    {
        using var manager = new WindowNotificationManager();
        manager.Show(new Notification("Notification", "content", expiration: TimeSpan.FromMinutes(1)));

        manager.IsLifetimePaused.ShouldBeTrue();

        manager.IsPauseOnHover = false;

        manager.IsLifetimePaused.ShouldBeTrue();

        ShowInWindow(manager, _ => manager.IsLifetimePaused.ShouldBeFalse());
    }

    [Fact]
    public void Message_Stack_Uses_One_Real_Card_And_Two_Static_Backplates()
    {
        using var manager = new WindowMessageManager(null)
        {
            IsMotionEnabled = false,
            IsStackEnabled = true
        };

        ShowInWindow(manager, window =>
        {
            ShowMessages(manager, 4);
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();

            var presenter = manager.GetVisualDescendants().OfType<FeedbackStackPresenter>().Single();
            presenter.IsCollapsed.ShouldBeTrue();
            manager.Cards.Count.ShouldBe(4);
            manager.Cards.Count(card => ((IFeedbackStackItem)card).IsStackVisible).ShouldBe(1);

            var backplates = manager.GetVisualDescendants()
                                    .OfType<Border>()
                                    .Where(border => border.Name is "PART_FirstBackplate" or "PART_SecondBackplate")
                                    .ToArray();
            backplates.Length.ShouldBe(2);
            backplates.ShouldAllBe(border => border.IsVisible && !border.IsHitTestVisible);
        });
    }

    [Fact]
    public void Default_Notification_Queue_Remains_Expanded_Above_The_Stack_Threshold()
    {
        using var manager = new WindowNotificationManager
        {
            IsMotionEnabled = false
        };

        ShowInWindow(manager, window =>
        {
            for (var i = 0; i < 4; i++)
            {
                manager.Show(new Notification($"Notification {i}", "content", expiration: TimeSpan.Zero));
            }
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();

            var presenter = manager.GetVisualDescendants().OfType<FeedbackStackPresenter>().Single();
            presenter.IsCollapsed.ShouldBeFalse();
            manager.Cards.Count(card => ((IFeedbackStackItem)card).IsStackVisible).ShouldBe(4);
        });
    }

    [Fact]
    public void First_Notification_Card_Uses_The_Rendered_Prepare_Frame_Before_Entering()
    {
        using var manager = new WindowNotificationManager
        {
            IsStackEnabled = false,
            IsMotionEnabled = true
        };

        ShowInWindow(manager, window =>
        {
            manager.Show(new Notification("Notification", "content", expiration: TimeSpan.Zero));
            manager.Cards[0].OpenCloseMotionDuration = TimeSpan.FromMinutes(1);
            window.UpdateLayout();
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();

            var actor = manager.Cards[0]
                               .GetVisualDescendants()
                               .OfType<Border>()
                               .Single(control => control.Name == "Frame");
            actor.IsAttachedToVisualTree().ShouldBeTrue();
            TopLevel.GetTopLevel(actor).ShouldBe(window);
            manager.Cards[0].IsMotionEnabled.ShouldBeTrue();
            manager.Cards[0].OpenCloseMotionDuration.ShouldBe(TimeSpan.FromMinutes(1));

            for (var i = 0; i < 3 && !actor.IsAnimating(Visual.OpacityProperty); i++)
            {
                AvaloniaHeadlessPlatform.ForceRenderTimerTick();
                Dispatcher.UIThread.RunJobs();
            }

            actor.IsAnimating(Visual.OpacityProperty).ShouldBeTrue();
            actor.RenderTransform.ShouldNotBeNull();
            actor.Opacity.ShouldBeLessThan(1);
        });
    }

    [Fact]
    public void Notification_Stack_Keeps_Three_Real_Cards_And_Retains_Older_Items()
    {
        using var manager = new WindowNotificationManager
        {
            IsMotionEnabled = false,
            IsStackEnabled = true
        };

        ShowInWindow(manager, window =>
        {
            for (var i = 0; i < 4; i++)
            {
                manager.Show(new Notification($"Notification {i}", "content", expiration: TimeSpan.Zero));
            }
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();

            var presenter = manager.GetVisualDescendants().OfType<FeedbackStackPresenter>().Single();
            presenter.IsCollapsed.ShouldBeTrue();
            manager.Cards.Count.ShouldBe(4);
            manager.Cards.Count(card => ((IFeedbackStackItem)card).IsStackVisible).ShouldBe(3);
            ((IFeedbackStackItem)manager.Cards[0]).IsStackVisible.ShouldBeFalse();
        });
    }

    [Fact]
    public void Notification_Expanded_To_Collapsed_Freezes_Only_Changed_Visible_Content_And_Expansion_Releases_It()
    {
        using var manager = new WindowNotificationManager
        {
            IsMotionEnabled = true,
            IsStackEnabled = true
        };

        ShowInWindow(manager, window =>
        {
            for (var i = 0; i < 4; i++)
            {
                manager.Show(new Notification(
                    $"Notification {i}",
                    "content",
                    expiration: TimeSpan.Zero));
            }
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();

            var presenter = manager.GetVisualDescendants().OfType<FeedbackStackPresenter>().Single();
            presenter.SetPointerOverState(true);
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();

            presenter.SetPointerOverState(false);
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();

            var hosts = manager.Cards
                               .Select(card => card.GetVisualDescendants()
                                                   .OfType<FeedbackStackTransitionSnapshotHost>()
                                                   .Single())
                               .ToArray();
            hosts.Count(host => host.IsSnapshotActive).ShouldBe(2);
            hosts[0].IsSnapshotActive.ShouldBeFalse();
            hosts[1].IsSnapshotActive.ShouldBeTrue();
            hosts[2].IsSnapshotActive.ShouldBeTrue();
            hosts[3].IsSnapshotActive.ShouldBeFalse();

            presenter.SetPointerOverState(true);
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();

            hosts.All(host => !host.IsSnapshotActive && host.SnapshotBitmap is null).ShouldBeTrue();
        });
    }

    [Fact]
    public void Notification_Close_Retemplate_And_Manager_Dispose_Release_Active_Content_Snapshots()
    {
        using var manager = new WindowNotificationManager
        {
            IsMotionEnabled = true,
            IsStackEnabled = true
        };

        ShowInWindow(manager, window =>
        {
            for (var i = 0; i < 4; i++)
            {
                manager.Show(new Notification(
                    $"Notification {i}",
                    "content",
                    expiration: TimeSpan.Zero));
            }
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();

            var presenter = manager.GetVisualDescendants().OfType<FeedbackStackPresenter>().Single();
            presenter.SetPointerOverState(true);
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            presenter.SetPointerOverState(false);
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();

            var second = manager.Cards[^2];
            var third = manager.Cards[^3];
            var secondHost = second.GetVisualDescendants()
                                   .OfType<FeedbackStackTransitionSnapshotHost>()
                                   .Single();
            var thirdHost = third.GetVisualDescendants()
                                 .OfType<FeedbackStackTransitionSnapshotHost>()
                                 .Single();
            secondHost.IsSnapshotActive.ShouldBeTrue();
            thirdHost.IsSnapshotActive.ShouldBeTrue();

            second.Close();
            secondHost.IsSnapshotActive.ShouldBeFalse();
            secondHost.SnapshotBitmap.ShouldBeNull();

            third.Template = null;
            third.ApplyTemplate();
            Dispatcher.UIThread.RunJobs();
            thirdHost.IsSnapshotActive.ShouldBeFalse();
            thirdHost.SnapshotBitmap.ShouldBeNull();

            presenter.SetPointerOverState(true);
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            presenter.SetPointerOverState(false);
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();

            var remainingActiveHosts = manager.Cards
                                              .SelectMany(card => card.GetVisualDescendants()
                                                                      .OfType<FeedbackStackTransitionSnapshotHost>())
                                              .Where(host => host.IsSnapshotActive)
                                              .ToArray();
            manager.Dispose();

            remainingActiveHosts.All(host => !host.IsSnapshotActive && host.SnapshotBitmap is null)
                                .ShouldBeTrue();
        });
    }

    [Fact]
    public void Notification_Stack_Hover_Still_Expands_And_Collapses_After_Window_Resize()
    {
        using var manager = new WindowNotificationManager
        {
            IsMotionEnabled = true,
            IsStackEnabled = true
        };

        ShowInWindow(manager, window =>
        {
            for (var i = 0; i < 4; i++)
            {
                manager.Show(new Notification(
                    $"Notification {i}",
                    "content",
                    expiration: TimeSpan.Zero));
            }
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();

            var presenter = manager.GetVisualDescendants().OfType<FeedbackStackPresenter>().Single();
            var panel = presenter.GetVisualDescendants().OfType<FeedbackStackPanel>().Single();
            presenter.IsCollapsed.ShouldBeTrue();

            window.Width = 940;
            window.Height = 620;
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();

            var latest = manager.Cards[^1];
            var stackPoint = latest.TranslatePoint(
                new Point(latest.Bounds.Width / 2, latest.Bounds.Height / 2),
                window).ShouldNotBeNull();
            window.MouseMove(stackPoint);
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();

            panel.IsStackExpanded.ShouldBeTrue();
            presenter.IsCollapsed.ShouldBeFalse();

            window.MouseMove(new Point(4, window.Bounds.Height - 4));
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();

            panel.IsStackExpanded.ShouldBeFalse();
            presenter.IsCollapsed.ShouldBeTrue();
        });
    }

    [Fact]
    public void Hosted_Notification_Stack_Hit_Region_Tracks_Window_Resize()
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

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            using var manager = new WindowNotificationManager(window)
            {
                IsMotionEnabled = true,
                IsStackEnabled = true
            };
            for (var i = 0; i < 4; i++)
            {
                manager.Show(new Notification(
                    $"Notification {i}",
                    "content",
                    expiration: TimeSpan.Zero));
            }
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();

            var presenter = manager.GetVisualDescendants().OfType<FeedbackStackPresenter>().Single();
            var panel = presenter.GetVisualDescendants().OfType<FeedbackStackPanel>().Single();
            presenter.IsCollapsed.ShouldBeTrue();

            window.Width = 940;
            window.Height = 620;
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();

            var latest = manager.Cards[^1];
            var stackPoint = latest.TranslatePoint(
                new Point(latest.Bounds.Width / 2, latest.Bounds.Height / 2),
                window).ShouldNotBeNull();
            window.MouseMove(stackPoint);
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();

            panel.IsStackExpanded.ShouldBeTrue();
            presenter.IsCollapsed.ShouldBeFalse();

            window.MouseMove(new Point(4, window.Bounds.Height - 4));
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();

            panel.IsStackExpanded.ShouldBeFalse();
            presenter.IsCollapsed.ShouldBeTrue();

        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    [Fact]
    public void Hosted_Notification_Stack_Revalidates_Hover_When_Resize_Moves_It_Away_From_The_Pointer()
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

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            using var manager = new WindowNotificationManager(window)
            {
                IsMotionEnabled = true,
                IsStackEnabled = true
            };
            for (var i = 0; i < 4; i++)
            {
                manager.Show(new Notification(
                    $"Notification {i}",
                    "content",
                    expiration: TimeSpan.Zero));
            }
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();

            var presenter = manager.GetVisualDescendants().OfType<FeedbackStackPresenter>().Single();
            var panel = presenter.GetVisualDescendants().OfType<FeedbackStackPanel>().Single();
            var latest = manager.Cards[^1];
            var originalStackPoint = latest.TranslatePoint(
                new Point(latest.Bounds.Width / 2, latest.Bounds.Height / 2),
                window).ShouldNotBeNull();

            window.MouseMove(originalStackPoint);
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            panel.IsStackExpanded.ShouldBeTrue();

            window.Width = 1040;
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();

            panel.IsStackExpanded.ShouldBeFalse();
            presenter.IsCollapsed.ShouldBeTrue();

            var resizedStackPoint = latest.TranslatePoint(
                new Point(latest.Bounds.Width / 2, latest.Bounds.Height / 2),
                window).ShouldNotBeNull();
            window.MouseMove(resizedStackPoint);
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();

            panel.IsStackExpanded.ShouldBeTrue();
            presenter.IsCollapsed.ShouldBeFalse();

            window.MouseMove(new Point(4, window.Bounds.Height - 4));
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();

            panel.IsStackExpanded.ShouldBeFalse();
            presenter.IsCollapsed.ShouldBeTrue();
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    [Fact]
    public void AtomUI_Window_Notification_Stack_Revalidates_Hover_After_Resize()
    {
        var window = new AtomUI.Desktop.Controls.Window
        {
            Width = 700,
            Height = 500,
            Content = new Border()
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            using var manager = new WindowNotificationManager(window)
            {
                IsMotionEnabled = true,
                IsStackEnabled = true
            };
            for (var i = 0; i < 4; i++)
            {
                manager.Show(new Notification(
                    $"Notification {i}",
                    "content",
                    expiration: TimeSpan.Zero));
            }
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();

            var presenter = manager.GetVisualDescendants().OfType<FeedbackStackPresenter>().Single();
            var panel = presenter.GetVisualDescendants().OfType<FeedbackStackPanel>().Single();
            var latest = manager.Cards[^1];
            var originalStackPoint = latest.TranslatePoint(
                new Point(latest.Bounds.Width / 2, latest.Bounds.Height / 2),
                window).ShouldNotBeNull();

            window.MouseMove(originalStackPoint);
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            panel.IsStackExpanded.ShouldBeTrue();

            window.Width = 1040;
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();

            panel.IsStackExpanded.ShouldBeFalse();
            presenter.IsCollapsed.ShouldBeTrue();

            var resizedStackPoint = latest.TranslatePoint(
                new Point(latest.Bounds.Width / 2, latest.Bounds.Height / 2),
                window).ShouldNotBeNull();
            window.MouseMove(resizedStackPoint);
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();

            panel.IsStackExpanded.ShouldBeTrue();
            presenter.IsCollapsed.ShouldBeFalse();

            window.MouseMove(new Point(4, window.Bounds.Height - 4));
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();

            panel.IsStackExpanded.ShouldBeFalse();
            presenter.IsCollapsed.ShouldBeTrue();
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    [Fact]
    public void DestroyAll_Closes_Each_Card_Once_And_Empties_The_Stable_Collection()
    {
        using var manager = new WindowNotificationManager
        {
            IsMotionEnabled = false
        };
        var closeCount = 0;

        ShowInWindow(manager, window =>
        {
            for (var i = 0; i < 4; i++)
            {
                manager.Show(new Notification(
                    $"Notification {i}",
                    "content",
                    expiration: TimeSpan.Zero,
                    onClose: () => closeCount++));
            }
            window.UpdateLayout();

            manager.DestroyAll();
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            manager.DestroyAll();

            manager.Cards.Count.ShouldBe(0);
            closeCount.ShouldBe(4);
            manager.GetVisualDescendants().OfType<NotificationCard>().ShouldBeEmpty();
        });
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public void Message_MaxItems_Closes_The_Oldest_Batch_When_Motion_Is_Disabled(int maxItems)
    {
        using var manager = new WindowMessageManager(null) { IsMotionEnabled = false };
        var closed = new List<int>();
        for (var i = 1; i <= 4; i++)
        {
            var index = i;
            manager.Show(new Message($"Message {i}", expiration: TimeSpan.Zero, onClose: () => closed.Add(index)));
        }
        manager.MaxItems = maxItems;

        manager.Show(new Message("Message 5", expiration: TimeSpan.Zero, onClose: () => closed.Add(5)));

        closed.ShouldBe(Enumerable.Range(1, 5 - maxItems));
        manager.Cards.Select(card => card.Message).ShouldBe(
            Enumerable.Range(6 - maxItems, maxItems).Select(i => $"Message {i}"));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public void Notification_MaxItems_Closes_The_Oldest_Batch_When_Motion_Is_Disabled(int maxItems)
    {
        using var manager = new WindowNotificationManager { IsMotionEnabled = false };
        var closed = new List<int>();
        for (var i = 1; i <= 4; i++)
        {
            var index = i;
            manager.Show(new Notification($"Notification {i}", "content", expiration: TimeSpan.Zero,
                onClose: () => closed.Add(index)));
        }
        manager.MaxItems = maxItems;

        manager.Show(new Notification("Notification 5", "content", expiration: TimeSpan.Zero,
            onClose: () => closed.Add(5)));

        closed.ShouldBe(Enumerable.Range(1, 5 - maxItems));
        manager.Cards.Select(card => card.Title).ShouldBe(
            Enumerable.Range(6 - maxItems, maxItems).Select(i => $"Notification {i}"));
    }

    [Theory]
    [InlineData("dispose")]
    [InlineData("destroy")]
    [InlineData("show")]
    public void Message_DestroyAll_Allows_Close_Callbacks_To_Change_The_Collection(string operation)
    {
        using var manager = new WindowMessageManager(null) { IsMotionEnabled = false };
        var closed = new List<int>();
        manager.Show(new Message("First", expiration: TimeSpan.Zero, onClose: () => closed.Add(1)));
        manager.Show(new Message("Second", expiration: TimeSpan.Zero, onClose: () =>
        {
            closed.Add(2);
            switch (operation)
            {
                case "dispose": manager.Dispose(); break;
                case "destroy": manager.DestroyAll(); break;
                case "show": manager.Show(new Message("New", expiration: TimeSpan.Zero)); break;
            }
        }));

        manager.DestroyAll();

        closed.ShouldBe(operation == "dispose" ? new[] { 2 } : new[] { 2, 1 });
        manager.Cards.Select(card => card.Message).ShouldBe(
            operation == "show" ? new[] { "New" } : Array.Empty<string>());
    }

    [Theory]
    [InlineData("dispose")]
    [InlineData("destroy")]
    [InlineData("show")]
    public void Notification_DestroyAll_Allows_Close_Callbacks_To_Change_The_Collection(string operation)
    {
        using var manager = new WindowNotificationManager { IsMotionEnabled = false };
        var closed = new List<int>();
        manager.Show(new Notification("First", "content", expiration: TimeSpan.Zero, onClose: () => closed.Add(1)));
        manager.Show(new Notification("Second", "content", expiration: TimeSpan.Zero, onClose: () =>
        {
            closed.Add(2);
            switch (operation)
            {
                case "dispose": manager.Dispose(); break;
                case "destroy": manager.DestroyAll(); break;
                case "show": manager.Show(new Notification("New", "content", expiration: TimeSpan.Zero)); break;
            }
        }));

        manager.DestroyAll();

        closed.ShouldBe(operation == "dispose" ? new[] { 2 } : new[] { 2, 1 });
        manager.Cards.Select(card => card.Title).ShouldBe(
            operation == "show" ? new[] { "New" } : Array.Empty<string>());
    }

    [Fact]
    public void Dispose_Releases_Manager_Card_And_User_Callback_Owner()
    {
        var references = CreateDisposedNotificationGraph();

        ForceFullCollection();

        references.Manager.IsAlive.ShouldBeFalse();
        references.Card.IsAlive.ShouldBeFalse();
        references.CallbackOwner.IsAlive.ShouldBeFalse();
    }

    [Fact]
    public void Dispose_After_Attachment_Releases_Window_Presenter_Card_Scheduler_And_Callback_Graph()
    {
        var references = CreateDisposedAttachedMessageGraph();

        ForceFullCollection();

        references.Manager.IsAlive.ShouldBeFalse();
        references.Presenter.IsAlive.ShouldBeFalse();
        references.Card.IsAlive.ShouldBeFalse();
        references.CallbackOwner.IsAlive.ShouldBeFalse();
    }

    [Fact]
    public void Dispose_After_Attachment_Releases_Notification_Window_Presenter_Card_Actor_And_Callback_Graph()
    {
        var references = CreateDisposedAttachedNotificationGraph();

        ForceFullCollection();

        references.Manager.IsAlive.ShouldBeFalse();
        references.Presenter.IsAlive.ShouldBeFalse();
        references.Card.IsAlive.ShouldBeFalse();
        references.MotionActor.IsAlive.ShouldBeFalse();
        references.CallbackOwner.IsAlive.ShouldBeFalse();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static (WeakReference Manager, WeakReference Card, WeakReference CallbackOwner)
        CreateDisposedNotificationGraph()
    {
        var callbackOwner = new object();
        var manager = new WindowNotificationManager();
        manager.Show(new Notification(
            "Notification",
            "content",
            expiration: TimeSpan.FromSeconds(10),
            onClose: () => GC.KeepAlive(callbackOwner)));
        var managerReference = new WeakReference(manager);
        var cardReference = new WeakReference(manager.Cards[0]);
        var callbackReference = new WeakReference(callbackOwner);
        manager.Dispose();
        return (managerReference, cardReference, callbackReference);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static (WeakReference Manager,
                    WeakReference Presenter,
                    WeakReference Card,
                    WeakReference CallbackOwner) CreateDisposedAttachedMessageGraph()
    {
        var callbackOwner = new object();
        var manager = new WindowMessageManager(null)
        {
            IsMotionEnabled = false,
            IsStackEnabled = true
        };
        var window = new AvaloniaWindow
        {
            Width = 700,
            Height = 500,
            Content = manager
        };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        manager.ApplyTemplate();
        manager.Show(new Message(
            "Message",
            expiration: TimeSpan.FromMinutes(1),
            onClose: () => GC.KeepAlive(callbackOwner)));
        Dispatcher.UIThread.RunJobs();
        window.UpdateLayout();

        var presenter = manager.GetVisualDescendants().OfType<FeedbackStackPresenter>().Single();
        var managerReference = new WeakReference(manager);
        var presenterReference = new WeakReference(presenter);
        var cardReference = new WeakReference(manager.Cards[0]);
        var callbackReference = new WeakReference(callbackOwner);

        manager.Dispose();
        window.Content = null;
        window.Close();
        Dispatcher.UIThread.RunJobs();
        return (managerReference, presenterReference, cardReference, callbackReference);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static (WeakReference Manager,
                    WeakReference Presenter,
                    WeakReference Card,
                    WeakReference MotionActor,
                    WeakReference CallbackOwner) CreateDisposedAttachedNotificationGraph()
    {
        var callbackOwner = new object();
        var manager = new WindowNotificationManager
        {
            IsMotionEnabled = false
        };
        var window = new AvaloniaWindow
        {
            Width = 700,
            Height = 500,
            Content = manager
        };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        manager.ApplyTemplate();
        manager.Show(new Notification(
            "Notification",
            "Content",
            expiration: TimeSpan.FromMinutes(1),
            onClose: () => GC.KeepAlive(callbackOwner)));
        Dispatcher.UIThread.RunJobs();
        window.UpdateLayout();

        var presenter = manager.GetVisualDescendants().OfType<FeedbackStackPresenter>().Single();
        var card = manager.Cards[0];
        var motionActor = card.GetVisualDescendants().OfType<Border>().Single(control => control.Name == "Frame");
        var managerReference = new WeakReference(manager);
        var presenterReference = new WeakReference(presenter);
        var cardReference = new WeakReference(card);
        var motionActorReference = new WeakReference(motionActor);
        var callbackReference = new WeakReference(callbackOwner);

        manager.Dispose();
        window.Content = null;
        window.Close();
        Dispatcher.UIThread.RunJobs();
        return (managerReference,
                presenterReference,
                cardReference,
                motionActorReference,
                callbackReference);
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

    private static void ShowMessages(WindowMessageManager manager, int count)
    {
        for (var i = 0; i < count; i++)
        {
            manager.Show(new Message($"Message {i}", expiration: TimeSpan.Zero));
        }
    }

    private static void ShowInWindow(Control content, Action<AvaloniaWindow> assertion)
    {
        var window = new AvaloniaWindow
        {
            Width = 700,
            Height = 500,
            Content = content
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            content.ApplyTemplate();
            window.UpdateLayout();
            assertion(window);
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }
}
