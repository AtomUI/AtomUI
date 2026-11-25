using System.Globalization;
using AtomUI.Animations;
using AtomUI.Controls.Primitives;
using AtomUI.Media;
using AtomUI.Theme.Resources;
using AtomUI.Utils;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.VisualTree;
using Avalonia.Threading;

namespace AtomUI.Controls.Commons;

internal class RadioIndicator : TemplatedControl, IFocusOutlineShapeProvider
{
    #region 公共属性定义

    public static readonly StyledProperty<bool?> IsCheckedProperty =
        ToggleButton.IsCheckedProperty.AddOwner<RadioIndicator>();

    public static readonly StyledProperty<double> PaddingInlineProperty =
        AvaloniaProperty.Register<RadioIndicator, double>(nameof(PaddingInline));

    public static readonly StyledProperty<IBrush?> RadioInnerBackgroundProperty =
        AvaloniaProperty.Register<RadioIndicator, IBrush?>(nameof(RadioInnerBackground));

    public static readonly StyledProperty<double> RadioDotEffectSizeProperty =
        AvaloniaProperty.Register<RadioIndicator, double>(nameof(RadioDotEffectSize));

    public bool? IsChecked
    {
        get => GetValue(IsCheckedProperty);
        set => SetValue(IsCheckedProperty, value);
    }

    public double PaddingInline
    {
        get => GetValue(PaddingInlineProperty);
        set => SetValue(PaddingInlineProperty, value);
    }

    public IBrush? RadioInnerBackground
    {
        get => GetValue(RadioInnerBackgroundProperty);
        set => SetValue(RadioInnerBackgroundProperty, value);
    }

    public double RadioDotEffectSize
    {
        get => GetValue(RadioDotEffectSizeProperty);
        set => SetValue(RadioDotEffectSizeProperty, value);
    }

    #endregion

    #region 内部属性定义

    internal static readonly StyledProperty<RadioDotGeometryTokenResourceKeys?> DotGeometryTokenResourceKeysProperty =
        AvaloniaProperty.Register<RadioIndicator, RadioDotGeometryTokenResourceKeys?>(nameof(DotGeometryTokenResourceKeys));

    internal static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<RadioIndicator>();

    internal static readonly StyledProperty<bool> IsWaveSpiritEnabledProperty =
        WaveSpiritAwareControlProperty.IsWaveSpiritEnabledProperty.AddOwner<RadioIndicator>();

    internal RadioDotGeometryTokenResourceKeys? DotGeometryTokenResourceKeys
    {
        get => GetValue(DotGeometryTokenResourceKeysProperty);
        set => SetValue(DotGeometryTokenResourceKeysProperty, value);
    }

