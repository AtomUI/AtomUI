using AtomUI.Controls.Converters;
using AtomUI.Desktop.Controls.Converters;
using Avalonia.Styling;

namespace AtomUI.Desktop.Controls.Themes;

/// <summary>
/// Theme of the <see cref="AtomUI.Desktop.Controls.DataGrid" />. The static fields are converter instances
/// that the DataGrid themes reference from AXAML.
/// </summary>
public class DataGridTheme : ControlTheme
{
    /// <summary>
    /// Corner radius filter for a header: it keeps the top-left and top-right corners of a radius and sets the
    /// bottom-left and bottom-right corners to 0.
    /// </summary>
    public static readonly CornerRadiusFilterConverter HeaderCornerRadiusConverter = new()
    {
        BottomLeft = false,
        BottomRight = false,
    };
    /// <summary>
    /// Converter that reduces a <c>Thickness</c> to its left edge. The DataGrid theme uses it for the height of
    /// the line between the column headers and the rows.
    /// </summary>
    public static readonly DataGridUniformBorderThicknessToScalarConverter UniformBorderThicknessToScalarConverter = new();

    /// <summary>
    /// Thickness filter that keeps only the bottom edge of the grid's <c>BorderThickness</c>. The DataGrid themes
    /// use it for the line under the title, the line between the column headers and the rows, and the title
    /// separators of the filter flyouts.
    /// </summary>
    public static readonly BorderThicknessFilterConverter TitleSeparatorThicknessConverter = new()
    {
        Left = false,
        Top = false,
        Right = false,
        Bottom = true,
    };

    /// <summary>
    /// Converter for the <c>IsVisible</c> of the pagination panel above the grid: the panel is visible when the
    /// effective pagination visibility includes <c>Top</c>.
    /// </summary>
    public static readonly DataGridPaginationVisibilityConvertor TopPaginationVisibilityConvertor = new()
    {
        IsTop = true
    };
    /// <summary>
    /// Converter for the <c>IsVisible</c> of the pagination panel below the grid: the panel is visible when the
    /// effective pagination visibility includes <c>Bottom</c>.
    /// </summary>
    public static readonly DataGridPaginationVisibilityConvertor BottomPaginationVisibilityConvertor = new()
    {
        IsTop = false
    };
}