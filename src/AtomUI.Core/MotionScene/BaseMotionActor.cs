using System.Diagnostics;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using AtomUI.Data;
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

    /// <summary>
    /// RenderTransform/MatrixTransform applied to MotionTransformRoot.
    /// </summary>
    protected readonly MatrixTransform MatrixTransform = new();

    /// <summary>
    /// Transformation matrix corresponding to _matrixTransform.
    /// </summary>
    protected Matrix Transformation = Matrix.Identity;

    private IDisposable? _transformChangedEventDisposable;

    /// <summary>
    /// Number of decimals to round the Matrix to.
    /// </summary>
    protected const int DecimalsAfterRound = 4;

    /// <summary>
    /// 动画是否在
    /// </summary>
    internal bool Animating = false;

    private BaseMotionActor? _followTarget;
    private CompositeDisposable? _followDisposables;

    static BaseMotionActor()
    {
        ClipToBoundsProperty.OverrideDefaultValue<BaseMotionActor>(true);

        MotionTransformProperty.Changed
                               .AddClassHandler<BaseMotionActor>((x, e) => x.HandleLayoutTransformChanged(e));
        
        MotionTransformOperationsProperty.Changed
                                         .AddClassHandler<BaseMotionActor>((x, e) => x.HandleLayoutTransformOperationsChanged(e));

        ContentProperty.Changed
                       .AddClassHandler<BaseMotionActor>((x, _) => x.HandleContentChanged());
        AffectsRender<BaseMotionActor>(MotionTransformProperty);
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
        RenderTransform        = MatrixTransform;
        InvalidateVisual();
    }

    private void HandleLayoutTransformChanged(AvaloniaPropertyChangedEventArgs change)
    {
        var newTransform = change.NewValue as Transform;

        _transformChangedEventDisposable?.Dispose();
        _transformChangedEventDisposable = null;

        if (newTransform != null)
        {
            _transformChangedEventDisposable = Observable.FromEventPattern(
                                                             v => newTransform.Changed += v, v => newTransform.Changed -= v)
                                                         .Subscribe(_ => ApplyMotionTransform());
        }

        ApplyMotionTransform();
    }

    private void HandleLayoutTransformOperationsChanged(AvaloniaPropertyChangedEventArgs change)
    {
        var newTransform = change.NewValue as Transform;

        _transformChangedEventDisposable?.Dispose();
        _transformChangedEventDisposable = null;

        if (newTransform != null)
        {
            _transformChangedEventDisposable = Observable.FromEventPattern(
                                                             v => newTransform.Changed += v, v => newTransform.Changed -= v)
                                                         .Subscribe(_ => ApplyMotionTransform());
        }

        ApplyMotionTransform();
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
    
    public virtual void NotifyMotionPreStart()
    {
        RaiseEvent(new RoutedEventArgs(PreStartEvent, this));
        Animating = true;
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
        _followDisposables?.Dispose();
        _followDisposables = new CompositeDisposable(4);
        _followDisposables.Add(BindUtils.RelayBind(target, MotionTransformProperty, this, MotionTransformProperty));
        _followDisposables.Add(BindUtils.RelayBind(target, MotionTransformOperationsProperty, this, MotionTransformOperationsProperty));
        _followDisposables.Add(BindUtils.RelayBind(target, OpacityProperty, this, OpacityProperty));
        _followDisposables.Add(BindUtils.RelayBind(target, RenderTransformOriginProperty, this, RenderTransformOriginProperty));
        _followTarget = target;
    }

    public void UnFollow()
    {
        _followDisposables?.Dispose();
        _followDisposables = null;
        _followTarget      = this;
    }
}