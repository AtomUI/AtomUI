using AtomUI.Theme;
using AtomUI.Controls.Commons;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// The loading indicator of the <see cref="Spin" /> control: four dots that fade in one after another, or
/// the custom indicator set with <c>CustomIndicator</c>, which replaces the dots. It has no tokens of its
/// own; its theme reads the tokens of the owner.
/// </summary>
[TokenOwnerTransparent]
public partial class SpinIndicator : AbstractSpinIndicator
{
    public SpinIndicator()
    {
    }
}
