using AtomUI.Animations;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Mixins;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace AtomUI.Controls.Commons;

/// <summary>
/// Base class of the items of an <see cref="AbstractSegmented" />: a content control with an optional leading
/// icon that the owner selects. It reports the selected, pressed and icon states as pseudo-classes, which the
/// theme styles.
/// </summary>
[PseudoClasses(StdPseudoClass.Pressed, StdPseudoClass.Selected, SegmentedPseudoClass.HasIcon)]
public abstract class AbstractSegmentedItem : ContentControl, ISelectable
{
    #region 公共属性定义

    /// <summary>
    /// Identifies the <see cref="IsSelected" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsSelectedProperty =
        SelectingItemsControl.IsSelectedProperty.AddOwner<AbstractSegmentedItem>();

    /// <summary>
    /// Identifies the <see cref="Icon" /> property.
    /// </summary>
    public static readonly StyledProperty<PathIcon?> IconProperty =
        AvaloniaProperty.Register<AbstractSegmentedItem, PathIcon?>(nameof(Icon));

    /// <summary>
    /// Whether this item is the selected one of its owner. The owner keeps the value in sync with its selection,
    /// and an item that declares <see langword="true" /> becomes the selected item of the owner.
    /// </summary>
    public bool IsSelected
    {
        get => GetValue(IsSelectedProperty);
        set => SetValue(IsSelectedProperty, value);
    }

    /// <summary>
    /// Icon shown to the left of the content, or <see langword="null" /> to show the content only. A non-null
    /// icon sets the <c>:has-icon</c> pseudo-class, which the theme uses to space the content from the icon.
    /// </summary>
    public PathIcon? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    #endregion

    #region 内部属性定义

    internal static readonly StyledProperty<CustomizableSizeType> SizeTypeProperty =
        CustomizableSizeTypeControlProperty.SizeTypeProperty.AddOwner<AbstractSegmentedItem>();

    internal static readonly StyledProperty<SegmentedShape> ShapeProperty =
        AbstractSegmented.ShapeProperty.AddOwner<AbstractSegmentedItem>();
    
    internal static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<AbstractSegmentedItem>();

    /// <summary>
    /// Identifies the <see cref="IsThumbMotionActive" /> property.
    /// </summary>
    internal static readonly StyledProperty<bool> IsThumbMotionActiveProperty =
        AvaloniaProperty.Register<AbstractSegmentedItem, bool>(nameof(IsThumbMotionActive));

    /// <summary>
    /// The size preset of the item. The owner binds it to its own <see cref="AbstractSegmented.SizeType" /> when it
    /// prepares the item, so an item follows the owner; the theme selects on it, as in <c>^[SizeType=Large]</c>,
    /// for the preset values.
    /// </summary>
    internal CustomizableSizeType SizeType
    {
        get => GetValue(SizeTypeProperty);
        set => SetValue(SizeTypeProperty, value);
    }

    /// <summary>
    /// The outline shape of the item. The owner binds it to its own <see cref="AbstractSegmented.Shape" /> when it
    /// prepares the item; the theme selects on it, as in <c>^[Shape=Round]</c>, for the round shape.
    /// </summary>
    internal SegmentedShape Shape
    {
        get => GetValue(ShapeProperty);
        set => SetValue(ShapeProperty, value);
    }

    /// <summary>
    /// Whether the colors of the item fade when its state changes. The owner binds it to its own
    /// <see cref="AbstractSegmented.IsMotionEnabled" /> when it prepares the item; the theme adds its transitions
    /// under <c>^[IsMotionEnabled=True]</c>. With <see langword="false" /> the item changes at once.
    /// </summary>
    internal bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }

    /// <summary>
    /// Whether the thumb of the owner is gliding to another item. The owner sets it on every item for the duration
    /// of that motion; the theme uses it to suppress the hover and pressed overlays of the items that are not
    /// selected. It is <see langword="false" /> while the thumb rests.
    /// </summary>
    internal bool IsThumbMotionActive
    {
        get => GetValue(IsThumbMotionActiveProperty);
        set => SetValue(IsThumbMotionActiveProperty, value);
    }

    #endregion
    
    private readonly TransitionInitializationSession _transitionInitialization;

    static AbstractSegmentedItem()
    {
        SelectableMixin.Attach<AbstractSegmentedItem>(IsSelectedProperty);
        PressedMixin.Attach<AbstractSegmentedItem>();
        FocusableProperty.OverrideDefaultValue<AbstractSegmentedItem>(true);
        AffectsRender<AbstractSegmentedItem>(BackgroundProperty);
    }

    protected AbstractSegmentedItem()
    {
        _transitionInitialization = new TransitionInitializationSession(this);
    }

    /// <summary>
    /// Completes the transition initialization once the item is loaded, so that state changes from now on animate.
    /// Overrides must call the base member.
    /// </summary>
    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        _transitionInitialization.CompleteInitialization();
    }

    /// <summary>
    /// Resets the transition initialization when the item leaves the visual tree, so that a later attach writes
    /// its first values without animation. Overrides must call the base member.
    /// </summary>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _transitionInitialization.Reset();
        base.OnDetachedFromVisualTree(e);
    }

    /// <summary>
    /// Resets the transition initialization when the template changes, so that the state of the new template
    /// parts is written without animation. Overrides must call the base member.
    /// </summary>
    protected override void OnTemplateChanged(AvaloniaPropertyChangedEventArgs e)
    {
        _transitionInitialization?.Reset();
        base.OnTemplateChanged(e);
    }

    /// <summary>
    /// Updates the pseudo-classes from the current <see cref="Icon" /> and completes the transition
    /// initialization when the item is already loaded. Overrides must call the base member.
    /// </summary>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        UpdatePseudoClasses();
        _transitionInitialization.CompleteInitialization();
    }

    /// <summary>
    /// Updates the pseudo-classes when <see cref="Icon" /> changes. While the item is selected and the owner uses
    /// <see cref="CustomizableSizeType.Custom" />, it also asks the owner to redraw when the corner radius of the
    /// item changes, so the thumb picks it up. Overrides must call the base member.
    /// </summary>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == IconProperty)
        {
            UpdatePseudoClasses();
        }
        else if (change.Property == CornerRadiusProperty && IsSelected &&
                 ItemsControl.ItemsControlFromItemContainer(this) is AbstractSegmented { SizeType: CustomizableSizeType.Custom } owner)
        {
            owner.NotifySelectedItemCornerRadiusChanged();
        }
    }

    /// <summary>
    /// Selects this item in the owner when the left mouse button is released over it, unless the event is already
    /// handled; other buttons and pointer types are left to the base member. Overrides must call the base member.
    /// </summary>
    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);

        if (!e.Handled && ItemsControl.ItemsControlFromItemContainer(this) is AbstractSegmented owner)
        {
            var p = e.GetCurrentPoint(this);

            if (p.Properties.PointerUpdateKind is PointerUpdateKind.LeftButtonReleased)
            {
                if (p.Pointer.Type == PointerType.Mouse)
                {
                    // If the pressed point comes from a mouse, perform the selection immediately.
                    e.Handled = owner.UpdateSelectionFromPointerEvent(this, e);
                }
            }
        }
    }

    private void UpdatePseudoClasses()
    {
        PseudoClasses.Set(SegmentedPseudoClass.HasIcon, Icon is not null);
    }
}
