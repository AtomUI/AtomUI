using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// A mark on the rail of a slider. Every entry of the <c>Marks</c> list of the slider is drawn as one mark
/// with its label.
/// </summary>
/// <param name="Label">
/// Text drawn below the mark on a horizontal slider and to the right of it on a vertical one.
/// </param>
/// <param name="Value">
/// Position of the mark on the rail, in the units of the slider value.
/// </param>
public record SliderMark(string Label, double Value)
{
    /// <summary>
    /// Brush of the label of this mark. <see langword="null" /> uses the mark label brush of the slider, which
    /// is also used while the slider is disabled.
    /// </summary>
    public IBrush? LabelBrush { get; set; }
    /// <summary>
    /// Font style of the label of this mark. The font family and the size come from the slider.
    /// </summary>
    public FontStyle LabelFontStyle { get; set; } = FontStyle.Normal;
    /// <summary>
    /// Font weight of the label of this mark. The font family and the size come from the slider.
    /// </summary>
    public FontWeight LabelFontWeight { get; set; } = FontWeight.Normal;
    internal Size LabelSize { get; set; }
    internal FormattedText? FormattedText { get; set; }
}
