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

/// <summary>
/// Base class of the progress controls. It computes <see cref="Percentage" /> from the value and the range,
/// owns the properties that style the groove and the indicator, and drives the progress info (percentage text,
/// status icon or custom content). Derived classes provide the geometry and the drawing through the abstract
/// members of this class.
/// </summary>
[PseudoClasses(ProgressBarPseudoClass.Indeterminate, ProgressBarPseudoClass.Completed)]
public abstract class AbstractProgressBar : RangeBase,
                                            ISizeTypeAware,
                                            IMotionAwareControl
{
    /// <summary>
    /// Stroke thickness, in DIPs, of the large size type. A line progress bar draws with it when its effective size
    /// type is <see cref="SizeType.Large" />.
    /// </summary>
    protected const double LARGE_STROKE_THICKNESS = 8;
    /// <summary>
    /// Stroke thickness, in DIPs, of the middle size type. A line progress bar draws with it when its effective size
    /// type is <see cref="SizeType.Middle" />.
    /// </summary>
    protected const double MIDDLE_STROKE_THICKNESS = 6;
    /// <summary>
    /// Stroke thickness, in DIPs, of the small size type. A line progress bar draws with it when its effective size
    /// type is <see cref="SizeType.Small" />.
    /// </summary>
    protected const double SMALL_STROKE_THICKNESS = 4;

    #region 公共属性定义
    /// <summary>
    /// Identifies the <see cref="IsIndeterminate" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsIndeterminateProperty =
        AvaloniaProperty.Register<AbstractProgressBar, bool>(nameof(IsIndeterminate));
    
    /// <summary>
    /// Identifies the <see cref="IsProgressInfoVisible" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsProgressInfoVisibleProperty =
        AvaloniaProperty.Register<AbstractProgressBar, bool>(nameof(IsProgressInfoVisible), true);
    
    /// <summary>
    /// Identifies the <see cref="ProgressTextFormat" /> property.
    /// </summary>
    public static readonly StyledProperty<string> ProgressTextFormatProperty =
        AvaloniaProperty.Register<AbstractProgressBar, string>(nameof(ProgressTextFormat), "{0:0}%");
    
    /// <summary>
    /// Identifies the <see cref="Percentage" /> property.
    /// </summary>
    public static readonly DirectProperty<AbstractProgressBar, double> PercentageProperty =
        AvaloniaProperty.RegisterDirect<AbstractProgressBar, double>(
            nameof(Percentage),
            o => o.Percentage,
            (o, v) => o.Percentage = v);
    
    /// <summary>
    /// Identifies the <see cref="StrokeBrush" /> property.
    /// </summary>
    public static readonly StyledProperty<IBrush?> StrokeBrushProperty =
        AvaloniaProperty.Register<AbstractProgressBar, IBrush?>(nameof(StrokeBrush));

    /// <summary>
    /// Identifies the <see cref="TrailColor" /> property.
    /// </summary>
    public static readonly StyledProperty<Color?> TrailColorProperty =
        AvaloniaProperty.Register<AbstractProgressBar, Color?>(nameof(TrailColor));

    /// <summary>
    /// Identifies the <see cref="StrokeLineCap" /> property.
    /// </summary>
    public static readonly StyledProperty<PenLineCap> StrokeLineCapProperty =
        AvaloniaProperty.Register<AbstractProgressBar, PenLineCap>(nameof(StrokeLineCap), PenLineCap.Round);

    /// <summary>
    /// Identifies the <see cref="SizeType" /> property.
    /// </summary>
    public static readonly StyledProperty<SizeType> SizeTypeProperty =
        SizeTypeControlProperty.SizeTypeProperty.AddOwner<AbstractProgressBar>();

    /// <summary>
    /// Identifies the <see cref="Status" /> property.
    /// </summary>
    public static readonly StyledProperty<ProgressStatus> StatusProperty =
        AvaloniaProperty.Register<AbstractProgressBar, ProgressStatus>(nameof(Status));

    /// <summary>
    /// Identifies the <see cref="IndicatorThickness" /> property.
    /// </summary>
    public static readonly StyledProperty<double> IndicatorThicknessProperty =
        AvaloniaProperty.Register<AbstractProgressBar, double>(nameof(IndicatorThickness), double.NaN);

    /// <summary>
    /// Identifies the <see cref="SuccessThreshold" /> property.
    /// </summary>
    public static readonly StyledProperty<double> SuccessThresholdProperty =
        AvaloniaProperty.Register<AbstractProgressBar, double>(nameof(SuccessThreshold), double.NaN);

    /// <summary>
    /// Identifies the <see cref="SuccessStrokeBrush" /> property.
    /// </summary>
    public static readonly StyledProperty<IBrush?> SuccessStrokeBrushProperty =
        AvaloniaProperty.Register<AbstractProgressBar, IBrush?>(nameof(SuccessStrokeBrush));

    /// <summary>
    /// Identifies the <see cref="IsMotionEnabled" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<AbstractProgressBar>();
    
    /// <summary>
    /// Identifies the <see cref="ExceptionCompletedIcon" /> property.
    /// </summary>
    public static readonly StyledProperty<PathIcon?> ExceptionCompletedIconProperty =
        AvaloniaProperty.Register<AbstractProgressBar, PathIcon?>(nameof(ExceptionCompletedIcon));
    
    /// <summary>
    /// Identifies the <see cref="SuccessCompletedIcon" /> property.
    /// </summary>
    public static readonly StyledProperty<PathIcon?> SuccessCompletedIconProperty =
        AvaloniaProperty.Register<AbstractProgressBar, PathIcon?>(nameof(SuccessCompletedIcon));

    /// <summary>
    /// Identifies the <see cref="ProgressInfoContent" /> property.
    /// </summary>
    public static readonly StyledProperty<object?> ProgressInfoContentProperty =
        AvaloniaProperty.Register<AbstractProgressBar, object?>(nameof(ProgressInfoContent));

    /// <summary>
    /// Identifies the <see cref="ProgressInfoContentTemplate" /> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> ProgressInfoContentTemplateProperty =
        AvaloniaProperty.Register<AbstractProgressBar, IDataTemplate?>(nameof(ProgressInfoContentTemplate));

    /// <summary>
    /// Gets or sets a value indicating whether the progress bar shows the actual value or a generic, continuous
    /// progress indicator (indeterminate state).
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
    
    /// <summary>
    /// Brush of the indicator, the filled part of the bar. The control theme sets it from <see cref="Status" /> and
    /// shows the success color once the progress is complete. A value set here replaces those colors for every
    /// status; clearing it returns to them.
    /// </summary>
    public IBrush? StrokeBrush
    {
        get => GetValue(StrokeBrushProperty);
        set => SetValue(StrokeBrushProperty, value);
    }

    /// <summary>
    /// Color of the groove, the unfilled part of the bar. Setting it replaces the groove brush of the theme with a
    /// solid brush of this color; clearing it restores the theme brush.
    /// </summary>
    public Color? TrailColor
    {
        get => GetValue(TrailColorProperty);
        set => SetValue(TrailColorProperty, value);
    }

    /// <summary>
    /// Line cap used for the stroke of the bar, <see cref="PenLineCap.Round" /> by default. In step mode the arc is
    /// drawn with flat ends and this value is ignored.
    /// </summary>
    public PenLineCap StrokeLineCap
    {
        get => GetValue(StrokeLineCapProperty);
        set => SetValue(StrokeLineCapProperty, value);
    }

    /// <summary>
    /// Size preset requested for the control, <see cref="SizeType.Large" /> by default. A change assigns the value to
    /// <see cref="EffectiveSizeType" />, from which the stroke thickness is taken; a line progress bar also derives
    /// the effective size type from its own thickness.
    /// </summary>
    public SizeType SizeType
    {
        get => GetValue(SizeTypeProperty);
        set => SetValue(SizeTypeProperty, value);
    }

    /// <summary>
    /// Status the progress bar presents, <see cref="ProgressStatus.Normal" /> by default. The theme derives the
    /// indicator color and the status icon from it. <see cref="ProgressStatus.Exception" /> shows the error color
    /// and the exception icon in place of the percentage text, while <see cref="ProgressStatus.Success" /> shows the
    /// success color. Normal and Active keep the default color.
    /// </summary>
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

    /// <summary>
    /// Thickness of the indicator stroke, in DIPs. <see cref="double.NaN" />, the default, uses the thickness of
    /// the effective size type. A circle progress bar raises a smaller value to its minimum stroke thickness, and a
    /// steps progress bar keeps <c>ChunkHeight</c> equal to this value.
    /// </summary>
    public double IndicatorThickness
    {
        get => GetValue(IndicatorThicknessProperty);
        set => SetValue(IndicatorThicknessProperty, value);
    }

    /// <summary>
    /// Brush of the part of the indicator that lies up to <see cref="SuccessThreshold" />. The theme sets it to the
    /// success color while the control is enabled.
    /// </summary>
    public IBrush? SuccessStrokeBrush
    {
        get => GetValue(SuccessStrokeBrushProperty);
        set => SetValue(SuccessStrokeBrushProperty, value);
    }

    /// <summary>
    /// Value, in the unit of <see cref="RangeBase.Value" />, up to which the indicator is overpainted with
    /// <see cref="SuccessStrokeBrush" />. <see cref="double.NaN" />, the default, adds no such segment. A value
    /// outside the range is clamped to it.
    /// </summary>
    public double SuccessThreshold
    {
        get => GetValue(SuccessThresholdProperty);
        set => SetValue(SuccessThresholdProperty, value);
    }

    /// <summary>
    /// Enables the transitions the theme applies to <see cref="RangeBase.Value" />, <see cref="StrokeBrush" />
    /// and the foreground brush. The theme sets it from the <c>EnableMotion</c> Token; a local value overrides
    /// the Token, and clearing the local value returns to it.
    /// </summary>
    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }
    
    /// <summary>
    /// Icon shown in place of the percentage text while <see cref="Status" /> is
    /// <see cref="ProgressStatus.Exception" />. When this property is <see langword="null" />, the derived control
    /// installs its built-in default icon.
    /// </summary>
    public PathIcon? ExceptionCompletedIcon
    {
        get => GetValue(ExceptionCompletedIconProperty);
        set => SetValue(ExceptionCompletedIconProperty, value);
    }

    /// <summary>
    /// Icon shown in place of the percentage text once the progress is complete. When this property is
    /// <see langword="null" />, the derived control installs its built-in default icon.
    /// </summary>
    public PathIcon? SuccessCompletedIcon
    {
        get => GetValue(SuccessCompletedIconProperty);
        set => SetValue(SuccessCompletedIconProperty, value);
    }

    /// <summary>
    /// Custom content of the progress info area. When it is not <see langword="null" />, it is always presented,
    /// regardless of <see cref="IsProgressInfoVisible" />, and it replaces the percentage text and the two
    /// status icons.
    /// </summary>
    public object? ProgressInfoContent
    {
        get => GetValue(ProgressInfoContentProperty);
        set => SetValue(ProgressInfoContentProperty, value);
    }

    /// <summary>
    /// Template used to present <see cref="ProgressInfoContent" />, or <see langword="null" /> to present the
    /// content directly, the way a <see cref="ContentPresenter" /> without a template presents it.
    /// </summary>
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

    internal static readonly DirectProperty<AbstractProgressBar, double> EffectiveStrokeThicknessProperty =
        AvaloniaProperty.RegisterDirect<AbstractProgressBar, double>(nameof(EffectiveStrokeThickness),
            o => o.EffectiveStrokeThickness,
            (o, v) => o.EffectiveStrokeThickness = v);

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
    /// Whether the progress info area is presented: <see cref="IsProgressInfoVisible" /> is
    /// <see langword="true" /> or <see cref="ProgressInfoContent" /> is set.
    /// </summary>
    internal bool IsProgressInfoPresented
    {
        get => _isProgressInfoPresented;
        private set => SetAndRaise(IsProgressInfoPresentedProperty, ref _isProgressInfoPresented, value);
    }
    // Status icon brushes resolved by the control theme from the progress control's own Tokens. The icon presenters are
    // registered controls with an identity of their own, so their brushes are read on the progress control and bound down.
    internal static readonly StyledProperty<IBrush?> ExceptionIconBrushProperty =
        AvaloniaProperty.Register<AbstractProgressBar, IBrush?>(nameof(ExceptionIconBrush));

    internal static readonly StyledProperty<IBrush?> SuccessIconBrushProperty =
        AvaloniaProperty.Register<AbstractProgressBar, IBrush?>(nameof(SuccessIconBrush));

    private SizeType _effectiveSizeType;

    /// <summary>
    /// Size step the progress bar currently uses, for its stroke thickness and for the font size the themes select.
    /// It starts as <see cref="SizeType" /> and follows every change of it. A line progress bar replaces it with the
    /// step that <see cref="CalculateEffectiveSizeType" /> maps from its stroke thickness or from an explicitly set
    /// dimension. While the control is attached, a change recalculates the stroke thickness through
    /// <see cref="NotifyEffectSizeTypeChanged" />; it always invalidates the measure.
    /// </summary>
    internal SizeType EffectiveSizeType
    {
        get => _effectiveSizeType;
        set => SetAndRaise(EffectiveSizeTypeProperty, ref _effectiveSizeType, value);
    }

    private double _strokeThickness;

    /// <summary>
    /// Stroke thickness, in DIPs, that the groove and the indicator are drawn with. The derived class assigns it in
    /// <see cref="CalculateStrokeThickness" />.
    /// </summary>
    protected double EffectiveStrokeThickness
    {
        get => _strokeThickness;
        set => SetAndRaise(EffectiveStrokeThicknessProperty, ref _strokeThickness, value);
    }

    /// <summary>
    /// Brush of the groove, the unfilled part of the bar. The built-in themes set it from the <c>RemainingColor</c>
    /// Token of the progress control and, while the control is disabled, from <c>ColorBgContainerDisabled</c>.
    /// Setting <see cref="TrailColor" /> replaces the theme value with a solid brush of that color; clearing
    /// <see cref="TrailColor" /> clears this property as well, so the theme value applies again. The default is
    /// <see langword="null" />.
    /// </summary>
    internal IBrush? GrooveBrush
    {
        get => GetValue(GrooveBrushProperty);
        set => SetValue(GrooveBrushProperty, value);
    }

    /// <summary>
    /// Whether the percentage text of the progress info is shown, <see langword="true" /> by default. The circle
    /// progress bar (<c>AbstractCircleProgress</c>) sets it to whether the "100%" text fits into the square inside
    /// the stroke. The circle and dashboard themes bind the visibility of the percentage text to it.
    /// </summary>
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

    /// <summary>
    /// Whether <see cref="RangeBase.Value" /> has reached <see cref="RangeBase.Maximum" />, <see langword="false" />
    /// by default. The control updates it whenever the value changes, but not when only the range changes. A change
    /// invalidates the measure. The theme selects on it to hide the percentage text and, for the Normal and Active
    /// statuses, to switch to the success color and show the success icon.
    /// </summary>
    internal bool IsCompleted
    {
        get => GetValue(IsCompletedProperty);
        set => SetValue(IsCompletedProperty, value);
    }

    /// <summary>
    /// Brush of the exception status icon. The theme of the progress bar binds the <c>IconBrush</c> of
    /// <c>PART_ExceptionCompletedIconPresenter</c> to it. The built-in theme sets it from the <c>ColorError</c> Token
    /// and, while the control is disabled, from <c>ControlItemBgActiveDisabled</c>. The default is
    /// <see langword="null" />.
    /// </summary>
    internal IBrush? ExceptionIconBrush
    {
        get => GetValue(ExceptionIconBrushProperty);
        set => SetValue(ExceptionIconBrushProperty, value);
    }

    /// <summary>
    /// Brush of the success status icon. The theme of the progress bar binds the <c>IconBrush</c> of
    /// <c>PART_SuccessCompletedIconPresenter</c> to it. The built-in theme sets it from the <c>ColorSuccess</c>
    /// Token and, while the control is disabled, from <c>ControlItemBgActiveDisabled</c>. The default is
    /// <see langword="null" />.
    /// </summary>
    internal IBrush? SuccessIconBrush
    {
        get => GetValue(SuccessIconBrushProperty);
        set => SetValue(SuccessIconBrushProperty, value);
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

    /// <summary>
    /// The <c>PART_LayoutTransformControl</c> part of the template, which hosts <see cref="PercentageLabel" />.
    /// The base class hides it while <see cref="ProgressInfoContent" /> is set. <see langword="null" /> when the
    /// template has no such part.
    /// </summary>
    protected LayoutTransformControl? LayoutTransformLabel;
    /// <summary>
    /// The <c>PART_PercentageLabel</c> part of the template, which shows the percentage formatted with
    /// <see cref="ProgressTextFormat" />. <see langword="null" /> when the template has no such part.
    /// </summary>
    protected Label? PercentageLabel;
    /// <summary>
    /// The <c>PART_ProgressIndicator</c> part of the template: the panel that
    /// <see cref="ArrangeProgressIndicator" /> places and sizes with a given rectangle.
    /// <see langword="null" /> when the template has no such part.
    /// </summary>
    private protected Panel? ProgressIndicator;
    /// <summary>
    /// The <c>PART_SuccessCompletedIconPresenter</c> part of the template, which shows
    /// <see cref="SuccessCompletedIcon" />. <see langword="null" /> when the template has no such part.
    /// </summary>
    protected IconPresenter? SuccessCompletedIconPresenter;
    /// <summary>
    /// The <c>PART_ExceptionCompletedIconPresenter</c> part of the template, which shows
    /// <see cref="ExceptionCompletedIcon" />. <see langword="null" /> when the template has no such part.
    /// </summary>
    protected IconPresenter? ExceptionCompletedIconPresenter;
    /// <summary>
    /// The <c>PART_ProgressInfoContentPresenter</c> part of the template, which presents
    /// <see cref="ProgressInfoContent" /> with <see cref="ProgressInfoContentTemplate" />.
    /// <see langword="null" /> when the template has no such part.
    /// </summary>
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
            EffectiveStrokeThicknessProperty,
            SuccessStrokeBrushProperty,
            SuccessThresholdProperty);
        ValueProperty.OverrideMetadata<AbstractProgressBar>(
            new StyledPropertyMetadata<double>(defaultBindingMode: BindingMode.OneWay));
        SizeTypeProperty.OverrideDefaultValue<AbstractProgressBar>(SizeType.Large);
    }

    public AbstractProgressBar()
    {
        _transitionInitialization = new TransitionInitializationSession(this);
        // A derived class can override the default value of IsProgressInfoVisible, so the flag starts from the actual
        // default instead of waiting for the first change.
        _isProgressInfoPresented = IsProgressInfoVisible || ProgressInfoContent is not null;
        _effectiveSizeType = SizeType;
    }

    /// <summary>
    /// Reacts to property changes with the derived state of the progress bar. It recalculates
    /// <see cref="Percentage" /> and the progress info, refreshes the pseudo classes, updates the visuals of
    /// <see cref="ProgressInfoContent" />, assigns <see cref="SizeType" /> to <see cref="EffectiveSizeType" />,
    /// updates <see cref="IsCompleted" />, and forwards every change to <see cref="NotifyPropertyChanged" />.
    /// Derived overrides must call the base member.
    /// </summary>
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
            // The info can be turned on at runtime and the status can leave Exception; both refresh the text and the
            // visibility of the info elements.
            UpdateProgress();
        }

        if (change.Property == IsIndeterminateProperty ||
            change.Property == ValueProperty ||
            change.Property == MinimumProperty ||
            change.Property == MaximumProperty)
        {
            // Completed and ValueZero follow the value and the range, so the pseudo classes are refreshed on each such
            // change; the theme values follow them.
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
                    // Restores the percentage text once the custom content is cleared.
                    UpdateProgress();
                }

                NotifyProgressInfoContentChanged();
            }
        }

        HandlePropertyChangedForStyle(change);
    }

    /// <summary>
    /// Releases the custom info content from the presenter of the previous template, calls the base member, looks up
    /// the template parts into fields such as <see cref="PercentageLabel" />, and then refreshes the info visuals,
    /// the stroke thickness and the progress. Derived overrides must call the base member.
    /// </summary>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        // The presenter of the previous template is still the visual parent of the custom content. It must release the
        // content before the new presenter takes it over, otherwise the content gets two visual parents.
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
        // The parts have just been created, so the size-dependent state is updated once here.
        NotifyEffectSizeTypeChanged();
        UpdateProgress();
    }

    /// <summary>
    /// Calls the base member and then refreshes the indeterminate, completed and value-zero pseudo classes from the
    /// current property values.
    /// </summary>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        UpdatePseudoClasses();
    }
    
    /// <summary>
    /// Maps a layout size, in DIPs, to the size type it belongs to, for example the thickness of a line progress bar
    /// to the size type of its stroke. The base class does not call it; a derived class calls it when that size
    /// changes and assigns the result to <see cref="EffectiveSizeType" />.
    /// </summary>
    /// <param name="size">
    /// The layout size, in DIPs, that decides the size type.
    /// </param>
    /// <returns>
    /// The size type that <paramref name="size" /> belongs to.
    /// </returns>
    protected abstract SizeType CalculateEffectiveSizeType(double size);

    /// <summary>
    /// Returns the rectangle the groove occupies within the control rectangle.
    /// </summary>
    /// <param name="controlRect">
    /// The rectangle of the control.
    /// </param>
    /// <returns>
    /// The rectangle of the groove.
    /// </returns>
    protected abstract Rect GetProgressBarRect(Rect controlRect);
    /// <summary>
    /// Returns the rectangle the progress info occupies within the control rectangle.
    /// </summary>
    /// <param name="controlRect">
    /// The rectangle of the control.
    /// </param>
    /// <returns>
    /// The rectangle of the progress info.
    /// </returns>
    protected abstract Rect GetExtraInfoRect(Rect controlRect);

    /// <summary>
    /// Draws the groove with <see cref="GrooveBrush" />. The base class calls it from <see cref="Render" /> after
    /// <see cref="NotifyPrepareDrawingContext" />, unless <see cref="HasTemplateProgressVisuals" /> is
    /// <see langword="true" />.
    /// </summary>
    protected abstract void RenderGroove(DrawingContext context);
    /// <summary>
    /// Draws the indicator, the filled part of the bar. The base class calls it from <see cref="Render" /> directly
    /// after <see cref="RenderGroove" />, unless <see cref="HasTemplateProgressVisuals" /> is
    /// <see langword="true" />.
    /// </summary>
    protected abstract void RenderIndicatorBar(DrawingContext context);
    /// <summary>
    /// Computes the stroke thickness and assigns it to <see cref="EffectiveStrokeThickness" />. The base class calls
    /// it from <see cref="NotifyEffectSizeTypeChanged" />; a derived class calls it again when a property the
    /// thickness depends on changes.
    /// </summary>
    protected abstract void CalculateStrokeThickness();

    /// <summary>
    /// Whether the template of the derived control provides a panel that draws the progress body itself. When it is
    /// <see langword="true" />, <see cref="Render" /> draws nothing and the template parts take the place of
    /// <see cref="RenderGroove" /> and <see cref="RenderIndicatorBar" />.
    /// </summary>
    private protected virtual bool HasTemplateProgressVisuals => false;

    /// <summary>
    /// Called after the template has been applied and, while the control is attached to a visual tree, whenever
    /// <see cref="EffectiveSizeType" /> changes. It recalculates the stroke thickness. Derived overrides must call
    /// the base member.
    /// </summary>
    protected virtual void NotifyEffectSizeTypeChanged()
    {
        CalculateStrokeThickness();
    }

    /// <summary>
    /// Places <see cref="ProgressIndicator" /> at the given rectangle and sizes it to that rectangle. Does nothing
    /// when the template has no such part.
    /// </summary>
    /// <param name="bounds">
    /// The rectangle to place the indicator panel at.
    /// </param>
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

    /// <summary>
    /// Returns the position of a value in the range of <see cref="RangeBase.Minimum" /> and
    /// <see cref="RangeBase.Maximum" />, as a ratio that is not clamped.
    /// </summary>
    /// <param name="value">
    /// The value to locate in the range.
    /// </param>
    /// <returns>
    /// <c>(value - Minimum) / (Maximum - Minimum)</c>, or <c>1.0</c> when the two limits are equal.
    /// </returns>
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

    /// <summary>
    /// Called after <see cref="Percentage" /> has been recalculated: when the value, the range,
    /// <see cref="IsIndeterminate" />, <see cref="ProgressTextFormat" />,
    /// <see cref="IsProgressInfoVisible" /> or <see cref="Status" /> changes, when
    /// <see cref="ProgressInfoContent" /> is cleared and after the template has been applied. While the progress
    /// info is visible it writes the formatted percentage into <see cref="PercentageLabel" />, unless
    /// <see cref="Status" /> is <see cref="ProgressStatus.Exception" />, and then calls
    /// <see cref="NotifyHandleExtraInfoVisibility" />. Derived overrides must call the base member.
    /// </summary>
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

    /// <summary>
    /// Lets a derived class update the visibility of the progress info elements. The base class calls it while
    /// <see cref="IsProgressInfoVisible" /> is <see langword="true" /> and the template provides
    /// <see cref="PercentageLabel" />, once from <see cref="NotifyUpdateProgress" /> and once when
    /// <see cref="Avalonia.Layout.Layoutable.Width" /> or <see cref="Avalonia.Layout.Layoutable.Height" /> changes
    /// while the control is attached to a visual tree.
    /// </summary>
    protected virtual void NotifyHandleExtraInfoVisibility()
    {
    }

    /// <summary>
    /// Called after <see cref="ProgressInfoContent" /> is set or cleared, so that a derived class can recalculate
    /// the size of the progress info area.
    /// </summary>
    protected virtual void NotifyProgressInfoContentChanged()
    {
    }

    // While custom content is present, the percentage label and the status icons are hidden; clearing the content
    // hands them back to the styles and the default logic.
    // The content is written into the presenter directly here, so a following measure reads the latest content.
    // The label and the icons are hidden at a disposable Animation priority: it outranks local values and bindings
    // without replacing them. The value is released when the content is cleared or the template changes, which
    // restores the local IsVisible values and bindings a custom template had set.
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

    // The default icon is written at template priority, so a theme icon or a local icon replaces it. Applying the
    // template again replaces the held handle, so that value frames do not accumulate.
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
    /// Measures the presenter of <see cref="ProgressInfoContent" /> with an infinite available size and returns its
    /// desired size, or <see langword="null" /> when no custom content is set or the template has no presenter.
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
        PseudoClasses.Set(ProgressBarPseudoClass.ValueZero, MathUtils.AreClose(Value, 0d));
    }

    /// <summary>
    /// Called at the end of <see cref="OnPropertyChanged" /> for every property change. The base member applies
    /// <see cref="TrailColor" /> to <see cref="GrooveBrush" />. Derived overrides must call the base member.
    /// </summary>
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

    /// <summary>
    /// Draws the groove and the indicator through <see cref="RenderGroove" /> and
    /// <see cref="RenderIndicatorBar" /> unless <see cref="HasTemplateProgressVisuals" /> is
    /// <see langword="true" />; in that case the template provides the progress visuals and nothing is drawn here.
    /// </summary>
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

    /// <summary>
    /// Called from <see cref="Render" /> before <see cref="RenderGroove" />, to let a derived class prepare the
    /// drawing context.
    /// </summary>
    /// <param name="context">
    /// The context the groove and the indicator are drawn into.
    /// </param>
    protected virtual void NotifyPrepareDrawingContext(DrawingContext context)
    {
    }

    /// <summary>
    /// Resets the transition initialization session and then calls the base member. Derived overrides must call the
    /// base member.
    /// </summary>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _transitionInitialization.Reset();
        base.OnDetachedFromVisualTree(e);
    }

    /// <summary>
    /// Calls the base member and then completes the transition initialization session. Derived overrides must call
    /// the base member, otherwise the session is never completed.
    /// </summary>
    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        _transitionInitialization.CompleteInitialization();
    }
}
