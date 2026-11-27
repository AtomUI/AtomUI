// Retain the established Desktop CLR name and forward the type from Desktop.
namespace AtomUI.Desktop.Controls;

/// <summary>
/// Sets where a form item places its label relative to its content.
/// </summary>
public enum FormItemLayout
{
    /// <summary>
    /// Places the label beside the content. This is the default.
    /// </summary>
    Horizontal,
    /// <summary>
    /// Places the label above the content.
    /// </summary>
    Vertical
}
