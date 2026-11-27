using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.VisualTree;

namespace AtomUI.Controls;

/// <summary>
/// A column of a <see cref="Row" />. Its properties decide how much of the 24 columns of the row the content of
/// the column occupies at each breakpoint.
/// </summary>
/// <remarks>
/// The properties take effect only when the column is a child of a <see cref="Row" />. Every change of them makes
/// the row lay out its children again.
/// </remarks>
public class Col : ContentControl
{
    /// <summary>
    /// Identifies the <see cref="Span" /> property.
    /// </summary>
    public static readonly StyledProperty<GridColSpanInfo> SpanProperty =
        AvaloniaProperty.Register<Col, GridColSpanInfo>(nameof(Span));

    /// <summary>
    /// Identifies the <see cref="Flex" /> property.
    /// </summary>
    public static readonly StyledProperty<GridColFlex?> FlexProperty =
        AvaloniaProperty.Register<Col, GridColFlex?>(nameof(Flex));

    /// <summary>
    /// Identifies the <see cref="Offset" /> property.
    /// </summary>
    public static readonly StyledProperty<int> OffsetProperty =
        AvaloniaProperty.Register<Col, int>(nameof(Offset), validate: v => v >= 0 && v <= 24);

    /// <summary>
    /// Identifies the <see cref="Order" /> property.
    /// </summary>
    public static readonly StyledProperty<int> OrderProperty =
        AvaloniaProperty.Register<Col, int>(nameof(Order));

    /// <summary>
    /// Identifies the <see cref="Push" /> property.
    /// </summary>
    public static readonly StyledProperty<int> PushProperty =
        AvaloniaProperty.Register<Col, int>(nameof(Push), validate: v => v >= 0 && v <= 24);

    /// <summary>
    /// Identifies the <see cref="Pull" /> property.
    /// </summary>
    public static readonly StyledProperty<int> PullProperty =
        AvaloniaProperty.Register<Col, int>(nameof(Pull), validate: v => v >= 0 && v <= 24);

    /// <summary>
    /// Identifies the <see cref="Xs" /> property.
    /// </summary>
    public static readonly StyledProperty<GridColSize?> XsProperty =
        AvaloniaProperty.Register<Col, GridColSize?>(nameof(Xs));

    /// <summary>
    /// Identifies the <see cref="Sm" /> property.
    /// </summary>
    public static readonly StyledProperty<GridColSize?> SmProperty =
        AvaloniaProperty.Register<Col, GridColSize?>(nameof(Sm));

    /// <summary>
    /// Identifies the <see cref="Md" /> property.
    /// </summary>
    public static readonly StyledProperty<GridColSize?> MdProperty =
        AvaloniaProperty.Register<Col, GridColSize?>(nameof(Md));

    /// <summary>
    /// Identifies the <see cref="Lg" /> property.
    /// </summary>
    public static readonly StyledProperty<GridColSize?> LgProperty =
        AvaloniaProperty.Register<Col, GridColSize?>(nameof(Lg));

    /// <summary>
    /// Identifies the <see cref="Xl" /> property.
    /// </summary>
    public static readonly StyledProperty<GridColSize?> XlProperty =
        AvaloniaProperty.Register<Col, GridColSize?>(nameof(Xl));

    /// <summary>
    /// Identifies the <see cref="Xxl" /> property.
    /// </summary>
    public static readonly StyledProperty<GridColSize?> XxlProperty =
        AvaloniaProperty.Register<Col, GridColSize?>(nameof(Xxl));

    /// <summary>
    /// Identifies the <see cref="Xxxl" /> property.
    /// </summary>
    public static readonly StyledProperty<GridColSize?> XxxlProperty =
        AvaloniaProperty.Register<Col, GridColSize?>(nameof(Xxxl));

    /// <summary>
    /// Gets or sets how many of the 24 columns of the row this column occupies, per breakpoint.
    /// </summary>
    /// <remarks>
    /// A single number applies to every breakpoint. A responsive text such as <c>xs: 24, sm: 12</c> gives every
    /// breakpoint the number of the nearest named breakpoint at or below it, or <c>0</c> when there is none.
    /// A span of <c>0</c> hides the column at that breakpoint. When no span applies, the column is sized by its
    /// content.
    /// </remarks>
    public GridColSpanInfo Span
    {
        get => GetValue(SpanProperty);
        set => SetValue(SpanProperty, value);
    }

