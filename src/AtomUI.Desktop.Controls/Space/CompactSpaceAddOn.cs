using AtomUI.Controls;
using Avalonia;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Layout;
using Avalonia.Metadata;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// A text or content piece attached to the start or end of a <see cref="CompactSpace" /> group and drawn as one
/// of its joined segments; its index in <see cref="CompactSpace.Children" /> decides the side. Match its
/// <see cref="StyleVariant" /> and <see cref="Status" /> with those of the joined input controls.
/// </summary>
public class CompactSpaceAddOn : TemplatedControl,
                                 ICustomizableSizeTypeAware,
                                 ICompactSpaceAware,
                                 IInputControlStatusAware,
                                 IInputControlStyleVariantAware
{
    #region 公共属性定义

    /// <summary>
    /// Identifies the <see cref="Content" /> property.
    /// </summary>
    public static readonly StyledProperty<object?> ContentProperty =
        ContentPresenter.ContentProperty.AddOwner<CompactSpaceAddOn>();
    
    /// <summary>
    /// Identifies the <see cref="ContentTemplate" /> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> ContentTemplateProperty =
        ContentPresenter.ContentTemplateProperty.AddOwner<CompactSpaceAddOn>();
    
    /// <summary>
    /// Identifies the <see cref="SizeType" /> property.
    /// </summary>
    public static readonly StyledProperty<CustomizableSizeType> SizeTypeProperty =
        CustomizableSizeTypeControlProperty.SizeTypeProperty.AddOwner<CompactSpaceAddOn>();
    
    /// <summary>
    /// Identifies the <see cref="StyleVariant" /> property.
    /// </summary>
    public static readonly StyledProperty<InputControlStyleVariant> StyleVariantProperty =
        InputControlStyleVariantProperty.StyleVariantProperty.AddOwner<CompactSpaceAddOn>();

    /// <summary>
    /// Identifies the <see cref="Status" /> property.
    /// </summary>
    public static readonly StyledProperty<InputControlStatus> StatusProperty =
        InputControlStatusProperty.StatusProperty.AddOwner<CompactSpaceAddOn>();

    /// <summary>
    /// The content shown inside the add-on. It is the XAML content property, so it can be written as the child
    /// element of the add-on. <see langword="null" /> shows nothing.
    /// </summary>
    [Content]
    [DependsOn(nameof(ContentTemplate))]
    public object? Content
    {
        get => GetValue(ContentProperty);
        set => SetValue(ContentProperty, value);
    }
    
    /// <summary>
    /// Template used to build <see cref="Content" />. <see langword="null" /> lets the content presenter choose:
    /// a control is shown as is, and any other value is looked up in the data templates of this control.
    /// </summary>
    public IDataTemplate? ContentTemplate
    {
        get => GetValue(ContentTemplateProperty);
        set => SetValue(ContentTemplateProperty, value);
    }
    
    /// <summary>
    /// Size preset of the add-on. The theme takes its font size, corner radius, padding and minimum height from
    /// it. Inside a <see cref="CompactSpace" /> the add-on receives the <see cref="CompactSpace.SizeType" /> of
    /// that control, which overrides a value set on the add-on itself.
    /// </summary>
    public CustomizableSizeType SizeType
    {
        get => GetValue(SizeTypeProperty);
        set => SetValue(SizeTypeProperty, value);
    }
    
    /// <summary>
    /// Visual style of the add-on, to be kept in step with the variant of the joined input controls. The theme
    /// styles <c>Outlined</c> with a border and a background and <c>Filled</c> with a background only; the other
    /// variants leave the add-on transparent and borderless. With <c>Underlined</c> the effective corner radius
    /// is always 0.
    /// </summary>
    public InputControlStyleVariant StyleVariant
    {
        get => GetValue(StyleVariantProperty);
        set => SetValue(StyleVariantProperty, value);
    }

    /// <summary>
    /// Status of the add-on, to be kept in step with the status of the joined input controls. <c>Error</c> and
    /// <c>Warning</c> recolor the text of the add-on, with <c>Outlined</c> also its border and with <c>Filled</c>
    /// also its background.
    /// </summary>
    public InputControlStatus Status
    {
        get => GetValue(StatusProperty);
        set => SetValue(StatusProperty, value);
    }
    #endregion
    
    #region 内部属性定义
    internal static readonly DirectProperty<CompactSpaceAddOn, CornerRadius> EffectiveCornerRadiusProperty =
        AvaloniaProperty.RegisterDirect<CompactSpaceAddOn, CornerRadius>(nameof(EffectiveCornerRadius),
            o => o.EffectiveCornerRadius,
            (o, v) => o.EffectiveCornerRadius = v);
    
    internal static readonly StyledProperty<SpaceItemPosition?> CompactSpaceItemPositionProperty = 
        CompactSpaceAwareControlProperty.CompactSpaceItemPositionProperty.AddOwner<CompactSpaceAddOn>();
    
    internal static readonly StyledProperty<Orientation> CompactSpaceOrientationProperty = 
        CompactSpaceAwareControlProperty.CompactSpaceOrientationProperty.AddOwner<CompactSpaceAddOn>();
    
    internal static readonly StyledProperty<bool> IsUsedInCompactSpaceProperty = 
        CompactSpaceAwareControlProperty.IsUsedInCompactSpaceProperty.AddOwner<CompactSpaceAddOn>();
    
    private CornerRadius _effectiveCornerRadius;

    internal CornerRadius EffectiveCornerRadius
    {
        get => _effectiveCornerRadius;
        set => SetAndRaise(EffectiveCornerRadiusProperty, ref _effectiveCornerRadius, value);
    }
    
    internal SpaceItemPosition? CompactSpaceItemPosition
    {
        get => GetValue(CompactSpaceItemPositionProperty);
        set => SetValue(CompactSpaceItemPositionProperty, value);
    }
    
    internal Orientation CompactSpaceOrientation
    {
        get => GetValue(CompactSpaceOrientationProperty);
        set => SetValue(CompactSpaceOrientationProperty, value);
    }
    
    internal bool IsUsedInCompactSpace
    {
        get => GetValue(IsUsedInCompactSpaceProperty);
        set => SetValue(IsUsedInCompactSpaceProperty, value);
    }
    
    #endregion

    static CompactSpaceAddOn()
    {
        AffectsMeasure<CompactSpaceAddOn>(SizeTypeProperty);
        AffectsRender<CompactSpaceAddOn>(StyleVariantProperty, StatusProperty);
    }

    /// <summary>
    /// Calls the base implementation, then recomputes the effective corner radius when
    /// <see cref="TemplatedControl.CornerRadius" />, the position in the <see cref="CompactSpace" /> or the
    /// orientation of that control changes.
    /// </summary>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == CornerRadiusProperty ||
            change.Property == CompactSpaceItemPositionProperty ||
            change.Property == CompactSpaceOrientationProperty)
        {
            ConfigureEffectiveCornerRadius();
        }
    }

    /// <summary>
    /// Applies the template, then recomputes the effective corner radius so that the template finds a value that
    /// matches the current position in a <see cref="CompactSpace" /> and its orientation.
    /// </summary>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        ConfigureEffectiveCornerRadius();
    }

    #region 实现 ICompactSpaceAware 接口

    void ICompactSpaceAware.NotifyPositionChange(SpaceItemPosition? position)
    {
        var isUsedInCompactSpace = position != null;
        if (IsUsedInCompactSpace != isUsedInCompactSpace)
        {
            IsUsedInCompactSpace = isUsedInCompactSpace;
        }

        if (CompactSpaceItemPosition != position)
        {
            CompactSpaceItemPosition = position;
        }
    }
    
    void ICompactSpaceAware.NotifyOrientationChange(Orientation orientation)
    {
        if (CompactSpaceOrientation != orientation)
        {
            CompactSpaceOrientation = orientation;
        }
    }

    bool ICompactSpaceAware.IgnoreZIndexChange() => true;
    
    double ICompactSpaceAware.GetBorderThickness()
    {
        return CompactSpaceOrientation == Orientation.Horizontal ? BorderThickness.Left : BorderThickness.Top;
    }

    #endregion

    private void ConfigureEffectiveCornerRadius()
    {
        if (StyleVariant != InputControlStyleVariant.Underlined)
        {
            SetCurrentValue(
                EffectiveCornerRadiusProperty,
                CompactSpace.CalculateEffectiveCornerRadius(
                    CornerRadius,
                    IsUsedInCompactSpace,
                    CompactSpaceItemPosition,
                    CompactSpaceOrientation));
        }
        else
        {
            SetCurrentValue(EffectiveCornerRadiusProperty, new CornerRadius(0));
        }
    }
}
