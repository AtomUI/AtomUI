using Avalonia.Interactivity;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Carries the request raised by the fit-to-window button of an image preview toolbar.
/// </summary>
public class ImageFitToWindowEventArgs : RoutedEventArgs
{
    /// <summary>
    /// Whether the image is requested to be fitted to the preview window. The built-in toolbar raises
    /// the request with <see langword="true" /> only.
    /// </summary>
    public bool IsFitToWindow { get; set; }
    
    /// <summary>
    /// Creates the arguments of a fit-to-window request.
    /// </summary>
    /// <param name="isFitToWindow">
    /// Whether the image is requested to be fitted to the preview window.
    /// </param>
    public ImageFitToWindowEventArgs(bool isFitToWindow)
    {
        IsFitToWindow = isFitToWindow;
    }
}