    /// <summary>
    /// Gets or sets how this column shares the free width or the overflow of its line, or <see langword="null" /> to
    /// size the column by <see cref="Span" /> or by its content.
    /// </summary>
    /// <remarks>
    /// Free width is given to the columns of the line in proportion to <see cref="GridColFlex.Grow" />, and an
    /// overflow is taken from them in proportion to <see cref="GridColFlex.Shrink" /> multiplied by their starting
    /// width. <see cref="GridColFlex.Basis" /> replaces the starting width.
    /// Nothing is shared while the row is measured with an infinite width.
    /// </remarks>
    public GridColFlex? Flex
    {
        get => GetValue(FlexProperty);
        set => SetValue(FlexProperty, value);
    }

    /// <summary>
    /// Gets or sets the number of empty columns to the left of this column. The value must be between 0 and 24.
    /// </summary>
    /// <remarks>
    /// The empty space counts as part of the column when the row decides whether the line is full.
    /// </remarks>
    public int Offset
    {
        get => GetValue(OffsetProperty);
        set => SetValue(OffsetProperty, value);
    }

    /// <summary>
    /// Gets or sets the position of this column among the children of the row. Columns with a lower order come first,
    /// and columns with the same order keep the order in which they were added.
    /// </summary>
    /// <remarks>
    /// Negative values are allowed. Only the layout follows the order; the children collection itself is not changed.
    /// </remarks>
    public int Order
    {
        get => GetValue(OrderProperty);
        set => SetValue(OrderProperty, value);
    }

    /// <summary>
    /// Gets or sets how many columns this column is moved to the right, without moving the other columns.
    /// The value must be between 0 and 24.
    /// </summary>
    /// <remarks>
    /// The move is the difference between <see cref="Push" /> and <see cref="Pull" />, so the column may overlap a
    /// neighbor. It is skipped while the row is measured with an infinite width.
    /// </remarks>
    public int Push
    {
        get => GetValue(PushProperty);
        set => SetValue(PushProperty, value);
    }

    /// <summary>
    /// Gets or sets how many columns this column is moved to the left, without moving the other columns.
    /// The value must be between 0 and 24.
    /// </summary>
    /// <remarks>
    /// The move is the difference between <see cref="Push" /> and <see cref="Pull" />, so the column may overlap a
    /// neighbor. It is skipped while the row is measured with an infinite width.
    /// </remarks>
    public int Pull
    {
        get => GetValue(PullProperty);
        set => SetValue(PullProperty, value);
    }

    /// <summary>
    /// Gets or sets the settings that replace the values of the column at every breakpoint, or <see langword="null" />
    /// to keep them.
    /// </summary>
    /// <remarks>
    /// Only settings that have a value in the <see cref="GridColSize" /> replace the value of the column:
    /// <see cref="Span" />, <see cref="Flex" />, <see cref="Offset" />, <see cref="Order" />,
    /// <see cref="Push" /> and <see cref="Pull" />.
    /// </remarks>
    public GridColSize? Xs
    {
        get => GetValue(XsProperty);
        set => SetValue(XsProperty, value);
    }

    /// <summary>
    /// Gets or sets the settings that replace the values of the column from the small breakpoint up, or
    /// <see langword="null" /> to keep them.
    /// </summary>
    /// <remarks>
    /// Only settings that have a value in the <see cref="GridColSize" /> replace the values of the column. When
    /// several breakpoints apply, the largest one wins for the settings it defines.
    /// </remarks>
    public GridColSize? Sm
    {
        get => GetValue(SmProperty);
        set => SetValue(SmProperty, value);
    }

    /// <summary>
    /// Gets or sets the settings that replace the values of the column from the medium breakpoint up, or
    /// <see langword="null" /> to keep them.
    /// </summary>
    /// <remarks>
    /// Only settings that have a value in the <see cref="GridColSize" /> replace the values of the column. When
    /// several breakpoints apply, the largest one wins for the settings it defines.
    /// </remarks>
    public GridColSize? Md
    {
        get => GetValue(MdProperty);
        set => SetValue(MdProperty, value);
    }

    /// <summary>
    /// Gets or sets the settings that replace the values of the column from the large breakpoint up, or
    /// <see langword="null" /> to keep them.
    /// </summary>
    /// <remarks>
    /// Only settings that have a value in the <see cref="GridColSize" /> replace the values of the column. When
    /// several breakpoints apply, the largest one wins for the settings it defines.
    /// </remarks>
    public GridColSize? Lg
    {
        get => GetValue(LgProperty);
        set => SetValue(LgProperty, value);
    }

