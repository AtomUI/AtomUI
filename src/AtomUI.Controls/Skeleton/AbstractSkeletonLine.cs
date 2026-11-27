using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace AtomUI.Controls.Commons;

/// <summary>
/// Base of a single skeleton bar, such as a title or one line of a paragraph. A template provides
/// <c>PART_RootLayout</c>, which is arranged to <see cref="LineWidth" /> inside the control.
/// </summary>
public abstract class AbstractSkeletonLine : AbstractSkeleton
{
    internal const string RootLayoutPart = "PART_RootLayout";

    #region 公共属性定义

    /// <summary>
    /// Identifies the <see cref="LineWidth" /> property.
    /// </summary>
    public static readonly StyledProperty<Dimension> LineWidthProperty =
        AvaloniaProperty.Register<AbstractSkeletonLine, Dimension>(nameof(LineWidth), new Dimension(100.0, DimensionUnitType.Percentage));

    /// <summary>
    /// Identifies the <see cref="IsRound" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsRoundProperty =
        AvaloniaProperty.Register<AbstractSkeletonLine, bool>(nameof(IsRound));

    /// <summary>
    /// Width of the bar. A pixel value is an exact width in DIPs; a percentage is taken of the width offered to the
    /// control and never exceeds it. The default fills the offered width.
    /// </summary>
    public Dimension LineWidth
    {
        get => GetValue(LineWidthProperty);
        set => SetValue(LineWidthProperty, value);
    }

    /// <summary>
    /// Selects the corner style of the bar: true gives it rounded ends, false gives it the block corner radius.
    /// </summary>
    public bool IsRound
    {
        get => GetValue(IsRoundProperty);
        set => SetValue(IsRoundProperty, value);
    }

    #endregion

    private Control? _rootLayout;

    static AbstractSkeletonLine()
    {
        AffectsMeasure<AbstractSkeletonLine>(LineWidthProperty);
    }

    /// <summary>
    /// Measures the control with <see cref="LineWidth" /> in place of the offered width when the width is a pixel
    /// value, so that the template is sized to the bar. Overrides must call the base member.
    /// </summary>
    protected override Size MeasureOverride(Size availableSize)
    {
        var actualWidth = availableSize.Width;
        if (LineWidth.IsAbsolute)
        {
            actualWidth = LineWidth.Value;
        }
        var actualSize = new Size(actualWidth, availableSize.Height);
        return base.MeasureOverride(actualSize);
    }

    /// <summary>
    /// Arranges <c>PART_RootLayout</c> to the resolved <see cref="LineWidth" />, capped at the width the control was
    /// given; the control itself keeps the full size it was given. Overrides must call the base member.
    /// </summary>
    protected override Size ArrangeOverride(Size finalSize)
    {
        var size        = base.ArrangeOverride(finalSize);
        var actualWidth = Math.Min(finalSize.Width, LineWidth.Resolve(finalSize.Width));

        if (_rootLayout != null)
        {
            _rootLayout.Arrange(new Rect(0, 0, actualWidth, finalSize.Height));
        }
        return size;
    }

    /// <summary>
    /// Locates <c>PART_RootLayout</c>, the element that is arranged to <see cref="LineWidth" />. Overrides must call
    /// the base member.
    /// </summary>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _rootLayout = e.NameScope.Find<Control>(RootLayoutPart);
    }
}
