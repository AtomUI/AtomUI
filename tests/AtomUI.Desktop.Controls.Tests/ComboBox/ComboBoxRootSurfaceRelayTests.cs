using AtomUI.Theme.Resources;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AtomUIComboBox = AtomUI.Desktop.Controls.ComboBox;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.ComboBox;

/// <summary>
/// ComboBox 的可见外框同样由共享 <c>AddOnDecoratedBox</c> 帧绘制，其状态机拥有 <c>BorderBrush</c> /
/// <c>Background</c>。ComboBox 派生自 Avalonia 的 ComboBox，不在 <c>AbstractTextInput</c> /
/// <c>AbstractSelect</c> 的继承链上，因此「输入族根边框定制失效」的修复（`RelayRootSurfaceBrush`）没有覆盖到它。
/// 这些测试锁定根级定制契约：owner 上的根 <c>BorderBrush</c> / <c>Background</c> Setter 必须中继到该帧，
/// 而不是被静默忽略（对齐 <c>SelectRootSurfaceRelayTests</c> 与 <c>TextInputRootBrushRelayTests</c>）。
/// </summary>
public class ComboBoxRootSurfaceRelayTests
{
    static ComboBoxRootSurfaceRelayTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Root_BorderBrush_Customization_Wins_Over_Hover_Border_State()
    {
        var comboBox = CreateComboBox(out var window);
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            var frame        = FindFrame(comboBox);
            var customBorder = new SolidColorBrush(Color.Parse("#f759ab"));

            comboBox.BorderBrush = customBorder;
            Dispatcher.UIThread.RunJobs();

            GetSolidBrushColor(frame.BorderBrush).ShouldBe(customBorder.Color,
                "The root BorderBrush set on the ComboBox owner must reach the input frame as a local value.");

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

    [Fact]
    public void Root_Background_Customization_Replaces_Rest_Background()
    {
        var comboBox = CreateComboBox(out var window);
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            var frame            = FindFrame(comboBox);
            var customBackground = new SolidColorBrush(Color.Parse("#fff1f0"));

            comboBox.Background = customBackground;
            Dispatcher.UIThread.RunJobs();

            GetSolidBrushColor(frame.Background).ShouldBe(customBackground.Color,
                "The root Background set on the ComboBox owner must reach the input frame as a local value.");
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Root_BorderBrush_Clear_Restores_The_Frame_State_Machine()
    {
        var comboBox = CreateComboBox(out var window);
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            var frame        = FindFrame(comboBox);
            var restBorder   = GetThemeResource<IBrush>(SharedTokenKind.ColorBorder);
            var customBorder = new SolidColorBrush(Color.Parse("#f759ab"));

            comboBox.BorderBrush = customBorder;
            Dispatcher.UIThread.RunJobs();
            GetSolidBrushColor(frame.BorderBrush).ShouldBe(customBorder.Color);

            comboBox.BorderBrush = null;
            Dispatcher.UIThread.RunJobs();

            GetSolidBrushColor(frame.BorderBrush).ShouldBe(GetSolidBrushColor(restBorder),
                "Clearing the root BorderBrush must fall back to the frame theme rest state.");
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Root_BorderBrush_Set_Before_Template_Is_Applied_Is_Still_Relayed()
    {
        var customBorder = new SolidColorBrush(Color.Parse("#f759ab"));
        var comboBox     = CreateComboBox(out var window, customBorder);
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            GetSolidBrushColor(FindFrame(comboBox).BorderBrush).ShouldBe(customBorder.Color,
                "A root brush assigned before the template exists must be relayed once the template is applied.");
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Uncustomized_ComboBox_Keeps_The_Frame_Theme_Rest_State()
    {
        var comboBox = CreateComboBox(out var window);
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            var frame      = FindFrame(comboBox);
            var restBorder = GetThemeResource<IBrush>(SharedTokenKind.ColorBorder);

            comboBox.BorderBrush.ShouldBeNull(
                "The ComboBox theme must not set a dead owner-level BorderBrush default; the frame theme owns the rest state.");
            GetSolidBrushColor(frame.BorderBrush).ShouldBe(GetSolidBrushColor(restBorder));
        }
        finally
        {
            window.Close();
        }
    }

    private static AtomUIComboBox CreateComboBox(
        out AvaloniaWindow window,
        IBrush? borderBrush = null)
    {
        var comboBox = new AtomUIComboBox
        {
            Width           = 200,
            IsMotionEnabled = false,
            ItemsSource     = new[] { "Alpha", "Beta" }
        };

        if (borderBrush is not null)
        {
            comboBox.BorderBrush = borderBrush;
        }

        window = new AvaloniaWindow
        {
            Width   = 260,
            Height  = 160,
            Content = comboBox
        };
        return comboBox;
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
