using System.Globalization;
using Avalonia.Data.Converters;

namespace AtomUI.Toolkits.GalleryBase.Controls;

/// <summary>
/// Converts the content height bound a SemanticPartPreview inherits from the
/// showcase host into the max height its parts pane may occupy: the bound minus
/// the fixed vertical insets of PART_InspectionPanel (20 top margin + 32 bottom
/// margin + 1x2 border thickness). When the host does not bound the content
/// height (the Examples tab or a plain GalleryStickyTabsHost), the default pane
/// cap applies instead.
/// </summary>
internal sealed class SemanticPartPaneMaxHeightConverter : IValueConverter
{
    public const double InspectionPanelVerticalInsets = 54;

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is double bound && double.IsFinite(bound)
            ? Math.Max(0, bound - InspectionPanelVerticalInsets)
            : SemanticPartPreviewLayoutPanel.PaneMaxHeightProperty.GetDefaultValue(
                typeof(SemanticPartPreviewLayoutPanel));
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return Avalonia.Data.BindingOperations.DoNothing;
    }
}
