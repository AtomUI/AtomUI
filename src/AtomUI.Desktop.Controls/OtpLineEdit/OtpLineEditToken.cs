using AtomUI.Theme.DesignTokens;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
public sealed partial class OtpLineEditToken : AbstractControlDesignToken
{

    /// <summary>Width of a character cell at the standard size.</summary>
    public partial double CellWidth { get; set; }

    /// <summary>Width of a character cell at the large size.</summary>
    public partial double CellWidthLG { get; set; }

    /// <summary>Width of a character cell at the small size.</summary>
    public partial double CellWidthSM { get; set; }

    /// <summary>Gap between adjacent character cells.</summary>
    public partial double CellGap { get; set; }

    /// <summary>Joins adjacent cells into one frame when no separator is provided.</summary>
    public partial bool JoinCells { get; set; }

}
