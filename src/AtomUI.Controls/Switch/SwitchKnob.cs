using System.Globalization;
using AtomUI.Animations;
using AtomUI.Controls.Commons;
using AtomUI.Media;
using AtomUI.Theme.Resources;
using AtomUI.Utils;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.VisualTree;

namespace AtomUI.Controls;

internal class SwitchKnob : TemplatedControl
{
    #region 公共属性定义
    
    public static readonly StyledProperty<bool> IsCheckedStateProperty = 
        AvaloniaProperty.Register<SwitchKnob, bool>(nameof(IsCheckedState));
    
    public static readonly StyledProperty<BoxShadow?> KnobBoxShadowProperty = 
        AvaloniaProperty.Register<SwitchKnob, BoxShadow?>(nameof(KnobBoxShadow));
    
    public bool IsCheckedState
    {
        get => GetValue(IsCheckedStateProperty);
        set => SetValue(IsCheckedStateProperty, value);
    }

    public BoxShadow? KnobBoxShadow
    {
        get => GetValue(KnobBoxShadowProperty);
        set => SetValue(KnobBoxShadowProperty, value);
    }

    #endregion
    
    #region 内部属性定义

    internal static readonly StyledProperty<int> RotationProperty =
        AvaloniaProperty.Register<SwitchKnob, int>(nameof(Rotation));
    
    internal static readonly StyledProperty<IBrush?> LoadIndicatorBrushProperty = 
        AvaloniaProperty.Register<SwitchKnob, IBrush?>(nameof(LoadIndicatorBrush));
    
    internal static readonly StyledProperty<Size> KnobSizeProperty =
        AvaloniaProperty.Register<SwitchKnob, Size>(nameof(KnobSize));
    
    internal static readonly StyledProperty<double> KnobWidthExtensionProperty =
        AvaloniaProperty.Register<SwitchKnob, double>(nameof(KnobWidthExtension));
    
    internal static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<SwitchKnob>();
    
    /// <summary>
    /// Identifies the <see cref="LoadIndicatorThickness" /> property.
    /// </summary>
    internal static readonly StyledProperty<double> LoadIndicatorThicknessProperty =
        AvaloniaProperty.Register<SwitchKnob, double>(nameof(LoadIndicatorThickness));

    internal static readonly StyledProperty<TimeSpan> LoadingAnimationDurationProperty =
        AvaloniaProperty.Register<SwitchKnob, TimeSpan>(nameof(LoadingAnimationDuration));

    internal static readonly DirectProperty<SwitchKnob, double> LoadingBgOpacityProperty =
        AvaloniaProperty.RegisterDirect<SwitchKnob, double>(nameof(LoadingBgOpacity),
            o => o.LoadingBgOpacity,
            (o, v) => o.LoadingBgOpacity = v);

    internal static readonly StyledProperty<SwitchLoadingIndicatorTokenResourceKeys?> LoadingIndicatorTokenResourceKeysProperty =
        AvaloniaProperty.Register<SwitchKnob, SwitchLoadingIndicatorTokenResourceKeys?>(nameof(LoadingIndicatorTokenResourceKeys));

    internal int Rotation
    {
        get => GetValue(RotationProperty);
        set => SetValue(RotationProperty, value);
    }
    
    /// <summary>
    /// Brush of the loading arc that the knob draws while it is loading. The switch theme sets it on the knob part
    /// from the <c>OffStateLoadIndicatorColor</c> Token and, while the switch is checked, from the
    /// <c>SwitchColor</c> Token. The default is <see langword="null" />.
    /// </summary>
    internal IBrush? LoadIndicatorBrush
    {
        get => GetValue(LoadIndicatorBrushProperty);
        set => SetValue(LoadIndicatorBrushProperty, value);
    }
    
    /// <summary>
    /// Size of the knob, in DIPs. The knob reports it as its desired size; its painted height is the height it is
    /// arranged with. The switch copies its own <c>KnobSize</c> to the knob after the template is applied and
    /// whenever that value changes; its theme takes that value from the <c>HandleSizeSM</c> Token for the small size
    /// and from the <c>HandleSize</c> Token for the other sizes.
    /// </summary>
    internal Size KnobSize
    {
        get => GetValue(KnobSizeProperty);
        set => SetValue(KnobSizeProperty, value);
    }
    
