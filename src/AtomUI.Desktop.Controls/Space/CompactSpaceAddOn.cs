using AtomUI.Controls;
using AtomUI.Theme;
using AtomUI.Utils;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Layout;
using Avalonia.Metadata;

namespace AtomUI.Desktop.Controls;

public class CompactSpaceAddOn : TemplatedControl,
                                 IControlSharedTokenResourcesHost,
                                 ISizeTypeAware,
                                 ICompactSpaceAware
{
    #region 公共属性定义

    public static readonly StyledProperty<object?> ContentProperty =
        ContentPresenter.ContentProperty.AddOwner<CompactSpaceAddOn>();
    
    public static readonly StyledProperty<IDataTemplate?> ContentTemplateProperty =
        ContentPresenter.ContentTemplateProperty.AddOwner<CompactSpaceAddOn>();
    
    public static readonly StyledProperty<SizeType> SizeTypeProperty =
        SizeTypeControlProperty.SizeTypeProperty.AddOwner<CompactSpaceAddOn>();
    
    public static readonly StyledProperty<AddOnDecoratedVariant> StyleVariantProperty =
        AddOnDecoratedBox.StyleVariantProperty.AddOwner<CompactSpaceAddOn>();

    public static readonly StyledProperty<AddOnDecoratedStatus> StatusProperty =
        AddOnDecoratedBox.StatusProperty.AddOwner<CompactSpaceAddOn>();

    [Content]
    [DependsOn(nameof(ContentTemplate))]
    public object? Content
    {
        get => GetValue(ContentProperty);
        set => SetValue(ContentProperty, value);
    }
    
    public IDataTemplate? ContentTemplate
    {
        get => GetValue(ContentTemplateProperty);
        set => SetValue(ContentTemplateProperty, value);
    }
    
    public SizeType SizeType
    {
        get => GetValue(SizeTypeProperty);
        set => SetValue(SizeTypeProperty, value);
    }
    
    public AddOnDecoratedVariant StyleVariant
    {
        get => GetValue(StyleVariantProperty);
        set => SetValue(StyleVariantProperty, value);
    }

    public AddOnDecoratedStatus Status
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
    
    Control IControlSharedTokenResourcesHost.HostControl => this;
    string IControlSharedTokenResourcesHost.TokenId => SpaceToken.ID;
    #endregion

    static CompactSpaceAddOn()
    {
        AffectsMeasure<CompactSpaceAddOn>(SizeTypeProperty);
        AffectsRender<CompactSpaceAddOn>(StyleVariantProperty, StatusProperty);
    }

    public CompactSpaceAddOn()
    {
        this.RegisterResources();
    }

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

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        ConfigureEffectiveCornerRadius();
    }

    private void ConfigureEffectiveCornerRadius()
    {
        if (StyleVariant != AddOnDecoratedVariant.Underlined)
        {
            var topLeftRadius     = CornerRadius.TopLeft;
            var topRightRadius    = CornerRadius.TopRight;
            var bottomLeftRadius  = CornerRadius.BottomLeft;
            var bottomRightRadius = CornerRadius.BottomRight;
        
            if (IsUsedInCompactSpace && CompactSpaceItemPosition.HasValue &&
                (!CompactSpaceItemPosition.Value.HasFlag(SpaceItemPosition.First) || !CompactSpaceItemPosition.Value.HasFlag(SpaceItemPosition.Last)))
            {
                if (CompactSpaceItemPosition.Value.HasFlag(SpaceItemPosition.First))
                {
                    if (CompactSpaceOrientation == Orientation.Horizontal)
                    {
                        topRightRadius    = 0;
                        bottomRightRadius = 0;
                    }
                    else
                    {
                        bottomLeftRadius  = 0;
                        bottomRightRadius = 0;
                    }
                }
                else if (CompactSpaceItemPosition.Value.HasFlag(SpaceItemPosition.Middle))
                {
                     topLeftRadius     = 0;
                     topRightRadius    = 0;
                     bottomLeftRadius  = 0;
                     bottomRightRadius = 0;
                }
                else if (CompactSpaceItemPosition.Value.HasFlag(SpaceItemPosition.Last))
                {
                    if (CompactSpaceOrientation == Orientation.Horizontal)
                    {
                        topLeftRadius    = 0;
                        bottomLeftRadius = 0;
                    }
                    else
                    {
                        topLeftRadius = 0;
                        topRightRadius = 0;
                    }
                }
            }
            
            SetCurrentValue(EffectiveCornerRadiusProperty, new CornerRadius(topLeftRadius,
                topRightRadius,
                bottomLeft: bottomLeftRadius,
                bottomRight: bottomRightRadius));
        }
        else
        {
            SetCurrentValue(EffectiveCornerRadiusProperty, new CornerRadius(0));
        }
    }
    
    void ICompactSpaceAware.NotifyPositionChange(SpaceItemPosition? position)
    {
        IsUsedInCompactSpace     = position != null;
        CompactSpaceItemPosition = position;
    }
    
    void ICompactSpaceAware.NotifyOrientationChange(Orientation orientation)
    {
        CompactSpaceOrientation = orientation;
    }

    bool ICompactSpaceAware.IgnoreZIndexChange() => true;
    
    double ICompactSpaceAware.GetBorderThickness()
    {
        return CompactSpaceOrientation ==  Orientation.Horizontal ? BorderThickness.Left : BorderThickness.Top;
    }
}