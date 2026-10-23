using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;

namespace AtomUI.Desktop.Controls;

internal sealed class OtpLineEditThicknessConverter : IMultiValueConverter
{
    public static OtpLineEditThicknessConverter Left { get; } = new(true);
    public static OtpLineEditThicknessConverter Bottom { get; } = new(false);

    private readonly bool _useLeft;

    private OtpLineEditThicknessConverter(bool useLeft)
    {
        _useLeft = useLeft;
    }

    public object Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Count != 1 || values[0] is not Thickness thickness)
        {
            return AvaloniaProperty.UnsetValue;
        }

        return _useLeft
            ? new Thickness(thickness.Left, 0, 0, 0)
            : new Thickness(0, 0, 0, thickness.Bottom);
    }
}
