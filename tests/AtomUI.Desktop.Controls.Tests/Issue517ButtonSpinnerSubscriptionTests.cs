using System.Reflection;
using Avalonia.Controls;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AtomUINumericUpDown = AtomUI.Desktop.Controls.NumericUpDown;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests;

public class Issue517ButtonSpinnerSubscriptionTests
{
    private static readonly FieldInfo MouseMoveDisposableField =
        typeof(ButtonSpinnerDecoratedBox).GetField("_mouseMoveDisposable",
            BindingFlags.Instance | BindingFlags.NonPublic)!;

    static Issue517ButtonSpinnerSubscriptionTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Enabling_After_Detach_Does_Not_Resubscribe_Global_Input()
    {
        var numericUpDown = new AtomUINumericUpDown
        {
            Width           = 160,
            Value           = 3,
            IsEnabled       = false,
            IsMotionEnabled = false
        };
        var host   = new ContentControl { Content = numericUpDown };
        var window = CreateWindow(host);
        try
        {
            var decoratedBox = GetDecoratedBox(numericUpDown);
            MouseMoveDisposableField.GetValue(decoratedBox).ShouldBeNull();

            host.Content = null;
            Dispatcher.UIThread.RunJobs();
            decoratedBox.IsAttachedToVisualTree().ShouldBeFalse();

            numericUpDown.IsEnabled = true;
            Dispatcher.UIThread.RunJobs();

            decoratedBox.IsEffectivelyEnabled.ShouldBeTrue();
            MouseMoveDisposableField.GetValue(decoratedBox).ShouldBeNull();
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    [Fact]
    public void Detach_Releases_Global_Input_Subscription()
    {
        var numericUpDown = new AtomUINumericUpDown
        {
            Width           = 160,
            Value           = 3,
            IsMotionEnabled = false
        };
        var host   = new ContentControl { Content = numericUpDown };
        var window = CreateWindow(host);
        try
        {
            var decoratedBox = GetDecoratedBox(numericUpDown);
            MouseMoveDisposableField.GetValue(decoratedBox).ShouldNotBeNull();

            host.Content = null;
            Dispatcher.UIThread.RunJobs();

            MouseMoveDisposableField.GetValue(decoratedBox).ShouldBeNull();
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    [Fact]
    public void Showing_Handle_After_Attach_Subscribes_Global_Input()
    {
        var numericUpDown = new AtomUINumericUpDown
        {
            Width           = 160,
            Value           = 3,
            IsMotionEnabled = false
        };
        var window = CreateWindow(numericUpDown);
        try
        {
            var decoratedBox = GetDecoratedBox(numericUpDown);
            numericUpDown.ShowButtonSpinner = false;
            Dispatcher.UIThread.RunJobs();
            decoratedBox.IsShowHandle.ShouldBeFalse();
            MouseMoveDisposableField.GetValue(decoratedBox).ShouldBeNull();

            numericUpDown.ShowButtonSpinner = true;
            Dispatcher.UIThread.RunJobs();
            decoratedBox.IsShowHandle.ShouldBeTrue();
            MouseMoveDisposableField.GetValue(decoratedBox).ShouldNotBeNull();
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    private static ButtonSpinnerDecoratedBox GetDecoratedBox(AtomUINumericUpDown numericUpDown)
    {
        return numericUpDown.GetVisualDescendants()
                            .OfType<ButtonSpinnerDecoratedBox>()
                            .Single();
    }

    private static AvaloniaWindow CreateWindow(Control content)
    {
        var window = new AvaloniaWindow
        {
            Width   = 240,
            Height  = 120,
            Content = content
        };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        return window;
    }
}
