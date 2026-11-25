using AtomUI.Controls.Commons;
using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Theme.Resources;

namespace AtomUI.Desktop.Controls;

internal class RibbonBadgeAdorner : AbstractRibbonBadgeAdorner
{
    private static readonly object[] s_geometryKeys =
    [
        RibbonBadgeTokenKind.Offset,
        RibbonBadgeTokenKind.CornerTransform,
        RibbonBadgeTokenKind.CornerDarkenAmount
    ];

    private protected override OwnerTokenSet CreateRibbonGeometryTokenSet(Action changed)
    {
        var set = new OwnerTokenSet(this, s_geometryKeys.Length, changed);
        for (var index = 0; index < s_geometryKeys.Length; ++index)
        {
            set.SetResourceKey(index, s_geometryKeys[index]);
        }
        return set;
    }
}
