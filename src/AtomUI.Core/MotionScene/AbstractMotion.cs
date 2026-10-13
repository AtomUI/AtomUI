using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using AtomUI.Animations;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Logging;
using Avalonia.Media;
using Avalonia.Media.Transformation;

namespace AtomUI.MotionScene;

public class AbstractMotion : IMotion
{
    public RelativePoint RenderTransformOrigin { get; protected set; }
    public IList<Animation> Animations { get; }
    public IList<INotifyTransitionCompleted> Transitions { get; }
    public TimeSpan Duration { get; set; }
    public Easing Easing { get; set; }
    public FillMode PropertyValueFillMode { get; set; }
    public MotionSpiritType SpiritType { get; set; } = MotionSpiritType.Transition;

    private Animation? _builtInAnimation;

    public AbstractMotion(TimeSpan? duration = null, Easing? easing = null, FillMode fillMode = FillMode.Forward)
    {
        Animations            = new List<Animation>();
        Duration              = duration ?? TimeSpan.FromMilliseconds(300);
        Easing                = easing ?? new LinearEasing();
        PropertyValueFillMode = fillMode;
        Transitions           = new List<INotifyTransitionCompleted>();
    }
    
    public void Run(BaseMotionActor actor, Action? aboutToStart = null, Action? completedAction = null)
    {
        VerifyActor(actor);
        _ = ObserveRunAsync(actor, aboutToStart, completedAction);
    }

    public async Task RunAsync(BaseMotionActor actor,
                               Action? aboutToStart = null,
                               CancellationToken cancellationToken = default)
    {
        VerifyActor(actor);
        await RunCoreAsync(actor, aboutToStart, null, cancellationToken);
    }

    private static void VerifyActor(BaseMotionActor actor)
    {
        if (actor.IsFollowMode())
        {
            throw new InvalidOperationException("The Actor is in follow mode and cannot perform animations.");
        }
    }

    private async Task ObserveRunAsync(BaseMotionActor actor, Action? aboutToStart, Action? completedAction)
    {
        try
        {
            await RunCoreAsync(actor, aboutToStart, completedAction, CancellationToken.None);
        }
        catch (OperationCanceledException)
        {
            // A newer motion on the same actor supersedes this fire-and-forget run.
        }
        catch (Exception exception)
        {
            Logger.TryGet(LogEventLevel.Error, AtomUILogArea.Control)?.Log(
                actor, "Motion execution failed: {Exception}", exception);
        }
    }

