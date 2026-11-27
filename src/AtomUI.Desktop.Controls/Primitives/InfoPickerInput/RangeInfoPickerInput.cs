using AtomUI.Animations;
using AtomUI.Utils;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls.Primitives;

/// <summary>
/// Base class of the range pickers (<c>RangeDatePicker</c>, <c>RangeTimePicker</c>): an info picker input whose
/// two text boxes hold the start and the end value. It tracks which of the two inputs is active, opens the
/// picker popup under that input and underlines it with the range indicator.
/// </summary>
public abstract class RangeInfoPickerInput : InfoPickerInput
{
    #region 公共属性定义

    /// <summary>
    /// Identifies the <see cref="SecondaryPlaceholderText" /> property.
    /// </summary>
    public static readonly StyledProperty<string?> SecondaryPlaceholderTextProperty =
        AvaloniaProperty.Register<RangeInfoPickerInput, string?>(nameof(SecondaryPlaceholderText));
    
    /// <summary>
    /// Gets or sets the placeholder text of the end input only; the start input has its own placeholder. The
    /// default is <see langword="null" />, which shows no placeholder.
    /// </summary>
    public string? SecondaryPlaceholderText
    {
        get => GetValue(SecondaryPlaceholderTextProperty);
        set => SetValue(SecondaryPlaceholderTextProperty, value);
    }
    
    #endregion

    #region 内部属性定义

    internal static readonly DirectProperty<RangeInfoPickerInput, RangeActivatedPart> RangeActivatedPartProperty =
        AvaloniaProperty.RegisterDirect<RangeInfoPickerInput, RangeActivatedPart>(nameof(RangeActivatedPart),
            o => o.RangeActivatedPart);
    
    /// <summary>
    /// Size of the range arrow between the two inputs. The arrow is a registered Icon, so an owner read on it would
    /// resolve to the Icon identity: the theme reads the picker's values on the picker and template-binds them to
    /// the arrow.
    /// </summary>
    internal static readonly StyledProperty<double> RangeArrowSizeProperty =
        AvaloniaProperty.Register<RangeInfoPickerInput, double>(nameof(RangeArrowSize), double.NaN);

    /// <summary>
    /// Brush of the range arrow (read on the picker for the same reason as <see cref="RangeArrowSizeProperty" />).
    /// </summary>
    internal static readonly StyledProperty<IBrush?> RangeArrowBrushProperty =
        AvaloniaProperty.Register<RangeInfoPickerInput, IBrush?>(nameof(RangeArrowBrush));

    internal double RangeArrowSize
    {
        get => GetValue(RangeArrowSizeProperty);
        set => SetValue(RangeArrowSizeProperty, value);
    }

    internal IBrush? RangeArrowBrush
    {
        get => GetValue(RangeArrowBrushProperty);
        set => SetValue(RangeArrowBrushProperty, value);
    }

    internal static readonly StyledProperty<double> RangePickerIndicatorOpacityProperty =
        AvaloniaProperty.Register<RangeInfoPickerInput, double>(nameof(RangePickerIndicatorOpacity), 0d);
    
    internal double RangePickerIndicatorOpacity
    {
        get => GetValue(RangePickerIndicatorOpacityProperty);
        set => SetValue(RangePickerIndicatorOpacityProperty, value);
    }

    internal static readonly StyledProperty<double> PickerIndicatorOffsetXProperty =
        AvaloniaProperty.Register<RangeInfoPickerInput, double>(nameof(PickerIndicatorOffsetX), double.NaN);
    
    internal static readonly StyledProperty<double> PickerIndicatorOffsetYProperty =
        AvaloniaProperty.Register<RangeInfoPickerInput, double>(nameof(PickerIndicatorOffsetY));
    
    internal static readonly StyledProperty<string?> SecondaryTextProperty =
        AvaloniaProperty.Register<RangeInfoPickerInput, string?>(nameof(SecondaryText));
    
    private RangeActivatedPart _rangeActivatedPart;

    internal RangeActivatedPart RangeActivatedPart
    {
        get => _rangeActivatedPart;
        set => SetAndRaise(RangeActivatedPartProperty, ref _rangeActivatedPart, value);
    }
    
    internal double PickerIndicatorOffsetX
    {
        get => GetValue(PickerIndicatorOffsetXProperty);
        set => SetValue(PickerIndicatorOffsetXProperty, value);
    }
    
