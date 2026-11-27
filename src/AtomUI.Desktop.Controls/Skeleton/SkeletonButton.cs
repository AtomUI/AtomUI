using AtomUI.Data;
using Avalonia;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Outline of a <see cref="SkeletonButton" /> placeholder.
/// </summary>
public enum SkeletonButtonShape
{
    /// <summary>
    /// Rectangle with the small corner radius the theme applies.
    /// </summary>
    Square,
    /// <summary>
    /// Rectangle with half its height as the corner radius, so both ends are fully rounded.
    /// </summary>
    Round,
    /// <summary>
    /// Block as wide as tall with the height as its corner radius, so it renders as a circle.
    /// </summary>
    Circle
}

/// <summary>
/// Block placeholder that stands in for a button, with the outline and the width chosen by
/// <see cref="Shape" />.
/// </summary>
public partial class SkeletonButton : SkeletonElement
{
    #region 公共属性定义
    /// <summary>
    /// Identifies the <see cref="Shape" /> property.
    /// </summary>
    public static readonly StyledProperty<SkeletonButtonShape> ShapeProperty =
        AvaloniaProperty.Register<SkeletonButton, SkeletonButtonShape>(nameof(Shape));

    /// <summary>
    /// Outline of the placeholder: <c>Square</c> has the small corner radius the theme applies,
    /// <c>Round</c> has half the height as its corner radius, and <c>Circle</c> is as wide as tall with the
    /// height as its corner radius. The default is <c>Square</c>.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     <c>Square</c> and <c>Round</c> are twice as wide as tall while
    ///     <see cref="SkeletonElement.IsBlock" /> is false. <c>Circle</c> sets its width to the height whenever
    ///     <see cref="Shape" /> or the height changes, whether or not <see cref="SkeletonElement.IsBlock" /> is true.
    ///     </para>
    /// </remarks>
    public SkeletonButtonShape Shape
    {
        get => GetValue(ShapeProperty);
        set => SetValue(ShapeProperty, value);
    }

    #endregion

    private IDisposable? _cornerRadiusValue;

    public SkeletonButton()
    {
    }

    /// <summary>
    /// Recalculates the width and the corner radius when <see cref="Shape" /> or the height changes, and the width
    /// when <see cref="SkeletonElement.IsBlock" /> changes. Overrides must call the base member.
    /// </summary>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == HeightProperty || change.Property == IsBlockProperty || change.Property == ShapeProperty)
        {
            ConfigureWidth();
        }
        if (change.Property == ShapeProperty || change.Property == HeightProperty)
        {
            ConfigureShape();
        }
    }

    private void ConfigureShape()
    {
        if (Shape == SkeletonButtonShape.Round)
        {
            this.SetHeldValue(CornerRadiusProperty, new CornerRadius(Height / 2), ref _cornerRadiusValue);
        }
        else if (Shape == SkeletonButtonShape.Circle)
        {
            SetTemplateWidth(Height);
            if (!double.IsNaN(Height))
            {
                SetTemplateMinWidth(Height);
            }
            this.SetHeldValue(CornerRadiusProperty, new CornerRadius(Height), ref _cornerRadiusValue);
        }
    }

    private void ConfigureWidth()
    {
        if (!double.IsNaN(Height) && !IsBlock)
        {
            if (Shape != SkeletonButtonShape.Circle)
            {
                SetTemplateWidth(Height * 2);
                if (!double.IsNaN(Height))
                {
                    SetTemplateMinWidth(Height * 2);
                }
            }
        }
    }
}