    private async Task RunCoreAsync(BaseMotionActor actor,
                                    Action? aboutToStart,
                                    Action? completedAction,
                                    CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var previousRun = actor.ActiveMotionRun;
        using var run = new MotionRun(previousRun?.OriginalOrigin ?? actor.RenderTransformOrigin, cancellationToken);
        actor.ActiveMotionRun = run;
        var isTransition = SpiritType == MotionSpiritType.Transition;
        INotifyTransitionCompleted[] transitions = [];
        Exception? executionFailure = null;

        try
        {
            // Publish ownership before canceling: an old continuation must never reset this run's actor.
            previousRun?.Cancel();
            Animation[] animations = [];
            if (isTransition)
            {
                try
                {
                    ConfigureTransitions();
                }
                finally
                {
                    transitions = Transitions.ToArray();
                }
            }
            else
            {
                ConfigureAnimation();
                animations = Animations.ToArray();
            }

            var origin = RenderTransformOrigin;
            if (!isTransition)
            {
                actor.RenderTransformOrigin = origin;
            }
            actor.NotifyMotionPreStart();
            run.Token.ThrowIfCancellationRequested();
            NotifyPreStart(actor);
            run.Token.ThrowIfCancellationRequested();
            aboutToStart?.Invoke();
            run.Token.ThrowIfCancellationRequested();

            if (isTransition)
            {
                actor.RenderTransformOrigin = origin;
                await RunTransitionsAsync(actor, transitions, run.Token);
            }
            else
            {
                foreach (var animation in animations)
                {
                    run.Token.ThrowIfCancellationRequested();
                    await animation.RunAsync(actor, run.Token);
                }
            }

            // Avalonia Animation.RunAsync completes normally on cancellation; our contract propagates it.
            run.Token.ThrowIfCancellationRequested();
            actor.NotifyMotionCompleted();
            run.Token.ThrowIfCancellationRequested();
            NotifyCompleted(actor);
            run.Token.ThrowIfCancellationRequested();
            completedAction?.Invoke();
        }
        catch (Exception exception)
        {
            executionFailure = exception;
            throw;
        }
        finally
        {
            List<Exception>? cleanupFailures = null;
            if (ReferenceEquals(actor.ActiveMotionRun, run))
            {
                actor.Animating = false;
                if (isTransition)
                {
                    try
                    {
                        actor.Transitions = null;
                    }
                    catch (Exception exception)
                    {
                        (cleanupFailures ??= []).Add(exception);
                    }
                    if (ReferenceEquals(actor.ActiveMotionRun, run))
                    {
                        try
                        {
                            actor.MotionTransform = null;
                        }
                        catch (Exception exception)
                        {
                            (cleanupFailures ??= []).Add(exception);
                        }
                    }
                }
                // Property notifications may throw or start another run. Neither permits this run to
                // skip releasing its resources or to reset state now owned by the replacement.
                if (ReferenceEquals(actor.ActiveMotionRun, run))
                {
                    try
                    {
                        actor.RenderTransformOrigin = run.OriginalOrigin;
                    }
                    catch (Exception exception)
                    {
                        (cleanupFailures ??= []).Add(exception);
                    }
                }
                if (ReferenceEquals(actor.ActiveMotionRun, run))
                {
                    actor.ActiveMotionRun = null;
                }
                try
                {
                    actor.InvalidateMeasure();
                }
                catch (Exception exception)
                {
                    (cleanupFailures ??= []).Add(exception);
                }
            }

            foreach (var transition in transitions)
            {
                try
                {
                    transition.Dispose();
                }
                catch (Exception exception)
                {
                    (cleanupFailures ??= []).Add(exception);
                }
            }

            if (cleanupFailures != null)
            {
                if (executionFailure != null)
                {
                    cleanupFailures.Insert(0, executionFailure);
                }
                throw new AggregateException("Motion execution cleanup failed.", cleanupFailures);
            }
        }
    }

    private async Task RunTransitionsAsync(BaseMotionActor actor,
                                           INotifyTransitionCompleted[] transitions,
                                           CancellationToken cancellationToken)
    {
        actor.Transitions = null;
        ConfigureMotionStartValue(actor);
        cancellationToken.ThrowIfCancellationRequested();

        using var completionCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        // Merge propagates one transition's failure immediately, even if another never completes.
        var completion = transitions.Select(transition => transition.CompletedObservable.LastAsync())
                                    .Merge()
                                    .All(status => status)
                                    .ToTask(completionCancellation.Token);
        try
        {
            var actorTransitions = new Transitions();
            var maxDuration = TimeSpan.Zero;
            foreach (var transition in transitions)
            {
                actorTransitions.Add(transition);
                if (transition.Duration > maxDuration)
                {
                    maxDuration = transition.Duration;
                }
            }

            var delta = maxDuration * 0.1;
            if (delta > TimeSpan.FromMilliseconds(100))
            {
                delta = TimeSpan.FromMilliseconds(100);
            }

            actor.Transitions = actorTransitions;
            ConfigureMotionEndValue(actor);
            cancellationToken.ThrowIfCancellationRequested();
            // Retain the timeout for unchanged properties that never produce a transition callback.
            var winner = await Task.WhenAny(completion, Task.Delay(maxDuration + delta, completionCancellation.Token));
            await winner;
            cancellationToken.ThrowIfCancellationRequested();
            if (completion.IsCompleted && !await completion)
            {
                throw new InvalidOperationException("A motion transition did not complete successfully.");
            }
        }
        finally
        {
            completionCancellation.Cancel();
        }
    }

    internal sealed class MotionRun : IDisposable
    {
        private readonly CancellationTokenSource _cancellation;

        internal RelativePoint OriginalOrigin { get; }
        internal CancellationToken Token => _cancellation.Token;

