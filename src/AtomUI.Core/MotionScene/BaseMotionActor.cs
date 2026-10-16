using System.Diagnostics;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Transformation;

namespace AtomUI.MotionScene;

public abstract class BaseMotionActor : ContentControl, IMotionActor
{
    public const string MotionActorPart = "PART_MotionActor";
    
    #region 公共属性定义

    public static readonly StyledProperty<ITransform?> MotionTransformProperty =
        MotionActorControlProperty.MotionTransformProperty.AddOwner<BaseMotionActor>();
    
    public static readonly StyledProperty<TransformOperations?> MotionTransformOperationsProperty =
        MotionActorControlProperty.MotionTransformOperationsProperty.AddOwner<BaseMotionActor>();

    public ITransform? MotionTransform
    {
        get => GetValue(MotionTransformProperty);
        set => SetValue(MotionTransformProperty, value);
    }
    
    public TransformOperations? MotionTransformOperations
    {
        get => GetValue(MotionTransformOperationsProperty);
        set => SetValue(MotionTransformOperationsProperty, value);
    }
    
    public Control? MotionTransformRoot => Presenter;
    
    public bool IsFollowMode() => _followTarget != null;

    #endregion

    #region 公共事件定义
    
    public static readonly RoutedEvent<RoutedEventArgs> PreStartEvent = 
        RoutedEvent.Register<MenuItem, RoutedEventArgs>(nameof(PreStart), RoutingStrategies.Bubble);
    
    public static readonly RoutedEvent<RoutedEventArgs> CompletedEvent = 
        RoutedEvent.Register<MenuItem, RoutedEventArgs>(nameof(Completed), RoutingStrategies.Bubble);

    public event EventHandler<RoutedEventArgs>? PreStart
    {
        add => AddHandler(PreStartEvent, value);
        remove => RemoveHandler(PreStartEvent, value);
    }
 
    public event EventHandler<RoutedEventArgs>? Completed
    {
        add => AddHandler(CompletedEvent, value);
        remove => RemoveHandler(CompletedEvent, value);
    }

    #endregion

    internal IMotionActorLayout? MotionLayout { get; set; }

    /// <summary>
    /// RenderTransform/MatrixTransform applied to MotionTransformRoot.
    /// </summary>
    protected readonly MatrixTransform MatrixTransform = new();

    /// <summary>
    /// Transformation matrix corresponding to _matrixTransform.
    /// </summary>
    protected Matrix Transformation = Matrix.Identity;

    private IDisposable? _transformChangedEventDisposable;
    private Transform? _observedTransform;
    private bool _detachedAfterAttachment;

    /// <summary>
    /// Number of decimals to round the Matrix to.
    /// </summary>
    protected const int DecimalsAfterRound = 4;

    /// <summary>
    /// 动画是否在
    /// </summary>
    internal bool Animating = false;

    private BaseMotionActor? _followTarget;
    private IDisposable? _followBindings;

    internal AbstractMotion.MotionRun? ActiveMotionRun;

    static BaseMotionActor()
    {
        ClipToBoundsProperty.OverrideDefaultValue<BaseMotionActor>(true);

        MotionTransformProperty.Changed
                               .AddClassHandler<BaseMotionActor>((x, _) => x.HandleTransformSourceChanged());
        
        MotionTransformOperationsProperty.Changed
                                         .AddClassHandler<BaseMotionActor>((x, _) => x.HandleTransformSourceChanged());

        ContentProperty.Changed
                       .AddClassHandler<BaseMotionActor>((x, _) => x.HandleContentChanged());
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        return MotionLayout?.Measure(availableSize) ?? base.MeasureOverride(availableSize);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        return MotionLayout?.Arrange(finalSize) ?? base.ArrangeOverride(finalSize);
    }

    protected override void ArrangeCore(Rect finalRect)
    {
        base.ArrangeCore(MotionLayout?.ConstrainArrangeRect(finalRect) ?? finalRect);
    }

