using AtomUI.Controls.Primitives;
using AtomUI.Icons.AntDesign;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;

namespace AtomUI.Desktop.Controls.Tests.MessageBox;

public class MessageBoxReentrancyTests
{
    static MessageBoxReentrancyTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Semantic_Defaults_Do_Not_Override_Local_Dialog_Configuration()
    {
        var messageBox = new AtomUI.Desktop.Controls.MessageBox
        {
            StandardButtons = DialogStandardButton.Yes | DialogStandardButton.No,
            DefaultStandardButton = DialogStandardButton.Yes,
            EscapeStandardButton = DialogStandardButton.No,
            HorizontalStartupLocation = DialogHorizontalAnchor.Left,
            VerticalStartupLocation = DialogVerticalAnchor.Bottom
        };

        messageBox.Style = MessageBoxStyle.Confirm;
        messageBox.OkButtonStyle = MessageBoxOkButtonStyle.Default;
        messageBox.IsCenterOnStartup = false;

        messageBox.StandardButtons.ShouldBe(DialogStandardButton.Yes | DialogStandardButton.No);
        messageBox.DefaultStandardButton.ShouldBe(DialogStandardButton.Yes);
        messageBox.EscapeStandardButton.ShouldBe(DialogStandardButton.No);
        messageBox.HorizontalStartupLocation.ShouldBe(DialogHorizontalAnchor.Left);
        messageBox.VerticalStartupLocation.ShouldBe(DialogVerticalAnchor.Bottom);
    }

    [Fact]
    public void Static_Async_MessageBox_Returns_The_Result_After_Complete_Teardown()
    {
        RunOnUIThread(() =>
        {
            var window = CreateWindow(new Border());

            try
            {
                var resultTask = AtomUI.Desktop.Controls.MessageBox.ShowMessageBoxModalAsync(
                    new AtomUI.Desktop.Controls.TextBlock { Text = "Confirm?" },
                    options: new MessageBoxOptions
                    {
                        Title           = "Confirm",
                        Style           = MessageBoxStyle.Confirm,
                        IsMotionEnabled = false
                    },
                    topLevel: window);

                PumpUntil(() => FindStandardButton(window, DialogStandardButton.Ok) is not null);
                FindStandardButton(window, DialogStandardButton.Ok)!
                    .RaiseEvent(new RoutedEventArgs(Avalonia.Controls.Button.ClickEvent));

                WaitWithDispatcherPump(resultTask).ShouldBe(DialogCode.Accepted);
                window.GetVisualDescendants().OfType<OverlayDialogPresenter>().ShouldBeEmpty();
                window.GetVisualDescendants().OfType<AtomUI.Desktop.Controls.MessageBox>().ShouldBeEmpty();
            }
            finally
            {
                window.Close();
            }
        });
    }

    [Fact]
    public void Static_MessageBox_From_A_Background_Thread_Uses_The_UI_Dispatcher()
    {
        RunOnUIThread(() =>
        {
            var window = CreateWindow(new Border());

            try
            {
                var content = new AtomUI.Desktop.Controls.TextBlock { Text = "Background" };
                var resultTask = Task.Run(() => AtomUI.Desktop.Controls.MessageBox.ShowMessageBoxAsync(
                    content,
                    options: new MessageBoxOptions { IsMotionEnabled = false },
                    topLevel: window,
                    cancellationToken: TestContext.Current.CancellationToken));
                PumpUntil(() => resultTask.IsCompleted ||
                                FindStandardButton(window, DialogStandardButton.Ok) is not null);

                resultTask.IsCompleted.ShouldBeFalse();
                FindStandardButton(window, DialogStandardButton.Ok)!
                    .RaiseEvent(new RoutedEventArgs(Avalonia.Controls.Button.ClickEvent));

                WaitWithDispatcherPump(resultTask).ShouldBe(DialogCode.Accepted);
            }
            finally
            {
                window.Close();
            }
        });
    }

