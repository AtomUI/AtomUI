using Shouldly;
using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Theme.Resources;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Input;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Xunit;

namespace AtomUI.Desktop.Controls.Tests.Dialog;

public class DialogSurfaceTests
{
    static DialogSurfaceTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Content_Binding_Preserves_Data_Objects_And_Clears_On_Dispose()
    {
        var initialContent = new object();
        var dialog = new AtomUI.Desktop.Controls.Dialog
        {
            Content = initialContent
        };
        var surface = new DialogSurface(dialog);

        surface.Content.ShouldBeSameAs(initialContent);

        dialog.Content = "updated content";
        surface.Content.ShouldBe("updated content");

        surface.Dispose();
        dialog.Content = "after dispose";
        surface.Content.ShouldBeNull();
    }

    [Fact]
    public void Visual_State_And_Effective_Footer_Follow_The_Dialog()
    {
        var dialog = new AtomUI.Desktop.Controls.Dialog
        {
            Title             = "Initial",
            IsFooterVisible   = true,
            StandardButtons   = DialogStandardButton.NoButton,
            IsLoading         = false,
            IsConfirmLoading  = false,
            IsClosable        = true,
            IsMaximizable     = false,
            IsMotionEnabled   = true
        };
        var surface = new DialogSurface(dialog);

        surface.Title.ShouldBe("Initial");
        surface.IsEffectiveFooterVisible.ShouldBeFalse();

        dialog.CustomButtons.Add(new DialogButton { Content = "Custom" });
        surface.IsEffectiveFooterVisible.ShouldBeTrue();

        dialog.IsLoading = true;
        surface.IsEffectiveFooterVisible.ShouldBeFalse();

        dialog.CustomButtons.Clear();
        dialog.IsLoading       = false;
        dialog.StandardButtons = DialogStandardButton.Ok;
        dialog.Title           = "Updated";
        dialog.IsConfirmLoading = true;

        surface.Title.ShouldBe("Updated");
        surface.IsConfirmLoading.ShouldBeTrue();
        surface.StandardButtons.ShouldBe(DialogStandardButton.Ok);
        surface.IsEffectiveFooterVisible.ShouldBeTrue();

        surface.Dispose();
    }

    [Fact]
    public void Custom_Buttons_Are_Synchronized_Through_The_Surface_Button_Box()
    {
        var first       = new DialogButton { Content = "First" };
        var second      = new DialogButton { Content = "Second" };
        var replacement = new DialogButton { Content = "Replacement" };
        var dialog      = new AtomUI.Desktop.Controls.Dialog();
        dialog.CustomButtons.Add(first);
        dialog.CustomButtons.Add(second);
        var surface = new DialogSurface(dialog);
        var window = new AtomUI.Desktop.Controls.Window
        {
            Content = surface
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            surface.ApplyTemplate();

            var buttonBox = surface.ButtonBox.ShouldNotBeNull();
            buttonBox.CustomButtons.ShouldBe(new[] { first, second });

            dialog.CustomButtons[0] = replacement;
            dialog.CustomButtons.Move(1, 0);
            buttonBox.CustomButtons.ShouldBe(new[] { second, replacement });

            dialog.CustomButtons.Clear();
            buttonBox.CustomButtons.ShouldBeEmpty();

            surface.Dispose();
            surface.ButtonBox.ShouldBeNull();
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Assigning_ButtonsConfigure_While_Open_Configures_The_Current_Sequence()
    {
        var dialog = new AtomUI.Desktop.Controls.Dialog
        {
            StandardButtons = DialogStandardButton.Ok | DialogStandardButton.Cancel
        };
        var surface = new DialogSurface(dialog);
        var window = new AtomUI.Desktop.Controls.Window { Content = surface };
        IReadOnlyList<DialogButton>? configuredButtons = null;

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            surface.ApplyTemplate();

            dialog.ButtonsConfigure = buttons => configuredButtons = buttons;

            configuredButtons.ShouldNotBeNull();
            configuredButtons.Select(button => button.StandardButtonType)
                             .ShouldBe(new DialogStandardButton?[]
                             {
                                 DialogStandardButton.Ok,
                                 DialogStandardButton.Cancel
                             }, ignoreOrder: true);
        }
        finally
        {
            surface.Dispose();
            window.Close();
        }
    }

    [Fact]
    public void Button_Click_Raises_Dialog_Event_Before_Requesting_Close()
    {
        var button = new DialogButton
        {
            Content = "Accept",
            Role    = DialogButtonRole.AcceptRole
        };
        var dialog = new AtomUI.Desktop.Controls.Dialog();
        dialog.CustomButtons.Add(button);
        var surface = new DialogSurface(dialog);
        var window = new AtomUI.Desktop.Controls.Window
        {
            Content = surface
        };
        var buttonClickedCount = 0;
        DialogSurfaceCloseRequestedEventArgs? closeRequest = null;
        dialog.ButtonClicked += (_, _) => buttonClickedCount++;
        surface.CloseRequested += (_, e) => closeRequest = e;

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            surface.ApplyTemplate();

            button.RaiseEvent(new RoutedEventArgs(AtomUI.Desktop.Controls.Button.ClickEvent));

            buttonClickedCount.ShouldBe(1);
            closeRequest.ShouldNotBeNull();
            closeRequest.Reason.ShouldBe(DialogCloseReason.Accepted);
            closeRequest.Result.ShouldBe(DialogCode.Accepted);
            closeRequest.SourceButton.ShouldBeSameAs(button);
        }
        finally
        {
            surface.Dispose();
            window.Close();
        }
    }

