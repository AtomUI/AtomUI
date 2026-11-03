using AtomUI.Theme.Resources;
using AtomUI.Controls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Mixins;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Media;
using Avalonia.Metadata;

namespace AtomUI.Desktop.Controls;

public class TourStep : ContentControl, ITourStepOption, IControlThemeGlobalOwnerSource
{
    #region 公共属性定义

    public static readonly StyledProperty<Control?> TargetProperty =
        AvaloniaProperty.Register<TourStep, Control?>(nameof(Target));

    public static readonly StyledProperty<bool?> IsArrowVisibleProperty =
        AvaloniaProperty.Register<TourStep, bool?>(nameof(IsArrowVisible));

    public static readonly StyledProperty<bool?> IsPointAtCenterProperty =
        AvaloniaProperty.Register<TourStep, bool?>(nameof(IsPointAtCenter));

    public static readonly StyledProperty<PathIcon?> CloseIconProperty =
        AvaloniaProperty.Register<TourStep, PathIcon?>(nameof(CloseIcon));

    public static readonly StyledProperty<object?> CoverProperty =
        AvaloniaProperty.Register<TourStep, object?>(nameof(Cover));

    public static readonly StyledProperty<IDataTemplate?> CoverTemplateProperty =
        AvaloniaProperty.Register<TourStep, IDataTemplate?>(nameof(CoverTemplate));

    public static readonly StyledProperty<object?> TitleProperty =
        AvaloniaProperty.Register<TourStep, object?>(nameof(Title));

    public static readonly StyledProperty<IDataTemplate?> TitleTemplateProperty =
        AvaloniaProperty.Register<TourStep, IDataTemplate?>(nameof(TitleTemplate));

    public static readonly StyledProperty<object?> DescriptionProperty =
        AvaloniaProperty.Register<TourStep, object?>(nameof(Description));

    public static readonly StyledProperty<IDataTemplate?> DescriptionTemplateProperty =
        AvaloniaProperty.Register<TourStep, IDataTemplate?>(nameof(DescriptionTemplate));

    public static readonly StyledProperty<TourPlacementMode?> PlacementProperty =
        AvaloniaProperty.Register<TourStep, TourPlacementMode?>(nameof(Placement));

    public static readonly StyledProperty<bool?> IsShowMaskProperty =
        AvaloniaProperty.Register<TourStep, bool?>(nameof(IsShowMask));

    public static readonly StyledProperty<IBrush?> MaskColorProperty =
        AvaloniaProperty.Register<TourStep, IBrush?>(nameof(MaskColor));

    public static readonly StyledProperty<TourStyleType?> StyleTypeProperty =
        AvaloniaProperty.Register<TourStep, TourStyleType?>(nameof(StyleType));

    public static readonly StyledProperty<bool?> IsScrollIntoViewProperty =
        AvaloniaProperty.Register<TourStep, bool?>(nameof(IsScrollIntoView));

    public Control? Target
    {
        get => GetValue(TargetProperty);
        set => SetValue(TargetProperty, value);
    }

    public bool? IsArrowVisible
    {
        get => GetValue(IsArrowVisibleProperty);
        set => SetValue(IsArrowVisibleProperty, value);
    }

    public bool? IsPointAtCenter
    {
        get => GetValue(IsPointAtCenterProperty);
        set => SetValue(IsPointAtCenterProperty, value);
    }

    public PathIcon? CloseIcon
    {
        get => GetValue(CloseIconProperty);
        set => SetValue(CloseIconProperty, value);
    }

    [DependsOn(nameof(CoverTemplate))]
    public object? Cover
    {
        get => GetValue(CoverProperty);
        set => SetValue(CoverProperty, value);
    }

    public IDataTemplate? CoverTemplate
    {
        get => GetValue(CoverTemplateProperty);
        set => SetValue(CoverTemplateProperty, value);
    }

    [DependsOn(nameof(TitleTemplate))]
    public object? Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public IDataTemplate? TitleTemplate
    {
        get => GetValue(TitleTemplateProperty);
        set => SetValue(TitleTemplateProperty, value);
    }

    [DependsOn(nameof(DescriptionTemplate))]
    public object? Description
    {
        get => GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    public IDataTemplate? DescriptionTemplate
    {
        get => GetValue(DescriptionTemplateProperty);
        set => SetValue(DescriptionTemplateProperty, value);
    }

    public TourPlacementMode? Placement
    {
        get => GetValue(PlacementProperty);
        set => SetValue(PlacementProperty, value);
    }

    public bool? IsShowMask
    {
        get => GetValue(IsShowMaskProperty);
        set => SetValue(IsShowMaskProperty, value);
    }

    public IBrush? MaskColor
    {
        get => GetValue(MaskColorProperty);
        set => SetValue(MaskColorProperty, value);
    }

    public TourStyleType? StyleType
    {
        get => GetValue(StyleTypeProperty);
        set => SetValue(StyleTypeProperty, value);
    }

    public bool? IsScrollIntoView
    {
        get => GetValue(IsScrollIntoViewProperty);
        set => SetValue(IsScrollIntoViewProperty, value);
    }

    #endregion

    #region 内部属性定义

    internal static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<TourStep>();

    internal static readonly StyledProperty<bool> IsSelectedProperty =
        SelectingItemsControl.IsSelectedProperty.AddOwner<TourStep>();

    internal static readonly StyledProperty<IIconTemplate?> CloseIconTemplateProperty =
        AvaloniaProperty.Register<TourStep, IIconTemplate?>(nameof(CloseIconTemplate));

    internal bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }

    internal bool IsSelected
    {
        get => GetValue(IsSelectedProperty);
        set => SetValue(IsSelectedProperty, value);
    }

    internal IIconTemplate? CloseIconTemplate
    {
        get => GetValue(CloseIconTemplateProperty);
        set => SetValue(CloseIconTemplateProperty, value);
    }

    #endregion

    #region 构造与初始化

    static TourStep()
    {
        SelectableMixin.Attach<TourStep>(IsSelectedProperty);
    }

    #endregion

    #region 公共与受保护 API

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == CloseIconTemplateProperty)
        {
            if (CloseIconTemplate != null)
            {
                SetCurrentValue(CloseIconProperty, CloseIconTemplate.Build());
            }
        }
    }

    #endregion

    #region 实现 IControlThemeGlobalOwnerSource 接口

    Avalonia.Controls.Control? IControlThemeGlobalOwnerSource.ThemeGlobalOwner => TourThemeOwner.Resolve(this);

    #endregion

}