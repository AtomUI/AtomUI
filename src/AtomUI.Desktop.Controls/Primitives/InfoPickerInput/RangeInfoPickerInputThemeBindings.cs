using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;

namespace AtomUI.Desktop.Controls.Primitives;

internal static class RangeInfoPickerInputThemeBindings
{
    #region 公共属性定义

    public static readonly StyledProperty<double> FrameBottomProperty = RangeInfoPickerInput.PickerIndicatorFrameBottomProperty;

    #endregion

    public static readonly IMultiValueConverter IndicatorTop = new IndicatorTopConverter();

    private sealed class IndicatorTopConverter : IMultiValueConverter
    {
        public object Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture) =>
            values.Count == 2 && values[0] is double bottom && values[1] is double height
                ? bottom - height : AvaloniaProperty.UnsetValue;
    }
}
