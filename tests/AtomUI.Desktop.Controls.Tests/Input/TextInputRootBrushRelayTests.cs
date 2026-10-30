using AtomUI.Theme.Resources;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AtomUILineEdit = AtomUI.Desktop.Controls.LineEdit;
using AtomUISearchEdit = AtomUI.Desktop.Controls.SearchEdit;
using AtomUITextArea = AtomUI.Desktop.Controls.TextArea;
using AtomUITextBox = AtomUI.Desktop.Controls.TextBox;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.Input;

public class TextInputRootBrushRelayTests
{
    static TextInputRootBrushRelayTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    public static TheoryData<string> TextInputKinds()
    {
        return new TheoryData<string>
        {
            nameof(AtomUILineEdit),
            nameof(AtomUITextBox),
            nameof(AtomUISearchEdit),
            nameof(AtomUITextArea)
        };
    }

    [Theory]
    [MemberData(nameof(TextInputKinds))]
    public void Root_BorderBrush_Customization_Wins_Over_Hover_And_Focus_Border_States(string kind)
    {
        var control = CreateTextInput(kind, out var window);
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            var frame = FindFrame(control);
            var customBorder = new SolidColorBrush(Color.Parse("#f759ab"));

            control.BorderBrush = customBorder;
            Dispatcher.UIThread.RunJobs();

            GetSolidBrushColor(frame.BorderBrush).ShouldBe(customBorder.Color,
                "The root BorderBrush set on the input owner must reach the InputControlFrame as a local value.");

            frame.IsInnerBoxHover = true;
            Dispatcher.UIThread.RunJobs();

            GetSolidBrushColor(frame.BorderBrush).ShouldBe(customBorder.Color,
                "A customized root border intentionally suppresses the hover border color, matching antd inline styles.root semantics.");

            frame.IsInputFocusWithin = true;
            Dispatcher.UIThread.RunJobs();

            GetSolidBrushColor(frame.BorderBrush).ShouldBe(customBorder.Color,
                "A customized root border intentionally suppresses the focus border color change.");
            frame.BoxShadow.ShouldBe(GetThemeResource<BoxShadows>(SharedTokenKind.InputActiveShadow),
                "Focus feedback must stay visible through the box-shadow glow because BoxShadow is a different property slot.");
        }
        finally
        {
            window.Close();
        }
    }

    [Theory]
    [MemberData(nameof(TextInputKinds))]
    public void Root_Background_Customization_Replaces_Rest_Background(string kind)
    {
        var control = CreateTextInput(kind, out var window);
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            var frame = FindFrame(control);
            var customBackground = new SolidColorBrush(Color.Parse("#fff1f0"));

            control.Background = customBackground;
            Dispatcher.UIThread.RunJobs();

            GetSolidBrushColor(frame.Background).ShouldBe(customBackground.Color,
                "The root Background set on the input owner must reach the InputControlFrame as a local value.");
        }
        finally
        {
            window.Close();
        }
    }

    [Theory]
    [MemberData(nameof(TextInputKinds))]
    public void Root_BorderBrush_Default_And_Clear_Use_The_Frame_State_Machine(string kind)
    {
        var control = CreateTextInput(kind, out var window);
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            var frame = FindFrame(control);
            var restBorder    = GetThemeResource<IBrush>(SharedTokenKind.ColorBorder);
            var hoverBorder   = GetThemeResource<IBrush>(SharedTokenKind.ColorPrimaryHover);
            var customBorder  = new SolidColorBrush(Color.Parse("#f759ab"));

            control.BorderBrush.ShouldBeNull(
                "The input themes must not set a dead owner-level BorderBrush default; the frame theme owns the rest state.");
            GetSolidBrushColor(frame.BorderBrush).ShouldBe(GetSolidBrushColor(restBorder));

            frame.IsInnerBoxHover = true;
            Dispatcher.UIThread.RunJobs();
            GetSolidBrushColor(frame.BorderBrush).ShouldBe(GetSolidBrushColor(hoverBorder));

            frame.IsInnerBoxHover = false;
            Dispatcher.UIThread.RunJobs();
            GetSolidBrushColor(frame.BorderBrush).ShouldBe(GetSolidBrushColor(restBorder));

            control.BorderBrush = customBorder;
            Dispatcher.UIThread.RunJobs();
            GetSolidBrushColor(frame.BorderBrush).ShouldBe(customBorder.Color);

            control.BorderBrush = null;
            Dispatcher.UIThread.RunJobs();

            GetSolidBrushColor(frame.BorderBrush).ShouldBe(GetSolidBrushColor(restBorder),
                "Clearing the root BorderBrush must fall back to the frame theme rest state.");

            frame.IsInnerBoxHover = true;
            Dispatcher.UIThread.RunJobs();

            GetSolidBrushColor(frame.BorderBrush).ShouldBe(GetSolidBrushColor(hoverBorder),
                "After the customization is cleared the hover state machine must work again.");
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Root_BorderBrush_Set_Before_Template_Is_Applied_Is_Still_Relayed()
    {
        var lineEdit = new AtomUILineEdit
        {
            Width           = 180,
            IsMotionEnabled = false,
            BorderBrush     = new SolidColorBrush(Color.Parse("#f759ab"))
        };
        var window = new AvaloniaWindow
        {
            Width   = 240,
            Height  = 120,
            Content = lineEdit
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            var frame = FindFrame(lineEdit);
            GetSolidBrushColor(frame.BorderBrush).ShouldBe(Color.Parse("#f759ab"),
                "A root brush assigned before the template exists must be relayed once the template is applied.");
        }
        finally
        {
            window.Close();
        }
    }

    private static AbstractTextInput CreateTextInput(string kind, out AvaloniaWindow window)
    {
        AbstractTextInput control = kind switch
        {
            nameof(AtomUILineEdit)   => new AtomUILineEdit { Width = 180, IsMotionEnabled = false },
            nameof(AtomUITextBox)    => new AtomUITextBox { Width = 180, IsMotionEnabled = false },
            nameof(AtomUISearchEdit) => new AtomUISearchEdit { Width = 180, IsMotionEnabled = false },
            nameof(AtomUITextArea)   => new AtomUITextArea { Width = 180, Height = 80, IsMotionEnabled = false },
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null)
        };

        window = new AvaloniaWindow
        {
            Width   = 240,
            Height  = 160,
            Content = control
        };
        return control;
    }

    private static InputControlFrame FindFrame(Control control)
    {
        var frame = control.GetVisualDescendants()
                           .OfType<InputControlFrame>()
                           .FirstOrDefault();
        frame.ShouldNotBeNull();
        return frame!;
    }

    private static T GetThemeResource<T>(object key)
    {
        var application = Application.Current;
        application.ShouldNotBeNull();
        application!.TryGetResource(key, application.ActualThemeVariant, out var value).ShouldBeTrue();
        value.ShouldBeAssignableTo<T>();
        return (T)value!;
    }

    private static Color GetSolidBrushColor(IBrush? brush)
    {
        brush.ShouldNotBeNull();
        brush.ShouldBeAssignableTo<ISolidColorBrush>();
        return ((ISolidColorBrush)brush!).Color;
    }
}
