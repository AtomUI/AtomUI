using AtomUI.Controls.Primitives;
using AtomUI.Desktop.Controls.Tests.Window;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.LogicalTree;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;

namespace AtomUI.Desktop.Controls.Tests.Dialog;

public class DialogContentPopupLayeringTests
{
    static DialogContentPopupLayeringTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Dialog_Overlay_Layer_Is_Released_With_The_Last_Presenter()
    {
        RunOnUIThread(() =>
        {
            var (window, _, presenter) = CreateDialogFixture(new AtomUI.Desktop.Controls.ComboBox());
            try
            {
                window.Show();
                Dispatcher.UIThread.RunJobs();
                WaitWithDispatcherPump(presenter.ShowAsync(CancellationToken.None).AsTask());
                Dispatcher.UIThread.RunJobs();

                var dialogLayer = presenter.Parent.ShouldBeOfType<DialogOverlayLayer>();
                var hostLayer = dialogLayer.GetVisualParent().ShouldBeAssignableTo<Panel>()!;
                dialogLayer.Bounds.Size.ShouldBe(window.ClientSize);

                WaitWithDispatcherPump(presenter.CloseAsync().AsTask());
                WaitWithDispatcherPump(presenter.DisposeAsync().AsTask());

                presenter.Parent.ShouldBeNull();
                dialogLayer.Parent.ShouldBeNull();
                hostLayer.Children.ShouldNotContain(dialogLayer);
            }
            finally
            {
                window.Close();
            }
        });
    }

    [Fact]
    public void ComboBox_Dropdown_In_Dialog_Content_Is_Hosted_Above_The_Dialog_And_Can_Select()
    {
        RunOnUIThread(() =>
        {
            var comboBox = new AtomUI.Desktop.Controls.ComboBox
            {
                Width = 160,
                IsMotionEnabled = false
            };
            comboBox.Items.Add(new AtomUI.Desktop.Controls.ComboBoxItem { Content = "1" });
            comboBox.Items.Add(new AtomUI.Desktop.Controls.ComboBoxItem { Content = "2" });
            var (window, _, presenter) = CreateDialogFixture(comboBox);
            try
            {
                window.Show();
                Dispatcher.UIThread.RunJobs();
                WaitWithDispatcherPump(presenter.ShowAsync(CancellationToken.None).AsTask());
                Dispatcher.UIThread.RunJobs();

                comboBox.SetValue(AtomUI.Desktop.Controls.ComboBox.IsDropDownOpenProperty, true);
                Dispatcher.UIThread.RunJobs();
                AvaloniaHeadlessPlatform.ForceRenderTimerTick(1);
                Dispatcher.UIThread.RunJobs();

                comboBox.IsDropDownOpen.ShouldBeTrue();

                var root = ((Visual)window).GetSelfAndVisualAncestors().Last();
                var popupHost = root.GetVisualDescendants()
                                    .OfType<OverlayPopupHost>()
                                    .LastOrDefault(host => host.GetLogicalAncestors()
                                                               .OfType<AtomUI.Desktop.Controls.ComboBox>()
                                                               .Contains(comboBox))
                                    .ShouldNotBeNull();

                var dialogLayer = presenter.Parent.ShouldBeOfType<DialogOverlayLayer>();
                dialogLayer.GetVisualParent().ShouldBeOfType<OverlayLayer>();
                popupHost.GetVisualParent().ShouldNotBeSameAs(dialogLayer.GetVisualParent());

                var secondItem = popupHost.GetVisualDescendants()
                                          .OfType<AtomUI.Desktop.Controls.ComboBoxItem>()
                                          .Single(item => item.Content?.ToString() == "2");
                ClickControl(window, secondItem);
                Dispatcher.UIThread.RunJobs();

                comboBox.SelectedIndex.ShouldBe(1);
                comboBox.IsDropDownOpen.ShouldBeFalse();
            }
            finally
            {
                WaitWithDispatcherPump(presenter.CloseAsync().AsTask());
                WaitWithDispatcherPump(presenter.DisposeAsync().AsTask());
                window.Close();
            }
        });
    }