    [Fact]
    public void Generic_Static_MessageBox_From_A_Background_Thread_Constructs_The_View_On_The_UI_Dispatcher()
    {
        RunOnUIThread(() =>
        {
            var window = CreateWindow(new Border());

            try
            {
                var resultTask = Task.Run(() =>
                    AtomUI.Desktop.Controls.MessageBox.ShowMessageBoxAsync<UiThreadMessageBoxContent, object>(
                        null,
                        new MessageBoxOptions { IsMotionEnabled = false },
                        window,
                        TestContext.Current.CancellationToken));
                PumpUntil(() => resultTask.IsCompleted ||
                                FindStandardButton(window, DialogStandardButton.Ok) is not null);

                resultTask.IsCompleted.ShouldBeFalse();
                FindStandardButton(window, DialogStandardButton.Ok)!
                    .RaiseEvent(new RoutedEventArgs(Avalonia.Controls.Button.ClickEvent));

                WaitWithDispatcherPump(resultTask).ShouldBe(DialogCode.Accepted);
            }
            finally
            {
                window.Close();
            }
        });
    }

    [Fact]
    public void Declarative_MessageBox_Uses_One_Dialog_Surface_And_Forwards_Semantic_Events()
    {
        RunOnUIThread(() =>
        {
            var placementTarget = new Border { Width = 80, Height = 32 };
            var messageBox = new AtomUI.Desktop.Controls.MessageBox
            {
                PlacementTarget = placementTarget,
                Content         = "Delete this item?",
                Style           = MessageBoxStyle.Confirm,
                IsMotionEnabled = false
            };
            var root = new ScopeAwareOverlayLayerPanel
            {
                Children = { placementTarget, messageBox }
            };
            var window = new AtomUI.Desktop.Controls.Window
            {
                Width   = 480,
                Height  = 360,
                Content = root
            };
            var cancelledCount = 0;
            var confirmedCount = 0;
            messageBox.Cancelled += (_, _) => cancelledCount++;
            messageBox.Confirmed += (_, _) => confirmedCount++;

            try
            {
                window.Show();
                messageBox.IsOpen = true;
                PumpUntil(() => messageBox.IsOpen &&
                                window.GetVisualDescendants().OfType<DialogSurface>().Any());

                var surface = window.GetVisualDescendants()
                                    .OfType<DialogSurface>()
                                    .ShouldHaveSingleItem();
                var content = surface.Content.ShouldBeOfType<MessageBoxContent>();
                content.Content.ShouldBe("Delete this item?");
                content.Style.ShouldBe(MessageBoxStyle.Confirm);
                content.StyleIcon.ShouldBeOfType<ExclamationCircleFilled>();
                window.GetVisualDescendants().OfType<DialogSurface>().Count().ShouldBe(1);

                FindStandardButton(window, DialogStandardButton.Cancel)!
                    .RaiseEvent(new RoutedEventArgs(Avalonia.Controls.Button.ClickEvent));
                PumpUntil(() => !messageBox.IsOpen);

                cancelledCount.ShouldBe(1);
                confirmedCount.ShouldBe(0);
                messageBox.Result.ShouldBe(DialogCode.Rejected);
            }
            finally
            {
                window.Close();
            }
        });
    }

