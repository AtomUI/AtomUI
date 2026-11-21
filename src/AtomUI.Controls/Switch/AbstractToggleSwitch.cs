using System.Globalization;
using System.Reactive.Disposables;
using AtomUI.Animations;
using AtomUI.Controls.Primitives;
using AtomUI.Data;
using AtomUI.Media;
using AtomUI.Theme.Resources;
using AtomUI.Utils;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.TextFormatting;
using Avalonia.Media.Transformation;
using Avalonia.Metadata;
using Avalonia.Rendering;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace AtomUI.Controls;

[TemplatePart("PART_SwitchKnob", typeof(SwitchKnob))]
public abstract class AbstractToggleSwitch : ToggleButton,
                                             IFocusOutlineShapeProvider,
                                             ICustomizableSizeTypeAware,
                                             ICustomHitTest,
                                             IWaveSpiritAwareControl,
                                             IFormItemAware,
                                             ITransitionInitializationOwner
{
    #region 公共属性定义
    
    public static readonly StyledProperty<IBrush?> GrooveBackgroundProperty =
        AvaloniaProperty.Register<AbstractToggleSwitch, IBrush?>(
            nameof(GrooveBackground));

    /// <summary>
    /// The brush of the groove's outline, drawn inside the groove; nothing is drawn while it is
    /// <see langword="null"/> or <see cref="GrooveBorderThickness"/> is 0.
    /// </summary>
    public static readonly StyledProperty<IBrush?> GrooveBorderBrushProperty =
        AvaloniaProperty.Register<AbstractToggleSwitch, IBrush?>(nameof(GrooveBorderBrush));

    /// <summary>
    /// The width of the groove's outline. The outline lies inside the groove, so it changes neither the layout nor the
    /// knob position.
    /// </summary>
    public static readonly StyledProperty<double> GrooveBorderThicknessProperty =
        AvaloniaProperty.Register<AbstractToggleSwitch, double>(nameof(GrooveBorderThickness));

    public static readonly StyledProperty<object?> OnContentProperty =
        AvaloniaProperty.Register<AbstractToggleSwitch, object?>(nameof(OnContent));
    
    public static readonly StyledProperty<IDataTemplate?> OnContentTemplateProperty = 
        AvaloniaProperty.Register<AbstractToggleSwitch, IDataTemplate?>(nameof(OnContentTemplate));
    
    public static readonly StyledProperty<object?> OffContentProperty =
        AvaloniaProperty.Register<AbstractToggleSwitch, object?>(nameof(OffContent));

    public static readonly StyledProperty<IDataTemplate?> OffContentTemplateProperty = 
        AvaloniaProperty.Register<AbstractToggleSwitch, IDataTemplate?>(nameof(OffContentTemplate));
    
    public static readonly StyledProperty<CustomizableSizeType> SizeTypeProperty =
        CustomizableSizeTypeControlProperty.SizeTypeProperty.AddOwner<AbstractToggleSwitch>();
    
    public static readonly StyledProperty<bool> IsLoadingProperty =
        AvaloniaProperty.Register<AbstractToggleSwitch, bool>(nameof(IsLoading));

    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<AbstractToggleSwitch>();

    public static readonly StyledProperty<bool> IsWaveSpiritEnabledProperty =
        WaveSpiritAwareControlProperty.IsWaveSpiritEnabledProperty.AddOwner<AbstractToggleSwitch>();

    /// <summary>
    /// 开关轨道高度
    /// </summary>
    public static readonly StyledProperty<double> TrackHeightProperty =
        AvaloniaProperty.Register<AbstractToggleSwitch, double>(nameof(TrackHeight));

    /// <summary>
    /// 开关轨道最小宽度
    /// </summary>
    public static readonly StyledProperty<double> TrackMinWidthProperty =
        AvaloniaProperty.Register<AbstractToggleSwitch, double>(nameof(TrackMinWidth));

    /// <summary>
    /// 开关轨道内边距，把手位置由该值决定，允许为负以实现把手溢出轨道
    /// </summary>
    public static readonly StyledProperty<double> TrackPaddingProperty =
        AvaloniaProperty.Register<AbstractToggleSwitch, double>(nameof(TrackPadding));

    /// <summary>
    /// 开关把手大小
    /// </summary>
    public static readonly StyledProperty<Size> KnobSizeProperty =
        AvaloniaProperty.Register<AbstractToggleSwitch, Size>(nameof(KnobSize));
    
    public IBrush? GrooveBackground
    {
        get => GetValue(GrooveBackgroundProperty);
        set => SetValue(GrooveBackgroundProperty, value);
    }

    /// <inheritdoc cref="GrooveBorderBrushProperty"/>
    public IBrush? GrooveBorderBrush
    {
        get => GetValue(GrooveBorderBrushProperty);
        set => SetValue(GrooveBorderBrushProperty, value);
    }

    /// <inheritdoc cref="GrooveBorderThicknessProperty"/>
    public double GrooveBorderThickness
    {
        get => GetValue(GrooveBorderThicknessProperty);
        set => SetValue(GrooveBorderThicknessProperty, value);
    }

    [DependsOn(nameof(OnContentTemplate))]
    public object? OnContent
    {
        get => GetValue(OnContentProperty);
        set => SetValue(OnContentProperty, value);
    }
    
    public IDataTemplate? OnContentTemplate
    {
        get => GetValue(OnContentTemplateProperty);
        set => SetValue(OnContentTemplateProperty, value);
    }
    
    [DependsOn(nameof(OffContentTemplate))]
    public object? OffContent
    {
        get => GetValue(OffContentProperty);
        set => SetValue(OffContentProperty, value);
    }
    
    public IDataTemplate? OffContentTemplate
    {
        get => GetValue(OffContentTemplateProperty);
        set => SetValue(OffContentTemplateProperty, value);
    }

    public CustomizableSizeType SizeType
    {
        get => GetValue(SizeTypeProperty);
        set => SetValue(SizeTypeProperty, value);
    }
    
    public bool IsLoading
    {
        get => GetValue(IsLoadingProperty);
        set => SetValue(IsLoadingProperty, value);
    }

    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }

    public bool IsWaveSpiritEnabled
    {
        get => GetValue(IsWaveSpiritEnabledProperty);
        set => SetValue(IsWaveSpiritEnabledProperty, value);
    }

    public double TrackHeight
    {
        get => GetValue(TrackHeightProperty);
        set => SetValue(TrackHeightProperty, value);
    }

    public double TrackMinWidth
    {
        get => GetValue(TrackMinWidthProperty);
        set => SetValue(TrackMinWidthProperty, value);
    }

    public double TrackPadding
    {
        get => GetValue(TrackPaddingProperty);
        set => SetValue(TrackPaddingProperty, value);
    }

    public Size KnobSize
    {
        get => GetValue(KnobSizeProperty);
        set => SetValue(KnobSizeProperty, value);
    }

    #endregion

    #region 内部属性定义

    internal static readonly StyledProperty<double> InnerMaxMarginProperty =
        AvaloniaProperty.Register<AbstractToggleSwitch, double>(nameof(InnerMaxMargin));

    internal static readonly StyledProperty<double> InnerMinMarginProperty =
        AvaloniaProperty.Register<AbstractToggleSwitch, double>(nameof(InnerMinMargin));

    internal static readonly StyledProperty<bool> UseContentMarginTokenResourcesProperty =
        AvaloniaProperty.Register<AbstractToggleSwitch, bool>(nameof(UseContentMarginTokenResources));

    internal static readonly StyledProperty<bool> EnableHandlePressStretchProperty =
        AvaloniaProperty.Register<AbstractToggleSwitch, bool>(nameof(EnableHandlePressStretch), true);

    internal static readonly StyledProperty<double> IconSizeProperty =
        AvaloniaProperty.Register<AbstractToggleSwitch, double>(nameof(IconSize));

    internal static readonly StyledProperty<ITransform?> KnobRenderTransformProperty =
        AvaloniaProperty.Register<AbstractToggleSwitch, ITransform?>(nameof(KnobRenderTransform));

    internal static readonly StyledProperty<double> KnobWidthExtensionProperty =
        SwitchKnob.KnobWidthExtensionProperty.AddOwner<AbstractToggleSwitch>();

    internal static readonly StyledProperty<Point> OnContentOffsetProperty =
        AvaloniaProperty.Register<AbstractToggleSwitch, Point>(nameof(OnContentOffset));

    internal static readonly StyledProperty<Point> OffContentOffsetProperty = 
        AvaloniaProperty.Register<AbstractToggleSwitch, Point>(nameof(OffContentOffset));

    internal double InnerMaxMargin
    {
        get => IsSet(InnerMaxMarginProperty) ? GetValue(InnerMaxMarginProperty) : _contentMargins.Max;
        set => SetValue(InnerMaxMarginProperty, value);
    }

    internal double InnerMinMargin
    {
        get => IsSet(InnerMinMarginProperty) ? GetValue(InnerMinMarginProperty) : _contentMargins.Min;
        set => SetValue(InnerMinMarginProperty, value);
    }

    internal bool UseContentMarginTokenResources
    {
        get => GetValue(UseContentMarginTokenResourcesProperty);
        set => SetValue(UseContentMarginTokenResourcesProperty, value);
    }

    internal bool EnableHandlePressStretch
    {
        get => GetValue(EnableHandlePressStretchProperty);
        set => SetValue(EnableHandlePressStretchProperty, value);
    }

    private OwnerTokenSet? _contentMarginTokens;
    private (double Min, double Max) _contentMargins;

    private protected virtual (object NormalMin, object NormalMax, object SmallMin, object SmallMax)? ContentMarginKeys => null;

    internal double IconSize
    {
        get => GetValue(IconSizeProperty);
        set => SetValue(IconSizeProperty, value);
    }

    // The knob target is the base value its transition animates to. SetCurrentValue would write it into the active
    // animation layer during a transition, and the end of that transition would drop it.
    internal ITransform? KnobRenderTransform
    {
        get => GetValue(KnobRenderTransformProperty);
        private set => SetValue(KnobRenderTransformProperty, value);
    }

    internal double KnobWidthExtension
    {
        get => GetValue(KnobWidthExtensionProperty);
        private set => SetValue(KnobWidthExtensionProperty, value);
    }

    internal Point OnContentOffset
    {
        get => GetValue(OnContentOffsetProperty);
        set => SetValue(OnContentOffsetProperty, value);
    }

    internal Point OffContentOffset
    {
        get => GetValue(OffContentOffsetProperty);
        set => SetValue(OffContentOffsetProperty, value);
    }

    #endregion

    private const double STRETCH_FACTOR = 1.3d;
    private ContentPresenter? _onContentPresenter;
    private ContentPresenter? _offContentPresenter;
    
    private Rect _knobTarget;
    private SwitchKnob? _switchKnob;
    private CompositeDisposable? _onBindingDisposables;
    private CompositeDisposable? _offBindingDisposables;
    private WaveSpiritDecorator? _waveSpiritDecorator;
    private Canvas? _mainLayout;
    private IDisposable? _loadingCursorValue;
    private double _onContentInkOffsetY;
    private double _offContentInkOffsetY;
    private DispatcherOperation? _pendingWave;
    private EventHandler? _formValueChanged;
    
    private readonly TransitionInitializationSession _transitionInitialization;
    private IPen? _grooveBorderPen;

    static AbstractToggleSwitch()
    {
        AffectsMeasure<AbstractToggleSwitch>(
            SizeTypeProperty,
            OnContentProperty,
            OnContentTemplateProperty,
            OffContentProperty,
            OffContentTemplateProperty,
            TrackHeightProperty,
            TrackMinWidthProperty,
            TrackPaddingProperty,
            KnobSizeProperty,
            InnerMinMarginProperty,
            InnerMaxMarginProperty);
        AffectsRender<AbstractToggleSwitch>(GrooveBackgroundProperty, GrooveBorderBrushProperty, GrooveBorderThicknessProperty);
        IsCheckedProperty.Changed.AddClassHandler<AbstractToggleSwitch>((toggleSwitch, args) => toggleSwitch.NotifyFormValueChanged(args.NewValue as bool?));
    }

    protected AbstractToggleSwitch()
    {
        _transitionInitialization = new TransitionInitializationSession(this);
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        if (IsArrangeValid)
        {
            CompleteTransitionInitializationMode(Bounds.Size);
        }
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _transitionInitialization.Reset();
        base.OnAttachedToVisualTree(e);
        UpdateContentMarginTokenSubscription();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _contentMarginTokens?.Dispose();
        _contentMarginTokens = null;
        _transitionInitialization.Reset();
        _pendingWave?.Abort();
        _pendingWave = null;
        base.OnDetachedFromVisualTree(e);
    }

    protected override void OnTemplateChanged(AvaloniaPropertyChangedEventArgs e)
    {
        _transitionInitialization?.Reset();
        base.OnTemplateChanged(e);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        _transitionInitialization.Reset();
        _pendingWave?.Abort();
        _pendingWave = null;
        base.OnApplyTemplate(e);
        var scope = e.NameScope;
        _switchKnob = scope.Find<SwitchKnob>("PART_SwitchKnob");
        if (_switchKnob is not null)
        {
            _switchKnob.KnobSize = KnobSize;
            // Custom templates can omit the bindings declared by the default theme. These fallback
            // bindings follow TemplatedParent so they release this owner when the part is replaced.
            if (!_switchKnob.IsSet(RenderTransformProperty))
            {
                _switchKnob.Bind(RenderTransformProperty, new TemplateBinding(KnobRenderTransformProperty));
                _switchKnob.SetValue(RenderTransformOriginProperty, new RelativePoint(0, 0, RelativeUnit.Relative), BindingPriority.Template);
            }
            if (!_switchKnob.IsSet(SwitchKnob.KnobWidthExtensionProperty))
            {
                _switchKnob.Bind(SwitchKnob.KnobWidthExtensionProperty, new TemplateBinding(KnobWidthExtensionProperty));
            }
        }

        _onContentPresenter  = scope.Find<ContentPresenter>("PART_OnContentPresenter");
        _offContentPresenter = scope.Find<ContentPresenter>("PART_OffContentPresenter");
        _waveSpiritDecorator = scope.Find<WaveSpiritDecorator>("PART_WaveSpirit");
        _mainLayout          = scope.Find<Canvas>("PART_MainContainer");
        HandleLoadingState(IsLoading);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == IsLoadingProperty)
        {
            HandleLoadingState(IsLoading);
        }
        else if (change.Property == UseContentMarginTokenResourcesProperty && this.IsAttachedToVisualTree())
        {
            UpdateContentMarginTokenSubscription();
        }
        else if (change.Property == SizeTypeProperty && _contentMarginTokens is not null)
        {
            RefreshContentMargins();
        }
        else if ((change.Property == IsPointerOverProperty && !IsLoading) ||
                 change.Property == IsCheckedProperty ||
                 change.Property == IsEnabledProperty)
        {
            if (change.Property == IsCheckedProperty)
            {
                CalculateElementsOffset(GrooveRect().Size);
                if (_transitionInitialization.IsReady && IsMotionEnabled && IsWaveSpiritEnabled)
                {
                    var waveSpirit = _waveSpiritDecorator;
                    _pendingWave?.Abort();
                    _pendingWave = Dispatcher.InvokeAsync(() =>
                    {
                        _pendingWave = null;
                        if (_transitionInitialization.IsReady && IsMotionEnabled && IsWaveSpiritEnabled &&
                            ReferenceEquals(waveSpirit, _waveSpiritDecorator))
                        {
                            ConfigureWaveSpiritBrush();
                            waveSpirit?.Play();
                        }
                    });
                }
            }
        }
        else if (change.Property == KnobSizeProperty)
        {
            if (_switchKnob is not null)
            {
                _switchKnob.KnobSize = KnobSize;
            }
        }

        if (change.Property == OffContentProperty ||
            change.Property == OnContentProperty)
        {
            SetupContent(change.OldValue, change.NewValue, change.Property == OnContentProperty);
        }

        if (_switchKnob is not null &&
            (change.Property == IsPressedProperty || (change.Property == EnableHandlePressStretchProperty && IsPressed)))
        {
            CalculateElementsOffset(GrooveRect().Size);
        }

        if (_mainLayout is not ToggleSwitchContentPanel)
        {
            if (change.Property == OnContentOffsetProperty)
            {
                ToggleSwitchContentPanel.SetContentOffset(_onContentPresenter, OnContentOffset);
            }
            else if (change.Property == OffContentOffsetProperty)
            {
                ToggleSwitchContentPanel.SetContentOffset(_offContentPresenter, OffContentOffset);
            }
        }
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        base.MeasureOverride(availableSize);
        double extraInfoWidth = 0;

        if (_offContentPresenter != null)
        {
            _offContentPresenter.Measure(availableSize);
            extraInfoWidth = Math.Max(extraInfoWidth, _offContentPresenter.DesiredSize.Width);
        }

        if (_onContentPresenter != null)
        {
            _onContentPresenter.Measure(availableSize);
            extraInfoWidth = Math.Max(extraInfoWidth, _onContentPresenter.DesiredSize.Width);
        }

        _onContentInkOffsetY = CalculateContentInkOffsetY(_onContentPresenter);
        _offContentInkOffsetY = CalculateContentInkOffsetY(_offContentPresenter);

        var switchHeight  = TrackHeight;
        // 无内容时内边距不参与宽度计算，宽度收敛到 TrackMinWidth
        var switchWidth   = extraInfoWidth > 0
            ? extraInfoWidth + InnerMinMargin + InnerMaxMargin
            : extraInfoWidth;
        // 显式宽度优先于内容测量宽度，保证把手与轨道几何跟随最终的排列宽度
        switchWidth = !double.IsNaN(Width)
            ? Math.Max(Width, TrackMinWidth)
            : Math.Max(switchWidth, TrackMinWidth);
        var targetSize = new Size(switchWidth, switchHeight);
        if (_switchKnob is not null)
        {
            _switchKnob.Measure(KnobSize);
        }
        
        return targetSize;
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        // Arrange 的内容盒已应用 Width/Min/Max，且不包含外部 Margin；把手与轨道
        // 必须使用这一最终几何，而不是测量阶段尚未应用约束的目标尺寸。
        CalculateElementsOffset(finalSize);
        _mainLayout?.Arrange(new Rect(finalSize));
        // 应用提供的既有模板仍可使用标准 Canvas；其布局不能由库的 ControlTheme
        // 声明式接管。只在这条兼容路径由 owner 排列，默认专用画布保持唯一排列 owner。
        if (_mainLayout is not ToggleSwitchContentPanel)
        {
            if (_offContentPresenter is { } offPresenter)
            {
                offPresenter.Arrange(new Rect(offPresenter.DesiredSize));
                ToggleSwitchContentPanel.SetContentOffset(offPresenter, OffContentOffset);
            }
            if (_onContentPresenter is { } onPresenter)
            {
                onPresenter.Arrange(new Rect(onPresenter.DesiredSize));
                ToggleSwitchContentPanel.SetContentOffset(onPresenter, OnContentOffset);
            }
        }
        _switchKnob?.Arrange(new Rect(KnobSize));


        _waveSpiritDecorator?.Arrange(new Rect(finalSize));
        CompleteTransitionInitializationMode(finalSize);
        
        return finalSize;
    }

    // The groove is a pill over the whole bounds.
    CornerRadius IFocusOutlineShapeProvider.FocusOutlineCornerRadius =>
        new CornerRadius(Math.Min(Bounds.Width, Bounds.Height) / 2);

    public sealed override void Render(DrawingContext context)
    {
        var rect      = new Rect(new Point(0, 0), Bounds.Size);
        var thickness = GrooveBorderThickness;
        if (thickness <= 0 || GrooveBorderBrush is null)
        {
            context.DrawPilledRect(GrooveBackground, null, rect);
            return;
        }

        // The outline is stroked on a rect deflated by half its width, so it stays inside the groove.
        PenUtils.TryModifyOrCreate(ref _grooveBorderPen, GrooveBorderBrush, thickness);
        context.DrawPilledRect(GrooveBackground, _grooveBorderPen, rect.Deflate(thickness / 2));
    }

    public bool HitTest(Point point)
    {
        if (!IsEnabled || IsLoading)
        {
            return false;
        }

        var grooveRect = GrooveRect();
        return grooveRect.Contains(point);
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        if (!IsLoading)
        {
            base.OnPointerPressed(e);
        }
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        if (!IsLoading)
        {
            base.OnPointerReleased(e);
        }
    }

    #region 实现 FormItem 接口

    event EventHandler? IFormItemAware.ValueChanged
    {
        add => _formValueChanged += value;
        remove => _formValueChanged -= value;
    }

    void IFormItemAware.SetFormValue(object? value) => NotifySetFormValue(value as bool?);

    object? IFormItemAware.GetFormValue() => NotifyGetFormValue();
    void IFormItemAware.ClearFormValue() => NotifyClearFormValue();
    void IFormItemAware.NotifyValidateStatus(FormValidateStatus status) => NotifyValidateStatus(status);

    protected virtual void NotifyFormValueChanged(object? value)
    {
        _formValueChanged?.Invoke(this, EventArgs.Empty);
    }

    protected virtual void NotifySetFormValue(bool? value)
    {
        SetCurrentValue(IsCheckedProperty, value);
    }

    protected virtual object? NotifyGetFormValue()
    {
        return IsChecked;
    }

    protected virtual void NotifyClearFormValue()
    {
        SetCurrentValue(IsCheckedProperty, null);
    }

    protected virtual void NotifyValidateStatus(FormValidateStatus status)
    {
    }

    #endregion

    #region 实现 ITransitionInitializationOwner 接口

    TransitionInitializationSession ITransitionInitializationOwner.TransitionInitialization => _transitionInitialization;

    #endregion

    private void CompleteTransitionInitializationMode(Size size)
    {
        if (_switchKnob is { IsArrangeValid: true } &&
            double.IsFinite(size.Width) && double.IsFinite(size.Height) &&
            MathUtils.GreaterThan(size.Width, 0) && MathUtils.GreaterThan(size.Height, 0))
        {
            _transitionInitialization.CompleteInitialization();
        }
    }

    private void SetupContent(object? oldContent, object? newContent, bool isOnContent)
    {
        if (oldContent != null)
        {
            if (isOnContent)
            {
                _onBindingDisposables?.Dispose();
                _onBindingDisposables = null;
            }
            else
            {
                _offBindingDisposables?.Dispose();
                _offBindingDisposables = null;
            }
        }
        if (newContent is PathIcon newPathIcon)
        {
            var disposables = new CompositeDisposable(4);
            disposables.Add(BindUtils.RelayBind(this, IconSizeProperty, newPathIcon, WidthProperty));
            disposables.Add(BindUtils.RelayBind(this, IconSizeProperty, newPathIcon, HeightProperty));
            disposables.Add(BindUtils.RelayBind(this, ForegroundProperty, newPathIcon, ForegroundProperty));
            if (newPathIcon is Icon icon)
            {
                disposables.Add(BindUtils.RelayBind(this, ForegroundProperty, icon, Icon.FillBrushProperty));
                disposables.Add(BindUtils.RelayBind(this, ForegroundProperty, icon, Icon.StrokeBrushProperty));
            }
            if (isOnContent)
            {
                _onBindingDisposables = disposables;
            }
            else
            {
                _offBindingDisposables = disposables;
            }
        }
    }

    private void CalculateElementsOffset(Size controlSize)
    {
        var isChecked = IsChecked.HasValue && IsChecked.Value;
        var stretchHandle = IsPressed && EnableHandlePressStretch;
        var handleWidth = stretchHandle ? KnobSize.Width * STRETCH_FACTOR : KnobSize.Width;
        var knobTarget = HandleRect(isChecked, controlSize, handleWidth);
        KnobWidthExtension = handleWidth - KnobSize.Width;
        if (KnobRenderTransform is null || _knobTarget != knobTarget)
        {
            _knobTarget = knobTarget;
            var builder = new TransformOperations.Builder(1);
            builder.AppendTranslate(knobTarget.X, knobTarget.Y);
            KnobRenderTransform = builder.Build();
        }

        var onExtraInfoRect  = ExtraInfoRect(true, controlSize);
        var offExtraInfoRect = ExtraInfoRect(false, controlSize);
        var pressedOffset = stretchHandle ? knobTarget.Width * (STRETCH_FACTOR - 1) : 0;
        if (isChecked)
        {
            OnContentOffset  = new Point(onExtraInfoRect.X - pressedOffset, onExtraInfoRect.Y);
            OffContentOffset = new Point(controlSize.Width + 1, onExtraInfoRect.Top);
        }
        else
        {
            OffContentOffset = new Point(offExtraInfoRect.X + pressedOffset, offExtraInfoRect.Y);
            OnContentOffset  = new Point(-offExtraInfoRect.Width, offExtraInfoRect.Top);
        }
    }

    private void ConfigureWaveSpiritBrush()
    {
        if (_waveSpiritDecorator is null)
        {
            return;
        }

        var waveBrush = WaveSpiritDecorator.ResolveWaveSpiritBrush(null, GrooveBackground);
        if (waveBrush is not null)
        {
            _waveSpiritDecorator.WaveBrush = waveBrush;
        }
        else
        {
            _waveSpiritDecorator.ClearValue(WaveSpiritDecorator.WaveBrushProperty);
        }
    }

    private void UpdateContentMarginTokenSubscription()
    {
        if (UseContentMarginTokenResources && ContentMarginKeys is { } keys)
        {
            if (_contentMarginTokens is null)
            {
                _contentMarginTokens = new OwnerTokenSet(this, 4, RefreshContentMargins);
                _contentMarginTokens.SetResourceKey(0, keys.NormalMin);
                _contentMarginTokens.SetResourceKey(1, keys.NormalMax);
                _contentMarginTokens.SetResourceKey(2, keys.SmallMin);
                _contentMarginTokens.SetResourceKey(3, keys.SmallMax);
            }
        }
        else if (_contentMarginTokens is not null)
        {
            _contentMarginTokens.Dispose();
            _contentMarginTokens = null;
        }
        RefreshContentMargins();
    }

    private void RefreshContentMargins()
    {
        var previous = (InnerMinMargin, InnerMaxMargin);
        var index = SizeType == CustomizableSizeType.Small ? 2 : 0;
        _contentMargins = (ReadMargin(index, _contentMargins.Min), ReadMargin(index + 1, _contentMargins.Max));
        if (!previous.Equals((InnerMinMargin, InnerMaxMargin)))
        {
            InvalidateMeasure();
        }
    }

    private double ReadMargin(int index, double previous)
    {
        var value = _contentMarginTokens?.GetValue(index);
        if (value is double margin)
        {
            return margin;
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

    private void HandleLoadingState(bool isLoading)
    {
        if (isLoading)
        {
            _loadingCursorValue ??= SetValue(CursorProperty, Cursor.Default, BindingPriority.Template);
            _switchKnob?.NotifyStartLoading();
        }
        else
        {
            _loadingCursorValue?.Dispose();
            _loadingCursorValue = null;
            _switchKnob?.NotifyStopLoading();
        }
    }

    private Rect GrooveRect()
    {
        return new Rect(Bounds.Size);
    }

    private Rect HandleRect(bool isChecked, Size controlSize, double handleWidth)
    {
        double handlePosX;
        double handlePosY;
        var    offsetX    = TrackPadding;
        var    offsetY    = TrackPadding;

        if (!isChecked)
        {
            handlePosX = offsetX;
            handlePosY = offsetY;
        }
        else
        {
            handlePosX = controlSize.Width - offsetX - handleWidth;
            handlePosY = offsetY;
        }

        return new Rect(handlePosX, handlePosY, handleWidth, KnobSize.Height);
    }

    private Rect ExtraInfoRect(bool isChecked, Size controlSize)
    {
        var    innerMinMargin = InnerMinMargin;
        var    innerMaxMargin = InnerMaxMargin;
        double yAdjustValue   = 0;
        var    targetRect     = new Rect(new Point(0, 0), controlSize);
        if (isChecked)
        {
            if (_onContentPresenter != null)
            {
                yAdjustValue = (controlSize.Height - _onContentPresenter.DesiredSize.Height) / 2 + _onContentInkOffsetY;
            }

            targetRect = targetRect.Inflate(new Thickness(-innerMinMargin, -yAdjustValue, innerMaxMargin, 0));
        }
        else
        {
            if (_offContentPresenter != null)
            {
                yAdjustValue = (controlSize.Height - _offContentPresenter.DesiredSize.Height) / 2 + _offContentInkOffsetY;
            }

            targetRect = targetRect.Inflate(new Thickness(-innerMaxMargin, -yAdjustValue, innerMinMargin, 0));
        }

        return targetRect;
    }

    private static double CalculateContentInkOffsetY(ContentPresenter? presenter)
    {
        if (presenter is not { Content: string, ContentTemplate: null, Child: TextBlock textBlock } ||
            presenter.FindDataTemplate(presenter.Content) is not null)
        {
            return 0;
        }

        // 默认字符串标签按可见字形居中；字体行盒包含给下伸字形预留的空间，
        // 直接居中行盒会让 On / Off 等标签偏上。自定义模板继续拥有自己的布局。
        // 真实字形、fallback 和 shaping offset 是运行时排版结果，ControlTheme 无法表达。
        // 复用已有排版，仅在 Measure 提取轮廓边界并缓存数值，动画的每帧 Arrange 不生成 Geometry。
        // GlyphRun.InkBounds 是保守包围盒，GlyphMetrics 可能只有 advance；均不能替代可见轮廓。
        var layout = textBlock.TextLayout;
        var inkTop = double.PositiveInfinity;
        var inkBottom = double.NegativeInfinity;
        var lineTop = 0d;
        foreach (var line in layout.TextLines)
        {
            foreach (var textRun in line.TextRuns)
            {
                if (textRun is not ShapedTextRun run)
                {
                    continue;
                }

                var inkBounds = CalculateGlyphRunInkBounds(run.GlyphRun);
                if (MathUtils.GreaterThan(inkBounds.Height, 0))
                {
                    var baselineOffset = lineTop + line.Baseline - run.Baseline;
                    inkTop = Math.Min(inkTop, baselineOffset + inkBounds.Top);
                    inkBottom = Math.Max(inkBottom, baselineOffset + inkBounds.Bottom);
                }
            }

            lineTop += line.Height;
        }

        if (double.IsFinite(inkTop) && double.IsFinite(inkBottom))
        {
            return (layout.Height - inkTop - inkBottom) / 2;
        }

        return 0;
    }

    private static Rect CalculateGlyphRunInkBounds(GlyphRun glyphRun)
    {
        if (glyphRun.GlyphInfos.All(glyph => MathUtils.IsZero(glyph.GlyphOffset.Y)))
        {
            return glyphRun.BuildGeometry().Bounds;
        }

        // Avalonia 12 的 Skia 轮廓构建未应用 GlyphOffset，实际绘制却会应用。
        // 有垂直 shaping offset 时逐字形设置基线，使轮廓边界与绘制位置一致。
        var bounds = default(Rect);
        foreach (var glyph in glyphRun.GlyphInfos)
        {
            using var single = new GlyphRun(glyphRun.GlyphTypeface, glyphRun.FontRenderingEmSize,
                glyphRun.Characters, [new GlyphInfo(glyph.GlyphIndex, 0, glyph.GlyphAdvance)],
                new Point(0, glyphRun.BaselineOrigin.Y + glyph.GlyphOffset.Y));
            var glyphBounds = single.BuildGeometry().Bounds;
            if (MathUtils.GreaterThan(glyphBounds.Height, 0))
            {
                bounds = bounds.Union(glyphBounds);
            }
        }

        return bounds;
    }
}
