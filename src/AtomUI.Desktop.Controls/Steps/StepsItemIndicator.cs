using AtomUI.Controls;
using AtomUI.Controls.Primitives;
using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Media;
using AtomUI.Theme.Resources;
using AtomUI.Utils;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

internal class StepsItemIndicator : TemplatedControl
{
    #region 公共属性定义

    public static readonly StyledProperty<StepsStatus> StatusProperty =
        AvaloniaProperty.Register<StepsItemIndicator, StepsStatus>(nameof(Status), StepsStatus.Process);

    public static readonly StyledProperty<SizeType> SizeTypeProperty =
        SizeTypeControlProperty.SizeTypeProperty.AddOwner<StepsItemIndicator>();

    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<StepsItemIndicator>();

    public static readonly StyledProperty<StepsType> TypeProperty =
        StepsItem.TypeProperty.AddOwner<StepsItemIndicator>();

    public static readonly StyledProperty<PathIcon?> IconProperty =
        StepsItem.IconProperty.AddOwner<StepsItemIndicator>();

    public StepsStatus Status
    {
        get => GetValue(StatusProperty);
        set => SetValue(StatusProperty, value);
    }

    public SizeType SizeType
    {
        get => GetValue(SizeTypeProperty);
        set => SetValue(SizeTypeProperty, value);
    }

    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }

    public StepsType Type
    {
        get => GetValue(TypeProperty);
        set => SetValue(TypeProperty, value);
    }

    public PathIcon? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    #endregion

    #region 内部属性定义

    internal static readonly StyledProperty<bool> IsWaveSpiritEnabledProperty =
        WaveSpiritAwareControlProperty.IsWaveSpiritEnabledProperty.AddOwner<StepsItemIndicator>();

    internal bool IsWaveSpiritEnabled
    {
        get => GetValue(IsWaveSpiritEnabledProperty);
        set => SetValue(IsWaveSpiritEnabledProperty, value);
    }

    internal static readonly DirectProperty<StepsItemIndicator, int> StepNumberProperty =
        AvaloniaProperty.RegisterDirect<StepsItemIndicator, int>(
            nameof(StepNumber),
            indicator => indicator.StepNumber,
            (indicator, value) => indicator.StepNumber = value);

    internal static readonly DirectProperty<StepsItemIndicator, int> DisplayStepNumberProperty =
        AvaloniaProperty.RegisterDirect<StepsItemIndicator, int>(
            nameof(DisplayStepNumber),
            indicator => indicator.DisplayStepNumber);

    internal static readonly DirectProperty<StepsItemIndicator, bool> IsCustomProperty =
        AvaloniaProperty.RegisterDirect<StepsItemIndicator, bool>(
            nameof(IsCustom),
            indicator => indicator.IsCustom,
            (indicator, value) => indicator.IsCustom = value);

    internal static readonly DirectProperty<StepsItemIndicator, bool> IsCurrentProperty =
        AvaloniaProperty.RegisterDirect<StepsItemIndicator, bool>(
            nameof(IsCurrent),
            indicator => indicator.IsCurrent,
            (indicator, value) => indicator.IsCurrent = value);

    internal static readonly DirectProperty<StepsItemIndicator, bool> IsClickableProperty =
        AvaloniaProperty.RegisterDirect<StepsItemIndicator, bool>(
            nameof(IsClickable),
            indicator => indicator.IsClickable,
            (indicator, value) => indicator.IsClickable = value);

    internal static readonly DirectProperty<StepsItemIndicator, bool> IsItemHoverProperty =
        AvaloniaProperty.RegisterDirect<StepsItemIndicator, bool>(
            nameof(IsItemHover),
            indicator => indicator.IsItemHover,
            (indicator, value) => indicator.IsItemHover = value);

    internal static readonly DirectProperty<StepsItemIndicator, bool> IsProgressVisibleProperty =
        AvaloniaProperty.RegisterDirect<StepsItemIndicator, bool>(
            nameof(IsProgressVisible),
            indicator => indicator.IsProgressVisible,
            (indicator, value) => indicator.IsProgressVisible = value);

    internal static readonly DirectProperty<StepsItemIndicator, bool> IsProgressFrameReservedProperty =
        AvaloniaProperty.RegisterDirect<StepsItemIndicator, bool>(
            nameof(IsProgressFrameReserved),
            indicator => indicator.IsProgressFrameReserved,
            (indicator, value) => indicator.IsProgressFrameReserved = value);

    internal static readonly DirectProperty<StepsItemIndicator, double?> PercentProperty =
        AvaloniaProperty.RegisterDirect<StepsItemIndicator, double?>(
            nameof(Percent),
            indicator => indicator.Percent,
            (indicator, value) => indicator.Percent = value);

    internal static readonly StyledProperty<double> ProgressLineThicknessProperty =
        AvaloniaProperty.Register<StepsItemIndicator, double>(nameof(ProgressLineThickness));

    internal static readonly StyledProperty<bool> UseProgressRingTokenResourcesProperty =
        AvaloniaProperty.Register<StepsItemIndicator, bool>(nameof(UseProgressRingTokenResources));

    private int _stepNumber;

    internal int StepNumber
    {
        get => _stepNumber;
        set
        {
            SetAndRaise(StepNumberProperty, ref _stepNumber, value);
            DisplayStepNumber = value + 1;
        }
    }

    private int _displayStepNumber = 1;

    internal int DisplayStepNumber
    {
        get => _displayStepNumber;
        private set => SetAndRaise(DisplayStepNumberProperty, ref _displayStepNumber, value);
    }

    private bool _isCustom;

    internal bool IsCustom
    {
        get => _isCustom;
        set => SetAndRaise(IsCustomProperty, ref _isCustom, value);
    }

    private bool _isCurrent;

    internal bool IsCurrent
    {
        get => _isCurrent;
        set => SetAndRaise(IsCurrentProperty, ref _isCurrent, value);
    }

    private bool _isClickable;

    internal bool IsClickable
    {
        get => _isClickable;
        set => SetAndRaise(IsClickableProperty, ref _isClickable, value);
    }

    private bool _isItemHover;

    internal bool IsItemHover
    {
        get => _isItemHover;
        set => SetAndRaise(IsItemHoverProperty, ref _isItemHover, value);
    }

    private bool _isProgressVisible;

    internal bool IsProgressVisible
    {
        get => _isProgressVisible;
        set => SetAndRaise(IsProgressVisibleProperty, ref _isProgressVisible, value);
    }

    private bool _isProgressFrameReserved;

    internal bool IsProgressFrameReserved
    {
        get => _isProgressFrameReserved;
        set => SetAndRaise(IsProgressFrameReservedProperty, ref _isProgressFrameReserved, value);
    }

    private double? _percent;

    internal double? Percent
    {
        get => _percent;
        set => SetAndRaise(PercentProperty, ref _percent, value);
    }

    internal double ProgressLineThickness
    {
        get => GetValue(ProgressLineThicknessProperty);
        set => SetValue(ProgressLineThicknessProperty, value);
    }

    internal bool UseProgressRingTokenResources
    {
        get => GetValue(UseProgressRingTokenResourcesProperty);
        set => SetValue(UseProgressRingTokenResourcesProperty, value);
    }

    #endregion

    private WaveSpiritDecorator? _waveSpiritDecorator;
    private IPen? _progressGroovePen;
    private IPen? _progressPen;
    private static readonly object s_progressGrooveKey = StepsTokenKind.ProgressGrooveColor;
    private static readonly object s_progressColorKey = StepsTokenKind.ProgressColor;
    private OwnerTokenSet? _progressPaintTokens;
    private (IBrush? Groove, IBrush? Track) _progressPaint;

    internal bool IsWavePlaying => _waveSpiritDecorator?.IsPlaying == true;

    static StepsItemIndicator()
    {
        AffectsMeasure<StepsItemIndicator>(
            SizeTypeProperty,
            TypeProperty,
            IsCurrentProperty,
            IsProgressFrameReservedProperty);
        AffectsRender<StepsItemIndicator>(
            PercentProperty,
            ProgressLineThicknessProperty,
            IsCurrentProperty,
            IsProgressVisibleProperty,
            UseLayoutRoundingProperty);
    }

    internal void PlayWave()
    {
        if (Type == StepsType.OutlineDot ||
            !IsEnabled ||
            !IsMotionEnabled ||
            !IsWaveSpiritEnabled ||
            !IsLoaded ||
            _waveSpiritDecorator is null)
        {
            return;
        }

        _waveSpiritDecorator.Play();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == IconProperty)
        {
            SetCurrentValue(IsCustomProperty, Icon is not null);
        }
        else if ((change.Property == UseProgressRingTokenResourcesProperty || change.Property == IsProgressVisibleProperty) &&
                 this.IsAttachedToVisualTree())
        {
            UpdateProgressRingTokenSubscription();
        }
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        UpdateProgressRingTokenSubscription();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _progressPaintTokens?.Dispose();
        _progressPaintTokens = null;
        _progressPaint = default;
        base.OnDetachedFromVisualTree(e);
    }

    private void UpdateProgressRingTokenSubscription()
    {
        if (IsProgressVisible && UseProgressRingTokenResources)
        {
            if (_progressPaintTokens is null)
            {
                _progressPaintTokens = new OwnerTokenSet(this, 2, RefreshProgressPaint);
                _progressPaintTokens.SetResourceKey(0, s_progressGrooveKey);
                _progressPaintTokens.SetResourceKey(1, s_progressColorKey);
            }
        }
        else if (_progressPaintTokens is not null)
        {
            _progressPaintTokens.Dispose();
            _progressPaintTokens = null;
        }
        RefreshProgressPaint();
    }

    private void RefreshProgressPaint()
    {
        var paint = (ReadBrush(_progressPaintTokens?.GetValue(0), _progressPaint.Groove),
                     ReadBrush(_progressPaintTokens?.GetValue(1), _progressPaint.Track));
        if (!_progressPaint.Equals(paint))
        {
            _progressPaint = paint;
            InvalidateVisual();
        }
    }

    private static IBrush? ReadBrush(object? value, IBrush? previous) => value switch
    {
        IBrush brush => brush,
        Color color => new ImmutableSolidColorBrush(color),
        string text when Color.TryParse(text, out var color) => new ImmutableSolidColorBrush(color),
        _ when ReferenceEquals(value, BindingOperations.DoNothing) => previous,
        _ => null
    };

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        _waveSpiritDecorator = null;
        base.OnApplyTemplate(e);
        _waveSpiritDecorator = e.NameScope.Find<WaveSpiritDecorator>(WaveSpiritDecorator.WaveSpiritPart);
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);
        if (!IsProgressVisible || !Percent.HasValue)
        {
            return;
        }

        var lineThickness = BorderUtils.BuildRenderScaleAwareThickness(this, ProgressLineThickness);
        var progressRect = new Rect(Bounds.Size).Deflate(lineThickness / 2);

        PenUtils.TryModifyOrCreate(ref _progressGroovePen, _progressPaint.Groove, lineThickness);
        context.DrawEllipse(null, _progressGroovePen, progressRect);

        PenUtils.TryModifyOrCreate(
            ref _progressPen,
            _progressPaint.Track,
            lineThickness,
            lineCap: PenLineCap.Round);
        context.DrawArc(_progressPen, progressRect, -90, 360 * Percent.Value / 100);
    }

}
