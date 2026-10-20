using AtomUI.Animations;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Mixins;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace AtomUI.Controls.Commons;

[PseudoClasses(StdPseudoClass.Pressed, StdPseudoClass.Selected, SegmentedPseudoClass.HasIcon)]
public abstract class AbstractSegmentedItem : ContentControl, ISelectable
{
    #region 公共属性定义

    public static readonly StyledProperty<bool> IsSelectedProperty =
        SelectingItemsControl.IsSelectedProperty.AddOwner<AbstractSegmentedItem>();

    public static readonly StyledProperty<PathIcon?> IconProperty =
        AvaloniaProperty.Register<AbstractSegmentedItem, PathIcon?>(nameof(Icon));

    public bool IsSelected
    {
        get => GetValue(IsSelectedProperty);
        set => SetValue(IsSelectedProperty, value);
    }

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
    /// 选中滑块飞行期间由 owner 置位，用于屏蔽 item 的 hover/pressed 遮罩。
    /// </summary>
    internal static readonly StyledProperty<bool> IsThumbMotionActiveProperty =
        AvaloniaProperty.Register<AbstractSegmentedItem, bool>(nameof(IsThumbMotionActive));

    internal CustomizableSizeType SizeType
    {
        get => GetValue(SizeTypeProperty);
        set => SetValue(SizeTypeProperty, value);
    }

    internal SegmentedShape Shape
    {
        get => GetValue(ShapeProperty);
        set => SetValue(ShapeProperty, value);
    }

    internal bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }

    internal bool IsThumbMotionActive
    {
        get => GetValue(IsThumbMotionActiveProperty);
        set => SetValue(IsThumbMotionActiveProperty, value);
    }

    #endregion
    
    private readonly StateMotionSession _stateMotion;

    static AbstractSegmentedItem()
    {
        SelectableMixin.Attach<AbstractSegmentedItem>(IsSelectedProperty);
        PressedMixin.Attach<AbstractSegmentedItem>();
        FocusableProperty.OverrideDefaultValue<AbstractSegmentedItem>(true);
        AffectsRender<AbstractSegmentedItem>(BackgroundProperty);
    }

    protected AbstractSegmentedItem()
    {
        _stateMotion = new StateMotionSession(this);
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        _stateMotion.CompleteInitialization();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _stateMotion.Reset();
        base.OnDetachedFromVisualTree(e);
    }

    protected override void OnTemplateChanged(AvaloniaPropertyChangedEventArgs e)
    {
        _stateMotion?.Reset();
        base.OnTemplateChanged(e);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        UpdatePseudoClasses();
        _stateMotion.CompleteInitialization();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == IconProperty)
        {
            UpdatePseudoClasses();
        }
    }

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
