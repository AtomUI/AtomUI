using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Media;

namespace AtomUIGallery.ShowCases.Window;

internal partial class WindowPlayground : AtomUI.Desktop.Controls.Window
{
    public WindowPlayground()
    {
        InitializeComponent();
        // macOS 使用原生红绿灯按钮，这三个 visibility 属性只作用于 Windows/Linux 的自绘按钮
        if (OperatingSystem.IsMacOS())
        {
            MinimizeCaptionRow.IsVisible = false;
            MaximizeCaptionRow.IsVisible = false;
            FullScreenCaptionRow.IsVisible = false;
        }
    }

    private void HandleTitleChanged(object? sender, TextChangedEventArgs e)
    {
        Title = string.IsNullOrWhiteSpace(TitleTextBox.Text) ? "AtomUI Playground" : TitleTextBox.Text;
    }

    private void HandleTitleVisibleSwitched(object? sender, RoutedEventArgs e)
    {
        if (sender is ToggleButton toggle)
        {
            IsTitleVisible = toggle.IsChecked == true;
        }
    }

    private void HandleTitleBarVisibleSwitched(object? sender, RoutedEventArgs e)
    {
        if (sender is ToggleButton toggle)
        {
            IsTitleBarVisible = toggle.IsChecked == true;
        }
    }

    private void HandleLogoSwitched(object? sender, RoutedEventArgs e)
    {
        if (sender is ToggleButton toggle)
        {
            // Icon 是 Visual，每次打开都新建实例，避免重新挂载时的 visual parent 冲突
            Logo = toggle.IsChecked == true ? new AtomUiLogoIcon() : null;
        }
    }

    private void HandleAddOnSwitched(object? sender, RoutedEventArgs e)
    {
        if (sender is ToggleButton toggle)
        {
            LeftAddOn = toggle.IsChecked == true ? Resources["PlaygroundLeftAddOn"] : null;
            RightAddOn = toggle.IsChecked == true ? Resources["PlaygroundRightAddOn"] : null;
        }
    }

    private void HandleAlignmentChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (sender is AtomUI.Desktop.Controls.Segmented segmented)
        {
            TitleAlignment = segmented.SelectedIndex switch
            {
                1 => AtomUI.Desktop.Controls.WindowTitleBarTitleAlignment.Left,
                2 => AtomUI.Desktop.Controls.WindowTitleBarTitleAlignment.Center,
                3 => AtomUI.Desktop.Controls.WindowTitleBarTitleAlignment.WindowCenter,
                4 => AtomUI.Desktop.Controls.WindowTitleBarTitleAlignment.Right,
                _ => AtomUI.Desktop.Controls.WindowTitleBarTitleAlignment.Auto
            };
        }
    }

    private void HandleLogoVisibilityChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (sender is AtomUI.Desktop.Controls.Segmented segmented)
        {
            LogoVisibility = segmented.SelectedIndex switch
            {
                1 => AtomUI.Desktop.Controls.WindowTitleBarLogoVisibility.Always,
                2 => AtomUI.Desktop.Controls.WindowTitleBarLogoVisibility.Never,
                _ => AtomUI.Desktop.Controls.WindowTitleBarLogoVisibility.Auto
            };
        }
    }

    private void HandleBackgroundPresetChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (sender is AtomUI.Desktop.Controls.Segmented segmented)
        {
            switch (segmented.SelectedIndex)
            {
                case 1:
                    TitleBarFrameBackground = Resources["PlaygroundGradientTitleBarBrush"] as IBrush;
                    ContentFrameBackground = Resources["PlaygroundGradientContentBrush"] as IBrush;
                    break;
                case 2:
                    TitleBarFrameBackground = Resources["PlaygroundDarkTitleBarBrush"] as IBrush;
                    ContentFrameBackground = Resources["PlaygroundDarkContentBrush"] as IBrush;
                    break;
                default:
                    TitleBarFrameBackground = null;
                    ContentFrameBackground = null;
                    break;
            }
        }
    }

    private void HandleMinimizeCaptionSwitched(object? sender, RoutedEventArgs e)
    {
        if (sender is ToggleButton toggle)
        {
            IsMinimizeCaptionButtonVisible = toggle.IsChecked == true;
        }
    }

    private void HandleMaximizeCaptionSwitched(object? sender, RoutedEventArgs e)
    {
        if (sender is ToggleButton toggle)
        {
            IsMaximizeCaptionButtonVisible = toggle.IsChecked == true;
        }
    }

    private void HandleFullScreenCaptionSwitched(object? sender, RoutedEventArgs e)
    {
        if (sender is ToggleButton toggle)
        {
            IsFullScreenCaptionButtonVisible = toggle.IsChecked == true;
        }
    }

    private void HandlePinCaptionSwitched(object? sender, RoutedEventArgs e)
    {
        if (sender is ToggleButton toggle)
        {
            IsPinCaptionButtonVisible = toggle.IsChecked == true;
        }
    }
}
