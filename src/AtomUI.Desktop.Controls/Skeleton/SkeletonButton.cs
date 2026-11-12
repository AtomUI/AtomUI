using AtomUI.Data;
using Avalonia;

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

    private IDisposable? _cornerRadiusValue;

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
