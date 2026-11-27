using System.Globalization;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Layout;
using Avalonia.Media;

namespace AtomUI.Controls.Converters;

/// <summary>
/// Value converter that creates a <see cref="TextBlock" /> for a string, with the layout settings of this
/// converter. Themes bind it to the content of a presenter so that string content is shown as text.
/// </summary>
public class StringToTextBlockConverter : IValueConverter
{
    /// <summary>
    /// Horizontal alignment of the created text block.
    /// </summary>
    public HorizontalAlignment HorizontalAlignment { get; set; } = HorizontalAlignment.Stretch;
    /// <summary>
    /// Vertical alignment of the created text block.
    /// </summary>
    public VerticalAlignment VerticalAlignment { get; set; } = VerticalAlignment.Stretch;
    /// <summary>
    /// Text wrapping of the created text block.
    /// </summary>
    public TextWrapping TextWrapping { get; set; } =  TextWrapping.NoWrap;
    
    /// <summary>
    /// Width of the created text block. <c>NaN</c>, the default, sets no explicit width.
    /// </summary>
    public double Width { get; set; } = double.NaN;
    /// <summary>
    /// Height of the created text block. <c>NaN</c>, the default, sets no explicit height.
    /// </summary>
    public double Height { get; set; } = double.NaN;
    /// <summary>
    /// Line height of the created text block. The value is set locally, so <c>NaN</c>, the default, also replaces
    /// a line height that the text block would inherit from its host.
    /// </summary>
    public double LineHeight { get; set; } = double.NaN;
    
    /// <summary>
    /// Creates a <see cref="TextBlock" /> that shows <paramref name="value" /> with the layout settings of this
    /// converter.
    /// </summary>
    /// <param name="value">
    /// The text to show. A value of any other type, including <see langword="null" />, is returned unchanged.
    /// </param>
    /// <param name="targetType">
    /// Not used.
    /// </param>
    /// <param name="parameter">
    /// Not used.
    /// </param>
    /// <param name="culture">
    /// Not used.
    /// </param>
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string str)
        {
            return new TextBlock
            {
                Text              = str,
                VerticalAlignment = VerticalAlignment,
                HorizontalAlignment = HorizontalAlignment,
                Height = Height,
                Width = Width,
                TextWrapping = TextWrapping,
                LineHeight = LineHeight,
            };
        }
        return value;
    }

    /// <summary>
    /// Not supported.
    /// </summary>
    /// <exception cref="NotImplementedException">
    /// Thrown on every call.
    /// </exception>
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

internal class StringToSelectableTextBlockConverter : IValueConverter
{
    public HorizontalAlignment HorizontalAlignment { get; set; } = HorizontalAlignment.Stretch;
    public VerticalAlignment VerticalAlignment { get; set; } = VerticalAlignment.Stretch;
    
    public double Width { get; set; } = double.NaN;
    public double Height { get; set; } = double.NaN;
    public double LineHeight { get; set; } = double.NaN;
    
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string str)
        {
            var textBlock = new SelectableTextBlock
            {
                Text                = str,
                VerticalAlignment   = VerticalAlignment,
                HorizontalAlignment = HorizontalAlignment,
                Height              = Height,
                Width               = Width
            };
            // LineHeight is inherited from the hosting presenter; a local NaN would block the presenter's theme value.
            if (!double.IsNaN(LineHeight))
            {
                textBlock.LineHeight = LineHeight;
            }
            return textBlock;
        }
        return value;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}