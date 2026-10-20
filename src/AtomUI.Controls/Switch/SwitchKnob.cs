using AtomUI.Animations;
using AtomUI.Media;
using AtomUI.Utils;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
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
    
    internal static readonly StyledProperty<double> KnobRenderWidthProperty =
        AvaloniaProperty.Register<SwitchKnob, double>(nameof(KnobRenderWidth));
    
    internal static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<SwitchKnob>();
    
    internal static readonly StyledProperty<TimeSpan> LoadingAnimationDurationProperty =
        AvaloniaProperty.Register<SwitchKnob, TimeSpan>(nameof(LoadingAnimationDuration));

    internal static readonly DirectProperty<SwitchKnob, double> LoadingBgOpacityProperty =
        AvaloniaProperty.RegisterDirect<SwitchKnob, double>(nameof(LoadingBgOpacity),
            o => o.LoadingBgOpacity,
            (o, v) => o.LoadingBgOpacity = v);

    internal int Rotation
    {
        get => GetValue(RotationProperty);
        set => SetValue(RotationProperty, value);
    }
    
    internal IBrush? LoadIndicatorBrush
    {
        get => GetValue(LoadIndicatorBrushProperty);
        set => SetValue(LoadIndicatorBrushProperty, value);
    }
    
    internal Size KnobSize
    {
        get => GetValue(KnobSizeProperty);
        set => SetValue(KnobSizeProperty, value);
    }
    
    internal double KnobRenderWidth
    {
        get => GetValue(KnobRenderWidthProperty);
        set => SetValue(KnobRenderWidthProperty, value);
    }
    
    internal bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }
    
    internal TimeSpan LoadingAnimationDuration
    {
        get => GetValue(LoadingAnimationDurationProperty);
        set => SetValue(LoadingAnimationDurationProperty, value);
    }
    
    private double _loadingBgOpacity;
    internal double LoadingBgOpacity
    {
        get => _loadingBgOpacity;
        set => SetAndRaise(LoadingBgOpacityProperty, ref _loadingBgOpacity, value);
    }
    
    #endregion
    
    private bool _isLoading;
    private CancellationTokenSource? _cancellationTokenSource;
    private IBrush? _loadIndicatorPenBrush;
    private Pen? _loadIndicatorPen;
    
    static SwitchKnob()
    {
        AffectsRender<SwitchKnob>(
            BackgroundProperty, RotationProperty, KnobRenderWidthProperty, LoadIndicatorBrushProperty);
        AffectsMeasure<SwitchKnob>(KnobSizeProperty);
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
            StartLoadingAnimation();
        }
    }

    public void NotifyStopLoading()
    {
        if (!_isLoading)
        {
            return;
        }

        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();
        _cancellationTokenSource = null;
        _isLoading               = false;
        IsEnabled                = true;
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        if (_isLoading)
        {
            StartLoadingAnimation();
        }
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        if (_isLoading)
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;
        }
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == KnobSizeProperty)
        {
            KnobRenderWidth = KnobSize.Width;
        }
        else if (change.Property == KnobBoxShadowProperty)
        {
            if (KnobBoxShadow != null)
            {
                // 派生效值以 Style 优先级写入即时帧；同优先级下用户样式帧后添加、后求值，
                // 因此语义样式可以覆盖主题派生的把手阴影
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
        var offsetX = 0d;
        var offsetY = 0d;
        if (IsCheckedState)
        {
            offsetX = Bounds.Width - KnobRenderWidth;
        }

        var targetRect = new Rect(offsetX, offsetY, KnobRenderWidth, Bounds.Height);
        if (MathUtils.AreClose(KnobRenderWidth, DesiredSize.Height))
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
            var loadingRectSize = targetRect.Size.Deflate(new Thickness(delta));
            var loadingRect = new Rect(new Point(-loadingRectSize.Width / 2, -loadingRectSize.Height / 2),
                loadingRectSize);
            var       pen                     = GetLoadIndicatorPen();
            var       translateToCenterMatrix = Matrix.CreateTranslation(targetRect.Center.X, targetRect.Center.Y);
            var       rotationMatrix          = Matrix.CreateRotation(Rotation * Math.PI / 180);
            using var translateToCenterState  = context.PushTransform(translateToCenterMatrix);
            using var rotationMatrixState     = context.PushTransform(rotationMatrix);
            using var bgOpacity               = context.PushOpacity(_loadingBgOpacity);

            context.DrawArc(pen, loadingRect, 0, 90);
        }
    }

    private void StartLoadingAnimation()
    {
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();
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
        _cancellationTokenSource = new CancellationTokenSource();
        var token = _cancellationTokenSource.Token;
        Dispatcher.UIThread.InvokeAsync(async () => await loadingAnimation.RunInfiniteAsync(this, token));
    }

    private Pen GetLoadIndicatorPen()
    {
        var brush = LoadIndicatorBrush;
        if (_loadIndicatorPen == null || !ReferenceEquals(_loadIndicatorPenBrush, brush))
        {
            _loadIndicatorPenBrush = brush;
            _loadIndicatorPen      = new Pen(brush, 1, null, PenLineCap.Round);
        }
        return _loadIndicatorPen;
    }
}