    [Fact]
    public void MessageBox_Can_Reopen_After_A_Completed_Session()
    {
        RunOnUIThread(() =>
        {
            var placementTarget = new Border { Width = 80, Height = 32 };
            var messageBox = new AtomUI.Desktop.Controls.MessageBox
            {
                PlacementTarget = placementTarget,
                Content         = "Again?",
                Style           = MessageBoxStyle.Information,
                IsMotionEnabled = false
            };
            var root = new ScopeAwareOverlayLayerPanel
            {
                Children = { placementTarget, messageBox }
            };
            var window = new AtomUI.Desktop.Controls.Window
            {
                Width   = 480,
                Height  = 360,
                Content = root
            };
            var openedCount = 0;
            var closedCount = 0;
            messageBox.Opened += (_, _) => openedCount++;
            messageBox.Closed += (_, _) => closedCount++;

            try
            {
                window.Show();

                for (var index = 0; index < 2; index++)
                {
                    messageBox.IsOpen = true;
                    PumpUntil(() => openedCount == index + 1);
                    FindStandardButton(window, DialogStandardButton.Ok)!
                        .RaiseEvent(new RoutedEventArgs(Avalonia.Controls.Button.ClickEvent));
                    PumpUntil(() => closedCount == index + 1);
                }

                openedCount.ShouldBe(2);
                closedCount.ShouldBe(2);
                messageBox.IsOpen.ShouldBeFalse();
                window.GetVisualDescendants().OfType<OverlayDialogPresenter>().ShouldBeEmpty();
            }
            finally
            {
                window.Close();
            }
        });
    }

    [Fact]
    public void MessageBox_Applies_And_Updates_Button_Text_And_Style()
    {
        RunOnUIThread(() =>
        {
            var placementTarget = new Border { Width = 80, Height = 32 };
            var messageBox = new AtomUI.Desktop.Controls.MessageBox
            {
                PlacementTarget = placementTarget,
                Style           = MessageBoxStyle.Confirm,
                OkButtonText    = "Proceed",
                CancelButtonText = "Keep",
                OkButtonStyle   = MessageBoxOkButtonStyle.Default,
                IsMotionEnabled = false
            };
            var root = new ScopeAwareOverlayLayerPanel
            {
                Children = { placementTarget, messageBox }
            };
            var window = new AtomUI.Desktop.Controls.Window
            {
                Width   = 480,
                Height  = 360,
                Content = root
            };

            try
            {
                window.Show();
                messageBox.IsOpen = true;
                PumpUntil(() => FindStandardButton(window, DialogStandardButton.Cancel) is not null);

                var okButton     = FindStandardButton(window, DialogStandardButton.Ok).ShouldNotBeNull();
                var cancelButton = FindStandardButton(window, DialogStandardButton.Cancel).ShouldNotBeNull();
                okButton.Content.ShouldBe("Proceed");
                okButton.ButtonType.ShouldBe(ButtonType.Default);
                cancelButton.Content.ShouldBe("Keep");

                messageBox.OkButtonText  = "Delete";
                messageBox.OkButtonStyle = MessageBoxOkButtonStyle.Primary;

                okButton.Content.ShouldBe("Delete");
                okButton.ButtonType.ShouldBe(ButtonType.Primary);

                var buttonBox = window.GetVisualDescendants().OfType<DialogButtonBox>().Single();
                buttonBox.OkButtonText = "Localized OK";
                buttonBox.CancelButtonText = "Localized Cancel";

                okButton.Content.ShouldBe("Delete");
                cancelButton.Content.ShouldBe("Keep");

                messageBox.OkButtonText = null;
                messageBox.CancelButtonText = null;

                okButton.Content.ShouldBe("Localized OK");
                cancelButton.Content.ShouldBe("Localized Cancel");

                messageBox.ButtonsConfigure = buttons =>
                    buttons.Single(button => button.StandardButtonType == DialogStandardButton.Ok)
                           .Content = "Configured";
                messageBox.OkButtonStyle = MessageBoxOkButtonStyle.Default;

                okButton.Content.ShouldBe("Configured");

                messageBox.Confirm();
                PumpUntil(() => !messageBox.IsOpen);
            }
            finally
            {
                window.Close();
            }
        });
    }