    internal double PickerIndicatorOffsetY
    {
        get => GetValue(PickerIndicatorOffsetYProperty);
        set => SetValue(PickerIndicatorOffsetYProperty, value);
    }
    

    internal string? SecondaryText
    {
        get => GetValue(SecondaryTextProperty);
        set => SetValue(SecondaryTextProperty, value);
    }

    #endregion
        
    private readonly TransitionInitializationSession _transitionInitialization;

    static RangeInfoPickerInput()
    {
        AffectsArrange<RangeInfoPickerInput>(PickerIndicatorOffsetXProperty, PickerIndicatorOffsetYProperty);
    }
    
    private protected Rectangle? RangePickerIndicator;
    private protected PathIcon? RangePickerArrow;
    private protected TextBox? SecondaryInfoInputBox;

    /// <summary>
    /// Clears the text of both inputs, the start input and the end input. The base implementation, which clears only
    /// the start input, is not called.
    /// </summary>
    public override void Clear()
    {
        InfoInputBox?.Clear();
        SecondaryInfoInputBox?.Clear();
    }
    

    protected RangeInfoPickerInput()
    {
        _transitionInitialization = new TransitionInitializationSession(this);
    }

    /// <summary>
    /// Calls the base implementation, then completes the transition initialization session of this control, as
    /// <see cref="OnApplyTemplate" /> does when the template is applied. Call the base method from an override.
    /// </summary>
    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        _transitionInitialization.CompleteInitialization();
    }

    /// <summary>
    /// Resets the transition initialization session of this control, then calls the base implementation. The session
    /// is completed again when the template is applied or the control loads.
    /// </summary>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _transitionInitialization.Reset();
        base.OnDetachedFromVisualTree(e);
    }

    /// <summary>
    /// Resets the transition initialization session when the template changes, before the base implementation runs.
    /// The session is completed again when the new template is applied.
    /// </summary>
    protected override void OnTemplateChanged(AvaloniaPropertyChangedEventArgs e)
    {
        _transitionInitialization?.Reset();
        base.OnTemplateChanged(e);
    }

    /// <summary>
    /// Calls the base implementation, then resolves the end input (<c>PART_SecondaryInfoInputBox</c>), the range
    /// indicator (<c>PART_RangePickerIndicator</c>) and the range arrow (<c>PART_RangePickerArrow</c>). The
    /// subscription to the indicator's height change moves from the previous indicator to the new one, the picker
    /// popup's <c>OverlayInputPassThroughElement</c> is set to the decorated box, and the transition initialization
    /// session is completed. Call the base method from an override.
    /// </summary>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        if (RangePickerIndicator is not null)
        {
            RangePickerIndicator.PropertyChanged -= HandleIndicatorPropertyChanged;
        }

        SecondaryInfoInputBox = e.NameScope.Get<TextBox>("PART_SecondaryInfoInputBox");
        RangePickerIndicator  = e.NameScope.Get<Rectangle>("PART_RangePickerIndicator");
        RangePickerIndicator.PropertyChanged += HandleIndicatorPropertyChanged;
        RangePickerArrow = e.NameScope.Get<PathIcon>("PART_RangePickerArrow");
        if (PickerPopup != null)
        {
            PickerPopup.OverlayInputPassThroughElement = DecoratedBox;
        }
        _transitionInitialization.CompleteInitialization();
    }

    // The active bar sits on the bottom edge (offset = frame height - bar height, computed in MeasureOverride): a new
    // thickness from the theme must re-measure, or the bar grows below the frame.
    private void HandleIndicatorPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs args)
    {
        if (args.Property == HeightProperty)
        {
            InvalidateMeasure();
        }
    }

    private bool IsPointerInTextBox(TextBox? textBox, Point position, double? contentStartOffsetX)
    {
        if (textBox is null)
        {
            return false;
        }

        var pos = textBox.TranslatePoint(new Point(0, 0), this);
        if (!pos.HasValue)
        {
            return false;
        }

        var targetWidth  = textBox.Bounds.Width;
        var targetHeight = textBox.Bounds.Height;
        var originalStartOffsetX = pos.Value.X;
        var startOffsetX         = contentStartOffsetX ?? originalStartOffsetX;
        var endOffsetX           = originalStartOffsetX + targetWidth;
        var offsetY      = pos.Value.Y;

        targetWidth = endOffsetX - startOffsetX;
        var bounds = new Rect(new Point(startOffsetX, offsetY), new Size(targetWidth, targetHeight));
        return bounds.Contains(position);
    }

    private double? GetContentStartOffsetX()
    {
        if (ContentLeftAddOn is Control leftContent)
        {
            var leftContentPos = leftContent.TranslatePoint(new Point(0, 0), this);
            if (leftContentPos.HasValue)
            {
                return leftContentPos.Value.X + leftContent.Bounds.Width;
            }
        }

        return null;
    }

    /// <summary>
    /// Calls the base implementation, then reacts to a change of <c>RangeActivatedPart</c>: it places the picker
    /// popup and the range indicator at the input of the new part, or hides the indicator when no part is active,
    /// and moves keyboard focus from the other input to the input of the active part. Call the base method from
    /// an override.
    /// </summary>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == RangeActivatedPartProperty)
        {
            NotifyRangeActivatedPartChanged();
            SyncKeyboardFocusToActivatedPart();
        }
    }
    
    // The activated part and the focused input stay in step, as in Ant Design: tabbing into an input activates its
    // part, and a part switch made by the editor (for example after the start date is chosen) moves focus with it.
    /// <summary>
    /// Calls the base implementation, then activates the start part when the start input receives focus and the
    /// end part when the end input does. Focus on any other element leaves the active part unchanged. Call the
    /// base method from an override.
    /// </summary>
    protected override void OnGotFocus(FocusChangedEventArgs e)
    {
        base.OnGotFocus(e);
        if (ReferenceEquals(e.Source, InfoInputBox))
        {
            SetRangeActivatedPart(RangeActivatedPart.Start);
        }
        else if (ReferenceEquals(e.Source, SecondaryInfoInputBox))
        {
            SetRangeActivatedPart(RangeActivatedPart.End);
        }
    }

    /// <summary>
    /// Calls the base implementation, then deactivates the active part, which hides the range indicator, unless the
    /// picker is open or the element that receives focus is a picker element (<c>IsPickerElement</c>). Call the base
    /// method from an override.
    /// </summary>
    protected override void OnLostFocus(FocusChangedEventArgs e)
    {
        base.OnLostFocus(e);
        if (!IsPickerOpen && !IsPickerElement(e.NewFocusedElement))
        {
            SetRangeActivatedPart(RangeActivatedPart.None);
        }
    }

    /// <summary>
    /// Calls the base implementation, then also accepts the secondary info input box (<c>SecondaryInfoInputBox</c>)
    /// and its visual descendants, so that keys typed into the end input are handled like keys typed into the start
    /// input. Call the base method from an override to keep both input boxes recognized.
    /// </summary>
    /// <param name="source">
    /// The source of the event, such as <c>KeyEventArgs.Source</c>.
    /// </param>
    /// <returns>
    /// <see langword="true" /> when <paramref name="source" /> is either input box or lies within one of them;
    /// otherwise <see langword="false" />.
    /// </returns>
    private protected override bool IsPickerInputElement(object? source) =>
        base.IsPickerInputElement(source) ||
        source is Visual visual && SecondaryInfoInputBox is not null &&
        (ReferenceEquals(visual, SecondaryInfoInputBox) || SecondaryInfoInputBox.IsVisualAncestorOf(visual));

    /// <summary>
    /// The input of the activated part, or null when no part is active.
    /// </summary>
    private protected TextBox? ActivatedInputBox => RangeActivatedPart switch
    {
        RangeActivatedPart.Start => InfoInputBox,
        RangeActivatedPart.End => SecondaryInfoInputBox,
        _ => null
    };

    private void SyncKeyboardFocusToActivatedPart()
    {
        var target = ActivatedInputBox;
        var focused = TopLevel.GetTopLevel(this)?.FocusManager?.GetFocusedElement();
        if (target is not null && !ReferenceEquals(focused, target) &&
            (ReferenceEquals(focused, InfoInputBox) || ReferenceEquals(focused, SecondaryInfoInputBox)))
        {
            target.Focus(NavigationMethod.Tab);
        }
    }

    /// <summary>
    /// Handles a pointer release on this control while it is not read-only: a release on the start input activates
    /// the start part and opens the picker, a release on the end input or anywhere else except the clear button
    /// activates the end part and opens the picker, and a release on the clear button in clear mode closes the
    /// picker. Each of these marks the release handled. A right-button release with this control as the source
    /// raises the context request and takes over its handled state. The base implementation is not called.
    /// </summary>
    protected override void OnPointerReleased(PointerReleasedEventArgs args)
    {
        if (args.Source == this
            && !args.Handled
            && args.InitialPressMouseButton == MouseButton.Right)
        {
            var contextRequestedEventArgs = new ContextRequestedEventArgs(args);
            RaiseEvent(contextRequestedEventArgs);
            args.Handled = contextRequestedEventArgs.Handled;
        }

        if (!args.Handled && !IsReadOnly && args.Source is Visual source)
        {
            // Check if click is inside the input area (not in popup)
            if (PickerPopup?.IsInsidePopup(source) != true)
            {
                var pointerPosition = args.GetPosition(this);
                var contentStartOffsetX = GetContentStartOffsetX();
                if (IsPointerInTextBox(InfoInputBox, pointerPosition, contentStartOffsetX))
                {
                    SetRangeActivatedPart(RangeActivatedPart.Start);
                    SetPickerOpenIfChanged(true);
                }
                else if (IsPointerInTextBox(SecondaryInfoInputBox, pointerPosition, contentStartOffsetX) ||
                         !ClickInClearUpButtonWithClearMode(args))
                {
                    SetRangeActivatedPart(RangeActivatedPart.End);
                    SetPickerOpenIfChanged(true);
                }
                else
                {
                    SetPickerOpenIfChanged(false);
                }
                args.Handled = true;
            }
        }
    }
    
    /// <summary>
    /// Arranges the content with the base implementation, places the range indicator at
    /// <see cref="PickerIndicatorOffsetX" /> and <see cref="PickerIndicatorOffsetY" />, and returns the base size
    /// inflated by the border thickness of the decorated box.
    /// </summary>
    protected override Size ArrangeOverride(Size finalSize)
    {
        var borderThickness = DecoratedBox?.BorderThickness ?? default;
        var size            = base.ArrangeOverride(finalSize).Inflate(borderThickness);
        if (RangePickerIndicator is not null)
        {
            if (!MathUtils.AreClose(Canvas.GetLeft(RangePickerIndicator), PickerIndicatorOffsetX))
            {
                Canvas.SetLeft(RangePickerIndicator, PickerIndicatorOffsetX);
            }
            if (!MathUtils.AreClose(Canvas.GetTop(RangePickerIndicator), PickerIndicatorOffsetY))
            {
                Canvas.SetTop(RangePickerIndicator, PickerIndicatorOffsetY);
            }
        }

        return size;
    }
    
    /// <summary>
    /// Measures the content with the base implementation and keeps the range indicator in step with the layout. The
    /// vertical offset is set to the bottom edge of the decorated box, that is its desired height minus the height
    /// of the indicator. While no part is active and the horizontal offset is still <c>double.NaN</c>, it is set to
    /// the left edge of the start input.
    /// </summary>
    protected override Size MeasureOverride(Size availableSize)
    {
        var size = base.MeasureOverride(availableSize);
        if (DecoratedBox is not null && RangePickerIndicator is not null)
        {
            var indicatorOffsetY = DecoratedBox.DesiredSize.Height - RangePickerIndicator.Height;
            if (!MathUtils.AreClose(PickerIndicatorOffsetY, indicatorOffsetY))
            {
                PickerIndicatorOffsetY = indicatorOffsetY;
            }
        }
    
        if (double.IsNaN(PickerIndicatorOffsetX))
        {
            if (_rangeActivatedPart == RangeActivatedPart.None)
            {
                var offset = InfoInputBox!.TranslatePoint(new Point(0, 0), this) ?? default;
                PickerIndicatorOffsetX = offset.X;
            }
        }
    
        return size;
    }
    
    /// <summary>
    /// Called when the picker popup is about to close, before <c>NotifyPickerClosed</c>: it deactivates the active
    /// part, which hides the range indicator. Call the base method from an override to deactivate the part.
    /// </summary>
    /// <param name="selectedIsValid">
    /// Whether the selection is valid. This implementation does not use it.
    /// </param>
    protected override void NotifyFlyoutAboutToClose(bool selectedIsValid)
    {
        SetRangeActivatedPart(RangeActivatedPart.None);
    }
    
    /// <summary>
    /// Called after the active part (<c>RangeActivatedPart</c>) changes. For the start part the picker popup is
    /// placed under the start input, aligned with its left edge; for the end part under the end input, aligned with
    /// its right edge. Then <see cref="SetupPickerIndicatorPosition" /> moves the range indicator under that input,
    /// or hides it when no part is active.
    /// </summary>
    protected virtual void NotifyRangeActivatedPartChanged()
    {
        if (RangeActivatedPart == RangeActivatedPart.Start)
        {
            SetPickerPlacement(PlacementMode.BottomEdgeAlignedLeft);
            SetPickerPopupPlacementTarget(InfoInputBox);
        }
        else if (RangeActivatedPart == RangeActivatedPart.End)
        {
            SetPickerPlacement(PlacementMode.BottomEdgeAlignedRight);
            SetPickerPopupPlacementTarget(SecondaryInfoInputBox);
        }

        SetupPickerIndicatorPosition();
    }
    
    /// <summary>
    /// Hides the range indicator while no part is active. Otherwise it shows the indicator, gives it the width of
    /// the input of the active part and moves it to that input's left edge. Does nothing while the indicator, the
    /// decorated box or one of the two inputs is missing.
    /// </summary>
    protected void SetupPickerIndicatorPosition()
    {
        if (RangePickerIndicator is null ||
            DecoratedBox is null ||
            InfoInputBox is null ||
            SecondaryInfoInputBox is null)
        {
            return;
        }
        
        if (_rangeActivatedPart == RangeActivatedPart.None)
        {
            SetRangePickerIndicatorOpacity(0);
        }
        else if (_rangeActivatedPart == RangeActivatedPart.Start)
        {
            SetRangePickerIndicatorOpacity(1);
            SetRangePickerIndicatorWidth(InfoInputBox.Bounds.Width);
            var offset = InfoInputBox.TranslatePoint(new Point(0, 0), this) ?? default;
            SetPickerIndicatorOffsetX(offset.X);
        }
        else if (_rangeActivatedPart == RangeActivatedPart.End)
        {
            SetRangePickerIndicatorOpacity(1);
            SetRangePickerIndicatorWidth(SecondaryInfoInputBox.Bounds.Width);
            var offset = SecondaryInfoInputBox.TranslatePoint(new Point(0, 0), this) ?? default;
            SetPickerIndicatorOffsetX(offset.X);
        }
    }

    private void SetRangePickerIndicatorOpacity(double opacity)
    {
        if (!MathUtils.AreClose(RangePickerIndicatorOpacity, opacity))
        {
            RangePickerIndicatorOpacity = opacity;
        }
    }

    private void SetRangePickerIndicatorWidth(double width)
    {
        if (RangePickerIndicator is not null && !MathUtils.AreClose(RangePickerIndicator.Width, width))
        {
            RangePickerIndicator.Width = width;
        }
    }

    private void SetPickerIndicatorOffsetX(double offsetX)
    {
        if (!MathUtils.AreClose(PickerIndicatorOffsetX, offsetX))
        {
            PickerIndicatorOffsetX = offsetX;
        }
    }

    private void SetRangeActivatedPart(RangeActivatedPart rangeActivatedPart)
    {
        if (RangeActivatedPart != rangeActivatedPart)
        {
            RangeActivatedPart = rangeActivatedPart;
        }
    }

    private void SetPickerPlacement(PlacementMode placement)
    {
        if (PickerPlacement != placement)
        {
            PickerPlacement = placement;
        }
    }

    private void SetPickerPopupPlacementTarget(Control? placementTarget)
    {
        if (PickerPopup != null && !ReferenceEquals(PickerPopup.PlacementTarget, placementTarget))
        {
            PickerPopup.PlacementTarget = placementTarget;
        }
    }
    
    /// <summary>
    /// Shows the clear button only when <c>IsClearButtonRevealed</c> is set, both inputs are editable (not
    /// read-only) and both have text; otherwise hides it. Nothing changes while the decorated box is missing. The
    /// base implementation is not called.
    /// </summary>
    protected override void ConfigureIsClearButtonVisible()
    {
        if (DecoratedBox is not null)
        {
            SetClearButtonVisibleIfChanged(
                IsClearButtonRevealed &&
                InfoInputBox?.IsReadOnly == false &&
                InfoInputBox.Text?.Length > 0 &&
                SecondaryInfoInputBox?.IsReadOnly == false &&
                SecondaryInfoInputBox?.Text?.Length > 0);
        }
    }

}

internal enum RangeActivatedPart
{
    None,
    Start,
    End
}
