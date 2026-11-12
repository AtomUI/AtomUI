using AtomUI.Controls;
using AtomUI.Data;
using Avalonia;

namespace AtomUI.Desktop.Controls;

public abstract class SkeletonElement : AbstractSkeleton, ICustomizableSizeTypeAware
{
    #region 公共属性定义
    
    public static readonly StyledProperty<CustomizableSizeType> SizeTypeProperty =
        CustomizableSizeTypeControlProperty.SizeTypeProperty.AddOwner<SkeletonElement>();
    
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

    // 基类与派生类都写宽度，共用一个句柄：后写者生效，且始终只占一个属性帧
    private IDisposable? _widthValue;
    private IDisposable? _minWidthValue;

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
                SetTemplateWidth(double.NaN);
            }
        }
    }

    private protected void SetTemplateWidth(double width) =>
        this.SetHeldValue(WidthProperty, width, ref _widthValue);

    private protected void SetTemplateMinWidth(double minWidth) =>
        this.SetHeldValue(MinWidthProperty, minWidth, ref _minWidthValue);

}
