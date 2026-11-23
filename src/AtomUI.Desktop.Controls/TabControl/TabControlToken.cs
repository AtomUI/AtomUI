using AtomUI.Theme.DesignTokens;
using Avalonia;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
public sealed partial class TabControlToken : AbstractLineTabToken
{
    /// <summary>Reserved thickness for the tab strip's overflow shadow caster.</summary>
    [ReservedToken("The overflow shadow casters sit outside the viewport; caster thickness has no visible meaning.")]
    public partial double MenuEdgeThickness { get; set; }

    /// <summary>Gap between the tab strip and the selected tab's content.</summary>
    public partial double TabAndContentGutter { get; set; }
    /// <summary>Reserved outer horizontal margin for the tab strip.</summary>
    [ReservedToken("antd Tabs horizontalMargin: no outer margin is consumed by the current tab layout.")]
    public partial Thickness HorizontalMargin { get; set; }
}
