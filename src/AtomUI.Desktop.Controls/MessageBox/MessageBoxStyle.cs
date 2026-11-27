namespace AtomUI.Desktop.Controls;

/// <summary>
/// Selects the status icon and the standard buttons that a <see cref="MessageBox" /> shows.
/// </summary>
public enum MessageBoxStyle
{
    /// <summary>
    /// Shows no icon and only an OK button.
    /// </summary>
    Normal,
    /// <summary>
    /// Shows an exclamation-circle icon with an OK button and a Cancel button. The Escape key activates Cancel.
    /// </summary>
    Confirm,
    /// <summary>
    /// Shows an information icon and only an OK button.
    /// </summary>
    Information,
    /// <summary>
    /// Shows a check-circle icon and only an OK button.
    /// </summary>
    Success,
    /// <summary>
    /// Shows an exclamation-circle icon and only an OK button.
    /// </summary>
    Warning,
    /// <summary>
    /// Shows a close-circle icon and only an OK button.
    /// </summary>
    Error
}