    [Fact]
    public void Static_MessageBox_Preserves_The_Theme_Default_Minimum_Width()
    {
        RunOnUIThread(() =>
        {
            var window = CreateWindow(new Border());

            try
            {
                var resultTask = AtomUI.Desktop.Controls.MessageBox.ShowMessageBoxAsync(
                    new AtomUI.Desktop.Controls.TextBlock { Text = "Information" },
                    options: new MessageBoxOptions { IsMotionEnabled = false },
                    topLevel: window);

                PumpUntil(() => window.GetVisualDescendants().OfType<DialogSurface>().Any());
                var surface = window.GetVisualDescendants().OfType<DialogSurface>().Single();
                // Keep the original narrow owner, then verify the same surface restores its themed minimum.
                foreach (var ownerWidth in new[] { 480d, 700d, 480d })
                {
                    window.Width = ownerWidth;
                    Dispatcher.UIThread.RunJobs();
                    window.UpdateLayout();
                    var availableWidth = window.ClientSize.Width -
                                         window.FrameShadowThickness.Left - window.FrameShadowThickness.Right -
                                         window.VisibleFrameBorderThickness.Left - window.VisibleFrameBorderThickness.Right;
                    surface.MinWidth.ShouldBe(Math.Min(410, availableWidth));
                    surface.Bounds.Width.ShouldBeGreaterThanOrEqualTo(surface.MinWidth);
                    surface.Bounds.Width.ShouldBeLessThanOrEqualTo(availableWidth);
                }

                FindStandardButton(window, DialogStandardButton.Ok)!
                    .RaiseEvent(new RoutedEventArgs(Avalonia.Controls.Button.ClickEvent));
                WaitWithDispatcherPump(resultTask);
            }
            finally
            {
                window.Close();
            }
        });
    }

    [Fact]
    public void Surface_Teardown_Releases_MessageBox_Button_References()
    {
        RunOnUIThread(() =>
        {
            var messageBox = new AtomUI.Desktop.Controls.MessageBox
            {
                Style           = MessageBoxStyle.Confirm,
                IsMotionEnabled = false
            };
            var surface = new DialogSurface(messageBox);
            var window = new AtomUI.Desktop.Controls.Window { Content = surface };

            try
            {
                window.Show();
                Dispatcher.UIThread.RunJobs();
                surface.ApplyTemplate();
                var okButton = window.GetVisualDescendants()
                                     .OfType<DialogButton>()
                                     .Single(button => button.StandardButtonType == DialogStandardButton.Ok);
                var contentBeforeTeardown = okButton.Content;

                surface.Dispose();
                messageBox.OkButtonText = "Stale";

                okButton.Content.ShouldBe(contentBeforeTeardown);
            }
            finally
            {
                window.Close();
            }
        });
    }

    private static AtomUI.Desktop.Controls.DialogButton? FindStandardButton(
        Visual root,
        DialogStandardButton standardButton)
    {
        return root.GetVisualDescendants()
                   .OfType<AtomUI.Desktop.Controls.DialogButton>()
                   .FirstOrDefault(button => button.StandardButtonType == standardButton);
    }

    private static AtomUI.Desktop.Controls.Window CreateWindow(Control content)
    {
        var root = new ScopeAwareOverlayLayerPanel
        {
            Width  = 480,
            Height = 360,
            Children = { content }
        };
        var window = new AtomUI.Desktop.Controls.Window
        {
            Width   = 480,
            Height  = 360,
            Content = root
        };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        return window;
    }

    private sealed class UiThreadMessageBoxContent : Border
    {
        public UiThreadMessageBoxContent()
        {
            Dispatcher.UIThread.CheckAccess().ShouldBeTrue();
        }
    }

    private static T WaitWithDispatcherPump<T>(Task<T> task)
    {
        PumpUntil(() => task.IsCompleted);
        return task.GetAwaiter().GetResult();
    }

    private static void PumpUntil(Func<bool> condition)
    {
        var timeoutAt = DateTimeOffset.UtcNow + TimeSpan.FromSeconds(5);
        while (!condition() && DateTimeOffset.UtcNow < timeoutAt)
        {
            Dispatcher.UIThread.RunJobs();
            Thread.Sleep(1);
        }

        condition().ShouldBeTrue();
    }

    private static void RunOnUIThread(Action action)
    {
        Dispatcher.UIThread.Invoke(action);
    }
}
