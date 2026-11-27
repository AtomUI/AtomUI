using AtomUI.Controls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Mixins;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Media;
using Avalonia.Metadata;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// One step of a <see cref="Tour" />: a popup card with a title, a cover, a description and a close button,
/// plus the options that tell the Tour which control to point at and how to show this step.
/// Put instances into <c>Tour.Steps</c>, or give the Tour other <see cref="ITourStepOption" /> items and it
/// creates a step for each of them. A step is visible only while it is the current step.
/// A <see langword="null" /> option makes the step use the value of the Tour. The Tour copies its value into
/// every step option that is still <see langword="null" /> when the Tour is initialized and whenever its steps
/// change, so the step then holds that copy instead of <see langword="null" />.
/// </summary>
public class TourStep : ContentControl, ITourStepOption
{
    #region 公共属性定义

    /// <summary>
    /// Identifies the <see cref="Target" /> property.
    /// </summary>
    public static readonly StyledProperty<Control?> TargetProperty =
        AvaloniaProperty.Register<TourStep, Control?>(nameof(Target));
    
    /// <summary>
    /// Identifies the <see cref="IsArrowVisible" /> property.
    /// </summary>
    public static readonly StyledProperty<bool?> IsArrowVisibleProperty =
        AvaloniaProperty.Register<TourStep, bool?>(nameof(IsArrowVisible));
    
    /// <summary>
    /// Identifies the <see cref="IsPointAtCenter" /> property.
    /// </summary>
    public static readonly StyledProperty<bool?> IsPointAtCenterProperty =
        AvaloniaProperty.Register<TourStep, bool?>(nameof(IsPointAtCenter));

    /// <summary>
    /// Identifies the <see cref="CloseIcon" /> property.
    /// </summary>
    public static readonly StyledProperty<PathIcon?> CloseIconProperty =
        AvaloniaProperty.Register<TourStep, PathIcon?>(nameof(CloseIcon));
    
    /// <summary>
    /// Identifies the <see cref="Cover" /> property.
    /// </summary>
    public static readonly StyledProperty<object?> CoverProperty =
        AvaloniaProperty.Register<TourStep, object?>(nameof(Cover));
    
    /// <summary>
    /// Identifies the <see cref="CoverTemplate" /> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> CoverTemplateProperty =
        AvaloniaProperty.Register<TourStep, IDataTemplate?>(nameof(CoverTemplate));
    
    /// <summary>
    /// Identifies the <see cref="Title" /> property.
    /// </summary>
    public static readonly StyledProperty<object?> TitleProperty =
        AvaloniaProperty.Register<TourStep, object?>(nameof(Title));
    
    /// <summary>
    /// Identifies the <see cref="TitleTemplate" /> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> TitleTemplateProperty =
        AvaloniaProperty.Register<TourStep, IDataTemplate?>(nameof(TitleTemplate));
    
    /// <summary>
    /// Identifies the <see cref="Description" /> property.
    /// </summary>
    public static readonly StyledProperty<object?> DescriptionProperty =
        AvaloniaProperty.Register<TourStep, object?>(nameof(Description));
    
    /// <summary>
    /// Identifies the <see cref="DescriptionTemplate" /> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> DescriptionTemplateProperty =
        AvaloniaProperty.Register<TourStep, IDataTemplate?>(nameof(DescriptionTemplate));
    
    /// <summary>
    /// Identifies the <see cref="Placement" /> property.
    /// </summary>
    public static readonly StyledProperty<TourPlacementMode?> PlacementProperty =
        AvaloniaProperty.Register<TourStep, TourPlacementMode?>(nameof(Placement));
    
    /// <summary>
    /// Identifies the <see cref="IsShowMask" /> property.
    /// </summary>
    public static readonly StyledProperty<bool?> IsShowMaskProperty =
        AvaloniaProperty.Register<TourStep, bool?>(nameof(IsShowMask));
    
    /// <summary>
    /// Identifies the <see cref="MaskColor" /> property.
    /// </summary>
    public static readonly StyledProperty<IBrush?> MaskColorProperty =
        AvaloniaProperty.Register<TourStep, IBrush?>(nameof(MaskColor));

    /// <summary>
    /// Identifies the <see cref="StyleType" /> property.
    /// </summary>
    public static readonly StyledProperty<TourStyleType?> StyleTypeProperty =
        AvaloniaProperty.Register<TourStep, TourStyleType?>(nameof(StyleType));
    
    /// <summary>
    /// Identifies the <see cref="IsScrollIntoView" /> property.
    /// </summary>
    public static readonly StyledProperty<bool?> IsScrollIntoViewProperty =
        AvaloniaProperty.Register<TourStep, bool?>(nameof(IsScrollIntoView));
    
    /// <summary>
    /// Control this step points at while it is current: the popup is placed next to it and the mask leaves it
    /// uncovered.
    /// <see langword="null" /> (the default) shows the step as a centered popup without an arrow, whatever
    /// <see cref="Placement" /> and <see cref="IsArrowVisible" /> are set to.
    /// </summary>
    public Control? Target
    {
        get => GetValue(TargetProperty);
        set => SetValue(TargetProperty, value);
    }
    
