using AtomUI.Controls;
using Avalonia;
using Avalonia.Collections;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// One entry of a <see cref="Descriptions" />: a label with its content. Put the entries into
/// <see cref="Descriptions.Items" />; the viewer creates the visual for each entry.
/// </summary>
[GenerateScopedResourceHost]
public partial class DescriptionItem : AvaloniaObject
{
    #region 公共属性定义

    /// <summary>
    /// Identifies the <see cref="Label" /> property.
    /// </summary>
    public static readonly DirectProperty<DescriptionItem, string> LabelProperty =
        AvaloniaProperty.RegisterDirect<DescriptionItem, string>(
            nameof(Label),
            o => o.Label,
            (o, v) => o.Label = v);

    /// <summary>
    /// Identifies the <see cref="Content" /> property.
    /// </summary>
    public static readonly DirectProperty<DescriptionItem, object?> ContentProperty =
        AvaloniaProperty.RegisterDirect<DescriptionItem, object?>(
            nameof(Content),
            o => o.Content,
            (o, v) => o.Content = v);

    /// <summary>
    /// Identifies the <see cref="IsFilled" /> property.
    /// </summary>
    public static readonly DirectProperty<DescriptionItem, bool> IsFilledProperty =
        AvaloniaProperty.RegisterDirect<DescriptionItem, bool>(
            nameof(IsFilled),
            o => o.IsFilled,
            (o, v) => o.IsFilled = v);

    /// <summary>
    /// Identifies the <see cref="Span" /> property.
    /// </summary>
    public static readonly DirectProperty<DescriptionItem, ResponsiveInt> SpanProperty =
        AvaloniaProperty.RegisterDirect<DescriptionItem, ResponsiveInt>(
            nameof(Span),
            o => o.Span,
            (o, v) => o.Span = v);

    private string _label = string.Empty;

    /// <summary>
    /// The text shown as the label of the entry. The default is an empty string.
    /// </summary>
    public string Label
    {
        get => _label;
        set => SetAndRaise(LabelProperty, ref _label, value);
    }

    private object? _content;

    /// <summary>
    /// The content shown with the label. The default is <see langword="null" />, which leaves the content
    /// area empty.
    /// </summary>
    public object? Content
    {
        get => _content;
        set => SetAndRaise(ContentProperty, ref _content, value);
    }

    private bool _isFilled;

    /// <summary>
    /// <see langword="true" /> to let the entry take all the columns that remain in its row, whatever
    /// <see cref="Span" /> is. The last entry of the list always fills its row.
    /// </summary>
    public bool IsFilled
    {
        get => _isFilled;
        set => SetAndRaise(IsFilledProperty, ref _isFilled, value);
    }

    private ResponsiveInt _span = new(1);

    /// <summary>
    /// The number of columns the entry spans, as a <see cref="ResponsiveInt" /> that may differ per breakpoint.
    /// The span is cut to the columns that remain in the row, and a value below 1 counts as 1. The default is 1.
    /// </summary>
    public ResponsiveInt Span
    {
        get => _span;
        set => SetAndRaise(SpanProperty, ref _span, value);
    }

    #endregion
}

/// <summary>
/// The collection of <see cref="DescriptionItem" /> entries that a <see cref="Descriptions" /> shows.
/// It is the type of the <see cref="Descriptions.Items" /> property.
/// </summary>
public class DescriptionItems : AvaloniaList<DescriptionItem> {}
