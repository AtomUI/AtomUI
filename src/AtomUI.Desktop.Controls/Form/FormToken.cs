using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// The design token of the form labels and of the spacing around a form item: label color, font size and marks,
/// the colon margin, the item spacing and the padding of a label in vertical layout. Each design language fills
/// the values in its own token rule.
/// </summary>
[ControlDesignToken]
public sealed partial class FormToken : AbstractControlDesignToken
{
    
    /// <summary>
    /// Foreground of the asterisk that marks the label of a required item. The other kinds of required mark do
    /// not use it.
    /// </summary>
    public partial Color LabelRequiredMarkColor { get; set; }
    
    /// <summary>
    /// Foreground of the label text of a form item.
    /// </summary>
    public partial Color LabelColor { get; set; }
    
    /// <summary>
    /// Font size of the label text of a form item.
    /// </summary>
    public partial double LabelFontSize { get; set; }
    
    /// <summary>
    /// Margin of the colon that ends the label. Its left and right values are the gaps to the label content and to
    /// the input. The colon is shown only while <c>IsColonVisible</c> is true.
    /// </summary>
    public partial Thickness LabelColonMargin { get; set; }
    
    /// <summary>
    /// Height reserved under the input of an item for its help text and validation messages. While the item has
    /// none of them, it is the gap between the input and the collapsed message row. Otherwise it is the minimum
    /// height of that row.
    /// </summary>
    public partial double FormItemSpacing { get; set; }
    
    /// <summary>
    /// Gap between the items of a form in inline layout, through <c>FormLayoutSpacing</c> of the form.
    /// </summary>
    public partial double InlineItemSpacing { get; set; }
    
    /// <summary>
    /// Padding around the label content of an item in vertical layout (<c>Layout</c> is Vertical). It separates
    /// the label from the input below it. Horizontal layouts do not use it.
    /// </summary>
    public partial Thickness VerticalLabelPadding { get; set; }
    
    /// <summary>
    /// Outer margin of the label row of an item in vertical layout (<c>Layout</c> is Vertical), outside
    /// <see cref="VerticalLabelPadding" />. Horizontal layouts do not use it.
    /// </summary>
    public partial Thickness VerticalLabelMargin { get; set; }
    
}