    [Fact]
    public void ComboBox_Dropdown_In_Dialog_Light_Dismisses_Without_Closing_The_Dialog()
    {
        RunOnUIThread(() =>
        {
            var comboBox = new AtomUI.Desktop.Controls.ComboBox
            {
                Width = 160,
                IsMotionEnabled = false
            };
            comboBox.Items.Add(new AtomUI.Desktop.Controls.ComboBoxItem { Content = "1" });
            comboBox.Items.Add(new AtomUI.Desktop.Controls.ComboBoxItem { Content = "2" });
            var (window, _, presenter) = CreateDialogFixture(comboBox);
            try
            {
                window.Show();
                Dispatcher.UIThread.RunJobs();
                WaitWithDispatcherPump(presenter.ShowAsync(CancellationToken.None).AsTask());

                comboBox.IsDropDownOpen = true;
                Dispatcher.UIThread.RunJobs();
                AvaloniaHeadlessPlatform.ForceRenderTimerTick(1);
                Dispatcher.UIThread.RunJobs();

                LightDismiss(window);

                comboBox.IsDropDownOpen.ShouldBeFalse();
                presenter.Parent.ShouldBeOfType<DialogOverlayLayer>();
            }
            finally
            {
                WaitWithDispatcherPump(presenter.CloseAsync().AsTask());
                WaitWithDispatcherPump(presenter.DisposeAsync().AsTask());
                window.Close();
            }
        });
    }

    [Fact]
    public void ComboBox_Dropdown_In_Dialog_Remains_Operational_When_Drawn_Decorations_Are_Attached()
    {
        RunOnUIThread(() =>
        {
            var comboBox = new AtomUI.Desktop.Controls.ComboBox
            {
                Width = 160,
                IsMotionEnabled = false
            };
            comboBox.Items.Add(new AtomUI.Desktop.Controls.ComboBoxItem { Content = "1" });
            comboBox.Items.Add(new AtomUI.Desktop.Controls.ComboBoxItem { Content = "2" });
            var (window, _, presenter) = CreateDialogFixture(comboBox);
            var dialogHost = new Panel { Name = "PART_DialogOverlayLayerHost" };
            Action? restoreDecorations = null;
            try
            {
                window.Show();
                Dispatcher.UIThread.RunJobs();
                restoreDecorations = DrawnDecorationsTestHost.InstallOnAttachedDecorations(window, dialogHost);

                TopLevel.GetTopLevel(dialogHost).ShouldBeNull();

                WaitWithDispatcherPump(presenter.ShowAsync(CancellationToken.None).AsTask());
                var dialogLayer = presenter.Parent.ShouldBeOfType<DialogOverlayLayer>();
                TopLevel.GetTopLevel(comboBox).ShouldBeSameAs(window);

                comboBox.IsDropDownOpen = true;
                Dispatcher.UIThread.RunJobs();
                AvaloniaHeadlessPlatform.ForceRenderTimerTick(1);
                Dispatcher.UIThread.RunJobs();

                var popupHost = window.GetVisualDescendants()
                                      .OfType<OverlayPopupHost>()
                                      .Last(host => host.GetLogicalAncestors()
                                                        .OfType<AtomUI.Desktop.Controls.ComboBox>()
                                                        .Contains(comboBox));
                popupHost.ShouldNotBeNull();
                dialogLayer.IsAttachedToVisualTree().ShouldBeTrue();
            }
            finally
            {
                WaitWithDispatcherPump(presenter.CloseAsync().AsTask());
                WaitWithDispatcherPump(presenter.DisposeAsync().AsTask());
                restoreDecorations?.Invoke();
                window.Close();
            }
        });
    }

    [Fact]
    public void Dialog_Overlay_Layer_Is_Shared_Until_All_Presenters_Close()
    {
        RunOnUIThread(() =>
        {
            var (window, placementTarget, firstPresenter) = CreateDialogFixture(new TextBlock { Text = "First" });
            var secondPresenter = CreateDialogPresenter(new TextBlock { Text = "Second" }, placementTarget);
            try
            {
                window.Show();
                Dispatcher.UIThread.RunJobs();
                WaitWithDispatcherPump(firstPresenter.ShowAsync(CancellationToken.None).AsTask());
                WaitWithDispatcherPump(secondPresenter.ShowAsync(CancellationToken.None).AsTask());

                var dialogLayer = firstPresenter.Parent.ShouldBeOfType<DialogOverlayLayer>();
                secondPresenter.Parent.ShouldBeSameAs(dialogLayer);
                var hostLayer = dialogLayer.GetVisualParent().ShouldBeAssignableTo<Panel>()!;

                WaitWithDispatcherPump(firstPresenter.CloseAsync().AsTask());
                WaitWithDispatcherPump(firstPresenter.DisposeAsync().AsTask());

                dialogLayer.Parent.ShouldBeSameAs(hostLayer);
                secondPresenter.Parent.ShouldBeSameAs(dialogLayer);

                WaitWithDispatcherPump(secondPresenter.CloseAsync().AsTask());
                WaitWithDispatcherPump(secondPresenter.DisposeAsync().AsTask());

                dialogLayer.Parent.ShouldBeNull();
                hostLayer.Children.ShouldNotContain(dialogLayer);
            }
            finally
            {
                WaitWithDispatcherPump(firstPresenter.DisposeAsync().AsTask());
                WaitWithDispatcherPump(secondPresenter.DisposeAsync().AsTask());
                window.Close();
            }
        });
    }

