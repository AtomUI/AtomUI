using AtomUI.Controls.Commons;
using AtomUI.Desktop.Controls.DesignTokens;

namespace AtomUI.Desktop.Controls;

public partial class CircleProgress : AbstractGeneralCircleProgress
{
    private static readonly (object Text, object Icon) s_extraInfoSizeTokenResourceKeys =
        (CircleProgressTokenKind.CircleMinimumTextFontSize, CircleProgressTokenKind.CircleMinimumIconSize);

    private protected override (object Text, object Icon)? ExtraInfoSizeTokenResourceKeys => s_extraInfoSizeTokenResourceKeys;
}