    protected virtual void ApplyMotionTransform()
    {
        // Get the transform matrix and apply it
        Matrix? matrix = default;

        if (MotionTransform == null && MotionTransformOperations == null)
        {
            matrix = Matrix.Identity;
        } 
        else if (MotionTransform != null)
        {
            matrix = RoundMatrix(MotionTransform.Value, DecimalsAfterRound);
        }
        else if (MotionTransformOperations != null)
        {
            matrix = RoundMatrix(MotionTransformOperations.Value, DecimalsAfterRound);
        }
        Debug.Assert(matrix != null);
        
        if (Transformation == matrix)
        {
            return;
        }

        Transformation         = matrix.Value;
        MatrixTransform.Matrix = matrix.Value;
        if (!ReferenceEquals(RenderTransform, MatrixTransform))
        {
            RenderTransform = MatrixTransform;
        }
    }

    private void HandleTransformSourceChanged()
    {
        UpdateActiveTransformSubscription();
        ApplyMotionTransform();
    }

    private void UpdateActiveTransformSubscription()
    {
        // MotionTransform wins over TransformOperations in ApplyMotionTransform. Only
        // the active mutable source may drive its copied matrix.
        var active = _detachedAfterAttachment ? null : MotionTransform as Transform;
        if (ReferenceEquals(active, _observedTransform))
        {
            return;
        }
        _transformChangedEventDisposable?.Dispose();
        _transformChangedEventDisposable = null;
        _observedTransform = active;
        if (active is not null)
        {
            _transformChangedEventDisposable = Observable.FromEventPattern(
                                                             v => active.Changed += v, v => active.Changed -= v)
                                                         .Subscribe(_ => ApplyMotionTransform());
        }
    }

    private void HandleContentChanged()
    {
        if (null != MotionTransformRoot)
        {
            // 这里我们会过滤掉 Scale 缩放
            MotionTransformRoot.RenderTransform       = MatrixTransform;
            MotionTransformRoot.RenderTransformOrigin = new RelativePoint(0, 0, RelativeUnit.Absolute);
        }

        ApplyMotionTransform();
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _detachedAfterAttachment = false;
        UpdateActiveTransformSubscription();
        ApplyMotionTransform();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        _detachedAfterAttachment = true;
        _transformChangedEventDisposable?.Dispose();
        _transformChangedEventDisposable = null;
        _observedTransform = null;
    }

    public virtual void NotifyMotionPreStart()
    {
        Animating = true;
        RaiseEvent(new RoutedEventArgs(PreStartEvent, this));
    }

    public virtual void NotifyMotionCompleted()
    {
        Animating = false;
        InvalidateMeasure();
        RaiseEvent(new RoutedEventArgs(CompletedEvent, this));
    }
    
    /// <summary>
    /// Rounds the non-offset elements of a Matrix to avoid issues due to floating point imprecision.
    /// </summary>
    /// <param name="matrix">Matrix to round.</param>
    /// <param name="decimals">Number of decimal places to round to.</param>
    /// <returns>Rounded Matrix.</returns>
    protected static Matrix RoundMatrix(Matrix matrix, int decimals)
    {
        return new Matrix(
            Math.Round(matrix.M11, decimals),
            Math.Round(matrix.M12, decimals),
            Math.Round(matrix.M21, decimals),
            Math.Round(matrix.M22, decimals),
            matrix.M31,
            matrix.M32);
    }

    public void Follow(BaseMotionActor target)
    {
        UnFollow();
        // These bindings follow a runtime-selected actor; ControlTheme cannot express that source.
        _followBindings = new CompositeDisposable(
            Bind(MotionTransformProperty, target.GetObservable(MotionTransformProperty)),
            Bind(MotionTransformOperationsProperty, target.GetObservable(MotionTransformOperationsProperty)),
            Bind(OpacityProperty, target.GetObservable(OpacityProperty)),
            Bind(RenderTransformOriginProperty, target.GetObservable(RenderTransformOriginProperty)));
        _followTarget = target;
    }

    public void UnFollow()
    {
        if (_followTarget == null)
        {
            return;
        }

        var transform = MotionTransform;
        var operations = MotionTransformOperations;
        var opacity = Opacity;
        var origin = RenderTransformOrigin;
        _followTarget = null;
        _followBindings?.Dispose();
        _followBindings = null;
        MotionTransform = transform;
        MotionTransformOperations = operations;
        Opacity = opacity;
        RenderTransformOrigin = origin;
    }
}
