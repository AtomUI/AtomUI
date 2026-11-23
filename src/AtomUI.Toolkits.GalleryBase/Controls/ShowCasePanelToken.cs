using AtomUI.Theme.DesignTokens;
using Avalonia;

namespace AtomUI.Toolkits.GalleryBase.Controls;

[ControlDesignToken]
internal sealed partial class ShowCasePanelToken : AbstractControlDesignToken
{

    public partial Thickness ContentMargin { get; set; }
    public partial double MinItemWidth { get; set; }
    public partial int MaxColumns { get; set; }
    public partial double ColumnGap { get; set; }
    public partial double RowGap { get; set; }

}
