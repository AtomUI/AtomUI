using AtomUI.Animations;
using AtomUI.Data;
using AtomUI.Utils;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.VisualTree;

namespace AtomUI.Controls.Commons;

[PseudoClasses(ProgressBarPseudoClass.Indeterminate, ProgressBarPseudoClass.Completed)]
public abstract class AbstractProgressBar : RangeBase,
                                            ISizeTypeAware,
                                            IMotionAwareControl
{
    protected const double LARGE_STROKE_THICKNESS = 8;
    protected const double MIDDLE_STROKE_THICKNESS = 6;
    protected const double SMALL_STROKE_THICKNESS = 4;

    #region 公共属性定义
    public static readonly StyledProperty<bool> IsIndeterminateProperty =
        AvaloniaProperty.Register<AbstractProgressBar, bool>(nameof(IsIndeterminate));
    
    public static readonly StyledProperty<bool> IsProgressInfoVisibleProperty =
        AvaloniaProperty.Register<AbstractProgressBar, bool>(nameof(IsProgressInfoVisible), true);
    
    public static readonly StyledProperty<string> ProgressTextFormatProperty =
        AvaloniaProperty.Register<AbstractProgressBar, string>(nameof(ProgressTextFormat), "{0:0}%");
    
    public static readonly DirectProperty<AbstractProgressBar, double> PercentageProperty =
        AvaloniaProperty.RegisterDirect<AbstractProgressBar, double>(
            nameof(Percentage),
            o => o.Percentage,
            (o, v) => o.Percentage = v);
    
    public static readonly StyledProperty<IBrush?> StrokeBrushProperty =
        AvaloniaProperty.Register<AbstractProgressBar, IBrush?>(nameof(StrokeBrush));

    public static readonly StyledProperty<Color?> TrailColorProperty =
        AvaloniaProperty.Register<AbstractProgressBar, Color?>(nameof(TrailColor));

    public static readonly StyledProperty<PenLineCap> StrokeLineCapProperty =
        AvaloniaProperty.Register<AbstractProgressBar, PenLineCap>(nameof(StrokeLineCap), PenLineCap.Round);

    public static readonly StyledProperty<SizeType> SizeTypeProperty =
        SizeTypeControlProperty.SizeTypeProperty.AddOwner<AbstractProgressBar>();

    public static readonly StyledProperty<ProgressStatus> StatusProperty =
        AvaloniaProperty.Register<AbstractProgressBar, ProgressStatus>(nameof(Status));

    public static readonly StyledProperty<double> IndicatorThicknessProperty =
        AvaloniaProperty.Register<AbstractProgressBar, double>(nameof(IndicatorThickness), double.NaN);

    public static readonly StyledProperty<double> SuccessThresholdProperty =
        AvaloniaProperty.Register<AbstractProgressBar, double>(nameof(SuccessThreshold), double.NaN);

    public static readonly StyledProperty<IBrush?> SuccessStrokeBrushProperty =
        AvaloniaProperty.Register<AbstractProgressBar, IBrush?>(nameof(SuccessStrokeBrush));

    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<AbstractProgressBar>();
    
    public static readonly StyledProperty<PathIcon?> ExceptionCompletedIconProperty =
        AvaloniaProperty.Register<AbstractProgressBar, PathIcon?>(nameof(ExceptionCompletedIcon));
    
    public static readonly StyledProperty<PathIcon?> SuccessCompletedIconProperty =
        AvaloniaProperty.Register<AbstractProgressBar, PathIcon?>(nameof(SuccessCompletedIcon));

    public static readonly StyledProperty<object?> ProgressInfoContentProperty =
        AvaloniaProperty.Register<AbstractProgressBar, object?>(nameof(ProgressInfoContent));

    public static readonly StyledProperty<IDataTemplate?> ProgressInfoContentTemplateProperty =
        AvaloniaProperty.Register<AbstractProgressBar, IDataTemplate?>(nameof(ProgressInfoContentTemplate));

    /// <summary>
    /// Gets or sets a value indicating whether the progress bar shows the actual value or a generic,
    /// continues progress indicator (indeterminate state).
    /// </summary>
    public bool IsIndeterminate
    {
        get => GetValue(IsIndeterminateProperty);
        set => SetValue(IsIndeterminateProperty, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether progress text will be shown.
    /// </summary>
    public bool IsProgressInfoVisible
    {
        get => GetValue(IsProgressInfoVisibleProperty);
        set => SetValue(IsProgressInfoVisibleProperty, value);
    }

    /// <summary>
    /// Gets or sets the format string applied to the internally calculated progress text before it is shown.
    /// </summary>
    public string ProgressTextFormat
    {
        get => GetValue(ProgressTextFormatProperty);
        set => SetValue(ProgressTextFormatProperty, value);
    }
    
    public IBrush? StrokeBrush
    {
        get => GetValue(StrokeBrushProperty);
        set => SetValue(StrokeBrushProperty, value);
    }

    public Color? TrailColor
    {
        get => GetValue(TrailColorProperty);
        set => SetValue(TrailColorProperty, value);
    }

    public PenLineCap StrokeLineCap
    {
        get => GetValue(StrokeLineCapProperty);
        set => SetValue(StrokeLineCapProperty, value);
    }

    public SizeType SizeType
    {
        get => GetValue(SizeTypeProperty);
        set => SetValue(SizeTypeProperty, value);
    }

    public ProgressStatus Status
    {
        get => GetValue(StatusProperty);
        set => SetValue(StatusProperty, value);
    }

    private double _percentage;

    /// <summary>
    /// Gets the overall percentage complete of the progress
    /// </summary>
    /// <remarks>
    /// This read-only property is automatically calculated using the current <see cref="RangeBase.Value" /> and
    /// the effective range (<see cref="RangeBase.Maximum" /> - <see cref="RangeBase.Minimum" />).
    /// </remarks>
    public double Percentage
    {
        get => _percentage;
        private set => SetAndRaise(PercentageProperty, ref _percentage, value);
    }

    public double IndicatorThickness
    {
        get => GetValue(IndicatorThicknessProperty);
        set => SetValue(IndicatorThicknessProperty, value);
    }

    public IBrush? SuccessStrokeBrush
    {
        get => GetValue(SuccessStrokeBrushProperty);
        set => SetValue(SuccessStrokeBrushProperty, value);
    }

    public double SuccessThreshold
    {
        get => GetValue(SuccessThresholdProperty);
        set => SetValue(SuccessThresholdProperty, value);
    }

    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }
    
    public PathIcon? ExceptionCompletedIcon
    {
        get => GetValue(ExceptionCompletedIconProperty);
        set => SetValue(ExceptionCompletedIconProperty, value);
    }

    public PathIcon? SuccessCompletedIcon
    {
        get => GetValue(SuccessCompletedIconProperty);
        set => SetValue(SuccessCompletedIconProperty, value);
    }

    /// <summary>
    /// 自定义进度信息内容。非空时总是显示（不受 <see cref="IsProgressInfoVisible"/> 约束），
    /// 并替代百分比文本与两个状态图标。
    /// </summary>
    public object? ProgressInfoContent
    {
        get => GetValue(ProgressInfoContentProperty);
        set => SetValue(ProgressInfoContentProperty, value);
    }

    public IDataTemplate? ProgressInfoContentTemplate
    {
        get => GetValue(ProgressInfoContentTemplateProperty);
        set => SetValue(ProgressInfoContentTemplateProperty, value);
    }
    #endregion

    #region 内部属性定义

    internal static readonly DirectProperty<AbstractProgressBar, bool> IsProgressInfoPresentedProperty =
        AvaloniaProperty.RegisterDirect<AbstractProgressBar, bool>(nameof(IsProgressInfoPresented),
            o => o.IsProgressInfoPresented);

    internal static readonly DirectProperty<AbstractProgressBar, SizeType> EffectiveSizeTypeProperty =
        AvaloniaProperty.RegisterDirect<AbstractProgressBar, SizeType>(nameof(EffectiveSizeType),
            o => o.EffectiveSizeType,
            (o, v) => o.EffectiveSizeType = v);

    internal static readonly DirectProperty<AbstractProgressBar, double> StrokeThicknessProperty =
        AvaloniaProperty.RegisterDirect<AbstractProgressBar, double>(nameof(StrokeThickness),
            o => o.StrokeThickness,
            (o, v) => o.StrokeThickness = v);

    internal static readonly StyledProperty<IBrush?> GrooveBrushProperty =
        AvaloniaProperty.Register<AbstractProgressBar, IBrush?>(nameof(GrooveBrush));

    internal static readonly StyledProperty<bool> IsPercentLabelVisibleProperty =
        AvaloniaProperty.Register<AbstractProgressBar, bool>(nameof(IsPercentLabelVisible), true);

    internal static readonly StyledProperty<bool> IsStatusIconVisibleProperty =
        AvaloniaProperty.Register<AbstractProgressBar, bool>(nameof(IsStatusIconVisible), true);

    internal static readonly StyledProperty<bool> IsCompletedProperty =
        AvaloniaProperty.Register<AbstractProgressBar, bool>(nameof(IsCompleted));

    private bool _isProgressInfoPresented;

    /// <summary>
    /// 信息区是否呈现：<see cref="IsProgressInfoVisible"/> 为真，或设置了 <see cref="ProgressInfoContent"/>。
    /// </summary>
    internal bool IsProgressInfoPresented
    {
        get => _isProgressInfoPresented;
        private set => SetAndRaise(IsProgressInfoPresentedProperty, ref _isProgressInfoPresented, value);
    }

    private SizeType _effectiveSizeType;

    internal SizeType EffectiveSizeType
    {
        get => _effectiveSizeType;
        set => SetAndRaise(EffectiveSizeTypeProperty, ref _effectiveSizeType, value);
    }

    private double _strokeThickness;

    protected double StrokeThickness
    {
        get => _strokeThickness;
        set => SetAndRaise(StrokeThicknessProperty, ref _strokeThickness, value);
    }

    internal IBrush? GrooveBrush
    {
        get => GetValue(GrooveBrushProperty);
        set => SetValue(GrooveBrushProperty, value);
    }

    internal bool IsPercentLabelVisible
    {
        get => GetValue(IsPercentLabelVisibleProperty);
        set => SetValue(IsPercentLabelVisibleProperty, value);
    }

    internal bool IsStatusIconVisible
    {
        get => GetValue(IsStatusIconVisibleProperty);
        set => SetValue(IsStatusIconVisibleProperty, value);
    }

    internal bool IsCompleted
    {
        get => GetValue(IsCompletedProperty);
        set => SetValue(IsCompletedProperty, value);
    }

    #endregion

    #region 内部协作 API

    internal const string ProgressBodyPart = "PART_ProgressBody";
    internal const string ProgressRailPart = "PART_ProgressRail";
    internal const string ProgressTrackPart = "PART_ProgressTrack";
    internal const string ProgressSuccessPart = "PART_ProgressSuccess";
    internal const string ProgressIndicatorPart = "PART_ProgressIndicator";
    internal const string LayoutTransformControlPart = "PART_LayoutTransformControl";
    internal const string PercentageLabelPart = "PART_PercentageLabel";
    internal const string ExceptionCompletedIconPresenterPart = "PART_ExceptionCompletedIconPresenter";
    internal const string SuccessCompletedIconPresenterPart = "PART_SuccessCompletedIconPresenter";
    internal const string ProgressInfoContentPresenterPart = "PART_ProgressInfoContentPresenter";

    #endregion

    protected LayoutTransformControl? LayoutTransformLabel;
    protected Label? PercentageLabel;
    private protected Panel? ProgressIndicator;
    protected IconPresenter? SuccessCompletedIconPresenter;
    protected IconPresenter? ExceptionCompletedIconPresenter;
    protected ContentPresenter? ProgressInfoContentPresenter;
    private readonly TransitionInitializationSession _transitionInitialization;
    private bool _isMeasuringProgressInfoContent;
    private IDisposable?[]? _infoContentVisibilityOverrides;
    private IDisposable? _defaultExceptionCompletedIconValue;
    private IDisposable? _defaultSuccessCompletedIconValue;

    static AbstractProgressBar()
    {
        AffectsMeasure<AbstractProgressBar>(EffectiveSizeTypeProperty,
            IsProgressInfoVisibleProperty,
            ProgressInfoContentProperty,
            ProgressTextFormatProperty,
            ValueProperty);
        AffectsRender<AbstractProgressBar>(StrokeBrushProperty,
            StrokeLineCapProperty,
            TrailColorProperty,
            StrokeThicknessProperty,
            SuccessStrokeBrushProperty,
            SuccessThresholdProperty);
        ValueProperty.OverrideMetadata<AbstractProgressBar>(
            new StyledPropertyMetadata<double>(defaultBindingMode: BindingMode.OneWay));
        SizeTypeProperty.OverrideDefaultValue<AbstractProgressBar>(SizeType.Large);
    }

    public AbstractProgressBar()
    {
        _transitionInitialization = new TransitionInitializationSession(this);
        // 子类可以覆盖 IsProgressInfoVisible 的默认值，标志要从实际默认值起算，而不是等第一次变化
        _isProgressInfoPresented = IsProgressInfoVisible || ProgressInfoContent is not null;
        _effectiveSizeType = SizeType;
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == ValueProperty ||
            change.Property == MinimumProperty ||
            change.Property == MaximumProperty ||
            change.Property == IsIndeterminateProperty ||
            change.Property == ProgressTextFormatProperty ||
            change.Property == IsProgressInfoVisibleProperty ||
            change.Property == StatusProperty)
        {
            // 信息区在运行中打开、或从异常状态恢复时，文本与可见性要随之刷新
            UpdateProgress();
        }

        if (change.Property == IsIndeterminateProperty)
        {
            UpdatePseudoClasses();
        }

        if (change.Property == IsProgressInfoVisibleProperty ||
            change.Property == ProgressInfoContentProperty)
        {
            IsProgressInfoPresented = IsProgressInfoVisible || ProgressInfoContent is not null;
        }

        if (change.Property == ProgressInfoContentProperty ||
            change.Property == ProgressInfoContentTemplateProperty)
        {
            UpdateProgressInfoContentVisuals();
            if (change.Property == ProgressInfoContentProperty)
            {
                if (ProgressInfoContent is null)
                {
                    // 清除后恢复百分比文本
                    UpdateProgress();
                }

                NotifyProgressInfoContentChanged();
            }
        }

        HandlePropertyChangedForStyle(change);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        // 旧呈现器仍是自定义内容的视觉父级，新呈现器接管前必须先释放，否则同一控件会出现两个父级
        if (ProgressInfoContentPresenter is not null)
        {
            ProgressInfoContentPresenter.Content         = null;
            ProgressInfoContentPresenter.ContentTemplate = null;
        }

        ReleaseInfoContentVisibilityOverrides();
        LayoutTransformLabel = null;
        PercentageLabel = null;
        ProgressIndicator = null;
        ExceptionCompletedIconPresenter = null;
        SuccessCompletedIconPresenter = null;
        ProgressInfoContentPresenter = null;
        base.OnApplyTemplate(e);
        LayoutTransformLabel = e.NameScope.Find<LayoutTransformControl>(LayoutTransformControlPart);
        PercentageLabel = e.NameScope.Find<Label>(PercentageLabelPart);
        ProgressIndicator = e.NameScope.Find<Panel>(ProgressIndicatorPart);
        ExceptionCompletedIconPresenter = e.NameScope.Find<IconPresenter>(ExceptionCompletedIconPresenterPart);
        SuccessCompletedIconPresenter = e.NameScope.Find<IconPresenter>(SuccessCompletedIconPresenterPart);
        ProgressInfoContentPresenter = e.NameScope.Find<ContentPresenter>(ProgressInfoContentPresenterPart);
        UpdateProgressInfoContentVisuals();
        // 创建完更新调用一次
        NotifyEffectSizeTypeChanged();
        UpdateProgress();
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        UpdatePseudoClasses();
    }
    
    protected abstract SizeType CalculateEffectiveSizeType(double size);

    protected abstract Rect GetProgressBarRect(Rect controlRect);
    protected abstract Rect GetExtraInfoRect(Rect controlRect);

    protected abstract void RenderGroove(DrawingContext context);
    protected abstract void RenderIndicatorBar(DrawingContext context);
    protected abstract void CalculateStrokeThickness();

    private protected virtual bool HasTemplateProgressVisuals => false;

    protected virtual void NotifyEffectSizeTypeChanged()
    {
        CalculateStrokeThickness();
    }

    private protected void ArrangeProgressIndicator(Rect bounds)
    {
        if (ProgressIndicator is null)
        {
            return;
        }

        Canvas.SetLeft(ProgressIndicator, bounds.X);
        Canvas.SetTop(ProgressIndicator, bounds.Y);
        ProgressIndicator.Width = bounds.Width;
        ProgressIndicator.Height = bounds.Height;
        ProgressIndicator.Measure(bounds.Size);
        ProgressIndicator.Arrange(bounds);
    }

    private protected double CalculateProgressRatio(double value)
    {
        var range = Maximum - Minimum;
        return Math.Abs(range) < double.Epsilon ? 1.0 : (value - Minimum) / range;
    }

    private void UpdateProgress()
    {
        Percentage = CalculateProgressRatio(Value) * 100;
        NotifyUpdateProgress();
    }

    protected virtual void NotifyUpdateProgress()
    {
        if (IsProgressInfoVisible && PercentageLabel != null)
        {
            if (Status != ProgressStatus.Exception)
            {
                PercentageLabel.Content = string.Format(ProgressTextFormat, _percentage);
            }

            NotifyHandleExtraInfoVisibility();
        }
    }

    protected virtual void NotifyHandleExtraInfoVisibility()
    {
    }

    /// <summary>
    /// 自定义信息内容被设置或清除后，子类据此重算信息区尺寸。
    /// </summary>
    protected virtual void NotifyProgressInfoContentChanged()
    {
    }

    // 自定义内容存在时，隐藏百分比标签与状态图标；清除后交还给样式与默认逻辑。
    // 呈现器的内容由这里直接写入，保证随后的测量读到的就是最新内容。
    // 标签与图标以可释放的 Animation 优先级隐藏：它高于本地值与绑定但不替换它们，
    // 清除内容或换模板时释放，自定义模板上原有的本地 IsVisible 与绑定随即恢复。
    private void UpdateProgressInfoContentVisuals()
    {
        var hasContent = ProgressInfoContent is not null;
        if (ProgressInfoContentPresenter is not null)
        {
            ProgressInfoContentPresenter.Content         = ProgressInfoContent;
            ProgressInfoContentPresenter.ContentTemplate = ProgressInfoContentTemplate;
            ProgressInfoContentPresenter.IsVisible       = hasContent;
        }

        if (hasContent)
        {
            _infoContentVisibilityOverrides ??=
            [
                HideForInfoContent(LayoutTransformLabel),
                HideForInfoContent(ExceptionCompletedIconPresenter),
                HideForInfoContent(SuccessCompletedIconPresenter)
            ];
            return;
        }

        ReleaseInfoContentVisibilityOverrides();
    }

    // 默认图标以模板优先级写入，主题或本地图标可替换它；重套模板时替换持有的句柄，避免累积属性帧
    private protected void SetupDefaultCompletedIcons(Func<PathIcon> createExceptionIcon, Func<PathIcon> createSuccessIcon)
    {
        if (ExceptionCompletedIcon == null)
        {
            this.SetHeldValue(ExceptionCompletedIconProperty, createExceptionIcon(), ref _defaultExceptionCompletedIconValue);
        }

        if (SuccessCompletedIcon == null)
        {
            this.SetHeldValue(SuccessCompletedIconProperty, createSuccessIcon(), ref _defaultSuccessCompletedIconValue);
        }
    }

    private static IDisposable? HideForInfoContent(Control? part)
    {
        return part?.SetValue(IsVisibleProperty, false, BindingPriority.Animation);
    }

    private void ReleaseInfoContentVisibilityOverrides()
    {
        if (_infoContentVisibilityOverrides is not { } overrides)
        {
            return;
        }

        _infoContentVisibilityOverrides = null;
        foreach (var handle in overrides)
        {
            handle?.Dispose();
        }
    }

    /// <summary>
    /// 自定义内容呈现器的期望尺寸；没有呈现器（模板缺失该部件）时返回 null。
    /// </summary>
    private protected Size? MeasureProgressInfoContent()
    {
        if (ProgressInfoContent is null || ProgressInfoContentPresenter is null)
        {
            return null;
        }

        _isMeasuringProgressInfoContent = true;
        try
        {
            ProgressInfoContentPresenter.UpdateChild();
            ProgressInfoContentPresenter.Measure(Size.Infinity);
        }
        finally
        {
            _isMeasuringProgressInfoContent = false;
        }

        return ProgressInfoContentPresenter.DesiredSize;
    }

    // A measure started here already feeds the current size into the caller. A size change found by the layout pass
    // itself (content resized after it was set) invalidates this control, whose next measure and arrange read the
    // presenter's current desired size. Recalculating here would re-enter the presenter's own measure.
    internal void HandleProgressInfoContentResized()
    {
        if (!_isMeasuringProgressInfoContent && ProgressInfoContent is not null)
        {
            InvalidateMeasure();
            InvalidateArrange();
        }
    }

    private void HandlePropertyChangedForStyle(AvaloniaPropertyChangedEventArgs change)
    {
        if (change.Property == SizeTypeProperty)
        {
            EffectiveSizeType = change.GetNewValue<SizeType>();
        }
        else if (change.Property == ValueProperty)
        {
            IsCompleted = MathUtils.AreClose(Value, Maximum);
            UpdatePseudoClasses();
        }
        else if (change.Property == IsCompletedProperty)
        {
            InvalidateMeasure();
        }

        if (this.IsAttachedToVisualTree())
        {
            if (change.Property == WidthProperty || change.Property == HeightProperty)
            {
                NotifyHandleExtraInfoVisibility();
            }
            else if (change.Property == EffectiveSizeTypeProperty)
            {
                NotifyEffectSizeTypeChanged();
            }
        }
        NotifyPropertyChanged(change);
    }

    private void UpdatePseudoClasses()
    {
        PseudoClasses.Set(ProgressBarPseudoClass.Indeterminate, IsIndeterminate);
        PseudoClasses.Set(ProgressBarPseudoClass.Completed, MathUtils.AreClose(Value, Maximum));
    }

    protected virtual void NotifyPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        if (change.Property == TrailColorProperty)
        {
            if (TrailColor.HasValue)
            {
                GrooveBrush = new SolidColorBrush(TrailColor.Value);
            }
            else
            {
                ClearValue(GrooveBrushProperty);
            }
        }
    }

    public override void Render(DrawingContext context)
    {
        if (HasTemplateProgressVisuals)
        {
            return;
        }

        NotifyPrepareDrawingContext(context);
        RenderGroove(context);
        RenderIndicatorBar(context);
    }

    protected virtual void NotifyPrepareDrawingContext(DrawingContext context)
    {
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _transitionInitialization.Reset();
        base.OnDetachedFromVisualTree(e);
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        _transitionInitialization.CompleteInitialization();
    }
}
