using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Layout;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// 占位文本元素的水平对齐：HorizontalContentAlignment 的 Stretch 映射为 Left。
/// 占位文字始终从内容对齐起点渲染，若让元素按 Stretch 撑满内容区，其边界
/// （语义部件 placeholder 高亮的度量基准）会宽于文字本身，与 Ant Design 的
/// placeholder 语义结构（内联、紧贴文字）不一致。
/// </summary>
internal class PlaceholderHorizontalAlignmentConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is HorizontalAlignment alignment)
        {
            return alignment == HorizontalAlignment.Stretch
                ? HorizontalAlignment.Left
                : alignment;
        }

        return HorizontalAlignment.Left;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
