using AtomUI.Controls.Commons;
using Avalonia;
using Avalonia.Controls;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Scroll viewer for desktop layouts; the templates of controls that scroll their content host it as well. Its
/// own template arranges the content, a horizontal and a vertical <c>ScrollBar</c> and the separator in the
/// corner between the two scroll bars, and the scroll bars take their settings from this control.
/// </summary>
public class ScrollViewer : AbstractScrollViewer
{
    #region 公共属性定义

    /// <summary>
    /// Identifies the <see cref="IsLiteMode" /> property.
    /// </summary>
    public static readonly AttachedProperty<bool> IsLiteModeProperty =
        AvaloniaProperty.RegisterAttached<ScrollViewer, Control, bool>(
            nameof(IsLiteMode));

    /// <summary>
    /// Selects lite mode for the scroll bars of this scroll viewer. In lite mode the content extends under the
    /// scroll bars, the scroll bars are transparent until the pointer is over the scroll viewer or a scroll bar is
    /// being dragged, and each thumb is thinner and takes the normal thickness while it is hovered or pressed. In
    /// normal mode the scroll bars reserve their own lane and are drawn opaque. The default is
    /// <see langword="false" />, and the theme of the control selects it as well.
    /// </summary>
    /// <remarks>
    /// The value is not inherited. The template of this control forwards it to its two <c>ScrollBar</c> elements
    /// and the template of a scroll bar forwards it to its thumb. Another control can carry the value as an
    /// attached property, but it reaches the scroll viewer inside that control only through a binding in the
    /// control's theme.
    /// </remarks>
    public bool IsLiteMode
    {
        get => GetValue(IsLiteModeProperty);
        set => SetValue(IsLiteModeProperty, value);
    }
    
    #endregion
    
    public ScrollViewer()
    {
    }
    
    /// <summary>
    /// Gets the value of the attached <see cref="IsLiteMode" /> property from an element.
    /// </summary>
    public static bool GetIsLiteMode(Control control)
    {
        return control.GetValue(IsLiteModeProperty);
    }
    
    /// <summary>
    /// Sets the attached <see cref="IsLiteMode" /> property on an element. On a scroll viewer, a scroll bar or a
    /// thumb the value selects the lite mode of that element. On any other control it takes effect only when the
    /// theme of that control forwards it to the scroll viewer inside.
    /// </summary>
    public static void SetIsLiteMode(Control control, bool value)
    {
        control.SetValue(IsLiteModeProperty, value);
    }
}