    /// <summary>
    /// Extra width of the knob, in DIPs, on top of the width of <see cref="KnobSize" />. The switch sets it to the
    /// difference between the stretched handle width and the knob width while it is pressed and press stretch is
    /// enabled, and to <c>0</c> otherwise; its template forwards the value to the knob. The knob paints a circle
    /// when its extended width equals its arranged height and a pill otherwise.
    /// </summary>
    internal double KnobWidthExtension
    {
        get => GetValue(KnobWidthExtensionProperty);
        set => SetValue(KnobWidthExtensionProperty, value);
    }
    
    /// <summary>
    /// Whether the knob rotates the loading arc while it is loading; <see langword="true" /> by default. The
    /// rotation runs while this is <see langword="true" />, the knob is attached to a visual tree and visible with
    /// every visual ancestor, the effective <see cref="MotionActivity" /> source is active and
    /// <see cref="LoadingAnimationDuration" /> is positive; setting this to <see langword="false" /> stops a running
    /// rotation. The switch template forwards the value of the switch to the knob.
    /// </summary>
    internal bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }
    
    /// <summary>
    /// Stroke width of the loading arc, in DIPs. An explicitly set value is used as it is. Without one, the
    /// thickness of the Token that <see cref="LoadingIndicatorTokenResourceKeys" /> names is used while the knob
    /// is loading and the keys are set; otherwise the raw value applies, <c>0</c> by default.
    /// </summary>
    internal double LoadIndicatorThickness
    {
        get => IsSet(LoadIndicatorThicknessProperty) || _loadingPaintTokens is null
            ? GetValue(LoadIndicatorThicknessProperty)
            : _loadingPaint.Thickness;
        set => SetValue(LoadIndicatorThicknessProperty, value);
    }

    /// <summary>
    /// Duration of one turn of the loading arc. A change restarts a running rotation with the new duration.
    /// <see cref="TimeSpan.Zero" />, the default, keeps the arc from rotating. The knob theme sets it from the
    /// <c>LoadingAnimationDuration</c> Token.
    /// </summary>
    internal TimeSpan LoadingAnimationDuration
    {
        get => GetValue(LoadingAnimationDurationProperty);
        set => SetValue(LoadingAnimationDurationProperty, value);
    }
    
    private double _loadingBgOpacity;
    internal double LoadingBgOpacity
    {
        get => _loadingPaintTokens is null ? _loadingBgOpacity : _loadingPaint.Opacity;
        set => SetAndRaise(LoadingBgOpacityProperty, ref _loadingBgOpacity, value);
    }

    /// <summary>
    /// The resource keys of the two Tokens that style the loading arc, its opacity and its stroke thickness. While
    /// the knob is loading and this is set, the knob subscribes to both Tokens, <see cref="LoadingBgOpacity" />
    /// returns the opacity of its Token and <see cref="LoadIndicatorThickness" /> returns the thickness of its Token
    /// unless a value is set explicitly. <see langword="null" /> removes the subscription. The knob theme assigns
    /// <c>ToggleSwitchResources.LoadingIndicatorTokenResourceKeys</c>.
    /// </summary>
    internal SwitchLoadingIndicatorTokenResourceKeys? LoadingIndicatorTokenResourceKeys
    {
        get => GetValue(LoadingIndicatorTokenResourceKeysProperty);
        set => SetValue(LoadingIndicatorTokenResourceKeysProperty, value);
    }
    
    #endregion
    
    private bool _isLoading;
    private IDisposable? _loadingAnimation;
    private MotionGate? _loadingMotionGate;
    private OwnerTokenSet? _loadingPaintTokens;
    private (double Opacity, double Thickness) _loadingPaint;
    private IBrush? _loadIndicatorPenBrush;
    private double _loadIndicatorPenThickness;
    private Pen? _loadIndicatorPen;
    
    static SwitchKnob()
    {
        AffectsRender<SwitchKnob>(
            BackgroundProperty, RotationProperty, KnobSizeProperty, KnobWidthExtensionProperty, LoadIndicatorBrushProperty,
            LoadIndicatorThicknessProperty);
        AffectsMeasure<SwitchKnob>(KnobSizeProperty);
        IsMotionEnabledProperty.OverrideDefaultValue<SwitchKnob>(true);
    }
    
    public SwitchKnob()
    {
        UseLayoutRounding = false;
    }

    public void NotifyStartLoading()
    {
        if (_isLoading)
        {
            return;
        }

        _isLoading = true;
        IsEnabled  = false;
        if (this.IsAttachedToVisualTree())
        {
            UpdateLoadingPaintTokenSubscription();
            AttachLoadingGate();
        }
    }

    public void NotifyStopLoading()
    {
        if (!_isLoading)
        {
            return;
        }

        _isLoading = false;
        IsEnabled = true;
        UpdateLoadingPaintTokenSubscription();
        RemoveLoadingGate();
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        if (_isLoading)
        {
            UpdateLoadingPaintTokenSubscription();
            AttachLoadingGate();
        }
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        ReleaseLoadingPaintTokenSubscription();
        base.OnDetachedFromVisualTree(e);
        RemoveLoadingGate();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == LoadingIndicatorTokenResourceKeysProperty && _isLoading && this.IsAttachedToVisualTree())
        {
            UpdateLoadingPaintTokenSubscription();
        }
        else if (change.Property == LoadingAnimationDurationProperty && _loadingMotionGate?.ShouldRun == true)
        {
            StartLoadingAnimation();
        }
        else if (change.Property == IsMotionEnabledProperty && _loadingMotionGate is { } gate)
        {
            gate.IsMotionEnabled = IsMotionEnabled;
            if (!IsMotionEnabled)
            {
                StopLoadingAnimation();
            }
        }
        else if (change.Property == KnobBoxShadowProperty)
        {
            if (KnobBoxShadow != null)
            {
                // The derived value is written at Style priority into the current value frame. A user style frame added
                // later at the same priority is evaluated later, so a semantic style can override the knob shadow that
                // the theme derived.
                SetValue(EffectProperty, (IEffect?)new DropShadowEffect
                {
                    OffsetX    = KnobBoxShadow.Value.OffsetX,
                    OffsetY    = KnobBoxShadow.Value.OffsetY,
                    Color      = KnobBoxShadow.Value.Color,
                    BlurRadius = KnobBoxShadow.Value.Blur
                }, BindingPriority.Style);
            }
            else
            {
                SetValue(EffectProperty, (IEffect?)null, BindingPriority.Style);
            }
        }
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        return KnobSize;
    }

    public sealed override void Render(DrawingContext context)
    {
        var renderWidth = KnobSize.Width + KnobWidthExtension;
        var targetRect  = new Rect(0, 0, renderWidth, Bounds.Height);
        if (MathUtils.AreClose(renderWidth, targetRect.Height))
        {
            context.DrawEllipse(Background, null, targetRect);
        }
        else
        {
            context.DrawPilledRect(Background, null, targetRect);
        }

        if (_isLoading)
        {
            var delta           = 2.5;
            var loadingRectSize = new Size(KnobSize.Width, targetRect.Height).Deflate(new Thickness(delta));
            var loadingRect = new Rect(new Point(-loadingRectSize.Width / 2, -loadingRectSize.Height / 2),
                loadingRectSize);
            var       pen                     = GetLoadIndicatorPen();
            var       translateToCenterMatrix = Matrix.CreateTranslation(targetRect.Center.X, targetRect.Center.Y);
            var       rotationMatrix          = Matrix.CreateRotation(Rotation * Math.PI / 180);
            using var translateToCenterState  = context.PushTransform(translateToCenterMatrix);
            using var rotationMatrixState     = context.PushTransform(rotationMatrix);
            using var bgOpacity               = context.PushOpacity(LoadingBgOpacity);

            context.DrawArc(pen, loadingRect, 0, 90);
        }
    }

    private void UpdateLoadingPaintTokenSubscription()
    {
        ReleaseLoadingPaintTokenSubscription();
        if (_isLoading && LoadingIndicatorTokenResourceKeys is { } keys)
        {
            _loadingPaintTokens = new OwnerTokenSet(this, 2, RefreshLoadingPaint);
            _loadingPaintTokens.SetResourceKey(0, keys.Opacity);
            _loadingPaintTokens.SetResourceKey(1, keys.Thickness);
        }
        RefreshLoadingPaint();
    }

    private void RefreshLoadingPaint()
    {
        _loadingPaint = (
            ReadNumber(_loadingPaintTokens?.GetValue(0), _loadingPaint.Opacity),
            ReadNumber(_loadingPaintTokens?.GetValue(1), _loadingPaint.Thickness));
        InvalidateVisual();
    }

    private void ReleaseLoadingPaintTokenSubscription()
    {
        _loadingPaintTokens?.Dispose();
        _loadingPaintTokens = null;
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

    private void AttachLoadingGate()
    {
        _loadingMotionGate ??= new MotionGate(this, HandleLoadingGateChanged) { IsMotionEnabled = IsMotionEnabled };
        if (_loadingMotionGate.ShouldRun && IsMotionEnabled)
        {
            StartLoadingAnimation();
        }
    }

    private void RemoveLoadingGate()
    {
        _loadingMotionGate?.Dispose();
        _loadingMotionGate = null;
        StopLoadingAnimation();
    }

    private void HandleLoadingGateChanged(bool animate)
    {
        if (animate && _loadingMotionGate is not null && _isLoading && IsMotionEnabled)
        {
            StartLoadingAnimation();
        }
        else
        {
            StopLoadingAnimation();
        }
    }

    private void StopLoadingAnimation()
    {
        var animation = _loadingAnimation;
        _loadingAnimation = null;
        animation?.Dispose();
    }

    private void StartLoadingAnimation()
    {
        StopLoadingAnimation();
        if (!_isLoading || !IsMotionEnabled || !this.IsAttachedToVisualTree() || LoadingAnimationDuration <= TimeSpan.Zero)
        {
            return;
        }
        var loadingAnimationDuration = LoadingAnimationDuration;
        var loadingAnimation = new Animation();
        loadingAnimation.Duration                     = loadingAnimationDuration;
        loadingAnimation.Children.Add(new KeyFrame
        {
            Setters =
            {
                new Setter
                {
                    Property = RotationProperty,
                    Value    = 0
                }
            },
            KeyTime = TimeSpan.FromMilliseconds(0)
        });
        loadingAnimation.Children.Add(new KeyFrame
        {
            Setters =
            {
                new Setter
                {
                    Property = RotationProperty,
                    Value    = 360
                }
            },
            KeyTime = loadingAnimationDuration
        });
        // Duration is a runtime snapshot and loading has explicit start/stop
        // ownership; putting this in ControlTheme would require duplicate state.
        var subscription = loadingAnimation.PlayLoop(this, RotationProperty);
        if (_isLoading && this.IsAttachedToVisualTree() && _loadingAnimation is null)
        {
            _loadingAnimation = subscription;
        }
        else
        {
            subscription.Dispose();
        }
    }

    private Pen GetLoadIndicatorPen()
    {
        var brush     = LoadIndicatorBrush;
        var thickness = LoadIndicatorThickness;
        if (_loadIndicatorPen == null || !ReferenceEquals(_loadIndicatorPenBrush, brush) ||
            !MathUtils.AreClose(_loadIndicatorPenThickness, thickness))
        {
            _loadIndicatorPenBrush     = brush;
            _loadIndicatorPenThickness = thickness;
            _loadIndicatorPen          = new Pen(brush, thickness, null, PenLineCap.Round);
        }
        return _loadIndicatorPen;
    }
}

/// <summary>
/// The resource keys of the Tokens that style the loading indicator of a switch: its opacity and its stroke
/// thickness; <c>LoadingIndicatorTokenResourceKeys</c> of the switch in AtomUI.Desktop.Controls exposes them.
/// </summary>
/// <remarks>
/// Each component is a Token resource key, not a value.
/// </remarks>
internal sealed record SwitchLoadingIndicatorTokenResourceKeys(object Opacity, object Thickness);
