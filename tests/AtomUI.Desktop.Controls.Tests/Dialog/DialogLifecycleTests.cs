using System.Runtime.CompilerServices;
using AtomUI.Controls.Primitives;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;

namespace AtomUI.Desktop.Controls.Tests.Dialog;

[Collection(DialogLifecycleTestCollection.Name)]
public class DialogLifecycleTests
{
    static DialogLifecycleTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Dialog_Default_Placement_Matches_DialogOptions()
    {
        var dialog = new AtomUI.Desktop.Controls.Dialog();
        var options = new DialogOptions();

        dialog.HorizontalStartupLocation.ShouldBe(options.HorizontalStartupLocation);
        dialog.VerticalStartupLocation.ShouldBe(options.VerticalStartupLocation);
        dialog.HorizontalStartupLocation.ShouldBe(DialogHorizontalAnchor.Center);
        dialog.VerticalStartupLocation.ShouldBe(DialogVerticalAnchor.Center);
        dialog.CalculatePlacementOffset(new Size(320, 180), new Size(640, 480))
              .ShouldBe(new Point(160, 150));
    }

    [Fact]
    public void DialogOptions_Defaults_Match_The_Static_Dialog_Defaults()
    {
        var options = new DialogOptions();

        options.DefaultStandardButton.ShouldBe(DialogStandardButton.Ok);
        options.HorizontalStartupLocation.ShouldBe(DialogHorizontalAnchor.Center);
        options.VerticalStartupLocation.ShouldBe(DialogVerticalAnchor.Center);
    }

    [Fact]
    public void OpenAsync_Represents_The_Complete_Modeless_Session()
    {
        RunOnUIThread(OpenAsync_Represents_The_Complete_Modeless_Session_Core);
    }

    [Fact]
    public void OpenAsync_From_A_Background_Thread_Waits_For_The_Complete_Session()
    {
        RunOnUIThread(() =>
        {
            var placementTarget = new Border { Width = 100, Height = 40 };
            var root = new ScopeAwareOverlayLayerPanel
            {
                Children = { placementTarget }
            };
            var window = new AtomUI.Desktop.Controls.Window
            {
                Width = 640,
                Height = 480,
                Content = root
            };
            var dialog = new AtomUI.Desktop.Controls.Dialog
            {
                PlacementTarget = placementTarget,
                IsMotionEnabled = false
            };

            try
            {
                window.Show();
                var sessionTask = Task.Run(() => dialog.OpenAsync(TestContext.Current.CancellationToken));
                PumpUntil(() => dialog.IsOpen &&
                                window.GetVisualDescendants().OfType<OverlayDialogPresenter>().Any());

                sessionTask.IsCompleted.ShouldBeFalse();
                dialog.Done();
                WaitWithDispatcherPump(sessionTask);
            }
            finally
            {
                window.Close();
            }
        });
    }

    [Fact]
    public void Programmatic_Close_From_A_Background_Thread_Runs_On_The_UI_Dispatcher()
    {
        RunOnUIThread(() =>
        {
            var placementTarget = new Border { Width = 100, Height = 40 };
            var root = new ScopeAwareOverlayLayerPanel
            {
                Children = { placementTarget }
            };
            var window = new AtomUI.Desktop.Controls.Window
            {
                Width = 640,
                Height = 480,
                Content = root
            };
            var dialog = new AtomUI.Desktop.Controls.Dialog
            {
                PlacementTarget = placementTarget,
                IsMotionEnabled = false
            };
            var opened = false;
            bool? closingOnUiThread = null;
            dialog.Opened += (_, _) => opened = true;
            dialog.Closing += (_, _) => closingOnUiThread = Dispatcher.UIThread.CheckAccess();

            try
            {
                window.Show();
                var sessionTask = dialog.OpenAsync(TestContext.Current.CancellationToken);
                PumpUntil(() => opened);

                WaitWithDispatcherPump(Task.Run(dialog.Accept));
                WaitWithDispatcherPump(sessionTask);

                closingOnUiThread.ShouldBe(true);
                dialog.Result.ShouldBe(DialogCode.Accepted);
            }
            finally
            {
                window.Close();
            }
        });
    }

