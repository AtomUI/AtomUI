using Avalonia;

namespace AtomUI.Controls.Primitives;

/// <summary>
/// Lets a focus target whose visual shape differs from its <c>CornerRadius</c> property (for example a
/// round or circle button, or a radio indicator) tell the keyboard focus outline which radius to follow.
/// </summary>
internal interface IFocusOutlineShapeProvider
{
    CornerRadius FocusOutlineCornerRadius { get; }
}
