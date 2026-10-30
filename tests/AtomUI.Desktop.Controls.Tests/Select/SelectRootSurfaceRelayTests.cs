using AtomUI.Theme.Resources;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AtomUICascader = AtomUI.Desktop.Controls.Cascader;
using AtomUISelect = AtomUI.Desktop.Controls.Select;
using AtomUITreeSelect = AtomUI.Desktop.Controls.TreeSelect;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.SelectControl;

/// <summary>
/// The visible Select / TreeSelect / Cascader outline is drawn by the shared
/// <c>AddOnDecoratedBox</c> frame, whose state machine owns <c>BorderBrush</c> / <c>Background</c>.
/// These tests pin the root-level customization contract so an owner-scoped root Setter reaches
/// that frame instead of being silently ignored, matching the input-family relay
/// (<c>TextInputRootBrushRelayTests</c>).
/// </summary>
public class SelectRootSurfaceRelayTests
{
    static SelectRootSurfaceRelayTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    public static TheoryData<string> SelectKinds()
    {
        return new TheoryData<string>
        {
            nameof(AtomUISelect),
            nameof(AtomUITreeSelect),
            nameof(AtomUICascader)
        };
    }

    [Theory]
    [MemberData(nameof(SelectKinds))]
    public void Root_BorderBrush_Customization_Wins_Over_Hover_And_Focus_Border_States(string kind)
    {
        var control = CreateSelect(kind, out var window);
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            var frame        = FindFrame(control);
            var customBorder = new SolidColorBrush(Color.Parse("#f759ab"));

            control.BorderBrush = customBorder;
            Dispatcher.UIThread.RunJobs();

            GetSolidBrushColor(frame.BorderBrush).ShouldBe(customBorder.Color,
                "The root BorderBrush set on the select owner must reach the input frame as a local value.");

            frame.IsInnerBoxHover = true;
            Dispatcher.UIThread.RunJobs();

            GetSolidBrushColor(frame.BorderBrush).ShouldBe(customBorder.Color,
                "A customized root border intentionally suppresses the hover border color.");
        }
        finally
        {
            window.Close();
        }
    }

    [Theory]
    [MemberData(nameof(SelectKinds))]
    public void Root_Background_Customization_Replaces_Rest_Background(string kind)
    {
        var control = CreateSelect(kind, out var window);
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            var frame            = FindFrame(control);
            var customBackground = new SolidColorBrush(Color.Parse("#fff1f0"));

            control.Background = customBackground;
            Dispatcher.UIThread.RunJobs();

            GetSolidBrushColor(frame.Background).ShouldBe(customBackground.Color,
                "The root Background set on the select owner must reach the input frame as a local value.");
        }
        finally
        {
            window.Close();
        }
    }

    [Theory]
    [MemberData(nameof(SelectKinds))]
    public void Root_BorderBrush_Clear_Restores_The_Frame_State_Machine(string kind)
    {
        var control = CreateSelect(kind, out var window);
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            var frame        = FindFrame(control);
            var restBorder   = GetThemeResource<IBrush>(SharedTokenKind.ColorBorder);
            var customBorder = new SolidColorBrush(Color.Parse("#f759ab"));

            control.BorderBrush = customBorder;
            Dispatcher.UIThread.RunJobs();
            GetSolidBrushColor(frame.BorderBrush).ShouldBe(customBorder.Color);

            control.BorderBrush = null;
            Dispatcher.UIThread.RunJobs();

            GetSolidBrushColor(frame.BorderBrush).ShouldBe(GetSolidBrushColor(restBorder),
                "Clearing the root BorderBrush must fall back to the frame theme rest state.");
        }
        finally
        {
            window.Close();
        }
    }

    [Theory]
    [MemberData(nameof(SelectKinds))]
    public void Root_BorderBrush_Set_Before_Template_Is_Applied_Is_Still_Relayed(string kind)
    {
        var customBorder = new SolidColorBrush(Color.Parse("#f759ab"));
        var control      = CreateSelect(kind, out var window, customBorder);
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            GetSolidBrushColor(FindFrame(control).BorderBrush).ShouldBe(customBorder.Color,
                "A root brush assigned before the template exists must be relayed once the template is applied.");
        }
        finally
        {
            window.Close();
        }
    }

    [Theory]
    [MemberData(nameof(SelectKinds))]
    public void Uncustomized_Selects_Keep_The_Frame_Theme_Rest_State(string kind)
    {
        var control = CreateSelect(kind, out var window);
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            var frame      = FindFrame(control);
            var restBorder = GetThemeResource<IBrush>(SharedTokenKind.ColorBorder);

            control.BorderBrush.ShouldBeNull(
                "The select themes must not set a dead owner-level BorderBrush default; the frame theme owns the rest state.");
            GetSolidBrushColor(frame.BorderBrush).ShouldBe(GetSolidBrushColor(restBorder));
        }
        finally
        {
            window.Close();
        }
    }

    private static AbstractSelect CreateSelect(
        string kind,
        out AvaloniaWindow window,
        IBrush? borderBrush = null)
    {
        AbstractSelect control = kind switch
        {
            nameof(AtomUISelect)     => new AtomUISelect { Width = 200, IsMotionEnabled = false },
            nameof(AtomUITreeSelect) => new AtomUITreeSelect { Width = 200, IsMotionEnabled = false },
            nameof(AtomUICascader)   => new AtomUICascader { Width = 200, IsMotionEnabled = false },
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null)
        };

        if (borderBrush is not null)
        {
            control.BorderBrush = borderBrush;
        }

        window = new AvaloniaWindow
        {
            Width   = 260,
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
