using Avalonia.Input.Raw;
using Avalonia.Input;
using Avalonia.Headless;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AtomUIButtonSpinner = AtomUI.Desktop.Controls.ButtonSpinner;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.ButtonSpinnerTests;

/// <summary>
/// The visible outer border is drawn by the shared input frame, whose state machine owns
/// <c>BorderBrush</c>. These tests pin the root-level customization contract: setting
/// <c>BorderBrush</c> on the owner (directly or through an owner-scoped Style, as the Gallery
/// Semantic Part example does) must reach that frame, and clearing it must hand the property
/// back to the state machine.
/// </summary>
public class ButtonSpinnerRootSurfaceTests
{
    static ButtonSpinnerRootSurfaceTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Root_BorderBrush_Relays_Onto_The_Input_Frame()
    {
        var borderBrush = new SolidColorBrush(Colors.MediumPurple);
        var spinner = new AtomUIButtonSpinner
        {
            Width       = 320,
            BorderBrush = borderBrush
        };

        var window = Show(spinner);
        try
        {
            FrameOf(spinner).BorderBrush.ShouldBe(borderBrush);
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Owner_Scoped_BorderBrush_Style_Reaches_The_Input_Frame()
    {
        var borderBrush = new SolidColorBrush(Colors.RoyalBlue);
        var spinner = new AtomUIButtonSpinner
        {
            Width   = 320,
            Classes = { "semantic-object" }
        };
        var ownerStyle = new Style(selector => selector.OfType<AtomUIButtonSpinner>().Class("semantic-object"));
        ownerStyle.Setters.Add(new Setter(AtomUIButtonSpinner.BorderBrushProperty, borderBrush));
        spinner.Styles.Add(ownerStyle);

        var window = Show(spinner);
        try
        {
            FrameOf(spinner).BorderBrush.ShouldBe(borderBrush);
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Clearing_The_Root_BorderBrush_Restores_The_Frame_State_Machine()
    {
        var borderBrush = new SolidColorBrush(Colors.MediumPurple);
        var spinner = new AtomUIButtonSpinner
        {
            Width       = 320,
            BorderBrush = borderBrush
        };

        var window = Show(spinner);
        try
        {
            var frame = FrameOf(spinner);
            frame.BorderBrush.ShouldBe(borderBrush);

            spinner.BorderBrush = null;
            Dispatcher.UIThread.RunJobs();

            frame.BorderBrush.ShouldNotBe(borderBrush);
            frame.BorderBrush.ShouldNotBeNull();
        }
        finally
        {
            window.Close();
        }
    }

    /// <summary>
    /// The frame theme defaults <c>IsMotionEnabled</c> to the shared motion token, so without an
    /// explicit owner propagation the frame keeps its BorderBrush/Background transitions running
    /// even when the owner turns motion off. That also breaks the relay below: while a
    /// SolidColorBrush transition is in flight its value outranks the relayed local value, so a
    /// root BorderBrush change appears not to apply.
    /// </summary>
    [Fact]
    public void Motion_And_Pointer_Hover_State_Reach_The_Input_Frame()
    {
        var spinner = new AtomUIButtonSpinner
        {
            Width           = 320,
            IsMotionEnabled = false,
            IsButtonSpinnerFloatable = true
        };

        var window = Show(spinner);
        try
        {
            var frame = FrameOf(spinner);
            frame.IsMotionEnabled.ShouldBeFalse(
                "The owner IsMotionEnabled setting must reach the input frame, matching LineEdit.");
            var point = frame.TranslatePoint(new Point(50, frame.Bounds.Height / 2), window).ShouldNotBeNull();
            var other = new AvaloniaWindow { Width = 480, Height = 160, Content = new Border { Background = Brushes.Blue } };
            other.Show();
            try
            {
                other.MouseMove(point);
                frame.IsSpinnerContentHover.ShouldBeFalse();
                window.MouseMove(point);
                frame.IsSpinnerContentHover.ShouldBeTrue();
                frame.HandleOpacity.ShouldBe(1);
                frame.ContentRightShift.ShouldBeLessThan(0d);

                spinner.IsButtonSpinnerVisible = false;
                frame.HandleOpacity.ShouldBe(0);
                frame.ContentRightShift.ShouldBe(0d);
                spinner.IsButtonSpinnerVisible = true;
                window.MouseMove(point);
                frame.ContentRightShift.ShouldBeLessThan(0d);

                using var device = new MouseDevice();
                var root = window.GetPresentationSource().ShouldNotBeNull().ShouldBeAssignableTo<IInputRoot>();
                var manager = (IInputManager)AvaloniaLocator.CurrentMutable.GetService(typeof(IInputManager))!;
                manager.ProcessInput(new RawPointerEventArgs(device, 42, root, RawPointerEventType.LeaveWindow, new Point(-1, -1), default));
                frame.IsSpinnerContentHover.ShouldBeFalse();
                frame.HandleOpacity.ShouldBe(0);
            }
            finally
            {
                other.Close();
            }
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Root_BorderBrush_Live_Change_Applies_Immediately_When_Motion_Is_Disabled()
    {
        var spinner = new AtomUIButtonSpinner
        {
            Width           = 320,
            IsMotionEnabled = false
        };

        var window = Show(spinner);
        try
        {
            var frame        = FrameOf(spinner);
            var customBorder = new SolidColorBrush(Colors.MediumPurple);

            // Live change on an already-templated control: this is the path an owner-scoped Style
            // or a property assignment on a running app takes.
            spinner.BorderBrush = customBorder;
            Dispatcher.UIThread.RunJobs();

            frame.BorderBrush.ShouldBe(customBorder,
                "With motion disabled the relayed border must be visible immediately, not animated from the frame rest state.");
        }
        finally
        {
            window.Close();
        }
    }

    private static ButtonSpinnerDecoratedBox FrameOf(AtomUIButtonSpinner spinner)
    {
        return spinner.GetVisualDescendants()
                      .OfType<ButtonSpinnerDecoratedBox>()
                      .Single();
    }

    private static AvaloniaWindow Show(Control content)
    {
        var window = new AvaloniaWindow
        {
            Width   = 480,
            Height  = 160,
            Content = content
        };
        window.Show();
        content.ApplyTemplate();
        window.UpdateLayout();
        Dispatcher.UIThread.RunJobs();
        return window;
    }
}
