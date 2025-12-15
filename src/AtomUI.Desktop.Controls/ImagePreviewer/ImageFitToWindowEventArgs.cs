using Avalonia.Interactivity;

namespace AtomUI.Desktop.Controls;

public class ImageFitToWindowEventArgs : RoutedEventArgs
{
    public bool IsFitToWindow { get; set; }
    
    public ImageFitToWindowEventArgs(bool isFitToWindow)
    {
        IsFitToWindow = isFitToWindow;
    }
}