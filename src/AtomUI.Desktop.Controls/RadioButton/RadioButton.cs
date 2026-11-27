using AtomUI.Controls.Commons;
using AtomUI.Desktop.Controls.DesignTokens;

namespace AtomUI.Desktop.Controls;

internal static class RadioIndicatorResources
{
    public static RadioDotGeometryTokenResourceKeys DotGeometryTokenResourceKeys { get; } =
        new(RadioButtonTokenKind.RadioSize, RadioButtonTokenKind.DotSize, RadioButtonTokenKind.DotPadding);
}

/// <summary>
/// Desktop radio button. It adds nothing to <see cref="AbstractRadioButton" /> and is the type that the desktop
/// theme and the semantic parts target.
/// </summary>
public partial class RadioButton : AbstractRadioButton
{
    public RadioButton()
    {
    }
}
