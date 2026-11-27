using AtomUI.Controls;
using AtomUI.Theme;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

// A Token-owner transparent part: a CardGridItem lives in the grid content of a Card and has no Own Tokens, so its theme
// reads the Card TokenResource and its Token-declared transitions resolve to the Card; Controls[Card] reaches the items.
/// <summary>
/// One cell of the grid content of a <see cref="Card" />, used directly as an item of
/// <see cref="CardGridContent" /> or created by it as the container of an item. The cell shows its content and
/// takes its place in the grid from <see cref="Row" />, <see cref="Column" />, <see cref="RowSpan" /> and
/// <see cref="ColumnSpan" />.
/// </summary>
[TokenOwnerTransparent]
public class CardGridItem : ContentControl
{
    /// <summary>
    /// Identifies the <see cref="BoxShadow" /> property.
    /// </summary>
    public static readonly StyledProperty<BoxShadows> BoxShadowProperty = 
        Border.BoxShadowProperty.AddOwner<CardGridItem>();
    
    /// <summary>
    /// Identifies the <see cref="Row" /> property.
    /// </summary>
    public static readonly StyledProperty<int> RowProperty = 
        AvaloniaProperty.Register<CardGridItem, int>(nameof (Row));
    
    /// <summary>
    /// Identifies the <see cref="Column" /> property.
    /// </summary>
    public static readonly StyledProperty<int> ColumnProperty = 
        AvaloniaProperty.Register<CardGridItem, int>(nameof (Column));
    
    /// <summary>
    /// Identifies the <see cref="RowSpan" /> property.
    /// </summary>
    public static readonly StyledProperty<int> RowSpanProperty = 
        AvaloniaProperty.Register<CardGridItem, int>(nameof (RowSpan));
    
    /// <summary>
    /// Identifies the <see cref="ColumnSpan" /> property.
    /// </summary>
    public static readonly StyledProperty<int> ColumnSpanProperty = 
        AvaloniaProperty.Register<CardGridItem, int>(nameof (ColumnSpan));
    
    /// <summary>
    /// Identifies the <see cref="IsHoverable" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsHoverableProperty = 
        AvaloniaProperty.Register<CardGridItem, bool>(nameof (IsHoverable), true);
    
    /// <summary>
    /// Resting shadow of the frame around the cell content. The theme sets it to the
    /// <c>CardGridItemShadows</c> token of the card; clearing a local value returns to that token. A change fades
    /// while motion is enabled. The shadow shown while the pointer is over the cell is not affected.
    /// </summary>
    public BoxShadows BoxShadow
    {
        get => GetValue(BoxShadowProperty);
        set => SetValue(BoxShadowProperty, value);
    }
    
    /// <summary>
    /// Zero-based index of the grid row that holds the item. Use it instead of the <c>Grid.Row</c> attached
    /// property: every change of the value overwrites <c>Grid.Row</c> on this item.
    /// </summary>
    public int Row
    {
        get => GetValue(RowProperty);
        set => SetValue(RowProperty, value);
    }
    
    /// <summary>
    /// Zero-based index of the grid column that holds the item. Use it instead of the <c>Grid.Column</c> attached
    /// property: every change of the value overwrites <c>Grid.Column</c> on this item.
    /// </summary>
    public int Column
    {
        get => GetValue(ColumnProperty);
        set => SetValue(ColumnProperty, value);
    }
    
    /// <summary>
    /// Number of grid rows the item fills; unset, the item fills one row. Every change of the value overwrites
    /// <c>Grid.RowSpan</c> on this item.
    /// </summary>
    public int RowSpan
    {
        get => GetValue(RowSpanProperty);
        set => SetValue(RowSpanProperty, value);
    }
    
    /// <summary>
    /// Number of grid columns the item fills; unset, the item fills one column. Every change of the value overwrites
    /// <c>Grid.ColumnSpan</c> on this item.
    /// </summary>
    public int ColumnSpan
    {
        get => GetValue(ColumnSpanProperty);
        set => SetValue(ColumnSpanProperty, value);
    }
    
    /// <summary>
    /// Whether the cell highlights itself while the pointer is over it: a dashed frame with the grid hover shadow
    /// and a light fill appears. The default is <see langword="true" />. Set it on the item; the
    /// <see cref="CardGridContent.IsHoverable" /> property is not read by any theme.
    /// </summary>
    public bool IsHoverable
    {
        get => GetValue(IsHoverableProperty);
        set => SetValue(IsHoverableProperty, value);
    }
    
    #region 内部属性定义

    /// <summary>
    /// Identifies the <see cref="SizeType" /> property.
    /// </summary>
    public static readonly StyledProperty<SizeType> SizeTypeProperty = 
        SizeTypeControlProperty.SizeTypeProperty.AddOwner<CardGridItem>();
    
    internal static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<CardGridItem>();

    internal static readonly DirectProperty<CardGridItem, RectangleGeometry?> GridHoverClipProperty =
        AvaloniaProperty.RegisterDirect<CardGridItem, RectangleGeometry?>(nameof(GridHoverClip), item => item.GridHoverClip);

    private RectangleGeometry? _gridHoverClip;

    internal RectangleGeometry? GridHoverClip => _gridHoverClip;
    
    /// <summary>
    /// Size step of the cell; selects the padding around its content to match the body of a card of that size.
    /// A <see cref="CardGridContent" /> binds its own <see cref="CardGridContent.SizeType" /> to every item it
    /// shows, so set it on the grid content or on the <see cref="Card" /> that hosts it. Unset, the value is
    /// <c>Middle</c>.
    /// </summary>
    public SizeType SizeType
    {
        get => GetValue(SizeTypeProperty);
        set => SetValue(SizeTypeProperty, value);
    }
    
    internal bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }

    #endregion

    internal void SetGridHoverClip(Rect? bounds)
    {
        if (bounds is null)
        {
            SetAndRaise(GridHoverClipProperty, ref _gridHoverClip, null);
        }
        else if (_gridHoverClip is { } clip)
        {
            clip.Rect = bounds.Value;
        }
        else
        {
            SetAndRaise(GridHoverClipProperty, ref _gridHoverClip, new RectangleGeometry(bounds.Value));
        }
    }

    /// <summary>
    /// Called when a styled property of the item changes. After the base call it writes a changed
    /// <see cref="Row" />, <see cref="Column" />, <see cref="RowSpan" /> or <see cref="ColumnSpan" /> to the matching
    /// <c>Grid</c> attached property of the item. An override must call the base member so that the grid position
    /// keeps following these properties.
    /// </summary>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == RowProperty)
        {
            Grid.SetRow(this, Row);
        }
        else if (change.Property == ColumnProperty)
        {
            Grid.SetColumn(this, Column);
        }
        else if (change.Property == ColumnSpanProperty)
        {
            Grid.SetColumnSpan(this, ColumnSpan);
        }
        else if (change.Property == RowSpanProperty)
        {
            Grid.SetRowSpan(this, RowSpan);
        }
    }

}
