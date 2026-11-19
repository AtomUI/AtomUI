using AtomUI.Controls.Commons;
using AtomUI.Desktop.Controls.DesignTokens;

namespace AtomUI.Desktop.Controls;

public partial class DashboardProgress : AbstractGeneralDashboardProgress
{
    private static readonly (object Text, object Icon) s_extraInfoSizeTokenResourceKeys =
        (DashboardProgressTokenKind.CircleMinimumTextFontSize, DashboardProgressTokenKind.CircleMinimumIconSize);

    private protected override (object Text, object Icon)? ExtraInfoSizeTokenResourceKeys => s_extraInfoSizeTokenResourceKeys;
}
