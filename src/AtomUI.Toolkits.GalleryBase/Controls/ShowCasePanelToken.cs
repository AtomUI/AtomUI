using AtomUI.Theme.DesignTokens;
using Avalonia;

namespace AtomUI.Toolkits.GalleryBase.Controls;

[ControlDesignToken]
internal sealed partial class ShowCasePanelToken : AbstractControlDesignToken
{

    /// <summary>
    /// Margin around the showcase items inside the panel.
    /// </summary>
    public partial Thickness ContentMargin { get; set; }
    /// <summary>
    /// Smallest width of a showcase item in the panel. The panel uses fewer columns than
    /// <see cref="MaxColumns" /> when the available width cannot fit that many columns of items of this width.
    /// </summary>
    public partial double MinItemWidth { get; set; }
    /// <summary>
    /// Largest number of columns the panel lays out its showcase items in.
    /// </summary>
    public partial int MaxColumns { get; set; }
    /// <summary>
    /// Horizontal gap between adjacent columns of showcase items in the panel.
    /// </summary>
    public partial double ColumnGap { get; set; }
    /// <summary>
    /// Vertical gap between the showcase items that are stacked in one column of the panel.
    /// </summary>
    public partial double RowGap { get; set; }

}
