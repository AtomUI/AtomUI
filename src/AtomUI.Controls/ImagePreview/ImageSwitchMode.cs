namespace AtomUI.Desktop.Controls;

/// <summary>
/// Chooses what the preview window shows while it switches to another image.
/// </summary>
public enum ImageSwitchMode
{
    /// <summary>
    /// Clears the displayed image at once after the switch and shows the loading state. This is the default.
    /// </summary>
    Immediate = 0,

    /// <summary>
    /// Keeps the previously loaded image displayed until the target image has loaded, then switches to it at once.
    /// </summary>
    WaitForLoaded = 1,
}