        internal MotionRun(RelativePoint originalOrigin, CancellationToken cancellationToken)
        {
            OriginalOrigin = originalOrigin;
            _cancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        }

        internal void Cancel() => _cancellation.Cancel();
        public void Dispose() => _cancellation.Dispose();
    }

    protected virtual void ConfigureAnimation()
    {
    }

    protected virtual void ConfigureTransitions()
    {
        // Each execution owns and disposes its snapshot; another actor may still be using the old list.
        Transitions.Clear();
        var opacityTransition             = new NotifiableDoubleTransition()
        {
            Duration = Duration,
            Easing   = Easing,
            Property = BaseMotionActor.OpacityProperty,
            Delay    = TimeSpan.FromMilliseconds(0),
        };
        Transitions.Add(opacityTransition);
        
        var transformOperationsTransition = new NotifiableTransformOperationsTransition()
        {
            Duration = Duration,
            Easing   = Easing,
            Property = BaseMotionActor.MotionTransformProperty,
        };
        Transitions.Add(transformOperationsTransition);
    }

    protected virtual void ConfigureMotionStartValue(BaseMotionActor actor)
    {
    }

    protected virtual void ConfigureMotionEndValue(BaseMotionActor actor)
    {
    }

    protected virtual void NotifyPreStart(BaseMotionActor actor)
    {
    }

    protected virtual void NotifyCompleted(BaseMotionActor actor)
    {
    }

    /// <summary>
    /// 计算顶层动画渲染层的大小
    /// </summary>
    /// <param name="motionTargetSize">
    /// 动画目标控件的大小，如果动画直接调度到控件本身，则是控件本身的大小，如果是顶层动画渲染，那么就是 ghost
    /// 的大小，如果有阴影这个大小包含阴影的 thickness
    /// 目前的实现没有加一个固定的 Padding
    /// </param>
    /// <returns></returns>
    internal virtual Size CalculateSceneSize(Size motionTargetSize)
    {
        return motionTargetSize;
    }

    /// <summary>
    /// 计算动画层的全局坐标
    /// </summary>
    /// <param name="motionTargetSize">动画目标控件的大小，包含阴影</param>
    /// <param name="motionTargetPosition">动画目标控件的最终全局坐标位置</param>
    /// <returns></returns>
    internal virtual Point CalculateScenePosition(Size motionTargetSize, Point motionTargetPosition)
    {
        return motionTargetPosition;
    }

    protected static ITransform BuildScaleTransform(double scaleX, double scaleY)
    {
        var builder = new TransformOperations.Builder(1);
        builder.AppendScale(scaleX, scaleY);
        return builder.Build();
    }

    protected static ITransform BuildScaleTransform(double scale)
    {
        return BuildScaleTransform(scale, scale);
    }

    protected static ITransform BuildScaleXTransform(double scale)
    {
        return BuildScaleTransform(scale, 1.0);
    }

    protected static ITransform BuildScaleYTransform(double scale)
    {
        return BuildScaleTransform(1.0, scale);
    }

    protected static ITransform BuildTranslateTransform(double offsetX, double offsetY)
    {
        var builder = new TransformOperations.Builder(1);
        builder.AppendTranslate(offsetX, offsetY);
        return builder.Build();
    }

    protected static ITransform BuildTranslateScaleAndTransform(
        double scaleX, double scaleY, double offsetX, double offsetY)
    {
        var builder = new TransformOperations.Builder(2);
        builder.AppendScale(scaleX, scaleY);
        builder.AppendTranslate(offsetX, offsetY);
        return builder.Build();
    }

    internal void SetBuiltInAnimation(Animation animation)
    {
        var index = _builtInAnimation == null ? -1 : Animations.IndexOf(_builtInAnimation);
        if (index >= 0)
        {
            Animations[index] = animation;
        }
        else
        {
            Animations.Add(animation);
        }
        _builtInAnimation = animation;
    }

    protected Animation CreateAnimation()
    {
        var animation = new Animation
        {
            Duration = Duration,
            Easing   = Easing,
            FillMode = PropertyValueFillMode
        };
        return animation;
    }
}