    /// <summary>
    /// Overrides <see cref="Tour.IsArrowVisible" /> for this step: whether the popup shows an arrow while this step
    /// is current. <see langword="null" /> (the default) uses the value of the Tour.
    /// A step without a <see cref="Target" /> never shows an arrow.
    /// </summary>
    public bool? IsArrowVisible
    {
        get => GetValue(IsArrowVisibleProperty);
        set => SetValue(IsArrowVisibleProperty, value);
    }

    /// <summary>
    /// Per-step value of <see cref="Tour.IsPointAtCenter" />.
    /// <see langword="null" /> (the default) uses the value of the Tour, which the tour copies into its steps.
    /// No code evaluates the value of a single step at present.
    /// </summary>
    public bool? IsPointAtCenter
    {
        get => GetValue(IsPointAtCenterProperty);
        set => SetValue(IsPointAtCenterProperty, value);
    }
    
    /// <summary>
    /// Icon shown by the close button in the header of the step; <see langword="null" /> shows the button without an
    /// icon.
    /// The Tour passes its own <see cref="Tour.CloseIcon" /> template down to the step, and the step builds this
    /// value from it each time that template changes, which replaces a value set here.
    /// </summary>
    public PathIcon? CloseIcon
    {
        get => GetValue(CloseIconProperty);
        set => SetValue(CloseIconProperty, value);
    }
    
    /// <summary>
    /// Content shown at the top of the body of the step, above the description; <see langword="null" /> shows no
    /// cover.
    /// </summary>
    [DependsOn(nameof(CoverTemplate))]
    public object? Cover
    {
        get => GetValue(CoverProperty);
        set => SetValue(CoverProperty, value);
    }
    
    /// <summary>
    /// Template that builds the cover from <see cref="Cover" />.
    /// <see langword="null" /> (the default) presents the cover without an explicit template.
    /// </summary>
    public IDataTemplate? CoverTemplate
    {
        get => GetValue(CoverTemplateProperty);
        set => SetValue(CoverTemplateProperty, value);
    }
    
    /// <summary>
    /// Content shown in the header of the step, to the left of the close button; <see langword="null" /> shows no
    /// title.
    /// </summary>
    [DependsOn(nameof(TitleTemplate))]
    public object? Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }
    
    /// <summary>
    /// Template that builds the title from <see cref="Title" />.
    /// <see langword="null" /> (the default) presents the title without an explicit template.
    /// </summary>
    public IDataTemplate? TitleTemplate
    {
        get => GetValue(TitleTemplateProperty);
        set => SetValue(TitleTemplateProperty, value);
    }
    
    /// <summary>
    /// Content shown in the body of the step, below the cover; <see langword="null" /> shows no description.
    /// </summary>
    [DependsOn(nameof(DescriptionTemplate))]
    public object? Description
    {
        get => GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }
    
    /// <summary>
    /// Template that builds the description from <see cref="Description" />.
    /// <see langword="null" /> (the default) presents the description without an explicit template.
    /// </summary>
    public IDataTemplate? DescriptionTemplate
    {
        get => GetValue(DescriptionTemplateProperty);
        set => SetValue(DescriptionTemplateProperty, value);
    }
    
    /// <summary>
    /// Overrides <see cref="Tour.Placement" /> for this step: the position of the popup relative to
    /// <see cref="Target" /> while this step is current. <see langword="null" /> (the default) uses the value of the
    /// Tour. A step without a target is centered, whatever this value is.
    /// </summary>
    public TourPlacementMode? Placement
    {
        get => GetValue(PlacementProperty);
        set => SetValue(PlacementProperty, value);
    }
    
    /// <summary>
    /// Overrides <see cref="Tour.IsShowMask" /> for this step: whether the mask is shown while this step is
    /// current. <see langword="null" /> (the default) uses the value of the Tour.
    /// </summary>
    public bool? IsShowMask
    {
        get => GetValue(IsShowMaskProperty);
        set => SetValue(IsShowMaskProperty, value);
    }
    
    /// <summary>
    /// Overrides <see cref="Tour.MaskColor" /> for this step: the color of the mask while this step is current.
    /// <see langword="null" /> (the default) uses the value of the Tour, which the theme binds to the
    /// <c>ColorBgMask</c> token. Unlike the other options, the Tour does not copy its value into the step.
    /// </summary>
    public IBrush? MaskColor
    {
        get => GetValue(MaskColorProperty);
        set => SetValue(MaskColorProperty, value);
    }
    
    /// <summary>
    /// Overrides <see cref="Tour.StyleType" /> for this step: the visual style of the popup while this step is
    /// current. <see langword="null" /> (the default) uses the value of the Tour.
    /// </summary>
    public TourStyleType? StyleType
    {
        get => GetValue(StyleTypeProperty);
        set => SetValue(StyleTypeProperty, value);
    }
    
    /// <summary>
    /// Per-step value of <see cref="Tour.IsScrollIntoView" />.
    /// <see langword="null" /> (the default) uses the value of the Tour, which the tour copies into its steps.
    /// No code evaluates the value of a single step at present.
    /// </summary>
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
    
    static TourStep()
    {
        SelectableMixin.Attach<TourStep>(IsSelectedProperty);
    }

    /// <summary>
    /// Called when a property of this step changes. After the base call it builds <see cref="CloseIcon" /> from
    /// <c>CloseIconTemplate</c> whenever that template changes to a value that is not <see langword="null" />.
    /// An override must call the base member, otherwise the icon is not rebuilt and the base classes are not
    /// notified of the change.
    /// </summary>
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
}