    /// <summary>
    /// Gets or sets the settings that replace the values of the column from the extra-large breakpoint up, or
    /// <see langword="null" /> to keep them.
    /// </summary>
    /// <remarks>
    /// Only settings that have a value in the <see cref="GridColSize" /> replace the values of the column. When
    /// several breakpoints apply, the largest one wins for the settings it defines.
    /// </remarks>
    public GridColSize? Xl
    {
        get => GetValue(XlProperty);
        set => SetValue(XlProperty, value);
    }

    /// <summary>
    /// Gets or sets the settings that replace the values of the column from the extra-extra-large breakpoint up, or
    /// <see langword="null" /> to keep them.
    /// </summary>
    /// <remarks>
    /// Only settings that have a value in the <see cref="GridColSize" /> replace the values of the column. When
    /// several breakpoints apply, the largest one wins for the settings it defines.
    /// </remarks>
    public GridColSize? Xxl
    {
        get => GetValue(XxlProperty);
        set => SetValue(XxlProperty, value);
    }

    /// <summary>
    /// Gets or sets the settings that replace the values of the column from the extra-extra-extra-large breakpoint
    /// up, or <see langword="null" /> to keep them.
    /// </summary>
    /// <remarks>
    /// Only settings that have a value in the <see cref="GridColSize" /> replace the values of the column. When
    /// several breakpoints apply, the largest one wins for the settings it defines.
    /// </remarks>
    public GridColSize? Xxxl
    {
        get => GetValue(XxxlProperty);
        set => SetValue(XxxlProperty, value);
    }

    static Col()
    {
        AffectsMeasure<Col>(
            SpanProperty,
            FlexProperty,
            OffsetProperty,
            OrderProperty,
            PushProperty,
            PullProperty,
            XsProperty,
            SmProperty,
            MdProperty,
            LgProperty,
            XlProperty,
            XxlProperty,
            XxxlProperty);
    }

    /// <summary>
    /// Calls the base implementation and then asks the parent to lay out again when one of the grid properties of
    /// this column changed.
    /// </summary>
    /// <remarks>
    /// A parent <see cref="Row" /> drops its cached child order and measures again. A parent of any other type is
    /// only asked to measure again.
    /// </remarks>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == SpanProperty ||
            change.Property == FlexProperty ||
            change.Property == OffsetProperty ||
            change.Property == OrderProperty ||
            change.Property == PushProperty ||
            change.Property == PullProperty ||
            change.Property == XsProperty ||
            change.Property == SmProperty ||
            change.Property == MdProperty ||
            change.Property == LgProperty ||
            change.Property == XlProperty ||
            change.Property == XxlProperty ||
            change.Property == XxxlProperty)
        {
            if (this.GetVisualParent() is Row row)
            {
                row.InvalidateChildLayout();
            }
            else if (this.GetVisualParent() is Layoutable parent)
            {
                parent.InvalidateMeasure();
            }
        }
    }

    internal GridColLayout ResolveLayout(MediaBreakPoint breakPoint)
    {
        var layout = new GridColLayout(Flex, Span.GetValue(breakPoint), IsSet(SpanProperty), Offset, Order, Push, Pull);

        if (Xs is not null)
        {
            layout = Xs.ApplyTo(layout);
        }

        if (breakPoint >= MediaBreakPoint.Small && Sm is not null)
        {
            layout = Sm.ApplyTo(layout);
        }

        if (breakPoint >= MediaBreakPoint.Medium && Md is not null)
        {
            layout = Md.ApplyTo(layout);
        }

        if (breakPoint >= MediaBreakPoint.Large && Lg is not null)
        {
            layout = Lg.ApplyTo(layout);
        }

        if (breakPoint >= MediaBreakPoint.ExtraLarge && Xl is not null)
        {
            layout = Xl.ApplyTo(layout);
        }

        if (breakPoint >= MediaBreakPoint.ExtraExtraLarge && Xxl is not null)
        {
            layout = Xxl.ApplyTo(layout);
        }

        if (breakPoint >= MediaBreakPoint.ExtraExtraExtraLarge && Xxxl is not null)
        {
            layout = Xxxl.ApplyTo(layout);
        }

        return layout;
    }
}
