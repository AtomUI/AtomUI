using Avalonia.Controls.Primitives;
using AtomUI.Animations;
using AtomUI.Controls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

using AvaloniaComboBoxItem = Avalonia.Controls.ComboBoxItem;

/// <summary>
/// An option of the <see cref="ComboBox" /> drop-down. Selecting it on a pointer press or tap goes through the
/// owner, which commits the selection and closes the drop-down. The owner sets
/// <see cref="IsCandidateSelected" /> on the item highlighted as the pending selection; <c>Enter</c> commits it.
/// </summary>
public class ComboBoxItem : AvaloniaComboBoxItem, ITransitionInitializationOwner
{
    #region 内部属性定义

    internal static readonly StyledProperty<CustomizableSizeType> SizeTypeProperty =
        CustomizableSizeTypeControlProperty.SizeTypeProperty.AddOwner<ComboBoxItem>();
    
    internal static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<ComboBoxItem>();

    internal static readonly StyledProperty<bool> IsCandidateSelectedProperty =
        AvaloniaProperty.Register<ComboBoxItem, bool>(nameof(IsCandidateSelected));

    internal CustomizableSizeType SizeType
    {
        get => GetValue(SizeTypeProperty);
        set => SetValue(SizeTypeProperty, value);
    }
    
    internal bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }

    internal bool IsCandidateSelected
    {
        get => GetValue(IsCandidateSelectedProperty);
        set => SetValue(IsCandidateSelectedProperty, value);
    }

    #endregion

    private static readonly Point s_invalidPoint = new Point(double.NaN, double.NaN);
    private Point _pointerDownPoint = s_invalidPoint;

    private readonly TransitionInitializationSession _transitionInitialization;

    public ComboBoxItem()
    {
        _transitionInitialization = new TransitionInitializationSession(this);
    }

    /// <summary>
    /// Completes the transition initialization once the template is applied. The base method runs first; an
    /// override that does not call it must still complete the initialization.
    /// </summary>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _transitionInitialization.CompleteInitialization();
    }

    /// <summary>
    /// Resets the transition initialization session, then lets the base method run. An override that does not call
    /// the base method must still reset the session, so that it is completed again for the new template by
    /// <see cref="OnApplyTemplate" /> or <see cref="OnLoaded" />.
    /// </summary>
    protected override void OnTemplateChanged(AvaloniaPropertyChangedEventArgs e)
    {
        _transitionInitialization?.Reset();
        base.OnTemplateChanged(e);
    }

    /// <summary>
    /// Resets the transition initialization session, then calls the base method. An override that does not call
    /// the base method must still reset the session, so that it is completed again by <see cref="OnLoaded" />
    /// when the item is attached once more.
    /// </summary>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _transitionInitialization.Reset();
        base.OnDetachedFromVisualTree(e);
    }

    /// <summary>
    /// Completes the transition initialization. The base method runs first; an override that does not call it
    /// must still complete the initialization.
    /// </summary>
    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        _transitionInitialization.CompleteInitialization();
    }

    /// <summary>
    /// Selects this item through the owner <see cref="ComboBox" /> when a mouse button or a pen right button is
    /// pressed on it and the press is not handled yet. When the owner commits the selection, the drop-down closes
    /// and the press is marked handled. A touch or pen tip press only stores the press point for
    /// <see cref="OnPointerReleased" />.
    /// </summary>
    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        _pointerDownPoint = s_invalidPoint;
        if (!e.Handled && ItemsControl.ItemsControlFromItemContainer(this) is ComboBox owner)
        {
            var p = e.GetCurrentPoint(this);
            if (p.Properties.PointerUpdateKind is PointerUpdateKind.LeftButtonPressed or
                PointerUpdateKind.RightButtonPressed)
            {
                if (p.Pointer.Type == PointerType.Mouse
                    || (p.Pointer.Type == PointerType.Pen && p.Properties.IsRightButtonPressed))
                {
                    // If the pressed point comes from a mouse or right-click pen, perform the selection immediately.
                    // In case of pen, only right-click is accepted, as left click (a tip touch) is used for scrolling.
                    e.Handled = owner.UpdateSelectionFromEvent(this, e);
                }
                else
                {
                    _pointerDownPoint = p.Position;
                }
            }
        }
    }

    /// <summary>
    /// Selects this item through the owner <see cref="ComboBox" /> after a tap of a touch or a pen tip: the release
    /// must not be handled yet, it must happen inside the item and within the platform tap tolerance of the press
    /// point that <see cref="OnPointerPressed" /> stored. A mouse press selects on press already and stores no
    /// point. When the owner commits the selection, the release event is raised on the owner again for a pointer
    /// that is not a mouse, and the event is marked handled.
    /// </summary>
    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        if (!e.Handled &&
            !double.IsNaN(_pointerDownPoint.X) &&
            e.InitialPressMouseButton is MouseButton.Left or MouseButton.Right)
        {
            var point    = e.GetCurrentPoint(this);
            var settings = (e.Source as Visual)?.GetPlatformSettings();
            var tapSize  = settings?.GetTapSize(point.Pointer.Type) ?? new Size(4, 4);
            var tapRect = new Rect(_pointerDownPoint, new Size())
                .Inflate(new Thickness(tapSize.Width, tapSize.Height));

            if (new Rect(Bounds.Size).ContainsExclusive(point.Position) &&
                tapRect.ContainsExclusive(point.Position) &&
                ItemsControl.ItemsControlFromItemContainer(this) is ComboBox owner)
            {
                if (owner.UpdateSelectionFromEvent(this, e))
                {
                    // As we only update selection from touch/pen on pointer release, we need to raise
                    // the pointer event on the owner to trigger a commit.
                    if (e.Pointer.Type != PointerType.Mouse)
                    {
                        var sourceBackup = e.Source;
                        owner.RaiseEvent(e);
                        e.Source = sourceBackup;
                    }

                    e.Handled = true;
                }
            }
        }
        _pointerDownPoint = s_invalidPoint;
    }
    #region 实现 ITransitionInitializationOwner 接口

    TransitionInitializationSession ITransitionInitializationOwner.TransitionInitialization => _transitionInitialization;

    #endregion

}