    [Fact]
    public void Handled_Button_Click_Does_Not_Request_Close()
    {
        var button = new DialogButton
        {
            Content = "Accept",
            Role    = DialogButtonRole.AcceptRole
        };
        var dialog = new AtomUI.Desktop.Controls.Dialog();
        dialog.CustomButtons.Add(button);
        var surface = new DialogSurface(dialog);
        var window = new AtomUI.Desktop.Controls.Window
        {
            Content = surface
        };
        var closeRequestCount = 0;
        dialog.ButtonClicked += (_, e) => e.Handled = true;
        surface.CloseRequested += (_, _) => closeRequestCount++;

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            surface.ApplyTemplate();

            button.RaiseEvent(new RoutedEventArgs(AtomUI.Desktop.Controls.Button.ClickEvent));

            closeRequestCount.ShouldBe(0);
        }
        finally
        {
            surface.Dispose();
            window.Close();
        }
    }

    [Fact]
    public void Escape_Invokes_The_Real_Default_Escape_Button_Once()
    {
        var dialog = new AtomUI.Desktop.Controls.Dialog
        {
            StandardButtons = DialogStandardButton.Ok | DialogStandardButton.Cancel
        };
        var surface = new DialogSurface(dialog);
        var window = new AtomUI.Desktop.Controls.Window
        {
            Content = surface
        };
        var buttonClickedCount = 0;
        DialogSurfaceCloseRequestedEventArgs? closeRequest = null;
        dialog.ButtonClicked += (_, _) => buttonClickedCount++;
        surface.CloseRequested += (_, e) => closeRequest = e;

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            surface.ApplyTemplate();

            surface.TryInvokeStandardButton(Key.Escape).ShouldBeTrue();

            buttonClickedCount.ShouldBe(1);
            closeRequest.ShouldNotBeNull();
            closeRequest.Result.ShouldBe(DialogCode.Rejected);
            closeRequest.SourceButton.StandardButtonType.ShouldBe(DialogStandardButton.Cancel);
        }
        finally
        {
            surface.Dispose();
            window.Close();
        }
    }

    [Fact]
    public void Close_Caption_Raises_A_Host_Close_Request()
    {
        var dialog  = new AtomUI.Desktop.Controls.Dialog();
        var surface = new DialogSurface(dialog);
        var window = new AtomUI.Desktop.Controls.Window
        {
            Content = surface
        };
        var closeRequestCount = 0;
        surface.HostCloseRequested += (_, _) => closeRequestCount++;

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            surface.ApplyTemplate();

            var header = surface.Header.ShouldNotBeNull();
            var closeButton = header.GetVisualDescendants()
                                    .OfType<DialogCaptionButton>()
                                    .Single(button => button.Name == "PART_CloseButton");
            closeButton.RaiseEvent(new RoutedEventArgs(AtomUI.Desktop.Controls.Button.ClickEvent));

            closeRequestCount.ShouldBe(1);
        }
        finally
        {
            surface.Dispose();
            window.Close();
        }
    }

    [Fact]
    public void Resizable_Surface_Owns_One_Template_Resizer()
    {
        var dialog = new AtomUI.Desktop.Controls.Dialog
        {
            IsResizable = true
        };
        var surface = new DialogSurface(dialog);
        var window = new AtomUI.Desktop.Controls.Window
        {
            Content = surface
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            surface.ApplyTemplate();

            var resizer = surface.Resizer.ShouldNotBeNull();
            resizer.IsVisible.ShouldBeTrue();

            dialog.IsResizable = false;
            resizer.IsVisible.ShouldBeFalse();
        }
        finally
        {
            surface.Dispose();
            window.Close();
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Each_Terminal_Default_Minimum_Frame_Follows_Its_Own_MinWidth_Semantics(bool messageBox)
    {
        // Dialog.MinWidth is the content viewport baseline (frame = Token + padding + border, 248 by default);
        // MessageBox.MinWidth sizes the whole frame (410 by default, like antd's confirm width).
        AtomUI.Desktop.Controls.Dialog dialog = messageBox ? new AtomUI.Desktop.Controls.MessageBox() : new AtomUI.Desktop.Controls.Dialog();
        dialog.IsFooterVisible = false;
        var surface = new DialogSurface(dialog);
        var window = new AtomUI.Desktop.Controls.Window { Content = surface };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            surface.ApplyTemplate();
            window.UpdateLayout();

            var contentFrame = surface.GetVisualDescendants().OfType<Border>().Single(border => border.Name == "ContentFrame");
            var horizontalChrome = contentFrame.Padding.Left + contentFrame.Padding.Right +
                                   contentFrame.BorderThickness.Left + contentFrame.BorderThickness.Right;
            var minWidth = messageBox
                ? ControlTokenAccessor.Capture<AtomUI.Desktop.Controls.MessageBox>(surface).GetOwn<double>(MessageBoxTokenKind.MinWidth)
                : ControlTokenAccessor.Capture<AtomUI.Desktop.Controls.Dialog>(surface).GetOwn<double>(DialogTokenKind.MinWidth);
            var expectedFrame = messageBox ? Math.Max(minWidth, horizontalChrome) : minWidth + horizontalChrome;

            surface.ContentViewportMinWidth.ShouldBe(minWidth);
            surface.MeasureStructuralMinimum().Width.ShouldBe(expectedFrame);
            surface.MeasureStructuralMinimum().Width.ShouldBe(messageBox ? 410d : 248d, "default frame width of the terminal");
        }
        finally
        {
            surface.Dispose();
            window.Close();
        }
    }

    [Fact]
    public void Structural_Minimum_Combines_Header_Content_Viewport_And_Visible_Footer()
    {
        var dialog = new AtomUI.Desktop.Controls.Dialog
        {
            Title = "Structural minimum",
            StandardButtons = DialogStandardButton.Ok | DialogStandardButton.Cancel
        };
        var surface = new DialogSurface(dialog);
        var window = new AtomUI.Desktop.Controls.Window { Content = surface };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            surface.ApplyTemplate();

            var withFooter = surface.MeasureStructuralMinimum();
            withFooter.Width.ShouldBeGreaterThanOrEqualTo(surface.ContentViewportMinWidth);
            withFooter.Height.ShouldBeGreaterThan(surface.ContentViewportMinHeight);

            dialog.IsFooterVisible = false;
            Dispatcher.UIThread.RunJobs();

            var withoutFooter = surface.MeasureStructuralMinimum();
            withoutFooter.Height.ShouldBeLessThan(withFooter.Height);
        }
        finally
        {
            surface.Dispose();
            window.Close();
        }
    }

    [Fact]
    public void Structural_Minimum_Does_Not_Include_Arbitrary_Content_DesiredSize()
    {
        var content = new Border { Width = 900, Height = 700 };
        var dialog = new AtomUI.Desktop.Controls.Dialog
        {
            Content = content,
            IsFooterVisible = false
        };
        var surface = new DialogSurface(dialog);
        var window = new AtomUI.Desktop.Controls.Window { Content = surface };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            surface.ApplyTemplate();

            var minimum = surface.MeasureStructuralMinimum();

            minimum.Width.ShouldBeLessThan(content.Width);
            minimum.Height.ShouldBeLessThan(content.Height);
        }
        finally
        {
            surface.Dispose();
            window.Close();
        }
    }

    [Fact]
    public void Structural_Minimum_Tracks_Effective_Button_Content()
    {
        var customButton = new DialogButton { Content = "OK" };
        var dialog = new AtomUI.Desktop.Controls.Dialog();
        dialog.CustomButtons.Add(customButton);
        var surface = new DialogSurface(dialog);
        var window = new AtomUI.Desktop.Controls.Window { Content = surface };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            surface.ApplyTemplate();
            var initialMinimum = surface.MeasureStructuralMinimum();

            customButton.Content = new string('W', 80);
            Dispatcher.UIThread.RunJobs();
            var updatedMinimum = surface.MeasureStructuralMinimum();

            updatedMinimum.Width.ShouldBeGreaterThan(initialMinimum.Width);
        }
        finally
        {
            surface.Dispose();
            window.Close();
        }
    }

    [Fact]
    public void Structural_Minimum_Change_Is_Published_When_Template_Spacing_Changes()
    {
        var dialog = new AtomUI.Desktop.Controls.Dialog
        {
            StandardButtons = DialogStandardButton.Ok | DialogStandardButton.Cancel
        };
        var surface = new DialogSurface(dialog);
        var window = new AtomUI.Desktop.Controls.Window { Content = surface };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            surface.ApplyTemplate();
            Dispatcher.UIThread.RunJobs();

            var footerFrame = surface.GetVisualDescendants()
                                     .OfType<Border>()
                                     .Single(border => border.Name == "FooterFrame");
            var initialMinimum = surface.MeasureStructuralMinimum();
            var changeCount = 0;
            surface.StructuralMinimumChanged += (_, _) => changeCount++;

            footerFrame.Padding = new Thickness(
                footerFrame.Padding.Left,
                footerFrame.Padding.Top + 20,
                footerFrame.Padding.Right,
                footerFrame.Padding.Bottom + 20);
            Dispatcher.UIThread.RunJobs();

            surface.MeasureStructuralMinimum().Height.ShouldBeGreaterThan(initialMinimum.Height);
            changeCount.ShouldBeGreaterThan(0);
        }
        finally
        {
            surface.Dispose();
            window.Close();
        }
    }
}
