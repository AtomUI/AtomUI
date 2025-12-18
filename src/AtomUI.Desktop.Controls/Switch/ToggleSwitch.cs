using System.Reactive.Disposables;
using AtomUI.Animations;
using AtomUI.Controls;
using AtomUI.Data;
using AtomUI.Desktop.Controls.Primitives;
using AtomUI.Desktop.Controls.Themes;
using AtomUI.Media;
using AtomUI.Theme;
using AtomUI.Utils;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Metadata;
using Avalonia.Rendering;

namespace AtomUI.Desktop.Controls;

public class ToggleSwitch : ToggleButton,
                            ISizeTypeAware,
                            ICustomHitTest,
                            IWaveSpiritAwareControl,
                            IControlSharedTokenResourcesHost
{
    #region 公共属性定义

    /// <summary>
    /// Defines the <see cref="GrooveBackground" /> property.
    /// </summary>
    public static readonly StyledProperty<IBrush?> GrooveBackgroundProperty =
        AvaloniaProperty.Register<ToggleSwitch, IBrush?>(
            nameof(GrooveBackground));

    /// <summary>
    /// Defines the <see cref="OnContent" /> property.
    /// </summary>
    public static readonly StyledProperty<object?> OnContentProperty =
        AvaloniaProperty.Register<ToggleSwitch, object?>(nameof(OnContent));
    
    public static readonly StyledProperty<IDataTemplate?> OnContentTemplateProperty = 
        AvaloniaProperty.Register<ToggleSwitch, IDataTemplate?>(nameof(OnContentTemplate));
    
    /// <summary>
    /// Defines the <see cref="OffContent" /> property.
    /// </summary>
    public static readonly StyledProperty<object?> OffContentProperty =
        AvaloniaProperty.Register<ToggleSwitch, object?>(nameof(OffContent));

    public static readonly StyledProperty<IDataTemplate?> OffContentTemplateProperty = 
        AvaloniaProperty.Register<ToggleSwitch, IDataTemplate?>(nameof(OffContentTemplate));

    /// <summary>
    /// 设置预置的大小类型
    /// </summary>
    public static readonly StyledProperty<SizeType> SizeTypeProperty =
        SizeTypeControlProperty.SizeTypeProperty.AddOwner<ToggleSwitch>();

    /// <summary>
    /// 是否处于加载状态
    /// </summary>
    public static readonly StyledProperty<bool> IsLoadingProperty =
        AvaloniaProperty.Register<ToggleSwitch, bool>(nameof(IsLoading));

    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<ToggleSwitch>();

    public static readonly StyledProperty<bool> IsWaveSpiritEnabledProperty =
        WaveSpiritAwareControlProperty.IsWaveSpiritEnabledProperty.AddOwner<ToggleSwitch>();

    /// <summary>
    /// Gets or Sets the Content that is displayed when in the On State.
    /// </summary>
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

    /// <summary>
    /// Gets or Sets the Content that is displayed when in the Off State.
    /// </summary>
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

    public SizeType SizeType
    {
        get => GetValue(SizeTypeProperty);
        set => SetValue(SizeTypeProperty, value);
    }

    /// <summary>
    /// 是否处于加载状态
    /// </summary>
    public bool IsLoading
    {
        get => GetValue(IsLoadingProperty);
        set => SetValue(IsLoadingProperty, value);
    }

    public IBrush? GrooveBackground
    {
        get => GetValue(GrooveBackgroundProperty);
        set => SetValue(GrooveBackgroundProperty, value);
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

    #endregion

    #region 私有属性定义

    internal static readonly StyledProperty<double> InnerMaxMarginProperty =
        AvaloniaProperty.Register<ToggleSwitch, double>(nameof(InnerMaxMargin));

    internal static readonly StyledProperty<double> InnerMinMarginProperty =
        AvaloniaProperty.Register<ToggleSwitch, double>(nameof(InnerMinMargin));

    internal static readonly StyledProperty<double> TrackHeightProperty =
        AvaloniaProperty.Register<ToggleSwitch, double>(nameof(TrackHeight));

    internal static readonly StyledProperty<double> IconSizeProperty =
        AvaloniaProperty.Register<ToggleSwitch, double>(nameof(IconSize));

    internal static readonly StyledProperty<double> TrackMinWidthProperty =
        AvaloniaProperty.Register<ToggleSwitch, double>(nameof(TrackMinWidth));

    internal static readonly StyledProperty<double> TrackPaddingProperty =
        AvaloniaProperty.Register<ToggleSwitch, double>(nameof(TrackPadding));

    // 这几个属性跟动画相关
    internal static readonly StyledProperty<Rect> KnobRectProperty = 
        AvaloniaProperty.Register<ToggleSwitch, Rect>(nameof(KnobRect));

    public static readonly StyledProperty<Size> KnobSizeProperty =
        AvaloniaProperty.Register<ToggleSwitch, Size>(nameof(KnobSize));

    internal static readonly StyledProperty<Rect> KnobMovingRectProperty = 
        AvaloniaProperty.Register<ToggleSwitch, Rect>(nameof(KnobMovingRect));

    internal static readonly StyledProperty<Point> OnContentOffsetProperty =
        AvaloniaProperty.Register<ToggleSwitch, Point>(nameof(OnContentOffset));

    internal static readonly StyledProperty<Point> OffContentOffsetProperty = 
        AvaloniaProperty.Register<ToggleSwitch, Point>(nameof(OffContentOffset));

    internal static readonly StyledProperty<double> SwitchOpacityProperty = 
        AvaloniaProperty.Register<ToggleSwitch, double>(nameof(SwitchOpacity), 1d);

    internal double InnerMaxMargin
    {
        get => GetValue(InnerMaxMarginProperty);
        set => SetValue(InnerMaxMarginProperty, value);
    }

    internal double InnerMinMargin
    {
        get => GetValue(InnerMinMarginProperty);
        set => SetValue(InnerMinMarginProperty, value);
    }

    internal double TrackHeight
    {
        get => GetValue(TrackHeightProperty);
        set => SetValue(TrackHeightProperty, value);
    }

    internal double IconSize
    {
        get => GetValue(IconSizeProperty);
        set => SetValue(IconSizeProperty, value);
    }

    internal double TrackMinWidth
    {
        get => GetValue(TrackMinWidthProperty);
        set => SetValue(TrackMinWidthProperty, value);
    }

    internal double TrackPadding
    {
        get => GetValue(TrackPaddingProperty);
        set => SetValue(TrackPaddingProperty, value);
    }

    internal Rect KnobRect
    {
        get => GetValue(KnobRectProperty);
        set => SetValue(KnobRectProperty, value);
    }

    public Size KnobSize
    {
        get => GetValue(KnobSizeProperty);
        set => SetValue(KnobSizeProperty, value);
    }

    internal Rect KnobMovingRect
    {
        get => GetValue(KnobMovingRectProperty);
        set => SetValue(KnobMovingRectProperty, value);
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

    internal double SwitchOpacity
    {
        get => GetValue(SwitchOpacityProperty);
        set => SetValue(SwitchOpacityProperty, value);
    }
    
    Control IControlSharedTokenResourcesHost.HostControl => this;
    string IControlSharedTokenResourcesHost.TokenId => ToggleSwitchToken.ID;

    #endregion

    private const double STRETCH_FACTOR = 1.3d;
    private ContentPresenter? _onContentPresenter;
    private ContentPresenter? _offContentPresenter;
    
    private bool _isCheckedChanged = false;
    private SwitchKnob? _switchKnob;
    private CompositeDisposable? _onBindingDisposables;
    private CompositeDisposable? _offBindingDisposables;
    private WaveSpiritDecorator? _waveSpiritDecorator;
    private Canvas? _mainLayout;
    
    static ToggleSwitch()
    {
        AffectsMeasure<ToggleSwitch>(SizeTypeProperty, IsCheckedProperty);
        AffectsArrange<ToggleSwitch>(
            IsPressedProperty,
            KnobRectProperty,
            KnobMovingRectProperty,
            OnContentOffsetProperty,
            OffContentOffsetProperty);
        AffectsRender<ToggleSwitch>(GrooveBackgroundProperty,
            SwitchOpacityProperty);
    }

    public ToggleSwitch()
    {
        this.RegisterResources();
    }

    private void ConfigureTransitions(bool force)
    {
        if (IsMotionEnabled)
        {
            if (force || Transitions == null)
            {
                Transitions = [
                    TransitionUtils.CreateTransition<RectTransition>(KnobMovingRectProperty),
                    TransitionUtils.CreateTransition<PointTransition>(OnContentOffsetProperty),
                    TransitionUtils.CreateTransition<PointTransition>(OffContentOffsetProperty),
                    TransitionUtils.CreateTransition<SolidColorBrushTransition>(GrooveBackgroundProperty),
                    TransitionUtils.CreateTransition<DoubleTransition>(SwitchOpacityProperty)
                ];
            }
        }
        else
        {
            Transitions = null;
        }
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        ConfigureTransitions(false);
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        base.OnUnloaded(e);
        Transitions = null;
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

        var switchHeight  = TrackHeight;
        var switchWidth   = extraInfoWidth;
        var trackMinWidth = TrackMinWidth;
        switchWidth += InnerMinMargin + InnerMaxMargin;
        switchWidth =  Math.Max(switchWidth, trackMinWidth);
        var targetSize = new Size(switchWidth, switchHeight);
        CalculateElementsOffset(targetSize);
        if (_switchKnob is not null)
        {
            _switchKnob.Measure(KnobRect.Size);
        }
        
        return targetSize;
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        _mainLayout?.Arrange(new Rect(DesiredSize));
        if (!_isCheckedChanged)
        {
            _switchKnob?.Arrange(KnobRect);
        }
        else
        {
            _switchKnob?.Arrange(KnobMovingRect);
        }
        
        _offContentPresenter?.Arrange(new Rect(new Point(OffContentOffset.X, OffContentOffset.Y), _offContentPresenter.DesiredSize));
        _onContentPresenter?.Arrange(new Rect(new Point(OnContentOffset.X, OnContentOffset.Y), _onContentPresenter.DesiredSize));
        _waveSpiritDecorator?.Arrange(new Rect(new Point(0, 0), DesiredSize.Deflate(Margin)));
        
        return finalSize;
    }
    
    private void AdjustOffsetOnPressed()
    {
        var handleRect = HandleRect();
        var handleSize = handleRect.Width;

        var contentOffsetDelta = handleSize * (STRETCH_FACTOR - 1);

        if (IsChecked.HasValue && IsChecked.Value)
        {
            // 点击的时候如果是选中，需要调整坐标
            OnContentOffset = new Point(OnContentOffset.X - contentOffsetDelta, OffContentOffset.Y);
        }
        else
        {
            OffContentOffset = new Point(OffContentOffset.X + contentOffsetDelta, OffContentOffset.Y);
        }

        if (_switchKnob is not null)
        {
            _switchKnob.KnobSize = new Size(handleSize, KnobSize.Height);
        }

        KnobRect       = handleRect;
        KnobMovingRect = KnobRect;
    }

    private void AdjustOffsetOnReleased()
    {
        CalculateElementsOffset(GrooveRect().Size);

        if (_switchKnob is not null)
        {
            _switchKnob.KnobSize = KnobSize;
        }
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        if (!IsLoading)
        {
            base.OnPointerPressed(e);
            if (!MathUtils.AreClose(KnobMovingRect.X, KnobRect.X))
            {
                return;
            }

            _isCheckedChanged = false;
            AdjustOffsetOnPressed();
        }
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        if (!IsLoading)
        {
            base.OnPointerReleased(e);
            AdjustOffsetOnReleased();
            InvalidateArrange();
        }
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == IsLoadingProperty)
        {
            HandleLoadingState(IsLoading);
        }
        else if ((change.Property == IsPointerOverProperty && !IsLoading) ||
                 change.Property == IsCheckedProperty ||
                 change.Property == IsEnabledProperty)
        {
            if (change.Property == IsCheckedProperty && IsMotionEnabled)
            {
                CalculateElementsOffset(GrooveRect().Size);
                _waveSpiritDecorator?.Play();
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
        else if (change.Property == IsCheckedProperty)
        {
            _isCheckedChanged = true;
        }

        if (IsLoaded)
        {
            if (change.Property == IsMotionEnabledProperty)
            {
                ConfigureTransitions(true);
            }
        }
        
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        var scope = e.NameScope;
        _switchKnob  = scope.Find<SwitchKnob>(ToggleSwitchThemeConstants.SwitchKnobPart);
        if (_switchKnob is not null)
        {
            _switchKnob.KnobSize = KnobSize;
        }

        _onContentPresenter  = scope.Find<ContentPresenter>(ToggleSwitchThemeConstants.OnContentPresenterPart);
        _offContentPresenter = scope.Find<ContentPresenter>(ToggleSwitchThemeConstants.OffContentPresenterPart);
        _waveSpiritDecorator = e.NameScope.Find<WaveSpiritDecorator>(ToggleSwitchThemeConstants.WaveSpiritPart);
        _mainLayout          = e.NameScope.Find<Canvas>(ToggleSwitchThemeConstants.MainContainerPart);
        HandleLoadingState(IsLoading);
    }

    public sealed override void Render(DrawingContext context)
    {
        using var state = context.PushOpacity(SwitchOpacity);
        var       size  = DesiredSize.Deflate(Margin);
        context.DrawPilledRect(GrooveBackground, null, new Rect(new Point(0, 0), size));
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
            var disposables = new CompositeDisposable(3);
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
        KnobRect       = HandleRect(isChecked, controlSize);
        KnobMovingRect = KnobRect;

        var onExtraInfoRect  = ExtraInfoRect(true, controlSize);
        var offExtraInfoRect = ExtraInfoRect(false, controlSize);
        if (isChecked)
        {
            OnContentOffset  = onExtraInfoRect.TopLeft;
            OffContentOffset = new Point(controlSize.Width + 1, onExtraInfoRect.Top);
        }
        else
        {
            OffContentOffset = offExtraInfoRect.TopLeft;
            OnContentOffset  = new Point(-offExtraInfoRect.Width, offExtraInfoRect.Top);
        }
    }

    private void HandleLoadingState(bool isLoading)
    {
        if (isLoading)
        {
            Cursor = new Cursor(StandardCursorType.Arrow);
            _switchKnob?.NotifyStartLoading();
        }
        else
        {
            Cursor = new Cursor(StandardCursorType.Hand);
            _switchKnob?.NotifyStopLoading();
        }
    }

    private Rect GrooveRect()
    {
        return new Rect(new Point(0, 0), DesiredSize.Deflate(Margin));
    }

    private Rect HandleRect()
    {
        return HandleRect(IsChecked.HasValue && IsChecked.Value, GrooveRect().Size);
    }

    private Rect HandleRect(bool isChecked, Size controlSize)
    {
        double handlePosX;
        double handlePosY;
        var    handleSize = KnobSize.Width;
        var    offsetX    = TrackPadding;
        var    offsetY    = TrackPadding;
        if (IsPressed)
        {
            handleSize *= STRETCH_FACTOR;
        }

        if (!isChecked)
        {
            handlePosX = offsetX;
            handlePosY = offsetY;
        }
        else
        {
            handlePosX = controlSize.Width - offsetX - handleSize;
            handlePosY = offsetY;
        }

        return new Rect(handlePosX, handlePosY, handleSize, KnobSize.Height);
    }

    private Rect ExtraInfoRect(bool isChecked, Size controlSize)
    {
        var    innerMinMargin = InnerMinMargin;
        var    innerMaxMargin = InnerMaxMargin;
        double yAdjustValue   = 0;
        var    targetRect     = new Rect(new Point(0, 0), controlSize);
        if (isChecked)
        {
            if (_offContentPresenter != null)
            {
                yAdjustValue = (controlSize.Height - _offContentPresenter.DesiredSize.Height) / 2;
            }

            targetRect = targetRect.Inflate(new Thickness(-innerMinMargin, -yAdjustValue, innerMaxMargin, 0));
        }
        else
        {
            if (_onContentPresenter != null)
            {
                yAdjustValue = (controlSize.Height - _onContentPresenter.DesiredSize.Height) / 2;
            }

            targetRect = targetRect.Inflate(new Thickness(-innerMaxMargin, -yAdjustValue, innerMinMargin, 0));
        }

        return targetRect;
    }
}