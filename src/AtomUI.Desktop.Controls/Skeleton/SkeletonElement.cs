using AtomUI.Controls;
using AtomUI.Data;
using Avalonia;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Base class of the block placeholders that stand in for a single control such as a button or an input. The
/// block is as tall as the control height of <see cref="SizeType" />.
/// </summary>
public abstract class SkeletonElement : AbstractSkeleton, ICustomizableSizeTypeAware
{
    #region 公共属性定义
    
    /// <summary>
    /// Identifies the <see cref="SizeType" /> property.
    /// </summary>
    public static readonly StyledProperty<CustomizableSizeType> SizeTypeProperty =
        CustomizableSizeTypeControlProperty.SizeTypeProperty.AddOwner<SkeletonElement>();
    
    /// <summary>
    /// Identifies the <see cref="IsBlock" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsBlockProperty =
        AvaloniaProperty.Register<SkeletonElement, bool>(nameof(IsBlock));
    
    /// <summary>
    /// Size step of the placeholder: the theme sets its height to the control height of the step
    /// (<c>ControlHeightLG</c>, <c>ControlHeight</c> or <c>ControlHeightSM</c>). The default is <c>Middle</c>.
    /// </summary>
    public CustomizableSizeType SizeType
    {
        get => GetValue(SizeTypeProperty);
        set => SetValue(SizeTypeProperty, value);
    }
    
    /// <summary>
    /// Lets the placeholder stretch to the full width of its parent; turning it on clears the width set from this
    /// class or a derived one. The default is false, which keeps the placeholder left-aligned at its own width.
    /// </summary>
    public bool IsBlock
    {
        get => GetValue(IsBlockProperty);
        set => SetValue(IsBlockProperty, value);
    }
    
    #endregion

    // The base class and derived classes both write the width through one shared handle: the last writer wins, and
    // the width always occupies a single property frame.
    private IDisposable? _widthValue;
    private IDisposable? _minWidthValue;

    static SkeletonElement()
    {
        AffectsMeasure<SkeletonElement>(IsBlockProperty);
    }
    
    /// <summary>
    /// Clears the width held by this class when <see cref="IsBlock" /> becomes true, so that the placeholder stretches
    /// to the width of its parent. Overrides must call the base member.
    /// </summary>
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
