using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

public record SliderMark(string Label, double Value)
{
    public IBrush? LabelBrush { get; set; }
    public FontStyle LabelFontStyle { get; set; } = FontStyle.Normal;
    public FontWeight LabelFontWeight { get; set; } = FontWeight.Normal;
    internal Size LabelSize { get; set; }
    internal FormattedText? FormattedText { get; set; }
}
