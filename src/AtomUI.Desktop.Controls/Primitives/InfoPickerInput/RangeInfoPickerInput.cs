using AtomUI.Animations;
using AtomUI.Utils;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace AtomUI.Desktop.Controls.Primitives;

public abstract class RangeInfoPickerInput : InfoPickerInput
{
    #region 公共属性定义

    public static readonly StyledProperty<string?> SecondaryPlaceholderTextProperty =
        AvaloniaProperty.Register<RangeInfoPickerInput, string?>(nameof(SecondaryPlaceholderText));
    
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

    public override void Clear()
    {
        InfoInputBox?.Clear();
        SecondaryInfoInputBox?.Clear();
    }
    

    protected RangeInfoPickerInput()
    {
        _transitionInitialization = new TransitionInitializationSession(this);
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        _transitionInitialization.CompleteInitialization();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _transitionInitialization.Reset();
        base.OnDetachedFromVisualTree(e);
    }

    protected override void OnTemplateChanged(AvaloniaPropertyChangedEventArgs e)
    {
        _transitionInitialization?.Reset();
        base.OnTemplateChanged(e);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        SecondaryInfoInputBox = e.NameScope.Get<TextBox>("PART_SecondaryInfoInputBox");
        RangePickerIndicator  = e.NameScope.Get<Rectangle>("PART_RangePickerIndicator");
        RangePickerArrow = e.NameScope.Get<PathIcon>("PART_RangePickerArrow");
        if (PickerPopup != null)
        {
            PickerPopup.OverlayInputPassThroughElement = DecoratedBox;
        }
        _transitionInitialization.CompleteInitialization();
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

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == RangeActivatedPartProperty)
        {
            NotifyRangeActivatedPartChanged();
        }
    }
    
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
    
    protected override void NotifyFlyoutAboutToClose(bool selectedIsValid)
    {
        SetRangeActivatedPart(RangeActivatedPart.None);
    }
    
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
    
    protected override void ConfigureIsClearButtonVisible()
    {
        if (DecoratedBox is not null)
        {
            SetClearButtonVisibleIfChanged(
                DecoratedBox.IsInnerBoxHover &&
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
