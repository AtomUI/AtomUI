using Avalonia.Threading;
using Shouldly;
using Xunit;
using AtomUIButton = AtomUI.Desktop.Controls.Button;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.Buttons;

public class ButtonCreationExceptionTests
{
    static ButtonCreationExceptionTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Configuring_A_Button_Before_It_Joins_A_Tree_Throws_No_Exceptions()
    {
        var thrown         = 0;
        var creatingThread = Environment.CurrentManagedThreadId;
        void Count(object? sender, System.Runtime.ExceptionServices.FirstChanceExceptionEventArgs e)
        {
            // Other tests may run in parallel; only count exceptions raised by this construction.
            if (Environment.CurrentManagedThreadId == creatingThread)
            {
                Interlocked.Increment(ref thrown);
            }
        }

        AppDomain.CurrentDomain.FirstChanceException += Count;
        AtomUIButton button;
        try
        {
            // Mirrors XAML: every property is applied while the Button has no themed ancestor.
            button = new AtomUIButton
            {
                ButtonType = ButtonType.Primary,
                IsDanger   = true,
                IsGhost    = true,
                Content    = "Primary"
            };
        }
        finally
        {
            AppDomain.CurrentDomain.FirstChanceException -= Count;
        }

        thrown.ShouldBe(0);

        var window = new AvaloniaWindow { Width = 200, Height = 80, Content = button };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            // The variant variables are still configured once the Button is attached.
            button.EffectiveIsDanger.ShouldBeTrue();
            button.Foreground.ShouldNotBeNull();
            button.BorderBrush.ShouldNotBeNull();
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }
}
