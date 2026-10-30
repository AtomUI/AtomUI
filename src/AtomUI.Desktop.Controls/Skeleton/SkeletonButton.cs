using Avalonia;
using Avalonia.Data;

namespace AtomUI.Desktop.Controls;

public enum SkeletonButtonShape
{
    Square,
    Round,
    Circle
}

public partial class SkeletonButton : SkeletonElement
{
    #region 公共属性定义
    public static readonly StyledProperty<SkeletonButtonShape> ShapeProperty =
        AvaloniaProperty.Register<SkeletonButton, SkeletonButtonShape>(nameof(Shape));

    public SkeletonButtonShape Shape
    {
        get => GetValue(ShapeProperty);
        set => SetValue(ShapeProperty, value);
    }

    #endregion

    public SkeletonButton()
    {
    }

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
            SetValue(CornerRadiusProperty, new CornerRadius(Height / 2), BindingPriority.Template);
        }
        else if (Shape == SkeletonButtonShape.Circle)
        {
            SetValue(WidthProperty, Height, BindingPriority.Template);
            if (!double.IsNaN(Height))
            {
                SetValue(MinWidthProperty, Height, BindingPriority.Template);
            }
            SetValue(CornerRadiusProperty, new CornerRadius(Height), BindingPriority.Template);
        }
    }

    private void ConfigureWidth()
    {
        if (!double.IsNaN(Height) && !IsBlock)
        {
            if (Shape != SkeletonButtonShape.Circle)
            {
                SetValue(WidthProperty, Height * 2, BindingPriority.Template);
                if (!double.IsNaN(Height))
                {
                    SetValue(MinWidthProperty, Height * 2, BindingPriority.Template);
                }
            }
        }
    }
}