    [Fact]
    public void Modal_Dialogs_Keep_Drawn_Chrome_Suppressed_Until_The_Last_One_Closes()
    {
        RunOnUIThread(() =>
        {
            var (window, placementTarget, firstPresenter) = CreateDialogFixture(new TextBlock { Text = "First" });
            var secondPresenter = CreateDialogPresenter(new TextBlock { Text = "Second" }, placementTarget);
            try
            {
                window.Show();
                Dispatcher.UIThread.RunJobs();
                window.IsDrawnChromeOverlayVisible.ShouldBeTrue();

                WaitWithDispatcherPump(firstPresenter.ShowAsync(CancellationToken.None).AsTask());
                window.IsDrawnChromeOverlayVisible.ShouldBeFalse();

                WaitWithDispatcherPump(secondPresenter.ShowAsync(CancellationToken.None).AsTask());
                WaitWithDispatcherPump(firstPresenter.CloseAsync().AsTask());
                WaitWithDispatcherPump(firstPresenter.DisposeAsync().AsTask());
                window.IsDrawnChromeOverlayVisible.ShouldBeFalse();

                WaitWithDispatcherPump(secondPresenter.CloseAsync().AsTask());
                WaitWithDispatcherPump(secondPresenter.DisposeAsync().AsTask());
                window.IsDrawnChromeOverlayVisible.ShouldBeTrue();
            }
            finally
            {
                WaitWithDispatcherPump(firstPresenter.DisposeAsync().AsTask());
                WaitWithDispatcherPump(secondPresenter.DisposeAsync().AsTask());
                window.Close();
            }
        });
    }

    private static (AtomUI.Desktop.Controls.Window Window, Border PlacementTarget, OverlayDialogPresenter Presenter)
        CreateDialogFixture(Control content)
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
        var presenter = CreateDialogPresenter(content, placementTarget);
        return (window, placementTarget, presenter);
    }

    private static OverlayDialogPresenter CreateDialogPresenter(Control content, Control placementTarget)
    {
        var dialog = new AtomUI.Desktop.Controls.Dialog
        {
            Content = content,
            IsModal = true,
            IsMaskClosable = false,
            IsMotionEnabled = false,
            HostWidth = 320,
            HostHeight = 220
        };
        return new OverlayDialogPresenter(dialog, placementTarget);
    }

    private static void ClickControl(Avalonia.Controls.Window window, Control control)
    {
        control.IsAttachedToVisualTree().ShouldBeTrue();
        control.Bounds.Width.ShouldBeGreaterThan(0);
        control.Bounds.Height.ShouldBeGreaterThan(0);
        var clickPoint = control.TranslatePoint(
            new Point(control.Bounds.Width / 2, control.Bounds.Height / 2),
            window).ShouldNotBeNull();

        RaisePointerClick(window, control, clickPoint);
    }

    private static void LightDismiss(Avalonia.Controls.Window window)
    {
        var dismissLayer = window.GetVisualDescendants()
                                 .Single(visual => visual.GetType().Name == "LightDismissOverlayLayer" && visual.IsVisible)
                                 .ShouldBeAssignableTo<InputElement>();
        dismissLayer.IsHitTestVisible.ShouldBeTrue();
        RaisePointerClick(window, dismissLayer, new Point(8, 8));
    }

    private static void RaisePointerClick(
        Avalonia.Controls.Window window,
        InputElement target,
        Point position)
    {
        var pointer = new Pointer(Pointer.GetNextFreeId(), PointerType.Mouse, true);
        target.RaiseEvent(new PointerPressedEventArgs(
            target,
            pointer,
            window,
            position,
            0,
            new PointerPointProperties(RawInputModifiers.LeftMouseButton, PointerUpdateKind.LeftButtonPressed),
            KeyModifiers.None));
        var releaseTarget = pointer.Captured as InputElement ?? target;
        releaseTarget.RaiseEvent(new PointerReleasedEventArgs(
            releaseTarget,
            pointer,
            window,
            position,
            1,
            new PointerPointProperties(RawInputModifiers.None, PointerUpdateKind.LeftButtonReleased),
            KeyModifiers.None,
            MouseButton.Left));
    }

    private static void WaitWithDispatcherPump(Task task)
    {
        var timeoutAt = DateTimeOffset.UtcNow + TimeSpan.FromSeconds(5);
        while (!task.IsCompleted && DateTimeOffset.UtcNow < timeoutAt)
        {
            Dispatcher.UIThread.RunJobs();
            Thread.Sleep(1);
        }

        task.IsCompleted.ShouldBeTrue();
        task.GetAwaiter().GetResult();
    }

    private static void RunOnUIThread(Action action)
    {
        Dispatcher.UIThread.Invoke(action);
    }
}
