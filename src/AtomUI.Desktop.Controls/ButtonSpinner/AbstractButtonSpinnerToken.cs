using AtomUI.Theme.DesignTokens;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// The handle tokens shared by <see cref="ButtonSpinnerToken" /> and <see cref="NumericUpDownToken" />: the width,
/// the glyph size and the colors of the increment and decrement buttons. Each design language computes them for
/// both controls in its <c>ButtonSpinnerTokenCalculator</c>.
/// </summary>
[ControlDesignToken]
public abstract partial class AbstractButtonSpinnerToken : AbstractControlDesignToken
{

    /// <summary>
    /// Width reserved for the spinner's increment and decrement buttons.
    /// </summary>

    public partial double HandleWidth { get; set; }

    /// <summary>
    /// Size of the glyph in a spinner button.
    /// </summary>
    public partial double HandleIconSize { get; set; }

    /// <summary>
    /// Resting background color behind <see cref="HandleIconColor" />, in every style except the filled style,
    /// which reads <see cref="FilledHandleBg" /> instead.
    /// </summary>
    public partial Color HandleBg { get; set; }

    /// <summary>
    /// Background color of a pressed spinner button behind <see cref="HandlePressedColor" />.
    /// </summary>
    public partial Color HandleActiveBg { get; set; }

    /// <summary>
    /// Glyph color of a hovered spinner button over <see cref="HandleHoverBg" />.
    /// </summary>
    public partial Color HandleHoverColor { get; set; }

    /// <summary>
    /// Background color of a hovered spinner button behind <see cref="HandleHoverColor" />.
    /// </summary>
    public partial Color HandleHoverBg { get; set; }

    /// <summary>
    /// Resting glyph color of a spinner button over <see cref="HandleBg" />.
    /// </summary>
    public partial Color HandleIconColor { get; set; }

    /// <summary>
    /// Glyph color of a pressed spinner button over <see cref="HandleActiveBg" />.
    /// </summary>
    public partial Color HandlePressedColor { get; set; }

    /// <summary>
    /// Color of the divider between the spinner buttons and the content, and of the line between the two buttons.
    /// </summary>
    public partial Color HandleBorderColor { get; set; }

    /// <summary>
    /// Resting background color of the spinner buttons in the filled style.
    /// </summary>
    public partial Color FilledHandleBg { get; set; }
}
