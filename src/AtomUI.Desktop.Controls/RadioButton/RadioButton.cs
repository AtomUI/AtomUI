using AtomUI.Controls.Commons;
using AtomUI.Desktop.Controls.DesignTokens;

namespace AtomUI.Desktop.Controls;

internal static class RadioIndicatorResources
{
    public static RadioDotGeometryTokenResourceKeys DotGeometryTokenResourceKeys { get; } =
        new(RadioButtonTokenKind.RadioSize, RadioButtonTokenKind.DotSize, RadioButtonTokenKind.DotPadding);
}

public partial class RadioButton : AbstractRadioButton
{
    public RadioButton()
    {
    }
}
