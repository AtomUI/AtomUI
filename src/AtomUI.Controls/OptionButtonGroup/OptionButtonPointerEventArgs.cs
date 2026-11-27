using AtomUI.Controls.Commons;

namespace AtomUI.Controls;

/// <summary>
/// Describes the pointer state of an option button when it reports <c>OptionButtonPointerEvent</c>.
/// </summary>
public class OptionButtonPointerEventArgs : EventArgs
{
    /// <summary>
    /// The option button that reported the pointer state.
    /// </summary>
    public AbstractOptionButton? Button { get; }
    /// <summary>
    /// <see langword="true" /> only in the report of a pointer press on the button.
    /// </summary>
    public bool IsPressed { get; set; }
    /// <summary>
    /// <see langword="true" /> while the pointer is over the button.
    /// </summary>
    public bool IsHovering { get; set; }

    /// <summary>
    /// Creates the arguments for <paramref name="button" />; <see cref="IsPressed" /> and
    /// <see cref="IsHovering" /> stay <see langword="false" /> until they are set.
    /// </summary>
    /// <param name="button">
    /// The option button that reports the pointer state.
    /// </param>
    public OptionButtonPointerEventArgs(AbstractOptionButton button)
    {
        Button = button;
    }
}