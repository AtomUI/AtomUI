namespace AtomUI.Controls;

/// <summary>
/// The visual type of a float button. It selects the icon color and the background of the button in the normal,
/// hovered and pressed states.
/// </summary>
public enum FloatButtonType
{
    /// <summary>
    /// The standard type. The icon takes the <c>DefaultIconColor</c> token and the background keeps the base
    /// background of the theme, which becomes <c>DefaultHoverBg</c> when hovered and <c>DefaultActiveBg</c> when
    /// pressed. This is the value in effect when the type is unset.
    /// </summary>
    Default,
    /// <summary>
    /// The primary type. The background is the <c>ColorPrimary</c> token, or its hover and active variants, and
    /// the icon takes the <c>PrimaryColor</c> token.
    /// </summary>
    Primary
}

/// <summary>
/// The outline of a float button. A circle shows only an icon; a square can also show a description.
/// </summary>
public enum FloatButtonShape
{
    /// <summary>
    /// A circle sized to the float button size of the theme. It shows only an icon. This is the value in effect
    /// when the shape is unset.
    /// </summary>
    Circle,
    /// <summary>
    /// A rounded rectangle. When the button has a description, the description is shown under the icon and the
    /// button grows in height to fit it.
    /// </summary>
    Square
}

/// <summary>
/// The edge or corner of the scoped overlay layer that a float button is anchored to.
/// </summary>
public enum FloatButtonPlacement
{
    /// <summary>
    /// The top edge of the layer. This is the value used when the placement of a button is left unset.
    /// </summary>
    Top,
    /// <summary>
    /// The bottom edge of the layer.
    /// </summary>
    Bottom,
    /// <summary>
    /// The left edge of the layer.
    /// </summary>
    Left,
    /// <summary>
    /// The right edge of the layer.
    /// </summary>
    Right,
    /// <summary>
    /// The top-left corner of the layer.
    /// </summary>
    TopLeft,
    /// <summary>
    /// The top-right corner of the layer.
    /// </summary>
    TopRight,
    /// <summary>
    /// The bottom-left corner of the layer.
    /// </summary>
    BottomLeft,
    /// <summary>
    /// The bottom-right corner of the layer.
    /// </summary>
    BottomRight,
    /// <summary>
    /// The center of the layer.
    /// </summary>
    Center
}