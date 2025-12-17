using AtomUI.Controls;
using Avalonia;
using Avalonia.Data;

namespace AtomUI.Desktop.Controls;

public abstract class SkeletonElement : AbstractSkeleton, ICustomizableSizeTypeAware
{
    #region 公共属性定义
    
    public static readonly StyledProperty<CustomizableSizeType> SizeTypeProperty =
        CustomizableSizeTypeControlProperty.SizeTypeProperty.AddOwner<SkeletonButton>();
    
    public static readonly StyledProperty<bool> IsBlockProperty =
        AvaloniaProperty.Register<SkeletonElement, bool>(nameof(IsBlock));
    
    public CustomizableSizeType SizeType
    {
        get => GetValue(SizeTypeProperty);
        set => SetValue(SizeTypeProperty, value);
    }
    
    public bool IsBlock
    {
        get => GetValue(IsBlockProperty);
        set => SetValue(IsBlockProperty, value);
    }
    
    #endregion

    static SkeletonElement()
    {
        AffectsMeasure<SkeletonElement>(IsBlockProperty);
    }
    
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == IsBlockProperty)
        {
            if (IsBlock)
            {
                SetValue(WidthProperty, double.NaN, BindingPriority.Template);
            }
        }
    }

}