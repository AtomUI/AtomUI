using Avalonia.Controls;
using Avalonia.Threading;
using Shouldly;
using Xunit;
using AtomUIToggleSwitch = AtomUI.Desktop.Controls.ToggleSwitch;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.Switch;

public sealed class ToggleSwitchLayoutTests
{
    static ToggleSwitchLayoutTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Content_Width_Recalculates_When_Either_Content_Gets_Wider()
    {
        var toggleSwitch = new AtomUIToggleSwitch
        {
            IsChecked = true,
            OnContent = "On",
            OffContent = "Off"
        };

        ShowInWindow(toggleSwitch, () =>
        {
            var initialWidth = toggleSwitch.Bounds.Width;

            toggleSwitch.OnContent = "Abrir";
            Dispatcher.UIThread.RunJobs();

            var onContentWidth = toggleSwitch.Bounds.Width;
            onContentWidth.ShouldBeGreaterThan(initialWidth);

            toggleSwitch.OffContent = "Fechar";
            Dispatcher.UIThread.RunJobs();

            toggleSwitch.Bounds.Width.ShouldBeGreaterThanOrEqualTo(onContentWidth);
        });
    }

    private static void ShowInWindow(Control content, Action assertion)
    {
        var window = new AvaloniaWindow
        {
            Width = 480,
            Height = 160,
            Content = content
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            assertion();
        }
        finally
        {
            window.Close();
        }
    }
}