    internal bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }

    internal bool IsWaveSpiritEnabled
    {
        get => GetValue(IsWaveSpiritEnabledProperty);
        set => SetValue(IsWaveSpiritEnabledProperty, value);
    }
    
    #endregion

    private IPen? _cachedPen;
    private WaveSpiritDecorator? _waveSpiritDecorator;
    private DispatcherOperation? _pendingWave;
    private OwnerTokenSet? _geometryTokens;
    private (double RadioSize, double DotSize, double DotPadding) _geometry;
    
    private readonly TransitionInitializationSession _transitionInitialization;

    static RadioIndicator()
    {
        AffectsRender<RadioIndicator>(
            IsCheckedProperty,
            BackgroundProperty,
            BorderBrushProperty,
            BorderThicknessProperty,
            CornerRadiusProperty,
            RadioInnerBackgroundProperty,
            RadioDotEffectSizeProperty,
            UseLayoutRoundingProperty);
    }

    public RadioIndicator()
    {
        _transitionInitialization = new TransitionInitializationSession(this);
    }

    protected override void OnTemplateChanged(AvaloniaPropertyChangedEventArgs e)
    {
        _pendingWave?.Abort();
        _pendingWave = null;
        _transitionInitialization?.Reset();
        base.OnTemplateChanged(e);
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _geometryTokens?.Dispose();
        _geometryTokens = null;
        _pendingWave?.Abort();
        _pendingWave = null;
        _transitionInitialization.Reset();
        base.OnDetachedFromVisualTree(e);
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        _transitionInitialization.CompleteInitialization();
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        UpdateGeometryTokenSubscription();
        UpdatePseudoClasses();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == IsPointerOverProperty ||
            change.Property == IsCheckedProperty ||
            change.Property == IsEnabledProperty)
        {
            UpdatePseudoClasses();

            if (change.Property == IsCheckedProperty)
            {
                _pendingWave?.Abort();
                _pendingWave = null;
            }

            if (change.Property == IsCheckedProperty &&
                _transitionInitialization.IsReady &&
                IsMotionEnabled &&
                IsWaveSpiritEnabled &&
                !PseudoClasses.Contains(StdPseudoClass.Disabled) &&
                PseudoClasses.Contains(StdPseudoClass.Checked))
            {
                _pendingWave = Dispatcher.InvokeAsync(PlayWaveSpirit);
            }
        }

        if (this.IsAttachedToVisualTree())
        {
            if (change.Property == DotGeometryTokenResourceKeysProperty)
            {
                UpdateGeometryTokenSubscription();
            }
            else if (change.Property == IsPointerOverProperty || change.Property == IsCheckedProperty ||
                     change.Property == IsEnabledProperty)
            {
                RadioDotEffectSize = CalculateDotSize(IsEnabled, IsChecked == true);
            }
        }
    }

    private void UpdateGeometryTokenSubscription()
    {
        _geometryTokens?.Dispose();
        _geometryTokens = null;
        if (DotGeometryTokenResourceKeys is { } keys)
        {
            _geometryTokens = new OwnerTokenSet(this, 3, RefreshGeometry);
            _geometryTokens.SetResourceKey(0, keys.RadioSize);
            _geometryTokens.SetResourceKey(1, keys.DotSize);
            _geometryTokens.SetResourceKey(2, keys.DotPadding);
        }
        RefreshGeometry();
    }

    private void RefreshGeometry()
    {
        _geometry = (
            ReadNumber(_geometryTokens?.GetValue(0), _geometry.RadioSize),
            ReadNumber(_geometryTokens?.GetValue(1), _geometry.DotSize),
            ReadNumber(_geometryTokens?.GetValue(2), _geometry.DotPadding));
        RadioDotEffectSize = CalculateDotSize(IsEnabled, IsChecked == true);
    }

    private static double ReadNumber(object? value, double previous)
    {
        if (value is double number)
        {
            return number;
        }
        if (ReferenceEquals(value, BindingOperations.DoNothing))
        {
            return previous;
        }
        try
        {
            return value is IConvertible convertible ? convertible.ToDouble(CultureInfo.CurrentCulture) : 0;
        }
        catch (Exception error) when (error is FormatException or InvalidCastException or OverflowException)
        {
            return 0;
        }
    }

    private void UpdatePseudoClasses()
    {
        PseudoClasses.Set(StdPseudoClass.Checked, IsChecked.HasValue && IsChecked.Value);
        PseudoClasses.Set(StdPseudoClass.UnChecked, (IsChecked.HasValue && !IsChecked.Value) || !IsChecked.HasValue);
    }

    private void PlayWaveSpirit()
    {
        _pendingWave = null;
        if (!_transitionInitialization.IsReady || !IsMotionEnabled || !IsWaveSpiritEnabled ||
            !IsEnabled || IsChecked != true || _waveSpiritDecorator is null)
        {
            return;
        }

        var waveBrush = WaveSpiritDecorator.ResolveWaveSpiritBrush(BorderBrush, Background);
        if (waveBrush is not null)
        {
            _waveSpiritDecorator.WaveBrush = waveBrush;
        }
        else
        {
            _waveSpiritDecorator.ClearValue(WaveSpiritDecorator.WaveBrushProperty);
        }

        _waveSpiritDecorator.Play();
    }

    private double CalculateDotSize(bool isEnabled, bool isChecked)
    {
        double targetValue;
        if (isChecked)
        {
            if (isEnabled)
            {
                targetValue = _geometry.DotSize;
            }
            else
            {
                targetValue = _geometry.RadioSize - _geometry.DotPadding * 2;
            }
        }
        else
        {
            targetValue = _geometry.DotSize * 0.6;
        }

        return targetValue;
    }

    // Without an explicit CornerRadius the indicator renders as a circle.
    CornerRadius IFocusOutlineShapeProvider.FocusOutlineCornerRadius =>
        IsSet(CornerRadiusProperty) ? CornerRadius : new CornerRadius(Math.Min(Bounds.Width, Bounds.Height) / 2);

    public sealed override void Render(DrawingContext context)
    {
        var borderThickness = BorderUtils.BuildRenderScaleAwareThickness(this, BorderThickness);
        var penWidth        = borderThickness.Top;
        PenUtils.TryModifyOrCreate(ref _cachedPen, BorderBrush, penWidth);
        var targetRect  = new Rect(0, 0, Bounds.Width, Bounds.Height);
        var borderRect  = targetRect.Deflate(penWidth / 2);
        var roundedRect = IsSet(CornerRadiusProperty)
            ? new RoundedRect(borderRect, CornerRadius)
            : new RoundedRect(borderRect, Math.Min(borderRect.Width, borderRect.Height) / 2);
        context.DrawRectangle(Background, _cachedPen, roundedRect);
        if (IsChecked.HasValue && IsChecked.Value)
        {
            var dotDiameter = RadioDotEffectSize / 2;
            context.DrawEllipse(RadioInnerBackground, null, targetRect.Center, dotDiameter, dotDiameter);
        }
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _waveSpiritDecorator = e.NameScope.Find<WaveSpiritDecorator>(WaveSpiritDecorator.WaveSpiritPart);
        _transitionInitialization.CompleteInitialization();
    }

}

internal sealed record RadioDotGeometryTokenResourceKeys(object RadioSize, object DotSize, object DotPadding);
