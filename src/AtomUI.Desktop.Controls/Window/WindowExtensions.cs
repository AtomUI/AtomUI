// Referenced from https://github.com/kikipoulet/SukiUI project

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform;

namespace AtomUI.Desktop.Controls;

public static class WindowExtensions
{
    public static Screen? GetHostScreen(this Window window)
    {
        if (window.Screens.ScreenCount == 0)
        {
            return null;
        }
        return window.Screens.ScreenFromWindow(window) ??
               window.Screens.Primary ??
               window.Screens.All[0];
    }
    
    public static void CenterOnScreen(this Window window)
    {
        window.CenterOnScreen(window.GetHostScreen());
    }
    
    public static void CenterOnScreen(this Window window, Screen? screen)
    {
        if (screen is null || window.WindowState != WindowState.Normal)
        {
            return;
        }

        window.Position = new PixelPoint((int)(screen.Bounds.X + screen.WorkingArea.Width / 2.0 - window.Bounds.Width / (2.0 / window.RenderScaling)),
            (int)(screen.Bounds.Y + screen.WorkingArea.Height / 2.0 - window.Bounds.Height / (2.0 / window.RenderScaling)));
    }
    
    public static void ConstrainMaxSizeToScreenRatio(this Window window, double maxWidthScreenRatio, double maxHeightScreenRatio)
    {
        Screen?      screen      = null;
        WindowState? windowState = null;

        if (!double.IsNaN(maxWidthScreenRatio))
        {
            windowState = window.WindowState;
            if (maxWidthScreenRatio <= 0 || windowState is WindowState.FullScreen or WindowState.Maximized)
            {
                window.MaxWidth = double.PositiveInfinity;
            }
            else
            {
                screen = window.GetHostScreen();
                if (screen is null)
                {
                    return;
                }

                var desiredMaxWidth = screen.WorkingArea.Width / window.RenderScaling * maxWidthScreenRatio;
                window.MaxWidth = Math.Max(window.MinWidth, desiredMaxWidth);
            }
        }

        if (!double.IsNaN(maxHeightScreenRatio))
        {
            windowState ??= window.WindowState;
            if (maxHeightScreenRatio <= 0 || windowState is WindowState.FullScreen or WindowState.Maximized)
            {
                window.MaxHeight = double.PositiveInfinity;
            }
            else
            {
                screen ??= window.GetHostScreen();
                if (screen is null)
                {
                    return;
                }

                var desiredMaxHeight = screen.WorkingArea.Height / window.RenderScaling * maxHeightScreenRatio;
                window.MaxHeight = Math.Max(window.MinHeight, desiredMaxHeight);
            }
        }
    }
}