    [Fact]
    public void Static_Dialog_From_A_Background_Thread_Uses_The_UI_Dispatcher()
    {
        RunOnUIThread(() =>
        {
            var window = new AtomUI.Desktop.Controls.Window
            {
                Width = 640,
                Height = 480,
                Content = new ScopeAwareOverlayLayerPanel()
            };

            try
            {
                window.Show();
                var content = new TextBlock { Text = "Background" };
                var sessionTask = Task.Run(() => AtomUI.Desktop.Controls.Dialog.ShowDialogAsync(
                    content,
                    options: new DialogOptions
                    {
                        StandardButtons = DialogStandardButton.Ok,
                        IsMotionEnabled = false
                    },
                    topLevel: window,
                    cancellationToken: TestContext.Current.CancellationToken));
                PumpUntil(() => sessionTask.IsCompleted ||
                                window.GetVisualDescendants()
                                      .OfType<DialogButton>()
                                      .Any(button => button.StandardButtonType == DialogStandardButton.Ok));

                sessionTask.IsCompleted.ShouldBeFalse();
                window.GetVisualDescendants()
                      .OfType<DialogButton>()
                      .Single(button => button.StandardButtonType == DialogStandardButton.Ok)
                      .RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(
                          Avalonia.Controls.Button.ClickEvent));

                WaitWithDispatcherPump(sessionTask);
                sessionTask.Result.ShouldBe(DialogCode.Accepted);
            }
            finally
            {
                window.Close();
            }
        });
    }

    [Fact]
    public void Generic_Static_Dialog_From_A_Background_Thread_Constructs_The_View_On_The_UI_Dispatcher()
    {
        RunOnUIThread(() =>
        {
            var window = new AtomUI.Desktop.Controls.Window
            {
                Width = 640,
                Height = 480,
                Content = new ScopeAwareOverlayLayerPanel()
            };

            try
            {
                window.Show();
                var sessionTask = Task.Run(() =>
                    AtomUI.Desktop.Controls.Dialog.ShowDialogAsync<UiThreadDialogContent, object>(
                        null,
                        new DialogOptions
                        {
                            StandardButtons = DialogStandardButton.Ok,
                            IsMotionEnabled = false
                        },
                        window,
                        TestContext.Current.CancellationToken));
                PumpUntil(() => sessionTask.IsCompleted ||
                                window.GetVisualDescendants()
                                      .OfType<DialogButton>()
                                      .Any(button => button.StandardButtonType == DialogStandardButton.Ok));

                sessionTask.IsCompleted.ShouldBeFalse();
                window.GetVisualDescendants()
                      .OfType<DialogButton>()
                      .Single(button => button.StandardButtonType == DialogStandardButton.Ok)
                      .RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(
                          Avalonia.Controls.Button.ClickEvent));

                WaitWithDispatcherPump(sessionTask);
                sessionTask.Result.ShouldBe(DialogCode.Accepted);
            }
            finally
            {
                window.Close();
            }
        });
    }

    private static void OpenAsync_Represents_The_Complete_Modeless_Session_Core()
    {
        var placementTarget = new Border { Width = 100, Height = 40 };
        var root = new ScopeAwareOverlayLayerPanel
        {
            Children = { placementTarget }
        };
        var window = new AtomUI.Desktop.Controls.Window
        {
            Width = 640,
            Height = 480,
            Content = root
        };
        var dialog = new AtomUI.Desktop.Controls.Dialog
        {
            PlacementTarget = placementTarget,
            Content = "Dialog",
            IsModal = false,
            IsMotionEnabled = false,
            StandardButtons = DialogStandardButton.Ok,
            HostWidth = 320,
            HostHeight = 180
        };
        var events = new List<string>();
        dialog.Opened += (_, _) => events.Add("Opened");
        dialog.Closing += (_, _) => events.Add("Closing");
        dialog.Accepted += (_, _) => events.Add("Accepted");
        dialog.Finished += (_, _) => events.Add("Finished");
        dialog.Closed += (_, _) => events.Add("Closed");

        try
        {
            window.Show();
            var sessionTask = dialog.OpenAsync(TestContext.Current.CancellationToken);
            Dispatcher.UIThread.RunJobs();

            dialog.IsOpen.ShouldBeTrue();
            sessionTask.IsCompleted.ShouldBeFalse();
            events.ShouldBe(new[] { "Opened" });

            dialog.Accept();
            WaitWithDispatcherPump(sessionTask);

            dialog.Result.ShouldBe(DialogCode.Accepted);
            dialog.IsOpen.ShouldBeFalse();
            events.ShouldBe(new[] { "Opened", "Closing", "Accepted", "Finished", "Closed" });
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Placement_Target_Detach_Forces_The_Active_Session_To_Tear_Down()
    {
        RunOnUIThread(Placement_Target_Detach_Forces_The_Active_Session_To_Tear_Down_Core);
    }

    private static void Placement_Target_Detach_Forces_The_Active_Session_To_Tear_Down_Core()
    {
        var placementTarget = new Border { Width = 100, Height = 40 };
        var root = new ScopeAwareOverlayLayerPanel
        {
            Children = { placementTarget }
        };
        var window = new AtomUI.Desktop.Controls.Window
        {
            Width = 640,
            Height = 480,
            Content = root
        };
        var beforeCloseCallCount = 0;
        var closingCallCount = 0;
        var dialog = new AtomUI.Desktop.Controls.Dialog
        {
            PlacementTarget = placementTarget,
            Content = "Dialog",
            IsModal = false,
            IsMotionEnabled = false,
            BeforeCloseAsync = _ =>
            {
                beforeCloseCallCount++;
                return ValueTask.FromResult(false);
            }
        };
        dialog.Closing += (_, e) =>
        {
            closingCallCount++;
            e.Cancel = true;
        };

        try
        {
            window.Show();
            var sessionTask = dialog.OpenAsync(TestContext.Current.CancellationToken);
            Dispatcher.UIThread.RunJobs();

            root.Children.Remove(placementTarget);
            WaitWithDispatcherPump(sessionTask);

            closingCallCount.ShouldBe(1);
            beforeCloseCallCount.ShouldBe(0);
            dialog.IsOpen.ShouldBeFalse();
            window.GetVisualDescendants().OfType<OverlayDialogPresenter>().ShouldBeEmpty();
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void IsOpen_False_During_Opening_Cannot_Reopen_A_Stale_Session()
    {
        RunOnUIThread(() =>
        {
            var placementTarget = new Border { Width = 100, Height = 40 };
            var root = new ScopeAwareOverlayLayerPanel
            {
                Children = { placementTarget }
            };
            var window = new AtomUI.Desktop.Controls.Window
            {
                Width = 640,
                Height = 480,
                Content = root
            };
            var dialog = new AtomUI.Desktop.Controls.Dialog
            {
                PlacementTarget = placementTarget,
                IsMotionEnabled = true,
                HostWidth = 320,
                HostHeight = 180
            };
            var openedCount = 0;
            var closedCount = 0;
            dialog.Opened += (_, _) => openedCount++;
            dialog.Closed += (_, _) => closedCount++;

            try
            {
                window.Show();
                dialog.IsOpen = true;
                Dispatcher.UIThread.RunJobs();
                window.GetVisualDescendants().OfType<OverlayDialogPresenter>().ShouldHaveSingleItem();

                dialog.IsOpen = false;
                PumpUntil(() => !window.GetVisualDescendants().OfType<OverlayDialogPresenter>().Any());

                dialog.IsOpen.ShouldBeFalse();
                openedCount.ShouldBe(0);
                closedCount.ShouldBe(1);
                Dispatcher.UIThread.RunJobs();
                window.GetVisualDescendants().OfType<OverlayDialogPresenter>().ShouldBeEmpty();
            }
            finally
            {
                window.Close();
            }
        });
    }

    [Fact]
    public void IsOpen_True_During_Closing_Starts_A_New_Session_After_Teardown()
    {
        RunOnUIThread(() =>
        {
            var placementTarget = new Border { Width = 100, Height = 40 };
            var root = new ScopeAwareOverlayLayerPanel
            {
                Children = { placementTarget }
            };
            var window = new AtomUI.Desktop.Controls.Window
            {
                Width = 640,
                Height = 480,
                Content = root
            };
            var dialog = new AtomUI.Desktop.Controls.Dialog
            {
                PlacementTarget = placementTarget,
                IsMotionEnabled = false,
                HostWidth = 320,
                HostHeight = 180
            };
            var openedCount = 0;
            var closedCount = 0;
            dialog.Opened += (_, _) => openedCount++;
            dialog.Closed += (_, _) => closedCount++;

            try
            {
                window.Show();
                dialog.IsOpen = true;
                PumpUntil(() => openedCount == 1);
                var firstPresenter = window.GetVisualDescendants()
                                           .OfType<OverlayDialogPresenter>()
                                           .ShouldHaveSingleItem();
                firstPresenter.IsMotionEnabled = true;
                firstPresenter.MotionDuration = TimeSpan.FromMilliseconds(80);

                dialog.IsOpen = false;
                Dispatcher.UIThread.RunJobs();
                firstPresenter.Parent.ShouldBeOfType<DialogOverlayLayer>();

                dialog.IsOpen = true;
                PumpUntil(() => openedCount == 2);

                dialog.IsOpen.ShouldBeTrue();
                closedCount.ShouldBe(1);
                window.GetVisualDescendants()
                      .OfType<OverlayDialogPresenter>()
                      .ShouldHaveSingleItem()
                      .ShouldNotBeSameAs(firstPresenter);

                dialog.IsOpen = false;
                PumpUntil(() => !window.GetVisualDescendants().OfType<OverlayDialogPresenter>().Any());
            }
            finally
            {
                window.Close();
            }
        });
    }

    [Fact]
    public void OpenAsync_Rejects_An_Existing_Active_Session()
    {
        RunOnUIThread(() =>
        {
            var placementTarget = new Border { Width = 100, Height = 40 };
            var root = new ScopeAwareOverlayLayerPanel
            {
                Children = { placementTarget }
            };
            var window = new AtomUI.Desktop.Controls.Window
            {
                Width = 640,
                Height = 480,
                Content = root
            };
            var dialog = new AtomUI.Desktop.Controls.Dialog
            {
                PlacementTarget = placementTarget,
                IsMotionEnabled = false
            };
            var openedCount = 0;
            dialog.Opened += (_, _) => openedCount++;

            try
            {
                window.Show();
                dialog.IsOpen = true;
                PumpUntil(() => openedCount == 1);

                Should.Throw<InvalidOperationException>(() =>
                    WaitWithDispatcherPump(dialog.OpenAsync(TestContext.Current.CancellationToken)));

                var firstPresenter = window.GetVisualDescendants()
                                           .OfType<OverlayDialogPresenter>()
                                           .ShouldHaveSingleItem();
                firstPresenter.IsMotionEnabled = true;
                firstPresenter.MotionDuration = TimeSpan.FromMilliseconds(80);

                dialog.IsOpen = false;
                Dispatcher.UIThread.RunJobs();
                Should.Throw<InvalidOperationException>(() =>
                    WaitWithDispatcherPump(dialog.OpenAsync(TestContext.Current.CancellationToken)));
                PumpUntil(() => !window.GetVisualDescendants().OfType<OverlayDialogPresenter>().Any());
                openedCount.ShouldBe(1);
            }
            finally
            {
                window.Close();
            }
        });
    }

    [Fact]
    public void IsOpen_True_Before_Attach_Defers_Opening_Until_A_Placement_Target_Is_Available()
    {
        RunOnUIThread(() =>
        {
            var dialog = new AtomUI.Desktop.Controls.Dialog
            {
                IsOpen = true,
                IsMotionEnabled = false
            };
            Dispatcher.UIThread.RunJobs();

            dialog.IsOpen.ShouldBeTrue();

            var placementTarget = new Border { Width = 80, Height = 32 };
            var root = new ScopeAwareOverlayLayerPanel
            {
                Width = 480,
                Height = 360,
                Children = { placementTarget, dialog }
            };
            var window = new AtomUI.Desktop.Controls.Window
            {
                Width = 480,
                Height = 360,
                Content = root
            };

            try
            {
                window.Show();
                PumpUntil(() => window.GetVisualDescendants().OfType<DialogSurface>().Any());

                dialog.IsOpen.ShouldBeTrue();
                dialog.Done();
                PumpUntil(() => !dialog.IsOpen);
            }
            finally
            {
                window.Close();
            }
        });
    }

    [Fact]
    public void OpenAsync_Without_An_Attached_Placement_Target_Fails_Explicitly()
    {
        RunOnUIThread(() =>
        {
            var dialog = new AtomUI.Desktop.Controls.Dialog
            {
                IsMotionEnabled = false
            };

            var exception = Should.Throw<InvalidOperationException>(() =>
                WaitWithDispatcherPump(dialog.OpenAsync(TestContext.Current.CancellationToken)));

            exception.Message.ShouldContain("placement target");
            dialog.IsOpen.ShouldBeFalse();
        });
    }

    [Fact]
    public void Detaching_The_Dialog_Forces_The_Active_Session_To_Tear_Down()
    {
        RunOnUIThread(() =>
        {
            var placementTarget = new Border { Width = 80, Height = 32 };
            var dialog = new AtomUI.Desktop.Controls.Dialog
            {
                PlacementTarget = placementTarget,
                IsMotionEnabled = false
            };
            var root = new ScopeAwareOverlayLayerPanel
            {
                Children = { placementTarget, dialog }
            };
            var window = new AtomUI.Desktop.Controls.Window
            {
                Width = 480,
                Height = 360,
                Content = root
            };
            var closedCount = 0;
            dialog.Closed += (_, _) => closedCount++;

            try
            {
                window.Show();
                dialog.IsOpen = true;
                PumpUntil(() => window.GetVisualDescendants().OfType<OverlayDialogPresenter>().Any());

                root.Children.Remove(dialog);
                PumpUntil(() => !window.GetVisualDescendants().OfType<OverlayDialogPresenter>().Any());

                dialog.IsOpen.ShouldBeFalse();
                closedCount.ShouldBe(1);
            }
            finally
            {
                window.Close();
            }
        });
    }

    [Fact]
    public void Declarative_Close_Policy_Failure_Is_Reported_Through_The_Dispatcher()
    {
        RunOnUIThread(() =>
        {
            var expectedException = new InvalidOperationException("policy failed");
            var placementTarget = new Border { Width = 80, Height = 32 };
            var dialog = new AtomUI.Desktop.Controls.Dialog
            {
                PlacementTarget = placementTarget,
                IsMotionEnabled = false,
                BeforeCloseAsync = _ => ValueTask.FromException<bool>(expectedException)
            };
            var root = new ScopeAwareOverlayLayerPanel
            {
                Children = { placementTarget, dialog }
            };
            var window = new AtomUI.Desktop.Controls.Window
            {
                Width = 480,
                Height = 360,
                Content = root
            };
            Exception? reportedException = null;
            void HandleUnhandledException(object? sender, DispatcherUnhandledExceptionEventArgs e)
            {
                reportedException = e.Exception;
                e.Handled = true;
            }

            Dispatcher.UIThread.UnhandledException += HandleUnhandledException;
            try
            {
                window.Show();
                dialog.IsOpen = true;
                PumpUntil(() => window.GetVisualDescendants().OfType<OverlayDialogPresenter>().Any());

                dialog.Accept();
                PumpUntil(() => reportedException is not null);

                reportedException.ShouldBeSameAs(expectedException);
                dialog.IsOpen.ShouldBeTrue();
                dialog.BeforeCloseAsync = null;
                dialog.Done();
                PumpUntil(() => !dialog.IsOpen);
            }
            finally
            {
                Dispatcher.UIThread.UnhandledException -= HandleUnhandledException;
                window.Close();
            }
        });
    }

    [Fact]
    public void Latest_Open_Intent_Is_Reconciled_After_A_Faulted_Teardown()
    {
        RunOnUIThread(() =>
        {
            var expectedException = new InvalidOperationException("closed failed");
            var placementTarget = new Border { Width = 80, Height = 32 };
            var dialog = new AtomUI.Desktop.Controls.Dialog
            {
                PlacementTarget = placementTarget,
                IsMotionEnabled = true
            };
            var root = new ScopeAwareOverlayLayerPanel
            {
                Children = { placementTarget, dialog }
            };
            var window = new AtomUI.Desktop.Controls.Window
            {
                Width = 480,
                Height = 360,
                Content = root
            };
            var openedCount = 0;
            var isFirstSession = true;
            Exception? reportedException = null;
            dialog.Opened += (_, _) => openedCount++;
            dialog.Finished += (_, _) =>
            {
                if (isFirstSession)
                {
                    dialog.IsOpen = true;
                }
            };
            dialog.Closed += (_, _) =>
            {
                if (isFirstSession)
                {
                    isFirstSession = false;
                    throw expectedException;
                }
            };
            void HandleUnhandledException(object? sender, DispatcherUnhandledExceptionEventArgs e)
            {
                reportedException = e.Exception;
                e.Handled = true;
            }

            Dispatcher.UIThread.UnhandledException += HandleUnhandledException;
            try
            {
                window.Show();
                dialog.IsOpen = true;
                PumpUntil(() => openedCount == 1);
                var firstPresenter = window.GetVisualDescendants()
                                           .OfType<OverlayDialogPresenter>()
                                           .ShouldHaveSingleItem();
                firstPresenter.MotionDuration = TimeSpan.FromMilliseconds(80);

                dialog.Accept();
                PumpUntil(() => reportedException is not null && openedCount == 2);

                reportedException.ShouldBeSameAs(expectedException);
                dialog.IsOpen.ShouldBeTrue();
                dialog.Done();
                PumpUntil(() => !dialog.IsOpen);
            }
            finally
            {
                Dispatcher.UIThread.UnhandledException -= HandleUnhandledException;
                window.Close();
            }
        });
    }

    [Fact]
    public void Nested_Dialogs_Restore_The_Lower_Surface_Then_The_Original_Focus()
    {
        RunOnUIThread(() =>
        {
            var trigger = new Border
            {
                Width = 100,
                Height = 40,
                Focusable = true
            };
            var first = new AtomUI.Desktop.Controls.Dialog
            {
                PlacementTarget = trigger,
                Content = "First",
                IsMotionEnabled = false,
                HostWidth = 320,
                HostHeight = 180
            };
            var second = new AtomUI.Desktop.Controls.Dialog
            {
                PlacementTarget = trigger,
                Content = "Second",
                IsMotionEnabled = false,
                HostWidth = 320,
                HostHeight = 180
            };
            var firstOpened = false;
            var secondOpened = false;
            first.Opened += (_, _) => firstOpened = true;
            second.Opened += (_, _) => secondOpened = true;
            var root = new ScopeAwareOverlayLayerPanel
            {
                Children = { trigger, first, second }
            };
            var window = new AtomUI.Desktop.Controls.Window
            {
                Width = 640,
                Height = 480,
                Content = root
            };

            try
            {
                window.Show();
                trigger.Focus().ShouldBeTrue();

                var firstTask = first.OpenAsync(TestContext.Current.CancellationToken);
                PumpUntil(() => firstOpened);
                var firstPresenter = window.GetVisualDescendants()
                                           .OfType<OverlayDialogPresenter>()
                                           .Single();
                var firstSurface = firstPresenter.Surface;
                window.FocusManager.GetFocusedElement().ShouldBeSameAs(firstSurface);

                var secondTask = second.OpenAsync(TestContext.Current.CancellationToken);
                PumpUntil(() => secondOpened);
                var secondSurface = window.GetVisualDescendants()
                                          .OfType<OverlayDialogPresenter>()
                                          .Single(presenter => !ReferenceEquals(presenter, firstPresenter))
                                          .Surface;
                window.FocusManager.GetFocusedElement().ShouldBeSameAs(secondSurface);

                second.Done();
                WaitWithDispatcherPump(secondTask);
                window.FocusManager.GetFocusedElement().ShouldBeSameAs(firstSurface);

                first.Done();
                WaitWithDispatcherPump(firstTask);
                window.FocusManager.GetFocusedElement().ShouldBeSameAs(trigger);
            }
            finally
            {
                window.Close();
            }
        });
    }

    [Fact]
    public void Closed_Overlay_Session_Releases_Presenter_Surface_And_Content()
    {
        ClosedSessionReferences? references = null;
        RunOnUIThread(() => references = CreateClosedSessionReferences());

        CollectGarbage();

        references.ShouldNotBeNull();
        references.Session.IsAlive.ShouldBeFalse();
        references.Presenter.IsAlive.ShouldBeFalse();
        references.Surface.IsAlive.ShouldBeFalse();
        references.Content.IsAlive.ShouldBeFalse();
    }

    [Fact]
    public void Closed_Overlay_Session_Releases_Surface_When_A_Custom_Button_Is_Retained()
    {
        RetainedButtonSessionReferences? references = null;
        RunOnUIThread(() => references = CreateRetainedButtonSessionReferences());

        CollectGarbage();

        references.ShouldNotBeNull();
        references.Button.ShouldNotBeNull();
        references.Presenter.IsAlive.ShouldBeFalse();
        references.Surface.IsAlive.ShouldBeFalse();
    }

    [Fact]
    public void Closed_Window_Session_Releases_Presenter_Surface_And_Content()
    {
        ClosedSessionReferences? references = null;
        RunOnUIThread(() => references = CreateClosedWindowSessionReferences());

        CollectGarbage();

        references.ShouldNotBeNull();
        references.Session.IsAlive.ShouldBeFalse();
        references.Presenter.IsAlive.ShouldBeFalse();
        references.Surface.IsAlive.ShouldBeFalse();
        references.Content.IsAlive.ShouldBeFalse();
        references.RetainedButton.ShouldNotBeNull();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static ClosedSessionReferences CreateClosedSessionReferences()
    {
        var placementTarget = new Border { Width = 100, Height = 40 };
        var content = new Border { Width = 180, Height = 70 };
        var root = new ScopeAwareOverlayLayerPanel
        {
            Children = { placementTarget }
        };
        var window = new AtomUI.Desktop.Controls.Window
        {
            Width = 640,
            Height = 480,
            Content = root
        };
        var dialog = new AtomUI.Desktop.Controls.Dialog
        {
            Content = content,
            IsMotionEnabled = false,
            HostWidth = 320,
            HostHeight = 180
        };
        var presenter = new OverlayDialogPresenter(dialog, placementTarget);
        var session = new DialogSession(
            dialog,
            presenter,
            CancellationToken.None,
            placementTarget,
            window);
        var references = new ClosedSessionReferences(
            new WeakReference(session),
            new WeakReference(presenter),
            new WeakReference(presenter.Surface),
            new WeakReference(content),
            null);

        window.Show();
        var sessionTask = session.RunAsync();
        PumpUntil(() => session.State == DialogSessionState.Open);
        WaitWithDispatcherPump(session.RequestCloseAsync(
            new DialogCloseRequest(null, DialogCloseReason.Programmatic, null)).AsTask());
        WaitWithDispatcherPump(sessionTask);
        WaitWithDispatcherPump(session.DisposeAsync().AsTask());
        AvaloniaHeadlessPlatform.ForceRenderTimerTick(1);
        Dispatcher.UIThread.RunJobs();
        window.Close();

        return references;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static RetainedButtonSessionReferences CreateRetainedButtonSessionReferences()
    {
        var placementTarget = new Border { Width = 100, Height = 40 };
        var customButton = new DialogButton
        {
            Content = "Retained",
            Role = DialogButtonRole.CustomRole
        };
        var root = new ScopeAwareOverlayLayerPanel
        {
            Children = { placementTarget }
        };
        var window = new AtomUI.Desktop.Controls.Window
        {
            Width = 640,
            Height = 480,
            Content = root
        };
        var dialog = new AtomUI.Desktop.Controls.Dialog
        {
            IsMotionEnabled = false,
            HostWidth = 320,
            HostHeight = 180
        };
        dialog.CustomButtons.Add(customButton);
        var presenter = new OverlayDialogPresenter(dialog, placementTarget);
        var session = new DialogSession(
            dialog,
            presenter,
            CancellationToken.None,
            placementTarget,
            window);
        var references = new RetainedButtonSessionReferences(
            customButton,
            new WeakReference(presenter),
            new WeakReference(presenter.Surface));

        window.Show();
        var sessionTask = session.RunAsync();
        PumpUntil(() => session.State == DialogSessionState.Open);
        WaitWithDispatcherPump(session.RequestCloseAsync(
            new DialogCloseRequest(null, DialogCloseReason.Programmatic, null)).AsTask());
        WaitWithDispatcherPump(sessionTask);
        AvaloniaHeadlessPlatform.ForceRenderTimerTick(1);
        Dispatcher.UIThread.RunJobs();
        window.Close();

        return references;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static ClosedSessionReferences CreateClosedWindowSessionReferences()
    {
        var placementTarget = new Border { Width = 100, Height = 40 };
        var content = new Border { Width = 180, Height = 70 };
        var root = new ScopeAwareOverlayLayerPanel
        {
            Children = { placementTarget }
        };
        var owner = new AtomUI.Desktop.Controls.Window
        {
            Width = 640,
            Height = 480,
            Content = root
        };
        var dialog = new AtomUI.Desktop.Controls.Dialog
        {
            Content = content,
            IsModal = false,
            IsMotionEnabled = false,
            HostWidth = 320,
            HostHeight = 180
        };
        var retainedButton = new DialogButton
        {
            Content = "Retained",
            Role = DialogButtonRole.CustomRole
        };
        dialog.CustomButtons.Add(retainedButton);
        var presenter = new WindowDialogPresenter(dialog, owner);
        var session = new DialogSession(
            dialog,
            presenter,
            CancellationToken.None,
            placementTarget,
            owner);
        var references = new ClosedSessionReferences(
            new WeakReference(session),
            new WeakReference(presenter),
            new WeakReference(presenter.FocusScope),
            new WeakReference(content),
            retainedButton);

        owner.Show();
        var sessionTask = session.RunAsync();
        PumpUntil(() => session.State == DialogSessionState.Open);
        WaitWithDispatcherPump(session.RequestCloseAsync(
            new DialogCloseRequest(null, DialogCloseReason.Programmatic, null)).AsTask());
        WaitWithDispatcherPump(sessionTask);
        WaitWithDispatcherPump(session.DisposeAsync().AsTask());
        AvaloniaHeadlessPlatform.ForceRenderTimerTick(1);
        Dispatcher.UIThread.RunJobs();
        owner.Close();

        return references;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void CollectGarbage()
    {
        for (var i = 0; i < 3; i++)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }
    }

    private sealed record ClosedSessionReferences(
        WeakReference Session,
        WeakReference Presenter,
        WeakReference Surface,
        WeakReference Content,
        DialogButton? RetainedButton);

    private sealed record RetainedButtonSessionReferences(
        DialogButton Button,
        WeakReference Presenter,
        WeakReference Surface);

    private sealed class UiThreadDialogContent : Border
    {
        public UiThreadDialogContent()
        {
            Dispatcher.UIThread.CheckAccess().ShouldBeTrue();
        }
    }

    private static void WaitWithDispatcherPump(Task task)
    {
        using var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        _ = task.ContinueWith(
            _ => cancellation.Cancel(),
            CancellationToken.None,
            TaskContinuationOptions.ExecuteSynchronously,
            TaskScheduler.Default);
        Dispatcher.UIThread.MainLoop(cancellation.Token);

        task.IsCompleted.ShouldBeTrue();
        task.GetAwaiter().GetResult();
    }

    private static void PumpUntil(Func<bool> condition)
    {
        var timeoutAt = DateTimeOffset.UtcNow + TimeSpan.FromSeconds(5);
        while (!condition() && DateTimeOffset.UtcNow < timeoutAt)
        {
            using var cancellation = new CancellationTokenSource(TimeSpan.FromMilliseconds(20));
            Dispatcher.UIThread.MainLoop(cancellation.Token);
        }

        condition().ShouldBeTrue();
    }

    private static void RunOnUIThread(Action action)
    {
        Dispatcher.UIThread.Invoke(action);
    }
}

[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class DialogLifecycleTestCollection
{
    public const string Name = "Dialog lifecycle tests";
}
