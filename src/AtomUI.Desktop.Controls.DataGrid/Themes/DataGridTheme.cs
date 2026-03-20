using AtomUI.Controls.Converters;
using AtomUI.Desktop.Controls.Converters;
using Avalonia.Styling;

namespace AtomUI.Desktop.Controls.Themes;

public class DataGridTheme : ControlTheme
{
    public static readonly CornerRadiusFilterConverter HeaderCornerRadiusConverter = new()
    {
        BottomLeft = false,
        BottomRight = false,
    };
    public static readonly DataGridUniformBorderThicknessToScalarConverter UniformBorderThicknessToScalarConverter = new();

    public static readonly BorderThicknessFilterConverter TitleSeparatorThicknessConverter = new()
    {
        Left = false,
        Top = false,
        Right = false,
        Bottom = true,
    };

    public static readonly DataGridPaginationVisibilityConvertor TopPaginationVisibilityConvertor = new()
    {
        IsTop = true
    };
    public static readonly DataGridPaginationVisibilityConvertor BottomPaginationVisibilityConvertor = new()
    {
        IsTop = false
    };
}