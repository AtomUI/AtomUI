using AtomUI.Desktop.Controls;
using Avalonia.Interactivity;

namespace AtomUIGallery.ShowCases.Window;

internal partial class FullScreenTitleBarDemoWindow : AtomUI.Desktop.Controls.Window
{
    public FullScreenTitleBarDemoWindow()
    {
        InitializeComponent();
    }

    private void HandlePinChanged(object? sender, RoutedEventArgs e)
    {
        FullScreenTitleBarMode = PinButton.IsChecked == true
            ? FullScreenTitleBarMode.Pinned
            : FullScreenTitleBarMode.AutoHide;
    }

    private void HandleToggleFullScreenClick(object? sender, RoutedEventArgs e)
    {
        ToggleFullScreen();
    }
}
