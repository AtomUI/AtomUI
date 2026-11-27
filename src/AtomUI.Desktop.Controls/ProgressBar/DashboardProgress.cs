using AtomUI.Controls.Commons;
using AtomUI.Desktop.Controls.DesignTokens;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// A dashboard progress indicator: an arc that leaves out a gap of <c>GapDegree</c> degrees on the side selected
/// by <c>DashboardGapPosition</c>, with the progress info in its center. Unlike the base class, it supplies the
/// Token resource keys of the minimum sizes of the centered text and icon, which the theme enables with
/// <c>UseExtraInfoSizeTokenResources</c>.
/// </summary>
public partial class DashboardProgress : AbstractGeneralDashboardProgress
{
    private static readonly (object Text, object Icon) s_extraInfoSizeTokenResourceKeys =
        (DashboardProgressTokenKind.CircleMinimumTextFontSize, DashboardProgressTokenKind.CircleMinimumIconSize);

    /// <summary>
    /// Returns the keys of the <c>CircleMinimumTextFontSize</c> and <c>CircleMinimumIconSize</c> Tokens, which give
    /// the minimum sizes of the centered text and icon while <c>UseExtraInfoSizeTokenResources</c> is
    /// <see langword="true" />.
    /// </summary>
    private protected override (object Text, object Icon)? ExtraInfoSizeTokenResourceKeys => s_extraInfoSizeTokenResourceKeys;
}
